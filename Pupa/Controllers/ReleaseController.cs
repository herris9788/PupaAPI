using Pupa.BusinessObjects;
using Pupa.BusinessObjects.Beesuite;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Pupa.Controllers
{
    /// <summary>
    /// Desktop release / update info endpoint consumed by the Flutter desktop app.
    /// Mirrors https://api2.waruna.id/api/v3/release/desktop-release.
    /// Flutter base URL = API_HOST (https://api2.waruna.id) + API_VERSION (/api/v3),
    /// so the route here is "api/v3/release/desktop-release".
    /// </summary>
    [ApiController]
    [Authorize]
    // Served at both the Flutter/script contract path (/api/v3/release/...) and
    // the project's beesuite/api convention (/beesuite/api/Release/...).
    [Route("beesuite/api/[controller]")]
    public class ReleaseController : ControllerBase
    {
        private const string ReleaseNamespace = "release";
        private const string DesktopReleaseKey = "desktop-release";

        private readonly BeesuiteDbContext _db;
        private readonly IConfiguration _config;
        private readonly IWebHostEnvironment _env;

        public ReleaseController(BeesuiteDbContext db, IConfiguration config, IWebHostEnvironment env)
        {
            _db = db;
            _config = config;
            _env = env;
        }

        // POST /api/v3/release/desktop-release
        // Optional JSON body: { "currentVersion": "1.2.3", "platform": "windows" }
        // currentVersion may also be supplied via query string as a fallback.
        // Dipilih untuk request JSON (cek update). Upload file ditangani
        // DesktopPublish di route yang sama, dibedakan via Content-Type.
        [HttpPost("desktop-release")]
        [Consumes("application/json")]
        public async Task<ActionResult<AppUpdateInfoDto>> DesktopRelease(
            [FromBody] DesktopReleaseRequest? request = null,
            [FromQuery] string? currentVersion = null)
        {
            currentVersion = !string.IsNullOrWhiteSpace(request?.CurrentVersion)
                ? request!.CurrentVersion
                : currentVersion;

            var config = await _db.AppConfig
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Namespace == ReleaseNamespace && c.Key == DesktopReleaseKey);

            // Nothing configured yet -> report "no update" rather than failing the client.
            if (config == null || string.IsNullOrWhiteSpace(config.ValJson))
            {
                return Ok(AppUpdateInfoDto.None());
            }

            StoredRelease? release;
            try
            {
                release = JsonSerializer.Deserialize<StoredRelease>(config.ValJson, JsonOpts);
            }
            catch (JsonException)
            {
                return Ok(AppUpdateInfoDto.None());
            }

            if (release == null || string.IsNullOrWhiteSpace(release.LatestVersion))
            {
                return Ok(AppUpdateInfoDto.None());
            }

            // Decide hasUpdate / forceUpdate. If the client tells us its current version
            // we compare; otherwise we surface the stored release as-is.
            bool hasUpdate;
            bool forceUpdate = release.ForceUpdate;

            if (!string.IsNullOrWhiteSpace(currentVersion))
            {
                hasUpdate = CompareVersions(currentVersion, release.LatestVersion) < 0;

                // Force when current is below the configured minimum supported version.
                if (!string.IsNullOrWhiteSpace(release.MinVersion) &&
                    CompareVersions(currentVersion, release.MinVersion) < 0)
                {
                    forceUpdate = true;
                }

                // No update available -> never force.
                if (!hasUpdate)
                {
                    forceUpdate = false;
                }
            }
            else
            {
                hasUpdate = release.HasUpdate ?? true;
            }

            return Ok(new AppUpdateInfoDto
            {
                HasUpdate = hasUpdate,
                ForceUpdate = forceUpdate,
                LatestVersion = release.LatestVersion ?? string.Empty,
                Title = string.IsNullOrWhiteSpace(release.Title) ? "Update Available" : release.Title!,
                Message = string.IsNullOrWhiteSpace(release.Message)
                    ? "A new version of BeeSuite is available. Please update the application."
                    : release.Message!,
                UpdateUrl = release.UpdateUrl ?? string.Empty
            });
        }

        // Upload build baru (multipart/form-data). Dilayani DI ROUTE YANG SAMA
        // dengan cek update (desktop-release) supaya cukup satu route di API
        // gateway — request dibedakan oleh Content-Type (multipart vs json).
        // Tetap tersedia juga di /desktop-publish bila gateway memetakannya.
        // Fields (matches tool/release_windows.ps1):
        //   file        : the build .zip
        //   version     : new version, e.g. 1.2.0
        //   platform    : "windows" (optional, default windows)
        //   forceUpdate : "true"/"false" (optional)
        //   title, message, minVersion : optional overrides stored in AppConfig
        // Stores the zip under wwwroot/<Release:PublicPath> (default "releases")
        // and upserts the AppConfig row so desktop-release returns the new updateUrl.
        [HttpPost("desktop-release")]
        [HttpPost("desktop-publish")]
        [Consumes("multipart/form-data")]
        [RequestSizeLimit(2_147_483_648)]      // 2 GB
        [RequestFormLimits(MultipartBodyLengthLimit = 2_147_483_648)]
        public async Task<IActionResult> DesktopPublish(
            IFormFile file,
            [FromForm] string? version,
            [FromForm] string? platform = "windows",
            [FromForm] bool forceUpdate = false,
            [FromForm] string? title = null,
            [FromForm] string? message = null,
            [FromForm] string? minVersion = null)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new { ok = false, message = "No file uploaded." });
            }
            if (string.IsNullOrWhiteSpace(version))
            {
                return BadRequest(new { ok = false, message = "version is required." });
            }
            platform = string.IsNullOrWhiteSpace(platform) ? "windows" : platform.Trim();

            // Deterministic, path-safe file name.
            var safeVersion = string.Concat(version.Trim().Where(ch => char.IsLetterOrDigit(ch) || ch is '.' or '-' or '_'));
            var fileName = $"beesuite-{platform}-{safeVersion}.zip";

            // URL path segment tempat file disajikan. HARUS sama dengan subfolder
            // fisik di bawah wwwroot, dan harus diroute lewat API gateway ke host
            // ini (tanpa auth) supaya app bisa mengunduhnya. Override via config
            // "Release:PublicPath" bila perlu (default: "releases").
            var publicPath = _config["Release:PublicPath"];
            if (string.IsNullOrWhiteSpace(publicPath)) publicPath = "releases";
            publicPath = publicPath.Trim('/');

            // Folder fisik penyimpanan. Default: wwwroot/<publicPath> supaya cocok
            // dengan URL publik di bawah (UseStaticFiles menyajikan wwwroot).
            var storageRoot = _config["Release:StoragePath"];
            if (string.IsNullOrWhiteSpace(storageRoot))
            {
                var webRoot = string.IsNullOrWhiteSpace(_env.WebRootPath)
                    ? Path.Combine(_env.ContentRootPath, "wwwroot")
                    : _env.WebRootPath;
                storageRoot = Path.Combine(webRoot, publicPath);
            }
            Directory.CreateDirectory(storageRoot);

            var fullPath = Path.Combine(storageRoot, fileName);
            await using (var fs = new FileStream(fullPath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                await file.CopyToAsync(fs);
            }

            // Public base URL: utamakan config (di belakang gateway, Request.Host
            // bisa jadi host internal yang salah). Set "Release:PublicBaseUrl"
            // = https://api2.waruna.id.
            var baseUrl = _config["Release:PublicBaseUrl"];
            if (string.IsNullOrWhiteSpace(baseUrl))
            {
                baseUrl = $"{Request.Scheme}://{Request.Host}";
            }
            // updateUrl HARUS bisa di-GET tanpa token (app mengunduh tanpa auth)
            // dan path-nya cocok dengan lokasi file di atas.
            var updateUrl = $"{baseUrl.TrimEnd('/')}/{publicPath}/{Uri.EscapeDataString(fileName)}";

            var stored = new StoredRelease
            {
                LatestVersion = version.Trim(),
                MinVersion = string.IsNullOrWhiteSpace(minVersion) ? null : minVersion.Trim(),
                ForceUpdate = forceUpdate,
                Title = string.IsNullOrWhiteSpace(title) ? null : title,
                Message = string.IsNullOrWhiteSpace(message) ? null : message,
                UpdateUrl = updateUrl
            };
            var json = JsonSerializer.Serialize(stored, JsonOpts);

            var cfg = await _db.AppConfig
                .FirstOrDefaultAsync(c => c.Namespace == ReleaseNamespace && c.Key == DesktopReleaseKey);
            var now = DateTimeOffset.UtcNow;
            if (cfg == null)
            {
                cfg = new AppConfig
                {
                    Namespace = ReleaseNamespace,
                    Key = DesktopReleaseKey,
                    DataType = "json",
                    ValJson = json,
                    Description = "Desktop release info (managed by desktop-publish).",
                    IsSecret = false,
                    IsReadonly = false,
                    CreatedAt = now,
                    UpdatedAt = now,
                    CreatedBy = User?.Identity?.Name
                };
                _db.AppConfig.Add(cfg);
            }
            else
            {
                cfg.DataType = "json";
                cfg.ValJson = json;
                cfg.UpdatedAt = now;
            }
            await _db.SaveChangesAsync();

            return Ok(new
            {
                ok = true,
                version = version.Trim(),
                platform,
                forceUpdate,
                updateUrl,
                fileName,
                sizeBytes = file.Length
            });
        }

        private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web)
        {
            PropertyNameCaseInsensitive = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true
        };

        /// <summary>
        /// Compares two dotted version strings (e.g. "1.4.2"). Returns &lt;0, 0 or &gt;0.
        /// Falls back to ordinal string comparison when the strings are not parseable.
        /// </summary>
        private static int CompareVersions(string a, string b)
        {
            if (Version.TryParse(Normalize(a), out var va) && Version.TryParse(Normalize(b), out var vb))
            {
                return va.CompareTo(vb);
            }
            return string.CompareOrdinal(a?.Trim(), b?.Trim());
        }

        // System.Version needs at least major.minor; pad "1" -> "1.0".
        private static string Normalize(string v)
        {
            v = (v ?? string.Empty).Trim();
            return v.Contains('.') ? v : v + ".0";
        }
    }

    /// <summary>
    /// Optional request body for the desktop-release POST. All fields optional so
    /// an empty body is accepted.
    /// </summary>
    public class DesktopReleaseRequest
    {
        [JsonPropertyName("currentVersion")]
        public string? CurrentVersion { get; set; }

        [JsonPropertyName("platform")]
        public string? Platform { get; set; }
    }

    /// <summary>
    /// Response contract — matches the Flutter AppUpdateInfo model
    /// (lib/apis/app_update_api.dart). Property names are pinned to camelCase
    /// so they are emitted regardless of the global PascalCase JSON policy.
    /// </summary>
    public class AppUpdateInfoDto
    {
        [JsonPropertyName("hasUpdate")]
        public bool HasUpdate { get; set; }

        [JsonPropertyName("forceUpdate")]
        public bool ForceUpdate { get; set; }

        [JsonPropertyName("latestVersion")]
        public string LatestVersion { get; set; } = string.Empty;

        [JsonPropertyName("title")]
        public string Title { get; set; } = "Update Available";

        [JsonPropertyName("message")]
        public string Message { get; set; } = string.Empty;

        [JsonPropertyName("updateUrl")]
        public string UpdateUrl { get; set; } = string.Empty;

        public static AppUpdateInfoDto None() => new()
        {
            HasUpdate = false,
            ForceUpdate = false,
            LatestVersion = string.Empty,
            Title = "Update Available",
            Message = string.Empty,
            UpdateUrl = string.Empty
        };
    }

    /// <summary>
    /// Shape stored in AppConfig.ValJson (Namespace='release', Key='desktop-release').
    /// Example:
    /// {
    ///   "latestVersion": "1.4.2",
    ///   "minVersion": "1.4.0",
    ///   "forceUpdate": false,
    ///   "title": "Update Available",
    ///   "message": "A new version of BeeSuite is available.",
    ///   "updateUrl": "https://.../BeeSuiteSetup-1.4.2.exe"
    /// }
    /// </summary>
    public class StoredRelease
    {
        public string? LatestVersion { get; set; }
        public string? MinVersion { get; set; }
        public bool ForceUpdate { get; set; }
        public bool? HasUpdate { get; set; }
        public string? Title { get; set; }
        public string? Message { get; set; }
        public string? UpdateUrl { get; set; }
    }
}
