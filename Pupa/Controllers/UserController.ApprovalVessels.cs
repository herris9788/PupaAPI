using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pupa.BusinessObjects.Beesuite;
using Pupa.ViewModels;
using System.Text.Json;

namespace Pupa.Controllers
{
    public partial class UserController
    {
        /// <summary>
        /// ONE call for the Approvals page's vessel list (what BeeSuite's /approvals used to build from
        /// ~3 requests per vessel plus a few unbounded list fetches, all counted client-side). Returns,
        /// per vessel the user can see, only the numbers and search words the page shows — no document rows.
        ///
        ///   GET beesuite/api/User/Approval/Vessels?UserName=&amp;Scope=my|team
        ///
        /// Scope "my" = documents waiting for THIS user's signature / signed or rejected by them; "team" =
        /// everything on those vessels (admin oversight). Vessels: SYSADMIN = every vessel, anyone else = the
        /// UserVesselRel ones. Rules are the ones the page applied before, reusing the existing server
        /// resolvers (PendingApproval for item turns, GetPendingJobRequestsForApprover for job turns):
        ///   Item  Pending  = not approved, not void/rejected, not reverted (or already revised) — "my" narrows it to
        ///                    the items PendingApproval says are this user's own turn (admin-override / reverted
        ///                    entries excluded).
        ///         Done     = fully approved and not reverted (+ "my": signed by the user at any level).
        ///         Cancel   = void/rejected (+ "my": RejectedBy = the user).
        ///         Reverted = still reverted (not revised, not void/rejected); ADMIN sees every reverted one,
        ///                    anyone else the ones they reverted / were addressed in RevertTargetAdmins (whole-name
        ///                    match) / legacy ones whose newest revert LogActivity targets them.
        ///   Job (PUPA) Pending / Done / Cancel — same split, Pending "my" = the user's own turn.
        /// The BeesuiteGO repair-job pending count lives in another service and is not part of this.
        /// </summary>
        [HttpGet("Approval/Vessels")]
        public async Task<IActionResult> ApprovalVessels([FromQuery] PendingApproverDTO Query, [FromQuery] string? Scope)
        {
            try
            {
                // These numbers must always be live: never let a browser or proxy keep a copy.
                Response.Headers["Cache-Control"] = "no-store, no-cache";
                var User = await ResolveApprovalUser(Query);
                bool Team = string.Equals(Scope, "team", StringComparison.OrdinalIgnoreCase);
                string Me = User.Username;
                string MeLower = Me.ToLower();
                bool IsAdmin = User.Role == "ADMIN";
                bool IsSysAdmin = User.Role == "SYSADMIN";

                // ── Vessels in scope ─────────────────────────────────────────────
                IQueryable<InventoryUser> VesselQuery = db.InventoryUser.AsNoTracking().Include(x => x.Group);
                if (IsSysAdmin)
                {
                    VesselQuery = VesselQuery.Where(x => x.IsVessel == true);
                }
                else
                {
                    var RelIds = await db.UserVesselRel.AsNoTracking()
                        .Where(x => x.UserID == User.ID)
                        .Select(x => x.VesselID)
                        .Distinct()
                        .ToListAsync();
                    VesselQuery = VesselQuery.Where(x => RelIds.Contains(x.ID));
                }
                var Vessels = await VesselQuery.ToListAsync();
                var VesselIds = Vessels.Select(v => v.ID).ToList();
                if (VesselIds.Count == 0)
                    return Ok(new { Success = true, Message = "OK", Data = new { Scope = Team ? "team" : "my", Vessels = new List<object>() } });

                var Reqs = db.Requisition.AsNoTracking()
                    .Where(x => x.VesselID.HasValue && VesselIds.Contains(x.VesselID.Value));

                // ── Item: newest document date, Done / Cancel counts per vessel ──
                var NewestDates = (await Reqs.GroupBy(x => x.VesselID!.Value)
                        .Select(g => new { VesselID = g.Key, Newest = g.Max(x => x.Date) })
                        .ToListAsync())
                    .ToDictionary(x => x.VesselID, x => x.Newest);
                var ItemDone = (await ItemsDone(Reqs, Team, MeLower).GroupBy(x => x.VesselID!.Value)
                        .Select(g => new { VesselID = g.Key, N = g.Count() }).ToListAsync())
                    .ToDictionary(x => x.VesselID, x => x.N);
                var ItemCancel = (await ItemsCancel(Reqs, Team, Me).GroupBy(x => x.VesselID!.Value)
                        .Select(g => new { VesselID = g.Key, N = g.Count() }).ToListAsync())
                    .ToDictionary(x => x.VesselID, x => x.N);

                // ── Item: Pending ("my" = the user's own turn) ───────────────────
                var Candidates = ItemsPendingCandidates(Reqs);
                if (!Team)
                {
                    var OwnTurn = await OwnTurnItemIds(User);
                    Candidates = Candidates.Where(x => OwnTurn.Contains(x.ID));
                }
                // Only the few columns the page shows/searches — not whole rows with expands.
                var PendingRows = await Candidates
                    .Select(x => new
                    {
                        x.ID,
                        VesselID = x.VesselID!.Value,
                        x.RequisitionNumber,
                        x.Group,
                        CategoryName = x.Category != null && x.Category.StockCategory != null ? x.Category.StockCategory.StockCategoryName : null,
                        FamilyName = x.Category != null ? x.Category.FamilyName : null
                    })
                    .ToListAsync();

                // ── Item: Reverted ───────────────────────────────────────────────
                var RevertedRows = await ItemsRevertedBase(Reqs)
                    .Select(x => new { x.ID, VesselID = x.VesselID!.Value, x.LastRevertedBy, x.RevertTargetAdmins })
                    .ToListAsync();
                var RevertedMine = await RevertedMineIds(
                    RevertedRows.Select(r => (r.ID, r.LastRevertedBy, r.RevertTargetAdmins)).ToList(), Me, MeLower, IsAdmin);
                var ItemReverted = new Dictionary<int, int>();
                foreach (var R in RevertedRows)
                    if (RevertedMine.Contains(R.ID)) ItemReverted[R.VesselID] = (ItemReverted.TryGetValue(R.VesselID, out var C) ? C : 0) + 1;

                // ── Job Request (PUPA) ───────────────────────────────────────────
                var Jobs = await db.JobRequest.AsNoTracking()
                    .Where(j => j.Type == "PUPA" && j.VesselInventoryUserRowID.HasValue
                        && VesselIds.Contains(j.VesselInventoryUserRowID.Value))
                    .Select(j => new
                    {
                        j.ID,
                        VesselID = j.VesselInventoryUserRowID!.Value,
                        j.ReportNo,
                        j.ApprovalStatus,
                        j.Status,
                        j.RejectedBy,
                        j.RevertStatus,
                        j.ApprovedBy1, j.ApprovedBy2, j.ApprovedBy3, j.ApprovedBy4, j.ApprovedBy5, j.ApprovedBy6, j.ApprovedBy7
                    })
                    .ToListAsync();

                HashSet<int> OwnTurnJobIds = Team
                    ? new HashSet<int>()
                    : (await GetPendingJobRequestsForApprover(User, VesselIds)).Select(j => j.ID).ToHashSet();

                var JobPending = new Dictionary<int, int>();
                var JobDone = new Dictionary<int, int>();
                var JobCancel = new Dictionary<int, int>();
                var ReqNumbers = new Dictionary<int, List<string>>();
                var CatFam = new Dictionary<int, List<string>>();
                void Bump(Dictionary<int, int> d, int k) => d[k] = (d.TryGetValue(k, out var c) ? c : 0) + 1;
                void Token(Dictionary<int, List<string>> d, int k, string? v)
                {
                    if (string.IsNullOrWhiteSpace(v)) return;
                    if (!d.TryGetValue(k, out var l)) d[k] = l = new List<string>();
                    var t = v.Trim().ToLower();
                    if (!l.Contains(t)) l.Add(t);
                }

                foreach (var J in Jobs)
                {
                    switch (JobBucket(J.ApprovalStatus, J.Status, J.RejectedBy, J.RevertStatus,
                        new[] { J.ApprovedBy1, J.ApprovedBy2, J.ApprovedBy3, J.ApprovedBy4, J.ApprovedBy5, J.ApprovedBy6, J.ApprovedBy7 }, Team, Me, MeLower))
                    {
                        case "pending":
                            if (Team || OwnTurnJobIds.Contains(J.ID))
                            {
                                Bump(JobPending, J.VesselID);
                                Token(ReqNumbers, J.VesselID, J.ReportNo);
                            }
                            break;
                        case "done": Bump(JobDone, J.VesselID); break;
                        case "cancel": Bump(JobCancel, J.VesselID); break;
                    }
                }

                var ItemPending = new Dictionary<int, int>();
                foreach (var R in PendingRows)
                {
                    Bump(ItemPending, R.VesselID);
                    Token(ReqNumbers, R.VesselID, R.RequisitionNumber);
                    Token(CatFam, R.VesselID, R.CategoryName);
                    Token(CatFam, R.VesselID, R.FamilyName);
                    Token(CatFam, R.VesselID, R.Group);
                }

                int Get(Dictionary<int, int> d, int k) => d.TryGetValue(k, out var v) ? v : 0;

                // newest document first; vessels without documents last, alphabetical
                var Rows = Vessels
                    .OrderByDescending(v => NewestDates.TryGetValue(v.ID, out var Nd) && Nd.HasValue)
                    .ThenByDescending(v => NewestDates.TryGetValue(v.ID, out var Nd) ? Nd : null)
                    .ThenBy(v => (v.InventoryUserName ?? "").ToLower())
                    .Select(v => new
                    {
                        Vessel = VesselDto(v),
                        NewestDate = NewestDates.TryGetValue(v.ID, out var D) ? D : null,
                        Item = new
                        {
                            Pending = Get(ItemPending, v.ID),
                            Done = Get(ItemDone, v.ID),
                            Cancel = Get(ItemCancel, v.ID),
                            Reverted = Get(ItemReverted, v.ID)
                        },
                        Job = new
                        {
                            Pending = Get(JobPending, v.ID),
                            Done = Get(JobDone, v.ID),
                            Cancel = Get(JobCancel, v.ID)
                        },
                        ReqNumbers = ReqNumbers.TryGetValue(v.ID, out var RN) ? RN : new List<string>(),
                        CatFam = CatFam.TryGetValue(v.ID, out var CF) ? CF : new List<string>()
                    })
                    .ToList();

                return Ok(new
                {
                    Success = true,
                    Message = "OK",
                    Data = new { Scope = Team ? "team" : "my", Vessels = Rows }
                });
            }
            catch (Exception Ex)
            {
                return BadRequest(new { Success = false, Message = Ex.Message, Data = (object?)null });
            }
        }

        /// <summary>
        /// ONE call for a vessel's Approvals detail page (the page used to make ~15: vessel, v2 group maps, three count
        /// queries, the pending resolver twice, every job of the vessel with attachments, reverted, the list...).
        ///
        ///   GET beesuite/api/User/Approval/VesselOrders?UserName=&amp;VesselID=&amp;Scope=my|team&amp;Tab=pending|done|cancel|reverted&amp;Page=0&amp;PageSize=20
        ///
        /// Returns the vessel, the four tab counts (item + job combined, same rules as Approval/Vessels) and only the
        /// cards of the requested tab — Done/Cancel are paged (jobs ride on page 0, as before), Pending/Reverted are
        /// complete. Cards carry just what the card shows; on a v2 vessel each item card also carries its resolved
        /// approval Group name (GroupLabel), so the page no longer downloads the group mapping tables.
        /// Repair-job approvals live in BeesuiteGO and are not included.
        /// </summary>
        [HttpGet("Approval/VesselOrders")]
        public async Task<IActionResult> ApprovalVesselOrders([FromQuery] PendingApproverDTO Query, [FromQuery] int VesselID,
            [FromQuery] string? Scope, [FromQuery] string? Tab, [FromQuery] int Page = 0, [FromQuery] int PageSize = 20)
        {
            try
            {
                // These numbers must always be live: never let a browser or proxy keep a copy.
                Response.Headers["Cache-Control"] = "no-store, no-cache";
                var User = await ResolveApprovalUser(Query);
                bool Team = string.Equals(Scope, "team", StringComparison.OrdinalIgnoreCase);
                string Me = User.Username;
                string MeLower = Me.ToLower();
                bool IsAdmin = User.Role == "ADMIN";
                string TabKey = (Tab ?? "pending").Trim().ToLower();
                if (TabKey != "pending" && TabKey != "done" && TabKey != "cancel" && TabKey != "reverted") TabKey = "pending";
                Page = Math.Max(0, Page);
                PageSize = Math.Clamp(PageSize, 1, 100);

                var Vessel = await db.InventoryUser.AsNoTracking().Include(x => x.Group).FirstOrDefaultAsync(x => x.ID == VesselID)
                    ?? throw new Exception("Vessel not found");
                var Reqs = db.Requisition.AsNoTracking().Where(x => x.VesselID == VesselID);

                // ── Item sets (same rules as Approval/Vessels) ───────────────────
                var Pending = ItemsPendingCandidates(Reqs);
                if (!Team)
                {
                    var OwnTurn = await OwnTurnItemIds(User);
                    Pending = Pending.Where(x => OwnTurn.Contains(x.ID));
                }
                var Done = ItemsDone(Reqs, Team, MeLower);
                var Cancel = ItemsCancel(Reqs, Team, Me);
                var RevertedRows = await ItemsRevertedBase(Reqs)
                    .Select(x => new { x.ID, x.LastRevertedBy, x.RevertTargetAdmins }).ToListAsync();
                var RevertedIds = (await RevertedMineIds(
                    RevertedRows.Select(r => (r.ID, r.LastRevertedBy, r.RevertTargetAdmins)).ToList(), Me, MeLower, IsAdmin)).ToList();

                int ItemPendingN = await Pending.CountAsync();
                int ItemDoneN = await Done.CountAsync();
                int ItemCancelN = await Cancel.CountAsync();

                // ── Job Request (PUPA) buckets ───────────────────────────────────
                var JobRows = await db.JobRequest.AsNoTracking()
                    .Where(j => j.Type == "PUPA" && j.VesselInventoryUserRowID == VesselID)
                    .Select(j => new
                    {
                        j.ID, j.ApprovalStatus, j.Status, j.RejectedBy, j.RevertStatus,
                        j.ApprovedBy1, j.ApprovedBy2, j.ApprovedBy3, j.ApprovedBy4, j.ApprovedBy5, j.ApprovedBy6, j.ApprovedBy7
                    })
                    .ToListAsync();
                HashSet<int> OwnTurnJobIds = Team
                    ? new HashSet<int>()
                    : (await GetPendingJobRequestsForApprover(User, new List<int> { VesselID })).Select(j => j.ID).ToHashSet();
                var JobPendingIds = new List<int>();
                var JobDoneIds = new List<int>();
                var JobCancelIds = new List<int>();
                foreach (var J in JobRows)
                {
                    switch (JobBucket(J.ApprovalStatus, J.Status, J.RejectedBy, J.RevertStatus,
                        new[] { J.ApprovedBy1, J.ApprovedBy2, J.ApprovedBy3, J.ApprovedBy4, J.ApprovedBy5, J.ApprovedBy6, J.ApprovedBy7 }, Team, Me, MeLower))
                    {
                        case "pending": if (Team || OwnTurnJobIds.Contains(J.ID)) JobPendingIds.Add(J.ID); break;
                        case "done": JobDoneIds.Add(J.ID); break;
                        case "cancel": JobCancelIds.Add(J.ID); break;
                    }
                }

                // ── The requested tab's cards ────────────────────────────────────
                IQueryable<Requisition> ListQuery = TabKey switch
                {
                    "done" => Done.OrderByDescending(x => x.Date).Skip(Page * PageSize).Take(PageSize),
                    "cancel" => Cancel.OrderByDescending(x => x.Date).Skip(Page * PageSize).Take(PageSize),
                    "reverted" => Reqs.Where(x => RevertedIds.Contains(x.ID)).OrderByDescending(x => x.LastRevertedAt),
                    _ => Pending.OrderBy(x => x.Date)
                };
                var Items = await ListQuery.Select(x => new
                {
                    x.ID,
                    x.RequisitionNumber,
                    x.Date,
                    x.Group,
                    x.Spec,
                    x.RequestBy,
                    x.Department,
                    x.SubDepartment,
                    x.Status,
                    x.Approved,
                    x.ApprovedBy1, x.ApprovedBy2, x.ApprovedBy3, x.ApprovedBy4, x.ApprovedBy5,
                    x.ApprovedBy1At, x.ApprovedBy2At, x.ApprovedBy3At, x.ApprovedBy4At, x.ApprovedBy5At, x.ApprovedBy6At, x.ApprovedBy7At, x.ApprovedBy8At,
                    ItemCount = x.RequisitionDetails.Count(),
                    FamilyID = x.CategoryID,
                    FamilyName = x.Category != null ? x.Category.FamilyName : null,
                    StockCategoryName = x.Category != null && x.Category.StockCategory != null ? x.Category.StockCategory.StockCategoryName : null
                }).ToListAsync();

                // v2 vessel: the approval Group name of each family (highest configured level wins, lowest GroupID on a tie).
                var FamilyLabels = new Dictionary<int, string>();
                if (Vessel.ApprovalRuleVersion == 2)
                {
                    var FamIds = Items.Where(i => i.FamilyID.HasValue).Select(i => i.FamilyID!.Value).Distinct().ToList();
                    if (FamIds.Count > 0)
                    {
                        var Maps = await db.ItemGroupMapping.AsNoTracking()
                            .Where(m => FamIds.Contains(m.FamilyID))
                            .Select(m => new { m.FamilyID, m.GroupID, m.GroupName }).ToListAsync();
                        var GroupIds = Maps.Select(m => m.GroupID).Distinct().ToList();
                        var MaxLevel = (await db.UserApprovalGroup.AsNoTracking()
                                .Where(g => GroupIds.Contains(g.GroupID))
                                .GroupBy(g => g.GroupID)
                                .Select(g => new { GroupID = g.Key, Max = g.Max(x => (int)x.Level) }).ToListAsync())
                            .ToDictionary(g => g.GroupID, g => g.Max);
                        foreach (var Fam in Maps.GroupBy(m => m.FamilyID))
                        {
                            var Cands = new Dictionary<int, string>();
                            foreach (var M in Fam)
                                if (!string.IsNullOrWhiteSpace(M.GroupName)) Cands[M.GroupID] = M.GroupName.Trim();
                            if (Cands.Count == 0) continue;
                            var Best = Cands.Keys
                                .OrderByDescending(g => MaxLevel.TryGetValue(g, out var L) ? L : 0)
                                .ThenBy(g => g).First();
                            FamilyLabels[Fam.Key] = Cands[Best];
                        }
                    }
                }

                var ItemCards = Items.Select(i => new
                {
                    i.ID, i.RequisitionNumber, i.Date, i.Group, i.Spec, i.RequestBy, i.Department, i.SubDepartment, i.Status, i.Approved,
                    i.ApprovedBy1, i.ApprovedBy2, i.ApprovedBy3, i.ApprovedBy4, i.ApprovedBy5,
                    i.ApprovedBy1At, i.ApprovedBy2At, i.ApprovedBy3At, i.ApprovedBy4At, i.ApprovedBy5At, i.ApprovedBy6At, i.ApprovedBy7At, i.ApprovedBy8At,
                    i.ItemCount,
                    GroupLabel = i.FamilyID.HasValue && FamilyLabels.TryGetValue(i.FamilyID.Value, out var Gl) ? Gl : null,
                    Category = new { i.FamilyID, i.FamilyName, StockCategory = new { i.StockCategoryName } }
                }).ToList();

                List<int> JobIds = TabKey switch
                {
                    "pending" => JobPendingIds,
                    "done" => Page == 0 ? JobDoneIds : new List<int>(),
                    "cancel" => Page == 0 ? JobCancelIds : new List<int>(),
                    _ => new List<int>()
                };
                var JobCards = new List<object>();
                if (JobIds.Count > 0)
                {
                    var Jobs = await db.JobRequest.AsNoTracking()
                        .Include(j => j.Jobs).ThenInclude(jb => jb.JobDetails)
                        .Where(j => JobIds.Contains(j.ID)).ToListAsync();
                    foreach (var J in Jobs)
                    {
                        var First = J.Jobs?.OrderBy(x => x.ID).FirstOrDefault();
                        JobCards.Add(new
                        {
                            J.ID,
                            J.ReportNo,
                            J.CreatedAt,
                            J.CreatedBy,
                            J.ApprovalStatus,
                            JobCount = J.Jobs?.Count ?? 0,
                            Category = First?.JobDetails?.OrderBy(d => d.JobID).FirstOrDefault()?.Category
                        });
                    }
                }

                return Ok(new
                {
                    Success = true,
                    Message = "OK",
                    Data = new
                    {
                        Scope = Team ? "team" : "my",
                        Tab = TabKey,
                        Page,
                        PageSize,
                        Vessel = VesselDto(Vessel),
                        Counts = new
                        {
                            Pending = ItemPendingN + JobPendingIds.Count,
                            Done = ItemDoneN + JobDoneIds.Count,
                            Cancel = ItemCancelN + JobCancelIds.Count,
                            Reverted = RevertedIds.Count
                        },
                        Items = ItemCards,
                        Jobs = JobCards
                    }
                });
            }
            catch (Exception Ex)
            {
                return BadRequest(new { Success = false, Message = Ex.Message, Data = (object?)null });
            }
        }

        private static object VesselDto(InventoryUser v) => new
        {
            v.ID,
            v.DB,
            v.InventoryUserID,
            v.InventoryUserCode,
            v.InventoryUserName,
            v.Department,
            v.HODUserName,
            v.UserName,
            v.CoState,
            CoUID = v.CoUID?.ToString(),
            v.Disabled,
            v.IsVessel,
            v.GroupID,
            v.Remarks,
            v.Specification,
            v.IMO,
            v.DWT,
            v.Size,
            v.KKM,
            v.Mualim,
            v.AutoRequester,
            v.Flag,
            v.ApprovalRuleVersion,
            Group = v.Group == null ? null : new { v.Group.ID, v.Group.DB, v.Group.GroupID, v.Group.GroupCode, v.Group.GroupName }
        };

        // ═════════ Shared rules (used by Approval/Vessels AND Approval/VesselOrders, so both always agree) ═════════

        private async Task<User> ResolveApprovalUser(PendingApproverDTO Query)
        {
            User? U = null;
            if (Query.UserName != null)
                U = await db.User.AsNoTracking().FirstOrDefaultAsync(x => x.Username.ToLower() == Query.UserName.ToLower());
            else if (Query.UserID != null)
                U = await db.User.AsNoTracking().FirstOrDefaultAsync(x => x.ID == Query.UserID);
            return U ?? throw new Exception("User not found");
        }

        /// <summary>Fully approved, not reverted; "my" scope also needs the user to have signed a level (v2's last step is ApprovedBy8, v1's ApprovedBy7).</summary>
        private static IQueryable<Requisition> ItemsDone(IQueryable<Requisition> q, bool team, string meLower)
        {
            q = q.Where(x => x.Approved == true && (x.RevertStatus == null || x.RevertStatus.ToLower() != "reverted"));
            if (team) return q;
            return q.Where(x =>
                (x.ApprovedBy1 != null && x.ApprovedBy1.ToLower() == meLower) ||
                (x.ApprovedBy2 != null && x.ApprovedBy2.ToLower() == meLower) ||
                (x.ApprovedBy3 != null && x.ApprovedBy3.ToLower() == meLower) ||
                (x.ApprovedBy4 != null && x.ApprovedBy4.ToLower() == meLower) ||
                (x.ApprovedBy5 != null && x.ApprovedBy5.ToLower() == meLower) ||
                (x.ApprovedBy6 != null && x.ApprovedBy6.ToLower() == meLower) ||
                (x.ApprovalRuleVersion == 2
                    ? (x.ApprovedBy8 != null && x.ApprovedBy8.ToLower() == meLower)
                    : (x.ApprovedBy7 != null && x.ApprovedBy7.ToLower() == meLower)));
        }

        /// <summary>Void / rejected; "my" scope only the ones this user rejected.</summary>
        private static IQueryable<Requisition> ItemsCancel(IQueryable<Requisition> q, bool team, string me)
        {
            q = q.Where(x => x.Status != null && (x.Status.ToLower() == "void" || x.Status.ToLower() == "rejected"));
            return team ? q : q.Where(x => x.RejectedBy == me);
        }

        /// <summary>Not approved, not void/rejected, not reverted (or reverted but already revised).</summary>
        private static IQueryable<Requisition> ItemsPendingCandidates(IQueryable<Requisition> q)
            => q.Where(x => x.Approved == false
                && x.Status != null && x.Status.ToLower() != "void" && x.Status.ToLower() != "rejected"
                && (x.LastRevertedBy == null || x.Revised == true));

        /// <summary>Items whose turn is this user's: the existing PendingApproval resolver, minus admin-override and reverted entries.</summary>
        private async Task<List<int>> OwnTurnItemIds(User user)
        {
            var Resolved = await PendingApproval(new PendingApproverDTO { UserID = user.ID }) as OkObjectResult;
            dynamic? Payload = Resolved?.Value;
            if (Payload == null || Payload.Success != true) throw new Exception("Pending resolver failed");
            var Ids = new List<int>();
            foreach (dynamic Item in Payload.Data.Items)
                if (Item.IsAdminOverride != true && Item.IsReverted != true) Ids.Add((int)Item.ID);
            return Ids;
        }

        private static IQueryable<Requisition> ItemsRevertedBase(IQueryable<Requisition> q)
            => q.Where(x => x.Approved == false && x.Revised != true && x.RevertStatus == "REVERTED"
                && x.Status != null && x.Status.ToLower() != "void" && x.Status.ToLower() != "rejected");

        /// <summary>
        /// Of the still-reverted documents [rows], the IDs that belong in this user's Reverted bucket. ADMIN: every one
        /// that has a reverter. Anyone else: ones they reverted, ones whose RevertTargetAdmins names them (whole-name),
        /// and legacy ones (RevertTargetAdmins never filled) whose newest revert LogActivity targets them.
        /// </summary>
        private async Task<HashSet<int>> RevertedMineIds(List<(int ID, string? LastRevertedBy, string? Targets)> rows, string Me, string MeLower, bool IsAdmin)
        {
            var Mine = new HashSet<int>();
            if (rows.Count == 0) return Mine;
            if (IsAdmin)
            {
                foreach (var r in rows) if (r.LastRevertedBy != null) Mine.Add(r.ID);
                return Mine;
            }
            var LegacyIds = rows
                .Where(r => string.IsNullOrWhiteSpace(r.Targets) && r.LastRevertedBy != null
                    && !string.Equals(r.LastRevertedBy, Me, StringComparison.OrdinalIgnoreCase))
                .Select(r => r.ID).ToList();
            var Legacy = new HashSet<int>();
            if (LegacyIds.Count > 0)
            {
                var Logs = await db.LogActivity.AsNoTracking()
                    .Where(l => l.DocumentID.HasValue && LegacyIds.Contains(l.DocumentID.Value)
                        && (l.ModuleName == "ItemRequest" || l.ModuleName == "Requisition")
                        && (l.ActivityType == "REVERT" || l.ActivityType == "Reverted" || l.ActivityType == "REVERT_IR"))
                    .OrderByDescending(l => l.CreatedAt)
                    .Select(l => new { DocumentID = l.DocumentID!.Value, l.NewValue })
                    .ToListAsync();
                foreach (var Newest in Logs.GroupBy(l => l.DocumentID).Select(g => g.First()))
                {
                    try
                    {
                        using var Doc = JsonDocument.Parse(Newest.NewValue ?? "");
                        if (Doc.RootElement.ValueKind == JsonValueKind.Object
                            && Doc.RootElement.TryGetProperty("RevertTargetAdmins", out var T)
                            && T.ValueKind == JsonValueKind.String
                            && NamesContain(T.GetString(), MeLower))
                            Legacy.Add(Newest.DocumentID);
                    }
                    catch { /* unparsable log entry: not addressed to anyone */ }
                }
            }
            foreach (var r in rows)
                if (string.Equals(r.LastRevertedBy, Me, StringComparison.OrdinalIgnoreCase)
                    || NamesContain(r.Targets, MeLower) || Legacy.Contains(r.ID))
                    Mine.Add(r.ID);
            return Mine;
        }

        /// <summary>Which bucket a Job Request (PUPA) falls in: "pending" (still to be signed — the caller decides whose turn), "done", "cancel" or null.</summary>
        private static string? JobBucket(string? approvalStatus, string? status, string? rejectedBy, string? revertStatus,
            IEnumerable<string?> approvedBy, bool team, string me, string meLower)
        {
            string App = (approvalStatus ?? "").ToLower();
            string St = (status ?? "").ToLower();
            if (App == "pending") return "pending";
            if (App == "approved")
            {
                bool Reverted = string.Equals(revertStatus, "REVERTED", StringComparison.OrdinalIgnoreCase);
                bool Signed = approvedBy.Any(a => !string.IsNullOrWhiteSpace(a) && a.Trim().ToLower() == meLower);
                return (team || Signed) && !Reverted ? "done" : null;
            }
            if (App == "rejected" || St == "void" || St == "rejected")
                return team || string.Equals(rejectedBy, me, StringComparison.OrdinalIgnoreCase) ? "cancel" : null;
            return null;
        }

        /// <summary>True when [csv] (a comma-separated list such as RevertTargetAdmins) holds exactly the name [nameLower]. Whole-name, case-insensitive: "ben" does not match "benny".</summary>
        private static bool NamesContain(string? csv, string nameLower)
        {
            if (string.IsNullOrWhiteSpace(csv) || string.IsNullOrWhiteSpace(nameLower)) return false;
            return csv.Split(',').Any(t => t.Trim().ToLower() == nameLower);
        }
    }
}
