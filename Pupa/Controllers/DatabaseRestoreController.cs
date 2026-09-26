using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using Pupa.Services;

namespace Pupa.Controllers
{
    /// <summary>
    /// Restore database PostgreSQL: salin isi database SOURCE ke database DESTINATION (replace penuh) dengan progress.
    /// Hanya SUPERUSER / SYSADMIN. Database produksi tidak pernah boleh jadi destination (dicek di service).
    ///
    /// Endpoint ini SENGAJA tidak memakai BeesuiteDbContext: DbContext dipilih lewat header X-API-DB dan bisa menunjuk ke
    /// database yang sedang di-restore (terkunci). Cek role & semua operasi memakai koneksi eksplisit ke database produksi.
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("beesuite/api/DatabaseRestore")]
    public class DatabaseRestoreController : ControllerBase
    {
        private static readonly HashSet<string> AllowedRoles = new(StringComparer.OrdinalIgnoreCase) { "SUPERUSER", "SYSADMIN" };

        private readonly DatabaseRestoreService _service;
        private readonly IConfiguration _config;

        public DatabaseRestoreController(DatabaseRestoreService service, IConfiguration config)
        {
            _service = service;
            _config = config;
        }

        // Username dari token: Identity.Name (plaintext) atau klaim "sub" yang di-AES-encrypt (token Ascend v1/v3/v4).
        private string? ResolveUsername()
        {
            var name = User.Identity?.IsAuthenticated == true ? User.Identity.Name : null;
            if (!string.IsNullOrEmpty(name)) return name;
            var sub = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                      ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                      ?? User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(sub)) return null;
            try { return Pupa.Helpers.SecurityHelper.Decrypt(sub); } catch { return sub; }
        }

        private async Task<(bool ok, string? username)> IsAdminAsync(CancellationToken ct)
        {
            var username = ResolveUsername();
            if (string.IsNullOrEmpty(username)) return (false, null);

            var roleClaim = User.FindFirst(ClaimTypes.Role)?.Value;
            if (!string.IsNullOrEmpty(roleClaim) && AllowedRoles.Contains(roleClaim)) return (true, username);

            var cs = _config.GetConnectionString("Beesuite");
            await using var c = new NpgsqlConnection(cs);
            await c.OpenAsync(ct);
            foreach (var table in new[] { "User", "UserV3" })
            {
                await using var cmd = new NpgsqlCommand($"select \"Role\" from public.\"{table}\" where lower(\"Username\") = lower(@u) limit 1", c);
                cmd.Parameters.AddWithValue("u", username);
                var role = (await cmd.ExecuteScalarAsync(ct)) as string;
                if (!string.IsNullOrEmpty(role) && AllowedRoles.Contains(role)) return (true, username);
            }
            return (false, username);
        }

        private async Task<IActionResult?> Guard(CancellationToken ct)
        {
            var (ok, _) = await IsAdminAsync(ct);
            return ok ? null : StatusCode(StatusCodes.Status403Forbidden, new { error = "Only SUPERUSER or SYSADMIN can restore databases." });
        }

        // GET beesuite/api/DatabaseRestore/Databases
        [HttpGet("Databases")]
        public async Task<IActionResult> Databases(CancellationToken ct)
        {
            if (await Guard(ct) is { } denied) return denied;
            try { return Ok(await _service.ListDatabasesAsync(ct)); }
            catch (Exception e) { return StatusCode(500, new { error = e.Message }); }
        }

        // GET beesuite/api/DatabaseRestore/Tables?database=beesuite
        [HttpGet("Tables")]
        public async Task<IActionResult> Tables([FromQuery] string database, CancellationToken ct)
        {
            if (await Guard(ct) is { } denied) return denied;
            try
            {
                var known = await _service.ListDatabasesAsync(ct);
                if (!known.Any(d => d.Name.Equals(database, StringComparison.OrdinalIgnoreCase))) return BadRequest(new { error = "Unknown database." });
                return Ok(await _service.ListTablesAsync(known.First(d => d.Name.Equals(database, StringComparison.OrdinalIgnoreCase)).Name, ct));
            }
            catch (Exception e) { return StatusCode(500, new { error = e.Message }); }
        }

        // POST beesuite/api/DatabaseRestore/Start   { Source, Destination, Confirm, ExcludeDataTables[] }
        [HttpPost("Start")]
        public async Task<IActionResult> Start([FromBody] RestoreRequest req, CancellationToken ct)
        {
            var (ok, username) = await IsAdminAsync(ct);
            if (!ok) return StatusCode(StatusCodes.Status403Forbidden, new { error = "Only SUPERUSER or SYSADMIN can restore databases." });
            try { return Ok(await _service.StartAsync(req, username ?? "unknown", ct)); }
            catch (InvalidOperationException e) { return BadRequest(new { error = e.Message }); }
            catch (Exception e) { return StatusCode(500, new { error = e.Message }); }
        }

        // GET beesuite/api/DatabaseRestore/Status[?jobId=...]   (tanpa jobId = job terakhir)
        [HttpGet("Status")]
        public async Task<IActionResult> Status([FromQuery] string? jobId, CancellationToken ct)
        {
            if (await Guard(ct) is { } denied) return denied;
            var snap = _service.Snapshot(jobId);
            return snap == null ? NoContent() : Ok(snap);
        }

        // POST beesuite/api/DatabaseRestore/Cancel[?jobId=...]
        [HttpPost("Cancel")]
        public async Task<IActionResult> Cancel([FromQuery] string? jobId, CancellationToken ct)
        {
            if (await Guard(ct) is { } denied) return denied;
            return _service.Cancel(jobId) ? Ok(new { cancelled = true }) : BadRequest(new { error = "No running restore to cancel." });
        }
    }
}
