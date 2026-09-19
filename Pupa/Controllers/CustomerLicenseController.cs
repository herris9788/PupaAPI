using System.Security.Cryptography;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pupa.BusinessObjects;
using Pupa.BusinessObjects.Beesuite;

namespace Pupa.Controllers
{
    // License keys for the NetSuite Customer app.
    //
    //   Admin: list / issue / edit / revoke / delete a key — ungated, same as
    //          CustomerCompanyController (reachable only from inside the ERP
    //          app's own Admin section, behind that app's login).
    //   Public: POST activate — what the mobile app calls with a key. On success
    //           it returns the company's configuration (name, logo, menu
    //           prefix, API base URL, ...), so entering one key is all it takes
    //           to point an installed app at a company. The app calls it again
    //           on every launch, which is also how a revoked / expired key gets
    //           noticed.
    [Route("beesuite/api/[controller]")]
    [ApiController]
    public class CustomerLicenseController : ControllerBase
    {
        private readonly BeesuiteDbContext _db;

        public CustomerLicenseController(BeesuiteDbContext db)
        {
            _db = db;
        }

        // No 0/O/1/I/L so a key read aloud or typed from a screenshot survives.
        private const string KeyAlphabet = "23456789ABCDEFGHJKMNPQRSTUVWXYZ";

        private static string NewKey()
        {
            var chars = new char[16];
            for (var i = 0; i < chars.Length; i++)
                chars[i] = KeyAlphabet[RandomNumberGenerator.GetInt32(KeyAlphabet.Length)];
            var s = new string(chars);
            return $"NSC-{s[..4]}-{s[4..8]}-{s[8..12]}-{s[12..]}";
        }

        // The columns are naive "timestamp" holding UTC (see
        // migration_CustomerLicense.sql), and Npgsql hands them back with an
        // Unspecified/Local Kind — label them UTC so they serialize with a
        // trailing "Z" instead of a bogus local offset.
        internal static DateTime? Utc(DateTime? value) =>
            value.HasValue ? DateTime.SpecifyKind(value.Value, DateTimeKind.Utc) : null;

        private static string StatusOf(CustomerLicense l, DateTime now) =>
            l.IsRevoked ? "revoked" : (l.ExpiresAt != null && l.ExpiresAt <= now) ? "expired" : "active";

        private static object ToDto(CustomerLicense l, DateTime now) => new
        {
            l.ID, l.CompanyID, l.LicenseKey, IssuedAt = Utc(l.IssuedAt), ExpiresAt = Utc(l.ExpiresAt),
            l.IsRevoked, l.Note, LastActivatedAt = Utc(l.LastActivatedAt), l.LastDeviceID, l.ActivationCount,
            Status = StatusOf(l, now),
        };

        // "none" = never expires; "years" = N years from now; "date" = exact
        // UTC instant. The same shape is used when issuing and when editing.
        public class ExpiryInput
        {
            public string? Mode { get; set; }
            public int? Years { get; set; }
            public DateTime? Date { get; set; }
        }

        private static bool TryResolveExpiry(ExpiryInput? input, out DateTime? expiresAt, out string? error)
        {
            expiresAt = null;
            error = null;
            switch ((input?.Mode ?? "none").ToLowerInvariant())
            {
                case "none":
                    return true;
                case "years":
                    if (input!.Years is null or < 1 or > 100)
                    {
                        error = "Years must be between 1 and 100.";
                        return false;
                    }
                    expiresAt = DateTime.UtcNow.AddYears(input.Years.Value);
                    return true;
                case "date":
                    if (input!.Date is null)
                    {
                        error = "Date is required.";
                        return false;
                    }
                    expiresAt = input.Date.Value.Kind == DateTimeKind.Unspecified
                        ? input.Date.Value
                        : input.Date.Value.ToUniversalTime();
                    expiresAt = DateTime.SpecifyKind(expiresAt.Value, DateTimeKind.Unspecified);
                    return true;
                default:
                    error = "Mode must be none, years or date.";
                    return false;
            }
        }

        public class CreateInput
        {
            public int CompanyID { get; set; }
            public ExpiryInput? Expiry { get; set; }
            public string? Note { get; set; }
        }

        public class UpdateInput
        {
            public bool? IsRevoked { get; set; }
            public string? Note { get; set; }
            // Absent = leave the expiry as it is.
            public ExpiryInput? Expiry { get; set; }
        }

        // GET beesuite/api/CustomerLicense?companyId=5
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int? companyId)
        {
            try
            {
                var query = _db.CustomerLicense.AsNoTracking().AsQueryable();
                if (companyId.HasValue) query = query.Where(l => l.CompanyID == companyId.Value);
                var now = DateTime.UtcNow;
                var data = (await query.OrderByDescending(l => l.IssuedAt).ToListAsync())
                    .Select(l => ToDto(l, now));
                return Ok(data);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        // POST beesuite/api/CustomerLicense
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateInput input)
        {
            try
            {
                if (!await _db.CustomerCompany.AnyAsync(c => c.ID == input.CompanyID))
                    return BadRequest(new { Error = "invalid", Message = "Company not found." });

                if (!TryResolveExpiry(input.Expiry, out var expiresAt, out var error))
                    return BadRequest(new { Error = "invalid", Message = error });

                var entity = new CustomerLicense
                {
                    CompanyID = input.CompanyID,
                    IssuedAt = DateTime.UtcNow,
                    ExpiresAt = expiresAt,
                    Note = string.IsNullOrWhiteSpace(input.Note) ? null : input.Note.Trim(),
                };

                // 32^16 possibilities — a collision is not going to happen, but
                // the unique index is the real guard, so retry rather than assume.
                for (var attempt = 0; ; attempt++)
                {
                    entity.LicenseKey = NewKey();
                    if (!await _db.CustomerLicense.AnyAsync(l => l.LicenseKey == entity.LicenseKey)) break;
                    if (attempt >= 5) return StatusCode(500, "Could not generate a unique license key.");
                }

                _db.CustomerLicense.Add(entity);
                await _db.SaveChangesAsync();
                return Ok(ToDto(entity, DateTime.UtcNow));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        // PUT beesuite/api/CustomerLicense/5
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateInput input)
        {
            try
            {
                var existing = await _db.CustomerLicense.FirstOrDefaultAsync(l => l.ID == id);
                if (existing == null) return NotFound();

                if (input.Expiry != null)
                {
                    if (!TryResolveExpiry(input.Expiry, out var expiresAt, out var error))
                        return BadRequest(new { Error = "invalid", Message = error });
                    existing.ExpiresAt = expiresAt;
                }
                if (input.IsRevoked.HasValue) existing.IsRevoked = input.IsRevoked.Value;
                if (input.Note != null) existing.Note = string.IsNullOrWhiteSpace(input.Note) ? null : input.Note.Trim();

                await _db.SaveChangesAsync();
                return Ok(ToDto(existing, DateTime.UtcNow));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        // DELETE beesuite/api/CustomerLicense/5
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var existing = await _db.CustomerLicense.FirstOrDefaultAsync(l => l.ID == id);
                if (existing == null) return NotFound();
                _db.CustomerLicense.Remove(existing);
                await _db.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        // GET beesuite/api/CustomerLicense/dashboard?code=HERRIS — public. The
        // Purchasing dashboard page (a web page inside the app's WebView, with
        // no license key of its own) asks for the layout its company chose in
        // the admin page. Only the layout is exposed — nothing about the
        // company's backend or licenses. 204 = "use the default layout".
        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboard([FromQuery] string? code)
        {
            try
            {
                var normalized = code?.Trim().ToUpperInvariant() ?? "";
                if (normalized.Length is < 2 or > 20) return NoContent();
                var config = await _db.CustomerCompany.AsNoTracking()
                    .Where(c => c.Code == normalized && c.IsActive)
                    .Select(c => c.DashboardConfig)
                    .FirstOrDefaultAsync();
                if (string.IsNullOrWhiteSpace(config)) return NoContent();
                return Content(config, "application/json");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        public class ActivateInput
        {
            public string? LicenseKey { get; set; }
            public string? DeviceID { get; set; }
            // A re-check of a license the app is already running on (done on
            // user interaction, so it can be frequent): same answer, but
            // nothing is recorded — no LastActivatedAt / device bookkeeping.
            public bool Passive { get; set; }
        }

        // POST beesuite/api/CustomerLicense/activate — public, called by the
        // mobile app. Every failure looks the same to a caller who doesn't
        // hold a real key ("invalid_license"), except for the states that only
        // a real key holder can reach (revoked / expired / company disabled).
        [HttpPost("activate")]
        public async Task<IActionResult> Activate([FromBody] ActivateInput input)
        {
            try
            {
                var key = input.LicenseKey?.Trim().ToUpperInvariant() ?? "";
                if (key.Length < 8 || key.Length > 40)
                    return NotFound(new { Error = "invalid_license", Message = "License key not found." });

                var license = await _db.CustomerLicense.FirstOrDefaultAsync(l => l.LicenseKey == key);
                if (license == null)
                    return NotFound(new { Error = "invalid_license", Message = "License key not found." });

                var company = await _db.CustomerCompany.AsNoTracking().FirstOrDefaultAsync(c => c.ID == license.CompanyID);
                if (company == null)
                    return NotFound(new { Error = "invalid_license", Message = "License key not found." });

                var now = DateTime.UtcNow;
                if (license.IsRevoked)
                    return StatusCode(403, new { Error = "revoked", Message = "This license has been revoked." });
                if (license.ExpiresAt != null && license.ExpiresAt <= now)
                    return StatusCode(403, new { Error = "expired", Message = "This license has expired.", ExpiresAt = Utc(license.ExpiresAt) });
                if (!company.IsActive)
                    return StatusCode(403, new { Error = "company_inactive", Message = "This company is disabled." });

                if (!input.Passive)
                {
                    var device = input.DeviceID?.Trim();
                    if (device?.Length > 80) device = device[..80];
                    if (!string.IsNullOrEmpty(device) && device != license.LastDeviceID)
                    {
                        license.LastDeviceID = device;
                        license.ActivationCount++;
                    }
                    license.LastActivatedAt = now;
                    await _db.SaveChangesAsync();
                }

                return Ok(new
                {
                    license.LicenseKey,
                    ExpiresAt = Utc(license.ExpiresAt),
                    Company = new
                    {
                        company.Code, company.Name, company.LogoUrl, company.MenuPrefix,
                        company.ApiBaseUrl, company.ApiDb, company.AuthBaseUrl, company.HomeConfig,
                    },
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}
