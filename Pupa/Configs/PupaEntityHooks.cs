using System.Security.Claims;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Models.CustomOData;
using Pupa.BusinessObjects;
using Pupa.BusinessObjects.Beesuite;

namespace Pupa.Configs
{
    /// <summary>
    /// Pupa-specific side effects of the generic CRUD controller that lives in DevOps.Models
    /// (<c>PupaDataController&lt;T&gt;</c>). Entity types stay in this project.
    /// </summary>
    public class PupaEntityHooks : IPupaEntityHooks
    {
        private sealed record ScopeState(int? VesselId, short? Level);

        public async Task OnCreatingAsync(DbContext ctx, object entity)
        {
            var _db = (BeesuiteDbContext)ctx;
            if (entity is InventoryUser newVessel &&
                (newVessel.InventoryUserID == null || newVessel.InventoryUserID == 0) &&
                !string.IsNullOrWhiteSpace(newVessel.DB))
            {
                // InventoryUserID is NOT NULL and UNIQUE per DB in Postgres
                // (Ascend.IC_InventoryUsers, constraint Unique_DB_InventoryUserID), but the
                // "Add Vessel" dialog cannot safely compute the next value itself (a client-side
                // guess could race a concurrent Add). Assign the next value for this DB here.
                var maxId = await _db.InventoryUser
                    .Where(x => x.DB == newVessel.DB)
                    .MaxAsync(x => (int?)x.InventoryUserID) ?? 0;
                newVessel.InventoryUserID = maxId + 1;
            }
        }

        public async Task OnCreatedAsync(DbContext ctx, object entity)
        {
            if (entity is UserApprovalScope2 v2Created && v2Created.VesselID != null)
            {
                await SyncLegacyMirrorForVessel((BeesuiteDbContext)ctx, v2Created.VesselID.Value, v2Created.Level);
            }
        }

        public Task<PupaHookGate> OnPatchingAsync(DbContext ctx, object entity, JsonElement patch, ClaimsPrincipal user)
        {
            // CanViewAllVesselDocuments is a global visibility override (see its doc-comment on
            // Pupa.BusinessObjects.Beesuite.User) that only a SYSADMIN may grant. SYSADMIN2 can edit
            // every other field on this dialog but must never change this one.
            if (entity is Pupa.BusinessObjects.Beesuite.User &&
                patch.EnumerateObject().Any(p => string.Equals(
                    p.Name, nameof(Pupa.BusinessObjects.Beesuite.User.CanViewAllVesselDocuments),
                    StringComparison.OrdinalIgnoreCase)))
            {
                var callerRole = user?.FindFirst(ClaimTypes.Role)?.Value;
                if (!string.Equals(callerRole, "SYSADMIN", StringComparison.OrdinalIgnoreCase))
                {
                    return Task.FromResult(PupaHookGate.Deny());
                }
            }

            // Captured BEFORE the patch so a PATCH that moves a UserApprovalScope2 row to/from a
            // different VesselID or Level re-syncs BOTH the old and new vessel+level's mirror.
            object state = entity is UserApprovalScope2 s ? new ScopeState(s.VesselID, s.Level) : null;
            return Task.FromResult(PupaHookGate.Allow(state));
        }

        public async Task OnPatchedAsync(DbContext ctx, object entity, object state)
        {
            if (entity is UserApprovalScope2 v2Patched && state is ScopeState pre)
            {
                var db = (BeesuiteDbContext)ctx;
                if (pre.VesselId != null)
                {
                    await SyncLegacyMirrorForVessel(db, pre.VesselId.Value, pre.Level);
                }
                if (v2Patched.VesselID != null &&
                    (v2Patched.VesselID != pre.VesselId || v2Patched.Level != pre.Level))
                {
                    await SyncLegacyMirrorForVessel(db, v2Patched.VesselID.Value, v2Patched.Level);
                }
            }
        }

        public Task<object> OnDeletingAsync(DbContext ctx, object entity)
        {
            var _db = (BeesuiteDbContext)ctx;

            // N8N.LSAWebhookAttachment has no DB-level FK: delete the children manually.
            if (entity is LSAWebhook webhook)
            {
                var atts = _db.Set<LSAWebhookAttachment>().Where(a => a.WebhookID == webhook.ID);
                _db.Set<LSAWebhookAttachment>().RemoveRange(atts);
            }

            // Captured before removal -- see SyncLegacyMirrorForVessel.
            object state = entity is UserApprovalScope2 s ? new ScopeState(s.VesselID, s.Level) : null;
            return Task.FromResult(state);
        }

        public async Task OnDeletedAsync(DbContext ctx, object entity, object state)
        {
            if (state is ScopeState del && del.VesselId != null)
            {
                await SyncLegacyMirrorForVessel((BeesuiteDbContext)ctx, del.VesselId.Value, del.Level);
            }
        }

        /// <summary>
        /// Keeps a legacy UserApprovalScope (v1) row in sync with a
        /// full-wildcard UserApprovalScope2 (v2) row scoped to one specific
        /// vessel — so the ALREADY-PUBLISHED mobile app (whose Job Request
        /// approver lookup only ever reads UserApprovalScope, hardcoding
        /// StockCategoryID/StockFamilyID to -1, and never reads
        /// UserApprovalScope2 at all) picks up a v2 "All Categories, All
        /// Families" rule without needing an app update/store release.
        ///
        /// Deliberately narrow scope:
        ///  - Only mirrors when the v2 row is a full wildcard for stock
        ///    (StockCategoryID AND StockFamilyID both null). A
        ///    category-specific v2 rule can never be represented this way —
        ///    the old mobile code discards any non-(-1) row for Job
        ///    Requests by design (hardcoded client logic); no backend/data
        ///    trick can change that without a new app build.
        ///  - Only mirrors when the v2 row targets one specific VesselID
        ///    (not a whole VesselGroupID) — a group-wide v2 rule would need
        ///    one v1 row per CompanyDB actually used in that group, which
        ///    this method doesn't attempt.
        /// Re-derives (rather than diffs) the mirror row for
        /// [vesselId]/[level] from whatever currently-active v2 row best
        /// matches, so edits/deletes/reactivations on the v2 side stay
        /// correctly reflected without separate add/remove bookkeeping.
        ///
        /// KNOWN SIDE EFFECT: because UserApprovalScope's StockCategoryID/
        /// StockFamilyID = -1 is a wildcard for EVERY consumer of that table
        /// (not just Job Requests), this mirror row also becomes a
        /// fallback approver for Item Requisitions on this vessel+level if
        /// the primary backend resolver (/User/Approver) is ever
        /// unreachable and the old mobile app falls back to local
        /// UserApprovalScope matching. Acceptable for now — vessels here
        /// are ApprovalRuleVersion 2 already, so that fallback path is not
        /// the normal one — but worth knowing before mirroring is applied
        /// more broadly.
        /// </summary>
        private static async Task SyncLegacyMirrorForVessel(BeesuiteDbContext _db, int vesselId, short? level)
        {
            if (level == null) return;

            // .Include(Group) is required here: InventoryUser.GroupID is a
            // legacy scalar column that is NOT the referenced group's PK (it
            // repeats across multiple real InventoryUserGroup rows in
            // different companies — e.g. GroupID=6 exists on 4 different
            // groups). Every matcher (ResolveApprovers, PendingApproval,
            // MatchesV2) compares UserApprovalScope(2).VesselGroupID against
            // Vessel.Group.ID — the navigated PK — not Vessel.GroupID.
            // Confirmed against real data: MV. AMETHYST has GroupID=6 but its
            // actual InventoryUserGroup.ID is 50.
            var vessel = await _db.InventoryUser.AsNoTracking()
                .Include(x => x.Group)
                .FirstOrDefaultAsync(x => x.ID == vesselId);
            if (vessel?.Group == null || string.IsNullOrEmpty(vessel.DB))
            {
                // Can't resolve what the old mobile query needs (VesselGroupID +
                // CompanyDB) — nothing safe to mirror.
                return;
            }

            // Most-specific currently-active v2 row that is STILL a full
            // wildcard for this exact vessel+level (mirrors the resolver's own
            // Specificity ordering, so this stays correct even if a
            // more-specific non-wildcard row is later added at the same level —
            // that row would win in ResolveApprovers, and should stop being
            // mirrored here too).
            var stillWildcard = await _db.UserApprovalScope2.AsNoTracking()
                .Where(s => s.IsActive != false && s.Level == level && s.VesselID == vesselId
                         && s.StockCategoryID == null && s.StockFamilyID == null)
                .OrderByDescending(s => s.Specificity).ThenBy(s => s.ID)
                .FirstOrDefaultAsync();

            // NOTE: the old mobile app's local match compares this column
            // against JobRequest.VesselInventoryUserRowID directly — which,
            // despite the name, holds InventoryUser.ID (the PK, == vesselId
            // here), NOT InventoryUser.InventoryUserID (a separate, unrelated
            // legacy column). Confirmed against real data before writing this.
            var mirrorRow = await _db.UserApprovalScope
                .FirstOrDefaultAsync(r => r.InventoryUserID == vesselId
                                       && r.VesselGroupID == vessel.Group.ID
                                       && r.CompanyDB == vessel.DB
                                       && r.StockCategoryID == -1 && r.StockFamilyID == -1
                                       && r.Level == level
                                       && r.Department == null && r.SubDepartment == null);

            if (stillWildcard?.UserID == null)
            {
                if (mirrorRow != null) _db.UserApprovalScope.Remove(mirrorRow);
                return;
            }

            if (mirrorRow == null)
            {
                _db.UserApprovalScope.Add(new UserApprovalScope
                {
                    UserID = stillWildcard.UserID,
                    CompanyDB = vessel.DB,
                    VesselGroupID = vessel.Group.ID,
                    InventoryUserID = vesselId,
                    StockCategoryID = -1,
                    StockFamilyID = -1,
                    Level = level,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                });
            }
            else if (mirrorRow.UserID != stillWildcard.UserID)
            {
                mirrorRow.UserID = stillWildcard.UserID;
                mirrorRow.UpdatedAt = DateTime.Now;
            }
        }
    }
}
