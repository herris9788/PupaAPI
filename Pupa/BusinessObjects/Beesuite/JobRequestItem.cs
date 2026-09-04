using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pupa.BusinessObjects.Beesuite
{
    /// <summary>
    /// Lookup table mapping a Job Request configuration to the inventory item
    /// that should be used for it (ItemCode / ItemName / ItemID). Columns are
    /// shared across job types with different shapes: Electronics uses
    /// Category / Brand / Type / PK; Underwater Inspection uses JobType /
    /// Position / Obstacle / VesselClass / QtyAvail. "Group" and "ServiceType"
    /// apply to both.
    /// </summary>
    [Table("JobRequestItem")]
    public class JobRequestItem : BaseEntity
    {
        private int _id;
        private string? _group;
        private string? _category;
        private string? _position;
        private string? _brand;
        private string? _type;
        private string? _pk;
        private string? _serviceType;
        private string? _itemCode;
        private string? _itemName;
        private int? _itemId;
        private DateTime _createdAt = DateTime.UtcNow;
        private DateTime _updatedAt = DateTime.UtcNow;
        private string? _jobType;
        private string? _obstacle;
        private string? _qtyAvail;
        private string? _vesselClass;

        [Key]
        [Column("ID")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public virtual int ID
        {
            get => _id;
            set { if (_id == value) return; OnPropertyChanging(); _id = value; OnPropertyChanged(); }
        }

        [StringLength(100)]
        [Column("Group")]
        public virtual string? Group
        {
            get => _group;
            set { if (_group == value) return; OnPropertyChanging(); _group = value; OnPropertyChanged(); }
        }

        [StringLength(150)]
        [Column("Category")]
        public virtual string? Category
        {
            get => _category;
            set { if (_category == value) return; OnPropertyChanging(); _category = value; OnPropertyChanged(); }
        }

        [StringLength(150)]
        [Column("Position")]
        public virtual string? Position
        {
            get => _position;
            set { if (_position == value) return; OnPropertyChanging(); _position = value; OnPropertyChanged(); }
        }

        [StringLength(100)]
        [Column("Brand")]
        public virtual string? Brand
        {
            get => _brand;
            set { if (_brand == value) return; OnPropertyChanging(); _brand = value; OnPropertyChanged(); }
        }

        [StringLength(150)]
        [Column("Type")]
        public virtual string? Type
        {
            get => _type;
            set { if (_type == value) return; OnPropertyChanging(); _type = value; OnPropertyChanged(); }
        }

        [StringLength(50)]
        [Column("PK")]
        public virtual string? PK
        {
            get => _pk;
            set { if (_pk == value) return; OnPropertyChanging(); _pk = value; OnPropertyChanged(); }
        }

        [StringLength(255)]
        [Column("ServiceType")]
        public virtual string? ServiceType
        {
            get => _serviceType;
            set { if (_serviceType == value) return; OnPropertyChanging(); _serviceType = value; OnPropertyChanged(); }
        }

        [StringLength(100)]
        [Column("ItemCode")]
        public virtual string? ItemCode
        {
            get => _itemCode;
            set { if (_itemCode == value) return; OnPropertyChanging(); _itemCode = value; OnPropertyChanged(); }
        }

        [StringLength(255)]
        [Column("ItemName")]
        public virtual string? ItemName
        {
            get => _itemName;
            set { if (_itemName == value) return; OnPropertyChanging(); _itemName = value; OnPropertyChanged(); }
        }

        [Column("ItemID")]
        public virtual int? ItemID
        {
            get => _itemId;
            set { if (_itemId == value) return; OnPropertyChanging(); _itemId = value; OnPropertyChanged(); }
        }

        [Column("CreatedAt")]
        public virtual DateTime CreatedAt
        {
            get => _createdAt;
            set { if (_createdAt == value) return; OnPropertyChanging(); _createdAt = value; OnPropertyChanged(); }
        }

        [Column("UpdatedAt")]
        public virtual DateTime UpdatedAt
        {
            get => _updatedAt;
            set { if (_updatedAt == value) return; OnPropertyChanging(); _updatedAt = value; OnPropertyChanged(); }
        }

        /// <summary>Underwater Inspection only: e.g. "PENGECEKAN", "PEMBERSIHAN", or the
        /// root "UNDERWATER INSPECTION (UWILD)" / "(TOCA)" values.</summary>
        [Column("JobType")]
        public virtual string? JobType
        {
            get => _jobType;
            set { if (_jobType == value) return; OnPropertyChanging(); _jobType = value; OnPropertyChanged(); }
        }

        /// <summary>Underwater Inspection only: e.g. "TERITIP", "TALI/JARING/TERPAL".</summary>
        [Column("Obstacle")]
        public virtual string? Obstacle
        {
            get => _obstacle;
            set { if (_obstacle == value) return; OnPropertyChanging(); _obstacle = value; OnPropertyChanged(); }
        }

        /// <summary>Underwater Inspection only: raw quantity spec — blank (no qty),
        /// a plain number (fixed), "[1,2]" (a choice), or "&lt;INPUT&gt;" (free entry).
        /// The client is responsible for parsing this format.</summary>
        [Column("QtyAvail")]
        public virtual string? QtyAvail
        {
            get => _qtyAvail;
            set { if (_qtyAvail == value) return; OnPropertyChanging(); _qtyAvail = value; OnPropertyChanged(); }
        }

        /// <summary>Underwater Inspection only: classification society for the
        /// UWILD/TOCA root rows — "CCS", "ABS", "BKI", "TULIS SENDIRI".</summary>
        [Column("VesselClass")]
        public virtual string? VesselClass
        {
            get => _vesselClass;
            set { if (_vesselClass == value) return; OnPropertyChanging(); _vesselClass = value; OnPropertyChanged(); }
        }
        private string? _Section { get; set; }
        [Column("Section")]
        public virtual string? Section
        {
            get => _Section;
            set { if (_Section == value) return; OnPropertyChanging(); _Section = value; OnPropertyChanged(); }
        }
        private string? _Side { get; set; }
        [Column("Side")]
        public virtual string? Side
        {
            get => _Side;
            set { if (_Side == value) return; OnPropertyChanging(); _Side = value; OnPropertyChanged(); }
        }
        private string? _Action { get; set; }
        [Column("Action")]
        public virtual string? Action
        {
            get => _Action;
            set { if (_Action == value) return; OnPropertyChanging(); _Action = value; OnPropertyChanged(); }
        }
        private string? _Specification { get; set; }
        [Column("Specification")]
        public virtual string? Specification
        {
            get => _Specification;
            set { if (_Specification == value) return; OnPropertyChanging(); _Specification = value; OnPropertyChanged(); }
        }
        private string? _SpecificationAscend { get; set; }
        [Column("SpecificationAscend")]
        public virtual string? SpecificationAscend
        {
            get => _SpecificationAscend;
            set { if (_SpecificationAscend == value) return; OnPropertyChanging(); _SpecificationAscend = value; OnPropertyChanged(); }
        }
    }
}
