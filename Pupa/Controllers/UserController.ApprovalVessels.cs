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
                User? User = null;
                if (Query.UserName != null)
                    User = await db.User.AsNoTracking().FirstOrDefaultAsync(x => x.Username.ToLower() == Query.UserName.ToLower());
                else if (Query.UserID != null)
                    User = await db.User.AsNoTracking().FirstOrDefaultAsync(x => x.ID == Query.UserID);
                if (User == null) throw new Exception("User not found");

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

                // ── Item: newest document date per vessel ────────────────────────
                var NewestDates = (await Reqs.GroupBy(x => x.VesselID!.Value)
                        .Select(g => new { VesselID = g.Key, Newest = g.Max(x => x.Date) })
                        .ToListAsync())
                    .ToDictionary(x => x.VesselID, x => x.Newest);

                // ── Item: Done / Cancel counts per vessel ────────────────────────
                var DoneQuery = Reqs.Where(x => x.Approved == true
                    && (x.RevertStatus == null || x.RevertStatus.ToLower() != "reverted"));
                if (!Team)
                {
                    // v2's last step is stored in ApprovedBy8 (v1 uses ApprovedBy7) — see ComputeItemDashboardCounts.
                    DoneQuery = DoneQuery.Where(x =>
                        (x.ApprovedBy1 != null && x.ApprovedBy1.ToLower() == MeLower) ||
                        (x.ApprovedBy2 != null && x.ApprovedBy2.ToLower() == MeLower) ||
                        (x.ApprovedBy3 != null && x.ApprovedBy3.ToLower() == MeLower) ||
                        (x.ApprovedBy4 != null && x.ApprovedBy4.ToLower() == MeLower) ||
                        (x.ApprovedBy5 != null && x.ApprovedBy5.ToLower() == MeLower) ||
                        (x.ApprovedBy6 != null && x.ApprovedBy6.ToLower() == MeLower) ||
                        (x.ApprovalRuleVersion == 2
                            ? (x.ApprovedBy8 != null && x.ApprovedBy8.ToLower() == MeLower)
                            : (x.ApprovedBy7 != null && x.ApprovedBy7.ToLower() == MeLower)));
                }
                var ItemDone = (await DoneQuery.GroupBy(x => x.VesselID!.Value)
                        .Select(g => new { VesselID = g.Key, N = g.Count() }).ToListAsync())
                    .ToDictionary(x => x.VesselID, x => x.N);

                var CancelQuery = Reqs.Where(x => x.Status != null
                    && (x.Status.ToLower() == "void" || x.Status.ToLower() == "rejected"));
                if (!Team) CancelQuery = CancelQuery.Where(x => x.RejectedBy == Me);
                var ItemCancel = (await CancelQuery.GroupBy(x => x.VesselID!.Value)
                        .Select(g => new { VesselID = g.Key, N = g.Count() }).ToListAsync())
                    .ToDictionary(x => x.VesselID, x => x.N);

                // ── Item: Pending ────────────────────────────────────────────────
                // Candidates = not approved, not void/rejected, not reverted (or reverted but already revised).
                var Candidates = Reqs.Where(x => x.Approved == false
                    && x.Status != null && x.Status.ToLower() != "void" && x.Status.ToLower() != "rejected"
                    && (x.LastRevertedBy == null || x.Revised == true));

                if (!Team)
                {
                    // Whose turn it is = the existing PendingApproval resolver (v1/v2 rules, ties, overrides).
                    var Resolved = await PendingApproval(new PendingApproverDTO { UserID = User.ID }) as OkObjectResult;
                    dynamic? Payload = Resolved?.Value;
                    var OwnTurnIds = new List<int>();
                    if (Payload != null && Payload.Success == true)
                    {
                        foreach (dynamic Item in Payload.Data.Items)
                        {
                            if (Item.IsAdminOverride != true && Item.IsReverted != true) OwnTurnIds.Add((int)Item.ID);
                        }
                    }
                    else
                    {
                        throw new Exception("Pending resolver failed");
                    }
                    Candidates = Candidates.Where(x => OwnTurnIds.Contains(x.ID));
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
                var RevertedRows = await Reqs
                    .Where(x => x.Approved == false && x.Revised != true && x.RevertStatus == "REVERTED"
                        && x.Status != null && x.Status.ToLower() != "void" && x.Status.ToLower() != "rejected")
                    .Select(x => new { x.ID, VesselID = x.VesselID!.Value, x.LastRevertedBy, x.RevertTargetAdmins })
                    .ToListAsync();

                var ItemReverted = new Dictionary<int, int>();
                if (RevertedRows.Count > 0)
                {
                    // Legacy documents (RevertTargetAdmins never filled): the newest revert LogActivity says who it went to.
                    var LegacyTargetedIds = new HashSet<int>();
                    if (!IsAdmin)
                    {
                        var LegacyIds = RevertedRows
                            .Where(r => string.IsNullOrWhiteSpace(r.RevertTargetAdmins)
                                && r.LastRevertedBy != null
                                && !string.Equals(r.LastRevertedBy, Me, StringComparison.OrdinalIgnoreCase))
                            .Select(r => r.ID).ToList();
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
                                        LegacyTargetedIds.Add(Newest.DocumentID);
                                }
                                catch { /* unparsable log entry: not addressed to anyone */ }
                            }
                        }
                    }

                    foreach (var R in RevertedRows)
                    {
                        bool Mine = IsAdmin
                            ? R.LastRevertedBy != null // ADMIN: every reverted document that has a reverter
                            : ( string.Equals(R.LastRevertedBy, Me, StringComparison.OrdinalIgnoreCase)
                            || NamesContain(R.RevertTargetAdmins, MeLower)
                            || LegacyTargetedIds.Contains(R.ID));
                        if (Mine) ItemReverted[R.VesselID] = (ItemReverted.TryGetValue(R.VesselID, out var C) ? C : 0) + 1;
                    }
                }

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
                    string App = (J.ApprovalStatus ?? "").ToLower();
                    string St = (J.Status ?? "").ToLower();
                    bool Reverted = string.Equals(J.RevertStatus, "REVERTED", StringComparison.OrdinalIgnoreCase);
                    if (App == "pending")
                    {
                        if (Team || OwnTurnJobIds.Contains(J.ID))
                        {
                            Bump(JobPending, J.VesselID);
                            Token(ReqNumbers, J.VesselID, J.ReportNo);
                        }
                    }
                    else if (App == "approved")
                    {
                        bool SignedByMe = new[] { J.ApprovedBy1, J.ApprovedBy2, J.ApprovedBy3, J.ApprovedBy4, J.ApprovedBy5, J.ApprovedBy6, J.ApprovedBy7 }
                            .Any(a => !string.IsNullOrWhiteSpace(a) && a.Trim().ToLower() == MeLower);
                        if ((Team || SignedByMe) && !Reverted) Bump(JobDone, J.VesselID);
                    }
                    else if (App == "rejected" || St == "void" || St == "rejected")
                    {
                        if (Team || string.Equals(J.RejectedBy, Me, StringComparison.OrdinalIgnoreCase)) Bump(JobCancel, J.VesselID);
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

                var Rows = Vessels
                    .Select(v => new
                    {
                        Vessel = new
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
                        },
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
                    // newest document first; vessels without documents last, alphabetical
                    .OrderByDescending(r => r.NewestDate.HasValue)
                    .ThenByDescending(r => r.NewestDate)
                    .ThenBy(r => (r.Vessel.InventoryUserName ?? "").ToLower())
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

        /// <summary>True when [csv] (a comma-separated list such as RevertTargetAdmins) holds exactly the name [nameLower]. Whole-name, case-insensitive: "ben" does not match "benny".</summary>
        private static bool NamesContain(string? csv, string nameLower)
        {
            if (string.IsNullOrWhiteSpace(csv) || string.IsNullOrWhiteSpace(nameLower)) return false;
            return csv.Split(',').Any(t => t.Trim().ToLower() == nameLower);
        }
    }
}
