using System;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pupa.BusinessObjects.Beesuite
{
    [Table("IC_StockFamily", Schema = "Ascend")]
    public class StockFamily : BaseEntity
    {
        public StockFamily()
        {
            this.Items = new ObservableCollection<Item>();
            this.COAs = new ObservableCollection<StockFamilyCOA>();
        }

        #region Backing Fields
        private int? _StockCategoryID;
        private string? _FamilyCode;
        private string? _FamilyName;
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
        private int? _AdjustmentAccountID;
        private string? _COGDepartment;
        private int? _ProjectPurchaseAccountID;
        private int? _PurchaseNonStockAccountID;
        private string? _BuyerGroup;
        private Guid? _CoUID;
        private int? _QCSetID;
        private string? _SFFlags;
        private bool? _DontCheckStock;
        private string? _SFConfig;
        private int? _ServiceDeptID;
        private decimal? _SFMinMargin;
        private decimal? _SFMaxMargin;
        private string? _SFAlias;
        #endregion

        #region Public Properties
        [Column("StockCategoryID")]
        public virtual int? StockCategoryID { get { return _StockCategoryID; } set { OnPropertyChanging(); _StockCategoryID = value; OnPropertyChanged(); } }

        [Key]
        [Column("FamilyID")]
        public virtual int FamilyID { get; set; }

        [Column("FamilyCode")]
        public virtual string? FamilyCode { get { return _FamilyCode; } set { OnPropertyChanging(); _FamilyCode = value; OnPropertyChanged(); } }

        [Column("FamilyName")]
        public virtual string? FamilyName { get { return _FamilyName; } set { OnPropertyChanging(); _FamilyName = value; OnPropertyChanged(); } }

        [Column("InventoryAccountID")]
        public virtual int? InventoryAccountID { get { return _InventoryAccountID; } set { OnPropertyChanging(); _InventoryAccountID = value; OnPropertyChanged(); } }

        [Column("SalesAccountID")]
        public virtual int? SalesAccountID { get { return _SalesAccountID; } set { OnPropertyChanging(); _SalesAccountID = value; OnPropertyChanged(); } }

        [Column("SalesReturnAccountID")]
        public virtual int? SalesReturnAccountID { get { return _SalesReturnAccountID; } set { OnPropertyChanging(); _SalesReturnAccountID = value; OnPropertyChanged(); } }

        [Column("SalesDiscountAccountID")]
        public virtual int? SalesDiscountAccountID { get { return _SalesDiscountAccountID; } set { OnPropertyChanging(); _SalesDiscountAccountID = value; OnPropertyChanged(); } }

        [Column("PurchaseAccountID")]
        public virtual int? PurchaseAccountID { get { return _PurchaseAccountID; } set { OnPropertyChanging(); _PurchaseAccountID = value; OnPropertyChanged(); } }

        [Column("PurchaseReturnAccountID")]
        public virtual int? PurchaseReturnAccountID { get { return _PurchaseReturnAccountID; } set { OnPropertyChanging(); _PurchaseReturnAccountID = value; OnPropertyChanged(); } }

        [Column("PurchaseDiscountAccountID")]
        public virtual int? PurchaseDiscountAccountID { get { return _PurchaseDiscountAccountID; } set { OnPropertyChanging(); _PurchaseDiscountAccountID = value; OnPropertyChanged(); } }

        [Column("UsageAccountID")]
        public virtual int? UsageAccountID { get { return _UsageAccountID; } set { OnPropertyChanging(); _UsageAccountID = value; OnPropertyChanged(); } }

        [Column("UsageReturnAccountID")]
        public virtual int? UsageReturnAccountID { get { return _UsageReturnAccountID; } set { OnPropertyChanging(); _UsageReturnAccountID = value; OnPropertyChanged(); } }

        [Column("InventoryBeginningAccountID")]
        public virtual int? InventoryBeginningAccountID { get { return _InventoryBeginningAccountID; } set { OnPropertyChanging(); _InventoryBeginningAccountID = value; OnPropertyChanged(); } }

        [Column("InventoryEndingAccountID")]
        public virtual int? InventoryEndingAccountID { get { return _InventoryEndingAccountID; } set { OnPropertyChanging(); _InventoryEndingAccountID = value; OnPropertyChanged(); } }

        [Column("COGAccountID")]
        public virtual int? COGAccountID { get { return _COGAccountID; } set { OnPropertyChanging(); _COGAccountID = value; OnPropertyChanged(); } }

        [Column("AdjustmentAccountID")]
        public virtual int? AdjustmentAccountID { get { return _AdjustmentAccountID; } set { OnPropertyChanging(); _AdjustmentAccountID = value; OnPropertyChanged(); } }

        [Column("COGDepartment")]
        public virtual string? COGDepartment { get { return _COGDepartment; } set { OnPropertyChanging(); _COGDepartment = value; OnPropertyChanged(); } }

        [Column("ProjectPurchaseAccountID")]
        public virtual int? ProjectPurchaseAccountID { get { return _ProjectPurchaseAccountID; } set { OnPropertyChanging(); _ProjectPurchaseAccountID = value; OnPropertyChanged(); } }

        [Column("PurchaseNonStockAccountID")]
        public virtual int? PurchaseNonStockAccountID { get { return _PurchaseNonStockAccountID; } set { OnPropertyChanging(); _PurchaseNonStockAccountID = value; OnPropertyChanged(); } }

        [Column("BuyerGroup")]
        public virtual string? BuyerGroup { get { return _BuyerGroup; } set { OnPropertyChanging(); _BuyerGroup = value; OnPropertyChanged(); } }

        [Column("CoUID")]
        public virtual Guid? CoUID { get { return _CoUID; } set { OnPropertyChanging(); _CoUID = value; OnPropertyChanged(); } }

        [Column("QCSetID")]
        public virtual int? QCSetID { get { return _QCSetID; } set { OnPropertyChanging(); _QCSetID = value; OnPropertyChanged(); } }

        [Column("SFFlags")]
        public virtual string? SFFlags { get { return _SFFlags; } set { OnPropertyChanging(); _SFFlags = value; OnPropertyChanged(); } }

        [Column("DontCheckStock")]
        public virtual bool? DontCheckStock { get { return _DontCheckStock; } set { OnPropertyChanging(); _DontCheckStock = value; OnPropertyChanged(); } }

        [Column("SFConfig")]
        public virtual string? SFConfig { get { return _SFConfig; } set { OnPropertyChanging(); _SFConfig = value; OnPropertyChanged(); } }

        [Column("ServiceDeptID")]
        public virtual int? ServiceDeptID { get { return _ServiceDeptID; } set { OnPropertyChanging(); _ServiceDeptID = value; OnPropertyChanged(); } }

        [Column("SFMinMargin")]
        public virtual decimal? SFMinMargin { get { return _SFMinMargin; } set { OnPropertyChanging(); _SFMinMargin = value; OnPropertyChanged(); } }

        [Column("SFMaxMargin")]
        public virtual decimal? SFMaxMargin { get { return _SFMaxMargin; } set { OnPropertyChanging(); _SFMaxMargin = value; OnPropertyChanged(); } }

        [Column("SFAlias")]
        public virtual string? SFAlias { get { return _SFAlias; } set { OnPropertyChanging(); _SFAlias = value; OnPropertyChanged(); } }

        public virtual ObservableCollection<Item>? Items { get; set; }
        #endregion

        #region Navigation Properties
        [ForeignKey("StockCategoryID")]
        public virtual StockCategory? StockCategory { get; set; }
        #endregion

        public virtual ObservableCollection<StockFamilyCOA> COAs { get; set; }
    }
}