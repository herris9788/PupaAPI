using Microsoft.Extensions.Configuration;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Pupa.Services
{
    /// <summary>
    /// Chunked attachment upload (AttachmentUploadController). A file is sent as its Base64 TEXT, exactly what the client would
    /// otherwise put in the Base64 property of Attachment / JobRequestAttachment / JobAttachment (raw base64 or a data URL), in
    /// pieces of at most <see cref="ChunkBytes"/>. The pieces are kept on disk ({Root}/{UploadID}/meta.json + {index}.part) until the
    /// upload is completed into a row or cancelled; sessions older than <see cref="Lifetime"/> are deleted.
    /// Every session belongs to the user who opened it: nobody else can add chunks to it, complete it or use it.
    /// The text is stored unchanged, so viewers / downloads read it exactly as before.
    /// </summary>
    public sealed class AttachmentUploadStore
    {
        public const long MaxFileBytes = 100L * 1024 * 1024;                 // 100 MB per file
        public const long MaxBase64Bytes = (MaxFileBytes + 2) / 3 * 4 + 512;  // its Base64 text (+ room for a data: prefix)
        public const int ChunkBytes = 8 * 1024 * 1024;                       // well below Cloudflare's / the gateway's request limits
        public static readonly TimeSpan Lifetime = TimeSpan.FromHours(24);

        private static readonly Regex IdRe = new("^[0-9a-f]{32}$", RegexOptions.Compiled);
        private static readonly Regex Base64Re = new(@"^(data:[\w.+-]+/[\w.+-]+(;[\w.+=-]+)*;base64,)?[A-Za-z0-9+/\r\n]*={0,2}\s*$", RegexOptions.Compiled);
        private readonly string _root;

        public AttachmentUploadStore(IConfiguration config)
        {
            _root = config["AttachmentUpload:Dir"] is { Length: > 0 } dir ? dir : Path.Combine(Path.GetTempPath(), "pupa-attachment-uploads");
            Directory.CreateDirectory(_root);
        }

        public sealed class Meta
        {
            public string UploadID { get; set; } = "";
            public string Owner { get; set; } = "";
            public long TotalSize { get; set; }
            public int ChunkSize { get; set; }
            public int TotalChunks { get; set; }
            public DateTime CreatedAt { get; set; }
        }

        public class UploadException : Exception
        {
            public int Status { get; }
            public UploadException(int status, string message) : base(message) { Status = status; }
        }

        private string Dir(string id) => Path.Combine(_root, id);

        public Meta Create(string owner, long totalSize)
        {
            if (string.IsNullOrWhiteSpace(owner)) throw new UploadException(401, "Not signed in.");
            if (totalSize <= 0) throw new UploadException(400, "TotalSize must be greater than 0.");
            if (totalSize > MaxBase64Bytes) throw new UploadException(413, $"The file is larger than {MaxFileBytes / 1024 / 1024} MB.");
            Cleanup();
            var meta = new Meta
            {
                UploadID = Guid.NewGuid().ToString("N"),
                Owner = owner,
                TotalSize = totalSize,
                ChunkSize = ChunkBytes,
                TotalChunks = (int)((totalSize + ChunkBytes - 1) / ChunkBytes),
                CreatedAt = DateTime.UtcNow,
            };
            Directory.CreateDirectory(Dir(meta.UploadID));
            File.WriteAllText(Path.Combine(Dir(meta.UploadID), "meta.json"), JsonSerializer.Serialize(meta));
            return meta;
        }

        public Meta Get(string id, string owner)
        {
            if (!IdRe.IsMatch(id ?? "")) throw new UploadException(404, "Upload not found.");
            var path = Path.Combine(Dir(id!), "meta.json");
            if (!File.Exists(path)) throw new UploadException(404, "Upload not found or expired.");
            var meta = JsonSerializer.Deserialize<Meta>(File.ReadAllText(path))!;
            // same answer as "not found": do not reveal that someone else's upload exists
            if (string.IsNullOrWhiteSpace(owner) || !string.Equals(meta.Owner, owner, StringComparison.Ordinal)) throw new UploadException(404, "Upload not found or expired.");
            if (DateTime.UtcNow - meta.CreatedAt > Lifetime) { Delete(id!); throw new UploadException(404, "Upload expired."); }
            return meta;
        }

        public List<int> Received(Meta meta) =>
            Enumerable.Range(0, meta.TotalChunks).Where(i => File.Exists(Path.Combine(Dir(meta.UploadID), $"{i}.part"))).ToList();

        private long ExpectedChunkSize(Meta meta, int index) =>
            index < meta.TotalChunks - 1 ? meta.ChunkSize : meta.TotalSize - (long)meta.ChunkSize * (meta.TotalChunks - 1);

        /// <summary>Stores chunk <paramref name="index"/> (re-sending an index overwrites it, so a failed chunk can be retried).</summary>
        public async Task<List<int>> SaveChunk(Meta meta, int index, Stream body, CancellationToken ct)
        {
            if (index < 0 || index >= meta.TotalChunks) throw new UploadException(400, $"Chunk index must be 0..{meta.TotalChunks - 1}.");
            var expected = ExpectedChunkSize(meta, index);
            var final = Path.Combine(Dir(meta.UploadID), $"{index}.part");
            var temp = final + "." + Guid.NewGuid().ToString("N") + ".tmp";
            long written = 0;
            try
            {
                await using (var file = File.Create(temp))
                {
                    var buffer = new byte[81920];
                    int read;
                    while ((read = await body.ReadAsync(buffer, ct)) > 0)
                    {
                        written += read;
                        if (written > expected) throw new UploadException(400, $"Chunk {index} is larger than {expected} bytes.");
                        await file.WriteAsync(buffer.AsMemory(0, read), ct);
                    }
                }
                if (written != expected) throw new UploadException(400, $"Chunk {index} must be {expected} bytes, got {written}.");
                File.Move(temp, final, overwrite: true);
            }
            finally
            {
                if (File.Exists(temp)) File.Delete(temp);
            }
            return Received(meta);
        }

        /// <summary>The whole Base64 text once every chunk is there; checked to be base64 (optionally a data URL).</summary>
        public async Task<string> ReadText(Meta meta, CancellationToken ct)
        {
            var missing = Enumerable.Range(0, meta.TotalChunks).Except(Received(meta)).ToList();
            if (missing.Count > 0) throw new UploadException(409, $"Upload is not complete, missing chunks: {string.Join(",", missing.Take(20))}.");
            var bytes = new byte[meta.TotalSize];
            var offset = 0;
            for (var i = 0; i < meta.TotalChunks; i++)
            {
                var part = await File.ReadAllBytesAsync(Path.Combine(Dir(meta.UploadID), $"{i}.part"), ct);
                part.CopyTo(bytes, offset);
                offset += part.Length;
            }
            if (offset != meta.TotalSize) throw new UploadException(409, "Upload size does not match.");
            var text = Encoding.ASCII.GetString(bytes);
            if (!Base64Re.IsMatch(text)) throw new UploadException(400, "The uploaded content is not Base64 text.");
            return text;
        }

        public void Delete(string id)
        {
            if (!IdRe.IsMatch(id ?? "")) return;
            try { if (Directory.Exists(Dir(id!))) Directory.Delete(Dir(id!), recursive: true); } catch { /* removed by Cleanup later */ }
        }

        /// <summary>Removes sessions older than <see cref="Lifetime"/> (called whenever a new upload starts).</summary>
        public void Cleanup()
        {
            foreach (var dir in Directory.EnumerateDirectories(_root))
            {
                try
                {
                    var meta = Path.Combine(dir, "meta.json");
                    var created = File.Exists(meta) ? File.GetCreationTimeUtc(meta) : Directory.GetCreationTimeUtc(dir);
                    if (DateTime.UtcNow - created > Lifetime) Directory.Delete(dir, recursive: true);
                }
                catch { /* next time */ }
            }
        }
    }
}
