using System;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pupa.BusinessObjects.Beesuite
{
    [Table("IC_Items", Schema = "Ascend")]
    public class Item : BaseEntity
    {
        public Item() { }

        #region Backing Fields
        // Primary Key tetap int (tidak nullable) agar identitas record pasti
        private int _ItemID;

        private string? _ItemCode;
        private string? _ItemName;
        private short? _ItemType;
        private int? _UOMID1;
        private int? _UOMID2;
        private int? _UOMID3;
        private int? _UOMID4;
        private decimal? _Packing2;
        private decimal? _Packing3;
        private decimal? _Packing4;
        private bool? _AutoConvert;
        private int? _CategoryID;
        private int? _BrandID;
        private int? _PriceGroupID;
        private int? _ManufacturerID;
        private int? _FamilyID;
        private int? _SubFamilyID;
        private bool? _UseSize;
        private int? _SizeRatioID;
        private string? _ValidSizes;
        private bool? _UseColor;
        private string? _ValidColors;
        private decimal? _DimensionLength;
        private decimal? _DimensionWidth;
        private decimal? _DimensionThickness;
        private decimal? _OuterDiameter;
        private decimal? _InnerDiameter;
        private decimal? _WeightPerUnit;
        private decimal? _GrossWeightPerUnit;
        private string? _LengthUnit;
        private string? _WidthUnit;
        private string? _ThicknessUnit;
        private string? _WeightUnit;
        private string? _GrossWeightUnit;
        private decimal? _Volume;
        private string? _VolumeUnit;
        private decimal? _MinQuantity;
        private decimal? _MaxQuantity;
        private decimal? _ReorderLevel;
        private decimal? _MinOrder;
        private bool? _Disabled;
        private bool? _Discontinue;
        private bool? _UseExpiryDate;
        private bool? _RequireSN;
        private bool? _UniqueSN;
        private string? _SerialNumberFormat;
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
        private int? _PurchaseTaxAccountID;
        private int? _ProjectPurchaseAccountID;
        private int? _AssemblyOutputAccountID;
        private string? _CostingMethod;
        private string? _ProcurementMethod;
        private int? _OrderIntervalID;
        private decimal? _OrderMultiple;
        private int? _LeadTime;
        private bool? _CanBeSold;
        private bool? _CanBePurchased;
        private bool? _CanBeUsed;
        private int? _OutsourceToSupplierID;
        private decimal? _OutsourceCost_Quantity;
        private int? _OutsourceCost_UOMLevel;
        private decimal? _OutsourceCostPerUnit;
        private int? _OutsourceCost_CurrencyID;
        private decimal? _SimpleRetailPrice;
        private string? _SimpleRetailPromo;
        private decimal? _SimpleMemberPrice;
        private string? _SimpleMemberPromo;
        private decimal? _SimplePurchasePrice;
        private decimal? _SimpleRetailMargin;
        private decimal? _SimpleMemberMargin;
        private string? _GoldItemType;
        private int? _GoldKadarID;
        private DateTime? _GoldReadyDate;
        private decimal? _GoldFinishWeight;
        private string? _CreatedBy;
        private DateTime? _CreateDate;
        private string? _LastModBy;
        private DateTime? _LastMod;
        private string? _CoState;
        private Guid? _CoUID;
        private Guid? _CoPkgID;
        private string? _Specification;
        private string? _Remarks;
        private string? _Flags;
        private string? _IConfig;
        private int? _CItemID;
        private string? _IMIID;
        private bool? _IsConfidential;
        private bool? _IsConsignment;
        private bool? _IsVehicle;
        private bool? _ReqCheck;
        private decimal? _BuildQuantity;
        private short? _BuildUOMLevel;
        private int? _CustomerWarranty;
        private int? _SupplierWarranty;
        private bool? _IgnoreInGrossProfitCheck;
        private int? _DefaultReceivingWarehouseID;
        private bool? _HMSForHousekeeping;
        private bool? _HMSForFBOutlets;
        private int? _LastPurchaseDetailID;
        private int? _IncentiveSchemaID;
        private string? _BlobPath;
        private string? _Grade;
        private int? _GoldJlhCetakan;
        private string? _GoldUkuran;
        private string? _GoldProcessDescription;
        private string? _GoldCustomer;
        private int? _ItemCounter;
        private int? _PriceCurrencyID;
        private decimal? _OldPrice;
        private string? _ExtraDiscount;
        private string? _SimplePurchaseDiscount;
        private short? _SimpleRetailUOMLevel;
        private short? _SimpleMemberUOMLevel;
        private short? _SimplePurchaseUOMLevel;
        private string? _ClassCode;
        private int? _DefaultSupplierID;
        private string? _SimplePurchaseTax;
        private string? _COGDepartment;
        private decimal? _DMS;
        private string? _DefaultLuxuryTax;
        private bool? _Checked;
        private string? _CheckedBy;
        private DateTime? _CheckedDateTime;
        private int? _AssemblyMethodID;
        private bool? _ItemNoTax;
        private decimal? _ItemPoints;
        private bool? _AllowSalesBonus;
        private int? _SourceItemID;
        private bool? _DontPostCOG;
        private string? _SimplePurchaseDiscountM;
        private string? _TaxMethod;
        private int? _ForCustomerID;
        private int? _DefaultCustomerID;
        private bool? _IsWIP;
        private string? _Flag;
        private string? _CustomStr01;
        private string? _CustomStr02;
        private string? _ValidWarehouses;
        private bool? _RequireCN;
        private decimal? _SOMinOrder;
        private int? _SimplePurchaseCurrencyID;
        private string? _SimplePurchaseExtraCost;
        private bool? _IgnoreCalc;
        private bool? _IsVarCost;
        private DateTime? _BOMDate;
        private bool? _ByWeight;
        private decimal? _MaxUsagePerYear;
        private int? _LifetimeMo;
        private string? _SalesDepartment;
        private bool? _IsTool;
        private bool? _DisabledInSales;
        private decimal? _ItemCustomVal1;
        private decimal? _ItemCustomVal2;
        private decimal? _ItemCustomVal3;
        private string? _ItemCustomStr1;
        private decimal? _ItemCustomVal4;
        private decimal? _ItemCustomVal5;
        private int? _ExColorID;
        private int? _ExCustomID1;
        private string? _ExPattern;
        private string? _Label;
        private int? _PackagingItemID;
        private int? _LabelItemID;
        private int? _MapItemID;
        private string? _ToolCategory;
        private string? _BundlePromoXML;
        private decimal? _MarkupMin;
        private decimal? _MarkupMax;
        private string? _ItemCustomStr2;
        private string? _ExtItemCode;
        private int? _ExCustomID2;
        private int? _ExCustomID3;
        private string? _PurchaseDepartment;
        private bool? _Candidate;
        private bool? _ReqCheck2;
        private string? _Check2By;
        private DateTime? _Check2DateTime;
        private string? _IChecker;
        private string? _ISpecialNote;
        private decimal? _SimpleMinPrice;
        private decimal? _SimpleMinPriceS;
        private string? _POAutoGenMode;
        private string? _CTItemCode;
        private decimal? _RFIDFactor;
        private string? _PartNoEx;
        private string? _Sourcing;
        private int? _ItemCategoryID;
        private int? _WNSID;
        private int? _MJSID;
        private int? _GMIID;
        private int? _AMTID;
        private int? _TTPID;
        private int? _WSIID;
        private bool? _RequiredOfflineSync;

        private string? _UOMCode;
        private decimal? _UOMDecimals;
        private int? _UOMIDx;
        #endregion

        #region Public Properties
        [Key]
        [Column("ItemID")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public virtual int ItemID { get { return _ItemID; } set { OnPropertyChanging(); _ItemID = value; OnPropertyChanged(); } }

        [Column("ItemCode")]
        public virtual string? ItemCode { get { return _ItemCode; } set { OnPropertyChanging(); _ItemCode = value; OnPropertyChanged(); } }

        [Column("ItemName")]
        public virtual string? ItemName { get { return _ItemName; } set { OnPropertyChanging(); _ItemName = value; OnPropertyChanged(); } }

        [Column("ItemType")]
        public virtual short? ItemType { get { return _ItemType; } set { OnPropertyChanging(); _ItemType = value; OnPropertyChanged(); } }

        [Column("Packing2")]
        public virtual decimal? Packing2 { get { return _Packing2; } set { OnPropertyChanging(); _Packing2 = value; OnPropertyChanged(); } }

        [Column("Packing3")]
        public virtual decimal? Packing3 { get { return _Packing3; } set { OnPropertyChanging(); _Packing3 = value; OnPropertyChanged(); } }

        [Column("Packing4")]
        public virtual decimal? Packing4 { get { return _Packing4; } set { OnPropertyChanging(); _Packing4 = value; OnPropertyChanged(); } }

        [Column("AutoConvert")]
        public virtual bool? AutoConvert { get { return _AutoConvert; } set { OnPropertyChanging(); _AutoConvert = value; OnPropertyChanged(); } }

        [Column("CategoryID")]
        public virtual int? CategoryID { get { return _CategoryID; } set { OnPropertyChanging(); _CategoryID = value; OnPropertyChanged(); } }

        [Column("BrandID")]
        public virtual int? BrandID { get { return _BrandID; } set { OnPropertyChanging(); _BrandID = value; OnPropertyChanged(); } }

        [Column("PriceGroupID")]
        public virtual int? PriceGroupID { get { return _PriceGroupID; } set { OnPropertyChanging(); _PriceGroupID = value; OnPropertyChanged(); } }

        [Column("ManufacturerID")]
        public virtual int? ManufacturerID { get { return _ManufacturerID; } set { OnPropertyChanging(); _ManufacturerID = value; OnPropertyChanged(); } }

        [Column("FamilyID")]
        public virtual int? FamilyID { get { return _FamilyID; } set { OnPropertyChanging(); _FamilyID = value; OnPropertyChanged(); } }

        [Column("SubFamilyID")]
        public virtual int? SubFamilyID { get { return _SubFamilyID; } set { OnPropertyChanging(); _SubFamilyID = value; OnPropertyChanged(); } }

        [Column("UseSize")]
        public virtual bool? UseSize { get { return _UseSize; } set { OnPropertyChanging(); _UseSize = value; OnPropertyChanged(); } }

        [Column("SizeRatioID")]
        public virtual int? SizeRatioID { get { return _SizeRatioID; } set { OnPropertyChanging(); _SizeRatioID = value; OnPropertyChanged(); } }

        [Column("ValidSizes")]
        public virtual string? ValidSizes { get { return _ValidSizes; } set { OnPropertyChanging(); _ValidSizes = value; OnPropertyChanged(); } }

        [Column("UseColor")]
        public virtual bool? UseColor { get { return _UseColor; } set { OnPropertyChanging(); _UseColor = value; OnPropertyChanged(); } }

        [Column("ValidColors")]
        public virtual string? ValidColors { get { return _ValidColors; } set { OnPropertyChanging(); _ValidColors = value; OnPropertyChanged(); } }

        [Column("DimensionLength")]
        public virtual decimal? DimensionLength { get { return _DimensionLength; } set { OnPropertyChanging(); _DimensionLength = value; OnPropertyChanged(); } }

        [Column("DimensionWidth")]
        public virtual decimal? DimensionWidth { get { return _DimensionWidth; } set { OnPropertyChanging(); _DimensionWidth = value; OnPropertyChanged(); } }

        [Column("DimensionThickness")]
        public virtual decimal? DimensionThickness { get { return _DimensionThickness; } set { OnPropertyChanging(); _DimensionThickness = value; OnPropertyChanged(); } }

        [Column("OuterDiameter")]
        public virtual decimal? OuterDiameter { get { return _OuterDiameter; } set { OnPropertyChanging(); _OuterDiameter = value; OnPropertyChanged(); } }

        [Column("InnerDiameter")]
        public virtual decimal? InnerDiameter { get { return _InnerDiameter; } set { OnPropertyChanging(); _InnerDiameter = value; OnPropertyChanged(); } }

        [Column("WeightPerUnit")]
        public virtual decimal? WeightPerUnit { get { return _WeightPerUnit; } set { OnPropertyChanging(); _WeightPerUnit = value; OnPropertyChanged(); } }

        [Column("GrossWeightPerUnit")]
        public virtual decimal? GrossWeightPerUnit { get { return _GrossWeightPerUnit; } set { OnPropertyChanging(); _GrossWeightPerUnit = value; OnPropertyChanged(); } }

        [Column("LengthUnit")]
        public virtual string? LengthUnit { get { return _LengthUnit; } set { OnPropertyChanging(); _LengthUnit = value; OnPropertyChanged(); } }

        [Column("WidthUnit")]
        public virtual string? WidthUnit { get { return _WidthUnit; } set { OnPropertyChanging(); _WidthUnit = value; OnPropertyChanged(); } }

        [Column("ThicknessUnit")]
        public virtual string? ThicknessUnit { get { return _ThicknessUnit; } set { OnPropertyChanging(); _ThicknessUnit = value; OnPropertyChanged(); } }

        [Column("WeightUnit")]
        public virtual string? WeightUnit { get { return _WeightUnit; } set { OnPropertyChanging(); _WeightUnit = value; OnPropertyChanged(); } }

        [Column("GrossWeightUnit")]
        public virtual string? GrossWeightUnit { get { return _GrossWeightUnit; } set { OnPropertyChanging(); _GrossWeightUnit = value; OnPropertyChanged(); } }

        [Column("Volume")]
        public virtual decimal? Volume { get { return _Volume; } set { OnPropertyChanging(); _Volume = value; OnPropertyChanged(); } }

        [Column("VolumeUnit")]
        public virtual string? VolumeUnit { get { return _VolumeUnit; } set { OnPropertyChanging(); _VolumeUnit = value; OnPropertyChanged(); } }

        [Column("MinQuantity")]
        public virtual decimal? MinQuantity { get { return _MinQuantity; } set { OnPropertyChanging(); _MinQuantity = value; OnPropertyChanged(); } }

        [Column("MaxQuantity")]
        public virtual decimal? MaxQuantity { get { return _MaxQuantity; } set { OnPropertyChanging(); _MaxQuantity = value; OnPropertyChanged(); } }

        [Column("ReorderLevel")]
        public virtual decimal? ReorderLevel { get { return _ReorderLevel; } set { OnPropertyChanging(); _ReorderLevel = value; OnPropertyChanged(); } }

        [Column("MinOrder")]
        public virtual decimal? MinOrder { get { return _MinOrder; } set { OnPropertyChanging(); _MinOrder = value; OnPropertyChanged(); } }

        [Column("Disabled")]
        public virtual bool? Disabled { get { return _Disabled; } set { OnPropertyChanging(); _Disabled = value; OnPropertyChanged(); } }

        [Column("Discontinue")]
        public virtual bool? Discontinue { get { return _Discontinue; } set { OnPropertyChanging(); _Discontinue = value; OnPropertyChanged(); } }

        [Column("UseExpiryDate")]
        public virtual bool? UseExpiryDate { get { return _UseExpiryDate; } set { OnPropertyChanging(); _UseExpiryDate = value; OnPropertyChanged(); } }

        [Column("RequireSN")]
        public virtual bool? RequireSN { get { return _RequireSN; } set { OnPropertyChanging(); _RequireSN = value; OnPropertyChanged(); } }

        [Column("UniqueSN")]
        public virtual bool? UniqueSN { get { return _UniqueSN; } set { OnPropertyChanging(); _UniqueSN = value; OnPropertyChanged(); } }

        [Column("SerialNumberFormat")]
        public virtual string? SerialNumberFormat { get { return _SerialNumberFormat; } set { OnPropertyChanging(); _SerialNumberFormat = value; OnPropertyChanged(); } }

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

        [Column("PurchaseTaxAccountID")]
        public virtual int? PurchaseTaxAccountID { get { return _PurchaseTaxAccountID; } set { OnPropertyChanging(); _PurchaseTaxAccountID = value; OnPropertyChanged(); } }

        [Column("CostingMethod")]
        public virtual string? CostingMethod { get { return _CostingMethod; } set { OnPropertyChanging(); _CostingMethod = value; OnPropertyChanged(); } }

        [Column("ProcurementMethod")]
        public virtual string? ProcurementMethod { get { return _ProcurementMethod; } set { OnPropertyChanging(); _ProcurementMethod = value; OnPropertyChanged(); } }

        [Column("OrderIntervalID")]
        public virtual int? OrderIntervalID { get { return _OrderIntervalID; } set { OnPropertyChanging(); _OrderIntervalID = value; OnPropertyChanged(); } }

        [Column("OrderMultiple")]
        public virtual decimal? OrderMultiple { get { return _OrderMultiple; } set { OnPropertyChanging(); _OrderMultiple = value; OnPropertyChanged(); } }

        [Column("LeadTime")]
        public virtual int? LeadTime { get { return _LeadTime; } set { OnPropertyChanging(); _LeadTime = value; OnPropertyChanged(); } }

        [Column("CanBeSold")]
        public virtual bool? CanBeSold { get { return _CanBeSold; } set { OnPropertyChanging(); _CanBeSold = value; OnPropertyChanged(); } }

        [Column("CanBePurchased")]
        public virtual bool? CanBePurchased { get { return _CanBePurchased; } set { OnPropertyChanging(); _CanBePurchased = value; OnPropertyChanged(); } }

        [Column("CanBeUsed")]
        public virtual bool? CanBeUsed { get { return _CanBeUsed; } set { OnPropertyChanging(); _CanBeUsed = value; OnPropertyChanged(); } }

        [Column("SimpleRetailPrice")]
        public virtual decimal? SimpleRetailPrice { get { return _SimpleRetailPrice; } set { OnPropertyChanging(); _SimpleRetailPrice = value; OnPropertyChanged(); } }

        [Column("SimpleMemberPrice")]
        public virtual decimal? SimpleMemberPrice { get { return _SimpleMemberPrice; } set { OnPropertyChanging(); _SimpleMemberPrice = value; OnPropertyChanged(); } }

        [Column("SimplePurchasePrice")]
        public virtual decimal? SimplePurchasePrice { get { return _SimplePurchasePrice; } set { OnPropertyChanging(); _SimplePurchasePrice = value; OnPropertyChanged(); } }

        [Column("CreatedBy")]
        public virtual string? CreatedBy { get { return _CreatedBy; } set { OnPropertyChanging(); _CreatedBy = value; OnPropertyChanged(); } }

        [Column("CreateDate")]
        public virtual DateTime? CreateDate { get { return _CreateDate; } set { OnPropertyChanging(); _CreateDate = value; OnPropertyChanged(); } }

        [Column("LastModBy")]
        public virtual string? LastModBy { get { return _LastModBy; } set { OnPropertyChanging(); _LastModBy = value; OnPropertyChanged(); } }

        [Column("LastMod")]
        public virtual DateTime? LastMod { get { return _LastMod; } set { OnPropertyChanging(); _LastMod = value; OnPropertyChanged(); } }

        [Column("CoState")]
        public virtual string? CoState { get { return _CoState; } set { OnPropertyChanging(); _CoState = value; OnPropertyChanged(); } }

        [Column("CoUID")]
        public virtual Guid? CoUID { get { return _CoUID; } set { OnPropertyChanging(); _CoUID = value; OnPropertyChanged(); } }

        [Column("Specification")]
        public virtual string? Specification { get { return _Specification; } set { OnPropertyChanging(); _Specification = value; OnPropertyChanged(); } }

        [Column("Remarks")]
        public virtual string? Remarks { get { return _Remarks; } set { OnPropertyChanging(); _Remarks = value; OnPropertyChanged(); } }

        [Column("Flags")]
        public virtual string? Flags { get { return _Flags; } set { OnPropertyChanging(); _Flags = value; OnPropertyChanged(); } }

        [Column("IsConfidential")]
        public virtual bool? IsConfidential { get { return _IsConfidential; } set { OnPropertyChanging(); _IsConfidential = value; OnPropertyChanged(); } }

        [Column("IsConsignment")]
        public virtual bool? IsConsignment { get { return _IsConsignment; } set { OnPropertyChanging(); _IsConsignment = value; OnPropertyChanged(); } }

        [Column("IsVehicle")]
        public virtual bool? IsVehicle { get { return _IsVehicle; } set { OnPropertyChanging(); _IsVehicle = value; OnPropertyChanged(); } }

        [Column("BuildQuantity")]
        public virtual decimal? BuildQuantity { get { return _BuildQuantity; } set { OnPropertyChanging(); _BuildQuantity = value; OnPropertyChanged(); } }

        [Column("DefaultReceivingWarehouseID")]
        public virtual int? DefaultReceivingWarehouseID { get { return _DefaultReceivingWarehouseID; } set { OnPropertyChanging(); _DefaultReceivingWarehouseID = value; OnPropertyChanged(); } }

        [Column("BlobPath")]
        public virtual string? BlobPath { get { return _BlobPath; } set { OnPropertyChanging(); _BlobPath = value; OnPropertyChanged(); } }

        [Column("Grade")]
        public virtual string? Grade { get { return _Grade; } set { OnPropertyChanging(); _Grade = value; OnPropertyChanged(); } }

        [Column("TaxMethod")]
        public virtual string? TaxMethod { get { return _TaxMethod; } set { OnPropertyChanging(); _TaxMethod = value; OnPropertyChanged(); } }

        [Column("CustomStr01")]
        public virtual string? CustomStr01 { get { return _CustomStr01; } set { OnPropertyChanging(); _CustomStr01 = value; OnPropertyChanged(); } }

        [Column("CustomStr02")]
        public virtual string? CustomStr02 { get { return _CustomStr02; } set { OnPropertyChanging(); _CustomStr02 = value; OnPropertyChanged(); } }

        [Column("PartNoEx")]
        public virtual string? PartNoEx { get { return _PartNoEx; } set { OnPropertyChanging(); _PartNoEx = value; OnPropertyChanged(); } }

        [Column("Sourcing")]
        public virtual string? Sourcing { get { return _Sourcing; } set { OnPropertyChanging(); _Sourcing = value; OnPropertyChanged(); } }

        [NotMapped]
        public virtual string? UOMCode { get { return _UOMCode; } set { OnPropertyChanging(); _UOMCode = value; OnPropertyChanged(); } }

        [NotMapped]
        public virtual decimal? UOMDecimals { get { return _UOMDecimals; } set { OnPropertyChanging(); _UOMDecimals = value; OnPropertyChanged(); } }

        [NotMapped]
        public virtual int? UOMIDx { get { return _UOMIDx; } set { OnPropertyChanging(); _UOMIDx = value; OnPropertyChanged(); } }
        #endregion

        // Navigation properties biasanya sudah nullable (virtual)
        [ForeignKey("CategoryID")]
        public virtual StockCategory? StockCategory { get; set; }

        [ForeignKey("FamilyID")]
        public virtual StockFamily? StockFamily { get; set; }

        [ForeignKey("UOMID1")]
        public virtual UOM? UOM1 { get; set; }
        [ForeignKey("UOMID2")]
        public virtual UOM? UOM2 { get; set; }
        [ForeignKey("UOMID3")]
        public virtual UOM? UOM3 { get; set; }
        [ForeignKey("UOMID4")]
        public virtual UOM? UOM4 { get; set; }
        public virtual bool? RequiredOfflineSync { get { return _RequiredOfflineSync; } set { OnPropertyChanging(); _RequiredOfflineSync = value; OnPropertyChanged(); } }

        public virtual ObservableCollection<ROB>? ROBs { get; set; }
        private string? _Alias { get; set; }
        public virtual string? Alias { get { return _Alias; } set { OnPropertyChanging(); _Alias = value; OnPropertyChanged(); } }
        private string? _PartBookAttachmentPath;
        public virtual string? PartBookAttachmentPath
        {
            get { return _PartBookAttachmentPath; }
            set { OnPropertyChanging(); _PartBookAttachmentPath = value; OnPropertyChanged(); }
        }
    }
}
