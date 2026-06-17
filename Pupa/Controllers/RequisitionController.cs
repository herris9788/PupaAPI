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

        private readonly IConfiguration _configuration;
        private readonly BeesuiteDbContext _db;
        public RequisitionController(BeesuiteDbContext db)
        {
            _db = db;
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

            var freonItemIds = await _db.Item
                .AsNoTracking()
                .Where(x => itemIds.Contains(x.ItemID) && x.ItemCode != null && x.ItemCode.StartsWith(FreonItemCodePrefix))
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

    }
}
