using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pupa.BusinessObjects.Beesuite
{
    [Table("UserApprovalScope")]
    public class UserApprovalScope : BaseEntity
    {
        public UserApprovalScope()
        {
        }

        #region Backing Fields
        private int? _UserID;
        private string? _CompanyDB;
        private int? _VesselGroupID;
        private int? _StockCategoryID;
        private int? _StockFamilyID;
        private short? _Level;
        private string? _Department;
        private string? _SubDepartment;
        private int? _InventoryUserID;
        private DateTime? _CreatedAt;
        private DateTime? _UpdatedAt;
        private int _ID;
        #endregion

        #region Public Properties
        [Key]
        [Column("ID")]
        public virtual int ID
        {
            get => _ID;
            set { OnPropertyChanging(); _ID = value; OnPropertyChanged(); }
        }

        [Column("UserID")]
        public virtual int? UserID
        {
            get => _UserID;
            set { OnPropertyChanging(); _UserID = value; OnPropertyChanged(); }
        }

        [Column("CompanyDB")]
        [MaxLength(20)]
        public virtual string? CompanyDB
        {
            get => _CompanyDB;
            set { OnPropertyChanging(); _CompanyDB = value; OnPropertyChanged(); }
        }

        [Column("VesselGroupID")]
        public virtual int? VesselGroupID
        {
            get => _VesselGroupID;
            set { OnPropertyChanging(); _VesselGroupID = value; OnPropertyChanged(); }
        }

        [Column("StockCategoryID")]
        public virtual int? StockCategoryID
        {
            get => _StockCategoryID;
            set { OnPropertyChanging(); _StockCategoryID = value; OnPropertyChanged(); }
        }

        [Column("StockFamilyID")]
        public virtual int? StockFamilyID
        {
            get => _StockFamilyID;
            set { OnPropertyChanging(); _StockFamilyID = value; OnPropertyChanged(); }
        }

        /// <summary>Approval level in chain: 1 = first approver … 6 = sixth approver. Null = no level assigned.</summary>
        [Column("Level")]
        [Range(1, 6)]
        public virtual short? Level
        {
            get => _Level;
            set { OnPropertyChanging(); _Level = value; OnPropertyChanged(); }
        }

        [Column("CreatedAt")]
        public virtual DateTime? CreatedAt
        {
            get => _CreatedAt;
            set { OnPropertyChanging(); _CreatedAt = value; OnPropertyChanged(); }
        }

        [Column("UpdatedAt")]
        public virtual DateTime? UpdatedAt
        {
            get => _UpdatedAt;
            set { OnPropertyChanging(); _UpdatedAt = value; OnPropertyChanged(); }
        }

        [Column("Department")]
        [MaxLength(10)]
        public virtual string? Department
        {
            get => _Department;
            set { OnPropertyChanging(); _Department = value; OnPropertyChanged(); }
        }

        [Column("SubDepartment")]
        public virtual string? SubDepartment
        {
            get => _SubDepartment;
            set { OnPropertyChanging(); _SubDepartment = value; OnPropertyChanged(); }
        }
        #endregion

        #region Navigation Properties
        [ForeignKey("UserID")]
        public virtual User? User { get; set; }

        [ForeignKey("VesselGroupID")]
        public virtual InventoryUserGroup? VesselGroup { get; set; }

        [ForeignKey("StockCategoryID")]
        public virtual StockCategory? StockCategory { get; set; }

        [ForeignKey("StockFamilyID")]
        public virtual StockFamily? StockFamily { get; set; }

        public virtual int? InventoryUserID
        {
            get => _InventoryUserID;
            set { OnPropertyChanging(); _InventoryUserID = value; OnPropertyChanged(); }
        }

        [ForeignKey("InventoryUserID")]
        public virtual InventoryUser? InventoryUser { get; set; }
        #endregion
    }
}