using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pupa.BusinessObjects.Beesuite;
using Pupa.ViewModels;

namespace Pupa.Controllers
{
    public partial class UserController
    {
        /// <summary>
        /// ONE call for everything the BeeSuite web Dashboard's stat cards show (it used to fire a few dozen
        /// requests on load: Dashboard/Summary, the user + vessel lookups, N pages of /odata/Requisition and
        /// /odata/JobRequest, the pending resolver, per-job approver resolution, the reverted counters, and for ADMIN
        /// the "pending on master" scan with one scope lookup per document).
        ///
        ///   GET beesuite/api/User/Dashboard/Overview?UserName=
        ///
        /// New route on purpose: Dashboard/Summary and Dashboard/ItemCounts stay exactly as they are because
        /// installed mobile builds still call them. This one only adds on top of them (it calls DashboardSummary
        /// in-process, so its numbers can never drift from it).
        ///
        /// What comes back depends on the user's role:
        ///   SYSADMIN   -> Counts { Users, Vessels, ApprovalRules }
        ///   ACCOUNTING -> Counts { COA }
        ///   REQUESTER / REPAIR_ADMINISTRATOR / REPAIR_MANAGER -> nothing extra (those dashboards fetch elsewhere)
        ///   anyone else -> Requester + Approver (+ PendingByMaster for ADMIN), already in the exact shape the
        ///                  cards use:
        ///     Requester.Pending  = Summary pending minus the reverted ones (those have their own Reverted card)
        ///     Requester.Reverted = own Item Requests currently REVERTED (not void/rejected)
        ///     Approver.Pending   = Summary Approver.Pending: Item own-turn (PendingApproval, admin-override and
        ///                          reverted entries excluded) + Job own-turn (GetPendingJobRequestsForApprover)
        ///     Approver.Reverted / Total = Summary + the reverts ADDRESSED to this user (RevertTargetAdmins, or the
        ///                          legacy LogActivity target) that Summary does not already count — same rule the
        ///                          app applied client-side (PendingApprovalsHelper.countRevertedToUserExtra).
        ///     PendingByMaster    = ADMIN only: pending Item Requests on the admin's vessels whose CURRENT approval
        ///                          turn belongs to an account whose username starts with "master".
        /// The BeesuiteGO repair numbers (service orders, attendance, service reports) live in another service and
        /// are not part of this.
        /// </summary>
        [HttpGet("Dashboard/Overview")]
        public async Task<IActionResult> DashboardOverview([FromQuery] PendingApproverDTO Query)
        {
            try
            {
                // Live numbers: never let a browser or proxy keep a copy.
                Response.Headers["Cache-Control"] = "no-store, no-cache";
                var User = await ResolveApprovalUser(Query);
                string Me = User.Username;
                string Role = User.Role ?? "";

                if (Role == "SYSADMIN")
                {
                    return Ok(new
                    {
                        Success = true,
                        Message = "OK",
                        Data = new
                        {
                            UserName = Me,
                            Role,
                            Counts = new
                            {
                                Users = await db.User.CountAsync(),
                                Vessels = await db.InventoryUser.CountAsync(x => x.IsVessel == true),
                                ApprovalRules = await db.Approval.CountAsync(),
                            }
                        }
                    });
                }
                if (Role == "ACCOUNTING")
                {
                    return Ok(new
                    {
                        Success = true,
                        Message = "OK",
                        Data = new { UserName = Me, Role, Counts = new { COA = await db.StockFamilyCOA.CountAsync() } }
                    });
                }
                if (Role == "REQUESTER" || Role == "REPAIR_ADMINISTRATOR" || Role == "REPAIR_MANAGER")
                {
                    return Ok(new { Success = true, Message = "OK", Data = new { UserName = Me, Role } });
                }

                bool IsAdmin = Role == "ADMIN";

                // Base numbers: the existing Summary, in-process.
                var SummaryResult = await DashboardSummary(new PendingApproverDTO { UserID = User.ID }) as OkObjectResult;
                dynamic? S = SummaryResult?.Value;
                if (S == null || S.Success != true) throw new Exception("Dashboard summary failed");

                int ReqPendingRaw = (int)S.Data.Requester.Pending;
                int ReqDone = (int)S.Data.Requester.Done;
                int ReqCancel = (int)S.Data.Requester.Cancel;
                int ReqTotal = (int)S.Data.Requester.Total;
                int AppPending = (int)S.Data.Approver.Pending;
                int AppDone = (int)S.Data.Approver.Done;
                int AppCancel = (int)S.Data.Approver.Cancel;
                int AppReverted = (int)S.Data.Approver.Reverted;
                int AppTotal = (int)S.Data.Approver.Total;

                // Requester.Reverted: Summary's Pending still includes reverted documents (their Status stays
                // PENDING), so they are split out here instead of being counted in both cards.
                int ReqReverted = await db.Requisition.CountAsync(x =>
                    x.RequestBy == Me && x.RevertStatus == "REVERTED"
                    && x.Status != null && x.Status.ToLower() != "void" && x.Status.ToLower() != "rejected");
                int ReqPending = Math.Max(0, ReqPendingRaw - ReqReverted);

                var ScopeVesselIds = await db.UserVesselRel.AsNoTracking()
                    .Where(x => x.UserID == User.ID)
                    .Select(x => x.VesselID)
                    .Distinct()
                    .ToListAsync();

                int RevertedToMeExtra = await CountRevertedTargetedToMeExtra(User, IsAdmin, ScopeVesselIds);

                int? PendingByMaster = null;
                if (IsAdmin)
                {
                    try { PendingByMaster = await CountPendingOnMaster(ScopeVesselIds); }
                    catch { PendingByMaster = null; } // the client falls back to its own scan for this one card
                }

                return Ok(new
                {
                    Success = true,
                    Message = "OK",
                    Data = new
                    {
                        UserName = Me,
                        Role,
                        Requester = new
                        {
                            Pending = ReqPending,
                            Reverted = ReqReverted,
                            Done = ReqDone,
                            Cancel = ReqCancel,
                            Total = ReqTotal,
                        },
                        Approver = new
                        {
                            Pending = AppPending,
                            Done = AppDone,
                            Cancel = AppCancel,
                            Reverted = AppReverted + RevertedToMeExtra,
                            Total = AppTotal + RevertedToMeExtra,
                        },
                        PendingByMaster,
                    }
                });
            }
            catch (Exception Ex)
            {
                return BadRequest(new { Success = false, Message = Ex.Message, Data = (object?)null });
            }
        }

        /// <summary>
        /// Still-reverted Item Requests ADDRESSED to [user] (RevertTargetAdmins names them whole-name, or — legacy rows
        /// with that column empty — the newest revert LogActivity does), regardless of role, vessel access or chain,
        /// minus the ones Dashboard/Summary already counts: the ones [user] reverted themselves, and for ADMIN the ones
        /// on a vessel in [scopeVesselIds] whose approval chain contains them.
        /// </summary>
        private async Task<int> CountRevertedTargetedToMeExtra(User user, bool isAdmin, List<int> scopeVesselIds)
        {
            string Me = user.Username;
            string MeLower = Me.Trim().ToLower();

            var Rows = await ItemsRevertedBase(db.Requisition.AsNoTracking())
                .Select(x => new { x.ID, x.VesselID, x.LastRevertedBy, x.RevertTargetAdmins })
                .ToListAsync();
            if (Rows.Count == 0) return 0;

            // Same membership test as the Approvals page's non-admin Reverted bucket: reverted by me OR addressed to me.
            var Mine = await RevertedMineIds(
                Rows.Select(r => (r.ID, r.LastRevertedBy, r.RevertTargetAdmins)).ToList(), Me, MeLower, false);
            var Targeted = Rows
                .Where(r => Mine.Contains(r.ID)
                    && (r.LastRevertedBy ?? "").Trim().ToLower() != MeLower)
                .ToList();
            if (Targeted.Count == 0) return 0;
            if (!isAdmin) return Targeted.Count;

            // ADMIN: Summary already counted the ones on their vessels where they sit in the approval chain.
            var InScopeIds = Targeted
                .Where(r => r.VesselID.HasValue && scopeVesselIds.Contains(r.VesselID.Value))
                .Select(r => r.ID)
                .ToList();
            if (InScopeIds.Count == 0) return Targeted.Count;

            var Docs = await db.Requisition.AsNoTracking()
                .Include(x => x.InventoryUser).ThenInclude(v => v!.Group)
                .Where(x => InScopeIds.Contains(x.ID))
                .ToListAsync();
            var GroupIds = Docs.Where(x => x.InventoryUser?.Group != null)
                .Select(x => x.InventoryUser!.Group!.ID).Distinct().ToList();
            var ScopesV1 = await db.UserApprovalScope.AsNoTracking()
                .Include(x => x.User)
                .Where(x => GroupIds.Contains(x.VesselGroupID.Value))
                .ToListAsync();
            var MandatoryVessels = await db.MandatoryVessel.AsNoTracking().ToListAsync();
            var MandatoryApprovers = await db.MandatoryVesselApprover.AsNoTracking().ToListAsync();
            var GroupMappings = await db.ItemGroupMapping.AsNoTracking().ToListAsync();
            var UserGroups = await db.UserApprovalGroup.AsNoTracking().ToListAsync();
            var UsersByUsernameLower = await db.User.AsNoTracking().ToDictionaryAsync(x => x.Username.ToLower(), x => x);
            var FamilyMap = await db.StockFamily.AsNoTracking().ToListAsync();

            int AlreadyCounted = 0;
            foreach (var Doc in Docs)
            {
                if (Doc.InventoryUser?.Group == null) continue;
                var Family = FamilyMap.FirstOrDefault(x => x.FamilyID == Doc.CategoryID);
                var Usernames = GetFullApproverUsernames(Doc, Doc.InventoryUser, Family, ScopesV1,
                    MandatoryVessels, MandatoryApprovers, GroupMappings, UserGroups, UsersByUsernameLower);
                if (Usernames.Contains(MeLower)) AlreadyCounted++;
            }
            return Targeted.Count - AlreadyCounted;
        }

        /// <summary>
        /// ADMIN card "Pending Requests": pending Item Requests on the admin's vessels (not reverted, not approved from
        /// the app) whose CURRENT approval turn — the lowest level nobody has signed yet — belongs to an account whose
        /// username starts with "master". The chain per document comes from the same ResolveApprovers every other
        /// resolver uses (v1/v2 aware); documents sharing vessel/category/department/group share one resolution.
        /// </summary>
        private async Task<int> CountPendingOnMaster(List<int> scopeVesselIds)
        {
            if (scopeVesselIds.Count == 0) return 0;

            var Docs = await db.Requisition.AsNoTracking()
                .Where(x => x.VesselID.HasValue && scopeVesselIds.Contains(x.VesselID.Value)
                    && x.Status == "PENDING"
                    && (x.ApprovedFromApp == null || x.ApprovedFromApp == false)
                    && (x.RevertStatus == null || x.RevertStatus.ToLower() != "reverted"))
                .ToListAsync();

            var ChainCache = new Dictionary<string, Dictionary<int, List<string>>>();
            int Count = 0;
            foreach (var Doc in Docs)
            {
                var Key = $"{Doc.VesselID}|{Doc.CategoryID}|{Doc.Department}|{Doc.SubDepartment}|{Doc.Group}";
                if (!ChainCache.TryGetValue(Key, out var Chain))
                {
                    Chain = new Dictionary<int, List<string>>();
                    try
                    {
                        var Resolved = await ResolveApprovers(new ResolveApproversDTO
                        {
                            VesselID = Doc.VesselID,
                            CategoryID = Doc.CategoryID,
                            Department = Doc.Department,
                            SubDepartment = Doc.SubDepartment,
                            Group = Doc.Group,
                        }) as OkObjectResult;
                        dynamic? Payload = Resolved?.Value;
                        if (Payload != null && Payload.Success == true)
                        {
                            foreach (dynamic Item in Payload.Data.Items)
                            {
                                List<string> Names = Item.Usernames;
                                Chain[(int)Item.Level] = Names;
                            }
                        }
                    }
                    catch
                    {
                        // One unresolvable vessel/category shouldn't take down the whole count.
                    }
                    ChainCache[Key] = Chain;
                }

                foreach (var Level in Chain.Keys.OrderBy(l => l))
                {
                    // v2 labels its 7th step "8" (see V2LevelLabel); GetRequisitionApprovedByName takes the position.
                    int Position = Level == 8 ? 7 : Level;
                    if (!string.IsNullOrWhiteSpace(GetRequisitionApprovedByName(Doc, Position))) continue;
                    if (Chain[Level].Any(u => !string.IsNullOrEmpty(u) && u.StartsWith("master", StringComparison.OrdinalIgnoreCase)))
                        Count++;
                    break; // first unsigned level = the current turn
                }
            }
            return Count;
        }
    }
}
