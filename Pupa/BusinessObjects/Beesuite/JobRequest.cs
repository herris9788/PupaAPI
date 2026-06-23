using System;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pupa.BusinessObjects.Beesuite
{
    [Table("JobRequest")]
    public class JobRequest : BaseEntity
    {
        private int _id;
        private string _reportNo = string.Empty;
        private string _status = "Draft";
        private string _approvalStatus = "Pending";
        private int _approvalMaxLevel = 1;
        private int? _vesselId;
        private string? _vesselName;
        private string? _companyName;
        private string? _portOfAttendance;
        private DateTime? _requestDate;
        private string? _requestedByName;
        private string? _requestedByRank;
        private string? _remarks;
        private string? _createdBy;
        private DateTime _createdAt = DateTime.UtcNow;
        private DateTime _updatedAt = DateTime.UtcNow;
        private DateTime? _deletedAt;
        private string? _deletedBy;
        private string? _type;
        private string? _approvedBy;
        private DateTime? _approvedAt;
        private string? _rejectedBy;
        private DateTime? _rejectedAt;
        private string? _approvalRemarks;

        // ApprovedBy1 - ApprovedBy7
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

        // VesselInventory
        private int? _vesselInventoryUserRowId;
        private string? _vesselInventoryUserDb;

        public JobRequest()
        {
            Jobs = new ObservableCollection<Job>();
            JobFieldValues = new ObservableCollection<JobFieldValue>();
            Attachments = new ObservableCollection<JobRequestAttachment>();
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
        [StringLength(200)]
        [Column("ReportNo")]
        public virtual string ReportNo
        {
            get => _reportNo;
            set { if (_reportNo == value) return; OnPropertyChanging(); _reportNo = value; OnPropertyChanged(); }
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
        [Column("ApprovalStatus")]
        public virtual string ApprovalStatus
        {
            get => _approvalStatus;
            set { if (_approvalStatus == value) return; OnPropertyChanging(); _approvalStatus = value; OnPropertyChanged(); }
        }

        [Required]
        [Column("ApprovalMaxLevel")]
        [Range(1, 7)]
        public virtual int ApprovalMaxLevel
        {
            get => _approvalMaxLevel;
            set { if (_approvalMaxLevel == value) return; OnPropertyChanging(); _approvalMaxLevel = value; OnPropertyChanged(); }
        }

        [Column("VesselID")]
        public virtual int? VesselID
        {
            get => _vesselId;
            set { if (_vesselId == value) return; OnPropertyChanging(); _vesselId = value; OnPropertyChanged(); }
        }

        [StringLength(255)]
        [Column("VesselName")]
        public virtual string? VesselName
        {
            get => _vesselName;
            set { if (_vesselName == value) return; OnPropertyChanging(); _vesselName = value; OnPropertyChanged(); }
        }

        [StringLength(255)]
        [Column("CompanyName")]
        public virtual string? CompanyName
        {
            get => _companyName;
            set { if (_companyName == value) return; OnPropertyChanging(); _companyName = value; OnPropertyChanged(); }
        }

        [StringLength(255)]
        [Column("PortOfAttendance")]
        public virtual string? PortOfAttendance
        {
            get => _portOfAttendance;
            set { if (_portOfAttendance == value) return; OnPropertyChanging(); _portOfAttendance = value; OnPropertyChanged(); }
        }

        [Column("RequestDate")]
        public virtual DateTime? RequestDate
        {
            get => _requestDate;
            set { if (_requestDate == value) return; OnPropertyChanging(); _requestDate = value; OnPropertyChanged(); }
        }

        [StringLength(150)]
        [Column("RequestedByName")]
        public virtual string? RequestedByName
        {
            get => _requestedByName;
            set { if (_requestedByName == value) return; OnPropertyChanging(); _requestedByName = value; OnPropertyChanged(); }
        }

        [StringLength(150)]
        [Column("RequestedByRank")]
        public virtual string? RequestedByRank
        {
            get => _requestedByRank;
            set { if (_requestedByRank == value) return; OnPropertyChanging(); _requestedByRank = value; OnPropertyChanged(); }
        }

        [Column("Remarks")]
        public virtual string? Remarks
        {
            get => _remarks;
            set { if (_remarks == value) return; OnPropertyChanging(); _remarks = value; OnPropertyChanged(); }
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

        [Column("Type")]
        public virtual string? Type
        {
            get => _type;
            set { if (_type == value) return; OnPropertyChanging(); _type = value; OnPropertyChanged(); }
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

        [Column("VesselInventoryUserRowID")]
        public virtual int? VesselInventoryUserRowID
        {
            get => _vesselInventoryUserRowId;
            set { if (_vesselInventoryUserRowId == value) return; OnPropertyChanging(); _vesselInventoryUserRowId = value; OnPropertyChanged(); }
        }

        [Column("VesselInventoryUserDB")]
        public virtual string? VesselInventoryUserDB
        {
            get => _vesselInventoryUserDb;
            set { if (_vesselInventoryUserDb == value) return; OnPropertyChanging(); _vesselInventoryUserDb = value; OnPropertyChanged(); }
        }

        public virtual ObservableCollection<Job> Jobs { get; set; }
        public virtual ObservableCollection<JobFieldValue>? JobFieldValues { get; set; }
        public virtual ObservableCollection<JobRequestAttachment>? Attachments { get; set; }


        private string? _entityTypeName = "JobRequest";
        public virtual string? EntityTypeName
        {
            get => _entityTypeName;
            private set { _entityTypeName = value; }
        }
    }
}