using System;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pupa.BusinessObjects.Beesuite
{
    [Table("Job")]
    public class Job : BaseEntity
    {
        private int _id;
        private int _jobRequestId;
        private string? _serviceOrderNo;
        private string? _externalId;
        private int? _equipmentId;
        private int _sequenceNo = 1;
        private string _status = "Pending";
        private string _formType = "JobRequestItem";
        private string? _createdBy;
        private DateTime _createdAt = DateTime.UtcNow;
        private DateTime _updatedAt = DateTime.UtcNow;
        private DateTime? _deletedAt;
        private string? _deletedBy;
        private string _approvalStatus = "Pending";
        private string? _approvedBy;
        private DateTime? _approvedAt;
        private string? _rejectedBy;
        private DateTime? _rejectedAt;
        private string? _approvalRemarks;
        private string? _itemCode;
        private int _approvalMaxLevel = 1;

        private string? _approvedBy1;
        private DateTime? _approvedBy1At;
        private string? _approvedBy2;
        private DateTime? _approvedBy2At;
        private string? _approvedBy3;
        private DateTime? _approvedBy3At;
        private string? _approvedBy4;
        private DateTime? _approvedBy4At;
        private string? _approvedBy5;
        private DateTime? _approvedBy5At;
        private string? _approvedBy6;
        private DateTime? _approvedBy6At;
        private string? _approvedBy7;
        private DateTime? _approvedBy7At;
        public Job()
        {
            JobDetails = new ObservableCollection<JobDetail>();
            Attachments = new ObservableCollection<JobAttachment>();
        }

        [Key]
        [Column("ID")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public virtual int ID
        {
            get => _id;
            set { if (_id == value) return; OnPropertyChanging(); _id = value; OnPropertyChanged(); }
        }

        [Required]
        [Column("JobRequestID")]
        public virtual int JobRequestId
        {
            get => _jobRequestId;
            set { if (_jobRequestId == value) return; OnPropertyChanging(); _jobRequestId = value; OnPropertyChanged(); }
        }

        [StringLength(200)]
        [Column("ServiceOrderNo")]
        public virtual string? ServiceOrderNo
        {
            get => _serviceOrderNo;
            set { if (_serviceOrderNo == value) return; OnPropertyChanging(); _serviceOrderNo = value; OnPropertyChanged(); }
        }

        [StringLength(100)]
        [Column("ExternalId")]
        public virtual string? ExternalID
        {
            get => _externalId;
            set { if (_externalId == value) return; OnPropertyChanging(); _externalId = value; OnPropertyChanged(); }
        }

        [Column("EquipmentID")]
        public virtual int? EquipmentID
        {
            get => _equipmentId;
            set { if (_equipmentId == value) return; OnPropertyChanging(); _equipmentId = value; OnPropertyChanged(); }
        }

        [Required]
        [Column("SequenceNo")]
        public virtual int SequenceNo
        {
            get => _sequenceNo;
            set { if (_sequenceNo == value) return; OnPropertyChanging(); _sequenceNo = value; OnPropertyChanged(); }
        }

        [Required]
        [StringLength(50)]
        [Column("Status")]
        public virtual string Status
        {
            get => _status;
            set { if (_status == value) return; OnPropertyChanging(); _status = value; OnPropertyChanged(); }
        }

        [Required]
        [StringLength(50)]
        [Column("FormType")]
        public virtual string FormType
        {
            get => _formType;
            set { if (_formType == value) return; OnPropertyChanging(); _formType = value; OnPropertyChanged(); }
        }

        [StringLength(100)]
        [Column("CreatedBy")]
        public virtual string? CreatedBy
        {
            get => _createdBy;
            set { if (_createdBy == value) return; OnPropertyChanging(); _createdBy = value; OnPropertyChanged(); }
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

        [Column("DeletedAt")]
        public virtual DateTime? DeletedAt
        {
            get => _deletedAt;
            set { if (_deletedAt == value) return; OnPropertyChanging(); _deletedAt = value; OnPropertyChanged(); }
        }

        [StringLength(100)]
        [Column("DeletedBy")]
        public virtual string? DeletedBy
        {
            get => _deletedBy;
            set { if (_deletedBy == value) return; OnPropertyChanging(); _deletedBy = value; OnPropertyChanged(); }
        }

        [Required]
        [StringLength(50)]
        [Column("ApprovalStatus")]
        public virtual string ApprovalStatus
        {
            get => _approvalStatus;
            set { if (_approvalStatus == value) return; OnPropertyChanging(); _approvalStatus = value; OnPropertyChanged(); }
        }

        [StringLength(100)]
        [Column("ApprovedBy")]
        public virtual string? ApprovedBy
        {
            get => _approvedBy;
            set { if (_approvedBy == value) return; OnPropertyChanging(); _approvedBy = value; OnPropertyChanged(); }
        }

        [Column("ApprovedAt")]
        public virtual DateTime? ApprovedAt
        {
            get => _approvedAt;
            set { if (_approvedAt == value) return; OnPropertyChanging(); _approvedAt = value; OnPropertyChanged(); }
        }

        [StringLength(100)]
        [Column("RejectedBy")]
        public virtual string? RejectedBy
        {
            get => _rejectedBy;
            set { if (_rejectedBy == value) return; OnPropertyChanging(); _rejectedBy = value; OnPropertyChanged(); }
        }

        [Column("RejectedAt")]
        public virtual DateTime? RejectedAt
        {
            get => _rejectedAt;
            set { if (_rejectedAt == value) return; OnPropertyChanging(); _rejectedAt = value; OnPropertyChanged(); }
        }

        [Column("ApprovalRemarks")]
        public virtual string? ApprovalRemarks
        {
            get => _approvalRemarks;
            set { if (_approvalRemarks == value) return; OnPropertyChanging(); _approvalRemarks = value; OnPropertyChanged(); }
        }

        [Column("ItemCode")]
        public virtual string? ItemCode
        {
            get => _itemCode;
            set { if (_itemCode == value) return; OnPropertyChanging(); _itemCode = value; OnPropertyChanged(); }
        }


        [Required]
        [Range(1, 7)]
        [Column("ApprovalMaxLevel")]
        public virtual int ApprovalMaxLevel
        {
            get => _approvalMaxLevel;
            set { if (_approvalMaxLevel == value) return; OnPropertyChanging(); _approvalMaxLevel = value; OnPropertyChanged(); }
        }

        // ApprovedBy1 - ApprovedBy7
        [StringLength(100)][Column("ApprovedBy1")] public virtual string? ApprovedBy1 { get => _approvedBy1; set { if (_approvedBy1 == value) return; OnPropertyChanging(); _approvedBy1 = value; OnPropertyChanged(); } }
        [Column("ApprovedBy1At")] public virtual DateTime? ApprovedBy1At { get => _approvedBy1At; set { if (_approvedBy1At == value) return; OnPropertyChanging(); _approvedBy1At = value; OnPropertyChanged(); } }

        [StringLength(100)][Column("ApprovedBy2")] public virtual string? ApprovedBy2 { get => _approvedBy2; set { if (_approvedBy2 == value) return; OnPropertyChanging(); _approvedBy2 = value; OnPropertyChanged(); } }
        [Column("ApprovedBy2At")] public virtual DateTime? ApprovedBy2At { get => _approvedBy2At; set { if (_approvedBy2At == value) return; OnPropertyChanging(); _approvedBy2At = value; OnPropertyChanged(); } }

        [StringLength(100)][Column("ApprovedBy3")] public virtual string? ApprovedBy3 { get => _approvedBy3; set { if (_approvedBy3 == value) return; OnPropertyChanging(); _approvedBy3 = value; OnPropertyChanged(); } }
        [Column("ApprovedBy3At")] public virtual DateTime? ApprovedBy3At { get => _approvedBy3At; set { if (_approvedBy3At == value) return; OnPropertyChanging(); _approvedBy3At = value; OnPropertyChanged(); } }

        [StringLength(100)][Column("ApprovedBy4")] public virtual string? ApprovedBy4 { get => _approvedBy4; set { if (_approvedBy4 == value) return; OnPropertyChanging(); _approvedBy4 = value; OnPropertyChanged(); } }
        [Column("ApprovedBy4At")] public virtual DateTime? ApprovedBy4At { get => _approvedBy4At; set { if (_approvedBy4At == value) return; OnPropertyChanging(); _approvedBy4At = value; OnPropertyChanged(); } }

        [StringLength(100)][Column("ApprovedBy5")] public virtual string? ApprovedBy5 { get => _approvedBy5; set { if (_approvedBy5 == value) return; OnPropertyChanging(); _approvedBy5 = value; OnPropertyChanged(); } }
        [Column("ApprovedBy5At")] public virtual DateTime? ApprovedBy5At { get => _approvedBy5At; set { if (_approvedBy5At == value) return; OnPropertyChanging(); _approvedBy5At = value; OnPropertyChanged(); } }

        [StringLength(100)][Column("ApprovedBy6")] public virtual string? ApprovedBy6 { get => _approvedBy6; set { if (_approvedBy6 == value) return; OnPropertyChanging(); _approvedBy6 = value; OnPropertyChanged(); } }
        [Column("ApprovedBy6At")] public virtual DateTime? ApprovedBy6At { get => _approvedBy6At; set { if (_approvedBy6At == value) return; OnPropertyChanging(); _approvedBy6At = value; OnPropertyChanged(); } }

        [StringLength(100)][Column("ApprovedBy7")] public virtual string? ApprovedBy7 { get => _approvedBy7; set { if (_approvedBy7 == value) return; OnPropertyChanging(); _approvedBy7 = value; OnPropertyChanged(); } }
        [Column("ApprovedBy7At")] public virtual DateTime? ApprovedBy7At { get => _approvedBy7At; set { if (_approvedBy7At == value) return; OnPropertyChanging(); _approvedBy7At = value; OnPropertyChanged(); } }

        // Navigation properties
        [ForeignKey("JobRequestId")]
        public virtual JobRequest? JobRequest { get; set; }

        //[ForeignKey("EquipmentId")]
        //public virtual Equipment? Equipment { get; set; }
        public virtual ObservableCollection<JobDetail> JobDetails { get; set; }

        private string? _entityTypeName = "Job";

        public virtual string? EntityTypeName
        {
            get => _entityTypeName;
            private set { _entityTypeName = value; }
        }
        public virtual ObservableCollection<JobFieldValue>? JobFieldValues { get; set; }
        public virtual ObservableCollection<JobAttachment>? Attachments { get; set; }
    }
}