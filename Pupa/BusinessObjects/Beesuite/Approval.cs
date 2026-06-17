using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;

namespace Pupa.BusinessObjects.Beesuite
{
    [Table("Approval", Schema = "Ascend")]
    public class Approval : BaseEntity
    {
        public Approval()
        {
        }

        #region Backing Fields
        private string? _ApprovedBy1;
        private string? _ApprovedBy2;
        private string? _ApprovedBy3;
        private string? _ApprovedBy4;
        private string? _ApprovedBy5;
        private string? _ApprovedBy6;
        private string? _ApprovedBy7;
        private int? _RequestCount;
        private int? _InventoryUserGroupID;
        private int? _StockCategoryID;
        private int? _FamilyID;
        #endregion

        #region Public Properties
        [Key]
        [Column("ID")]
        public virtual int ID
        {
            get;
            set;
        }

        [Column("ApprovedBy1")]
        public virtual string? ApprovedBy1
        {
            get => _ApprovedBy1;
            set { OnPropertyChanging(); _ApprovedBy1 = value; OnPropertyChanged(); }
        }

        [Column("ApprovedBy2")]
        public virtual string? ApprovedBy2
        {
            get => _ApprovedBy2;
            set { OnPropertyChanging(); _ApprovedBy2 = value; OnPropertyChanged(); }
        }

        [Column("ApprovedBy3")]
        public virtual string? ApprovedBy3
        {
            get => _ApprovedBy3;
            set { OnPropertyChanging(); _ApprovedBy3 = value; OnPropertyChanged(); }
        }

        [Column("ApprovedBy4")]
        public virtual string? ApprovedBy4
        {
            get => _ApprovedBy4;
            set { OnPropertyChanging(); _ApprovedBy4 = value; OnPropertyChanged(); }
        }

        [Column("ApprovedBy5")]
        public virtual string? ApprovedBy5
        {
            get => _ApprovedBy5;
            set { OnPropertyChanging(); _ApprovedBy5 = value; OnPropertyChanged(); }
        }

        [Column("ApprovedBy6")]
        public virtual string? ApprovedBy6
        {
            get => _ApprovedBy6;
            set { OnPropertyChanging(); _ApprovedBy6 = value; OnPropertyChanged(); }
        }

        [Column("ApprovedBy7")]
        public virtual string? ApprovedBy7
        {
            get => _ApprovedBy7;
            set { OnPropertyChanging(); _ApprovedBy7 = value; OnPropertyChanged(); }
        }

        [Column("RequestCount")]
        public virtual int? RequestCount
        {
            get => _RequestCount;
            set { OnPropertyChanging(); _RequestCount = value; OnPropertyChanged(); }
        }

        [Column("InventoryUserGroupID")]
        public virtual int? InventoryUserGroupID
        {
            get => _InventoryUserGroupID;
            set { OnPropertyChanging(); _InventoryUserGroupID = value; OnPropertyChanged(); }
        }

        [Column("StockCategoryID")]
        public virtual int? StockCategoryID
        {
            get => _StockCategoryID;
            set { OnPropertyChanging(); _StockCategoryID = value; OnPropertyChanged(); }
        }
        [Column("FamilyID")]
        public virtual int? FamilyID
        {
            get => _FamilyID;
            set { OnPropertyChanging(); _FamilyID = value; OnPropertyChanged(); }
        }
        #endregion
        [ForeignKey("InventoryUserGroupID")]
        public virtual InventoryUserGroup? InventoryUserGroup { get; set; }
        [ForeignKey("StockCategoryID")]
        public virtual StockCategory? StockCategory { get; set; }
        [ForeignKey("FamilyID")]
        public virtual StockFamily? StockFamily { get; set; }

        public virtual bool? IsAll { get; set; }
        public virtual string? StockCategoryApprovers { get; set;  }


    }
}