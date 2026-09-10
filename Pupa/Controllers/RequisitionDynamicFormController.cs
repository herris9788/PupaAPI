using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pupa.BusinessObjects;
using Pupa.BusinessObjects.Beesuite;
using Pupa.Services;

namespace Pupa.Controllers
{
    /// <summary>
    /// Endpoint form dinamis Requisition (Template -> Config -> Data).
    /// Referensi desain: Pupa/docs/dynamic-requisition-wizard-recommended.md
    ///
    /// ADITIF & TERISOLASI: controller baru, tidak mengubah endpoint Requisition
    /// yang sudah ada. Butuh tabel dari migration_RequisitionDynamicForm.sql.
    /// </summary>
    [Route("beesuite/api/[controller]")]
    public class RequisitionDynamicFormController : Controller
    {
        private readonly BeesuiteDbContext _db;
        private readonly RequisitionDynamicFormService _svc;

        public RequisitionDynamicFormController(BeesuiteDbContext db, RequisitionDynamicFormService svc)
        {
            _db = db;
            _svc = svc;
        }

        private string? Actor(string? fromBody) =>
            !string.IsNullOrWhiteSpace(User?.Identity?.Name) ? User!.Identity!.Name
            : (string.IsNullOrWhiteSpace(fromBody) ? null : fromBody);

        // ─────────────────────────── TEMPLATE ───────────────────────────────

        [HttpGet("template")]
        public async Task<IActionResult> ListTemplates([FromQuery] string? code, [FromQuery] bool activeOnly = true)
        {
            var q = _db.RequisitionFormTemplate.AsNoTracking().AsQueryable();
            if (!string.IsNullOrWhiteSpace(code)) q = q.Where(t => t.Code == code);
            if (activeOnly) q = q.Where(t => t.IsActive);
            return Ok(await q.OrderBy(t => t.Code).ThenByDescending(t => t.Version).ToListAsync());
        }

        [HttpGet("template/{id:int}")]
        public async Task<IActionResult> GetTemplate(int id)
        {
            var t = await _db.RequisitionFormTemplate.AsNoTracking().FirstOrDefaultAsync(x => x.ID == id);
            return t == null ? NotFound() : Ok(t);
        }

        /// <summary>Berapa config (& ItemCode mana) yang masih menunjuk ke template
        /// ini — dipakai UI untuk cek sebelum Delete / Unpublish.</summary>
        [HttpGet("template/{id:int}/usage")]
        public async Task<IActionResult> GetTemplateUsage(int id)
        {
            if (!await _db.RequisitionFormTemplate.AnyAsync(x => x.ID == id)) return NotFound();
            var configs = await _db.RequisitionFormConfig.AsNoTracking()
                .Where(c => c.TemplateID == id)
                .Select(c => new { c.ID, c.ItemCode, c.EntityType, c.IsActive })
                .ToListAsync();
            return Ok(new
            {
                templateId = id,
                configCount = configs.Count,
                itemCodes = configs.Select(c => c.ItemCode).Distinct().ToList(),
                configs,
            });
        }

        [HttpPost("template")]
        public async Task<IActionResult> CreateTemplate([FromBody] RequisitionFormTemplate body)
        {
            if (body == null || string.IsNullOrWhiteSpace(body.Code) || string.IsNullOrWhiteSpace(body.Name))
                return BadRequest("Code & Name wajib.");
            if (!IsValidJson(body.SchemaJson)) return BadRequest("SchemaJson bukan JSON valid.");

            body.ID = 0;
            if (body.Version <= 0) body.Version = 1;
            body.CreatedAt = body.UpdatedAt = DateTime.Now;
            _db.RequisitionFormTemplate.Add(body);
            await _db.SaveChangesAsync();
            return CreatedAtAction(nameof(GetTemplate), new { id = body.ID }, body);
        }

        [HttpPut("template/{id:int}")]
        public async Task<IActionResult> UpdateTemplate(int id, [FromBody] RequisitionFormTemplate body)
        {
            var t = await _db.RequisitionFormTemplate.FirstOrDefaultAsync(x => x.ID == id);
            if (t == null) return NotFound();
            if (t.IsPublished)
                return Conflict("Template sudah published (immutable). Pakai POST template/{id}/new-version.");
            if (body.SchemaJson != null && !IsValidJson(body.SchemaJson))
                return BadRequest("SchemaJson bukan JSON valid.");

            t.Name = body.Name ?? t.Name;
            if (body.SchemaJson != null) t.SchemaJson = body.SchemaJson;
            t.IsActive = body.IsActive;
            t.UpdatedAt = DateTime.Now;
            await _db.SaveChangesAsync();
            return Ok(t);
        }

        [HttpPost("template/{id:int}/publish")]
        public async Task<IActionResult> PublishTemplate(int id)
        {
            var t = await _db.RequisitionFormTemplate.FirstOrDefaultAsync(x => x.ID == id);
            if (t == null) return NotFound();
            t.IsPublished = true;
            t.UpdatedAt = DateTime.Now;
            await _db.SaveChangesAsync();
            return Ok(t);
        }

        /// <summary>
        /// Kembalikan template published jadi DRAFT (bisa diedit lagi via PUT).
        /// Efek samping: item yang dipin ke versi ini kehilangan wizard-nya,
        /// KECUALI ada versi published+aktif lain dengan Code sama — resolusi
        /// effective-schema akan fallback ke sana (lihat ResolveEffectiveTemplateAsync).
        /// Alternatif non-destruktif: PUT template/{id} dengan IsActive=false
        /// (hanya untuk draft) atau nonaktifkan lewat endpoint ini + PUT.
        /// </summary>
        [HttpPost("template/{id:int}/unpublish")]
        public async Task<IActionResult> UnpublishTemplate(int id)
        {
            var t = await _db.RequisitionFormTemplate.FirstOrDefaultAsync(x => x.ID == id);
            if (t == null) return NotFound();
            if (!t.IsPublished) return Ok(t); // idempoten
            t.IsPublished = false;
            t.UpdatedAt = DateTime.Now;
            await _db.SaveChangesAsync();
            return Ok(t);
        }

        /// <summary>Set IsActive tanpa menyentuh IsPublished / SchemaJson — boleh
        /// dipakai pada template published untuk "matikan wizard" tanpa mengubah
        /// statusnya jadi draft. Body: { "isActive": true|false }.</summary>
        [HttpPost("template/{id:int}/active")]
        public async Task<IActionResult> SetTemplateActive(int id, [FromBody] JsonElement body)
        {
            var t = await _db.RequisitionFormTemplate.FirstOrDefaultAsync(x => x.ID == id);
            if (t == null) return NotFound();
            if (body.ValueKind != JsonValueKind.Object || !body.TryGetProperty("isActive", out var v)
                || (v.ValueKind != JsonValueKind.True && v.ValueKind != JsonValueKind.False))
                return BadRequest("Body harus { \"isActive\": true|false }.");
            t.IsActive = v.GetBoolean();
            t.UpdatedAt = DateTime.Now;
            await _db.SaveChangesAsync();
            return Ok(t);
        }

        [HttpPost("template/{id:int}/new-version")]
        public async Task<IActionResult> NewVersion(int id, [FromBody] JsonElement? patch)
        {
            var src = await _db.RequisitionFormTemplate.AsNoTracking().FirstOrDefaultAsync(x => x.ID == id);
            if (src == null) return NotFound();

            var nextVer = 1 + await _db.RequisitionFormTemplate
                .Where(x => x.Code == src.Code).MaxAsync(x => (int?)x.Version) ?? 1;

            var schema = src.SchemaJson;
            if (patch is JsonElement p && p.ValueKind == JsonValueKind.Object &&
                p.TryGetProperty("schemaJson", out var sj))
            {
                schema = sj.ValueKind == JsonValueKind.String ? sj.GetString()! : sj.GetRawText();
                if (!IsValidJson(schema)) return BadRequest("schemaJson bukan JSON valid.");
            }

            var copy = new RequisitionFormTemplate
            {
                Code = src.Code,
                Name = src.Name,
                Version = nextVer,
                SchemaJson = schema,
                IsActive = true,
                IsPublished = false,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
            };
            _db.RequisitionFormTemplate.Add(copy);
            await _db.SaveChangesAsync();
            return CreatedAtAction(nameof(GetTemplate), new { id = copy.ID }, copy);
        }

        /// <summary>
        /// Hard delete template DRAFT. Default: ditolak (409) kalau masih dipakai
        /// config. `deleteConfigs=true` -> hapus juga semua config yang menunjuk
        /// ke template ini dalam satu transaksi (config Mode=Template tanpa
        /// template tidak valid — CHECK ck_reqformcfg_mode melarang TemplateID
        /// NULL — jadi config-nya ikut dihapus, bukan di-null-kan).
        /// </summary>
        [HttpDelete("template/{id:int}")]
        public async Task<IActionResult> DeleteTemplate(int id, [FromQuery] bool deleteConfigs = false)
        {
            var t = await _db.RequisitionFormTemplate.FirstOrDefaultAsync(x => x.ID == id);
            if (t == null) return NotFound();
            if (t.IsPublished)
                return Conflict("Template sudah published — tidak bisa dihapus. Set IsActive=false atau buat versi baru.");

            var configs = await _db.RequisitionFormConfig.Where(c => c.TemplateID == id).ToListAsync();
            if (configs.Count > 0 && !deleteConfigs)
                return Conflict(new
                {
                    Message = $"Template masih dipakai oleh {configs.Count} config.",
                    Hint = "Panggil lagi dengan ?deleteConfigs=true untuk menghapus config-config itu sekalian, "
                         + "atau nonaktifkan template (IsActive=false) tanpa menghapus.",
                    ConfigCount = configs.Count,
                    ItemCodes = configs.Select(c => c.ItemCode).Distinct().ToList(),
                });

            await using var tx = await _db.Database.BeginTransactionAsync();
            if (configs.Count > 0) _db.RequisitionFormConfig.RemoveRange(configs);
            // RequisitionFormData.TemplateID -> FK ON DELETE SET NULL (pointer
            // histori saja; SchemaSnapshot per baris tetap utuh).
            _db.RequisitionFormTemplate.Remove(t);
            await _db.SaveChangesAsync();
            await tx.CommitAsync();
            return Ok(new { deletedTemplate = id, deletedConfigs = configs.Count });
        }

        // ──────────────────────────── CONFIG ────────────────────────────────

        [HttpGet("config")]
        public async Task<IActionResult> GetConfig([FromQuery] string itemCode, [FromQuery] string entityType = RequisitionDynamicFormService.EntityDetail)
        {
            if (string.IsNullOrWhiteSpace(itemCode)) return BadRequest("itemCode wajib.");
            var c = await _svc.ResolveConfigAsync(_db, itemCode, entityType);
            return c == null ? NotFound() : Ok(c);
        }

        [HttpPost("config")]
        public async Task<IActionResult> UpsertConfig([FromBody] RequisitionFormConfig body)
        {
            if (body == null || string.IsNullOrWhiteSpace(body.ItemCode)) return BadRequest("ItemCode wajib.");
            if (body.Mode != "Template" && body.Mode != "Custom") return BadRequest("Mode harus 'Template' atau 'Custom'.");
            if (body.Mode == "Template" && body.TemplateID == null) return BadRequest("Mode=Template butuh TemplateID.");
            if (body.Mode == "Custom" && !IsValidJson(body.SchemaJson)) return BadRequest("Mode=Custom butuh SchemaJson JSON valid.");
            if (body.SchemaJson != null && !IsValidJson(body.SchemaJson)) return BadRequest("SchemaJson bukan JSON valid.");
            if (string.IsNullOrWhiteSpace(body.EntityType)) body.EntityType = RequisitionDynamicFormService.EntityDetail;

            var existing = await _db.RequisitionFormConfig
                .FirstOrDefaultAsync(c => c.ItemCode == body.ItemCode && c.EntityType == body.EntityType);

            if (existing == null)
            {
                body.ID = 0;
                body.CreatedAt = body.UpdatedAt = DateTime.Now;
                _db.RequisitionFormConfig.Add(body);
            }
            else
            {
                existing.Mode = body.Mode;
                existing.TemplateID = body.TemplateID;
                existing.SchemaJson = body.SchemaJson;
                existing.IsActive = body.IsActive;
                existing.UpdatedAt = DateTime.Now;
            }
            await _db.SaveChangesAsync();
            return Ok(existing ?? body);
        }

        /// <summary>Hard delete satu config. RequisitionFormData.ConfigID -> FK
        /// ON DELETE SET NULL, jadi jawaban wizard yang sudah tersimpan tidak
        /// ikut terhapus (SchemaSnapshot per baris tetap jadi acuan).</summary>
        [HttpDelete("config/{id:int}")]
        public async Task<IActionResult> DeleteConfig(int id)
        {
            var c = await _db.RequisitionFormConfig.FirstOrDefaultAsync(x => x.ID == id);
            if (c == null) return NotFound();
            _db.RequisitionFormConfig.Remove(c);
            await _db.SaveChangesAsync();
            return NoContent();
        }

        /// <summary>Alternatif hapus config berdasarkan (itemCode, entityType).</summary>
        [HttpDelete("config")]
        public async Task<IActionResult> DeleteConfigByItem(
            [FromQuery] string itemCode,
            [FromQuery] string entityType = RequisitionDynamicFormService.EntityDetail)
        {
            if (string.IsNullOrWhiteSpace(itemCode)) return BadRequest("itemCode wajib.");
            var rows = await _db.RequisitionFormConfig
                .Where(c => c.ItemCode == itemCode && c.EntityType == entityType)
                .ToListAsync();
            if (rows.Count == 0) return NotFound();
            _db.RequisitionFormConfig.RemoveRange(rows);
            await _db.SaveChangesAsync();
            return Ok(new { deleted = rows.Count });
        }

        // ─────────────────────── EFFECTIVE SCHEMA ───────────────────────────

        [HttpGet("effective-schema")]
        public async Task<IActionResult> EffectiveSchema(
            [FromQuery] string itemCode,
            [FromQuery] string entityType = RequisitionDynamicFormService.EntityDetail)
        {
            if (string.IsNullOrWhiteSpace(itemCode)) return BadRequest("itemCode wajib.");
            var r = await _svc.ResolveEffectiveSchemaAsync(_db, itemCode, entityType);
            // camelCase on purpose: the Flutter client (RequisitionDynamicFormApi /
            // WizardOrchestrator) reads res['hasConfig'] / res['schema'] /
            // res['templateCode'], matching dynamic-requisition-form-frontend.md §1c
            // and the /data endpoint's ToResponse(). PupaAPI's global
            // PropertyNamingPolicy=null would otherwise emit PascalCase and the
            // wizard would never render (hasConfig always null -> falsy).
            return Ok(new
            {
                entityType = r.EntityType,
                itemCode = r.ItemCode,
                hasConfig = r.HasConfig,
                configId = r.ConfigID,
                templateId = r.TemplateID,
                templateCode = r.TemplateCode,
                templateVersion = r.TemplateVersion,
                schema = r.Schema,
            });
        }

        // ──────────────────────────── DATA ──────────────────────────────────

        [HttpGet("data")]
        public async Task<IActionResult> GetData([FromQuery] int requisitionId)
        {
            if (requisitionId <= 0) return BadRequest("requisitionId wajib.");
            var forms = await _svc.LoadForRequisitionAsync(_db, requisitionId);
            var arr = new JsonArray();
            foreach (var f in forms) arr.Add(RequisitionDynamicFormService.ToResponse(f));
            return Ok(arr);
        }

        public sealed class SaveDataRequest
        {
            public int RequisitionID { get; set; }
            public int? RequisitionDetailID { get; set; }
            public string ItemCode { get; set; } = "";
            public JsonElement DynamicForm { get; set; }
            public string? CreatedBy { get; set; }
        }

        /// <summary>Attach / update satu form dinamis ke requisition (transaksi sendiri).</summary>
        [HttpPost("data")]
        public async Task<IActionResult> SaveData([FromBody] SaveDataRequest body)
        {
            if (body == null || body.RequisitionID <= 0) return BadRequest("RequisitionID wajib.");
            if (body.DynamicForm.ValueKind != JsonValueKind.Object) return BadRequest("DynamicForm harus object.");

            var reqExists = await _db.Requisition.AnyAsync(r => r.ID == body.RequisitionID);
            if (!reqExists) return NotFound($"Requisition {body.RequisitionID} tidak ditemukan.");
            if (body.RequisitionDetailID is int did &&
                !await _db.RequisitionDetail.AnyAsync(d => d.ID == did && d.RequisitionID == body.RequisitionID))
                return BadRequest($"RequisitionDetail {did} bukan milik Requisition {body.RequisitionID}.");

            await using var tx = await _db.Database.BeginTransactionAsync();
            try
            {
                await _svc.UpsertAsync(_db, body.RequisitionID, body.RequisitionDetailID,
                    body.ItemCode, body.DynamicForm, Actor(body.CreatedBy));
                await _db.SaveChangesAsync();
                await tx.CommitAsync();
            }
            catch (RequisitionDynamicFormService.DynamicFormValidationException ex)
            {
                await tx.RollbackAsync();
                return BadRequest(new { Message = "Validasi form dinamis gagal.", ex.Errors });
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync();
                return StatusCode(500, new { Message = "Gagal menyimpan form dinamis.", Detail = ex.Message });
            }

            var forms = await _svc.LoadForRequisitionAsync(_db, body.RequisitionID);
            var saved = forms.FirstOrDefault(f => f.RequisitionDetailID == body.RequisitionDetailID);
            return Ok(saved == null ? null : RequisitionDynamicFormService.ToResponse(saved));
        }

        private static bool IsValidJson(string? s)
        {
            if (string.IsNullOrWhiteSpace(s)) return false;
            try { JsonNode.Parse(s); return true; } catch { return false; }
        }
    }
}
