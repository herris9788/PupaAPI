using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pupa.BusinessObjects.Beesuite;
using Pupa.BusinessObjects;
using System.Collections.ObjectModel;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Pupa.Controllers
{
    // Creates a JobRequest with its whole nested graph (Jobs -> JobDetails,
    // Jobs -> Attachments [JobAttachment, per-item], and the JobRequest's own
    // Attachments [JobRequestAttachment, document-level]) in one call — the
    // Job Request equivalent of RequisitionController.CreateRequisition,
    // replacing the external Go backend's POST /api/v5/job-request. Every
    // nested navigation collection here (JobRequest.Jobs, Job.JobDetails,
    // JobRequest.Attachments, Job.Attachments) is already wired via
    // [ForeignKey] attributes on the child entities with no Fluent API
    // overrides, so a single _db.AddAsync(Body) + SaveChangesAsync() lets EF
    // Core cascade-insert the entire graph in one transaction, exactly like
    // RequisitionController does for Requisition -> RequisitionDetails.
    [Route("beesuite/api/[controller]")]
    public class JobRequestController : Controller
    {
        private readonly BeesuiteDbContext _db;

        // Same options the app-wide MVC binder already uses for every other
        // [FromBody] entity (Startup.cs AddJsonOptions) — needed here because
        // this action reads the body as JsonElement first (to also pull out
        // COA/OtherPurpose, which aren't real JobRequest columns) instead of
        // letting MVC bind [FromBody] JobRequest directly.
        private static readonly JsonSerializerOptions JobRequestJsonOptions = new()
        {
            ReferenceHandler = ReferenceHandler.IgnoreCycles,
            MaxDepth = 100,
            PropertyNamingPolicy = null,
            IncludeFields = true,
        };

        public JobRequestController(BeesuiteDbContext db)
        {
            _db = db;
        }

        [HttpPost("Create")]
        public async Task<IActionResult> CreateJobRequest([FromBody] JsonElement RawBody)
        {
            try
            {
                JobRequest? Body;
                try
                {
                    Body = JsonSerializer.Deserialize<JobRequest>(RawBody.GetRawText(), JobRequestJsonOptions);
                }
                catch (JsonException ex)
                {
                    return BadRequest($"Invalid request body: {ex.Message}");
                }
                if (Body == null)
                    return BadRequest("Request body cannot be null.");
                if (Body.VesselID == null)
                    return BadRequest("VesselID is required.");

                // COA/OtherPurpose aren't real JobRequest columns — every
                // other reader in the app (VesselApproval.dart,
                // ApprovalDetail.dart, ReviewApproveOrderPageWeb.dart) reads
                // them back via JobFieldValue rows keyed by
                // (EntityType='JobRequest', EntityID=<this JobRequest's ID>),
                // so persist them the same way once the ID is known below.
                string? Coa = RawBody.TryGetProperty("COA", out var coaEl) && coaEl.ValueKind == JsonValueKind.String
                    ? coaEl.GetString() : null;
                string? OtherPurpose = RawBody.TryGetProperty("OtherPurpose", out var opEl) && opEl.ValueKind == JsonValueKind.String
                    ? opEl.GetString() : null;

                var vessel = await _db.InventoryUser.FirstOrDefaultAsync(x => x.ID == Body.VesselID);
                if (vessel == null)
                    return NotFound($"Vessel with ID {Body.VesselID} not found.");

                // Same <VC><YY><MM><N4> template substitution as
                // RequisitionController.CreateRequisition's ReportNo
                // generation — the "JobRequest" DocumentNumbering.Type row
                // already exists per vessel (format "JR<VC><YY><MM><N4>").
                var docFormat = await _db.DocumentNumbering.FirstOrDefaultAsync(
                    x => x.Vessel == vessel.InventoryUserName && x.Type == "JobRequest");
                if (docFormat == null)
                    return NotFound($"JobRequest numbering format for vessel '{vessel.InventoryUserName}' not found.");

                var now = DateTime.Now;
                var yearStr = now.ToString("yy");
                var monthStr = now.ToString("MM");
                var prefix = docFormat.Format
                    .Replace("<VC>", docFormat.VesselCode)
                    .Replace("<YY>", yearStr)
                    .Replace("<MM>", monthStr)
                    .Replace("<N4>", "");
                var lastNumber = await _db.JobRequest
                    .Where(x => x.ReportNo.StartsWith(prefix) &&
                                x.CreatedAt.Year == now.Year &&
                                x.CreatedAt.Month == now.Month)
                    .OrderByDescending(x => x.ReportNo)
                    .Select(x => x.ReportNo)
                    .FirstOrDefaultAsync();
                int nextNumber = 1;
                if (lastNumber != null)
                {
                    var lastSeq = lastNumber.Substring(prefix.Length);
                    if (int.TryParse(lastSeq, out int parsed))
                        nextNumber = parsed + 1;
                }
                Body.ReportNo = docFormat.Format
                    .Replace("<VC>", docFormat.VesselCode)
                    .Replace("<YY>", yearStr)
                    .Replace("<MM>", monthStr)
                    .Replace("<N4>", nextNumber.ToString("D4"));

                Body.CreatedAt = now;
                Body.UpdatedAt = now;
                Body.Status = "Submitted";
                Body.ApprovalStatus = "Pending";

                await _db.AddAsync(Body);
                await _db.SaveChangesAsync();

                if (!string.IsNullOrWhiteSpace(Coa) || !string.IsNullOrWhiteSpace(OtherPurpose))
                {
                    var fieldValues = new List<JobFieldValue>();
                    if (!string.IsNullOrWhiteSpace(Coa))
                        fieldValues.Add(new JobFieldValue { EntityType = "JobRequest", EntityID = Body.ID, FieldKey = "COA", ValueType = "text", ValueText = Coa });
                    if (!string.IsNullOrWhiteSpace(OtherPurpose))
                        fieldValues.Add(new JobFieldValue { EntityType = "JobRequest", EntityID = Body.ID, FieldKey = "OtherPurpose", ValueType = "text", ValueText = OtherPurpose });
                    await _db.AddRangeAsync(fieldValues);
                    await _db.SaveChangesAsync();
                }

                // Trim the nested graph before returning, matching
                // CreateRequisition's own response shape (avoids echoing the
                // whole just-inserted graph back, including now-stale
                // in-memory nav collections).
                Body.Jobs = new ObservableCollection<Job>();
                Body.Attachments = new ObservableCollection<JobRequestAttachment>();
                return Ok(Body);
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, new
                {
                    Message = "Database error occurred while saving job request.",
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
    }
}
