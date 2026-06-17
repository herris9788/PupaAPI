using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pupa.BusinessObjects.Beesuite
{
    [Table("JobDetail", Schema = "public")]
    public class JobDetail : BaseEntity
    {
        private long _jobId;
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
        public virtual long JobID
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

        // Navigation property
        [ForeignKey("JobID")]
        public virtual Job? Job { get; set; }
    }
}