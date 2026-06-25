using System;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pupa.BusinessObjects.Beesuite
{
    /// <summary>
    /// Item usage document (header). One usage records the consumption of one
    /// or more items on board and can go through an approval flow.
    ///
    /// This is a standalone log: it does NOT change the ROB (remaining on board)
    /// stock. The line items live in <see cref="UsageDetail"/>.
    ///
    /// The approval columns mirror <see cref="Requisition"/> so the existing
    /// approval engine can be reused: ApprovedByN keeps the original approver,
    /// ApprovedByNActualBy records a delegate who acted on their behalf.
    /// </summary>
    [Table("Usage")]
    public class Usage : BaseEntity
    {
        private int _ID;
        private string? _DocumentNumber;
        private string? _DB;
        private int? _InventoryUserID;
        private string? _InventoryUserCode;
        private string? _VesselName;
        private DateTime? _UsageDate = DateTime.Now;
        private string? _UsedBy;
        private string? _Purpose;
        private string? _Remarks;
        private string? _Status = "Pending Approval";
        private bool? _Approved = false;
        private int? _ApprovalMaxLevel;
        private string? _RejectedBy;
        private DateTime? _RejectedTime;
        private string? _CreatedBy;
        private DateTime? _CreatedAt = DateTime.Now;
        private DateTime? _UpdatedAt = DateTime.Now;

        private string? _ApprovedBy1;
        private string? _ApprovedBy2;
        private string? _ApprovedBy3;
        private string? _ApprovedBy4;
        private string? _ApprovedBy5;
        private string? _ApprovedBy6;
        private string? _ApprovedBy7;

        private DateTime? _ApprovedBy1At;
        private DateTime? _ApprovedBy2At;
        private DateTime? _ApprovedBy3At;
        private DateTime? _ApprovedBy4At;
        private DateTime? _ApprovedBy5At;
        private DateTime? _ApprovedBy6At;
        private DateTime? _ApprovedBy7At;

        private string? _ApprovedBy1ActualBy;
        private string? _ApprovedBy2ActualBy;
        private string? _ApprovedBy3ActualBy;
        private string? _ApprovedBy4ActualBy;
        private string? _ApprovedBy5ActualBy;
        private string? _ApprovedBy6ActualBy;
        private string? _ApprovedBy7ActualBy;

        public Usage()
        {
            UsageDetails = new ObservableCollection<UsageDetail>();
        }

        [Key]
        [Column("ID")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public virtual int ID { get { return _ID; } set { OnPropertyChanging(); _ID = value; OnPropertyChanged(); } }

        [Column("DocumentNumber")]
        [StringLength(100)]
        public virtual string? DocumentNumber { get { return _DocumentNumber; } set { OnPropertyChanging(); _DocumentNumber = value; OnPropertyChanged(); } }

        [Column("DB")]
        public virtual string? DB { get { return _DB; } set { OnPropertyChanging(); _DB = value; OnPropertyChanged(); } }

        [Column("InventoryUserID")]
        public virtual int? InventoryUserID { get { return _InventoryUserID; } set { OnPropertyChanging(); _InventoryUserID = value; OnPropertyChanged(); } }

        [Column("InventoryUserCode")]
        public virtual string? InventoryUserCode { get { return _InventoryUserCode; } set { OnPropertyChanging(); _InventoryUserCode = value; OnPropertyChanged(); } }

        [Column("VesselName")]
        public virtual string? VesselName { get { return _VesselName; } set { OnPropertyChanging(); _VesselName = value; OnPropertyChanged(); } }

        /// <summary>When the items were used.</summary>
        [Column("UsageDate")]
        public virtual DateTime? UsageDate { get { return _UsageDate; } set { OnPropertyChanging(); _UsageDate = value; OnPropertyChanged(); } }

        /// <summary>Person / crew who used the items.</summary>
        [Column("UsedBy")]
        public virtual string? UsedBy { get { return _UsedBy; } set { OnPropertyChanging(); _UsedBy = value; OnPropertyChanged(); } }

        /// <summary>Why the items were used (job, maintenance, etc.).</summary>
        [Column("Purpose")]
        public virtual string? Purpose { get { return _Purpose; } set { OnPropertyChanging(); _Purpose = value; OnPropertyChanged(); } }

        [Column("Remarks")]
        public virtual string? Remarks { get { return _Remarks; } set { OnPropertyChanging(); _Remarks = value; OnPropertyChanged(); } }

        [Column("Status")]
        [StringLength(100)]
        public virtual string? Status { get { return _Status; } set { OnPropertyChanging(); _Status = value; OnPropertyChanged(); } }

        [Column("Approved")]
        public virtual bool? Approved { get { return _Approved; } set { OnPropertyChanging(); _Approved = value; OnPropertyChanged(); } }

        [Column("ApprovalMaxLevel")]
        public virtual int? ApprovalMaxLevel { get { return _ApprovalMaxLevel; } set { OnPropertyChanging(); _ApprovalMaxLevel = value; OnPropertyChanged(); } }

        [Column("RejectedBy")]
        public virtual string? RejectedBy { get { return _RejectedBy; } set { OnPropertyChanging(); _RejectedBy = value; OnPropertyChanged(); } }

        [Column("RejectedTime")]
        public virtual DateTime? RejectedTime { get { return _RejectedTime; } set { OnPropertyChanging(); _RejectedTime = value; OnPropertyChanged(); } }

        [Column("CreatedBy")]
        public virtual string? CreatedBy { get { return _CreatedBy; } set { OnPropertyChanging(); _CreatedBy = value; OnPropertyChanged(); } }

        [Column("CreatedAt")]
        public virtual DateTime? CreatedAt { get { return _CreatedAt; } set { OnPropertyChanging(); _CreatedAt = value; OnPropertyChanged(); } }

        [Column("UpdatedAt")]
        public virtual DateTime? UpdatedAt { get { return _UpdatedAt; } set { OnPropertyChanging(); _UpdatedAt = value; OnPropertyChanged(); } }

        // ── Approval chain (mirrors Requisition) ───────────────────────────────
        [Column("ApprovedBy1")] [StringLength(500)] public virtual string? ApprovedBy1 { get { return _ApprovedBy1; } set { OnPropertyChanging(); _ApprovedBy1 = value; OnPropertyChanged(); } }
        [Column("ApprovedBy2")] [StringLength(500)] public virtual string? ApprovedBy2 { get { return _ApprovedBy2; } set { OnPropertyChanging(); _ApprovedBy2 = value; OnPropertyChanged(); } }
        [Column("ApprovedBy3")] [StringLength(500)] public virtual string? ApprovedBy3 { get { return _ApprovedBy3; } set { OnPropertyChanging(); _ApprovedBy3 = value; OnPropertyChanged(); } }
        [Column("ApprovedBy4")] [StringLength(500)] public virtual string? ApprovedBy4 { get { return _ApprovedBy4; } set { OnPropertyChanging(); _ApprovedBy4 = value; OnPropertyChanged(); } }
        [Column("ApprovedBy5")] [StringLength(500)] public virtual string? ApprovedBy5 { get { return _ApprovedBy5; } set { OnPropertyChanging(); _ApprovedBy5 = value; OnPropertyChanged(); } }
        [Column("ApprovedBy6")] [StringLength(500)] public virtual string? ApprovedBy6 { get { return _ApprovedBy6; } set { OnPropertyChanging(); _ApprovedBy6 = value; OnPropertyChanged(); } }
        [Column("ApprovedBy7")] [StringLength(500)] public virtual string? ApprovedBy7 { get { return _ApprovedBy7; } set { OnPropertyChanging(); _ApprovedBy7 = value; OnPropertyChanged(); } }

        [Column("ApprovedBy1At")] public virtual DateTime? ApprovedBy1At { get { return _ApprovedBy1At; } set { OnPropertyChanging(); _ApprovedBy1At = value; OnPropertyChanged(); } }
        [Column("ApprovedBy2At")] public virtual DateTime? ApprovedBy2At { get { return _ApprovedBy2At; } set { OnPropertyChanging(); _ApprovedBy2At = value; OnPropertyChanged(); } }
        [Column("ApprovedBy3At")] public virtual DateTime? ApprovedBy3At { get { return _ApprovedBy3At; } set { OnPropertyChanging(); _ApprovedBy3At = value; OnPropertyChanged(); } }
        [Column("ApprovedBy4At")] public virtual DateTime? ApprovedBy4At { get { return _ApprovedBy4At; } set { OnPropertyChanging(); _ApprovedBy4At = value; OnPropertyChanged(); } }
        [Column("ApprovedBy5At")] public virtual DateTime? ApprovedBy5At { get { return _ApprovedBy5At; } set { OnPropertyChanging(); _ApprovedBy5At = value; OnPropertyChanged(); } }
        [Column("ApprovedBy6At")] public virtual DateTime? ApprovedBy6At { get { return _ApprovedBy6At; } set { OnPropertyChanging(); _ApprovedBy6At = value; OnPropertyChanged(); } }
        [Column("ApprovedBy7At")] public virtual DateTime? ApprovedBy7At { get { return _ApprovedBy7At; } set { OnPropertyChanging(); _ApprovedBy7At = value; OnPropertyChanged(); } }

        // Delegation audit: ApprovedByN keeps the ORIGINAL approver, while
        // ApprovedByNActualBy records the delegate who actually performed it.
        [Column("ApprovedBy1ActualBy")] [StringLength(500)] public virtual string? ApprovedBy1ActualBy { get { return _ApprovedBy1ActualBy; } set { OnPropertyChanging(); _ApprovedBy1ActualBy = value; OnPropertyChanged(); } }
        [Column("ApprovedBy2ActualBy")] [StringLength(500)] public virtual string? ApprovedBy2ActualBy { get { return _ApprovedBy2ActualBy; } set { OnPropertyChanging(); _ApprovedBy2ActualBy = value; OnPropertyChanged(); } }
        [Column("ApprovedBy3ActualBy")] [StringLength(500)] public virtual string? ApprovedBy3ActualBy { get { return _ApprovedBy3ActualBy; } set { OnPropertyChanging(); _ApprovedBy3ActualBy = value; OnPropertyChanged(); } }
        [Column("ApprovedBy4ActualBy")] [StringLength(500)] public virtual string? ApprovedBy4ActualBy { get { return _ApprovedBy4ActualBy; } set { OnPropertyChanging(); _ApprovedBy4ActualBy = value; OnPropertyChanged(); } }
        [Column("ApprovedBy5ActualBy")] [StringLength(500)] public virtual string? ApprovedBy5ActualBy { get { return _ApprovedBy5ActualBy; } set { OnPropertyChanging(); _ApprovedBy5ActualBy = value; OnPropertyChanged(); } }
        [Column("ApprovedBy6ActualBy")] [StringLength(500)] public virtual string? ApprovedBy6ActualBy { get { return _ApprovedBy6ActualBy; } set { OnPropertyChanging(); _ApprovedBy6ActualBy = value; OnPropertyChanged(); } }
        [Column("ApprovedBy7ActualBy")] [StringLength(500)] public virtual string? ApprovedBy7ActualBy { get { return _ApprovedBy7ActualBy; } set { OnPropertyChanging(); _ApprovedBy7ActualBy = value; OnPropertyChanged(); } }

        // ── Navigation ─────────────────────────────────────────────────────────
        public virtual ObservableCollection<UsageDetail> UsageDetails { get; set; }

        [ForeignKey("InventoryUserID")]
        public virtual InventoryUser? InventoryUser { get; set; }
    }
}
