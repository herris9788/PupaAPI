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

                var ApprovalMatrix = new List<object>();
                var ApproverCount = Requisition.ApprovalMaxLevel;

                for (int i = 0; i < ApproverCount; i++)
                {
                    var Level = i + 1;
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

                    var ActualApprover = GetActualApproverName(Level);

                    ApprovalMatrix.Add(new
                    {
                        Level,
                        Found = ResolvedScope != null,
                        ShouldBeApproverUserID = ResolvedScope?.UserID,
                        ShouldBeApproverUsername = ResolvedScope?.User?.Username,
                        ActualApprovedBy = ActualApprover,
                        IsActuallyApproved = !string.IsNullOrWhiteSpace(ActualApprover),
                        MatchedScope = ResolvedScope == null ? null : new
                        {
                            ResolvedScope.ID,
                            ResolvedScope.InventoryUserID,
                            ResolvedScope.StockCategoryID,
                            ResolvedScope.StockFamilyID,
                            ResolvedScope.Department,
                            ResolvedScope.SubDepartment,
                        }
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

                // Semua scope yang dimiliki user ini (lintas VesselGroup)
                var MyScopes = await db.UserApprovalScope.AsNoTracking()
                    .Where(x => x.UserID == User.ID)
                    .ToListAsync();

                if (!MyScopes.Any())
                {
                    return Ok(new
                    {
                        Success = true,
                        Message = "OK",
                        Data = new { Items = new List<object>(), TotalCount = 0, Offset = 1, Limit = 0 }
                    });
                }

                var MyVesselGroupIDs = MyScopes.Select(x => x.VesselGroupID).Distinct().ToList();

                // Semua scope di VesselGroup-VesselGroup yang relevan (sekali query saja)
                var Scopes = await db.UserApprovalScope.AsNoTracking()
                    .Include(x => x.User)
                    .Where(x => MyVesselGroupIDs.Contains(x.VesselGroupID))
                    .ToListAsync();

                // Semua vessel yang ada di vessel group milik user ini
                var Vessels = await db.InventoryUser.AsNoTracking()
                    .Include(x => x.Group)
                    .Where(x => x.Group != null && MyVesselGroupIDs.Contains(x.Group.ID))
                    .ToListAsync();

                var VesselIDs = Vessels.Select(x => x.ID).ToList();

                // Semua requisition pada vessel-vessel tersebut
                var Requisitions = await db.Requisition.Include(x=> x.InventoryUser).AsNoTracking()
                    .Where(x => VesselIDs.Contains(x.VesselID.Value) && x.Status == "PENDING" && x.RequisitionNumber.Substring(0,2) != "SO")
                    .ToListAsync();

                // Preload StockFamily sekali saja (hindari query berulang di dalam loop)
                var FamilyMap = await db.StockFamily.AsNoTracking().ToListAsync();

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

                // Resolusi scope untuk 1 requisition di 1 level tertentu.
                // Urutan rule [1]-[18] sama persis dengan endpoint CheckApprover.
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

                var PendingList = new List<object>();

                foreach (var Requisition in Requisitions)
                {
                    var Vessel = Vessels.FirstOrDefault(x => x.ID == Requisition.VesselID);
                    if (Vessel?.Group == null) continue;

                    // Cari level pertama yang belum diapprove (current pending level)
                    int? PendingLevel = null;
                    for (int i = 1; i <= Requisition.ApprovalMaxLevel; i++)
                    {
                        if (string.IsNullOrWhiteSpace(GetActualApproverName(Requisition, i)))
                        {
                            PendingLevel = i;
                            break;
                        }
                    }

                    if (PendingLevel == null) continue; // sudah fully approved, skip

                    var ResolvedScope = ResolveScope(Requisition, Vessel, Vessel.Group.ID, PendingLevel.Value);

                    // Hanya masukkan kalau scope yang resolve memang user ini
                    if (ResolvedScope?.UserID == User.ID)
                    {
                        PendingList.Add(new
                        {
                            Requisition.ID,
                            Requisition.RequisitionNumber,
                            PendingLevel,
                            Requisition.Department,
                            Requisition.SubDepartment,
                            Vessel = new
                            {
                                ID = Requisition.InventoryUser.ID,
                                DB = Requisition.InventoryUser.DB,
                                InventoryUserID = Requisition.InventoryUser.InventoryUserID,
                                InventoryUserCode = Requisition.InventoryUser.InventoryUserCode,
                                InventoryUserName= Requisition.InventoryUser.InventoryUserName
                            },
                            VesselID = Vessel.ID,
                            VesselGroupID = Vessel.Group.ID,
                            MatchedScope = new
                            {
                                ResolvedScope.ID,
                                ResolvedScope.InventoryUserID,
                                ResolvedScope.StockCategoryID,
                                ResolvedScope.StockFamilyID,
                                ResolvedScope.Department,
                                ResolvedScope.SubDepartment,
                            }
                        });
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