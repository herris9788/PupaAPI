using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;

namespace Pupa.BusinessObjects.Beesuite
{
    [Table("IC_InventoryUsers", Schema = "Ascend")]
    public class InventoryUser : BaseEntity
    {
        public InventoryUser()
        {
            this.Requisitions = new ObservableCollection<Requisition>();
            this.InventoryUserSpecs = new ObservableCollection<InventoryUserSpec>();
            this.ROBs = new ObservableCollection<ROB>();
        }

        #region Backing Fields
        private int? _InventoryUserID;
        private string? _InventoryUserCode;
        private string? _DB;
        private string? _InventoryUserName;
        private string? _Department;
        private string? _HODUserName;
        private string? _UserName;
        private string? _CoState;
        private Guid? _CoUID;
        private Guid? _CoPkgID;
        private Guid? _CoSourcePkgID;
        private bool? _Disabled;
        private string? _AssistantUserName;
        private bool? _SpecificStockCategories;
        private string? _StockCategories;
        private string? _WarehouseIDs;
        private bool? _IsGlobal;
        private int? _OS1;
        private int? _GroupID;
        private bool? _UseInProject;
        private bool? _IsVessel;
        private bool? _BatchResource;
        private string? _Remarks;
        private string? _Specification;
        private string? _DWT;
        private string? _IMO;
        #endregion

        #region Public Properties
        [Key]
        [Column("ID")]
        public virtual int ID
        {
            get; set;
        }
        [Column("DB")]
        public virtual string? DB
        {
            get => _DB;
            set { OnPropertyChanging(); _DB = value; OnPropertyChanged(); }
        }

        [Column("InventoryUserID")]
        public virtual int? InventoryUserID
        {
            get => _InventoryUserID;
            set { OnPropertyChanging(); _InventoryUserID = value; OnPropertyChanged(); }
        }

        [Column("InventoryUserCode")]
        public virtual string? InventoryUserCode
        {
            get => _InventoryUserCode;
            set { OnPropertyChanging(); _InventoryUserCode = value; OnPropertyChanged(); }
        }

        [Column("InventoryUserName")]
        public virtual string? InventoryUserName
        {
            get => _InventoryUserName;
            set { OnPropertyChanging(); _InventoryUserName = value; OnPropertyChanged(); }
        }

        [Column("Department")]
        public virtual string? Department
        {
            get => _Department;
            set { OnPropertyChanging(); _Department = value; OnPropertyChanged(); }
        }

        [Column("HODUserName")]
        public virtual string? HODUserName
        {
            get => _HODUserName;
            set { OnPropertyChanging(); _HODUserName = value; OnPropertyChanged(); }
        }

        [Column("UserName")]
        public virtual string? UserName
        {
            get => _UserName;
            set { OnPropertyChanging(); _UserName = value; OnPropertyChanged(); }
        }

        [Column("CoState")]
        public virtual string? CoState
        {
            get => _CoState;
            set { OnPropertyChanging(); _CoState = value; OnPropertyChanged(); }
        }

        [Column("CoUID")]
        public virtual Guid? CoUID
        {
            get => _CoUID;
            set { OnPropertyChanging(); _CoUID = value; OnPropertyChanged(); }
        }

        [Column("CoPkgID")]
        public virtual Guid? CoPkgID
        {
            get => _CoPkgID;
            set { OnPropertyChanging(); _CoPkgID = value; OnPropertyChanged(); }
        }

        [Column("CoSourcePkgID")]
        public virtual Guid? CoSourcePkgID
        {
            get => _CoSourcePkgID;
            set { OnPropertyChanging(); _CoSourcePkgID = value; OnPropertyChanged(); }
        }

        [Column("Disabled")]
        public virtual bool? Disabled
        {
            get => _Disabled;
            set { OnPropertyChanging(); _Disabled = value; OnPropertyChanged(); }
        }

        [Column("AssistantUserName")]
        public virtual string? AssistantUserName
        {
            get => _AssistantUserName;
            set { OnPropertyChanging(); _AssistantUserName = value; OnPropertyChanged(); }
        }

        [Column("SpecificStockCategories")]
        public virtual bool? SpecificStockCategories
        {
            get => _SpecificStockCategories;
            set { OnPropertyChanging(); _SpecificStockCategories = value; OnPropertyChanged(); }
        }

        [Column("StockCategories")]
        public virtual string? StockCategories
        {
            get => _StockCategories;
            set { OnPropertyChanging(); _StockCategories = value; OnPropertyChanged(); }
        }

        [Column("WarehouseIDs")]
        public virtual string? WarehouseIDs
        {
            get => _WarehouseIDs;
            set { OnPropertyChanging(); _WarehouseIDs = value; OnPropertyChanged(); }
        }

        [Column("IsGlobal")]
        public virtual bool? IsGlobal
        {
            get => _IsGlobal;
            set { OnPropertyChanging(); _IsGlobal = value; OnPropertyChanged(); }
        }

        [Column("OS1")]
        public virtual int? OS1
        {
            get => _OS1;
            set { OnPropertyChanging(); _OS1 = value; OnPropertyChanged(); }
        }

        [Column("GroupID")]
        public virtual int? GroupID
        {
            get => _GroupID;
            set { OnPropertyChanging(); _GroupID = value; OnPropertyChanged(); }
        }

        [Column("UseInProject")]
        public virtual bool? UseInProject
        {
            get => _UseInProject;
            set { OnPropertyChanging(); _UseInProject = value; OnPropertyChanged(); }
        }

        [Column("IsVessel")]
        public virtual bool? IsVessel
        {
            get => _IsVessel;
            set { OnPropertyChanging(); _IsVessel = value; OnPropertyChanged(); }
        }

        [Column("BatchResource")]
        public virtual bool? BatchResource
        {
            get => _BatchResource;
            set { OnPropertyChanging(); _BatchResource = value; OnPropertyChanged(); }
        }

        [Column("Remarks")]
        public virtual string? Remarks
        {
            get => _Remarks;
            set { OnPropertyChanging(); _Remarks = value; OnPropertyChanged(); }
        }

        [Column("Specification")]
        public virtual string? Specification
        {
            get => _Specification;
            set { OnPropertyChanging(); _Specification = value; OnPropertyChanged(); }
        }
        [Column("DWT")]
        public virtual string? DWT
        {
            get => _DWT;
            set { OnPropertyChanging(); _DWT = value; OnPropertyChanged(); }
        }
        [Column("IMO")]
        public virtual string? IMO
        {
            get => _IMO;
            set { OnPropertyChanging(); _IMO = value; OnPropertyChanged(); }
        }
        private string? _KKM { get; set; }
        [Column("KKM")]
        public virtual string? KKM
        {
            get => _KKM;
            set { OnPropertyChanging(); _KKM = value; OnPropertyChanged(); }
        }
        private string? _Mualim { get; set; }
        [Column("Mualim")]
        public virtual string? Mualim
        {
            get => _Mualim;
            set { OnPropertyChanging(); _Mualim = value; OnPropertyChanged(); }
        }
        private bool? _AutoRequester { get; set; }
        [Column("AutoRequester")]
        public virtual bool? AutoRequester
        {
            get => _AutoRequester;
            set { OnPropertyChanging(); _AutoRequester = value; OnPropertyChanged(); }
        }
        private string? _Flag { get; set; }
        [Column("Flag")]
        public virtual string? Flag
        {
            get => _Flag;
            set { OnPropertyChanging(); _Flag = value; OnPropertyChanged(); }
        }

        #endregion
        public virtual ObservableCollection<UserVesselRel>? UserVesselRels { get; set; }
        public virtual ObservableCollection<Requisition>? Requisitions { get; set; }

        public virtual InventoryUserGroup? Group { get; set; }
        public virtual ObservableCollection<InventoryUserSpec>? InventoryUserSpecs { get;set;  }
        public virtual ObservableCollection<ROB>? ROBs { get; set; }

    }
}