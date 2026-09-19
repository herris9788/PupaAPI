using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pupa.BusinessObjects;
using Pupa.BusinessObjects.Beesuite;

namespace Pupa.Controllers
{
    // Admin CRUD for the companies that use the NetSuite Customer app. A
    // company's row is what the app loads once a valid license is entered
    // (see CustomerLicenseController.Activate). Ungated, same as MenuController
    // and LunchController's own admin actions — reachable only from inside the
    // ERP app's own Admin section (ProfileScreen), behind that app's login.
    [Route("beesuite/api/[controller]")]
    [ApiController]
    public class CustomerCompanyController : ControllerBase
    {
        private readonly BeesuiteDbContext _db;

        public CustomerCompanyController(BeesuiteDbContext db)
        {
            _db = db;
        }

        public class CompanyInput
        {
            public string? Code { get; set; }
            public string? Name { get; set; }
            public string? LogoUrl { get; set; }
            public string? ApiBaseUrl { get; set; }
            public string? ApiDb { get; set; }
            public string? AuthBaseUrl { get; set; }
            public string? DashboardConfig { get; set; }
            public string? HomeConfig { get; set; }
            public bool? IsActive { get; set; }
        }

        // Codes are a single A-Z0-9 segment on purpose: the menu prefix is
        // "CS_<CODE>_" and menus are matched with StartsWith, so two codes can
        // never produce prefixes where one is a prefix of the other (which
        // would leak one company's menus into another's list).
        private static readonly Regex CodePattern = new("^[A-Z0-9]{2,20}$", RegexOptions.Compiled);
        private static readonly Regex DataUriLogo = new(@"^data:image/(png|jpeg|webp);base64,[A-Za-z0-9+/=]+$", RegexOptions.Compiled);
        private static readonly Regex UrlLogo = new(@"^https?://\S+$", RegexOptions.Compiled);
        private const int MaxLogoLength = 400_000;

        internal static string PrefixFor(string code) => $"CS_{code}_";

        // The dashboard is a list of widgets the mobile page knows how to draw.
        // Unknown widget types are rejected here rather than silently ignored,
        // so a typo in the admin page can't ship a blank dashboard.
        private static readonly HashSet<string> WidgetTypes = new()
        {
            "kpis", "quickActions", "pipeline", "spendChart", "actionRequired",
            "upcomingDeliveries", "recentActivity", "topProducts", "invoiceAging", "ordersByStatus",
        };
        private const int MaxDashboardLength = 20_000;

        // Returns an error message, or null when the JSON is an acceptable layout.
        internal static string? ValidateDashboard(string? json)
        {
            if (string.IsNullOrWhiteSpace(json)) return null;
            if (json.Length > MaxDashboardLength) return "DashboardConfig is too large.";
            try
            {
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;
                if (root.ValueKind != JsonValueKind.Object ||
                    !root.TryGetProperty("widgets", out var widgets) ||
                    widgets.ValueKind != JsonValueKind.Array)
                    return "DashboardConfig must be an object with a widgets array.";
                if (widgets.GetArrayLength() > 30) return "DashboardConfig has too many widgets.";
                foreach (var w in widgets.EnumerateArray())
                {
                    if (w.ValueKind != JsonValueKind.Object ||
                        !w.TryGetProperty("type", out var type) ||
                        type.ValueKind != JsonValueKind.String ||
                        !WidgetTypes.Contains(type.GetString()!))
                        return "DashboardConfig contains an unknown widget type.";
                }
                return null;
            }
            catch (JsonException)
            {
                return "DashboardConfig is not valid JSON.";
            }
        }

        private static readonly HashSet<string> HomeTints = new() { "primary", "accent", "amber", "pink", "success", "danger" };
        private const int MaxHomeConfigLength = 8_000;
        private const int MaxHomeStats = 6;
        private const int MaxHomeActivity = 12;
        private const int MaxHomeFieldLength = 60;

        // The native Home screen's own content: its progress-card row and
        // recent-activity list. Kept deliberately simple (plain strings the
        // admin types in, no live data) since the native app has no access to
        // a company's Purchasing data — that only exists inside the web
        // /purchasing page's own browser storage.
        internal static string? ValidateHomeConfig(string? json)
        {
            if (string.IsNullOrWhiteSpace(json)) return null;
            if (json.Length > MaxHomeConfigLength) return "HomeConfig is too large.";
            try
            {
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;
                if (root.ValueKind != JsonValueKind.Object) return "HomeConfig must be an object.";

                if (root.TryGetProperty("stats", out var stats))
                {
                    if (stats.ValueKind != JsonValueKind.Array) return "HomeConfig.stats must be an array.";
                    if (stats.GetArrayLength() > MaxHomeStats) return $"HomeConfig.stats has more than {MaxHomeStats} cards.";
                    foreach (var s in stats.EnumerateArray())
                    {
                        if (s.ValueKind != JsonValueKind.Object) return "Each stat must be an object.";
                        if (!HasShortString(s, "label") || !HasShortString(s, "value") || !HasShortString(s, "icon"))
                            return "Each stat needs label, value and icon.";
                        if (!s.TryGetProperty("tint", out var tint) || tint.ValueKind != JsonValueKind.String ||
                            !HomeTints.Contains(tint.GetString() ?? ""))
                            return "Each stat's tint must be one of: " + string.Join(", ", HomeTints);
                    }
                }

                if (root.TryGetProperty("activity", out var activity))
                {
                    if (activity.ValueKind != JsonValueKind.Array) return "HomeConfig.activity must be an array.";
                    if (activity.GetArrayLength() > MaxHomeActivity) return $"HomeConfig.activity has more than {MaxHomeActivity} rows.";
                    foreach (var a in activity.EnumerateArray())
                    {
                        if (a.ValueKind != JsonValueKind.Object) return "Each activity row must be an object.";
                        if (!HasShortString(a, "title") || !HasShortString(a, "time") || !HasShortString(a, "amount"))
                            return "Each activity row needs title, time and amount.";
                    }
                }
                return null;
            }
            catch (JsonException)
            {
                return "HomeConfig is not valid JSON.";
            }

            static bool HasShortString(JsonElement obj, string prop) =>
                obj.TryGetProperty(prop, out var v) && v.ValueKind == JsonValueKind.String &&
                v.GetString() is { Length: > 0 and <= MaxHomeFieldLength };
        }

        // Timestamps are naive UTC columns (see migration_CustomerLicense.sql);
        // label them UTC so they serialize with a trailing "Z".
        private static object ToDto(CustomerCompany c) => new
        {
            c.ID, c.Code, c.Name, c.LogoUrl, c.MenuPrefix, c.ApiBaseUrl, c.ApiDb, c.AuthBaseUrl,
            c.DashboardConfig, c.HomeConfig, c.IsActive,
            CreatedAt = CustomerLicenseController.Utc(c.CreatedAt),
            UpdatedAt = CustomerLicenseController.Utc(c.UpdatedAt),
        };

        private static string? NormalizeUrl(string? value, out bool valid)
        {
            valid = true;
            var v = value?.Trim();
            if (string.IsNullOrEmpty(v)) return null;
            if (!Uri.TryCreate(v, UriKind.Absolute, out var uri) ||
                (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
            {
                valid = false;
                return null;
            }
            return v.TrimEnd('/');
        }

        private static bool IsValidLogo(string? logo) =>
            string.IsNullOrEmpty(logo) ||
            (logo.Length <= MaxLogoLength && (DataUriLogo.IsMatch(logo) || UrlLogo.IsMatch(logo)));

        // Applies + validates input; returns an error message or null.
        private static string? Apply(CustomerCompany target, CompanyInput input, bool creating)
        {
            var name = input.Name?.Trim();
            if (string.IsNullOrEmpty(name)) return "Name is required.";

            var apiBase = NormalizeUrl(input.ApiBaseUrl, out var apiOk);
            if (!apiOk || apiBase == null) return "ApiBaseUrl must be a valid http(s) URL.";

            var authBase = NormalizeUrl(input.AuthBaseUrl, out var authOk);
            if (!authOk) return "AuthBaseUrl must be a valid http(s) URL.";

            var dashboardError = ValidateDashboard(input.DashboardConfig);
            if (dashboardError != null) return dashboardError;

            var homeConfigError = ValidateHomeConfig(input.HomeConfig);
            if (homeConfigError != null) return homeConfigError;

            var logo = string.IsNullOrWhiteSpace(input.LogoUrl) ? null : input.LogoUrl.Trim();
            if (!IsValidLogo(logo)) return "LogoUrl must be an https URL or a PNG/JPEG/WebP data URI (max ~300 KB).";

            if (creating)
            {
                var code = input.Code?.Trim().ToUpperInvariant() ?? "";
                if (!CodePattern.IsMatch(code)) return "Code must be 2-20 letters/digits (A-Z, 0-9).";
                target.Code = code;
                target.MenuPrefix = PrefixFor(code);
            }

            target.Name = name;
            target.LogoUrl = logo;
            target.ApiBaseUrl = apiBase;
            target.ApiDb = string.IsNullOrWhiteSpace(input.ApiDb) ? null : input.ApiDb.Trim();
            target.AuthBaseUrl = authBase;
            target.DashboardConfig = string.IsNullOrWhiteSpace(input.DashboardConfig) ? null : input.DashboardConfig.Trim();
            target.HomeConfig = string.IsNullOrWhiteSpace(input.HomeConfig) ? null : input.HomeConfig.Trim();
            if (input.IsActive.HasValue) target.IsActive = input.IsActive.Value;
            target.UpdatedAt = DateTime.UtcNow;
            return null;
        }

        // GET beesuite/api/CustomerCompany
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var now = DateTime.UtcNow;
                var companies = await _db.CustomerCompany.AsNoTracking().OrderBy(c => c.Name).ToListAsync();
                var licenses = await _db.CustomerLicense.AsNoTracking()
                    .Select(l => new { l.CompanyID, l.IsRevoked, l.ExpiresAt })
                    .ToListAsync();

                var data = companies.Select(c =>
                {
                    var mine = licenses.Where(l => l.CompanyID == c.ID).ToList();
                    return new
                    {
                        c.ID, c.Code, c.Name, c.LogoUrl, c.MenuPrefix, c.ApiBaseUrl, c.ApiDb,
                        c.AuthBaseUrl, c.DashboardConfig, c.HomeConfig, c.IsActive,
                        CreatedAt = CustomerLicenseController.Utc(c.CreatedAt),
                        UpdatedAt = CustomerLicenseController.Utc(c.UpdatedAt),
                        LicenseCount = mine.Count,
                        ActiveLicenseCount = mine.Count(l => !l.IsRevoked && (l.ExpiresAt == null || l.ExpiresAt > now)),
                    };
                });
                return Ok(data);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        // GET beesuite/api/CustomerCompany/5
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var item = await _db.CustomerCompany.AsNoTracking().FirstOrDefaultAsync(c => c.ID == id);
                return item == null ? NotFound() : Ok(ToDto(item));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        // POST beesuite/api/CustomerCompany
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CompanyInput input)
        {
            try
            {
                var entity = new CustomerCompany { CreatedAt = DateTime.UtcNow };
                var error = Apply(entity, input, creating: true);
                if (error != null) return BadRequest(new { Error = "invalid", Message = error });

                if (await _db.CustomerCompany.AnyAsync(c => c.Code == entity.Code))
                    return Conflict(new { Error = "duplicate", Message = $"Company code {entity.Code} already exists." });

                _db.CustomerCompany.Add(entity);
                await _db.SaveChangesAsync();
                return CreatedAtAction(nameof(GetById), new { id = entity.ID }, ToDto(entity));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        // PUT beesuite/api/CustomerCompany/5 — Code (and so MenuPrefix) is
        // immutable: changing it would orphan every menu row already created
        // under the old prefix.
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] CompanyInput input)
        {
            try
            {
                var existing = await _db.CustomerCompany.FirstOrDefaultAsync(c => c.ID == id);
                if (existing == null) return NotFound();

                var error = Apply(existing, input, creating: false);
                if (error != null) return BadRequest(new { Error = "invalid", Message = error });

                await _db.SaveChangesAsync();
                return Ok(ToDto(existing));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        // DELETE beesuite/api/CustomerCompany/5 — also removes its licenses
        // (ON DELETE CASCADE). Its Menu rows are left alone.
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var existing = await _db.CustomerCompany.FirstOrDefaultAsync(c => c.ID == id);
                if (existing == null) return NotFound();
                _db.CustomerCompany.Remove(existing);
                await _db.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}
