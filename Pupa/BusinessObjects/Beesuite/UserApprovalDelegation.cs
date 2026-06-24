using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pupa.BusinessObjects.Beesuite
{
    /// <summary>
    /// User-to-user approval delegation. While the original approver (FromUser) is
    /// away, the delegate (ToUser) may approve in their place during the window
    /// [StartDate, EndDate]. The recorded approver on the requisition stays the
    /// original user; the delegate is only kept as an audit trail.
    /// </summary>
    [Table("UserApprovalDelegation")]
    public class UserApprovalDelegation : BaseEntity
    {
        private int _id;
        private int _fromUserID;
        private int _toUserID;
        private DateTime? _startDate;
        private DateTime? _endDate;
        private bool? _isActive = true;
        private string? _note;
        private DateTime? _createdAt = DateTime.UtcNow;
        private string? _createdBy;

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("ID")]
        public virtual int ID
        {
            get => _id;
            set { OnPropertyChanging(); _id = value; OnPropertyChanged(); }
        }

        /// <summary>The original approver who is away (delegation source).</summary>
        [Column("FromUserID")]
        [Required]
        public virtual int FromUserID
        {
            get => _fromUserID;
            set { OnPropertyChanging(); _fromUserID = value; OnPropertyChanged(); }
        }

        /// <summary>The user who may approve on the original's behalf (delegate).</summary>
        [Column("ToUserID")]
        [Required]
        public virtual int ToUserID
        {
            get => _toUserID;
            set { OnPropertyChanging(); _toUserID = value; OnPropertyChanged(); }
        }

        /// <summary>Start of the absence window (null = effective immediately).</summary>
        [Column("StartDate")]
        public virtual DateTime? StartDate
        {
            get => _startDate;
            set { OnPropertyChanging(); _startDate = value; OnPropertyChanged(); }
        }

        /// <summary>End of the absence window (null = open-ended).</summary>
        [Column("EndDate")]
        public virtual DateTime? EndDate
        {
            get => _endDate;
            set { OnPropertyChanging(); _endDate = value; OnPropertyChanged(); }
        }

        /// <summary>Manual on/off switch on top of the date window.</summary>
        [Column("IsActive")]
        public virtual bool? IsActive
        {
            get => _isActive;
            set { OnPropertyChanging(); _isActive = value; OnPropertyChanged(); }
        }

        [Column("Note")]
        [MaxLength(255)]
        public virtual string? Note
        {
            get => _note;
            set { OnPropertyChanging(); _note = value; OnPropertyChanged(); }
        }

        [Column("CreatedAt")]
        public virtual DateTime? CreatedAt
        {
            get => _createdAt;
            set { OnPropertyChanging(); _createdAt = value; OnPropertyChanged(); }
        }

        [Column("CreatedBy")]
        [MaxLength(100)]
        public virtual string? CreatedBy
        {
            get => _createdBy;
            set { OnPropertyChanging(); _createdBy = value; OnPropertyChanged(); }
        }

        // ── Navigation ───────────────────────────────────────────────────────
        [ForeignKey("FromUserID")]
        public virtual User? FromUser { get; set; }

        [ForeignKey("ToUserID")]
        public virtual User? ToUser { get; set; }
    }
}
