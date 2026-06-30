using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pupa.BusinessObjects.Beesuite
{
    [Table("Requisition")]
    public class Requisition : BaseEntity
    {
        private int? _vesselId;
        private DateTime? _date = DateTime.Now;
        private string? _spec;
        private string? _attachment;
        private string? _requestBy;
        private string? _approvedBy1;
        private string? _approvedBy2;
        private string? _approvedByPurchasing;
        private bool? _approved;
        private string? _status;
        private string? _revertStatus;
        private string? _requisitionNumber;
        private int? _categoryID;

        private bool? _requestUnderReview;
        private DateTime? _requestUnderReviewTime;
        private DateTime? _receivedTime;




        private DateTime? _createdAt = DateTime.Now;
        private DateTime? _updatedAt = DateTime.Now;
        private string? _subDepartment;

        public Requisition()
        {
            RequisitionDetails = new ObservableCollection<RequisitionDetail>();
            RequisitionAttachmentRels = new ObservableCollection<RequisitionAttachmentRel>();
            EngineNumbers = new ObservableCollection<RequisitionEngineNumber>();
        }

        [Key]
        public virtual int ID
        {
            get; set;
        }

        public virtual int? VesselID
        {
            get => _vesselId;
            set
            {
                if (_vesselId == value) return;
                OnPropertyChanging();
                _vesselId = value;
                OnPropertyChanged();
            }
        }

        public virtual DateTime? Date
        {
            get => _date;
            set
            {
                if (_date == value) return;
                OnPropertyChanging();
                _date = value;
                OnPropertyChanged();
            }
        }

        [StringLength(500)]
        public virtual string? Spec
        {
            get => _spec;
            set
            {
                if (_spec == value) return;
                OnPropertyChanging();
                _spec = value;
                OnPropertyChanged();
            }
        }

        public virtual string? Attachment
        {
            get => _attachment;
            set
            {
                if (_attachment == value) return;
                OnPropertyChanging();
                _attachment = value;
                OnPropertyChanged();
            }
        }
        private string? _AttachmentName { get; set; }
        public virtual string? AttachmentName
        {
            get => _AttachmentName;
            set
            {
                if (_AttachmentName == value) return;
                OnPropertyChanging();
                _AttachmentName = value;
                OnPropertyChanged();
            }
        }

        [StringLength(500)]
        public virtual string? RequestBy
        {
            get => _requestBy;
            set
            {
                if (_requestBy == value) return;
                OnPropertyChanging();
                _requestBy = value;
                OnPropertyChanged();
            }
        }
        [StringLength(500)]
        public virtual string? ApprovedBy1
        {
            get => _approvedBy1;
            set
            {
                if (_approvedBy1 == value) return;
                OnPropertyChanging();
                _approvedBy1 = value;
                OnPropertyChanged();
            }
        }
        [StringLength(500)]
        public virtual string? ApprovedBy2
        {
            get => _approvedBy2;
            set
            {
                if (_approvedBy2 == value) return;
                OnPropertyChanging();
                _approvedBy2 = value;
                OnPropertyChanged();
            }
        }
        private string? _approvedBy3 { get; set; }
        [StringLength(500)]
        public virtual string? ApprovedBy3
        {
            get => _approvedBy3;
            set
            {
                if (_approvedBy3 == value) return;
                OnPropertyChanging();
                _approvedBy3 = value;
                OnPropertyChanged();
            }
        }
        private string? _approvedBy4 { get; set; }
        [StringLength(500)]
        public virtual string? ApprovedBy4
        {
            get => _approvedBy4;
            set
            {
                if (_approvedBy4 == value) return;
                OnPropertyChanging();
                _approvedBy4 = value;
                OnPropertyChanged();
            }
        }
        private string? _approvedBy5 { get; set; }
        [StringLength(500)]
        public virtual string? ApprovedBy5
        {
            get => _approvedBy5;
            set
            {
                if (_approvedBy5 == value) return;
                OnPropertyChanging();
                _approvedBy5 = value;
                OnPropertyChanged();
            }
        }
        private string? _approvedBy6 { get; set; }
        [StringLength(500)]
        public virtual string? ApprovedBy6
        {
            get => _approvedBy6;
            set
            {
                if (_approvedBy6 == value) return;
                OnPropertyChanging();
                _approvedBy6 = value;
                OnPropertyChanged();
            }
        }
        private string? _approvedBy7 { get; set; }
        [StringLength(500)]
        public virtual string? ApprovedBy7
        {
            get => _approvedBy7;
            set
            {
                if (_approvedBy7 == value) return;
                OnPropertyChanging();
                _approvedBy7 = value;
                OnPropertyChanged();
            }
        }

        // Delegation audit: when an approval is performed by a delegate on behalf of
        // the original approver, ApprovedByN keeps the ORIGINAL approver, while
        // ApprovedByNActualBy records the delegate who actually performed it.
        // Null = the original approver acted themselves (no delegation).
        private string? _approvedBy1ActualBy { get; set; }
        [StringLength(500)]
        public virtual string? ApprovedBy1ActualBy
        {
            get => _approvedBy1ActualBy;
            set { if (_approvedBy1ActualBy == value) return; OnPropertyChanging(); _approvedBy1ActualBy = value; OnPropertyChanged(); }
        }
        private string? _approvedBy2ActualBy { get; set; }
        [StringLength(500)]
        public virtual string? ApprovedBy2ActualBy
        {
            get => _approvedBy2ActualBy;
            set { if (_approvedBy2ActualBy == value) return; OnPropertyChanging(); _approvedBy2ActualBy = value; OnPropertyChanged(); }
        }
        private string? _approvedBy3ActualBy { get; set; }
        [StringLength(500)]
        public virtual string? ApprovedBy3ActualBy
        {
            get => _approvedBy3ActualBy;
            set { if (_approvedBy3ActualBy == value) return; OnPropertyChanging(); _approvedBy3ActualBy = value; OnPropertyChanged(); }
        }
        private string? _approvedBy4ActualBy { get; set; }
        [StringLength(500)]
        public virtual string? ApprovedBy4ActualBy
        {
            get => _approvedBy4ActualBy;
            set { if (_approvedBy4ActualBy == value) return; OnPropertyChanging(); _approvedBy4ActualBy = value; OnPropertyChanged(); }
        }
        private string? _approvedBy5ActualBy { get; set; }
        [StringLength(500)]
        public virtual string? ApprovedBy5ActualBy
        {
            get => _approvedBy5ActualBy;
            set { if (_approvedBy5ActualBy == value) return; OnPropertyChanging(); _approvedBy5ActualBy = value; OnPropertyChanged(); }
        }
        private string? _approvedBy6ActualBy { get; set; }
        [StringLength(500)]
        public virtual string? ApprovedBy6ActualBy
        {
            get => _approvedBy6ActualBy;
            set { if (_approvedBy6ActualBy == value) return; OnPropertyChanging(); _approvedBy6ActualBy = value; OnPropertyChanged(); }
        }
        private string? _approvedBy7ActualBy { get; set; }
        [StringLength(500)]
        public virtual string? ApprovedBy7ActualBy
        {
            get => _approvedBy7ActualBy;
            set { if (_approvedBy7ActualBy == value) return; OnPropertyChanging(); _approvedBy7ActualBy = value; OnPropertyChanged(); }
        }

        private bool? _Received { get; set; }
        public virtual bool? Received
        {
            get => _Received;
            set
            {
                if (_Received == value) return;
                OnPropertyChanging();
                _Received = value;
                OnPropertyChanged();
            }
        }
        private bool? _POGenerated { get; set; }
        public virtual bool? POGenerated
        {
            get => _POGenerated;
            set
            {
                if (_POGenerated == value) return;
                OnPropertyChanging();
                _POGenerated = value;
                OnPropertyChanged();
            }
        }

        private bool? _ReceivedAtWarehouse { get; set; }
        public virtual bool? ReceivedAtWarehouse
        {
            get => _ReceivedAtWarehouse;
            set
            {
                if (_ReceivedAtWarehouse == value) return;
                OnPropertyChanging();
                _ReceivedAtWarehouse = value;
                OnPropertyChanged();
            }
        }
        private bool? _DeliveredToWarehouse { get; set; }
        public virtual bool? DeliveredToWarehouse
        {
            get => _DeliveredToWarehouse;
            set
            {
                if (_DeliveredToWarehouse == value) return;
                OnPropertyChanging();
                _DeliveredToWarehouse = value;
                OnPropertyChanged();
            }
        }
        public virtual bool? Approved
        {
            get => _approved;
            set
            {
                if (_approved == value) return;
                OnPropertyChanging();
                _approved = value;
                OnPropertyChanged();
            }
        }
        [StringLength(500)]
        public virtual string? Status
        {
            get => _status;
            set
            {
                if (_status == value) return;
                OnPropertyChanging();
                _status = value;
                OnPropertyChanged();
            }
        }
        [StringLength(20)]
        public virtual string? RevertStatus
        {
            get => _revertStatus;
            set
            {
                if (_revertStatus == value) return;
                OnPropertyChanging();
                _revertStatus = value;
                OnPropertyChanged();
            }
        }
        [StringLength(500)]
        public virtual string? RequisitionNumber
        {
            get => _requisitionNumber;
            set
            {
                if (_requisitionNumber == value) return;
                OnPropertyChanging();
                _requisitionNumber = value;
                OnPropertyChanged();
            }
        }
        public virtual int? CategoryID
        {
            get => _categoryID;
            set
            {
                if (_categoryID == value) return;
                OnPropertyChanging();
                _categoryID = value;
                OnPropertyChanged();
            }
        }


        public virtual DateTime? CreatedAt
        {
            get => _createdAt;
            set
            {
                if (_createdAt == value) return;
                OnPropertyChanging();
                _createdAt = value;
                OnPropertyChanged();
            }
        }

        public virtual DateTime? UpdatedAt
        {
            get => _updatedAt;
            set
            {
                if (_updatedAt == value) return;
                OnPropertyChanging();
                _updatedAt = value;
                OnPropertyChanged();
            }
        }

        public virtual ObservableCollection<RequisitionDetail> RequisitionDetails { get; set; }
        [ForeignKey("CategoryID")]
        public virtual StockFamily? Category { get; set; }
        [ForeignKey("VesselID")]
        public virtual InventoryUser? InventoryUser { get; set; }
        public virtual ObservableCollection<RequisitionAttachmentRel>? RequisitionAttachmentRels { get; set; }
        
        private string? _RunningHours { get; set; }
        public virtual string? RunningHours
        {
            get => _RunningHours;
            set
            {
                if (_RunningHours == value) return;
                OnPropertyChanging();
                _RunningHours = value;
                OnPropertyChanged();
            }
        }
        private string? _Machine { get; set; }
        public virtual string? Machine
        {
            get => _Machine;
            set
            {
                if (_Machine == value) return;
                OnPropertyChanging();
                _Machine = value;
                OnPropertyChanged();
            }
        }
        private int? _NumberOfRepair { get; set; }
        public virtual int? NumberOfRepair
        {
            get => _NumberOfRepair;
            set
            {
                if (_NumberOfRepair == value) return;
                OnPropertyChanging();
                _NumberOfRepair = value;
                OnPropertyChanged();
            }
        }
        private bool? _IsOriginalElectricMotor { get; set; }
        public virtual bool? IsOriginalElectricMotor
        {
            get => _IsOriginalElectricMotor;
            set
            {
                if (_IsOriginalElectricMotor == value) return;
                OnPropertyChanging();
                _IsOriginalElectricMotor = value;
                OnPropertyChanged();
            }
        }
        private string? _MotorType { get; set; }
        public virtual string? MotorType
        {
            get => _MotorType;
            set
            {
                if (_MotorType == value) return;
                OnPropertyChanging();
                _MotorType = value;
                OnPropertyChanged();
            }
        }
        private string? _Reason { get; set; }
        public virtual string? Reason
        {
            get => _Reason;
            set
            {
                if (_Reason == value) return;
                OnPropertyChanging();
                _Reason = value;
                OnPropertyChanged();
            }
        }
        private int? _ApprovalMaxLevel { get; set; }
        public virtual int? ApprovalMaxLevel
        {
            get => _ApprovalMaxLevel;
            set
            {
                if (_ApprovalMaxLevel == value) return;
                OnPropertyChanging();
                _ApprovalMaxLevel = value;
                OnPropertyChanged();
            }
        }
        private DateTime? _ApprovedBy1At { get; set; }
        public virtual DateTime? ApprovedBy1At
        {
            get => _ApprovedBy1At;
            set
            {
                if (_ApprovedBy1At == value) return;
                OnPropertyChanging();
                _ApprovedBy1At = value;
                OnPropertyChanged();
            }
        }
        private DateTime? _ApprovedBy2At { get; set; }
        public virtual DateTime? ApprovedBy2At
        {
            get => _ApprovedBy2At;
            set
            {
                if (_ApprovedBy2At == value) return;
                OnPropertyChanging();
                _ApprovedBy2At = value;
                OnPropertyChanged();
            }
        }
        private DateTime? _ApprovedBy3At { get; set; }
        public virtual DateTime? ApprovedBy3At
        {
            get => _ApprovedBy3At;
            set
            {
                if (_ApprovedBy3At == value) return;
                OnPropertyChanging();
                _ApprovedBy3At = value;
                OnPropertyChanged();
            }
        }
        private DateTime? _ApprovedBy4At { get; set; }
        public virtual DateTime? ApprovedBy4At
        {
            get => _ApprovedBy4At;
            set
            {
                if (_ApprovedBy4At == value) return;
                OnPropertyChanging();
                _ApprovedBy4At = value;
                OnPropertyChanged();
            }
        }
        private DateTime? _ApprovedBy5At { get; set; }
        public virtual DateTime? ApprovedBy5At
        {
            get => _ApprovedBy5At;
            set
            {
                if (_ApprovedBy5At == value) return;
                OnPropertyChanging();
                _ApprovedBy5At = value;
                OnPropertyChanged();
            }
        }
        private DateTime? _ApprovedBy6At { get; set; }
        public virtual DateTime? ApprovedBy6At
        {
            get => _ApprovedBy6At;
            set
            {
                if (_ApprovedBy6At == value) return;
                OnPropertyChanging();
                _ApprovedBy6At = value;
                OnPropertyChanged();
            }
        }
        private DateTime? _ApprovedBy7At { get; set; }
        public virtual DateTime? ApprovedBy7At
        {
            get => _ApprovedBy7At;
            set
            {
                if (_ApprovedBy7At == value) return;
                OnPropertyChanging();
                _ApprovedBy7At = value;
                OnPropertyChanged();
            }
        }
        private string? _Department { get; set; }
        public virtual string? Department {
            get => _Department;
            set
            {

                if (_Department == value) return;
                OnPropertyChanging();
                _Department = value;
                OnPropertyChanged();
            }
        }
        private bool? _ApprovedFromApp { get; set; }
        public virtual bool? ApprovedFromApp
        {
            get => _ApprovedFromApp;
            set
            {

                if (_ApprovedFromApp == value) return;
                OnPropertyChanging();
                _ApprovedFromApp = value;
                OnPropertyChanged();
            }
        }
        public virtual bool? RequestUnderReview
        {
            get => _requestUnderReview;
            set
            {

                if (_requestUnderReview == value) return;
                OnPropertyChanging();
                _requestUnderReview = value;
                OnPropertyChanged();
            }
        }
        public virtual DateTime? RequestUnderReviewTime
        {
            get => _requestUnderReviewTime;
            set
            {

                if (_requestUnderReviewTime == value) return;
                OnPropertyChanging();
                _requestUnderReviewTime = value;
                OnPropertyChanged();
            }
        }
        public virtual DateTime? ReceivedTime
        {
            get => _receivedTime;
            set
            {

                if (_receivedTime == value) return;
                OnPropertyChanging();
                _receivedTime = value;
                OnPropertyChanged();
            }
        }

        private DateTime? _RejectedTime { get; set; }
        public virtual DateTime? RejectedTime
        {
            get => _RejectedTime;
            set
            {

                if (_RejectedTime == value) return;
                OnPropertyChanging();
                _RejectedTime = value;
                OnPropertyChanged();
            }
        }
        private string? _RejectedBy { get; set; }
        public virtual string? RejectedBy
        {
            get => _RejectedBy;
            set
            {

                if (_RejectedBy == value) return;
                OnPropertyChanging();
                _RejectedBy = value;
                OnPropertyChanged();
            }
        }
        private DateTime? _RejectedByPurchasingTime { get; set; }
        public virtual DateTime? RejectedByPurchasingTime
        {
            get => _RejectedByPurchasingTime;
            set
            {

                if (_RejectedByPurchasingTime == value) return;
                OnPropertyChanging();
                _RejectedByPurchasingTime = value;
                OnPropertyChanged();
            }
        }
        private string? _RejectedByPurchasing { get; set; }
        public virtual string? RejectedByPurchasing
        {
            get => _RejectedByPurchasing;
            set
            {

                if (_RejectedByPurchasing == value) return;
                OnPropertyChanging();
                _RejectedByPurchasing = value;
                OnPropertyChanged();
            }
        }
        private string? _Remarks { get; set; }
        public virtual string? Remarks
        {
            get => _Remarks;
            set
            {

                if (_Remarks == value) return;
                OnPropertyChanging();
                _Remarks = value;
                OnPropertyChanged();
            }
        }
        public virtual string? SubDepartment
        {
            get => _subDepartment;
            set
            {

                if (_subDepartment == value) return;
                OnPropertyChanging();
                _subDepartment = value;
                OnPropertyChanged();
            }
        }
        private string? _PurposeSummary { get; set; }
        public virtual string? PurposeSummary
        {
            get => _PurposeSummary;
            set
            {
                if (_PurposeOfRequest == value) return;
                OnPropertyChanging();
                _PurposeSummary = value;
                OnPropertyChanged();
            }
        }
        private string? _PurposeOfRequest { get; set; }
        public virtual string? PurposeOfRequest
        {
            get => _PurposeOfRequest;
            set
            {
                if (_PurposeOfRequest == value) return;
                OnPropertyChanging();
                _PurposeOfRequest = value;
                OnPropertyChanged();
            }
        }

        private string? _RepairKitType { get; set; }
        public virtual string? RepairKitType
        {
            get => _RepairKitType;
            set
            {
                if (_RepairKitType == value) return;
                OnPropertyChanging();
                _RepairKitType = value;
                OnPropertyChanged();
            }
        }
        private int? _SPWarehouseID { get; set; }
        public virtual int? SPWarehouseID
        {
            get => _SPWarehouseID;
            set
            {
                if (_SPWarehouseID == value) return;
                OnPropertyChanging();
                _SPWarehouseID = value;
                OnPropertyChanged();
            }
        }
        private int? _ROB { get; set; }
        public virtual int? ROB
        {
            get => _ROB;
            set
            {
                if (_ROB == value) return;
                OnPropertyChanging();
                _ROB = value;
                OnPropertyChanged();
            }
        }
        private string? _LastRevertedBy { get; set; }
        public virtual string? LastRevertedBy
        {
            get => _LastRevertedBy;
            set
            {
                if (_LastRevertedBy == value) return;
                OnPropertyChanging();
                _LastRevertedBy = value;
                OnPropertyChanged();
            }
        }
        private bool? _Revised { get; set; }
        public virtual bool? Revised
        {
            get => _Revised;
            set
            {
                if (_Revised == value) return;
                OnPropertyChanging();
                _Revised = value;
                OnPropertyChanged();
            }
        }
        private string? _RevisedBy { get; set; }
        public virtual string? RevisedBy
        {
            get => _RevisedBy;
            set
            {
                if (_RevisedBy == value) return;
                OnPropertyChanging();
                _RevisedBy = value;
                OnPropertyChanged();
            }
        }

        [NotMapped]
        public virtual ObservableCollection<LogActivity> Logs { get; set; } = new ObservableCollection<LogActivity>();
        public virtual ObservableCollection<RequisitionEngineNumber> EngineNumbers { get; set; }
    }
}

