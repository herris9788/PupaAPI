namespace Pupa.ViewModels
{
    public class ResolveApproversDTO
    {
        public int? VesselID { get; set; }
        // Confusingly named to match Requisition.CategoryID, which actually
        // stores a StockFamily.FamilyID, not a StockCategory id.
        public int? CategoryID { get; set; }
        public string? Department { get; set; }
        public string? SubDepartment { get; set; }
    }
}
