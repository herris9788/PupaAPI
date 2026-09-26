using System.Text;
using System.Text.RegularExpressions;
using Npgsql;

namespace Pupa.Services
{
    /// <summary>
    /// Replace isi sebuah database PostgreSQL (DESTINATION) dengan salinan database lain (SOURCE) di server yang sama,
    /// dengan progress yang bisa dipantau. Murni Npgsql -- tidak butuh pg_dump/pg_restore terpasang di server Pupa.
    ///
    /// ATURAN KESELAMATAN (semuanya dicek di sisi server, bukan hanya di UI):
    ///   * Database produksi ("beesuite" + database dari connection string "Beesuite") TIDAK PERNAH boleh jadi destination.
    ///   * Destination harus cocok pola <c>DatabaseRestore:AllowedDestinationPattern</c> (default ^beesuite.+$) dan sudah ada.
    ///   * Source hanya dibaca (transaksi READ ONLY REPEATABLE READ = snapshot konsisten), tidak ada satu pun perubahan ke source.
    ///   * Destination diganti dalam SATU transaksi: kalau gagal/dibatalkan, isi lama destination kembali utuh (rollback).
    ///   * Sebelum menyentuh destination, current_database() koneksinya dicek ulang terhadap daftar terlarang.
    ///
    /// Yang disalin: schema, enum, sequence (+ nilainya), tabel + kolom (default/identity), data, PK/unique/check/FK, index,
    /// function, trigger, comment. Yang TIDAK disalin (sumber ditolak kalau memakainya): view, partisi, inheritance, domain,
    /// rule, RLS, generated column, collation khusus. Grant/owner tidak disalin (semua objek jadi milik user koneksi).
    /// </summary>
    public class DatabaseRestoreService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<DatabaseRestoreService> _log;
        private readonly object _lock = new();
        private readonly Dictionary<string, RestoreJob> _jobs = new();
        private string? _latestJobId;
        private string? _runningJobId;

        public DatabaseRestoreService(IConfiguration config, ILogger<DatabaseRestoreService> log)
        {
            _config = config;
            _log = log;
        }

        // ── Konfigurasi & aturan ────────────────────────────────────────────────────────────────────────────────────

        private string BaseConnectionString =>
            _config.GetConnectionString("Beesuite") ?? throw new InvalidOperationException("Connection string 'Beesuite' is missing.");

        /// <summary>Nama database produksi (tidak boleh jadi destination).</summary>
        public HashSet<string> ProtectedDatabases()
        {
            var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "beesuite", "postgres", "template0", "template1" };
            var prod = new NpgsqlConnectionStringBuilder(BaseConnectionString).Database;
            if (!string.IsNullOrWhiteSpace(prod)) set.Add(prod!);
            return set;
        }

        private Regex AllowedDestination()
        {
            var pattern = _config["DatabaseRestore:AllowedDestinationPattern"];
            if (string.IsNullOrWhiteSpace(pattern)) pattern = "^beesuite.+$";
            return new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        }

        public string ProductionName => new NpgsqlConnectionStringBuilder(BaseConnectionString).Database ?? "beesuite";

        public bool IsAllowedDestination(string name) =>
            !string.IsNullOrWhiteSpace(name) && !ProtectedDatabases().Contains(name) && AllowedDestination().IsMatch(name);

        private string ConnFor(string database)
        {
            var b = new NpgsqlConnectionStringBuilder(BaseConnectionString)
            {
                Database = database,
                Pooling = false,
                CommandTimeout = 0,
                Timeout = 30,
                KeepAlive = 30,
                IncludeErrorDetail = true,
            };
            return b.ConnectionString;
        }

        public async Task<List<RestoreDatabaseInfo>> ListDatabasesAsync(CancellationToken ct)
        {
            await using var c = new NpgsqlConnection(ConnFor("postgres"));
            await c.OpenAsync(ct);
            var list = new List<RestoreDatabaseInfo>();
            await using var cmd = new NpgsqlCommand(
                "select datname, pg_database_size(datname) from pg_database where not datistemplate and datallowconn order by 1", c);
            await using var r = await cmd.ExecuteReaderAsync(ct);
            while (await r.ReadAsync(ct))
            {
                var name = r.GetString(0);
                list.Add(new RestoreDatabaseInfo
                {
                    Name = name,
                    SizeBytes = r.GetInt64(1),
                    IsProduction = ProtectedDatabases().Contains(name) && name.Equals(ProductionName, StringComparison.OrdinalIgnoreCase),
                    CanBeDestination = IsAllowedDestination(name),
                });
            }
            return list;
        }

        /// <summary>Tabel di source beserta ukurannya -- untuk memilih tabel yang datanya dilewati.</summary>
        public async Task<List<RestoreTableInfo>> ListTablesAsync(string database, CancellationToken ct)
        {
            await using var c = new NpgsqlConnection(ConnFor(database));
            await c.OpenAsync(ct);
            var list = new List<RestoreTableInfo>();
            await using var cmd = new NpgsqlCommand(@"
select n.nspname, c.relname, pg_table_size(c.oid), c.reltuples::bigint
from pg_class c join pg_namespace n on n.oid = c.relnamespace
where c.relkind = 'r' and n.nspname not like 'pg\_%' and n.nspname <> 'information_schema'
order by pg_table_size(c.oid) desc", c);
            await using var r = await cmd.ExecuteReaderAsync(ct);
            while (await r.ReadAsync(ct))
                list.Add(new RestoreTableInfo { Schema = r.GetString(0), Name = r.GetString(1), SizeBytes = r.GetInt64(2), EstimatedRows = r.GetInt64(3) });
            return list;
        }

        // ── Job API ─────────────────────────────────────────────────────────────────────────────────────────────────

        public RestoreJob? Get(string? jobId)
        {
            lock (_lock)
            {
                var id = string.IsNullOrEmpty(jobId) ? _latestJobId : jobId;
                return id != null && _jobs.TryGetValue(id, out var j) ? j : null;
            }
        }

        public RestoreJobSnapshot? Snapshot(string? jobId)
        {
            var job = Get(jobId);
            if (job == null) return null;
            lock (job.Sync) return job.ToSnapshot();
        }

        public bool Cancel(string? jobId)
        {
            var job = Get(jobId);
            if (job == null || job.State != RestoreState.Running) return false;
            job.Cts.Cancel();
            job.Append("Cancel requested -- rolling back the destination…");
            return true;
        }

        /// <summary>Validasi lalu jalankan di background. Melempar <see cref="InvalidOperationException"/> (pesan siap tampil) kalau ditolak.</summary>
        public async Task<RestoreJobSnapshot> StartAsync(RestoreRequest req, string startedBy, CancellationToken ct)
        {
            var source = (req.Source ?? "").Trim();
            var dest = (req.Destination ?? "").Trim();
            if (source.Length == 0 || dest.Length == 0) throw new InvalidOperationException("Source and destination are required.");
            if (source.Equals(dest, StringComparison.OrdinalIgnoreCase)) throw new InvalidOperationException("Source and destination must be different databases.");
            if (ProtectedDatabases().Contains(dest))
                throw new InvalidOperationException($"\"{dest}\" is a protected database and can never be a destination.");
            if (!IsAllowedDestination(dest))
                throw new InvalidOperationException($"\"{dest}\" is not an allowed destination (pattern {AllowedDestination()}).");
            if (!string.Equals(req.Confirm?.Trim(), dest, StringComparison.Ordinal))
                throw new InvalidOperationException("Confirmation does not match the destination database name.");

            var dbs = await ListDatabasesAsync(ct);
            if (!dbs.Any(d => d.Name.Equals(source, StringComparison.OrdinalIgnoreCase))) throw new InvalidOperationException($"Source database \"{source}\" does not exist.");
            var destInfo = dbs.FirstOrDefault(d => d.Name.Equals(dest, StringComparison.Ordinal))
                           ?? throw new InvalidOperationException($"Destination database \"{dest}\" does not exist (create it first).");
            source = dbs.First(d => d.Name.Equals(source, StringComparison.OrdinalIgnoreCase)).Name;
            _ = destInfo;

            RestoreJob job;
            lock (_lock)
            {
                if (_runningJobId != null && _jobs.TryGetValue(_runningJobId, out var running) && running.State == RestoreState.Running)
                    throw new InvalidOperationException($"Another restore is already running ({running.Source} → {running.Destination}).");
                job = new RestoreJob
                {
                    Id = Guid.NewGuid().ToString("N"),
                    Source = source,
                    Destination = dest,
                    StartedBy = startedBy,
                    StartedAt = DateTime.UtcNow,
                    State = RestoreState.Running,
                    ExcludeData = new HashSet<string>(req.ExcludeDataTables ?? new List<string>(), StringComparer.OrdinalIgnoreCase),
                };
                _jobs[job.Id] = job;
                _latestJobId = job.Id;
                _runningJobId = job.Id;
            }

            _ = Task.Run(() => RunAsync(job));
            lock (job.Sync) return job.ToSnapshot();
        }

        // ── Runner ──────────────────────────────────────────────────────────────────────────────────────────────────

        private async Task RunAsync(RestoreJob job)
        {
            var ct = job.Cts.Token;
            try
            {
                job.Append($"Restore started by {job.StartedBy}: {job.Source} → {job.Destination}");
                await CloneAsync(job, ct);
                job.Finish(RestoreState.Succeeded, "Done.");
            }
            catch (OperationCanceledException)
            {
                job.Finish(RestoreState.Cancelled, "Cancelled -- the destination was rolled back to its previous content.");
            }
            catch (Exception e)
            {
                _log.LogError(e, "Database restore {Source} -> {Dest} failed", job.Source, job.Destination);
                job.Fail(e);
            }
            finally
            {
                lock (_lock) { if (_runningJobId == job.Id) _runningJobId = null; }
            }
        }

        private static string Q(string id) => "\"" + id.Replace("\"", "\"\"") + "\"";
        private static string Lit(string s) => "'" + s.Replace("'", "''") + "'";
        private static string QT(string schema, string table) => Q(schema) + "." + Q(table);
        private const string UserSchemaFilter = "nspname not like 'pg\\_%' and nspname <> 'information_schema'";

        private static async Task ExecAsync(NpgsqlConnection c, string sql, CancellationToken ct)
        {
            await using var cmd = new NpgsqlCommand(sql, c) { CommandTimeout = 0 };
            await cmd.ExecuteNonQueryAsync(ct);
        }

        private async Task CloneAsync(RestoreJob job, CancellationToken ct)
        {
            // ── 0. Koneksi ──
            job.SetPhase("Connecting", 0, "Connecting to source and destination…");
            await using var src = new NpgsqlConnection(ConnFor(job.Source));
            await using var dst = new NpgsqlConnection(ConnFor(job.Destination));
            await src.OpenAsync(ct);
            await dst.OpenAsync(ct);

            // Pengaman terakhir: pastikan koneksi destination BENAR-BENAR bukan database yang dilindungi.
            string actualDest;
            await using (var cmd = new NpgsqlCommand("select current_database()", dst)) actualDest = (string)(await cmd.ExecuteScalarAsync(ct))!;
            if (ProtectedDatabases().Contains(actualDest) || !IsAllowedDestination(actualDest) || !actualDest.Equals(job.Destination, StringComparison.Ordinal))
                throw new InvalidOperationException($"Refusing to write to \"{actualDest}\".");

            // Source: snapshot konsisten, hanya-baca.
            await ExecAsync(src, "BEGIN ISOLATION LEVEL REPEATABLE READ, READ ONLY", ct);
            await ExecAsync(src, "SET LOCAL search_path = ''", ct);
            await ExecAsync(src, "SET LOCAL statement_timeout = 0", ct);
            await ExecAsync(src, "SET LOCAL idle_in_transaction_session_timeout = 0", ct);

            // ── 1. Baca metadata source ──
            job.SetPhase("Reading source schema", 1, "Reading the source schema…");
            var meta = await ReadMetadataAsync(src, job, ct);
            job.InitTables(meta.Tables.Select(t => new RestoreTableState
            {
                Name = $"{t.Schema}.{t.Name}",
                SizeBytes = t.SizeBytes,
                State = job.ExcludeData.Contains($"{t.Schema}.{t.Name}") ? "skipped" : "pending",
            }).ToList());
            var dataBytesTotal = Math.Max(1L, meta.Tables.Where(t => !job.ExcludeData.Contains($"{t.Schema}.{t.Name}")).Sum(t => t.SizeBytes));
            job.Append($"Source has {meta.Tables.Count} tables, {meta.Sequences.Count} sequences, {meta.Functions.Count} functions " +
                       $"({FormatBytes(dataBytesTotal)} of table data to copy, {job.ExcludeData.Count} table(s) data skipped).");

            // ── 2. Destination: satu transaksi ──
            await ExecAsync(dst, "BEGIN", ct);
            try
            {
                await ExecAsync(dst, "SET LOCAL search_path = ''", ct);
                await ExecAsync(dst, "SET LOCAL lock_timeout = '60s'", ct);
                await ExecAsync(dst, "SET LOCAL statement_timeout = 0", ct);
                await ExecAsync(dst, "SET LOCAL idle_in_transaction_session_timeout = 0", ct);
                await ExecAsync(dst, "SET LOCAL check_function_bodies = off", ct);
                await ExecAsync(dst, "SET LOCAL synchronous_commit = off", ct);
                await ExecAsync(dst, "SET LOCAL maintenance_work_mem = '512MB'", ct);

                // 2a. Kosongkan destination
                job.SetPhase("Clearing destination", 3, $"Dropping existing objects in {job.Destination}…");
                var existing = new List<string>();
                await using (var cmd = new NpgsqlCommand($"select nspname from pg_namespace where {UserSchemaFilter}", dst))
                await using (var r = await cmd.ExecuteReaderAsync(ct))
                    while (await r.ReadAsync(ct)) existing.Add(r.GetString(0));
                foreach (var s in existing)
                {
                    ct.ThrowIfCancellationRequested();
                    await ExecAsync(dst, $"DROP SCHEMA {Q(s)} CASCADE", ct);
                }
                job.Append($"Dropped {existing.Count} schema(s) in the destination.");

                // 2b. Struktur
                job.SetPhase("Creating structure", 5, "Creating schemas, types, sequences and tables…");
                foreach (var ext in meta.Extensions) await ExecAsync(dst, $"CREATE EXTENSION IF NOT EXISTS {Q(ext)}", ct);
                foreach (var s in meta.Schemas) await ExecAsync(dst, $"CREATE SCHEMA {Q(s)}", ct);
                foreach (var e in meta.Enums)
                    await ExecAsync(dst, $"CREATE TYPE {QT(e.Schema, e.Name)} AS ENUM ({string.Join(", ", e.Labels.Select(Lit))})", ct);
                foreach (var sq in meta.Sequences.Where(x => x.OwnerKind != 'i'))
                    await ExecAsync(dst, sq.CreateSql(), ct);
                foreach (var t in meta.Tables)
                {
                    ct.ThrowIfCancellationRequested();
                    await ExecAsync(dst, t.CreateSql(), ct);
                }
                foreach (var sq in meta.Sequences.Where(x => x.OwnerKind == 'a'))
                    await ExecAsync(dst, $"ALTER SEQUENCE {QT(sq.Schema, sq.Name)} OWNED BY {QT(sq.OwnerSchema!, sq.OwnerTable!)}.{Q(sq.OwnerColumn!)}", ct);
                foreach (var f in meta.Functions) await ExecAsync(dst, f, ct);
                job.Append($"Created {meta.Tables.Count} tables.");

                // 2c. Data
                job.SetPhase("Copying data", 6, "Copying table data…");
                long copied = 0;
                foreach (var t in meta.Tables)
                {
                    ct.ThrowIfCancellationRequested();
                    var full = $"{t.Schema}.{t.Name}";
                    if (job.ExcludeData.Contains(full)) { job.Append($"Skipped data of {full}."); continue; }
                    var cols = string.Join(", ", t.Columns.Select(c => Q(c.Name)));
                    var qt = QT(t.Schema, t.Name);
                    job.TableStart(full);
                    long tableBytes = 0;
                    var lastReport = DateTime.UtcNow;
                    await using (var input = await src.BeginRawBinaryCopyAsync($"COPY {qt} ({cols}) TO STDOUT (FORMAT BINARY)", ct))
                    await using (var output = await dst.BeginRawBinaryCopyAsync($"COPY {qt} ({cols}) FROM STDIN (FORMAT BINARY)", ct))
                    {
                        var buf = new byte[256 * 1024];
                        int n;
                        while ((n = await input.ReadAsync(buf, 0, buf.Length, ct)) > 0)
                        {
                            await output.WriteAsync(buf, 0, n, ct);
                            tableBytes += n;
                            if ((DateTime.UtcNow - lastReport).TotalMilliseconds > 500)
                            {
                                lastReport = DateTime.UtcNow;
                                var within = t.SizeBytes > 0 ? Math.Min(0.98, (double)tableBytes / t.SizeBytes) * t.SizeBytes : 0;
                                job.DataProgress(full, tableBytes, copied + (long)within, dataBytesTotal);
                            }
                        }
                    }
                    long rows = 0;
                    await using (var cmd = new NpgsqlCommand("select coalesce(pg_stat_get_xact_tuples_inserted($1::regclass), 0)", dst))
                    {
                        cmd.Parameters.AddWithValue(qt);
                        rows = Convert.ToInt64(await cmd.ExecuteScalarAsync(ct));
                    }
                    copied += t.SizeBytes;
                    job.TableDone(full, tableBytes, rows, copied, dataBytesTotal);
                }
                job.Append("Data copy finished.");

                // 2d. Constraint, index, FK, trigger
                job.SetPhase("Building constraints and indexes", 62, "Creating primary keys, unique constraints, indexes…");
                var post = new List<(string label, string sql, long weight)>();
                foreach (var t in meta.Tables)
                {
                    var w = Math.Max(1L << 20, t.SizeBytes);
                    foreach (var x in t.KeyConstraintSql) post.Add(($"{t.Schema}.{t.Name}", x, w));
                    foreach (var x in t.IndexSql) post.Add(($"{t.Schema}.{t.Name}", x, w));
                }
                foreach (var t in meta.Tables)
                    foreach (var x in t.ForeignKeySql) post.Add(($"{t.Schema}.{t.Name}", x, Math.Max(1L << 20, t.SizeBytes / 3)));
                foreach (var t in meta.Tables)
                    foreach (var x in t.TriggerSql) post.Add(($"{t.Schema}.{t.Name}", x, 1L << 20));
                foreach (var t in meta.Tables)
                    foreach (var x in t.CommentSql) post.Add(($"{t.Schema}.{t.Name}", x, 1L << 10));
                var postTotal = Math.Max(1L, post.Sum(p => p.weight));
                long postDone = 0;
                int step = 0;
                foreach (var p in post)
                {
                    ct.ThrowIfCancellationRequested();
                    job.PostProgress(p.label, ++step, post.Count, postDone, postTotal);
                    await ExecAsync(dst, p.sql, ct);
                    postDone += p.weight;
                }
                job.PostProgress(null, post.Count, post.Count, postTotal, postTotal);

                // 2e. Nilai sequence -- dibaca SETELAH data disalin supaya tidak pernah lebih kecil dari id yang ada.
                job.SetPhase("Syncing sequences", 96, "Copying sequence values…");
                foreach (var sq in meta.Sequences)
                {
                    long? last = null;
                    await using (var cmd = new NpgsqlCommand($"select pg_catalog.pg_sequence_last_value({Lit(QT(sq.Schema, sq.Name))}::regclass)", src))
                    {
                        var v = await cmd.ExecuteScalarAsync(ct);
                        if (v != null && v != DBNull.Value) last = Convert.ToInt64(v);
                    }
                    if (last == null) continue;
                    string target = QT(sq.Schema, sq.Name);
                    if (sq.OwnerKind == 'i')
                    {
                        await using var cmd = new NpgsqlCommand($"select pg_catalog.pg_get_serial_sequence({Lit(QT(sq.OwnerSchema!, sq.OwnerTable!))}, {Lit(sq.OwnerColumn!)})", dst);
                        target = (string)(await cmd.ExecuteScalarAsync(ct))!;
                        await ExecAsync(dst, $"ALTER SEQUENCE {target} {sq.OptionsSql()}", ct);
                    }
                    await ExecAsync(dst, $"select pg_catalog.setval({Lit(target)}::regclass, {last.Value}, true)", ct);
                }

                // Sanity terakhir sebelum commit: connection ini masih di database yang benar.
                await using (var cmd = new NpgsqlCommand("select current_database()", dst))
                    if ((string)(await cmd.ExecuteScalarAsync(ct))! != job.Destination) throw new InvalidOperationException("Destination connection changed database unexpectedly.");

                job.SetPhase("Committing", 97, "Committing the destination (this can take a moment)…");
                ct.ThrowIfCancellationRequested();
                await ExecAsync(dst, "COMMIT", ct);
            }
            catch
            {
                try { await ExecAsync(dst, "ROLLBACK", CancellationToken.None); } catch { /* koneksi mati = server sudah rollback sendiri */ }
                throw;
            }
            finally
            {
                try { await ExecAsync(src, "ROLLBACK", CancellationToken.None); } catch { }
            }

            // ── 3. ANALYZE (di luar transaksi, gagal tidak fatal) ──
            job.SetPhase("Analyzing", 97, "Updating table statistics…");
            try
            {
                int i = 0;
                foreach (var t in meta.Tables)
                {
                    ct.ThrowIfCancellationRequested();
                    await ExecAsync(dst, $"ANALYZE {QT(t.Schema, t.Name)}", ct);
                    job.SetPercent(97 + 3.0 * (++i) / Math.Max(1, meta.Tables.Count));
                }
            }
            catch (OperationCanceledException) { job.Append("Analyze skipped (cancelled); the data itself is already committed."); }
            catch (Exception e) { job.Append("Analyze failed (non-fatal): " + e.Message); }
        }

        // ── Metadata source ─────────────────────────────────────────────────────────────────────────────────────────

        private sealed class Meta
        {
            public List<string> Schemas = new();
            public List<string> Extensions = new();
            public List<EnumDef> Enums = new();
            public List<SeqDef> Sequences = new();
            public List<TableDef> Tables = new();
            public List<string> Functions = new();
        }
        private sealed class EnumDef { public string Schema = ""; public string Name = ""; public string[] Labels = Array.Empty<string>(); }
        private sealed class ColDef
        {
            public string Name = ""; public string Type = ""; public bool NotNull; public string? Default; public char Identity;
        }
        private sealed class SeqDef
        {
            public string Schema = "", Name = "", TypeName = "";
            public long Start, Min, Max, Inc, Cache; public bool Cycle;
            public char OwnerKind; // 'a' = OWNED BY kolom (serial), 'i' = sequence identity, '\0' = mandiri
            public string? OwnerSchema, OwnerTable, OwnerColumn;
            public string OptionsSql() => $"INCREMENT BY {Inc} MINVALUE {Min} MAXVALUE {Max} START WITH {Start} CACHE {Cache} {(Cycle ? "CYCLE" : "NO CYCLE")}";
            public string CreateSql() => $"CREATE SEQUENCE {QT(Schema, Name)} AS {TypeName} {OptionsSql()}";
        }
        private sealed class TableDef
        {
            public string Schema = "", Name = ""; public bool Unlogged; public long SizeBytes;
            public List<ColDef> Columns = new();
            public List<string> KeyConstraintSql = new(), ForeignKeySql = new(), IndexSql = new(), TriggerSql = new(), CommentSql = new();
            public string CreateSql()
            {
                var sb = new StringBuilder();
                sb.Append("CREATE ").Append(Unlogged ? "UNLOGGED " : "").Append("TABLE ").Append(QT(Schema, Name)).Append(" (\n");
                for (int i = 0; i < Columns.Count; i++)
                {
                    var c = Columns[i];
                    sb.Append("  ").Append(Q(c.Name)).Append(' ').Append(c.Type);
                    if (c.Identity == 'a') sb.Append(" GENERATED ALWAYS AS IDENTITY");
                    else if (c.Identity == 'd') sb.Append(" GENERATED BY DEFAULT AS IDENTITY");
                    else if (c.Default != null) sb.Append(" DEFAULT ").Append(c.Default);
                    if (c.NotNull) sb.Append(" NOT NULL");
                    if (i < Columns.Count - 1) sb.Append(',');
                    sb.Append('\n');
                }
                sb.Append(')');
                return sb.ToString();
            }
        }

        private static async Task<List<T>> QueryAsync<T>(NpgsqlConnection c, string sql, Func<NpgsqlDataReader, T> map, CancellationToken ct)
        {
            var list = new List<T>();
            await using var cmd = new NpgsqlCommand(sql, c) { CommandTimeout = 0 };
            await using var r = await cmd.ExecuteReaderAsync(ct);
            while (await r.ReadAsync(ct)) list.Add(map(r));
            return list;
        }

        private async Task<Meta> ReadMetadataAsync(NpgsqlConnection src, RestoreJob job, CancellationToken ct)
        {
            var m = new Meta();
            const string relNs = "n.nspname not like 'pg\\_%' and n.nspname <> 'information_schema'";

            // Sumber yang memakai fitur yang belum didukung ditolak, bukan disalin setengah-setengah.
            var problems = new List<string>();
            async Task Check(string what, string sql)
            {
                var n = (await QueryAsync(src, sql, r => r.GetInt64(0), ct)).FirstOrDefault();
                if (n > 0) problems.Add($"{what} ({n})");
            }
            await Check("views / materialized views / partitioned tables / foreign tables / composite types",
                $"select count(*) from pg_class c join pg_namespace n on n.oid = c.relnamespace where {relNs} and c.relkind in ('v','m','p','f','c','I')");
            await Check("table inheritance", "select count(*) from pg_inherits");
            await Check("domains / range types",
                $"select count(*) from pg_type t join pg_namespace n on n.oid = t.typnamespace where {relNs} and t.typtype in ('d','r','m')");
            await Check("rules", $"select count(*) from pg_rewrite r join pg_class c on c.oid = r.ev_class join pg_namespace n on n.oid = c.relnamespace where {relNs} and c.relkind = 'r' and r.rulename <> '_RETURN'");
            await Check("row level security policies", "select count(*) from pg_policy");
            await Check("event triggers", "select count(*) from pg_event_trigger");
            await Check("generated columns", $"select count(*) from pg_attribute a join pg_class c on c.oid = a.attrelid join pg_namespace n on n.oid = c.relnamespace where {relNs} and c.relkind = 'r' and a.attnum > 0 and not a.attisdropped and a.attgenerated <> ''");
            await Check("custom column collations", $"select count(*) from pg_attribute a join pg_class c on c.oid = a.attrelid join pg_namespace n on n.oid = c.relnamespace join pg_type t on t.oid = a.atttypid where {relNs} and c.relkind = 'r' and a.attnum > 0 and not a.attisdropped and a.attcollation <> 0 and a.attcollation <> t.typcollation");
            await Check("extended statistics", "select count(*) from pg_statistic_ext");
            await Check("aggregates / non-plpgsql-sql functions", $"select count(*) from pg_proc p join pg_namespace n on n.oid = p.pronamespace join pg_language l on l.oid = p.prolang where {relNs} and (p.prokind not in ('f','p') or l.lanname not in ('plpgsql','sql'))");
            if (problems.Count > 0)
                throw new InvalidOperationException("The source database uses features the restore does not support: " + string.Join("; ", problems) + ".");

            m.Schemas = await QueryAsync(src, $"select nspname from pg_namespace n where {relNs} order by 1", r => r.GetString(0), ct);
            m.Extensions = await QueryAsync(src, "select extname from pg_extension where extname <> 'plpgsql' order by 1", r => r.GetString(0), ct);
            m.Enums = await QueryAsync(src, $@"
select n.nspname, t.typname, array_agg(e.enumlabel::text order by e.enumsortorder)
from pg_type t join pg_namespace n on n.oid = t.typnamespace join pg_enum e on e.enumtypid = t.oid
where {relNs} group by 1, 2 order by 1, 2",
                r => new EnumDef { Schema = r.GetString(0), Name = r.GetString(1), Labels = r.GetFieldValue<string[]>(2) }, ct);

            // Sequence + pemiliknya
            m.Sequences = await QueryAsync(src, $@"
select n.nspname, s.relname, format_type(q.seqtypid, null), q.seqstart, q.seqmin, q.seqmax, q.seqincrement, q.seqcache, q.seqcycle,
       d.deptype, tn.nspname, t.relname, a.attname
from pg_class s
join pg_namespace n on n.oid = s.relnamespace
join pg_sequence q on q.seqrelid = s.oid
left join pg_depend d on d.classid = 'pg_class'::regclass and d.objid = s.oid and d.refclassid = 'pg_class'::regclass and d.deptype in ('a','i')
left join pg_class t on t.oid = d.refobjid
left join pg_namespace tn on tn.oid = t.relnamespace
left join pg_attribute a on a.attrelid = t.oid and a.attnum = d.refobjsubid
where s.relkind = 'S' and {relNs} order by 1, 2",
                r => new SeqDef
                {
                    Schema = r.GetString(0), Name = r.GetString(1), TypeName = r.GetString(2),
                    Start = r.GetInt64(3), Min = r.GetInt64(4), Max = r.GetInt64(5), Inc = r.GetInt64(6), Cache = r.GetInt64(7), Cycle = r.GetBoolean(8),
                    OwnerKind = r.IsDBNull(9) ? '\0' : Convert.ToChar(r.GetFieldValue<char>(9)),
                    OwnerSchema = r.IsDBNull(10) ? null : r.GetString(10), OwnerTable = r.IsDBNull(11) ? null : r.GetString(11), OwnerColumn = r.IsDBNull(12) ? null : r.GetString(12),
                }, ct);

            // Tabel + kolom
            var tables = await QueryAsync(src, $@"
select c.oid::bigint, n.nspname, c.relname, c.relpersistence = 'u', pg_table_size(c.oid)
from pg_class c join pg_namespace n on n.oid = c.relnamespace
where c.relkind = 'r' and {relNs} order by 2, 3",
                r => (oid: r.GetInt64(0), t: new TableDef { Schema = r.GetString(1), Name = r.GetString(2), Unlogged = r.GetBoolean(3), SizeBytes = r.GetInt64(4) }), ct);
            var byOid = tables.ToDictionary(x => x.oid, x => x.t);
            m.Tables = tables.Select(x => x.t).ToList();

            var cols = await QueryAsync(src, $@"
select a.attrelid::bigint, a.attname, format_type(a.atttypid, a.atttypmod), a.attnotnull, pg_get_expr(d.adbin, d.adrelid), a.attidentity::text
from pg_attribute a
join pg_class c on c.oid = a.attrelid join pg_namespace n on n.oid = c.relnamespace
left join pg_attrdef d on d.adrelid = a.attrelid and d.adnum = a.attnum
where c.relkind = 'r' and {relNs} and a.attnum > 0 and not a.attisdropped
order by a.attrelid, a.attnum",
                r => (oid: r.GetInt64(0), c: new ColDef
                {
                    Name = r.GetString(1), Type = r.GetString(2), NotNull = r.GetBoolean(3),
                    Default = r.IsDBNull(4) ? null : r.GetString(4), Identity = string.IsNullOrEmpty(r.GetString(5)) ? '\0' : r.GetString(5)[0],
                }), ct);
            foreach (var (oid, c) in cols) if (byOid.TryGetValue(oid, out var t)) t.Columns.Add(c);

            // Constraint: PK/unique/exclude/check dulu, FK terpisah (dibuat setelah semua tabel punya PK/unique)
            var cons = await QueryAsync(src, $@"
select k.conrelid::bigint, k.contype::text, k.conname, pg_get_constraintdef(k.oid)
from pg_constraint k join pg_class c on c.oid = k.conrelid join pg_namespace n on n.oid = c.relnamespace
where k.contype in ('p','u','x','c','f') and c.relkind = 'r' and {relNs}
order by k.conrelid, case k.contype when 'p' then 0 when 'u' then 1 when 'x' then 2 when 'c' then 3 else 4 end, k.conname",
                r => (oid: r.GetInt64(0), type: r.GetString(1), name: r.GetString(2), def: r.GetString(3)), ct);
            foreach (var k in cons)
            {
                if (!byOid.TryGetValue(k.oid, out var t)) continue;
                var sql = $"ALTER TABLE {QT(t.Schema, t.Name)} ADD CONSTRAINT {Q(k.name)} {k.def}";
                (k.type == "f" ? t.ForeignKeySql : t.KeyConstraintSql).Add(sql);
            }

            // Index yang bukan milik constraint
            var idx = await QueryAsync(src, $@"
select i.indrelid::bigint, pg_get_indexdef(i.indexrelid)
from pg_index i join pg_class tc on tc.oid = i.indrelid join pg_namespace n on n.oid = tc.relnamespace
where tc.relkind = 'r' and {relNs}
  and not exists (select 1 from pg_constraint k where k.conindid = i.indexrelid and k.contype in ('p','u','x'))
order by i.indrelid, i.indexrelid",
                r => (oid: r.GetInt64(0), def: r.GetString(1)), ct);
            foreach (var i in idx) if (byOid.TryGetValue(i.oid, out var t)) t.IndexSql.Add(i.def);

            // Trigger
            var trg = await QueryAsync(src, $@"
select g.tgrelid::bigint, pg_get_triggerdef(g.oid), g.tgname, g.tgenabled::text
from pg_trigger g join pg_class c on c.oid = g.tgrelid join pg_namespace n on n.oid = c.relnamespace
where not g.tgisinternal and c.relkind = 'r' and {relNs} order by g.tgrelid, g.tgname",
                r => (oid: r.GetInt64(0), def: r.GetString(1), name: r.GetString(2), enabled: r.GetString(3)), ct);
            foreach (var g in trg)
            {
                if (!byOid.TryGetValue(g.oid, out var t)) continue;
                t.TriggerSql.Add(g.def);
                if (g.enabled == "D") t.TriggerSql.Add($"ALTER TABLE {QT(t.Schema, t.Name)} DISABLE TRIGGER {Q(g.name)}");
            }

            // Comment tabel & kolom
            var cm = await QueryAsync(src, $@"
select d.objoid::bigint, d.objsubid, a.attname, d.description
from pg_description d join pg_class c on c.oid = d.objoid and d.classoid = 'pg_class'::regclass join pg_namespace n on n.oid = c.relnamespace
left join pg_attribute a on a.attrelid = c.oid and a.attnum = d.objsubid and d.objsubid > 0
where c.relkind = 'r' and {relNs} order by 1, 2",
                r => (oid: r.GetInt64(0), sub: r.GetInt32(1), col: r.IsDBNull(2) ? null : r.GetString(2), text: r.GetString(3)), ct);
            foreach (var c in cm)
            {
                if (!byOid.TryGetValue(c.oid, out var t)) continue;
                t.CommentSql.Add(c.sub == 0
                    ? $"COMMENT ON TABLE {QT(t.Schema, t.Name)} IS {Lit(c.text)}"
                    : $"COMMENT ON COLUMN {QT(t.Schema, t.Name)}.{Q(c.col!)} IS {Lit(c.text)}");
            }

            // Function
            m.Functions = await QueryAsync(src, $@"
select pg_get_functiondef(p.oid)
from pg_proc p join pg_namespace n on n.oid = p.pronamespace
where {relNs} and p.prokind in ('f','p')
  and not exists (select 1 from pg_depend d where d.classid = 'pg_proc'::regclass and d.objid = p.oid and d.deptype = 'e')
order by n.nspname, p.proname, p.oid",
                r => r.GetString(0), ct);

            return m;
        }

        internal static string FormatBytes(long b)
        {
            string[] u = { "B", "KB", "MB", "GB", "TB" };
            double v = b; int i = 0;
            while (v >= 1024 && i < u.Length - 1) { v /= 1024; i++; }
            return $"{v:0.#} {u[i]}";
        }
    }

    // ── DTO / state ─────────────────────────────────────────────────────────────────────────────────────────────────

    public enum RestoreState { Running, Succeeded, Failed, Cancelled }

    public class RestoreRequest
    {
        public string? Source { get; set; }
        public string? Destination { get; set; }
        /// <summary>Harus persis sama dengan nama destination (konfirmasi ketik ulang).</summary>
        public string? Confirm { get; set; }
        /// <summary>"schema.table" yang STRUKTURnya dibuat tapi DATAnya tidak disalin (mis. tabel log besar).</summary>
        public List<string>? ExcludeDataTables { get; set; }
    }

    public class RestoreDatabaseInfo
    {
        public string Name { get; set; } = "";
        public long SizeBytes { get; set; }
        public bool IsProduction { get; set; }
        public bool CanBeDestination { get; set; }
    }

    public class RestoreTableInfo
    {
        public string Schema { get; set; } = "";
        public string Name { get; set; } = "";
        public long SizeBytes { get; set; }
        public long EstimatedRows { get; set; }
    }

    public class RestoreTableState
    {
        public string Name { get; set; } = "";
        public long SizeBytes { get; set; }
        public string State { get; set; } = "pending"; // pending | copying | done | skipped
        public long BytesCopied { get; set; }
        public long Rows { get; set; }
    }

    public class RestoreJobSnapshot
    {
        public string Id { get; set; } = "";
        public string Source { get; set; } = "";
        public string Destination { get; set; } = "";
        public string StartedBy { get; set; } = "";
        public DateTime StartedAt { get; set; }
        public DateTime? FinishedAt { get; set; }
        public string State { get; set; } = "";
        public string Phase { get; set; } = "";
        public string Message { get; set; } = "";
        public double Percent { get; set; }
        public string? Error { get; set; }
        public string? CurrentTable { get; set; }
        public long DataBytesCopied { get; set; }
        public long DataBytesTotal { get; set; }
        public int TablesTotal { get; set; }
        public int TablesDone { get; set; }
        public int PostStep { get; set; }
        public int PostTotal { get; set; }
        public double ElapsedSeconds { get; set; }
        public List<string> Log { get; set; } = new();
        public List<RestoreTableState> Tables { get; set; } = new();
    }

    public class RestoreJob
    {
        public readonly object Sync = new();
        public readonly CancellationTokenSource Cts = new();
        public string Id = "", Source = "", Destination = "", StartedBy = "";
        public DateTime StartedAt;
        public DateTime? FinishedAt;
        public volatile RestoreState State;
        public HashSet<string> ExcludeData = new(StringComparer.OrdinalIgnoreCase);

        private string _phase = "", _message = "", _error = "", _currentTable = "";
        private double _percent;
        private long _dataCopied, _dataTotal;
        private int _postStep, _postTotal;
        private List<RestoreTableState> _tables = new();
        private readonly LinkedList<string> _log = new();

        public void Append(string line)
        {
            lock (Sync)
            {
                _log.AddLast($"{DateTime.UtcNow:HH:mm:ss} {line}");
                while (_log.Count > 400) _log.RemoveFirst();
            }
        }

        public void SetPhase(string phase, double percent, string message)
        {
            lock (Sync) { _phase = phase; _percent = Math.Max(_percent, percent); _message = message; }
            Append(message);
        }

        public void SetPercent(double p) { lock (Sync) _percent = Math.Max(_percent, Math.Min(99.9, p)); }

        public void InitTables(List<RestoreTableState> tables) { lock (Sync) { _tables = tables; _dataTotal = tables.Where(t => t.State != "skipped").Sum(t => t.SizeBytes); } }

        public void TableStart(string name)
        {
            lock (Sync)
            {
                _currentTable = name;
                _message = $"Copying {name}…";
                var t = _tables.FirstOrDefault(x => x.Name == name);
                if (t != null) t.State = "copying";
            }
        }

        // Data: 6% .. 62%
        public void DataProgress(string name, long tableBytes, long copiedTotal, long total)
        {
            lock (Sync)
            {
                var t = _tables.FirstOrDefault(x => x.Name == name);
                if (t != null) t.BytesCopied = tableBytes;
                _dataCopied = copiedTotal;
                _percent = Math.Max(_percent, 6 + 56.0 * Math.Min(1.0, (double)copiedTotal / total));
            }
        }

        public void TableDone(string name, long tableBytes, long rows, long copiedTotal, long total)
        {
            lock (Sync)
            {
                var t = _tables.FirstOrDefault(x => x.Name == name);
                if (t != null) { t.State = "done"; t.BytesCopied = tableBytes; t.Rows = rows; }
                _dataCopied = copiedTotal;
                _percent = Math.Max(_percent, 6 + 56.0 * Math.Min(1.0, (double)copiedTotal / total));
            }
            if (tableBytes > (8L << 20)) Append($"Copied {name}: {rows:N0} rows, {DatabaseRestoreService.FormatBytes(tableBytes)}.");
        }

        // Constraint/index/FK/trigger: 62% .. 96%
        public void PostProgress(string? label, int step, int totalSteps, long done, long total)
        {
            lock (Sync)
            {
                _postStep = step; _postTotal = totalSteps;
                if (label != null) { _currentTable = label; _message = $"Building constraints & indexes ({step}/{totalSteps}) on {label}…"; }
                _percent = Math.Max(_percent, 62 + 34.0 * Math.Min(1.0, (double)done / total));
            }
        }

        public void Finish(RestoreState state, string message)
        {
            lock (Sync)
            {
                State = state; FinishedAt = DateTime.UtcNow; _message = message; _currentTable = "";
                if (state == RestoreState.Succeeded) { _percent = 100; _phase = "Done"; }
                else _phase = state == RestoreState.Cancelled ? "Cancelled" : "Failed";
            }
            Append(message);
        }

        public void Fail(Exception e)
        {
            var msg = e is PostgresException pg ? $"{pg.MessageText} ({pg.SqlState}){(string.IsNullOrEmpty(pg.Detail) ? "" : " – " + pg.Detail)}" : e.Message;
            lock (Sync) _error = msg;
            Finish(RestoreState.Failed, "Failed: " + msg + " -- the destination was rolled back to its previous content.");
        }

        public RestoreJobSnapshot ToSnapshot() => new()
        {
            Id = Id, Source = Source, Destination = Destination, StartedBy = StartedBy, StartedAt = StartedAt, FinishedAt = FinishedAt,
            State = State.ToString(), Phase = _phase, Message = _message, Percent = Math.Round(_percent, 1),
            Error = string.IsNullOrEmpty(_error) ? null : _error, CurrentTable = string.IsNullOrEmpty(_currentTable) ? null : _currentTable,
            DataBytesCopied = _dataCopied, DataBytesTotal = _dataTotal,
            TablesTotal = _tables.Count(t => t.State != "skipped"), TablesDone = _tables.Count(t => t.State == "done"),
            PostStep = _postStep, PostTotal = _postTotal,
            ElapsedSeconds = ((FinishedAt ?? DateTime.UtcNow) - StartedAt).TotalSeconds,
            Log = _log.ToList(),
            Tables = _tables.Select(t => new RestoreTableState { Name = t.Name, SizeBytes = t.SizeBytes, State = t.State, BytesCopied = t.BytesCopied, Rows = t.Rows }).ToList(),
        };
    }
}
