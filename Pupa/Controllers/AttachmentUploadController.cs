using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pupa.BusinessObjects;
using Pupa.BusinessObjects.Beesuite;
using Pupa.Services;

namespace Pupa.Controllers
{
    /// <summary>
    /// Chunked upload of an attachment's Base64 text, so a big file never has to fit in one request (Cloudflare / gateway / Kestrel
    /// limits). Reached through the gateway as /api/v3/AttachmentUpload/... (-> beesuite/api/AttachmentUpload/...).
    ///   1. POST   AttachmentUpload                      { TotalSize }            -> { UploadID, ChunkSize, TotalChunks }
    ///   2. PUT    AttachmentUpload/{id}/chunks/{index}  raw piece of the text     -> { UploadID, Received, Complete }
    ///   3a. POST  AttachmentUpload/{id}/complete        { Target, Fields }       -> the new row (like an OData POST, without Base64)
    ///   3b. or put "UploadID" instead of "Base64" in an attachment of POST JobRequest/Create (JobRequestController).
    ///   DELETE    AttachmentUpload/{id}                                          cancel
    /// Target: Attachment | JobRequestAttachment | JobAttachment. Fields are the row's other columns (only the listed ones are taken).
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("beesuite/api/[controller]")]
    public class AttachmentUploadController : ControllerBase
    {
        private readonly AttachmentUploadStore _store;
        private readonly BeesuiteDbContext _db;

        public AttachmentUploadController(AttachmentUploadStore store, BeesuiteDbContext db)
        {
            _store = store;
            _db = db;
        }

        /// <summary>The user an upload belongs to (same claims DatabaseRestoreController reads).</summary>
        public static string Caller(ClaimsPrincipal user) =>
            user.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
            ?? user.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? user.FindFirst("sub")?.Value
            ?? user.Identity?.Name
            ?? "";

        public class CreateBody { public long TotalSize { get; set; } }
        public class CompleteBody { public string? Target { get; set; } public JsonElement? Fields { get; set; } }

        private IActionResult Fail(AttachmentUploadStore.UploadException e) => StatusCode(e.Status, new { error = e.Message });

        [HttpPost]
        public IActionResult Create([FromBody] CreateBody body)
        {
            try
            {
                var m = _store.Create(Caller(User), body?.TotalSize ?? 0);
                return StatusCode(201, new { m.UploadID, m.ChunkSize, m.TotalChunks, m.TotalSize, MaxFileBytes = AttachmentUploadStore.MaxFileBytes });
            }
            catch (AttachmentUploadStore.UploadException e) { return Fail(e); }
        }

        [HttpGet("{id}")]
        public IActionResult Status(string id)
        {
            try
            {
                var m = _store.Get(id, Caller(User));
                var received = _store.Received(m);
                return Ok(new { m.UploadID, m.TotalChunks, Received = received, Complete = received.Count == m.TotalChunks });
            }
            catch (AttachmentUploadStore.UploadException e) { return Fail(e); }
        }

        [HttpPut("{id}/chunks/{index:int}")]
        [RequestSizeLimit(AttachmentUploadStore.ChunkBytes + 64 * 1024)]
        public async Task<IActionResult> Chunk(string id, int index, CancellationToken ct)
        {
            try
            {
                var m = _store.Get(id, Caller(User));
                var received = await _store.SaveChunk(m, index, Request.Body, ct);
                return Ok(new { m.UploadID, m.TotalChunks, Received = received, Complete = received.Count == m.TotalChunks });
            }
            catch (AttachmentUploadStore.UploadException e) { return Fail(e); }
        }

        [HttpDelete("{id}")]
        public IActionResult Cancel(string id)
        {
            try
            {
                _store.Get(id, Caller(User));
                _store.Delete(id);
                return NoContent();
            }
            catch (AttachmentUploadStore.UploadException e) { return Fail(e); }
        }

        [HttpPost("{id}/complete")]
        public async Task<IActionResult> Complete(string id, [FromBody] CompleteBody body, CancellationToken ct)
        {
            AttachmentUploadStore.Meta m;
            string text;
            try
            {
                m = _store.Get(id, Caller(User));
                text = await _store.ReadText(m, ct);
            }
            catch (AttachmentUploadStore.UploadException e) { return Fail(e); }

            var f = body?.Fields is { ValueKind: JsonValueKind.Object } o ? o : default;
            string? S(string name) => f.ValueKind == JsonValueKind.Object && f.TryGetProperty(name, out var v) && v.ValueKind == JsonValueKind.String ? v.GetString() : null;
            int? I(string name) => f.ValueKind == JsonValueKind.Object && f.TryGetProperty(name, out var v) && v.ValueKind == JsonValueKind.Number && v.TryGetInt32(out var n) ? n : null;
            bool? B(string name) => f.ValueKind == JsonValueKind.Object && f.TryGetProperty(name, out var v) && (v.ValueKind == JsonValueKind.True || v.ValueKind == JsonValueKind.False) ? v.GetBoolean() : null;

            object row;
            switch (body?.Target)
            {
                case "Attachment":
                    row = new Attachment { FileName = S("FileName"), Base64 = text, Path = S("Path"), MimeType = S("MimeType"), IsBinary = B("IsBinary") ?? true, Type = I("Type") };
                    break;
                case "JobRequestAttachment":
                    if (I("JobRequestID") is null) return BadRequest(new { error = "Fields.JobRequestID is required." });
                    row = new JobRequestAttachment
                    {
                        JobRequestID = I("JobRequestID"), Name = S("Name"), MimeType = S("MimeType"), Kind = S("Kind"), Base64 = text,
                        FileSize = I("FileSize"), FileExtension = S("FileExtension"), SortOrder = I("SortOrder") ?? 0,
                        StorageProvider = S("StorageProvider") ?? "database", PreviewUrl = S("PreviewUrl") ?? "",
                        BucketName = S("BucketName"), StorageKey = S("StorageKey"), CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now,
                    };
                    break;
                case "JobAttachment":
                    if (I("JobID") is null) return BadRequest(new { error = "Fields.JobID is required." });
                    row = new JobAttachment
                    {
                        JobID = I("JobID"), Name = S("Name"), MimeType = S("MimeType"), Kind = S("Kind"), Base64 = text,
                        FileSize = I("FileSize"), FileExtension = S("FileExtension"), SortOrder = I("SortOrder") ?? 0,
                        StorageProvider = S("StorageProvider") ?? "database", PreviewUrl = S("PreviewUrl") ?? "",
                        BucketName = S("BucketName"), StorageKey = S("StorageKey"), CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now,
                    };
                    break;
                default:
                    return BadRequest(new { error = "Target must be Attachment, JobRequestAttachment or JobAttachment." });
            }

            await _db.AddAsync(row, ct);
            await _db.SaveChangesAsync(ct);
            _store.Delete(id);

            // the new row as an OData POST would return it, minus the Base64 the client already has
            var json = JsonSerializer.SerializeToElement(row, row.GetType());
            var result = new Dictionary<string, object?>();
            foreach (var p in json.EnumerateObject())
                if (p.Name != "Base64" && p.Value.ValueKind != JsonValueKind.Object && p.Value.ValueKind != JsonValueKind.Array) result[p.Name] = p.Value;
            return StatusCode(201, result);
        }
    }
}
