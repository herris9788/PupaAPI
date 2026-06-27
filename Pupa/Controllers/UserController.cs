using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pupa.BusinessObjects;
using Pupa.BusinessObjects.Beesuite;
using Pupa.ViewModels;
using System.Data;


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
            Requisition Requisition = null;
            if (Query.RequisitionNumber != null)
            {
                Requisition = await db.Requisition.FirstOrDefaultAsync(x => x.RequisitionNumber == Query.RequisitionNumber);
            }
            else if (Query.RequisitionID != null) {
                Requisition = await db.Requisition.FirstOrDefaultAsync(x => x.ID == Query.RequisitionID);
            }

            if (Requisition == null) return Ok(Array.Empty<object>());

            var Family = db.StockFamily.FirstOrDefault(x => x.FamilyID == Requisition.CategoryID);
            if (Family == null) return BadRequest();

            var Vessel = await db.InventoryUser.AsNoTracking()
                .Include(x => x.Group)
                .FirstOrDefaultAsync(x => x.ID == Requisition.VesselID);

            if (Vessel?.Group == null) return Ok(Array.Empty<object>());

            var VesselGroupId = Vessel.Group.ID;

            var Scopes = await db.UserApprovalScope.AsNoTracking()
                .Include(x => x.User)
                .Where(x => x.VesselGroupID == VesselGroupId)
                .ToListAsync();

            // Approver aktual selalu diambil dari ApprovedByN (berlaku utk mode App maupun Manual)
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

                // [1] InventoryUser + StockCategory + StockFamily + Department + SubDepartment
                ResolvedScope = Scopes.FirstOrDefault(x =>
                    x.Level == Level &&
                    x.InventoryUserID == Vessel.ID &&
                    x.StockCategoryID == Family.StockCategoryID &&
                    x.StockFamilyID == Family.FamilyID &&
                    x.Department == Requisition.Department &&
                    x.SubDepartment == Requisition.SubDepartment);

                // [2] InventoryUser + StockCategory + StockFamily + Department
                ResolvedScope ??= Scopes.FirstOrDefault(x =>
                    x.Level == Level &&
                    x.InventoryUserID == Vessel.ID &&
                    x.StockCategoryID == Family.StockCategoryID &&
                    x.StockFamilyID == Family.StockCategoryID &&
                    x.Department == Requisition.Department &&
                    x.SubDepartment == null);

                // [3] InventoryUser + StockCategory + StockFamily
                ResolvedScope ??= Scopes.FirstOrDefault(x =>
                    x.Level == Level &&
                    x.InventoryUserID == Vessel.ID &&
                    x.StockCategoryID == Family.StockCategoryID &&
                    x.StockFamilyID == Family.FamilyID &&
                    x.Department == null &&
                    x.SubDepartment == null);

                // [4] InventoryUser + StockCategory + Department
                ResolvedScope ??= Scopes.FirstOrDefault(x =>
                    x.Level == Level &&
                    x.InventoryUserID == Vessel.ID &&
                    x.StockCategoryID == Family.StockCategoryID &&
                    x.StockFamilyID == -1 &&
                    x.Department == Requisition.Department &&
                    x.SubDepartment == null);

                // [5] InventoryUser + StockCategory
                ResolvedScope ??= Scopes.FirstOrDefault(x =>
                    x.Level == Level &&
                    x.InventoryUserID == Vessel.ID &&
                    x.StockCategoryID == Family.StockCategoryID &&
                    x.StockFamilyID == -1 &&
                    x.Department == null &&
                    x.SubDepartment == null);

                // [6] InventoryUser + StockFamily + Department
                ResolvedScope ??= Scopes.FirstOrDefault(x =>
                    x.Level == Level &&
                    x.InventoryUserID == Vessel.ID &&
                    x.StockCategoryID == -1 &&
                    x.StockFamilyID == Requisition.CategoryID &&
                    x.Department == Requisition.Department &&
                    x.SubDepartment == null);

                // [7] InventoryUser + StockFamily
                ResolvedScope ??= Scopes.FirstOrDefault(x =>
                    x.Level == Level &&
                    x.InventoryUserID == Vessel.ID &&
                    x.StockCategoryID == -1 &&
                    x.StockFamilyID == Family.FamilyID &&
                    x.Department == null &&
                    x.SubDepartment == null);

                // [8] InventoryUser + Department
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

                // Global (tanpa InventoryUser)

                // [10] Global + StockCategory + StockFamily + Department + SubDepartment
                ResolvedScope ??= Scopes.FirstOrDefault(x =>
                    x.Level == Level &&
                    x.InventoryUserID == null &&
                    x.StockCategoryID == Family.StockCategoryID &&
                    x.StockFamilyID == Family.FamilyID &&
                    x.Department == Requisition.Department &&
                    x.SubDepartment == Requisition.SubDepartment);

                // [11] Global + StockCategory + StockFamily + Department
                ResolvedScope ??= Scopes.FirstOrDefault(x =>
                    x.Level == Level &&
                    x.InventoryUserID == null &&
                    x.StockCategoryID == Family.StockCategoryID &&
                    x.StockFamilyID == Family.FamilyID &&
                    x.Department == Requisition.Department &&
                    x.SubDepartment == null);

                // [12] Global + StockCategory + StockFamily
                ResolvedScope ??= Scopes.FirstOrDefault(x =>
                    x.Level == Level &&
                    x.InventoryUserID == null &&
                    x.StockCategoryID == Family.StockCategoryID &&
                    x.StockFamilyID == Family.FamilyID &&
                    x.Department == null &&
                    x.SubDepartment == null);

                // [13] Global + StockCategory + Department
                ResolvedScope ??= Scopes.FirstOrDefault(x =>
                    x.Level == Level &&
                    x.InventoryUserID == null &&
                    x.StockCategoryID == Family.StockCategoryID &&
                    x.StockFamilyID == -1 &&
                    x.Department == Requisition.Department &&
                    x.SubDepartment == null);

                // [14] Global + StockCategory
                ResolvedScope ??= Scopes.FirstOrDefault(x =>
                    x.Level == Level &&
                    x.InventoryUserID == null &&
                    x.StockCategoryID == Family.StockCategoryID &&
                    x.StockFamilyID == -1 &&
                    x.Department == null &&
                    x.SubDepartment == null);

                // [15] Global + StockFamily + Department
                ResolvedScope ??= Scopes.FirstOrDefault(x =>
                    x.Level == Level &&
                    x.InventoryUserID == null &&
                    x.StockCategoryID == -1 &&
                    x.StockFamilyID == Family.StockCategoryID &&
                    x.Department == Requisition.Department &&
                    x.SubDepartment == null);

                // [16] Global + StockFamily
                ResolvedScope ??= Scopes.FirstOrDefault(x =>
                    x.Level == Level &&
                    x.InventoryUserID == null &&
                    x.StockCategoryID == -1 &&
                    x.StockFamilyID == Family.StockCategoryID &&
                    x.Department == null &&
                    x.SubDepartment == null);

                // [17] Global + Department
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

                // ----------------------------------------------------------------
                // Output seragam untuk semua level, baik mode App maupun Manual
                // ----------------------------------------------------------------
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

            return Ok(ApprovalMatrix);
        }
    }
}
