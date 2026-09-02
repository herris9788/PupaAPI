using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pupa.BusinessObjects.Beesuite
{
    [Table("JobDetail", Schema = "public")]
    public class JobDetail : BaseEntity
    {
        private int _jobId;
        private string? _category;
        private string? _conductBy;
        private string? _jobType;
        private string? _equipmentName;
        private string? _serialNumber;
        private string? _equipmentPosition;
        private string? _equipmentPositionOther;
        private string? _jobRequest;
        private DateTime _createdAt = DateTime.UtcNow;
        private DateTime _updatedAt = DateTime.UtcNow;
        private string? _measurement;
        private string? _tankName;
        private string? _repairSpecialBehaviour;

        [Key]
        [Column("JobID")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)] // PK sekaligus FK, tidak auto-generate
        public virtual int JobID
        {
            get => _jobId;
            set { if (_jobId == value) return; OnPropertyChanging(); _jobId = value; OnPropertyChanged(); }
        }

        [StringLength(50)]
        [Column("Category")]
        public virtual string? Category
        {
            get => _category;
            set { if (_category == value) return; OnPropertyChanging(); _category = value; OnPropertyChanged(); }
        }

        [StringLength(100)]
        [Column("ConductBy")]
        public virtual string? ConductBy
        {
            get => _conductBy;
            set { if (_conductBy == value) return; OnPropertyChanging(); _conductBy = value; OnPropertyChanged(); }
        }

        [StringLength(100)]
        [Column("JobType")]
        public virtual string? JobType
        {
            get => _jobType;
            set { if (_jobType == value) return; OnPropertyChanging(); _jobType = value; OnPropertyChanged(); }
        }

        [StringLength(255)]
        [Column("EquipmentName")]
        public virtual string? EquipmentName
        {
            get => _equipmentName;
            set { if (_equipmentName == value) return; OnPropertyChanging(); _equipmentName = value; OnPropertyChanged(); }
        }

        [StringLength(100)]
        [Column("SerialNumber")]
        public virtual string? SerialNumber
        {
            get => _serialNumber;
            set { if (_serialNumber == value) return; OnPropertyChanging(); _serialNumber = value; OnPropertyChanged(); }
        }

        [StringLength(100)]
        [Column("EquipmentPosition")]
        public virtual string? EquipmentPosition
        {
            get => _equipmentPosition;
            set { if (_equipmentPosition == value) return; OnPropertyChanging(); _equipmentPosition = value; OnPropertyChanged(); }
        }

        [StringLength(255)]
        [Column("EquipmentPositionOther")]
        public virtual string? EquipmentPositionOther
        {
            get => _equipmentPositionOther;
            set { if (_equipmentPositionOther == value) return; OnPropertyChanging(); _equipmentPositionOther = value; OnPropertyChanged(); }
        }

        [Column("JobRequest")]
        public virtual string? JobRequest
        {
            get => _jobRequest;
            set { if (_jobRequest == value) return; OnPropertyChanging(); _jobRequest = value; OnPropertyChanged(); }
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

        [StringLength(100)]
        [Column("Measurement")]
        public virtual string? Measurement
        {
            get => _measurement;
            set { if (_measurement == value) return; OnPropertyChanging(); _measurement = value; OnPropertyChanged(); }
        }

        [StringLength(50)]
        [Column("TankName")]
        public virtual string? TankName
        {
            get => _tankName;
            set { if (_tankName == value) return; OnPropertyChanging(); _tankName = value; OnPropertyChanged(); }
        }

        [StringLength(50)]
        [Column("RepairSpecialBehaviour")]
        public virtual string? RepairSpecialBehaviour
        {
            get => _repairSpecialBehaviour;
            set { if (_repairSpecialBehaviour == value) return; OnPropertyChanging(); _repairSpecialBehaviour = value; OnPropertyChanged(); }
        }

        private int? _ItemID { get; set; }
        [Column("ItemID")]
        public virtual int? ItemID
        {
            get => _ItemID;
            set { if (_ItemID == value) return; OnPropertyChanging(); _ItemID = value; OnPropertyChanged(); }
        }

        private string? _Remarks { get; set; }
        [Column("Remarks")]
        public virtual string? Remarks
        {
            get => _Remarks;
            set { if (_Remarks == value) return; OnPropertyChanging(); _Remarks = value; OnPropertyChanged(); }
        }

        // ── Per-item approval quantities (mirrors RequisitionDetail) ──────────────
        // Job Request approval works like Item Request: the requested quantity is the
        // baseline, and each approver on the document-level chain may adjust the
        // approved quantity per item. QtyApproved holds the current/running value;
        // QtyApproved1..7 snapshot the value each approval level signed off on.
        private decimal? _QtyRequest = 0;
        [Column("QtyRequest")]
        public virtual decimal? QtyRequest
        {
            get => _QtyRequest;
            set { if (_QtyRequest == value) return; OnPropertyChanging(); _QtyRequest = value; OnPropertyChanged(); }
        }

        private decimal? _QtyApproved = 0;
        [Column("QtyApproved")]
        public virtual decimal? QtyApproved
        {
            get => _QtyApproved;
            set { if (_QtyApproved == value) return; OnPropertyChanging(); _QtyApproved = value; OnPropertyChanged(); }
        }

        private decimal? _QtyApproved1 { get; set; }
        [Column("QtyApproved1")]
        public virtual decimal? QtyApproved1
        {
            get => _QtyApproved1;
            set { if (_QtyApproved1 == value) return; OnPropertyChanging(); _QtyApproved1 = value; OnPropertyChanged(); }
        }

        private decimal? _QtyApproved2 { get; set; }
        [Column("QtyApproved2")]
        public virtual decimal? QtyApproved2
        {
            get => _QtyApproved2;
            set { if (_QtyApproved2 == value) return; OnPropertyChanging(); _QtyApproved2 = value; OnPropertyChanged(); }
        }

        private decimal? _QtyApproved3 { get; set; }
        [Column("QtyApproved3")]
        public virtual decimal? QtyApproved3
        {
            get => _QtyApproved3;
            set { if (_QtyApproved3 == value) return; OnPropertyChanging(); _QtyApproved3 = value; OnPropertyChanged(); }
        }

        private decimal? _QtyApproved4 { get; set; }
        [Column("QtyApproved4")]
        public virtual decimal? QtyApproved4
        {
            get => _QtyApproved4;
            set { if (_QtyApproved4 == value) return; OnPropertyChanging(); _QtyApproved4 = value; OnPropertyChanged(); }
        }

        private decimal? _QtyApproved5 { get; set; }
        [Column("QtyApproved5")]
        public virtual decimal? QtyApproved5
        {
            get => _QtyApproved5;
            set { if (_QtyApproved5 == value) return; OnPropertyChanging(); _QtyApproved5 = value; OnPropertyChanged(); }
        }

        private decimal? _QtyApproved6 { get; set; }
        [Column("QtyApproved6")]
        public virtual decimal? QtyApproved6
        {
            get => _QtyApproved6;
            set { if (_QtyApproved6 == value) return; OnPropertyChanging(); _QtyApproved6 = value; OnPropertyChanged(); }
        }

        private decimal? _QtyApproved7 { get; set; }
        [Column("QtyApproved7")]
        public virtual decimal? QtyApproved7
        {
            get => _QtyApproved7;
            set { if (_QtyApproved7 == value) return; OnPropertyChanging(); _QtyApproved7 = value; OnPropertyChanged(); }
        }

        private int? _UOMLevel { get; set; }
        [Column("UOMLevel")]
        public virtual int? UOMLevel
        {
            get => _UOMLevel;
            set { if (_UOMLevel == value) return; OnPropertyChanging(); _UOMLevel = value; OnPropertyChanged(); }
        }

        [ForeignKey("ItemID")]
        public virtual Item? Item { get; set;  }

        // Navigation property
        [ForeignKey("JobID")]
        public virtual Job? Job { get; set; }
    }
}