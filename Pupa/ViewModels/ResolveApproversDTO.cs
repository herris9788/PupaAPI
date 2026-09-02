namespace Pupa.ViewModels
{
    public class ResolveApproversDTO
    {
        public int? VesselID { get; set; }
        // Confusingly named to match Requisition.CategoryID, which actually
        // stores a StockFamily.FamilyID, not a StockCategory id.
        public int? CategoryID { get; set; }
        // Job Request: categories are a plain-text label (JobDetail.Category,
        // e.g. "BALLAST WATER MANAGEMENT SYSTEM (BWMS) SAMPLING & ANALYSIS"),
        // not tied to the StockFamily catalog Item Requisitions use, so there
        // is no numeric CategoryID to pass. When CategoryID is absent and this
        // is set, it's resolved to a StockCategory by name (case/whitespace-
        // insensitive) and matched at the category level only (no family) —
        // if CategoryID is also given, it wins and Category is ignored.
        public string? Category { get; set; }
        public string? Department { get; set; }
        public string? SubDepartment { get; set; }
        // Item Request V2: when set, resolve the combined approval chain for
        // this Group instead of the per-Family cascade (CategoryID is ignored
        // when Group is non-empty).
        public string? Group { get; set; }
    }
}
