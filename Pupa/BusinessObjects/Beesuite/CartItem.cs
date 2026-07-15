using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pupa.BusinessObjects.Beesuite
{
    /// <summary>
    /// Item Request cart / history (one row per item a user put in a vessel's cart).
    ///
    /// Flow:
    ///   1. A user keeps adding items to a vessel's cart over time (Status = 'CART').
    ///   2. Later they tick which cart items to process first (partial is allowed).
    ///   3. The system groups the ticked items BY FAMILY (CategoryID) and creates one
    ///      Requisition per family, then marks those cart rows 'PROCESSED' and links
    ///      them to the requisition (RequisitionID / RequisitionNumber).
    ///   4. Un-ticked items stay in the cart; processed rows remain as history.
    ///
    /// Custom (free-text) items have ItemID = NULL and carry ItemName instead.
    /// WizardData holds the Step-4 technical wizard form (_wizForm) as JSON.
    /// </summary>
    [Table("CartItem")]
    public class CartItem : BaseEntity
    {
        public CartItem()
        {
            this.CartItemAttachments = new ObservableCollection<CartItemAttachment>();
        }

        private int _id;
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("ID")]
        public virtual int ID
        {
            get => _id;
            set { OnPropertyChanging(); _id = value; OnPropertyChanged(); }
        }

        // ── Who + which vessel (the cart is grouped per user + vessel) ────────
        private string? _userName;
        [Column("UserName")]
        [Required]
        [MaxLength(100)]
        public virtual string? UserName
        {
            get => _userName;
            set { OnPropertyChanging(); _userName = value; OnPropertyChanged(); }
        }

        private int _vesselID;
        [Column("VesselID")]
        [Required]
        public virtual int VesselID
        {
            get => _vesselID;
            set { OnPropertyChanging(); _vesselID = value; OnPropertyChanged(); }
        }

        private string? _vesselName;
        [Column("VesselName")]
        [MaxLength(255)]
        public virtual string? VesselName
        {
            get => _vesselName;
            set { OnPropertyChanging(); _vesselName = value; OnPropertyChanged(); }
        }

        private string? _companyDB;
        [Column("CompanyDB")]
        [MaxLength(20)]
        public virtual string? CompanyDB
        {
            get => _companyDB;
            set { OnPropertyChanging(); _companyDB = value; OnPropertyChanged(); }
        }

        // ── Scope captured when the item was added ────────────────────────────
        private string? _department;
        [Column("Department")]
        [MaxLength(10)]
        public virtual string? Department
        {
            get => _department;
            set { OnPropertyChanging(); _department = value; OnPropertyChanged(); }
        }

        private string? _subDepartment;
        [Column("SubDepartment")]
        [MaxLength(50)]
        public virtual string? SubDepartment
        {
            get => _subDepartment;
            set { OnPropertyChanging(); _subDepartment = value; OnPropertyChanged(); }
        }

        // ── Family / category — what the system splits by when processing ─────
        private int? _categoryID;
        [Column("CategoryID")]
        public virtual int? CategoryID
        {
            get => _categoryID;
            set { OnPropertyChanging(); _categoryID = value; OnPropertyChanged(); }
        }

        private string? _categoryName;
        [Column("CategoryName")]
        [MaxLength(255)]
        public virtual string? CategoryName
        {
            get => _categoryName;
            set { OnPropertyChanging(); _categoryName = value; OnPropertyChanged(); }
        }

        private string? _familyName;
        [Column("FamilyName")]
        [MaxLength(255)]
        public virtual string? FamilyName
        {
            get => _familyName;
            set { OnPropertyChanging(); _familyName = value; OnPropertyChanged(); }
        }

        // ── The item itself ───────────────────────────────────────────────────
        private int? _itemID;
        [Column("ItemID")]
        public virtual int? ItemID
        {
            get => _itemID;
            set { OnPropertyChanging(); _itemID = value; OnPropertyChanged(); }
        }

        private string? _itemCode;
        [Column("ItemCode")]
        [MaxLength(50)]
        public virtual string? ItemCode
        {
            get => _itemCode;
            set { OnPropertyChanging(); _itemCode = value; OnPropertyChanged(); }
        }

        private string? _itemName;
        [Column("ItemName")]
        [MaxLength(255)]
        public virtual string? ItemName
        {
            get => _itemName;
            set { OnPropertyChanging(); _itemName = value; OnPropertyChanged(); }
        }

        private decimal? _qty = 1;
        [Column("Qty")]
        public virtual decimal? Qty
        {
            get => _qty;
            set { OnPropertyChanging(); _qty = value; OnPropertyChanged(); }
        }

        private int? _uomLevel = 1;
        [Column("UOMLevel")]
        public virtual int? UOMLevel
        {
            get => _uomLevel;
            set { OnPropertyChanging(); _uomLevel = value; OnPropertyChanged(); }
        }

        private string? _uomName;
        [Column("UOMName")]
        [MaxLength(50)]
        public virtual string? UOMName
        {
            get => _uomName;
            set { OnPropertyChanging(); _uomName = value; OnPropertyChanged(); }
        }

        private string? _remarks;
        [Column("Remarks")]
        [MaxLength(1000)]
        public virtual string? Remarks
        {
            get => _remarks;
            set { OnPropertyChanging(); _remarks = value; OnPropertyChanged(); }
        }

        /// <summary>COA — usually empty until approval.</summary>
        private string? _purpose;
        [Column("Purpose")]
        [MaxLength(255)]
        public virtual string? Purpose
        {
            get => _purpose;
            set { OnPropertyChanging(); _purpose = value; OnPropertyChanged(); }
        }

        /// <summary>Wizard purpose (Overhaul / Damage-Trouble / ...).</summary>
        private string? _purposeOfRequest;
        [Column("PurposeOfRequest")]
        [MaxLength(255)]
        public virtual string? PurposeOfRequest
        {
            get => _purposeOfRequest;
            set { OnPropertyChanging(); _purposeOfRequest = value; OnPropertyChanged(); }
        }

        private string? _prDocuments;
        [Column("PRDocuments")]
        [MaxLength(500)]
        public virtual string? PRDocuments
        {
            get => _prDocuments;
            set { OnPropertyChanging(); _prDocuments = value; OnPropertyChanged(); }
        }

        private string? _partBookAttachmentPath;
        [Column("PartBookAttachmentPath")]
        [MaxLength(500)]
        public virtual string? PartBookAttachmentPath
        {
            get => _partBookAttachmentPath;
            set { OnPropertyChanging(); _partBookAttachmentPath = value; OnPropertyChanged(); }
        }

        private string? _requiredEdition;
        [Column("RequiredEdition")]
        [MaxLength(100)]
        public virtual string? RequiredEdition
        {
            get => _requiredEdition;
            set { OnPropertyChanging(); _requiredEdition = value; OnPropertyChanged(); }
        }

        private string? _specification;
        [Column("Specification")]
        public virtual string? Specification
        {
            get => _specification;
            set { OnPropertyChanging(); _specification = value; OnPropertyChanged(); }
        }

        private bool? _isCustom = false;
        [Column("IsCustom")]
        public virtual bool? IsCustom
        {
            get => _isCustom;
            set { OnPropertyChanging(); _isCustom = value; OnPropertyChanged(); }
        }

        /// <summary>Step-4 technical wizard form (_wizForm) serialized as JSON.</summary>
        private string? _wizardData;
        [Column("WizardData", TypeName = "jsonb")]
        public virtual string? WizardData
        {
            get => _wizardData;
            set { OnPropertyChanging(); _wizardData = value; OnPropertyChanged(); }
        }

        // ── Audit ─────────────────────────────────────────────────────────────
        private bool? _isActive = true;
        [Column("IsActive")]
        public virtual bool? IsActive
        {
            get => _isActive;
            set { OnPropertyChanging(); _isActive = value; OnPropertyChanged(); }
        }

        private DateTime? _createdAt = DateTime.UtcNow;
        [Column("CreatedAt")]
        public virtual DateTime? CreatedAt
        {
            get => _createdAt;
            set { OnPropertyChanging(); _createdAt = value; OnPropertyChanged(); }
        }

        private string? _createdBy;
        [Column("CreatedBy")]
        [MaxLength(100)]
        public virtual string? CreatedBy
        {
            get => _createdBy;
            set { OnPropertyChanging(); _createdBy = value; OnPropertyChanged(); }
        }

        private DateTime? _updatedAt = DateTime.UtcNow;
        [Column("UpdatedAt")]
        public virtual DateTime? UpdatedAt
        {
            get => _updatedAt;
            set { OnPropertyChanging(); _updatedAt = value; OnPropertyChanged(); }
        }

        private string? _updatedBy;
        [Column("UpdatedBy")]
        [MaxLength(100)]
        public virtual string? UpdatedBy
        {
            get => _updatedBy;
            set { OnPropertyChanging(); _updatedBy = value; OnPropertyChanged(); }
        }

        // ── Navigation ────────────────────────────────────────────────────────
        [ForeignKey("ItemID")]
        public virtual Item? Item { get; set; }

        public virtual ObservableCollection<CartItemAttachment> CartItemAttachments { get; set; }
    }
}
