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
                bool MatchesV2(UserApprovalScope2 s, InventoryUser Vsl, int? CatId, int? FamId, int Lvl, string? Grp)
                {
                    if (s.IsActive == false) return false;
                    if (s.Level != null && s.Level != Lvl) return false;
                    if (s.VesselID != null && s.VesselID != Vsl.ID) return false;
                    if (s.VesselGroupID != null && s.VesselGroupID != Vsl.Group?.ID) return false;
                    if (s.CompanyDB != null && s.CompanyDB != Vsl.DB) return false;
                    if (s.Department != null && s.Department != Requisition.Department) return false;
                    if (s.SubDepartment != null && s.SubDepartment != Requisition.SubDepartment) return false;

                    if (!string.IsNullOrEmpty(Grp))
                    {
                        return s.Group == Grp;
                    }

                    if (s.StockCategoryID != null && s.StockCategoryID != CatId) return false;
                    if (s.StockFamilyID != null && s.StockFamilyID != FamId) return false;
                    return true;
                }

                UserApprovalScope2? ResolveScopeV2(InventoryUser Vsl, int? CatId, int? FamId, int Lvl, string? Grp)
                {
                    return ScopesV2.Where(s => MatchesV2(s, Vsl, CatId, FamId, Lvl, Grp))
                        .OrderByDescending(s => s.Specificity)
                        .ThenBy(s => s.ID)
                        .FirstOrDefault();
                }

                // "All Groups" fallback: a row with Group == NULL applies to every
                // Group at that Level. Used per-level below when the specific Group
                // has no row of its own at that Level (e.g. Group defines only
                // Level 2, so Level 1 falls back to the vessel's "All Groups" Level 1).
                bool MatchesV2AllGroups(UserApprovalScope2 s, InventoryUser Vsl, int Lvl)
                {
                    if (s.IsActive == false) return false;
                    if (s.Group != null) return false;
                    if (s.Level != null && s.Level != Lvl) return false;
                    if (s.VesselID != null && s.VesselID != Vsl.ID) return false;
                    if (s.VesselGroupID != null && s.VesselGroupID != Vsl.Group?.ID) return false;
                    if (s.CompanyDB != null && s.CompanyDB != Vsl.DB) return false;
                    if (s.Department != null && s.Department != Requisition.Department) return false;
                    if (s.SubDepartment != null && s.SubDepartment != Requisition.SubDepartment) return false;
                    return true;
                }

                UserApprovalScope2? ResolveScopeV2AllGroups(InventoryUser Vsl, int Lvl)
                {
                    return ScopesV2.Where(s => MatchesV2AllGroups(s, Vsl, Lvl))
                        .OrderByDescending(s => s.Specificity)
                        .ThenBy(s => s.ID)
                        .FirstOrDefault();
                }

                // Group-combined documents: admins number a Group's approval rows
                // however makes sense to them (e.g. only Level 2, no Level 1) — the
                // DB Level value is NOT a required-contiguous-from-1 position, it's
                // just a tie-breaker/ordering key. Resolve the full chain ONCE by
                // scanning every possible Level and compacting (skipping gaps),
                // exactly like ResolveApprovers already does at submit time, then
                // index into it POSITIONALLY below instead of re-querying by a
                // literal Level number that may not exist in the matrix at all.
                List<UserApprovalScope2> GroupChain = new();
                if (Requisition.ApprovalRuleVersion == 2 && !string.IsNullOrEmpty(Requisition.Group))
                {
                    for (int lvl = 1; lvl <= 7; lvl++)
                    {
                        var r = ResolveScopeV2(Vessel, null, null, lvl, Requisition.Group)
                            ?? ResolveScopeV2AllGroups(Vessel, lvl);
                        if (r != null) GroupChain.Add(r);
                    }
                }

                var ApprovalMatrix = new List<object>();
                var ApproverCount = Requisition.ApprovalMaxLevel;

                for (int i = 0; i < ApproverCount; i++)
                {
                    var Level = i + 1;
                    int? ResolvedUserId = null;
                    string? ResolvedUsername = null;
                    object? MatchedSummary = null;

                    if (Requisition.ApprovalRuleVersion == 2)
                    {
                        var ResolvedV2 = !string.IsNullOrEmpty(Requisition.Group)
                            ? GroupChain.ElementAtOrDefault(i)
                            : ResolveScopeV2(Vessel, FamilyStockCategoryID, FamilyFamilyID, Level, Requisition.Group);
                        ResolvedUserId = ResolvedV2?.UserID;
                        ResolvedUsername = ResolvedV2?.User?.Username;
                        MatchedSummary = ResolvedV2 == null ? null : new
                        {
                            ResolvedV2.ID,
                            ResolvedV2.VesselID,
                            ResolvedV2.VesselGroupID,
                            ResolvedV2.CompanyDB,
                            ResolvedV2.Group,
                            ResolvedV2.StockCategoryID,
                            ResolvedV2.StockFamilyID,
                            ResolvedV2.Department,
                            ResolvedV2.SubDepartment,
                        };
                    }
                    else
                    {
                    UserApprovalScope? ResolvedScope = null;

                    // ----------------------------------------------------------------
                    // Rule [1]-[7], [10]-[16] HANYA dijalankan kalau Family ditemukan,
                    // karena rule-rule ini butuh StockCategoryID/StockFamilyID spesifik.
                    // Kalau Family null, langsung skip ke rule [8],[9],[17],[18]
                    // (yang tidak butuh StockFamily).
                    // ----------------------------------------------------------------
                    if (Family != null)
                    {
                        // [1] InventoryUser + StockCategory + StockFamily + Department + SubDepartment
                        ResolvedScope = Scopes.FirstOrDefault(x =>
                            x.Level == Level &&
                            x.InventoryUserID == Vessel.ID &&
                            x.StockCategoryID == FamilyStockCategoryID &&
                            x.StockFamilyID == FamilyFamilyID &&
                            x.Department == Requisition.Department &&
                            x.SubDepartment == Requisition.SubDepartment);

                        // [2] InventoryUser + StockCategory + StockFamily + Department
                        ResolvedScope ??= Scopes.FirstOrDefault(x =>
                            x.Level == Level &&
                            x.InventoryUserID == Vessel.ID &&
                            x.StockCategoryID == FamilyStockCategoryID &&
                            x.StockFamilyID == FamilyFamilyID &&
                            x.Department == Requisition.Department &&
                            x.SubDepartment == null);

                        // [3] InventoryUser + StockCategory + StockFamily
                        ResolvedScope ??= Scopes.FirstOrDefault(x =>
                            x.Level == Level &&
                            x.InventoryUserID == Vessel.ID &&
                            x.StockCategoryID == FamilyStockCategoryID &&
                            x.StockFamilyID == FamilyFamilyID &&
                            x.Department == null &&
                            x.SubDepartment == null);

                        // [4] InventoryUser + StockCategory + Department
                        ResolvedScope ??= Scopes.FirstOrDefault(x =>
                            x.Level == Level &&
                            x.InventoryUserID == Vessel.ID &&
                            x.StockCategoryID == FamilyStockCategoryID &&
                            x.StockFamilyID == -1 &&
                            x.Department == Requisition.Department &&
                            x.SubDepartment == null);

                        // [5] InventoryUser + StockCategory
                        ResolvedScope ??= Scopes.FirstOrDefault(x =>
                            x.Level == Level &&
                            x.InventoryUserID == Vessel.ID &&
                            x.StockCategoryID == FamilyStockCategoryID &&
                            x.StockFamilyID == -1 &&
                            x.Department == null &&
                            x.SubDepartment == null);

                        // [6] InventoryUser + StockFamily + Department
                        ResolvedScope ??= Scopes.FirstOrDefault(x =>
                            x.Level == Level &&
                            x.InventoryUserID == Vessel.ID &&
                            x.StockCategoryID == -1 &&
                            x.StockFamilyID == FamilyFamilyID &&
                            x.Department == Requisition.Department &&
                            x.SubDepartment == null);

                        // [7] InventoryUser + StockFamily
                        ResolvedScope ??= Scopes.FirstOrDefault(x =>
                            x.Level == Level &&
                            x.InventoryUserID == Vessel.ID &&
                            x.StockCategoryID == -1 &&
                            x.StockFamilyID == FamilyFamilyID &&
                            x.Department == null &&
                            x.SubDepartment == null);
                    }

                    // [8] InventoryUser + Department  (tidak butuh StockFamily, selalu dijalankan)
                    ResolvedScope ??= Scopes.FirstOrDefault(x =>
                        x.Level == Level &&
                        x.InventoryUserID == Vessel.ID &&
                        x.StockCategoryID == -1 &&
                        x.StockFamilyID == -1 &&
                        x.Department == Requisition.Department &&
                        x.SubDepartment == null);

                    // [9] InventoryUser saja
                    ResolvedScope ??= Scopes.FirstOrDefault(x =>
                        x.Level == Level &&
                        x.InventoryUserID == Vessel.ID &&
                        x.StockCategoryID == -1 &&
                        x.StockFamilyID == -1 &&
                        x.Department == null &&
                        x.SubDepartment == null);

                    if (Family != null)
                    {
                        // [10] Global + StockCategory + StockFamily + Department + SubDepartment
                        ResolvedScope ??= Scopes.FirstOrDefault(x =>
                            x.Level == Level &&
                            x.InventoryUserID == null &&
                            x.StockCategoryID == FamilyStockCategoryID &&
                            x.StockFamilyID == FamilyFamilyID &&
                            x.Department == Requisition.Department &&
                            x.SubDepartment == Requisition.SubDepartment);

                        // [11] Global + StockCategory + StockFamily + Department
                        ResolvedScope ??= Scopes.FirstOrDefault(x =>
                            x.Level == Level &&
                            x.InventoryUserID == null &&
                            x.StockCategoryID == FamilyStockCategoryID &&
                            x.StockFamilyID == FamilyFamilyID &&
                            x.Department == Requisition.Department &&
                            x.SubDepartment == null);

                        // [12] Global + StockCategory + StockFamily
                        ResolvedScope ??= Scopes.FirstOrDefault(x =>
                            x.Level == Level &&
                            x.InventoryUserID == null &&
                            x.StockCategoryID == FamilyStockCategoryID &&
                            x.StockFamilyID == FamilyFamilyID &&
                            x.Department == null &&
                            x.SubDepartment == null);

                        // [13] Global + StockCategory + Department
                        ResolvedScope ??= Scopes.FirstOrDefault(x =>
                            x.Level == Level &&
                            x.InventoryUserID == null &&
                            x.StockCategoryID == FamilyStockCategoryID &&
                            x.StockFamilyID == -1 &&
                            x.Department == Requisition.Department &&
                            x.SubDepartment == null);

                        // [14] Global + StockCategory
                        ResolvedScope ??= Scopes.FirstOrDefault(x =>
                            x.Level == Level &&
                            x.InventoryUserID == null &&
                            x.StockCategoryID == FamilyStockCategoryID &&
                            x.StockFamilyID == -1 &&
                            x.Department == null &&
                            x.SubDepartment == null);

                        // [15] Global + StockFamily + Department
                        ResolvedScope ??= Scopes.FirstOrDefault(x =>
                            x.Level == Level &&
                            x.InventoryUserID == null &&
                            x.StockCategoryID == -1 &&
                            x.StockFamilyID == FamilyFamilyID &&
                            x.Department == Requisition.Department &&
                            x.SubDepartment == null);

                        // [16] Global + StockFamily
                        ResolvedScope ??= Scopes.FirstOrDefault(x =>
                            x.Level == Level &&
                            x.InventoryUserID == null &&
                            x.StockCategoryID == -1 &&
                            x.StockFamilyID == FamilyFamilyID &&
                            x.Department == null &&
                            x.SubDepartment == null);
                    }

                    // [17] Global + Department (tidak butuh StockFamily, selalu dijalankan)
                    ResolvedScope ??= Scopes.FirstOrDefault(x =>
                        x.Level == Level &&
                        x.InventoryUserID == null &&
                        x.StockCategoryID == -1 &&
                        x.StockFamilyID == -1 &&
                        x.Department == Requisition.Department &&
                        x.SubDepartment == null);

                    // [18] Paling general: catch-all VesselGroup
                    ResolvedScope ??= Scopes.FirstOrDefault(x =>
                        x.Level == Level &&
                        x.InventoryUserID == null &&
                        x.StockCategoryID == -1 &&
                        x.StockFamilyID == -1 &&
                        x.Department == null &&
                        x.SubDepartment == null);

                    ResolvedUserId = ResolvedScope?.UserID;
                    ResolvedUsername = ResolvedScope?.User?.Username;
                    MatchedSummary = ResolvedScope == null ? null : new
                    {
                        ResolvedScope.ID,
                        ResolvedScope.InventoryUserID,
                        ResolvedScope.StockCategoryID,
                        ResolvedScope.StockFamilyID,
                        ResolvedScope.Department,
                        ResolvedScope.SubDepartment,
                    };
                    }

                    var ActualApprover = GetActualApproverName(Level);

                    ApprovalMatrix.Add(new
                    {
                        Level,
                        Found = ResolvedUserId != null,
                        ShouldBeApproverUserID = ResolvedUserId,
                        ShouldBeApproverUsername = ResolvedUsername,
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
                        Offset = 1,
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

                var ResolveMatrix = new List<object>();

                if (Vessel.ApprovalRuleVersion == 2)
                {
                    var ScopesV2 = await db.UserApprovalScope2.AsNoTracking()
                        .Include(x => x.User)
                        .Where(x => x.IsActive != false)
                        .ToListAsync();

                    bool MatchesV2(UserApprovalScope2 s, int Lvl)
                    {
                        if (s.IsActive == false) return false;
                        if (s.Level != null && s.Level != Lvl) return false;
                        if (s.VesselID != null && s.VesselID != Vessel.ID) return false;
                        if (s.VesselGroupID != null && s.VesselGroupID != Vessel.Group?.ID) return false;
                        if (s.CompanyDB != null && s.CompanyDB != Vessel.DB) return false;
                        if (s.Department != null && s.Department != Query.Department) return false;
                        if (s.SubDepartment != null && s.SubDepartment != Query.SubDepartment) return false;

                        // Item Request V2 combined submission: resolve the Group's
                        // own combined chain. Any row carrying this Group label is
                        // eligible — Category/Family on the row are ignored here
                        // (they only matter for client-side Group-membership
                        // derivation, not for who approves the combined document).
                        if (!string.IsNullOrEmpty(Query.Group))
                        {
                            return s.Group == Query.Group;
                        }

                        if (s.StockCategoryID != null && s.StockCategoryID != FamilyStockCategoryID) return false;
                        if (s.StockFamilyID != null && s.StockFamilyID != FamilyFamilyID) return false;
                        return true;
                    }

                    // "All Groups" fallback: a row with Group == NULL applies to every
                    // Group at that Level. Used per-level when the specific Group has
                    // no row of its own at that Level.
                    bool MatchesV2AllGroups(UserApprovalScope2 s, int Lvl)
                    {
                        if (s.IsActive == false) return false;
                        if (s.Group != null) return false;
                        if (s.Level != null && s.Level != Lvl) return false;
                        if (s.VesselID != null && s.VesselID != Vessel.ID) return false;
                        if (s.VesselGroupID != null && s.VesselGroupID != Vessel.Group?.ID) return false;
                        if (s.CompanyDB != null && s.CompanyDB != Vessel.DB) return false;
                        if (s.Department != null && s.Department != Query.Department) return false;
                        if (s.SubDepartment != null && s.SubDepartment != Query.SubDepartment) return false;
                        return true;
                    }

                    for (int Level = 1; Level <= 7; Level++)
                    {
                        var Resolved = ScopesV2.Where(s => MatchesV2(s, Level))
                            .OrderByDescending(s => s.Specificity)
                            .ThenBy(s => s.ID)
                            .FirstOrDefault();

                        if (Resolved == null && !string.IsNullOrEmpty(Query.Group))
                        {
                            Resolved = ScopesV2.Where(s => MatchesV2AllGroups(s, Level))
                                .OrderByDescending(s => s.Specificity)
                                .ThenBy(s => s.ID)
                                .FirstOrDefault();
                        }

                        if (Resolved?.UserID == null) continue;

                        ResolveMatrix.Add(new
                        {
                            Level,
                            Resolved.UserID,
                            Username = Resolved.User?.Username,
                            Phone = Resolved.User?.Phone
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
                        UserApprovalScope? ResolvedScope = null;

                        if (Family != null)
                        {
                            ResolvedScope = Scopes.FirstOrDefault(x => x.Level == Level && x.InventoryUserID == Vessel.ID && x.StockCategoryID == FamilyStockCategoryID && x.StockFamilyID == FamilyFamilyID && x.Department == Query.Department && x.SubDepartment == Query.SubDepartment);
                            ResolvedScope ??= Scopes.FirstOrDefault(x => x.Level == Level && x.InventoryUserID == Vessel.ID && x.StockCategoryID == FamilyStockCategoryID && x.StockFamilyID == FamilyFamilyID && x.Department == Query.Department && x.SubDepartment == null);
                            ResolvedScope ??= Scopes.FirstOrDefault(x => x.Level == Level && x.InventoryUserID == Vessel.ID && x.StockCategoryID == FamilyStockCategoryID && x.StockFamilyID == FamilyFamilyID && x.Department == null && x.SubDepartment == null);
                            ResolvedScope ??= Scopes.FirstOrDefault(x => x.Level == Level && x.InventoryUserID == Vessel.ID && x.StockCategoryID == FamilyStockCategoryID && x.StockFamilyID == -1 && x.Department == Query.Department && x.SubDepartment == null);
                            ResolvedScope ??= Scopes.FirstOrDefault(x => x.Level == Level && x.InventoryUserID == Vessel.ID && x.StockCategoryID == FamilyStockCategoryID && x.StockFamilyID == -1 && x.Department == null && x.SubDepartment == null);
                            ResolvedScope ??= Scopes.FirstOrDefault(x => x.Level == Level && x.InventoryUserID == Vessel.ID && x.StockCategoryID == -1 && x.StockFamilyID == FamilyFamilyID && x.Department == Query.Department && x.SubDepartment == null);
                            ResolvedScope ??= Scopes.FirstOrDefault(x => x.Level == Level && x.InventoryUserID == Vessel.ID && x.StockCategoryID == -1 && x.StockFamilyID == FamilyFamilyID && x.Department == null && x.SubDepartment == null);
                        }

                        ResolvedScope ??= Scopes.FirstOrDefault(x => x.Level == Level && x.InventoryUserID == Vessel.ID && x.StockCategoryID == -1 && x.StockFamilyID == -1 && x.Department == Query.Department && x.SubDepartment == null);
                        ResolvedScope ??= Scopes.FirstOrDefault(x => x.Level == Level && x.InventoryUserID == Vessel.ID && x.StockCategoryID == -1 && x.StockFamilyID == -1 && x.Department == null && x.SubDepartment == null);

                        if (Family != null)
                        {
                            ResolvedScope ??= Scopes.FirstOrDefault(x => x.Level == Level && x.InventoryUserID == null && x.StockCategoryID == FamilyStockCategoryID && x.StockFamilyID == FamilyFamilyID && x.Department == Query.Department && x.SubDepartment == Query.SubDepartment);
                            ResolvedScope ??= Scopes.FirstOrDefault(x => x.Level == Level && x.InventoryUserID == null && x.StockCategoryID == FamilyStockCategoryID && x.StockFamilyID == FamilyFamilyID && x.Department == Query.Department && x.SubDepartment == null);
                            ResolvedScope ??= Scopes.FirstOrDefault(x => x.Level == Level && x.InventoryUserID == null && x.StockCategoryID == FamilyStockCategoryID && x.StockFamilyID == FamilyFamilyID && x.Department == null && x.SubDepartment == null);
                            ResolvedScope ??= Scopes.FirstOrDefault(x => x.Level == Level && x.InventoryUserID == null && x.StockCategoryID == FamilyStockCategoryID && x.StockFamilyID == -1 && x.Department == Query.Department && x.SubDepartment == null);
                            ResolvedScope ??= Scopes.FirstOrDefault(x => x.Level == Level && x.InventoryUserID == null && x.StockCategoryID == FamilyStockCategoryID && x.StockFamilyID == -1 && x.Department == null && x.SubDepartment == null);
                            ResolvedScope ??= Scopes.FirstOrDefault(x => x.Level == Level && x.InventoryUserID == null && x.StockCategoryID == -1 && x.StockFamilyID == FamilyFamilyID && x.Department == Query.Department && x.SubDepartment == null);
                            ResolvedScope ??= Scopes.FirstOrDefault(x => x.Level == Level && x.InventoryUserID == null && x.StockCategoryID == -1 && x.StockFamilyID == FamilyFamilyID && x.Department == null && x.SubDepartment == null);
                        }

                        ResolvedScope ??= Scopes.FirstOrDefault(x => x.Level == Level && x.InventoryUserID == null && x.StockCategoryID == -1 && x.StockFamilyID == -1 && x.Department == Query.Department && x.SubDepartment == null);
                        ResolvedScope ??= Scopes.FirstOrDefault(x => x.Level == Level && x.InventoryUserID == null && x.StockCategoryID == -1 && x.StockFamilyID == -1 && x.Department == null && x.SubDepartment == null);

                        if (ResolvedScope?.UserID == null) continue;

                        ResolveMatrix.Add(new
                        {
                            Level,
                            ResolvedScope.UserID,
                            Username = ResolvedScope.User?.Username,
                            Phone = ResolvedScope.User?.Phone
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
                        Data = new { Items = new List<object>(), TotalCount = 0, Offset = 1, Limit = 0 }
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

                UserApprovalScope? ResolveScope(Requisition Requisition, InventoryUser Vessel, int VesselGroupId, int Level)
                {
                    var Family = FamilyMap.FirstOrDefault(x => x.FamilyID == Requisition.CategoryID);
                    int? FamilyStockCategoryID = Family?.StockCategoryID;
                    int? FamilyFamilyID = Family?.FamilyID;

                    var ScopesInGroup = Scopes.Where(x => x.VesselGroupID == VesselGroupId);

                    UserApprovalScope? Resolved = null;

                    if (Family != null)
                    {
                        Resolved ??= ScopesInGroup.FirstOrDefault(x => x.Level == Level && x.InventoryUserID == Vessel.ID && x.StockCategoryID == FamilyStockCategoryID && x.StockFamilyID == FamilyFamilyID && x.Department == Requisition.Department && x.SubDepartment == Requisition.SubDepartment);
                        Resolved ??= ScopesInGroup.FirstOrDefault(x => x.Level == Level && x.InventoryUserID == Vessel.ID && x.StockCategoryID == FamilyStockCategoryID && x.StockFamilyID == FamilyFamilyID && x.Department == Requisition.Department && x.SubDepartment == null);
                        Resolved ??= ScopesInGroup.FirstOrDefault(x => x.Level == Level && x.InventoryUserID == Vessel.ID && x.StockCategoryID == FamilyStockCategoryID && x.StockFamilyID == FamilyFamilyID && x.Department == null && x.SubDepartment == null);
                        Resolved ??= ScopesInGroup.FirstOrDefault(x => x.Level == Level && x.InventoryUserID == Vessel.ID && x.StockCategoryID == FamilyStockCategoryID && x.StockFamilyID == -1 && x.Department == Requisition.Department && x.SubDepartment == null);
                        Resolved ??= ScopesInGroup.FirstOrDefault(x => x.Level == Level && x.InventoryUserID == Vessel.ID && x.StockCategoryID == FamilyStockCategoryID && x.StockFamilyID == -1 && x.Department == null && x.SubDepartment == null);
                        Resolved ??= ScopesInGroup.FirstOrDefault(x => x.Level == Level && x.InventoryUserID == Vessel.ID && x.StockCategoryID == -1 && x.StockFamilyID == FamilyFamilyID && x.Department == Requisition.Department && x.SubDepartment == null);
                        Resolved ??= ScopesInGroup.FirstOrDefault(x => x.Level == Level && x.InventoryUserID == Vessel.ID && x.StockCategoryID == -1 && x.StockFamilyID == FamilyFamilyID && x.Department == null && x.SubDepartment == null);
                    }

                    Resolved ??= ScopesInGroup.FirstOrDefault(x => x.Level == Level && x.InventoryUserID == Vessel.ID && x.StockCategoryID == -1 && x.StockFamilyID == -1 && x.Department == Requisition.Department && x.SubDepartment == null);
                    Resolved ??= ScopesInGroup.FirstOrDefault(x => x.Level == Level && x.InventoryUserID == Vessel.ID && x.StockCategoryID == -1 && x.StockFamilyID == -1 && x.Department == null && x.SubDepartment == null);

                    if (Family != null)
                    {
                        Resolved ??= ScopesInGroup.FirstOrDefault(x => x.Level == Level && x.InventoryUserID == null && x.StockCategoryID == FamilyStockCategoryID && x.StockFamilyID == FamilyFamilyID && x.Department == Requisition.Department && x.SubDepartment == Requisition.SubDepartment);
                        Resolved ??= ScopesInGroup.FirstOrDefault(x => x.Level == Level && x.InventoryUserID == null && x.StockCategoryID == FamilyStockCategoryID && x.StockFamilyID == FamilyFamilyID && x.Department == Requisition.Department && x.SubDepartment == null);
                        Resolved ??= ScopesInGroup.FirstOrDefault(x => x.Level == Level && x.InventoryUserID == null && x.StockCategoryID == FamilyStockCategoryID && x.StockFamilyID == FamilyFamilyID && x.Department == null && x.SubDepartment == null);
                        Resolved ??= ScopesInGroup.FirstOrDefault(x => x.Level == Level && x.InventoryUserID == null && x.StockCategoryID == FamilyStockCategoryID && x.StockFamilyID == -1 && x.Department == Requisition.Department && x.SubDepartment == null);
                        Resolved ??= ScopesInGroup.FirstOrDefault(x => x.Level == Level && x.InventoryUserID == null && x.StockCategoryID == FamilyStockCategoryID && x.StockFamilyID == -1 && x.Department == null && x.SubDepartment == null);
                        Resolved ??= ScopesInGroup.FirstOrDefault(x => x.Level == Level && x.InventoryUserID == null && x.StockCategoryID == -1 && x.StockFamilyID == FamilyFamilyID && x.Department == Requisition.Department && x.SubDepartment == null);
                        Resolved ??= ScopesInGroup.FirstOrDefault(x => x.Level == Level && x.InventoryUserID == null && x.StockCategoryID == -1 && x.StockFamilyID == FamilyFamilyID && x.Department == null && x.SubDepartment == null);
                    }

                    Resolved ??= ScopesInGroup.FirstOrDefault(x => x.Level == Level && x.InventoryUserID == null && x.StockCategoryID == -1 && x.StockFamilyID == -1 && x.Department == Requisition.Department && x.SubDepartment == null);
                    Resolved ??= ScopesInGroup.FirstOrDefault(x => x.Level == Level && x.InventoryUserID == null && x.StockCategoryID == -1 && x.StockFamilyID == -1 && x.Department == null && x.SubDepartment == null);

                    return Resolved;
                }

                // v2 match: every dimension is a wildcard when NULL. When the
                // requisition has a Group (Item Request V2 combined submission),
                // any row carrying that Group label is eligible — see the
                // identical branch in CheckApprover/ResolveApprovers above for
                // the full rationale.
                bool MatchesV2(UserApprovalScope2 s, Requisition Req, InventoryUser Vsl, int? CatId, int? FamId, int Lvl)
                {
                    if (s.IsActive == false) return false;
                    if (s.Level != null && s.Level != Lvl) return false;
                    if (s.VesselID != null && s.VesselID != Vsl.ID) return false;
                    if (s.VesselGroupID != null && s.VesselGroupID != Vsl.Group?.ID) return false;
                    if (s.CompanyDB != null && s.CompanyDB != Vsl.DB) return false;
                    if (s.Department != null && s.Department != Req.Department) return false;
                    if (s.SubDepartment != null && s.SubDepartment != Req.SubDepartment) return false;

                    if (!string.IsNullOrEmpty(Req.Group))
                    {
                        return s.Group == Req.Group;
                    }

                    if (s.StockCategoryID != null && s.StockCategoryID != CatId) return false;
                    if (s.StockFamilyID != null && s.StockFamilyID != FamId) return false;
                    return true;
                }

                // "All Groups" fallback: a row with Group == NULL applies to every
                // Group at that Level. Used per-level when the specific Group has
                // no row of its own at that Level.
                bool MatchesV2AllGroups(UserApprovalScope2 s, Requisition Req, InventoryUser Vsl, int Lvl)
                {
                    if (s.IsActive == false) return false;
                    if (s.Group != null) return false;
                    if (s.Level != null && s.Level != Lvl) return false;
                    if (s.VesselID != null && s.VesselID != Vsl.ID) return false;
                    if (s.VesselGroupID != null && s.VesselGroupID != Vsl.Group?.ID) return false;
                    if (s.CompanyDB != null && s.CompanyDB != Vsl.DB) return false;
                    if (s.Department != null && s.Department != Req.Department) return false;
                    if (s.SubDepartment != null && s.SubDepartment != Req.SubDepartment) return false;
                    return true;
                }

                // Group-combined documents: same rationale as CheckApprover above —
                // a Group's DB Level values aren't required to be contiguous from 1,
                // so [Level] here means "the Nth resolved approver" (position),
                // resolved by scanning+compacting every possible Level, not "the row
                // whose own Level field equals [Level]".
                List<UserApprovalScope2> ResolveGroupChain(Requisition Requisition, InventoryUser Vessel)
                {
                    var chain = new List<UserApprovalScope2>();
                    for (int lvl = 1; lvl <= 7; lvl++)
                    {
                        var r = ScopesV2
                            .Where(s => MatchesV2(s, Requisition, Vessel, null, null, lvl))
                            .OrderByDescending(s => s.Specificity)
                            .ThenBy(s => s.ID)
                            .FirstOrDefault();

                        r ??= ScopesV2
                            .Where(s => MatchesV2AllGroups(s, Requisition, Vessel, lvl))
                            .OrderByDescending(s => s.Specificity)
                            .ThenBy(s => s.ID)
                            .FirstOrDefault();

                        if (r != null) chain.Add(r);
                    }
                    return chain;
                }

                // Unified resolver: picks the v1 cascade or the v2 Specificity-based match
                // depending on the vessel's ApprovalRuleVersion flag, normalized to a plain
                // (UserID, display summary) pair since the two source tables are different
                // C# types.
                (int? UserId, object? Matched) ResolveApprover(Requisition Requisition, InventoryUser Vessel, int Level)
                {
                    if (Requisition.ApprovalRuleVersion == 2)
                    {
                        var Family = FamilyMap.FirstOrDefault(x => x.FamilyID == Requisition.CategoryID);
                        var ResolvedV2 = !string.IsNullOrEmpty(Requisition.Group)
                            ? ResolveGroupChain(Requisition, Vessel).ElementAtOrDefault(Level - 1)
                            : ScopesV2
                                .Where(s => MatchesV2(s, Requisition, Vessel, Family?.StockCategoryID, Family?.FamilyID, Level))
                                .OrderByDescending(s => s.Specificity)
                                .ThenBy(s => s.ID)
                                .FirstOrDefault();

                        object? MatchedV2 = ResolvedV2 == null ? null : new
                        {
                            ResolvedV2.ID,
                            ResolvedV2.VesselID,
                            ResolvedV2.VesselGroupID,
                            ResolvedV2.CompanyDB,
                            ResolvedV2.Group,
                            ResolvedV2.StockCategoryID,
                            ResolvedV2.StockFamilyID,
                            ResolvedV2.Department,
                            ResolvedV2.SubDepartment,
                        };
                        return (ResolvedV2?.UserID, MatchedV2);
                    }

                    var Resolved = ResolveScope(Requisition, Vessel, Vessel.Group.ID, Level);
                    object? Matched = Resolved == null ? null : new
                    {
                        Resolved.ID,
                        Resolved.InventoryUserID,
                        Resolved.StockCategoryID,
                        Resolved.StockFamilyID,
                        Resolved.Department,
                        Resolved.SubDepartment,
                    };
                    return (Resolved?.UserID, Matched);
                }

                object BuildItem(Requisition Requisition, InventoryUser Vessel, int? Level, object? MatchedScopeSummary, bool IsAdminOverride, string? AdminApprovedBy = null)
                {
                    var IsReverted = Requisition.RevertStatus == "REVERTED";

                    return new
                    {
                        Requisition.ID,
                        Requisition.RequisitionNumber,
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

                            if (Resolved.UserId == User.ID)
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
                        Offset = 1,
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
                        Offset = 1,
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