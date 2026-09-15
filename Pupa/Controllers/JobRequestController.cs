using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pupa.BusinessObjects.Beesuite;
using Pupa.BusinessObjects;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Pupa.Controllers
{
    // Creates a JobRequest with its nested Jobs, each job's own Attachments
    // [JobAttachment, per-item], and the JobRequest's own Attachments
    // [JobRequestAttachment, document-level] in one call — the Job Request
    // equivalent of RequisitionController.CreateRequisition, replacing the
    // external Go backend's POST /api/v5/job-request. JobRequest.Jobs,
    // JobRequest.Attachments, and Job.Attachments all use a normal separate-
    // identity-PK + FK-column shape (same as Requisition.RequisitionDetails,
    // the already-proven cascade pattern), so a single _db.AddAsync(Body) +
    // SaveChangesAsync() lets EF Core cascade-insert them together in one
    // transaction. Job.JobDetails is handled separately (see below).
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

                // JobRequest.VesselID has always stored InventoryUser.
                // InventoryUserID (the business id), NOT InventoryUser.ID
                // (the surrogate PK) — confirmed against every real
                // JobRequest row created via the old Go backend (e.g.
                // JobRequest.VesselID=151 -> InventoryUserID=151 resolves to
                // MV. AMETHYST; InventoryUser.ID=151 is an unrelated row).
                // Matching by ID here previously resolved to the wrong
                // vessel entirely. InventoryUserID alone isn't unique either
                // (live data has several unrelated vessels sharing the same
                // InventoryUserID), so also match VesselName — the client
                // already sends both from the same Vessel object.
                var vessel = await _db.InventoryUser.FirstOrDefaultAsync(
                    x => x.InventoryUserID == Body.VesselID && x.InventoryUserName == Body.VesselName);
                if (vessel == null)
                    return NotFound($"Vessel with ID {Body.VesselID} not found.");

                // VesselInventoryUserRowID/VesselInventoryUserDB (the real
                // InventoryUser.ID/DB, despite the "RowID" name) are what
                // every listing/tracking page (Track Item, Approval pages,
                // PendingApprovalsHelper, VesselApproval.dart, mobile
                // ApprovalDetail's own vessel resolution) actually matches
                // Job Requests to their vessel by — NOT VesselID (the
                // business id). Left unset, a new JobRequest silently never
                // shows up anywhere despite existing in the table.
                Body.VesselInventoryUserRowID = vessel.ID;
                Body.VesselInventoryUserDB = vessel.DB;

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
                Body.Approved = false;

                // Each Job gets its own real ServiceOrderNo (format
                // "SO<VC><YY><MM><N4>", same DocumentNumbering.Type=
                // "ServiceOrder" row every vessel already has) instead of
                // the client's literal "AUTO" placeholder — mirrors
                // ReportNo's own generation above. Numbered sequentially
                // against the highest existing Job.ServiceOrderNo with this
                // prefix, then bumped per job within this same request too
                // (a submission can create several jobs at once).
                var soFormat = await _db.DocumentNumbering.FirstOrDefaultAsync(
                    x => x.Vessel == vessel.InventoryUserName && x.Type == "ServiceOrder");
                if (soFormat != null)
                {
                    var soPrefix = soFormat.Format
                        .Replace("<VC>", soFormat.VesselCode)
                        .Replace("<YY>", yearStr)
                        .Replace("<MM>", monthStr)
                        .Replace("<N4>", "");
                    var lastSo = await _db.Job
                        .Where(x => x.ServiceOrderNo != null && x.ServiceOrderNo.StartsWith(soPrefix) &&
                                    x.CreatedAt.Year == now.Year && x.CreatedAt.Month == now.Month)
                        .OrderByDescending(x => x.ServiceOrderNo)
                        .Select(x => x.ServiceOrderNo)
                        .FirstOrDefaultAsync();
                    int nextSoNumber = 1;
                    if (lastSo != null)
                    {
                        var lastSoSeq = lastSo!.Substring(soPrefix.Length);
                        if (int.TryParse(lastSoSeq, out int parsedSo))
                            nextSoNumber = parsedSo + 1;
                    }
                    foreach (var job in Body.Jobs)
                    {
                        job.ServiceOrderNo = soFormat.Format
                            .Replace("<VC>", soFormat.VesselCode)
                            .Replace("<YY>", yearStr)
                            .Replace("<MM>", monthStr)
                            .Replace("<N4>", nextSoNumber.ToString("D4"));
                        nextSoNumber++;
                        job.CreatedBy = Body.CreatedBy;
                        job.CreatedAt = now;
                        job.UpdatedAt = now;
                    }
                }

                // StorageProvider/PreviewUrl are NOT NULL columns on both
                // JobRequestAttachment and JobAttachment (legacy rows always
                // had a real object-storage location here) — a client
                // sending only Base64 (self-contained, no external storage)
                // still needs a placeholder for these or the insert violates
                // that constraint.
                foreach (var att in Body.Attachments)
                {
                    att.StorageProvider ??= "database";
                    att.PreviewUrl ??= "";
                }
                foreach (var job in Body.Jobs)
                {
                    foreach (var att in job.Attachments ?? Enumerable.Empty<JobAttachment>())
                    {
                        att.StorageProvider ??= "database";
                        att.PreviewUrl ??= "";
                    }
                }

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

                // Job.JobDetails is deliberately NOT part of this cascade —
                // JobDetail's PK is also its FK to Job (shared primary key,
                // DatabaseGeneratedOption.None), an association shape no
                // other cascade-insert in this codebase exercises yet. Safer
                // to create each JobDetail as its own explicit, already-
                // proven POST /odata/JobDetail call once this response hands
                // back each Job's real (now-generated) ID — see
                // JobRequestApi.submitFullRequest's second phase.
                //
                // Trim the nested graph before returning (mirrors
                // CreateRequisition not echoing RequisitionDetails back) —
                // but keep each Job's ID/SequenceNo so the client can match
                // its own per-job data back to the right Job for that
                // JobDetail follow-up call.
                foreach (var job in Body.Jobs)
                {
                    job.Attachments = new ObservableCollection<JobAttachment>();
                    job.JobDetails = new ObservableCollection<JobDetail>();
                }
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
