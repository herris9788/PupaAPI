using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using System.IO;
using Microsoft.Data.SqlClient;
using System.Security.Policy;
using System.Drawing;
using System.Threading;
using System.Data;
using Microsoft.EntityFrameworkCore;
using Pupa.BusinessObjects.Beesuite;
using Pupa.BusinessObjects;
using System.Collections.ObjectModel;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;


namespace Pupa.Controllers
{

    [Route("beesuite/api/[controller]")]
    public class RequisitionController : Controller
    {
        private const string FreonItemCodePrefix = "T05.002";
        private const string FreonAcSystem = "AC System";
        private const string FreonProvisionSystem = "Provision Refrigeration System";
        private const string FreonDamageReportType = "FreonDamageReport";
        private const int FreonStandardCycleDays = 90;
        private const string InvoiceReceiptType = "InvoiceReceipt";

        private readonly IConfiguration _configuration;
        private readonly BeesuiteDbContext _db;
        private readonly IHttpClientFactory _httpClientFactory;

        public RequisitionController(BeesuiteDbContext db, IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            _db = db;
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
        }


        [HttpPost("Create")]
        public async Task<IActionResult> CreateRequisition([FromBody] Requisition Body)
        {
            try
            {
                if (Body == null)
                    return BadRequest("Request body cannot be null.");
                var vessel = await _db.InventoryUser.FirstOrDefaultAsync(x => x.ID == Body.VesselID);
                if (vessel == null)
                    return NotFound($"Vessel with ID {Body.VesselID} not found.");
                var docFormat = await _db.DocumentNumbering.FirstOrDefaultAsync(x => x.Vessel == vessel.InventoryUserName && x.Type == "ItemRequest");
                if (docFormat == null)
                    return NotFound($"Document numbering format for vessel '{vessel.InventoryUserName}' not found.");
                var now = DateTime.Now;
                var yearStr = now.ToString("yy");
                var monthStr = now.ToString("MM");
                var prefix = docFormat.Format
                    .Replace("<VC>", docFormat.VesselCode)
                    .Replace("<YY>", yearStr)
                    .Replace("<MM>", monthStr)
                    .Replace("<N4>", "");
                var lastNumber = await _db.Requisition
                    .Where(x => x.RequisitionNumber.StartsWith(prefix) &&
                                x.CreatedAt.Value.Year == now.Year &&
                                x.CreatedAt.Value.Month == now.Month)
                    .OrderByDescending(x => x.RequisitionNumber)
                    .Select(x => x.RequisitionNumber)
                    .FirstOrDefaultAsync();
                int nextNumber = 1;
                if (lastNumber != null)
                {
                    var lastSeq = lastNumber.Substring(prefix.Length);
                    if (int.TryParse(lastSeq, out int parsed))
                        nextNumber = parsed + 1;
                }
                Body.RequisitionNumber = docFormat.Format
                    .Replace("<VC>", docFormat.VesselCode)
                    .Replace("<YY>", yearStr)
                    .Replace("<MM>", monthStr)
                    .Replace("<N4>", nextNumber.ToString("D4"));
                Body.CreatedAt = now;
                var freonValidationError = await ApplyFreonEvaluationAsync(Body, now);
                if (freonValidationError != null)
                    return freonValidationError;

                await _db.AddAsync(Body);
                await _db.SaveChangesAsync();
                Body.RequisitionDetails = new ObservableCollection<RequisitionDetail>();
                Body.InventoryUser = null;
                return Ok(Body);
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, new
                {
                    Message = "Database error occurred while saving requisition.",
                    Detail = ex.InnerException?.Message ?? ex.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Message = "An unexpected error occurred.",
                    Detail = ex.InnerException?.Message ?? ex.Message
                });
            }
        }

        [HttpGet("FreonLastRequest")]
        public async Task<IActionResult> GetFreonLastRequest(
            [FromQuery] int vesselID,
            [FromQuery] int itemID,
            [FromQuery] string placementArea,
            [FromQuery] decimal qtyRequest = 1,
            [FromQuery] bool includeVoid = false,
            [FromQuery] bool includePending = false)
        {
            if (vesselID <= 0)
                return BadRequest("VesselID is required.");
            if (itemID <= 0)
                return BadRequest("ItemID is required.");
            if (string.IsNullOrWhiteSpace(placementArea))
                return BadRequest("PlacementArea is required.");

            var item = await _db.Item
                .AsNoTracking()
                .Where(x => x.ItemID == itemID)
                .Select(x => new { x.ItemID, x.ItemCode })
                .FirstOrDefaultAsync();

            if (item == null)
                return NotFound($"Item with ID {itemID} not found.");
            if (!IsFreonItemCode(item.ItemCode))
                return BadRequest($"Item {item.ItemCode} is not a Freon/Refrigerant item.");

            var now = DateTime.Now;
            var area = placementArea.Trim();
            var lastRequestDate = await GetLastFreonRequestDateAsync(vesselID, itemID, area, includeVoid, includePending);
            var intervalDays = GetIntervalDays(lastRequestDate, now);
            var damageReportRequired = IsFreonDamageReportRequired(qtyRequest, intervalDays);

            return Ok(new
            {
                LastRequestDate = lastRequestDate,
                FreonIntervalDays = intervalDays,
                FreonDamageReportRequired = damageReportRequired,
                FreonSystem = InferFreonSystem(area),
                FreonEvaluationScenario = GetFreonEvaluationScenario(qtyRequest, intervalDays)
            });
        }

        [HttpPost("{id:int}/VerifyNota")]
        public async Task<IActionResult> VerifyNota(
            int id,
            [FromQuery] bool force,
            CancellationToken cancellationToken)
        {
            var webhookUrl = _configuration["NotaVerification:WebhookUrl"];
            if (string.IsNullOrWhiteSpace(webhookUrl))
                return StatusCode(500, new { Message = "Nota verification webhook URL is not configured." });

            var requisitionExists = await _db.Requisition
                .AsNoTracking()
                .AnyAsync(x => x.ID == id, cancellationToken);

            if (!requisitionExists)
                return NotFound($"Requisition with ID {id} not found.");

            var invoiceRel = await _db.RequisitionAttachmentRel
                .Include(x => x.Attachment)
                .Where(x => x.RequisitionID == id &&
                            x.Type != null &&
                            x.Type.ToLower() == InvoiceReceiptType.ToLower() &&
                            x.AttachmentID.HasValue)
                .OrderByDescending(x => x.ID)
                .FirstOrDefaultAsync(cancellationToken);

            if (invoiceRel?.Attachment == null)
                return NotFound($"Invoice receipt attachment for requisition {id} not found.");

            var existing = await _db.RequisitionNotaVerification
                .FirstOrDefaultAsync(x =>
                    x.RequisitionID == id &&
                    x.AttachmentID == invoiceRel.AttachmentID, cancellationToken);

            if (existing != null && existing.ScanStatus == "SUCCESS" && !force)
                return Ok(existing);

            var row = existing ?? new RequisitionNotaVerification
            {
                RequisitionID = id,
                RequisitionAttachmentRelID = invoiceRel.ID,
                AttachmentID = invoiceRel.AttachmentID
            };

            row.RequisitionAttachmentRelID = invoiceRel.ID;
            row.AttachmentID = invoiceRel.AttachmentID;
            row.FileName = invoiceRel.Attachment.FileName;
            row.ScanStatus = "PROCESSING";
            row.ErrorMessage = null;
            row.LastAttemptAt = DateTime.UtcNow;
            row.NextRetryAt = null;

            if (existing == null)
                _db.RequisitionNotaVerification.Add(row);

            await _db.SaveChangesAsync(cancellationToken);

            await RunNotaVerificationAsync(row, invoiceRel.Attachment, webhookUrl, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);

            return Ok(row);
        }

        private async Task<IActionResult?> ApplyFreonEvaluationAsync(Requisition requisition, DateTime now)
        {
            if (requisition.RequisitionDetails == null || requisition.RequisitionDetails.Count == 0)
                return null;

            var itemIds = requisition.RequisitionDetails
                .Where(x => x.ItemID.HasValue && x.ItemID.Value > 0)
                .Select(x => x.ItemID!.Value)
                .Distinct()
                .ToList();

            if (itemIds.Count == 0)
                return null;

            var _itemCodes = new List<string>()
            {
                "T05.002.0008","T05.002.0011","T05.002.00116","T05.002.00118","T05.002.00121","T05.002.00123","T05.002.00126","T05.002.00127","T05.002.00128","T05.002.00129","T05.002.0013","T05.002.00132","T05.002.00149","T05.002.00193","T05.002.0024","T05.002.0025","T05.002.0035"
            };

            var freonItemIds = await _db.Item
                .AsNoTracking()
                .Where(x => itemIds.Contains(x.ItemID) && x.ItemCode != null && _itemCodes.Contains(x.ItemCode))
                .Select(x => x.ItemID)
                .ToListAsync();

            if (freonItemIds.Count == 0)
                return null;

            var freonItemIdSet = freonItemIds.ToHashSet();

            foreach (var detail in requisition.RequisitionDetails.Where(x => x.ItemID.HasValue && freonItemIdSet.Contains(x.ItemID.Value)))
            {
                var area = detail.PlacementArea?.Trim();
                if (string.IsNullOrWhiteSpace(area))
                {
                    return BadRequest(new
                    {
                        Message = "PlacementArea is required for Freon/Refrigerant item T05.002.",
                        ItemID = detail.ItemID
                    });
                }

                detail.PlacementArea = area;

                var reason = ExtractCommentValue(detail.Comments, "Reason") ?? NormalizeFreonReason(detail.Purpose);
                if (string.IsNullOrWhiteSpace(reason))
                {
                    return BadRequest(new
                    {
                        Message = "Reason is required for Freon/Refrigerant item T05.002.",
                        ItemID = detail.ItemID,
                        PlacementArea = detail.PlacementArea
                    });
                }

                detail.FreonSystem = string.IsNullOrWhiteSpace(detail.FreonSystem)
                    ? InferFreonSystem(area)
                    : detail.FreonSystem.Trim();

                var lastRequestDate = await GetLastFreonRequestDateAsync(requisition.VesselID!.Value, detail.ItemID!.Value, area);
                var intervalDays = GetIntervalDays(lastRequestDate, now);
                var qty = detail.QtyRequest ?? 0;
                var damageReportRequired = IsFreonDamageReportRequired(qty, intervalDays);

                detail.FreonLastRequestDate = lastRequestDate;
                detail.FreonIntervalDays = intervalDays;
                detail.FreonDamageReportRequired = damageReportRequired;
                detail.FreonEvaluationScenario = GetFreonEvaluationScenario(qty, intervalDays);

                if (damageReportRequired && !HasFreonDamageReport(detail))
                {
                    return BadRequest(new
                    {
                        Message = "Damage Report is required for Freon/Refrigerant item T05.002.",
                        ItemID = detail.ItemID,
                        PlacementArea = detail.PlacementArea,
                        detail.QtyRequest,
                        detail.FreonLastRequestDate,
                        detail.FreonIntervalDays,
                        detail.FreonEvaluationScenario
                    });
                }
            }

            return null;
        }

        private async Task<DateTime?> GetLastFreonRequestDateAsync(
            int vesselId,
            int itemId,
            string placementArea,
            bool includeVoid = false,
            bool includePending = false)
        {
            var normalizedArea = placementArea.Trim().ToLower();

            var query = _db.RequisitionDetail
                .AsNoTracking()
                .Where(x => x.ItemID == itemId &&
                            x.PlacementArea != null &&
                            x.PlacementArea.ToLower() == normalizedArea &&
                            x.Requisition != null &&
                            x.Requisition.VesselID == vesselId);

            if (!includeVoid)
            {
                query = query.Where(x => x.Requisition!.Status == null ||
                                         x.Requisition.Status.ToUpper() != "VOID");
            }

            if (!includePending)
            {
                query = query.Where(x => x.Requisition!.Status == null ||
                                         x.Requisition.Status.ToUpper() != "PENDING");
            }

            return await query
                .OrderByDescending(x => x.Requisition!.CreatedAt ?? x.Requisition.Date)
                .Select(x => x.Requisition!.CreatedAt ?? x.Requisition.Date)
                .FirstOrDefaultAsync();
        }

        private static bool IsFreonItemCode(string? itemCode)
        {
            return itemCode?.StartsWith(FreonItemCodePrefix, StringComparison.OrdinalIgnoreCase) == true;
        }

        private static int? GetIntervalDays(DateTime? lastRequestDate, DateTime now)
        {
            if (!lastRequestDate.HasValue)
                return null;

            return Math.Max(0, (int)Math.Floor((now.Date - lastRequestDate.Value.Date).TotalDays));
        }

        private static bool IsFreonDamageReportRequired(decimal qtyRequest, int? intervalDays)
        {
            if (qtyRequest >= 4)
                return true;
            if (!intervalDays.HasValue)
                return false;

            return intervalDays.Value < FreonStandardCycleDays;
        }

        private static string GetFreonEvaluationScenario(decimal qtyRequest, int? intervalDays)
        {
            if (qtyRequest >= 4)
                return "B_HIGH_VOLUME";
            if (!intervalDays.HasValue)
                return "NO_HISTORY";
            if (intervalDays.Value >= FreonStandardCycleDays)
                return "A1_STANDARD_CYCLE";

            return "A2_FREQUENT_REQUEST";
        }

        private static string InferFreonSystem(string placementArea)
        {
            return string.Equals(placementArea.Trim(), FreonProvisionSystem, StringComparison.OrdinalIgnoreCase)
                ? FreonProvisionSystem
                : FreonAcSystem;
        }

        private static string? NormalizeFreonReason(string? reason)
        {
            if (string.IsNullOrWhiteSpace(reason))
                return null;

            return reason.Trim();
        }

        private static string? ExtractCommentValue(string? comments, string label)
        {
            if (string.IsNullOrWhiteSpace(comments))
                return null;

            var marker = $"[{label}]";
            var lines = comments.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
            foreach (var line in lines)
            {
                var trimmed = line.Trim();
                if (!trimmed.StartsWith(marker, StringComparison.OrdinalIgnoreCase))
                    continue;

                var value = trimmed.Substring(marker.Length).Trim();
                return string.IsNullOrWhiteSpace(value) ? null : value;
            }

            return null;
        }

        private static bool HasFreonDamageReport(RequisitionDetail detail)
        {
            return detail.RequisitionDetailAttachmentRels?.Any(x =>
                string.Equals(x.Type, FreonDamageReportType, StringComparison.OrdinalIgnoreCase) &&
                (x.AttachmentID.HasValue || x.Attachment != null)) == true;
        }

        private async Task RunNotaVerificationAsync(
            RequisitionNotaVerification row,
            Attachment attachment,
            string webhookUrl,
            CancellationToken cancellationToken)
        {
            byte[] fileBytes;
            try
            {
                fileBytes = DecodeAttachmentBase64(attachment);
            }
            catch (Exception ex)
            {
                MarkNotaVerificationFailed(row, $"Attachment content is invalid: {ex.Message}");
                return;
            }

            const int maxAttempts = 3;
            string? lastError = null;

            for (var attempt = 1; attempt <= maxAttempts; attempt++)
            {
                row.RetryCount += 1;
                row.LastAttemptAt = DateTime.UtcNow;

                try
                {
                    using var request = new MultipartFormDataContent();
                    using var fileContent = new ByteArrayContent(fileBytes);
                    var contentType = ResolveAttachmentContentType(attachment);
                    fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);
                    request.Add(fileContent, "file", attachment.FileName ?? $"nota-{attachment.ID}.jpg");

                    var client = _httpClientFactory.CreateClient();
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                    using var response = await client.PostAsync(webhookUrl, request, cancellationToken);
                    var body = await response.Content.ReadAsStringAsync(cancellationToken);

                    if (response.IsSuccessStatusCode)
                    {
                        if (string.IsNullOrWhiteSpace(body))
                        {
                            lastError = "Nota verification webhook returned HTTP 200 with empty response body.";
                            if (attempt == maxAttempts)
                                break;

                            await Task.Delay(TimeSpan.FromSeconds(attempt * 2), cancellationToken);
                            continue;
                        }

                        NotaVerificationWebhookResponse? result;
                        try
                        {
                            result = JsonSerializer.Deserialize<NotaVerificationWebhookResponse>(
                                body,
                                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                        }
                        catch (JsonException ex)
                        {
                            lastError = $"Nota verification webhook returned invalid JSON: {ex.Message}. Body: {TrimTelegramText(body, 500)}";
                            if (attempt == maxAttempts)
                                break;

                            await Task.Delay(TimeSpan.FromSeconds(attempt * 2), cancellationToken);
                            continue;
                        }

                        if (result == null)
                        {
                            MarkNotaVerificationFailed(row, "Nota verification webhook returned an empty response.");
                            return;
                        }

                        row.ScanStatus = "SUCCESS";
                        row.VerificationStatus = result.Status;
                        var invoiceDate = ParseNotaDate(result.InvoiceDate);
                        row.InvoiceDate = invoiceDate;
                        row.VendorName = result.VendorName;
                        row.AgeInDays = result.AgeInDays;
                        row.OverdueDays = result.OverdueDays;
                        row.RawResponse = body;
                        row.ErrorMessage = invoiceDate == null
                            ? "Invoice date was not detected by AI."
                            : null;
                        row.NextRetryAt = null;
                        row.UpdatedAt = DateTime.UtcNow;
                        return;
                    }

                    lastError = $"Nota verification webhook failed with HTTP {(int)response.StatusCode}: {body}";
                    if (!IsTransientStatusCode(response.StatusCode) || attempt == maxAttempts)
                        break;
                }
                catch (Exception ex) when (attempt < maxAttempts)
                {
                    lastError = ex.Message;
                }

                await Task.Delay(TimeSpan.FromSeconds(attempt * 2), cancellationToken);
            }

            MarkNotaVerificationFailed(row, lastError ?? "Nota verification webhook failed.");
        }

        private static byte[] DecodeAttachmentBase64(Attachment attachment)
        {
            if (string.IsNullOrWhiteSpace(attachment.Base64))
                throw new InvalidOperationException("Attachment Base64 is empty.");

            var base64 = attachment.Base64.Trim();
            var commaIndex = base64.IndexOf(',');
            if (commaIndex >= 0)
                base64 = base64[(commaIndex + 1)..];

            return Convert.FromBase64String(base64);
        }

        private static string ResolveAttachmentContentType(Attachment attachment)
        {
            if (!string.IsNullOrWhiteSpace(attachment.MimeType))
                return attachment.MimeType;

            var extension = Path.GetExtension(attachment.FileName ?? string.Empty).ToLowerInvariant();
            return extension switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".webp" => "image/webp",
                ".pdf" => "application/pdf",
                _ => "application/octet-stream"
            };
        }

        private static bool IsTransientStatusCode(HttpStatusCode statusCode)
        {
            return statusCode == HttpStatusCode.RequestTimeout ||
                   statusCode == (HttpStatusCode)429 ||
                   (int)statusCode >= 500;
        }

        private static DateTime? ParseNotaDate(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            return DateTime.TryParse(value, out var parsed)
                ? parsed
                : null;
        }

        private static void MarkNotaVerificationFailed(RequisitionNotaVerification row, string errorMessage)
        {
            row.ScanStatus = "FAILED";
            row.VerificationStatus = null;
            row.InvoiceDate = null;
            row.VendorName = null;
            row.AgeInDays = null;
            row.OverdueDays = null;
            row.RawResponse = null;
            row.ErrorMessage = errorMessage;
            row.NextRetryAt = null;
            row.UpdatedAt = DateTime.UtcNow;
        }

        private static string TrimTelegramText(string? value, int maxLength)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;

            return value.Length <= maxLength
                ? value
                : value[..maxLength] + "...";
        }

        private sealed class NotaVerificationWebhookResponse
        {
            [JsonPropertyName("status")]
            public string? Status { get; set; }

            [JsonPropertyName("tanggal_nota")]
            public string? InvoiceDate { get; set; }

            [JsonPropertyName("nama_vendor")]
            public string? VendorName { get; set; }

            [JsonPropertyName("usia_hari")]
            public int? AgeInDays { get; set; }

            [JsonPropertyName("hari_terlambat")]
            public int? OverdueDays { get; set; }
        }

    }
}
