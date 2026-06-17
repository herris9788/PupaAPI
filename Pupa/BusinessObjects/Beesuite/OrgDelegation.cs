using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pupa.BusinessObjects.Beesuite
{
    [Table("OrgDelegation")]
    public class OrgDelegation : BaseEntity
    {
        private int _id;
        private string _type = "DELEGATE";
        private int? _fromDepartmentID;
        private int? _fromPositionID;
        private int? _toDepartmentID;
        private int? _toPositionID;
        private string? _note;
        private bool? _isActive = true;
        private DateTime? _createdAt;

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("ID")]
        public virtual int ID
        {
            get => _id;
            set { OnPropertyChanging(); _id = value; OnPropertyChanged(); }
        }

        /// <summary>DELEGATE = dialihkan | SHARE = dikerjakan bersama</summary>
        [Column("Type")]
        [MaxLength(20)]
        [Required]
        public virtual string Type
        {
            get => _type;
            set { OnPropertyChanging(); _type = value; OnPropertyChanged(); }
        }

        [Column("FromDepartmentID")]
        public virtual int? FromDepartmentID
        {
            get => _fromDepartmentID;
            set { OnPropertyChanging(); _fromDepartmentID = value; OnPropertyChanged(); }
        }

        [Column("FromPositionID")]
        public virtual int? FromPositionID
        {
            get => _fromPositionID;
            set { OnPropertyChanging(); _fromPositionID = value; OnPropertyChanged(); }
        }

        [Column("ToDepartmentID")]
        public virtual int? ToDepartmentID
        {
            get => _toDepartmentID;
            set { OnPropertyChanging(); _toDepartmentID = value; OnPropertyChanged(); }
        }

        [Column("ToPositionID")]
        public virtual int? ToPositionID
        {
            get => _toPositionID;
            set { OnPropertyChanging(); _toPositionID = value; OnPropertyChanged(); }
        }

        [Column("Note")]
        [MaxLength(255)]
        public virtual string? Note
        {
            get => _note;
            set { OnPropertyChanging(); _note = value; OnPropertyChanged(); }
        }

        [Column("IsActive")]
        public virtual bool? IsActive
        {
            get => _isActive;
            set { OnPropertyChanging(); _isActive = value; OnPropertyChanged(); }
        }

        [Column("CreatedAt")]
        public virtual DateTime? CreatedAt
        {
            get => _createdAt;
            set { OnPropertyChanging(); _createdAt = value; OnPropertyChanged(); }
        }

        // ── Navigation ───────────────────────────────────────────────────────
        [ForeignKey("FromDepartmentID")]
        public virtual OrgDepartment? FromDepartment { get; set; }

        [ForeignKey("FromPositionID")]
        public virtual OrgPosition? FromPosition { get; set; }

        [ForeignKey("ToDepartmentID")]
        public virtual OrgDepartment? ToDepartment { get; set; }

        [ForeignKey("ToPositionID")]
        public virtual OrgPosition? ToPosition { get; set; }
    }
}
