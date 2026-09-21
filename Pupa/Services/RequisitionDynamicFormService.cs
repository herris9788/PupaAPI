using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Pupa.BusinessObjects;
using Pupa.BusinessObjects.Beesuite;

namespace Pupa.Services
{
    /// <summary>
    /// Logika form dinamis Requisition (Template -> Config -> Data).
    /// Referensi desain: Pupa/docs/dynamic-requisition-wizard-recommended.md
    ///
    /// ADITIF: tidak mengubah perilaku Requisition existing. Semua method di sini
    /// hanya dipanggil bila payload membawa field <c>DynamicForm</c>.
    /// </summary>
    public class RequisitionDynamicFormService
    {
        public const string EntityHeader = "Requisition";
        public const string EntityDetail = "RequisitionDetail";

        // ── Resolusi config & effective schema ──────────────────────────────

        public async Task<RequisitionFormConfig?> ResolveConfigAsync(
            BeesuiteDbContext db, string itemCode, string entityType)
        {
            if (string.IsNullOrWhiteSpace(itemCode)) return null;
            return await db.RequisitionFormConfig
                .Include(c => c.Template)
                .Where(c => c.ItemCode == itemCode && c.EntityType == entityType && c.IsActive)
                .OrderByDescending(c => c.ID)
                .FirstOrDefaultAsync();
        }

        public async Task<RequisitionFormTemplate?> ResolveTemplateByCodeAsync(
            BeesuiteDbContext db, string code)
        {
            if (string.IsNullOrWhiteSpace(code)) return null;
            return await db.RequisitionFormTemplate
                .Where(t => t.Code == code && t.IsActive && t.IsPublished)
                .OrderByDescending(t => t.Version)
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Template yang benar-benar dipakai untuk sebuah config Mode=Template:
        ///   1. versi yang dipin di config.TemplateID — HANYA kalau masih
        ///      IsActive && IsPublished (kalau admin men-draft-kan / nonaktifkan
        ///      versi itu, wizard harus ikut mati);
        ///   2. kalau tidak, versi published+aktif TERBARU dengan Code yang sama
        ///      (jadi publish versi baru otomatis menggeser wizard maju);
        ///   3. kalau tidak ada satupun -> null -> tidak ada wizard.
        /// </summary>
        public async Task<RequisitionFormTemplate?> ResolveEffectiveTemplateAsync(
            BeesuiteDbContext db, RequisitionFormConfig config)
        {
            if (config.Template is { IsActive: true, IsPublished: true })
                return config.Template;
            var code = config.Template?.Code;
            return string.IsNullOrWhiteSpace(code)
                ? null
                : await ResolveTemplateByCodeAsync(db, code!);
        }

        /// <summary>Effective schema (JSON string). effectiveTemplate = hasil
        /// ResolveEffectiveTemplateAsync (null untuk Mode=Custom).</summary>
        public string BuildEffectiveSchema(RequisitionFormConfig config, RequisitionFormTemplate? effectiveTemplate)
        {
            if (config.Mode == "Custom")
                return string.IsNullOrWhiteSpace(config.SchemaJson) ? "{}" : config.SchemaJson!;

            // Mode == "Template"
            var baseSchema = effectiveTemplate?.SchemaJson ?? "{}";
            if (string.IsNullOrWhiteSpace(config.SchemaJson))
                return baseSchema;

            // Template + override parsial
            try
            {
                var merged = MergeJson(JsonNode.Parse(baseSchema), JsonNode.Parse(config.SchemaJson!));
                return merged?.ToJsonString() ?? baseSchema;
            }
            catch
            {
                return baseSchema; // override tak valid -> jangan blokir, pakai template apa adanya
            }
        }

        /// <summary>True kalau schema punya minimal satu step berisi field —
        /// draft kosong / schema "{}" dianggap "tidak ada wizard".</summary>
        private static bool SchemaHasSteps(JsonNode? schema)
        {
            if (schema is not JsonObject obj) return false;
            if (obj["steps"] is JsonArray steps && steps.OfType<JsonObject>().Any(
                    s => s["fields"] is JsonArray f && f.Count > 0))
                return true;
            return obj["fields"] is JsonArray flat && flat.Count > 0;
        }

        public sealed class EffectiveSchemaResult
        {
            public string EntityType { get; set; } = EntityDetail;
            public string ItemCode { get; set; } = "";
            public int? ConfigID { get; set; }
            public int? TemplateID { get; set; }
            public string? TemplateCode { get; set; }
            public int? TemplateVersion { get; set; }
            public bool HasConfig { get; set; }
            public JsonNode? Schema { get; set; }
        }

        public async Task<EffectiveSchemaResult> ResolveEffectiveSchemaAsync(
            BeesuiteDbContext db, string itemCode, string entityType)
        {
            var result = new EffectiveSchemaResult { EntityType = entityType, ItemCode = itemCode };
            var config = await ResolveConfigAsync(db, itemCode, entityType);
            if (config == null) return result; // tidak ada config -> tidak ada wizard

            RequisitionFormTemplate? tpl = null;
            if (config.Mode == "Template")
            {
                tpl = await ResolveEffectiveTemplateAsync(db, config);
                if (tpl == null) return result; // template di-draft-kan / nonaktif / hilang -> tidak ada wizard
            }

            var schemaNode = SafeParse(BuildEffectiveSchema(config, tpl));
            if (!SchemaHasSteps(schemaNode)) return result; // schema kosong -> tidak ada wizard

            result.HasConfig = true;
            result.ConfigID = config.ID;
            result.TemplateID = tpl?.ID;
            result.TemplateCode = tpl?.Code;
            result.TemplateVersion = tpl?.Version;
            result.Schema = schemaNode;
            return result;
        }

        // ── Simpan jawaban (dipanggil di dalam transaksi caller) ────────────

        public sealed class DynamicFormValidationException : Exception
        {
            public IReadOnlyList<string> Errors { get; }
            public DynamicFormValidationException(IReadOnlyList<string> errors)
                : base("Validasi form dinamis gagal: " + string.Join("; ", errors))
                => Errors = errors;
        }

        /// <summary>
        /// Validasi + upsert satu RequisitionFormData. TIDAK memanggil SaveChanges —
        /// caller yang commit dalam transaksinya sendiri.
        /// </summary>
        public async Task UpsertAsync(
            BeesuiteDbContext db,
            int requisitionId,
            int? requisitionDetailId,
            string itemCode,
            JsonElement dynamicForm,
            string? actor)
        {
            var entityType = requisitionDetailId == null ? EntityHeader : EntityDetail;
            var (values, templateCode, payloadItemCode) = ParsePayload(dynamicForm);
            if (string.IsNullOrWhiteSpace(itemCode) && !string.IsNullOrWhiteSpace(payloadItemCode))
                itemCode = payloadItemCode!;

            string schema;
            int? configId = null;
            int? templateId = null;

            var config = await ResolveConfigAsync(db, itemCode, entityType);
            if (config != null)
            {
                // Best-effort: pakai template efektif; kalau wizard sudah
                // dimatikan sejak user membuka form, jangan hilangkan input yang
                // sudah diisi — pakai versi yang dipin sebagai fallback snapshot.
                RequisitionFormTemplate? tpl = config.Mode == "Template"
                    ? (await ResolveEffectiveTemplateAsync(db, config) ?? config.Template)
                    : null;
                schema = BuildEffectiveSchema(config, tpl);
                configId = config.ID;
                templateId = tpl?.ID ?? (config.Mode == "Template" ? config.TemplateID : null);
            }
            else if (!string.IsNullOrWhiteSpace(templateCode))
            {
                var tpl = await ResolveTemplateByCodeAsync(db, templateCode!)
                          ?? throw new DynamicFormValidationException(new[]
                             { $"Template '{templateCode}' tidak ditemukan / belum published." });
                schema = tpl.SchemaJson;
                templateId = tpl.ID;
            }
            else
            {
                throw new DynamicFormValidationException(new[]
                {
                    $"Tidak ada konfigurasi form dinamis untuk item '{itemCode}' ({entityType}), " +
                    "dan payload tidak menyertakan 'templateCode'."
                });
            }

            var schemaNode = SafeParse(schema);
            var errors = Validate(schemaNode, values);
            if (errors.Count > 0)
                throw new DynamicFormValidationException(errors);

            var status = AllRequiredFilled(schemaNode, values) ? "Completed" : "Draft";
            var now = DateTime.Now;
            var valuesJson = values.ToJsonString();

            var existing = await db.RequisitionFormData.FirstOrDefaultAsync(d =>
                d.RequisitionID == requisitionId &&
                d.RequisitionDetailID == requisitionDetailId);

            if (existing == null)
            {
                db.RequisitionFormData.Add(new RequisitionFormData
                {
                    RequisitionID = requisitionId,
                    RequisitionDetailID = requisitionDetailId,
                    ItemCode = itemCode,
                    ConfigID = configId,
                    TemplateID = templateId,
                    SchemaSnapshot = schema,
                    Values = valuesJson,
                    Status = status,
                    CreatedAt = now,
                    UpdatedAt = now,
                    CreatedBy = actor,
                    UpdatedBy = actor,
                });
            }
            else
            {
                existing.ItemCode = itemCode;
                existing.ConfigID = configId;
                existing.TemplateID = templateId;
                existing.SchemaSnapshot = schema;
                existing.Values = valuesJson;
                existing.Status = status;
                existing.UpdatedAt = now;
                existing.UpdatedBy = actor;
            }
        }

        // ── Persist dari graph Requisition (dipakai di CreateRequisition) ───

        /// <summary>True bila ada minimal satu DynamicForm di header / detail.</summary>
        public static bool GraphHasDynamicForm(Requisition requisition)
        {
            if (requisition.DynamicForm != null) return true;
            if (requisition.RequisitionDetails != null)
                foreach (var d in requisition.RequisitionDetails)
                    if (d.DynamicForm != null) return true;
            return false;
        }

        /// <summary>
        /// Validasi SEMUA DynamicForm di graph tanpa menulis apa pun. Dipanggil
        /// SEBELUM requisition disimpan → gagal = 400, tidak ada yang tersimpan.
        /// </summary>
        public async Task ValidateGraphAsync(BeesuiteDbContext db, Requisition requisition)
        {
            if (!GraphHasDynamicForm(requisition)) return;
            var errors = new List<string>();

            if (requisition.DynamicForm is JsonElement hdr)
                errors.AddRange(await ValidateOneAsync(db, EntityHeader, await HeaderItemCodeAsync(db, requisition), hdr));

            if (requisition.RequisitionDetails != null)
            {
                foreach (var d in requisition.RequisitionDetails)
                {
                    if (d.DynamicForm is not JsonElement df) continue;
                    var code = await DetailItemCodeAsync(db, d, df);
                    errors.AddRange(await ValidateOneAsync(db, EntityDetail, code, df,
                        prefix: $"Item '{code}': "));
                }
            }

            if (errors.Count > 0)
                throw new DynamicFormValidationException(errors);
        }

        /// <summary>
        /// Tulis semua DynamicForm di graph (requisition & detail sudah punya ID).
        /// TIDAK memanggil SaveChanges. Panggil ValidateGraphAsync lebih dulu.
        /// </summary>
        public async Task PersistGraphAsync(BeesuiteDbContext db, Requisition requisition, string? actor)
        {
            if (!GraphHasDynamicForm(requisition)) return;

            if (requisition.DynamicForm is JsonElement hdr)
                await UpsertAsync(db, requisition.ID, null, await HeaderItemCodeAsync(db, requisition), hdr, actor);

            if (requisition.RequisitionDetails != null)
            {
                foreach (var d in requisition.RequisitionDetails)
                {
                    if (d.DynamicForm is not JsonElement df) continue;
                    await UpsertAsync(db, requisition.ID, d.ID, await DetailItemCodeAsync(db, d, df), df, actor);
                }
            }
        }

        // ── Baca kembali (overlay ke response) ─────────────────────────────

        public async Task<List<RequisitionFormData>> LoadForRequisitionAsync(
            BeesuiteDbContext db, int requisitionId)
        {
            return await db.RequisitionFormData
                .AsNoTracking()
                .Where(d => d.RequisitionID == requisitionId)
                .ToListAsync();
        }

        /// <summary>Bentuk response satu form: { templateCode, templateVersion, status, values }.</summary>
        public static JsonObject ToResponse(RequisitionFormData d)
        {
            return new JsonObject
            {
                ["formId"] = d.ID,
                ["requisitionDetailId"] = d.RequisitionDetailID,
                ["itemCode"] = d.ItemCode,
                ["status"] = d.Status,
                ["values"] = SafeParse(d.Values) ?? new JsonObject(),
                ["schema"] = SafeParse(d.SchemaSnapshot),
            };
        }

        public async Task OverlayAsync(BeesuiteDbContext db, Requisition requisition)
        {
            var forms = await LoadForRequisitionAsync(db, requisition.ID);
            if (forms.Count == 0) return;

            var header = forms.FirstOrDefault(f => f.RequisitionDetailID == null);
            if (header != null)
                requisition.DynamicForm = JsonSerializer.SerializeToElement(ToResponse(header));

            if (requisition.RequisitionDetails != null)
            {
                var byDetail = forms.Where(f => f.RequisitionDetailID != null)
                                    .ToDictionary(f => f.RequisitionDetailID!.Value);
                foreach (var d in requisition.RequisitionDetails)
                    if (byDetail.TryGetValue(d.ID, out var f))
                        d.DynamicForm = JsonSerializer.SerializeToElement(ToResponse(f));
            }
        }

        // ── Helper internal ───────────────────────────────────────────────

        private async Task<List<string>> ValidateOneAsync(
            BeesuiteDbContext db, string entityType, string itemCode, JsonElement payload, string prefix = "")
        {
            var (values, templateCode, _) = ParsePayload(payload);
            string? schema = null;

            var config = await ResolveConfigAsync(db, itemCode, entityType);
            if (config != null)
            {
                RequisitionFormTemplate? tpl = config.Mode == "Template"
                    ? (await ResolveEffectiveTemplateAsync(db, config) ?? config.Template)
                    : null;
                schema = BuildEffectiveSchema(config, tpl);
            }
            else if (!string.IsNullOrWhiteSpace(templateCode))
                schema = (await ResolveTemplateByCodeAsync(db, templateCode!))?.SchemaJson;

            if (schema == null)
                return new List<string> { $"{prefix}tidak ada konfigurasi form dinamis untuk item '{itemCode}'." };

            return Validate(SafeParse(schema), values).Select(e => prefix + e).ToList();
        }

        private async Task<string> HeaderItemCodeAsync(BeesuiteDbContext db, Requisition r)
        {
            if (r.DynamicForm is JsonElement e)
            {
                var (_, _, code) = ParsePayload(e);
                if (!string.IsNullOrWhiteSpace(code)) return code!;
            }
            var firstDetail = r.RequisitionDetails?.FirstOrDefault();
            return firstDetail == null ? "" : await ResolveItemCodeAsync(db, firstDetail.ItemID);
        }

        private async Task<string> DetailItemCodeAsync(BeesuiteDbContext db, RequisitionDetail d, JsonElement payload)
        {
            var (_, _, code) = ParsePayload(payload);
            if (!string.IsNullOrWhiteSpace(code)) return code!;
            return await ResolveItemCodeAsync(db, d.ItemID);
        }

        private static async Task<string> ResolveItemCodeAsync(BeesuiteDbContext db, int? itemId)
        {
            if (itemId == null) return "";
            var code = await db.Item.AsNoTracking()
                .Where(i => i.ItemID == itemId.Value)
                .Select(i => i.ItemCode)
                .FirstOrDefaultAsync();
            return code ?? "";
        }

        private static (JsonObject values, string? templateCode, string? itemCode) ParsePayload(JsonElement payload)
        {
            if (payload.ValueKind != JsonValueKind.Object)
                return (new JsonObject(), null, null);

            string? templateCode = null;
            if (payload.TryGetProperty("templateCode", out var tc) && tc.ValueKind == JsonValueKind.String)
                templateCode = tc.GetString();

            string? itemCode = null;
            if (payload.TryGetProperty("itemCode", out var ic) && ic.ValueKind == JsonValueKind.String)
                itemCode = ic.GetString();

            JsonObject values;
            if (payload.TryGetProperty("values", out var v) && v.ValueKind == JsonValueKind.Object)
                values = JsonNode.Parse(v.GetRawText())!.AsObject();
            else
            {
                // seluruh object = values (minus key meta)
                var obj = JsonNode.Parse(payload.GetRawText())!.AsObject();
                obj.Remove("templateCode");
                obj.Remove("templateVersion");
                obj.Remove("itemCode");
                values = obj;
            }
            return (values, templateCode, itemCode);
        }

        private static JsonNode? SafeParse(string? json)
        {
            if (string.IsNullOrWhiteSpace(json)) return null;
            try { return JsonNode.Parse(json); } catch { return null; }
        }

        /// Field dari step yang SEDANG TAMPIL saja (step.visibleWhen dievaluasi
        /// terhadap [values]). Field renderer FE cuma pernah menampilkan field di
        /// step yang visible (tidak ada visibleWhen per-field, cuma per-step —
        /// lihat DynamicFormWizardRenderer._visibleSteps) — kalau ini tidak
        /// mengikuti, field wajib di step yang disembunyikan (mis. "Audit Result"
        /// yang cuma tampil kalau Purpose = Audit Findings) akan tetap dianggap
        /// wajib untuk kondisi lain (mis. Purpose = New Cargo) dan submit gagal
        /// 400 padahal user tidak pernah melihat field itu.
        private static IEnumerable<(string? StepKey, JsonObject Field)> FieldsOf(JsonNode? schema, JsonObject values)
        {
            if (schema is not JsonObject obj) yield break;
            if (obj["steps"] is JsonArray steps)
            {
                foreach (var step in steps.OfType<JsonObject>())
                {
                    if (step["visibleWhen"]?.GetValue<string>() is string vw && !EvalCondition(vw, values))
                        continue;
                    var stepKey = step["key"]?.GetValue<string>();
                    if (step["fields"] is JsonArray fields)
                        foreach (var f in fields.OfType<JsonObject>())
                            yield return (stepKey, f);
                }
            }
            else if (obj["fields"] is JsonArray flat)
            {
                foreach (var f in flat.OfType<JsonObject>())
                    yield return (null, f);
            }
        }

        // Key di [values] mengikuti konvensi FE: "{step.key}.{field.key}" kalau field
        // ada dalam step, atau field.key saja untuk schema flat tanpa step.
        private static string? CompositeKey(string? stepKey, string? fieldKey)
            => string.IsNullOrEmpty(fieldKey)
                ? null
                : string.IsNullOrEmpty(stepKey) ? fieldKey : $"{stepKey}.{fieldKey}";

        private static bool IsBlank(JsonNode? v)
            => v == null
               || (v is JsonValue jv && jv.TryGetValue<string>(out var s) && string.IsNullOrWhiteSpace(s))
               || (v is JsonArray a && a.Count == 0);

        private static List<string> Validate(JsonNode? schema, JsonObject values)
        {
            var errors = new List<string>();
            foreach (var (stepKey, field) in FieldsOf(schema, values))
            {
                var key = CompositeKey(stepKey, field["key"]?.GetValue<string>());
                if (string.IsNullOrEmpty(key)) continue;
                var label = field["label"]?.GetValue<string>() ?? key;

                bool required = field["required"]?.GetValue<bool?>() == true;
                if (!required && field["requiredWhen"]?.GetValue<string>() is string rw)
                    required = EvalCondition(rw, values);

                if (required && IsBlank(values.TryGetPropertyValue(key, out var val) ? val : null))
                    errors.Add($"'{label}' wajib diisi.");
            }

            // blok validation[] tingkat schema (ekspresi sederhana)
            if (schema is JsonObject o && o["validation"] is JsonArray rules)
            {
                foreach (var rule in rules.OfType<JsonObject>())
                {
                    var expr = rule["rule"]?.GetValue<string>();
                    if (string.IsNullOrWhiteSpace(expr)) continue;
                    // rule dianggap "harus true"; kalau tak terparse -> lewati
                    if (TryEval(expr!, values, out var ok) && !ok)
                        errors.Add(rule["message"]?.GetValue<string>() ?? $"Aturan tidak terpenuhi: {expr}");
                }
            }
            return errors;
        }

        private static bool AllRequiredFilled(JsonNode? schema, JsonObject values)
        {
            foreach (var (stepKey, field) in FieldsOf(schema, values))
            {
                var key = CompositeKey(stepKey, field["key"]?.GetValue<string>());
                if (string.IsNullOrEmpty(key)) continue;
                bool required = field["required"]?.GetValue<bool?>() == true;
                if (!required && field["requiredWhen"]?.GetValue<string>() is string rw)
                    required = EvalCondition(rw, values);
                if (required && IsBlank(values.TryGetPropertyValue(key, out var v) ? v : null))
                    return false;
            }
            return true;
        }

        // Evaluator ekspresi minimal: "a.b == 'x' && c > 3 || d != false"
        //  - operand: fieldKey atau step.fieldKey, dipakai utuh sebagai key ke
        //    [values] (mengikuti konvensi composite key FE, lihat CompositeKey)
        //  - operator: == != < <= > >=
        //  - literal: 'str', angka, true/false
        //  - gabung: && (semua) / || (salah satu). Presedensi: || memisah grup &&.
        private static bool EvalCondition(string expr, JsonObject values)
            => TryEval(expr, values, out var r) && r;

        private static bool TryEval(string expr, JsonObject values, out bool result)
        {
            result = false;
            try
            {
                foreach (var orPart in expr.Split("||"))
                {
                    bool all = true;
                    foreach (var andPart in orPart.Split("&&"))
                    {
                        if (!TryEvalComparison(andPart.Trim(), values, out var ok)) return false;
                        all &= ok;
                    }
                    if (all) { result = true; return true; }
                }
                result = false;
                return true;
            }
            catch { return false; }
        }

        private static bool TryEvalComparison(string expr, JsonObject values, out bool ok)
        {
            ok = false;
            string[] ops = { "==", "!=", "<=", ">=", "<", ">" };
            var op = ops.FirstOrDefault(o => expr.Contains(o));
            if (op == null) return false;

            var idx = expr.IndexOf(op, StringComparison.Ordinal);
            var lhsRaw = expr[..idx].Trim();
            var rhsRaw = expr[(idx + op.Length)..].Trim();

            var lhs = ResolveOperand(lhsRaw, values);
            var rhs = ParseLiteral(rhsRaw);

            switch (op)
            {
                case "==": ok = LooseEquals(lhs, rhs); return true;
                case "!=": ok = !LooseEquals(lhs, rhs); return true;
            }

            if (TryNum(lhs, out var ln) && TryNum(rhs, out var rn))
            {
                ok = op switch
                {
                    "<" => ln < rn,
                    ">" => ln > rn,
                    "<=" => ln <= rn,
                    ">=" => ln >= rn,
                    _ => false
                };
                return true;
            }
            return true; // tidak bisa dibandingkan numerik -> anggap tidak melanggar
        }

        private static object? ResolveOperand(string raw, JsonObject values)
        {
            if (raw.Length >= 2 && (raw[0] == '\'' || raw[0] == '"')) return raw.Trim('\'', '"');
            if (raw is "true" or "false") return bool.Parse(raw);
            if (double.TryParse(raw, NumberStyles.Any, CultureInfo.InvariantCulture, out var n)) return n;

            if (values.TryGetPropertyValue(raw, out var node) && node is JsonValue jv)
            {
                if (jv.TryGetValue<bool>(out var b)) return b;
                if (jv.TryGetValue<double>(out var d)) return d;
                if (jv.TryGetValue<string>(out var s)) return s;
            }
            return null;
        }

        private static object? ParseLiteral(string raw)
        {
            if (raw.Length >= 2 && (raw[0] == '\'' || raw[0] == '"')) return raw.Trim('\'', '"');
            if (raw is "true" or "false") return bool.Parse(raw);
            if (double.TryParse(raw, NumberStyles.Any, CultureInfo.InvariantCulture, out var n)) return n;
            return raw;
        }

        private static bool TryNum(object? o, out double n)
        {
            switch (o)
            {
                case double d: n = d; return true;
                case string s when double.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var p):
                    n = p; return true;
                default: n = 0; return false;
            }
        }

        private static bool LooseEquals(object? a, object? b)
        {
            if (a == null || b == null) return a == null && b == null;
            if (TryNum(a, out var an) && TryNum(b, out var bn)) return Math.Abs(an - bn) < 1e-9;
            return string.Equals(a.ToString(), b.ToString(), StringComparison.OrdinalIgnoreCase);
        }

        private static JsonNode? MergeJson(JsonNode? baseNode, JsonNode? overrideNode)
        {
            if (overrideNode == null) return baseNode;
            if (baseNode == null) return overrideNode.DeepClone();

            if (baseNode is JsonObject bo && overrideNode is JsonObject oo)
            {
                var result = (JsonObject)bo.DeepClone();
                foreach (var kv in oo)
                {
                    result[kv.Key] = result.TryGetPropertyValue(kv.Key, out var existing)
                        ? MergeJson(existing, kv.Value)
                        : kv.Value?.DeepClone();
                }
                return result;
            }

            // array of objects with "key" -> merge by key; else override wins
            if (baseNode is JsonArray ba && overrideNode is JsonArray oa &&
                ba.OfType<JsonObject>().All(x => x["key"] != null))
            {
                var result = new JsonArray();
                var oMap = oa.OfType<JsonObject>().Where(x => x["key"] != null)
                            .ToDictionary(x => x["key"]!.GetValue<string>());
                foreach (var item in ba.OfType<JsonObject>())
                {
                    var k = item["key"]!.GetValue<string>();
                    result.Add(oMap.TryGetValue(k, out var ov)
                        ? MergeJson(item, ov)
                        : item.DeepClone());
                    oMap.Remove(k);
                }
                foreach (var leftover in oMap.Values) result.Add(leftover.DeepClone());
                return result;
            }

            return overrideNode.DeepClone();
        }
    }
}
