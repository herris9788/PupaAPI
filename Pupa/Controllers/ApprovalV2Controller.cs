using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pupa.BusinessObjects;

namespace Pupa.Controllers
{
    /// <summary>
    /// Approval Rule V2 prep -- plain (non-OData) test/reference endpoint for
    /// the two-tier resolution logic: MandatoryVessel/MandatoryVesselApprover
    /// first (levels 1..cutoff, specific named approvers), falling through to
    /// ItemGroupMapping/UserApprovalGroup (Group-based, generic) for levels
    /// past the cutoff or vessels not marked mandatory.
    ///
    /// This does NOT touch or get called by any V1 approval flow -- it exists
    /// purely so the resolution logic can be exercised and verified against
    /// real data while the V2 rule tables above are still being populated.
    /// </summary>
    [Route("beesuite/api/ApprovalV2")]
    public class ApprovalV2Controller : Controller
    {
        private readonly BeesuiteDbContext db;
        public ApprovalV2Controller(BeesuiteDbContext db) { this.db = db; }

        [HttpGet("ResolveApprover")]
        public async Task<IActionResult> ResolveApprover(
            [FromQuery] int vesselId,
            [FromQuery] string companyDb,
            [FromQuery] int level,
            [FromQuery] int stockCategoryId,
            [FromQuery] int stockFamilyId = -1)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(companyDb))
                    throw new Exception("companyDb is required");

                var mandatory = await db.MandatoryVessel
                    .AsNoTracking()
                    .FirstOrDefaultAsync(m => m.VesselID == vesselId && m.CompanyDB == companyDb);

                if (mandatory != null && level <= mandatory.MandatoryLevelCutoff)
                {
                    var mandatoryApprovers = await db.MandatoryVesselApprover
                        .AsNoTracking()
                        .Where(a => a.VesselID == vesselId && a.CompanyDB == companyDb && a.Level == level)
                        .Select(a => a.Username)
                        .Distinct()
                        .ToListAsync();

                    return Ok(new
                    {
                        Success = true,
                        Message = "OK",
                        Data = new
                        {
                            Source = "MandatoryVesselApprover",
                            MandatoryLevelCutoff = mandatory.MandatoryLevelCutoff,
                            Approvers = mandatoryApprovers
                        }
                    });
                }

                // Group-based fallback: resolve the ItemGroupMapping Group(s)
                // for this category/family (StockFamilyID == -1 means "any
                // family under this category" -- same wildcard convention as
                // UserApprovalScope).
                var groupIds = await db.ItemGroupMapping
                    .AsNoTracking()
                    .Where(g => g.StockCategoryID == stockCategoryId
                                && (g.FamilyID == stockFamilyId || stockFamilyId == -1))
                    .Select(g => g.GroupID)
                    .Distinct()
                    .ToListAsync();

                if (!groupIds.Any())
                {
                    return Ok(new
                    {
                        Success = true,
                        Message = "No ItemGroupMapping match for this Category/Family -- Group-based fallback has nothing to resolve.",
                        Data = new { Source = "UserApprovalGroup", Groups = Array.Empty<string>(), Approvers = Array.Empty<string>() }
                    });
                }

                var groupNames = await db.ApprovalGroup
                    .AsNoTracking()
                    .Where(g => groupIds.Contains(g.ID))
                    .Select(g => g.GroupName)
                    .ToListAsync();

                var groupApprovers = await db.UserApprovalGroup
                    .AsNoTracking()
                    .Where(u => groupIds.Contains(u.GroupID))
                    .Select(u => u.Username)
                    .Distinct()
                    .ToListAsync();

                return Ok(new
                {
                    Success = true,
                    Message = "OK",
                    Data = new
                    {
                        Source = "UserApprovalGroup",
                        Groups = groupNames,
                        Approvers = groupApprovers
                    }
                });
            }
            catch (Exception Ex)
            {
                return BadRequest(new { Success = false, Message = Ex.Message, Data = (object?)null });
            }
        }
    }
}
