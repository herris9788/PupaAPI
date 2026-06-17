using System;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pupa.BusinessObjects.Beesuite
{
    [Table("IC_StockCategories", Schema = "Ascend")]
    public class StockCategory : BaseEntity
    {
        public StockCategory()
        {
            this.Items = new ObservableCollection<Item>();
            this.StockFamilies = new ObservableCollection<StockFamily>();
            this.Approvals = new ObservableCollection<Approval>();
        }

        private int? _StockCategoryID;
        private string? _StockCategoryName;
        private int? _InventoryAccountID;
        private int? _SalesAccountID;
        private int? _SalesReturnAccountID;
        private int? _SalesDiscountAccountID;
        private int? _PurchaseAccountID;
        private int? _PurchaseReturnAccountID;
        private int? _PurchaseDiscountAccountID;
        private int? _UsageAccountID;
        private int? _UsageReturnAccountID;
        private int? _InventoryBeginningAccountID;
        private int? _InventoryEndingAccountID;
        private int? _COGAccountID;
        private string? _CoState = "New";
        private Guid? _CoUID = Guid.NewGuid();
        private Guid? _CoPkgID;
        private Guid? _CoSourcePkgID;
        private int? _DisplaySequence;
        private string? _StockCategoryCode = Guid.NewGuid().ToString();
        private int? _AdjustmentAccountID;
        private string? _COGDepartment = "";
        private bool? _PORequireDirectorApproval;
        private int? _ProjectPurchaseAccountID;
        private string? _PerformanceCriteria = "";
        private decimal? _PointValue;
        private int? _PurchaseNonStockAccountID;
        private string? _InvoiceTypes = "";
        private bool? _DontPostUsage;
        private bool? _MustImportFromPI;
        private bool? _Capex;
        private bool? _ShowInBudget;
        private decimal? _TolerancePct;
        private string? _ItemTypes = "All";
        private int? _AdjustmentPosAccountID;
        private bool? _PRByLogisticOnly;
        private int? _MasterCategoryID;
        private int? _Flags;
        private bool? _SCDisabled;
        private decimal? _CustomVal1;
        private string? _SCFlags = "";
        private int? _ServiceDeptID;
        private bool? _SCFPostToAsset;
        private string? _Config = "";
        private string? _SOTypes = "";
        private string? _SCRemarks = "";
        private int? _SCWarehouseID;
        private string? _AdjustmentDepartment = "";

        [Key]
        public virtual int? StockCategoryID { get { return _StockCategoryID; } set { OnPropertyChanging(); _StockCategoryID = value; OnPropertyChanged(); } }
        public virtual string? StockCategoryName { get { return _StockCategoryName; } set { OnPropertyChanging(); _StockCategoryName = value; OnPropertyChanged(); } }
        public virtual int? InventoryAccountID { get { return _InventoryAccountID; } set { OnPropertyChanging(); _InventoryAccountID = value; OnPropertyChanged(); } }
        public virtual int? SalesAccountID { get { return _SalesAccountID; } set { OnPropertyChanging(); _SalesAccountID = value; OnPropertyChanged(); } }
        public virtual int? SalesReturnAccountID { get { return _SalesReturnAccountID; } set { OnPropertyChanging(); _SalesReturnAccountID = value; OnPropertyChanged(); } }
        public virtual int? SalesDiscountAccountID { get { return _SalesDiscountAccountID; } set { OnPropertyChanging(); _SalesDiscountAccountID = value; OnPropertyChanged(); } }
        public virtual int? PurchaseAccountID { get { return _PurchaseAccountID; } set { OnPropertyChanging(); _PurchaseAccountID = value; OnPropertyChanged(); } }
        public virtual int? PurchaseReturnAccountID { get { return _PurchaseReturnAccountID; } set { OnPropertyChanging(); _PurchaseReturnAccountID = value; OnPropertyChanged(); } }
        public virtual int? PurchaseDiscountAccountID { get { return _PurchaseDiscountAccountID; } set { OnPropertyChanging(); _PurchaseDiscountAccountID = value; OnPropertyChanged(); } }
        public virtual int? UsageAccountID { get { return _UsageAccountID; } set { OnPropertyChanging(); _UsageAccountID = value; OnPropertyChanged(); } }
        public virtual int? UsageReturnAccountID { get { return _UsageReturnAccountID; } set { OnPropertyChanging(); _UsageReturnAccountID = value; OnPropertyChanged(); } }

        public virtual int? InventoryBeginningAccountID { get { return _InventoryBeginningAccountID; } set { OnPropertyChanging(); _InventoryBeginningAccountID = value; OnPropertyChanged(); } }

        public virtual int? InventoryEndingAccountID { get { return _InventoryEndingAccountID; } set { OnPropertyChanging(); _InventoryEndingAccountID = value; OnPropertyChanged(); } }

        public virtual int? COGAccountID { get { return _COGAccountID; } set { OnPropertyChanging(); _COGAccountID = value; OnPropertyChanged(); } }

        public virtual string? CoState { get { return _CoState; } set { OnPropertyChanging(); _CoState = value; OnPropertyChanged(); } }

        public virtual Guid? CoUID { get { return _CoUID; } set { OnPropertyChanging(); _CoUID = value; OnPropertyChanged(); } }

        public virtual Guid? CoPkgID { get { return _CoPkgID; } set { OnPropertyChanging(); _CoPkgID = value; OnPropertyChanged(); } }

        public virtual Guid? CoSourcePkgID { get { return _CoSourcePkgID; } set { OnPropertyChanging(); _CoSourcePkgID = value; OnPropertyChanged(); } }

        public virtual int? DisplaySequence { get { return _DisplaySequence; } set { OnPropertyChanging(); _DisplaySequence = value; OnPropertyChanged(); } }

        public virtual string? StockCategoryCode { get { return _StockCategoryCode; } set { OnPropertyChanging(); _StockCategoryCode = value; OnPropertyChanged(); } }

        public virtual int? AdjustmentAccountID { get { return _AdjustmentAccountID; } set { OnPropertyChanging(); _AdjustmentAccountID = value; OnPropertyChanged(); } }

        public virtual string? COGDepartment { get { return _COGDepartment; } set { OnPropertyChanging(); _COGDepartment = value; OnPropertyChanged(); } }

        public virtual bool? PORequireDirectorApproval { get { return _PORequireDirectorApproval; } set { OnPropertyChanging(); _PORequireDirectorApproval = value; OnPropertyChanged(); } }

        public virtual int? ProjectPurchaseAccountID { get { return _ProjectPurchaseAccountID; } set { OnPropertyChanging(); _ProjectPurchaseAccountID = value; OnPropertyChanged(); } }

        public virtual string? PerformanceCriteria { get { return _PerformanceCriteria; } set { OnPropertyChanging(); _PerformanceCriteria = value; OnPropertyChanged(); } }

        public virtual decimal? PointValue { get { return _PointValue; } set { OnPropertyChanging(); _PointValue = value; OnPropertyChanged(); } }

        public virtual int? PurchaseNonStockAccountID { get { return _PurchaseNonStockAccountID; } set { OnPropertyChanging(); _PurchaseNonStockAccountID = value; OnPropertyChanged(); } }

        public virtual string? InvoiceTypes { get { return _InvoiceTypes; } set { OnPropertyChanging(); _InvoiceTypes = value; OnPropertyChanged(); } }

        public virtual bool? DontPostUsage { get { return _DontPostUsage; } set { OnPropertyChanging(); _DontPostUsage = value; OnPropertyChanged(); } }

        public virtual bool? MustImportFromPI { get { return _MustImportFromPI; } set { OnPropertyChanging(); _MustImportFromPI = value; OnPropertyChanged(); } }

        public virtual bool? Capex { get { return _Capex; } set { OnPropertyChanging(); _Capex = value; OnPropertyChanged(); } }

        public virtual bool? ShowInBudget { get { return _ShowInBudget; } set { OnPropertyChanging(); _ShowInBudget = value; OnPropertyChanged(); } }

        public virtual decimal? TolerancePct { get { return _TolerancePct; } set { OnPropertyChanging(); _TolerancePct = value; OnPropertyChanged(); } }

        public virtual string? ItemTypes { get { return _ItemTypes; } set { OnPropertyChanging(); _ItemTypes = value; OnPropertyChanged(); } }

        public virtual int? AdjustmentPosAccountID { get { return _AdjustmentPosAccountID; } set { OnPropertyChanging(); _AdjustmentPosAccountID = value; OnPropertyChanged(); } }

        public virtual bool? PRByLogisticOnly { get { return _PRByLogisticOnly; } set { OnPropertyChanging(); _PRByLogisticOnly = value; OnPropertyChanged(); } }

        public virtual int? MasterCategoryID { get { return _MasterCategoryID; } set { OnPropertyChanging(); _MasterCategoryID = value; OnPropertyChanged(); } }

        public virtual int? Flags { get { return _Flags; } set { OnPropertyChanging(); _Flags = value; OnPropertyChanged(); } }

        public virtual bool? SCDisabled { get { return _SCDisabled; } set { OnPropertyChanging(); _SCDisabled = value; OnPropertyChanged(); } }

        public virtual decimal? CustomVal1 { get { return _CustomVal1; } set { OnPropertyChanging(); _CustomVal1 = value; OnPropertyChanged(); } }

        public virtual string? SCFlags { get { return _SCFlags; } set { OnPropertyChanging(); _SCFlags = value; OnPropertyChanged(); } }

        public virtual int? ServiceDeptID { get { return _ServiceDeptID; } set { OnPropertyChanging(); _ServiceDeptID = value; OnPropertyChanged(); } }

        public virtual bool? SCFPostToAsset { get { return _SCFPostToAsset; } set { OnPropertyChanging(); _SCFPostToAsset = value; OnPropertyChanged(); } }

        public virtual string? Config { get { return _Config; } set { OnPropertyChanging(); _Config = value; OnPropertyChanged(); } }

        public virtual string? SOTypes { get { return _SOTypes; } set { OnPropertyChanging(); _SOTypes = value; OnPropertyChanged(); } }

        public virtual string? SCRemarks { get { return _SCRemarks; } set { OnPropertyChanging(); _SCRemarks = value; OnPropertyChanged(); } }

        public virtual int? SCWarehouseID { get { return _SCWarehouseID; } set { OnPropertyChanging(); _SCWarehouseID = value; OnPropertyChanged(); } }

        public virtual string? AdjustmentDepartment { get { return _AdjustmentDepartment; } set { OnPropertyChanging(); _AdjustmentDepartment = value; OnPropertyChanged(); } }

        public virtual ObservableCollection<Item>? Items { get; set; }
        public virtual ObservableCollection<StockFamily>? StockFamilies { get; set; }
        public virtual ObservableCollection<Approval>? Approvals { get; set; }

    }
}