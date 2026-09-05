using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pupa.BusinessObjects;
using Pupa.BusinessObjects.Beesuite;
using Pupa.ViewModels;
using System.Data;
using System.Linq;


namespace Pupa.Controllers
{

    [Route("beesuite/api/[controller]")]
    public class UserController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly BeesuiteDbContext db;
        public UserController(BeesuiteDbContext db)
        {
            this.db = db;
        }
        [HttpGet("Approver")]
        public async Task<IActionResult> CheckApprover([FromQuery] FindApproverDTO Query)
        {
            try
            {
                Requisition Requisition = null;
                if (Query.RequisitionNumber != null)
                {
                    Requisition = await db.Requisition.FirstOrDefaultAsync(x => x.RequisitionNumber == Query.RequisitionNumber);
                }
                else if (Query.RequisitionID != null)
                {
                    Requisition = await db.Requisition.FirstOrDefaultAsync(x => x.ID == Query.RequisitionID);
                }

                if (Requisition == null) throw new Exception("Requisition not found");

                // Family bisa null kalau CategoryID tidak punya mapping StockFamily.
                // Tidak throw lagi — nanti rule yang butuh StockFamily/StockCategory akan di-skip.
                var Family = db.StockFamily.FirstOrDefault(x => x.FamilyID == Requisition.CategoryID);

                // Null kalau Family tidak ditemukan — dipakai untuk kondisi rule-rule di bawah
                int? FamilyStockCategoryID = Family?.StockCategoryID;
                int? FamilyFamilyID = Family?.FamilyID;

                var Vessel = await db.InventoryUser.AsNoTracking()
                    .Include(x => x.Group)
                    .FirstOrDefaultAsync(x => x.ID == Requisition.VesselID);

                if (Vessel?.Group == null) throw new Exception("Vessel or vessel group not found");

                var VesselGroupId = Vessel.Group.ID;

                var Scopes = await db.UserApprovalScope.AsNoTracking()
                    .Include(x => x.User)
                    .Where(x => x.VesselGroupID == VesselGroupId)
                    .ToListAsync();

                // This document's OWN snapshot decides the rule engine — not the
                // vessel's current flag, which may have changed since submission.
                // Only loaded when actually needed (v1 stays untouched otherwise).
                List<UserApprovalScope2> ScopesV2 = new();
                if (Requisition.ApprovalRuleVersion == 2)
                {
                    ScopesV2 = await db.UserApprovalScope2.AsNoTracking()
                        .Include(x => x.User)
                        .Where(x => x.IsActive != false)
                        .ToListAsync();
                }

                string? GetActualApproverName(int level)
                {
                    return level switch
                    {
                        1 => Requisition.ApprovedBy1,
                        2 => Requisition.ApprovedBy2,
                        3 => Requisition.ApprovedBy3,
                        4 => Requisition.ApprovedBy4,
                        5 => Requisition.ApprovedBy5,
                        6 => Requisition.ApprovedBy6,
                        7 => Requisition.ApprovedBy7,
                        _ => null
                    };
                }

                // v2 match: every dimension is a wildcard when NULL. When the
                // document has a Group (Item Request V2 combined submission),
                // ANY row carrying that Group label is eligible to be the
                // approver — the same row an admin uses to declare "this
                // Category/Family belongs to Group X at Level N, approved by
                // Y" doubles as both the membership declaration AND the real
                // approval-chain entry (confirmed against real usage: admins
                // don't create a separate parallel set of "pure" Group rows).
                // Category/Family on the row are simply ignored in this mode
                // — they were never meant to further restrict who approves
                // the combined document, only to (elsewhere, client-side)
                // determine which items belong to the Group in the first
                // place. Otherwise Group is ignored, exactly as before — it
                // has no bearing on routing for a normal (non-combined)
                // document.
                // Group-combined documents: admins number a Group's approval rows
                // however makes sense to them (e.g. only Level 2, no Level 1) — the
                // DB Level value is NOT a required-contiguous-from-1 position, it's
                // just a tie-breaker/ordering key. Resolve the full chain ONCE by
                // scanning every possible Level and compacting (skipping gaps),
                // exactly like ResolveApprovers already does at submit time, then
                // index into it POSITIONALLY below instead of re-querying by a
                // literal Level number that may not exist in the matrix at all.
                // Each position is a LIST — every candidate tied at that
                // position's winning tier, not just one (see
                // ResolveScopeCandidatesV2 above).
                List<List<UserApprovalScope2>> GroupChain = new();
                if (Requisition.ApprovalRuleVersion == 2 && !string.IsNullOrEmpty(Requisition.Group))
                {
                    for (int lvl = 1; lvl <= 7; lvl++)
                    {
                        var candidates = ResolveScopeCandidatesV2(ScopesV2, Vessel, null, null, lvl, Requisition.Group, Requisition.Department, Requisition.SubDepartment);
                        if (candidates.Count > 0) GroupChain.Add(candidates);
                    }
                }

                var ApprovalMatrix = new List<object>();
                var ApproverCount = Requisition.ApprovalMaxLevel;

                // Self-heal: a null ApprovalMaxLevel means it was never (re)computed
                // for this document — most commonly because the vessel's approval
                // matrix (e.g. its "All Groups" rows) was set up or changed AFTER
                // this document was already submitted, so the snapshot taken at
                // submit time came back empty/zero and stuck that way. Recompute it
                // live from the CURRENT matrix and persist it, so the document
                // stops showing a blank approval chain here — and, since
                // PendingApproval's own query filters out any Requisition whose
                // stored ApprovalMaxLevel isn't > 0, stops being silently excluded
                // from the approver's Pending Approvals list too.
                if (ApproverCount == null && Requisition.ApprovalRuleVersion == 2)
                {
                    int LiveCount = 0;
                    if (!string.IsNullOrEmpty(Requisition.Group))
                    {
                        LiveCount = GroupChain.Count;
                    }
                    else
                    {
                        for (int lvl = 1; lvl <= 7; lvl++)
                        {
                            if (ResolveScopeCandidatesV2(ScopesV2, Vessel, FamilyStockCategoryID, FamilyFamilyID, lvl, Requisition.Group, Requisition.Department, Requisition.SubDepartment).Count > 0)
                                LiveCount = lvl;
                        }
                    }
                    if (LiveCount > 0)
                    {
                        Requisition.ApprovalMaxLevel = LiveCount;
                        await db.SaveChangesAsync();
                        ApproverCount = LiveCount;
                    }
                }

                for (int i = 0; i < ApproverCount; i++)
                {
                    var Level = i + 1;
                    List<int> ResolvedUserIds;
                    List<string> ResolvedUsernames;
                    object? MatchedSummary;

                    if (Requisition.ApprovalRuleVersion == 2)
                    {
                        var CandidatesV2 = (!string.IsNullOrEmpty(Requisition.Group)
                            ? (GroupChain.ElementAtOrDefault(i) ?? new List<UserApprovalScope2>())
                            : ResolveScopeCandidatesV2(ScopesV2, Vessel, FamilyStockCategoryID, FamilyFamilyID, Level, Requisition.Group, Requisition.Department, Requisition.SubDepartment))
                            .OrderBy(s => s.ID).ToList();
                        ResolvedUserIds = CandidatesV2.Where(s => s.UserID != null).Select(s => s.UserID!.Value).ToList();
                        ResolvedUsernames = CandidatesV2.Where(s => s.User?.Username != null).Select(s => s.User!.Username!).ToList();
                        var FirstV2 = CandidatesV2.FirstOrDefault();
                        MatchedSummary = FirstV2 == null ? null : new
                        {
                            FirstV2.ID,
                            FirstV2.VesselID,
                            FirstV2.VesselGroupID,
                            FirstV2.CompanyDB,
                            FirstV2.Group,
                            FirstV2.StockCategoryID,
                            FirstV2.StockFamilyID,
                            FirstV2.Department,
                            FirstV2.SubDepartment,
                        };
                    }
                    else
                    {
                        // Rule [1]-[7], [10]-[16] only run when a Family was found —
                        // see ResolveScopeCandidatesV1 for the full tier order/
                        // rationale (verbatim transcription of the historical
                        // cascade this replaces, now returning every tied candidate
                        // at the winning tier instead of just one).
                        var CandidatesV1 = ResolveScopeCandidatesV1(Scopes, Level, Vessel.ID, Family != null, FamilyStockCategoryID, FamilyFamilyID, Requisition.Department, Requisition.SubDepartment)
                            .OrderBy(s => s.ID).ToList();
                        ResolvedUserIds = CandidatesV1.Where(s => s.UserID != null).Select(s => s.UserID!.Value).ToList();
                        ResolvedUsernames = CandidatesV1.Where(s => s.User?.Username != null).Select(s => s.User!.Username!).ToList();
                        var FirstV1 = CandidatesV1.FirstOrDefault();
                        MatchedSummary = FirstV1 == null ? null : new
                        {
                            FirstV1.ID,
                            FirstV1.InventoryUserID,
                            FirstV1.StockCategoryID,
                            FirstV1.StockFamilyID,
                            FirstV1.Department,
                            FirstV1.SubDepartment,
                        };
                    }

                    var ActualApprover = GetActualApproverName(Level);

                    ApprovalMatrix.Add(new
                    {
                        Level,
                        Found = ResolvedUserIds.Count > 0,
                        // Singular fields kept for back-compat (existing consumers,
                        // including the React Webview, read these) — first candidate
                        // by ID. New plural fields carry every tied approver.
                        ShouldBeApproverUserID = ResolvedUserIds.Count > 0 ? (int?)ResolvedUserIds[0] : null,
                        ShouldBeApproverUsername = ResolvedUsernames.Count > 0 ? ResolvedUsernames[0] : null,
                        ShouldBeApproverUserIDs = ResolvedUserIds,
                        ShouldBeApproverUsernames = ResolvedUsernames,
                        ActualApprovedBy = ActualApprover,
                        IsActuallyApproved = !string.IsNullOrWhiteSpace(ActualApprover),
                        MatchedScope = MatchedSummary
                    });
                }

                return Ok(new
                {
                    Success = true,
                    Message = "OK",
                    Data = new
                    {
                        Items = ApprovalMatrix,
                        TotalCount = ApprovalMatrix.Count,
                        Offset = 0,
                        Limit = ApprovalMatrix.Count
                    }
                });
            }
            catch (Exception Ex)
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = Ex.Message,
                    Data = (object?)null
                });
            }
        }

        // ------------------------------------------------------------------
        // Shared candidate resolvers — multiple approvers per level.
        //
        // Every action below (CheckApprover, ResolveApprovers,
        // PendingApproval) used to duplicate its own copy of the same tiered
        // v1 cascade / v2 Specificity match, each picking exactly ONE row
        // via ??=/.FirstOrDefault(). If an admin configures two different
        // UserIDs at the identical scope+Level (nothing in the schema stops
        // this), only one was ever honored — the other user never saw the
        // document as pending and was never notified, even though either of
        // them should be able to approve it. These two helpers return EVERY
        // row tied at the winning (most specific) tier instead of just the
        // first — normally 1 row, >1 only when that's genuinely configured.
        // Callers treat the returned list as "any one of these approving is
        // enough"; the actual approval write (ApprovedBy{N}) still only ever
        // records whichever single user's client sent the request.
        // ------------------------------------------------------------------

        // De-duplicates by UserID (keeping the first occurrence — callers
        // already .OrderBy(ID) beforehand where it matters), so a user with
        // two overlapping scope rows at the same winning tier (e.g. a stale
        // duplicate row from re-editing the Approval Rules admin UI) doesn't
        // show up twice in the same level's approver list.
        private List<T> DedupeByUserId<T>(List<T> rows, Func<T, int?> getUserId)
        {
            var seen = new HashSet<int>();
            var result = new List<T>();
            foreach (var r in rows)
            {
                var id = getUserId(r);
                if (id == null || seen.Add(id.Value)) result.Add(r);
            }
            return result;
        }

        // v1 (UserApprovalScope) — tier order and predicates are a verbatim
        // transcription of the historical ??= cascade (tiers [1]-[18], with
        // [Nb] SubDepartment-only variants), just returning every matching
        // row at the first tier that has any, instead of the first row.
        private List<UserApprovalScope> ResolveScopeCandidatesV1(
            IEnumerable<UserApprovalScope> ScopesInGroup, int Level, int? VesselId,
            bool HasFamily, int? FamilyStockCategoryID, int? FamilyFamilyID,
            string? Department, string? SubDepartment)
        {
            return DedupeByUserId(
                ResolveScopeCandidatesV1Raw(ScopesInGroup, Level, VesselId, HasFamily, FamilyStockCategoryID, FamilyFamilyID, Department, SubDepartment),
                s => s.UserID);
        }

        private List<UserApprovalScope> ResolveScopeCandidatesV1Raw(
            IEnumerable<UserApprovalScope> ScopesInGroup, int Level, int? VesselId,
            bool HasFamily, int? FamilyStockCategoryID, int? FamilyFamilyID,
            string? Department, string? SubDepartment)
        {
            List<UserApprovalScope> Tier(Func<UserApprovalScope, bool> pred) =>
                ScopesInGroup.Where(x => x.Level == Level && pred(x)).ToList();
            List<UserApprovalScope> hit;

            if (HasFamily)
            {
                // [1][2][3][2b]
                hit = Tier(x => x.InventoryUserID == VesselId && x.StockCategoryID == FamilyStockCategoryID && x.StockFamilyID == FamilyFamilyID && x.Department == Department && x.SubDepartment == SubDepartment); if (hit.Count > 0) return hit;
                hit = Tier(x => x.InventoryUserID == VesselId && x.StockCategoryID == FamilyStockCategoryID && x.StockFamilyID == FamilyFamilyID && x.Department == Department && x.SubDepartment == null); if (hit.Count > 0) return hit;
                hit = Tier(x => x.InventoryUserID == VesselId && x.StockCategoryID == FamilyStockCategoryID && x.StockFamilyID == FamilyFamilyID && x.Department == null && x.SubDepartment == null); if (hit.Count > 0) return hit;
                hit = Tier(x => x.InventoryUserID == VesselId && x.StockCategoryID == FamilyStockCategoryID && x.StockFamilyID == FamilyFamilyID && x.Department == null && x.SubDepartment == SubDepartment); if (hit.Count > 0) return hit;
                // [4][5][4b]
                hit = Tier(x => x.InventoryUserID == VesselId && x.StockCategoryID == FamilyStockCategoryID && x.StockFamilyID == -1 && x.Department == Department && x.SubDepartment == null); if (hit.Count > 0) return hit;
                hit = Tier(x => x.InventoryUserID == VesselId && x.StockCategoryID == FamilyStockCategoryID && x.StockFamilyID == -1 && x.Department == null && x.SubDepartment == null); if (hit.Count > 0) return hit;
                hit = Tier(x => x.InventoryUserID == VesselId && x.StockCategoryID == FamilyStockCategoryID && x.StockFamilyID == -1 && x.Department == null && x.SubDepartment == SubDepartment); if (hit.Count > 0) return hit;
                // [6][7][6b]
                hit = Tier(x => x.InventoryUserID == VesselId && x.StockCategoryID == -1 && x.StockFamilyID == FamilyFamilyID && x.Department == Department && x.SubDepartment == null); if (hit.Count > 0) return hit;
                hit = Tier(x => x.InventoryUserID == VesselId && x.StockCategoryID == -1 && x.StockFamilyID == FamilyFamilyID && x.Department == null && x.SubDepartment == null); if (hit.Count > 0) return hit;
                hit = Tier(x => x.InventoryUserID == VesselId && x.StockCategoryID == -1 && x.StockFamilyID == FamilyFamilyID && x.Department == null && x.SubDepartment == SubDepartment); if (hit.Count > 0) return hit;
            }

            // [8][8b][9]
            hit = Tier(x => x.InventoryUserID == VesselId && x.StockCategoryID == -1 && x.StockFamilyID == -1 && x.Department == Department && x.SubDepartment == null); if (hit.Count > 0) return hit;
            hit = Tier(x => x.InventoryUserID == VesselId && x.StockCategoryID == -1 && x.StockFamilyID == -1 && x.Department == null && x.SubDepartment == SubDepartment); if (hit.Count > 0) return hit;
            hit = Tier(x => x.InventoryUserID == VesselId && x.StockCategoryID == -1 && x.StockFamilyID == -1 && x.Department == null && x.SubDepartment == null); if (hit.Count > 0) return hit;

            if (HasFamily)
            {
                // [10][11][12][11b]
                hit = Tier(x => x.InventoryUserID == null && x.StockCategoryID == FamilyStockCategoryID && x.StockFamilyID == FamilyFamilyID && x.Department == Department && x.SubDepartment == SubDepartment); if (hit.Count > 0) return hit;
                hit = Tier(x => x.InventoryUserID == null && x.StockCategoryID == FamilyStockCategoryID && x.StockFamilyID == FamilyFamilyID && x.Department == Department && x.SubDepartment == null); if (hit.Count > 0) return hit;
                hit = Tier(x => x.InventoryUserID == null && x.StockCategoryID == FamilyStockCategoryID && x.StockFamilyID == FamilyFamilyID && x.Department == null && x.SubDepartment == null); if (hit.Count > 0) return hit;
                hit = Tier(x => x.InventoryUserID == null && x.StockCategoryID == FamilyStockCategoryID && x.StockFamilyID == FamilyFamilyID && x.Department == null && x.SubDepartment == SubDepartment); if (hit.Count > 0) return hit;
                // [13][14][13b]
                hit = Tier(x => x.InventoryUserID == null && x.StockCategoryID == FamilyStockCategoryID && x.StockFamilyID == -1 && x.Department == Department && x.SubDepartment == null); if (hit.Count > 0) return hit;
                hit = Tier(x => x.InventoryUserID == null && x.StockCategoryID == FamilyStockCategoryID && x.StockFamilyID == -1 && x.Department == null && x.SubDepartment == null); if (hit.Count > 0) return hit;
                hit = Tier(x => x.InventoryUserID == null && x.StockCategoryID == FamilyStockCategoryID && x.StockFamilyID == -1 && x.Department == null && x.SubDepartment == SubDepartment); if (hit.Count > 0) return hit;
                // [15][16][15b]
                hit = Tier(x => x.InventoryUserID == null && x.StockCategoryID == -1 && x.StockFamilyID == FamilyFamilyID && x.Department == Department && x.SubDepartment == null); if (hit.Count > 0) return hit;
                hit = Tier(x => x.InventoryUserID == null && x.StockCategoryID == -1 && x.StockFamilyID == FamilyFamilyID && x.Department == null && x.SubDepartment == null); if (hit.Count > 0) return hit;
                hit = Tier(x => x.InventoryUserID == null && x.StockCategoryID == -1 && x.StockFamilyID == FamilyFamilyID && x.Department == null && x.SubDepartment == SubDepartment); if (hit.Count > 0) return hit;
            }

            // [17][17b][18]
            hit = Tier(x => x.InventoryUserID == null && x.StockCategoryID == -1 && x.StockFamilyID == -1 && x.Department == Department && x.SubDepartment == null); if (hit.Count > 0) return hit;
            hit = Tier(x => x.InventoryUserID == null && x.StockCategoryID == -1 && x.StockFamilyID == -1 && x.Department == null && x.SubDepartment == SubDepartment); if (hit.Count > 0) return hit;
            hit = Tier(x => x.InventoryUserID == null && x.StockCategoryID == -1 && x.StockFamilyID == -1 && x.Department == null && x.SubDepartment == null); if (hit.Count > 0) return hit;

            return new List<UserApprovalScope>();
        }

        // v2 (UserApprovalScope2) — same MatchesV2/MatchesV2AllGroups
        // predicates as the historical single-row resolver, but returns
        // every row tied at the winning (max) Specificity instead of the
        // first by ID.
        private List<UserApprovalScope2> ResolveScopeCandidatesV2(
            IEnumerable<UserApprovalScope2> ScopesV2, InventoryUser Vessel, int? CatId, int? FamId,
            int Level, string? Group, string? Department, string? SubDepartment)
        {
            bool Matches(UserApprovalScope2 s)
            {
                if (s.IsActive == false) return false;
                if (s.Level != null && s.Level != Level) return false;
                if (s.VesselID != null && s.VesselID != Vessel.ID) return false;
                if (s.VesselGroupID != null && s.VesselGroupID != Vessel.Group?.ID) return false;
                if (s.CompanyDB != null && s.CompanyDB != Vessel.DB) return false;
                if (s.Department != null && s.Department != Department) return false;
                if (s.SubDepartment != null && s.SubDepartment != SubDepartment) return false;

                if (!string.IsNullOrEmpty(Group))
                {
                    return s.Group == Group;
                }

                if (s.StockCategoryID != null && s.StockCategoryID != CatId) return false;
                if (s.StockFamilyID != null && s.StockFamilyID != FamId) return false;
                return true;
            }

            bool MatchesAllGroups(UserApprovalScope2 s)
            {
                if (s.IsActive == false) return false;
                if (s.Group != null) return false;
                if (s.Level != null && s.Level != Level) return false;
                if (s.VesselID != null && s.VesselID != Vessel.ID) return false;
                if (s.VesselGroupID != null && s.VesselGroupID != Vessel.Group?.ID) return false;
                if (s.CompanyDB != null && s.CompanyDB != Vessel.DB) return false;
                if (s.Department != null && s.Department != Department) return false;
                if (s.SubDepartment != null && s.SubDepartment != SubDepartment) return false;
                return true;
            }

            var matched = ScopesV2.Where(Matches).ToList();
            if (matched.Count == 0 && !string.IsNullOrEmpty(Group))
            {
                matched = ScopesV2.Where(MatchesAllGroups).ToList();
            }
            if (matched.Count == 0) return matched;

            int maxSpecificity = matched.Max(x => x.Specificity);
            var tied = matched.Where(x => x.Specificity == maxSpecificity)
                .OrderBy(x => x.ID).ToList();
            return DedupeByUserId(tied, s => s.UserID);
        }

        // Pre-creation preview: what would the approver chain / ApprovalMaxLevel
        // be for a NOT-YET-CREATED requisition on this vessel? CheckApprover
        // above needs an existing Requisition row to read Department/CategoryID
        // /ApprovalRuleVersion from — the web/mobile "create IR" flows call this
        // instead, BEFORE the row exists, so ApprovalMaxLevel and the first
        // approver's notification are computed the same way (v1 cascade or v2
        // Specificity-based UserApprovalScope2, per Vessel.ApprovalRuleVersion —
        // there's no Requisition snapshot yet to read) as everywhere else,
        // rather than being recomputed client-side against UserApprovalScope
        // only. Returns one Items[] entry per level (1-7) that resolved to an
        // approver; ApprovalMaxLevel = Items.Count (a level with no match is
        // simply absent, matching how the client used to count matched rows).
        [HttpGet("ResolveApprovers")]
        public async Task<IActionResult> ResolveApprovers([FromQuery] ResolveApproversDTO Query)
        {
            try
            {
                if (Query.VesselID == null) throw new Exception("VesselID is required");

                var Vessel = await db.InventoryUser.AsNoTracking()
                    .Include(x => x.Group)
                    .FirstOrDefaultAsync(x => x.ID == Query.VesselID);

                if (Vessel?.Group == null) throw new Exception("Vessel or vessel group not found");

                var VesselGroupId = Vessel.Group.ID;

                var Family = Query.CategoryID != null
                    ? db.StockFamily.FirstOrDefault(x => x.FamilyID == Query.CategoryID)
                    : null;
                int? FamilyStockCategoryID = Family?.StockCategoryID;
                int? FamilyFamilyID = Family?.FamilyID;

                // Job Request: no numeric CategoryID (its categories, e.g. "BWMS
                // SAMPLING & ANALYSIS", are a plain-text label on JobDetail, not
                // tied to the StockFamily catalog) — resolve StockCategoryID by
                // name instead, so the SAME family→category-general→fully-general
                // cascade below applies. FamilyFamilyID stays null here on
                // purpose: no family means every family-scoped tier below simply
                // won't match, falling through to the category-wide ("StockFamilyID
                // == -1"/null-wildcard) tier — i.e. "match the family if one
                // exists, otherwise take the one assigned to all families".
                if (Family == null && !string.IsNullOrWhiteSpace(Query.Category))
                {
                    var CategoryNameTrimmed = Query.Category.Trim();
                    var MatchedCategory = db.StockCategory.FirstOrDefault(
                        x => x.StockCategoryName != null && x.StockCategoryName.ToLower() == CategoryNameTrimmed.ToLower());
                    FamilyStockCategoryID = MatchedCategory?.StockCategoryID;
                }
                // Whether there's ANY category context to scope by (a real Family
                // via CategoryID, or a Category name resolved to a StockCategory) —
                // gates the family/category-specific tiers in the V1 cascade below,
                // which used to check "Family != null" but that object is never
                // populated for the Category-string path above.
                var HasCategoryContext = FamilyStockCategoryID != null;

                var ResolveMatrix = new List<object>();

                if (Vessel.ApprovalRuleVersion == 2)
                {
                    var ScopesV2 = await db.UserApprovalScope2.AsNoTracking()
                        .Include(x => x.User)
                        .Where(x => x.IsActive != false)
                        .ToListAsync();

                    for (int Level = 1; Level <= 7; Level++)
                    {
                        var Candidates = ResolveScopeCandidatesV2(ScopesV2, Vessel, FamilyStockCategoryID, FamilyFamilyID, Level, Query.Group, Query.Department, Query.SubDepartment)
                            .Where(s => s.UserID != null)
                            .OrderBy(s => s.ID)
                            .ToList();
                        if (Candidates.Count == 0) continue;

                        var UserIds = Candidates.Select(s => s.UserID!.Value).ToList();
                        var Usernames = Candidates.Where(s => s.User?.Username != null).Select(s => s.User!.Username!).ToList();
                        var Phones = Candidates.Where(s => s.User?.Phone != null).Select(s => s.User!.Phone!).ToList();

                        ResolveMatrix.Add(new
                        {
                            Level,
                            // Singular fields kept for back-compat (existing
                            // consumers, including the React Webview, read these) —
                            // first candidate by ID. New plural fields carry every
                            // tied approver.
                            UserID = UserIds[0],
                            Username = Usernames.Count > 0 ? Usernames[0] : null,
                            Phone = Phones.Count > 0 ? Phones[0] : null,
                            UserIDs = UserIds,
                            Usernames,
                            Phones,
                        });
                    }
                }
                else
                {
                    var Scopes = await db.UserApprovalScope.AsNoTracking()
                        .Include(x => x.User)
                        .Where(x => x.VesselGroupID == VesselGroupId)
                        .ToListAsync();

                    for (int Level = 1; Level <= 7; Level++)
                    {
                        var Candidates = ResolveScopeCandidatesV1(Scopes, Level, Vessel.ID, HasCategoryContext, FamilyStockCategoryID, FamilyFamilyID, Query.Department, Query.SubDepartment)
                            .Where(s => s.UserID != null)
                            .OrderBy(s => s.ID)
                            .ToList();
                        if (Candidates.Count == 0) continue;

                        var UserIds = Candidates.Select(s => s.UserID!.Value).ToList();
                        var Usernames = Candidates.Where(s => s.User?.Username != null).Select(s => s.User!.Username!).ToList();
                        var Phones = Candidates.Where(s => s.User?.Phone != null).Select(s => s.User!.Phone!).ToList();

                        ResolveMatrix.Add(new
                        {
                            Level,
                            UserID = UserIds[0],
                            Username = Usernames.Count > 0 ? Usernames[0] : null,
                            Phone = Phones.Count > 0 ? Phones[0] : null,
                            UserIDs = UserIds,
                            Usernames,
                            Phones,
                        });
                    }
                }

                return Ok(new
                {
                    Success = true,
                    Message = "OK",
                    Data = new
                    {
                        Items = ResolveMatrix,
                        ApprovalMaxLevel = ResolveMatrix.Count,
                        ApprovalRuleVersion = Vessel.ApprovalRuleVersion ?? 1
                    }
                });
            }
            catch (Exception Ex)
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = Ex.Message,
                    Data = (object?)null
                });
            }
        }

        // Item Request (Requisition)-only Requester+Approver
        // Pending/Done/Cancel/Reverted tallies — shared by both
        // DashboardSummary (which adds Job Request numbers on top) and the
        // legacy DashboardItemCounts endpoint (kept for app builds still on
        // it — see DashboardItemCounts below). Pulling this into one helper
        // means both endpoints share the exact same bug fixes (e.g. the
        // IsAdminOverride/IsReverted exclusion in ApproverPending) instead of
        // silently drifting apart.
        private sealed class ItemDashboardCounts
        {
            public int ReqPending, ReqDone, ReqCancel;
            public int ApproverPending, ApproverDone, ApproverCancel, ApproverReverted;
        }

        private async Task<ItemDashboardCounts> ComputeItemDashboardCounts(User User)
        {
            var IsAdminUser = User.Role == "ADMIN";
            var UsernameLower = User.Username.ToLower();

            // ── Requester: documents THIS user created ───────────────────
            int ReqPending = await db.Requisition.CountAsync(x =>
                x.RequestBy == User.Username && x.Status == "PENDING" && x.ApprovedFromApp != true);
            int ReqDone = await db.Requisition.CountAsync(x =>
                x.RequestBy == User.Username
                && (x.Approved == true || (x.ApprovedBy7 != null && x.ApprovedBy7 != ""))
                && x.Status != "VOID" && x.Status != "REJECTED");
            int ReqCancel = await db.Requisition.CountAsync(x =>
                x.RequestBy == User.Username && (x.Status == "VOID" || x.Status == "REJECTED"));

            // ── Approver: documents awaiting/handled by THIS user ─────────
            int ApproverPending;
            {
                var ItemPendingResult = await PendingApproval(new PendingApproverDTO { UserName = User.Username }) as OkObjectResult;
                dynamic? Payload = ItemPendingResult?.Value;
                if (Payload != null && Payload.Success == true)
                {
                    // Payload.Data.TotalCount also counts two kinds of entries that
                    // don't belong in "Pending":
                    //  - IsAdminOverride: documents where this ADMIN isn't literally
                    //    the next required approver but can act anyway (oversight).
                    //  - IsReverted: a reverted document PendingApproval always keeps
                    //    visible (to any ADMIN, and to whoever reverted it) even while
                    //    Approved eq false — but it already has its own home in
                    //    ApproverReverted below, so counting it here too would show it
                    //    in both buckets at once.
                    // Every "pending for me" surface in the app (Track Item, the
                    // Pending Approvals detail sheet's own list, the Approvals page
                    // badges — all via PendingApprovalsHelper.fetchPendingItemsForUser)
                    // already excludes both, since neither is genuinely "awaiting my
                    // turn". Match that here so the Dashboard number (and this same
                    // value reused for the detail sheet's header) agrees with it.
                    int Count = 0;
                    foreach (dynamic Item in Payload.Data.Items)
                    {
                        if (Item.IsAdminOverride != true && Item.IsReverted != true) Count++;
                    }
                    ApproverPending = Count;
                }
                else
                {
                    ApproverPending = 0;
                }
            }

            int ApproverDone = await db.Requisition.CountAsync(x =>
                ((x.ApprovedBy1 != null && x.ApprovedBy1.ToLower() == UsernameLower) ||
                 (x.ApprovedBy2 != null && x.ApprovedBy2.ToLower() == UsernameLower) ||
                 (x.ApprovedBy3 != null && x.ApprovedBy3.ToLower() == UsernameLower) ||
                 (x.ApprovedBy4 != null && x.ApprovedBy4.ToLower() == UsernameLower) ||
                 (x.ApprovedBy5 != null && x.ApprovedBy5.ToLower() == UsernameLower) ||
                 (x.ApprovedBy6 != null && x.ApprovedBy6.ToLower() == UsernameLower) ||
                 (x.ApprovedBy7 != null && x.ApprovedBy7.ToLower() == UsernameLower))
                && (x.RevertStatus == null || x.RevertStatus.ToLower() != "reverted")
                && x.Status.ToLower() != "void" && x.Status.ToLower() != "rejected");

            int ApproverCancel = await db.Requisition.CountAsync(x => x.RejectedBy == User.Username);

            var VesselIdsInScope = await db.UserVesselRel.AsNoTracking()
                .Where(x => x.UserID == User.ID)
                .Select(x => x.VesselID)
                .Distinct()
                .ToListAsync();

            // ── Approver: Reverted (Item) — ADMIN sees every reverted
            // document on their authorized vessels where they're actually
            // one of the assigned approvers (any level); non-ADMIN sees
            // only the ones they personally reverted.
            int ApproverReverted = 0;
            {
                if (VesselIdsInScope.Any())
                {
                    var RevertedQuery = db.Requisition.AsNoTracking()
                        .Where(x => x.VesselID.HasValue && VesselIdsInScope.Contains(x.VesselID.Value)
                            && x.Approved == false
                            && x.Revised != true
                            && x.RevertStatus == "REVERTED"
                            && x.Status.ToLower() != "void" && x.Status.ToLower() != "rejected");

                    if (IsAdminUser)
                    {
                        var RevertedCandidates = await RevertedQuery
                            .Where(x => x.LastRevertedBy != null)
                            .Include(x => x.InventoryUser).ThenInclude(v => v!.Group)
                            .ToListAsync();

                        if (RevertedCandidates.Any())
                        {
                            var RelevantVesselGroupIds = RevertedCandidates
                                .Where(x => x.InventoryUser?.Group != null)
                                .Select(x => x.InventoryUser!.Group!.ID)
                                .Distinct()
                                .ToList();

                            var ScopesV1 = await db.UserApprovalScope.AsNoTracking()
                                .Include(x => x.User)
                                .Where(x => RelevantVesselGroupIds.Contains(x.VesselGroupID.Value))
                                .ToListAsync();
                            var ScopesV2 = await db.UserApprovalScope2.AsNoTracking()
                                .Include(x => x.User)
                                .Where(x => x.IsActive != false)
                                .ToListAsync();
                            var FamilyMap = await db.StockFamily.AsNoTracking().ToListAsync();

                            foreach (var r in RevertedCandidates)
                            {
                                // An ADMIN who personally reverted this document counts
                                // it as theirs even if they aren't formally scoped as an
                                // approver for this vessel-group/family (an admin override
                                // revert, not a normal scoped approval) — otherwise their
                                // own reverted actions silently never showed up on their
                                // own Dashboard.
                                if (r.LastRevertedBy != null && r.LastRevertedBy.ToLower() == UsernameLower)
                                {
                                    ApproverReverted++;
                                    continue;
                                }
                                if (r.InventoryUser?.Group == null) continue;
                                var Family = FamilyMap.FirstOrDefault(x => x.FamilyID == r.CategoryID);
                                var Usernames = GetFullApproverUsernames(r, r.InventoryUser, Family, ScopesV1, ScopesV2);
                                if (Usernames.Contains(UsernameLower)) ApproverReverted++;
                            }
                        }
                    }
                    else
                    {
                        ApproverReverted = await RevertedQuery.CountAsync(x =>
                            x.LastRevertedBy != null && x.LastRevertedBy.ToLower() == UsernameLower);
                    }
                }
            }

            return new ItemDashboardCounts
            {
                ReqPending = ReqPending,
                ReqDone = ReqDone,
                ReqCancel = ReqCancel,
                ApproverPending = ApproverPending,
                ApproverDone = ApproverDone,
                ApproverCancel = ApproverCancel,
                ApproverReverted = ApproverReverted,
            };
        }

        /// <summary>
        /// Restored for backward compatibility: iOS builds already in
        /// production (App Store review lag means an older binary can still
        /// be live after Android has moved on) still call this old
        /// Item-Request-only route directly. Do not remove again without
        /// confirming no client still calls it. Android/current builds use
        /// Dashboard/Summary below instead, which this shares its item-count
        /// logic with via ComputeItemDashboardCounts (so both stay
        /// bug-for-bug consistent) but additionally combines in Job Request
        /// numbers — this endpoint deliberately stays Item Request only,
        /// matching the old contract byte-for-byte (Job Request's numbers
        /// were always fetched separately, client-side, by the app builds
        /// that call this route).
        /// </summary>
        [HttpGet("Dashboard/ItemCounts")]
        public async Task<IActionResult> DashboardItemCounts([FromQuery] PendingApproverDTO Query)
        {
            try
            {
                User? User = null;
                if (Query.UserName != null)
                {
                    User = await db.User.FirstOrDefaultAsync(x => x.Username.ToLower() == Query.UserName.ToLower());
                }
                else if (Query.UserID != null)
                {
                    User = await db.User.FirstOrDefaultAsync(x => x.ID == Query.UserID);
                }
                if (User == null) throw new Exception("User not found");

                var Item = await ComputeItemDashboardCounts(User);

                return Ok(new
                {
                    Success = true,
                    Message = "OK",
                    Data = new
                    {
                        Requester = new
                        {
                            Pending = Item.ReqPending,
                            Done = Item.ReqDone,
                            Cancel = Item.ReqCancel,
                            Total = Item.ReqPending + Item.ReqDone + Item.ReqCancel,
                        },
                        Approver = new
                        {
                            Pending = Item.ApproverPending,
                            Done = Item.ApproverDone,
                            Cancel = Item.ApproverCancel,
                            Reverted = Item.ApproverReverted,
                            Total = Item.ApproverPending + Item.ApproverDone + Item.ApproverCancel + Item.ApproverReverted,
                        }
                    }
                });
            }
            catch (Exception Ex)
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = Ex.Message,
                    Data = (object?)null
                });
            }
        }

        /// <summary>
        /// One dedicated Dashboard summary call — both the "documents I
        /// created" (Requester) and "documents awaiting my sign-off"
        /// (Approver) views, each broken down Pending/Done/Cancel/Reverted
        /// (+Total), covering Item Request (Requisition) AND Job Request
        /// combined into the SAME numbers. Item Request's half comes from
        /// ComputeItemDashboardCounts (shared with the legacy
        /// Dashboard/ItemCounts endpoint above); this adds Job Request's
        /// numbers on top — including a Job Request Reverted count that the
        /// old raw client-side approach never had at all (Job Request's own
        /// dashboard numbers were only ever Pending/Done/Cancel), which is
        /// why a reverted Job Request never showed up on the Dashboard.
        ///
        /// Approver.Pending reuses the EXISTING PendingApproval action (Item)
        /// and CountPendingJobRequestsForApprover (Job) in-process — same
        /// v1/v2 + admin-override + revert-aware resolution, not
        /// re-implemented here.
        /// </summary>
        [HttpGet("Dashboard/Summary")]
        public async Task<IActionResult> DashboardSummary([FromQuery] PendingApproverDTO Query)
        {
            try
            {
                User? User = null;
                if (Query.UserName != null)
                {
                    User = await db.User.FirstOrDefaultAsync(x => x.Username.ToLower() == Query.UserName.ToLower());
                }
                else if (Query.UserID != null)
                {
                    User = await db.User.FirstOrDefaultAsync(x => x.ID == Query.UserID);
                }
                if (User == null) throw new Exception("User not found");

                var IsAdminUser = User.Role == "ADMIN";
                var UsernameLower = User.Username.ToLower();
                var ItemCounts = await ComputeItemDashboardCounts(User);
                int ReqPending = ItemCounts.ReqPending, ReqDone = ItemCounts.ReqDone, ReqCancel = ItemCounts.ReqCancel;
                int ApproverPending = ItemCounts.ApproverPending, ApproverDone = ItemCounts.ApproverDone,
                    ApproverCancel = ItemCounts.ApproverCancel, ApproverReverted = ItemCounts.ApproverReverted;

                // Job Request — same CreatedBy/Status/ApprovalStatus filters
                // the old client-side raw OData calls used, just moved
                // server-side.
                int JobReqPending = await db.JobRequest.CountAsync(x =>
                    x.Type == "PUPA" && x.CreatedBy != null && x.CreatedBy.ToLower() == UsernameLower
                    && x.Status == "Submitted" && x.ApprovalStatus == "Pending");
                int JobReqDone = await db.JobRequest.CountAsync(x =>
                    x.Type == "PUPA" && x.CreatedBy != null && x.CreatedBy.ToLower() == UsernameLower
                    && x.ApprovalStatus == "Approved");
                int JobReqCancel = await db.JobRequest.CountAsync(x =>
                    x.Type == "PUPA" && x.CreatedBy != null && x.CreatedBy.ToLower() == UsernameLower
                    && x.ApprovalStatus == "Rejected");

                // Job Request Done/Cancel — same ApprovedByN / RejectedBy
                // filters the old client-side raw calls used.
                int JobApproverDone = await db.JobRequest.CountAsync(x =>
                    ((x.ApprovedBy1 != null && x.ApprovedBy1.ToLower() == UsernameLower) ||
                     (x.ApprovedBy2 != null && x.ApprovedBy2.ToLower() == UsernameLower) ||
                     (x.ApprovedBy3 != null && x.ApprovedBy3.ToLower() == UsernameLower) ||
                     (x.ApprovedBy4 != null && x.ApprovedBy4.ToLower() == UsernameLower) ||
                     (x.ApprovedBy5 != null && x.ApprovedBy5.ToLower() == UsernameLower) ||
                     (x.ApprovedBy6 != null && x.ApprovedBy6.ToLower() == UsernameLower) ||
                     (x.ApprovedBy7 != null && x.ApprovedBy7.ToLower() == UsernameLower))
                    && x.ApprovalStatus.ToLower() != "rejected");
                int JobApproverCancel = await db.JobRequest.CountAsync(x =>
                    x.RejectedBy != null && x.RejectedBy.ToLower() == UsernameLower);

                // Job Request Pending — reuses the existing (previously
                // unwired) CountPendingJobRequestsForApprover helper, same
                // ResolveApprovers-based, v1/v2-aware, tied-approver-aware
                // resolution Pending Approvals itself already uses.
                int JobApproverPending = await CountPendingJobRequestsForApprover(User);

                var VesselIdsInScope = await db.UserVesselRel.AsNoTracking()
                    .Where(x => x.UserID == User.ID)
                    .Select(x => x.VesselID)
                    .Distinct()
                    .ToListAsync();

                // ── Approver: Job Request Reverted (NEW — never existed
                // before; this is the actual "reverted doesn't show up" gap).
                // Same ADMIN/non-ADMIN split as Item Reverted above, but
                // ADMIN membership is checked via ResolveApprovers (Job
                // Request's own approver resolution, keyed by
                // VesselInventoryUserRowID + the job's Category — see
                // CountPendingJobRequestsForApprover) instead of
                // GetFullApproverUsernames, which only understands
                // Requisition.
                int JobApproverReverted = 0;
                {
                    if (VesselIdsInScope.Any())
                    {
                        var RevertedJobsQuery = db.JobRequest.AsNoTracking()
                            .Include(j => j.Jobs).ThenInclude(jb => jb.JobDetails)
                            .Where(j => j.VesselInventoryUserRowID.HasValue && VesselIdsInScope.Contains(j.VesselInventoryUserRowID.Value)
                                && j.Approved == false
                                && j.Revised != true
                                && j.RevertStatus == "REVERTED"
                                && j.ApprovalStatus.ToLower() != "rejected");

                        if (IsAdminUser)
                        {
                            var RevertedJobCandidates = await RevertedJobsQuery
                                .Where(j => j.LastRevertedBy != null)
                                .ToListAsync();

                            foreach (var job in RevertedJobCandidates)
                            {
                                // Same override rule as Item Reverted above — an ADMIN who
                                // personally reverted this job counts it as theirs even if
                                // not formally in the resolved approval chain.
                                if (job.LastRevertedBy != null && job.LastRevertedBy.ToLower() == UsernameLower)
                                {
                                    JobApproverReverted++;
                                    continue;
                                }
                                try
                                {
                                    var Category = job.Jobs?.FirstOrDefault()?.JobDetails?.FirstOrDefault()?.Category;
                                    var ResolveResult = await ResolveApprovers(new ResolveApproversDTO
                                    {
                                        VesselID = job.VesselInventoryUserRowID,
                                        Category = Category,
                                    }) as OkObjectResult;
                                    dynamic? Payload = ResolveResult?.Value;
                                    if (Payload == null || Payload.Success != true) continue;

                                    bool IsInChain = false;
                                    foreach (dynamic Item in Payload.Data.Items)
                                    {
                                        List<string> Unames = Item.Usernames;
                                        if (Unames.Any(u => !string.IsNullOrEmpty(u) && u.ToLower() == UsernameLower))
                                        {
                                            IsInChain = true;
                                            break;
                                        }
                                    }
                                    if (IsInChain) JobApproverReverted++;
                                }
                                catch
                                {
                                    // One bad Job Request shouldn't take down the whole count.
                                }
                            }
                        }
                        else
                        {
                            JobApproverReverted = await RevertedJobsQuery.CountAsync(j =>
                                j.LastRevertedBy != null && j.LastRevertedBy.ToLower() == UsernameLower);
                        }
                    }
                }

                int ApproverTotal = (ApproverPending + JobApproverPending)
                    + (ApproverDone + JobApproverDone)
                    + (ApproverCancel + JobApproverCancel)
                    + (ApproverReverted + JobApproverReverted);

                return Ok(new
                {
                    Success = true,
                    Message = "OK",
                    Data = new
                    {
                        Requester = new
                        {
                            Pending = ReqPending + JobReqPending,
                            Done = ReqDone + JobReqDone,
                            Cancel = ReqCancel + JobReqCancel,
                            Total = ReqPending + ReqDone + ReqCancel + JobReqPending + JobReqDone + JobReqCancel,
                        },
                        Approver = new
                        {
                            Pending = ApproverPending + JobApproverPending,
                            Done = ApproverDone + JobApproverDone,
                            Cancel = ApproverCancel + JobApproverCancel,
                            Reverted = ApproverReverted + JobApproverReverted,
                            Total = ApproverTotal,
                        }
                    }
                });
            }
            catch (Exception Ex)
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = Ex.Message,
                    Data = (object?)null
                });
            }
        }

        // Every username assigned as approver at ANY level (1-7) for
        // [requisition] on [vessel] — used by the Dashboard's Reverted count
        // to check "is this ADMIN actually part of the approval chain",
        // dispatching on ApprovalRuleVersion like everywhere else in this
        // file. Built on the same ResolveScopeCandidatesV1/V2 helpers the
        // rest of the approver-resolution actions use, so it stays correct
        // for both v1 and v2 vessels (the Dart version this replaces only
        // ever queried UserApprovalScope — always empty for v2 vessels).
        private HashSet<string> GetFullApproverUsernames(
            Requisition requisition, InventoryUser vessel, StockFamily? family,
            List<UserApprovalScope> scopesV1, List<UserApprovalScope2> scopesV2)
        {
            var usernames = new HashSet<string>();
            bool hasFamily = family != null;
            int? familyCategoryId = family?.StockCategoryID;
            int? familyFamilyId = family?.FamilyID;
            var scopesInGroup = scopesV1.Where(x => x.VesselGroupID == vessel.Group!.ID).ToList();

            for (int level = 1; level <= 7; level++)
            {
                if (requisition.ApprovalRuleVersion == 2)
                {
                    var candidates = ResolveScopeCandidatesV2(scopesV2, vessel, familyCategoryId, familyFamilyId, level, requisition.Group, requisition.Department, requisition.SubDepartment);
                    foreach (var c in candidates)
                    {
                        if (!string.IsNullOrEmpty(c.User?.Username)) usernames.Add(c.User!.Username!.ToLower());
                    }
                }
                else
                {
                    var candidates = ResolveScopeCandidatesV1(scopesInGroup, level, vessel.ID, hasFamily, familyCategoryId, familyFamilyId, requisition.Department, requisition.SubDepartment);
                    foreach (var c in candidates)
                    {
                        if (!string.IsNullOrEmpty(c.User?.Username)) usernames.Add(c.User!.Username!.ToLower());
                    }
                }
            }
            return usernames;
        }

        /// <summary>
        /// Job Requests on any vessel [user] is assigned to (UserVesselRel)
        /// where it is currently [user]'s turn to sign — resolved per job via
        /// ResolveApprovers (the same category-aware, v1/v2-aware resolver
        /// this session's other Job Request approver-chain fixes call), not
        /// the client-side UserApprovalScope-only matching PendingApprovalsHelper
        /// used to do (and, for v1, still does — v2 vessels and category-specific
        /// rules were never reachable there for Job Requests).
        /// </summary>
        private async Task<int> CountPendingJobRequestsForApprover(User user)
        {
            var VesselIdsInScope = await db.UserVesselRel.AsNoTracking()
                .Where(x => x.UserID == user.ID)
                .Select(x => x.VesselID)
                .Distinct()
                .ToListAsync();
            if (!VesselIdsInScope.Any()) return 0;

            // JobRequest.VesselInventoryUserRowID holds InventoryUser.ID (the
            // PK) despite the name — confirmed against real data (see
            // CustomDataController.SyncLegacyMirrorForVessel for the same
            // finding), so it's compared directly against InventoryUser.ID
            // via UserVesselRel.VesselID (also the PK) here.
            var PendingJobs = await db.JobRequest
                .Include(j => j.Jobs).ThenInclude(jb => jb.JobDetails)
                .Where(j => j.Type == "PUPA" && j.Status == "Submitted" && j.ApprovalStatus == "Pending"
                         && j.VesselInventoryUserRowID.HasValue
                         && VesselIdsInScope.Contains(j.VesselInventoryUserRowID.Value))
                .ToListAsync();
            if (!PendingJobs.Any()) return 0;

            var UsernameLower = user.Username.ToLower();
            int Count = 0;

            foreach (var Job in PendingJobs)
            {
                try
                {
                    var Category = Job.Jobs?.FirstOrDefault()?.JobDetails?.FirstOrDefault()?.Category;
                    var ResolveResult = await ResolveApprovers(new ResolveApproversDTO
                    {
                        VesselID = Job.VesselInventoryUserRowID,
                        Category = Category,
                    }) as OkObjectResult;
                    dynamic? Payload = ResolveResult?.Value;
                    if (Payload == null || Payload.Success != true) continue;

                    string? GetSigned(int lvl) => lvl switch
                    {
                        1 => Job.ApprovedBy1,
                        2 => Job.ApprovedBy2,
                        3 => Job.ApprovedBy3,
                        4 => Job.ApprovedBy4,
                        5 => Job.ApprovedBy5,
                        6 => Job.ApprovedBy6,
                        7 => Job.ApprovedBy7,
                        _ => null
                    };
                    int MaxLevel = Job.ApprovalMaxLevel > 0 ? Job.ApprovalMaxLevel : 7;
                    int PendingLevel = MaxLevel + 1; // default: fully signed already
                    for (int i = 1; i <= MaxLevel; i++)
                    {
                        if (string.IsNullOrWhiteSpace(GetSigned(i))) { PendingLevel = i; break; }
                    }
                    if (PendingLevel > MaxLevel) continue;

                    foreach (dynamic Item in Payload.Data.Items)
                    {
                        if ((int)Item.Level == PendingLevel)
                        {
                            // Multiple tied approvers at this level (see
                            // ResolveApprovers' new Usernames field) — any one of
                            // them counts as pending for them.
                            List<string> Unames = Item.Usernames;
                            if (Unames.Any(u => !string.IsNullOrEmpty(u) && u.ToLower() == UsernameLower)) Count++;
                            break;
                        }
                    }
                }
                catch
                {
                    // One bad Job Request shouldn't take down the whole count.
                }
            }

            return Count;
        }


        [HttpGet("Approval/Pending")]
        public async Task<IActionResult> PendingApproval([FromQuery] PendingApproverDTO Query)
        {
            try
            {
                User? User = null;
                if (Query.UserName != null)
                {
                    User = await db.User.FirstOrDefaultAsync(x => x.Username.ToLower() == Query.UserName.ToLower());
                }
                else if (Query.UserID != null)
                {
                    User = await db.User.FirstOrDefaultAsync(x => x.ID == Query.UserID);
                }

                if (User == null) throw new Exception("User not found");

                var IsAdminUser = User.Role == "ADMIN";
                var UserNameLower = User.Username.ToLower();

                // -----------------------------------------------------------------
                // Kumpulkan requisition dari 2 sumber terpisah:
                // A) Requisition normal pada inventory user (kapal) yang di-assign ke user ini.
                // B) Requisition yang di-revert OLEH user ini (LastRevertedBy == username dia),
                //    walaupun kapalnya di luar assignment dia.
                // -----------------------------------------------------------------
                var NormalRequisitions = new List<Requisition>();

                var VesselIDsInScope = await db.UserVesselRel.AsNoTracking()
                    .Where(x => x.UserID == User.ID)
                    .Select(x => x.VesselID)
                    .Distinct()
                    .ToListAsync();

                if (VesselIDsInScope.Any())
                {
                    NormalRequisitions = await db.Requisition.Include(x => x.InventoryUser).AsNoTracking()
                        .Where(x => x.VesselID.HasValue
                            && VesselIDsInScope.Contains(x.VesselID.Value)
                            && (x.Status == "PENDING" || x.RevertStatus == "REVERTED")
                            && x.RequisitionNumber.Substring(0, 2) != "SO")
                        .ToListAsync();
                }

                var RevertedByMeRequisitions = await db.Requisition.Include(x => x.InventoryUser).AsNoTracking()
                    .Where(x => x.RevertStatus == "REVERTED"
                        && x.LastRevertedBy != null
                        && x.LastRevertedBy.ToLower() == UserNameLower
                        && x.RequisitionNumber.Substring(0, 2) != "SO")
                    .ToListAsync();

                var Requisitions = NormalRequisitions
                    .Concat(RevertedByMeRequisitions)
                    .GroupBy(x => x.ID)
                    .Select(g => g.First())
                    // ✅ Guard: skip requisition yang datanya tidak lengkap dari awal
                    .Where(x => x.VesselID.HasValue && x.ApprovalMaxLevel.HasValue && x.ApprovalMaxLevel.Value > 0)
                    .ToList();

                if (!Requisitions.Any())
                {
                    return Ok(new
                    {
                        Success = true,
                        Message = "OK",
                        Data = new { Items = new List<object>(), TotalCount = 0, Offset = 0, Limit = 0 }
                    });
                }

                // Vessel info (+Group) untuk SEMUA requisition yang relevan
                var NeededVesselIDs = Requisitions
                    .Where(x => x.VesselID.HasValue)
                    .Select(x => x.VesselID.Value)
                    .Distinct()
                    .ToList();

                var Vessels = await db.InventoryUser.AsNoTracking()
                    .Include(x => x.Group)
                    .Where(x => NeededVesselIDs.Contains(x.ID))
                    .ToListAsync();

                var RelevantVesselGroupIDs = Vessels.Where(x => x.Group != null).Select(x => x.Group.ID).Distinct().ToList();

                var Scopes = await db.UserApprovalScope.AsNoTracking()
                    .Include(x => x.User)
                    .Where(x => RelevantVesselGroupIDs.Contains(x.VesselGroupID.Value))
                    .ToListAsync();

                // Each document's OWN snapshot decides the rule engine — not the
                // vessel's current flag. Only loaded when at least one relevant
                // requisition actually opted into v2 (v1-only requests don't pay
                // for the extra query).
                List<UserApprovalScope2> ScopesV2 = new();
                if (Requisitions.Any(r => r.ApprovalRuleVersion == 2))
                {
                    ScopesV2 = await db.UserApprovalScope2.AsNoTracking()
                        .Include(x => x.User)
                        .Where(x => x.IsActive != false)
                        .ToListAsync();
                }

                var FamilyMap = await db.StockFamily.AsNoTracking().ToListAsync();

                var UserByUsernameLower = await db.User.AsNoTracking()
                    .ToDictionaryAsync(x => x.Username.ToLower(), x => x);

                string? GetActualApproverName(Requisition Requisition, int level)
                {
                    return level switch
                    {
                        1 => Requisition.ApprovedBy1,
                        2 => Requisition.ApprovedBy2,
                        3 => Requisition.ApprovedBy3,
                        4 => Requisition.ApprovedBy4,
                        5 => Requisition.ApprovedBy5,
                        6 => Requisition.ApprovedBy6,
                        7 => Requisition.ApprovedBy7,
                        _ => null
                    };
                }

                // Group-combined documents: same rationale as CheckApprover above —
                // a Group's DB Level values aren't required to be contiguous from 1,
                // so [Level] here means "the Nth resolved approver" (position),
                // resolved by scanning+compacting every possible Level, not "the row
                // whose own Level field equals [Level]". Each position is a LIST —
                // every candidate tied at that position's winning tier.
                List<List<UserApprovalScope2>> ResolveGroupChain(Requisition Requisition, InventoryUser Vessel)
                {
                    var chain = new List<List<UserApprovalScope2>>();
                    for (int lvl = 1; lvl <= 7; lvl++)
                    {
                        var candidates = ResolveScopeCandidatesV2(ScopesV2, Vessel, null, null, lvl, Requisition.Group, Requisition.Department, Requisition.SubDepartment);
                        if (candidates.Count > 0) chain.Add(candidates);
                    }
                    return chain;
                }

                // Unified resolver: picks the v1 cascade or the v2 Specificity-based
                // match depending on the vessel's ApprovalRuleVersion flag, normalized
                // to a plain (candidate UserIds, display summary) pair since the two
                // source tables are different C# types. UserIds may hold more than one
                // ID when an admin has configured multiple tied approvers for the
                // identical scope — any one of them approving is enough.
                (List<int> UserIds, object? Matched) ResolveApprover(Requisition Requisition, InventoryUser Vessel, int Level)
                {
                    if (Requisition.ApprovalRuleVersion == 2)
                    {
                        var Family = FamilyMap.FirstOrDefault(x => x.FamilyID == Requisition.CategoryID);
                        var CandidatesV2 = (!string.IsNullOrEmpty(Requisition.Group)
                            ? (ResolveGroupChain(Requisition, Vessel).ElementAtOrDefault(Level - 1) ?? new List<UserApprovalScope2>())
                            : ResolveScopeCandidatesV2(ScopesV2, Vessel, Family?.StockCategoryID, Family?.FamilyID, Level, Requisition.Group, Requisition.Department, Requisition.SubDepartment))
                            .OrderBy(s => s.ID).ToList();

                        var IdsV2 = CandidatesV2.Where(s => s.UserID != null).Select(s => s.UserID!.Value).ToList();
                        var FirstV2 = CandidatesV2.FirstOrDefault();
                        object? MatchedV2 = FirstV2 == null ? null : new
                        {
                            FirstV2.ID,
                            FirstV2.VesselID,
                            FirstV2.VesselGroupID,
                            FirstV2.CompanyDB,
                            FirstV2.Group,
                            FirstV2.StockCategoryID,
                            FirstV2.StockFamilyID,
                            FirstV2.Department,
                            FirstV2.SubDepartment,
                        };
                        return (IdsV2, MatchedV2);
                    }

                    var Family1 = FamilyMap.FirstOrDefault(x => x.FamilyID == Requisition.CategoryID);
                    var CandidatesV1 = ResolveScopeCandidatesV1(
                            Scopes.Where(x => x.VesselGroupID == Vessel.Group.ID), Level, Vessel.ID,
                            Family1 != null, Family1?.StockCategoryID, Family1?.FamilyID,
                            Requisition.Department, Requisition.SubDepartment)
                        .OrderBy(s => s.ID).ToList();

                    var IdsV1 = CandidatesV1.Where(s => s.UserID != null).Select(s => s.UserID!.Value).ToList();
                    var FirstV1 = CandidatesV1.FirstOrDefault();
                    object? MatchedV1 = FirstV1 == null ? null : new
                    {
                        FirstV1.ID,
                        FirstV1.InventoryUserID,
                        FirstV1.StockCategoryID,
                        FirstV1.StockFamilyID,
                        FirstV1.Department,
                        FirstV1.SubDepartment,
                    };
                    return (IdsV1, MatchedV1);
                }

                object BuildItem(Requisition Requisition, InventoryUser Vessel, int? Level, object? MatchedScopeSummary, bool IsAdminOverride, string? AdminApprovedBy = null)
                {
                    var IsReverted = Requisition.RevertStatus == "REVERTED";

                    return new
                    {
                        Requisition.ID,
                        Requisition.RequisitionNumber,
                        // Lets the Approvals list page (React Webview) sort vessels by
                        // newest pending order without a separate full-Requisition
                        // fetch just for this one field.
                        Requisition.Date,
                        PendingLevel = Level,
                        Requisition.Status,
                        Requisition.RevertStatus,
                        IsAdminOverride,
                        IsReverted,
                        RevertDetail = !IsReverted ? null : new
                        {
                            Requisition.LastRevertedBy
                        },
                        AdminApprovedBy,
                        Requisition.Department,
                        Requisition.SubDepartment,
                        Vessel = new
                        {
                            ID = Requisition.InventoryUser.ID,
                            DB = Requisition.InventoryUser.DB,
                            InventoryUserID = Requisition.InventoryUser.InventoryUserID,
                            InventoryUserCode = Requisition.InventoryUser.InventoryUserCode,
                            InventoryUserName = Requisition.InventoryUser.InventoryUserName
                        },
                        VesselID = Vessel.ID,
                        VesselGroupID = Vessel.Group.ID,
                        MatchedScope = MatchedScopeSummary
                    };
                }

                var PendingList = new List<object>();

                foreach (var Requisition in Requisitions)
                {
                    try
                    {
                        // ✅ Skip kalau VesselID null
                        if (Requisition.VesselID == null) continue;

                        var Vessel = Vessels.FirstOrDefault(x => x.ID == Requisition.VesselID);
                        if (Vessel?.Group == null) continue;

                        // ✅ Skip kalau ApprovalMaxLevel null/invalid
                        if (Requisition.ApprovalMaxLevel == null || Requisition.ApprovalMaxLevel.Value <= 0) continue;

                        int NormalPendingLevel = Requisition.ApprovalMaxLevel.Value + 1; // default: fully approved
                        var AdminApprovedLevels = new List<(int Level, string ApprovedByName)>();

                        for (int i = 1; i <= Requisition.ApprovalMaxLevel; i++)
                        {
                            var ApprovedByName = GetActualApproverName(Requisition, i);

                            if (string.IsNullOrWhiteSpace(ApprovedByName))
                            {
                                NormalPendingLevel = i;
                                break;
                            }

                            if (UserByUsernameLower.TryGetValue(ApprovedByName.ToLower(), out var ApproverUser)
                                && ApproverUser.Role == "ADMIN")
                            {
                                AdminApprovedLevels.Add((i, ApprovedByName));
                            }
                        }

                        bool IsFullyApproved = NormalPendingLevel > Requisition.ApprovalMaxLevel;
                        bool AddedThisRequisition = false;

                        // 1) Flow normal
                        if (!IsFullyApproved)
                        {
                            var Resolved = ResolveApprover(Requisition, Vessel, NormalPendingLevel);

                            // Multiple tied approvers at this level — any one of
                            // them being the current user makes it pending for them.
                            if (Resolved.UserIds.Contains(User.ID))
                            {
                                PendingList.Add(BuildItem(Requisition, Vessel, NormalPendingLevel, Resolved.Matched, IsAdminOverride: false));
                                AddedThisRequisition = true;
                            }
                        }

                        // 2) Flow khusus ADMIN — level yang sudah di-approve oleh user ber-Role ADMIN
                        // dimunculkan lagi untuk ADMIN LAIN (oversight), tapi TIDAK untuk admin yang
                        // approve-nya sendiri — dia sudah tahu, jangan tampil balik ke pending-nya dia.
                        if (IsAdminUser && AdminApprovedLevels.Any())
                        {
                            foreach (var (Level, ApprovedByName) in AdminApprovedLevels)
                            {
                                if (ApprovedByName.ToLower() == UserNameLower) continue;
                                var Resolved = ResolveApprover(Requisition, Vessel, Level);
                                PendingList.Add(BuildItem(Requisition, Vessel, Level, Resolved.Matched, IsAdminOverride: true, ApprovedByName));
                                AddedThisRequisition = true;
                            }
                        }

                        // 3) Flow khusus ADMIN — requisition REVERTED apapun statusnya HARUS tetap kelihatan
                        if (IsAdminUser && Requisition.RevertStatus == "REVERTED" && !AddedThisRequisition)
                        {
                            var RevertLevel = IsFullyApproved ? (int?)null : NormalPendingLevel;
                            object? RevertMatched = IsFullyApproved ? null : ResolveApprover(Requisition, Vessel, NormalPendingLevel).Matched;
                            PendingList.Add(BuildItem(Requisition, Vessel, RevertLevel, RevertMatched, IsAdminOverride: false));
                            AddedThisRequisition = true;
                        }

                        // 4) Flow: siapapun yang me-revert harus tetap lihat requisition ini
                        if (!AddedThisRequisition
                            && Requisition.RevertStatus == "REVERTED"
                            && !string.IsNullOrWhiteSpace(Requisition.LastRevertedBy)
                            && Requisition.LastRevertedBy.ToLower() == UserNameLower)
                        {
                            PendingList.Add(BuildItem(Requisition, Vessel, null, null, IsAdminOverride: false));
                            AddedThisRequisition = true;
                        }
                    }
                    catch (Exception ItemEx)
                    {
                        // ✅ Satu requisition bermasalah tidak boleh mematikan seluruh response.
                        // Ganti dengan logger kamu kalau ada (ErrorLogger, Serilog, dll).
                        Console.WriteLine($"[PendingApproval] Skip Requisition ID={Requisition.ID}: {ItemEx.Message}");
                        continue;
                    }
                }

                return Ok(new
                {
                    Success = true,
                    Message = "OK",
                    Data = new
                    {
                        Items = PendingList,
                        TotalCount = PendingList.Count,
                        Offset = 0,
                        Limit = PendingList.Count
                    }
                });
            }
            catch (Exception Ex)
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = Ex.Message,
                    Data = (object?)null
                });
            }
        }
        [HttpGet("Approver/Done")]
        public async Task<IActionResult> DoneApproval([FromQuery] PendingApproverDTO Query)
        {
            try
            {
                User? User = null;
                if (Query.UserName != null)
                {
                    User = await db.User.FirstOrDefaultAsync(x => x.Username.ToLower() == Query.UserName.ToLower());
                }
                else if (Query.UserID != null)
                {
                    User = await db.User.FirstOrDefaultAsync(x => x.ID == Query.UserID);
                }
                if (User == null) throw new Exception("User not found");

                var UserNameLower = User.Username.ToLower();

                // Filter di level SQL: ApprovedFromApp = true DAN minimal 1 kolom ApprovedByN match username
                var Requisitions = await db.Requisition.AsNoTracking()
                    .Where(x => x.ApprovedFromApp == true &&
                        (
                            (x.ApprovedBy1 != null && x.ApprovedBy1.ToLower() == UserNameLower) ||
                            (x.ApprovedBy2 != null && x.ApprovedBy2.ToLower() == UserNameLower) ||
                            (x.ApprovedBy3 != null && x.ApprovedBy3.ToLower() == UserNameLower) ||
                            (x.ApprovedBy4 != null && x.ApprovedBy4.ToLower() == UserNameLower) ||
                            (x.ApprovedBy5 != null && x.ApprovedBy5.ToLower() == UserNameLower) ||
                            (x.ApprovedBy6 != null && x.ApprovedBy6.ToLower() == UserNameLower) ||
                            (x.ApprovedBy7 != null && x.ApprovedBy7.ToLower() == UserNameLower)
                        ) && x.Status != "PENDING" && x.Status != "VOID" && x.Status != "REJECTED")
                    .ToListAsync();

                string? GetActualApproverName(Requisition Requisition, int level)
                {
                    return level switch
                    {
                        1 => Requisition.ApprovedBy1,
                        2 => Requisition.ApprovedBy2,
                        3 => Requisition.ApprovedBy3,
                        4 => Requisition.ApprovedBy4,
                        5 => Requisition.ApprovedBy5,
                        6 => Requisition.ApprovedBy6,
                        7 => Requisition.ApprovedBy7,
                        _ => null
                    };
                }

                var DoneList = new List<object>();

                foreach (var Requisition in Requisitions)
                {
                    // Bisa ada lebih dari 1 level yang di-approve oleh user yang sama
                    for (int Level = 1; Level <= Requisition.ApprovalMaxLevel; Level++)
                    {
                        var ApprovedByName = GetActualApproverName(Requisition, Level);
                        if (!string.IsNullOrWhiteSpace(ApprovedByName) && ApprovedByName.ToLower() == UserNameLower)
                        {
                            DoneList.Add(new
                            {
                                Requisition.ID,
                                Requisition.RequisitionNumber,
                                Level,
                                ApprovedBy = ApprovedByName,
                                Requisition.Department,
                                Requisition.SubDepartment,
                                Requisition.VesselID,
                            });
                        }
                    }
                }

                return Ok(new
                {
                    Success = true,
                    Message = "OK",
                    Data = new
                    {
                        Items = DoneList,
                        TotalCount = DoneList.Count,
                        Offset = 0,
                        Limit = DoneList.Count
                    }
                });
            }
            catch (Exception Ex)
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = Ex.Message,
                    Data = (object?)null
                });
            }
        }
    }
}