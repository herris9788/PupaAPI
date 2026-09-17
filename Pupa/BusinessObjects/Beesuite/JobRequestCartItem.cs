using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pupa.BusinessObjects.Beesuite
{
    /// <summary>
    /// Job Request draft cart (one row per not-yet-submitted request card in the
    /// Job Request wizard's "Number of Requests" list), mirroring CartItem's
    /// role for Item Request.
    ///
    /// Flow:
    ///   1. A user fills in one or more request cards for a vessel (Category /
    ///      Subcategory already picked for the whole draft — see CategoryName /
    ///      SubCategoryName below).
    ///   2. Every meaningful edit write-throughs here so the draft survives a
    ///      reload/navigate-away (see JobRequestPageWeb.dart's _syncCartToDb).
    ///   3. On successful submit, every row for that user + vessel is deleted —
    ///      a Job Request draft never lingers as "history" the way Item
    ///      Request's cart rows do after being marked PROCESSED.
    ///
    /// WizardData holds the full per-request form map (all the dynamic
    /// category-specific fields — AcBrand, DispProblem, etc.) as JSON, minus
    /// its in-memory-only PlatformFile attachments (never round-tripped, same
    /// convention as CartItem/WizardData for Item Request).
    /// </summary>
    [Table("JobRequestCartItem")]
    public class JobRequestCartItem : BaseEntity
    {
        private int _id;
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("ID")]
        public virtual int ID
        {
            get => _id;
            set { OnPropertyChanging(); _id = value; OnPropertyChanged(); }
        }

        // ── Who + which vessel (the draft is grouped per user + vessel) ────────
        private string? _userName;
        [Column("UserName")]
        [Required]
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
        public virtual string? VesselName
        {
            get => _vesselName;
            set { OnPropertyChanging(); _vesselName = value; OnPropertyChanged(); }
        }

        private string? _companyDB;
        [Column("CompanyDB")]
        public virtual string? CompanyDB
        {
            get => _companyDB;
            set { OnPropertyChanging(); _companyDB = value; OnPropertyChanged(); }
        }

        // ── Category / Subcategory picked for the whole draft ──────────────────
        private string? _categoryName;
        [Column("CategoryName")]
        public virtual string? CategoryName
        {
            get => _categoryName;
            set { OnPropertyChanging(); _categoryName = value; OnPropertyChanged(); }
        }

        private string? _subCategoryName;
        [Column("SubCategoryName")]
        public virtual string? SubCategoryName
        {
            get => _subCategoryName;
            set { OnPropertyChanging(); _subCategoryName = value; OnPropertyChanged(); }
        }

        /// <summary>JobFormType.name — informational only, not used to resolve the form.</summary>
        private string? _formType;
        [Column("FormType")]
        public virtual string? FormType
        {
            get => _formType;
            set { OnPropertyChanging(); _formType = value; OnPropertyChanged(); }
        }

        /// <summary>This row's position within the "Number of Requests" list.</summary>
        private int? _sortOrder = 0;
        [Column("SortOrder")]
        public virtual int? SortOrder
        {
            get => _sortOrder;
            set { OnPropertyChanging(); _sortOrder = value; OnPropertyChanged(); }
        }

        /// <summary>Shared Review-step Purpose text, repeated on every row of the draft.</summary>
        private string? _orderPurpose;
        [Column("OrderPurpose")]
        public virtual string? OrderPurpose
        {
            get => _orderPurpose;
            set { OnPropertyChanging(); _orderPurpose = value; OnPropertyChanged(); }
        }

        /// <summary>Calibration's On Board / Ashore pick, shared across the draft.</summary>
        private string? _calibrationLocation;
        [Column("CalibrationLocation")]
        public virtual string? CalibrationLocation
        {
            get => _calibrationLocation;
            set { OnPropertyChanging(); _calibrationLocation = value; OnPropertyChanged(); }
        }

        /// <summary>This request card's full dynamic form field map, serialized as JSON.</summary>
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
        public virtual string? UpdatedBy
        {
            get => _updatedBy;
            set { OnPropertyChanging(); _updatedBy = value; OnPropertyChanged(); }
        }
    }
}
