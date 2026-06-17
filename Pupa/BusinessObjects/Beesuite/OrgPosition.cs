using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pupa.BusinessObjects.Beesuite
{
    [Table("OrgPosition")]
    public class OrgPosition : BaseEntity
    {
        private int _id;
        private int _levelID;
        private int? _userID;
        private int? _parentPositionID;
        private string? _title;
        private bool? _isActive = true;
        private DateTime? _createdAt;
        private DateTime? _updatedAt;

        public OrgPosition()
        {
            Children = new ObservableCollection<OrgPosition>();
            DelegationsFrom = new ObservableCollection<OrgDelegation>();
            DelegationsTo = new ObservableCollection<OrgDelegation>();
            PositionDepartments = new ObservableCollection<OrgPositionDepartment>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("ID")]
        public virtual int ID
        {
            get => _id;
            set { OnPropertyChanging(); _id = value; OnPropertyChanged(); }
        }

        [Column("LevelID")]
        [Required]
        public virtual int LevelID
        {
            get => _levelID;
            set { OnPropertyChanging(); _levelID = value; OnPropertyChanged(); }
        }

        /// <summary>Boleh NULL — posisi belum terisi user</summary>
        [Column("UserID")]
        public virtual int? UserID
        {
            get => _userID;
            set { OnPropertyChanging(); _userID = value; OnPropertyChanged(); }
        }

        /// <summary>Atasan langsung — boleh lompat level jika level di atasnya kosong</summary>
        [Column("ParentPositionID")]
        public virtual int? ParentPositionID
        {
            get => _parentPositionID;
            set { OnPropertyChanging(); _parentPositionID = value; OnPropertyChanged(); }
        }

        /// <summary>Override label, mis. "Assistant Manager Purchasing"</summary>
        [Column("Title")]
        [MaxLength(200)]
        public virtual string? Title
        {
            get => _title;
            set { OnPropertyChanging(); _title = value; OnPropertyChanged(); }
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

        [Column("UpdatedAt")]
        public virtual DateTime? UpdatedAt
        {
            get => _updatedAt;
            set { OnPropertyChanging(); _updatedAt = value; OnPropertyChanged(); }
        }

        // ── Navigation ───────────────────────────────────────────────────────
        [ForeignKey("LevelID")]
        public virtual OrgLevel? Level { get; set; }

        [ForeignKey("UserID")]
        public virtual UserV3? User { get; set; }

        [ForeignKey("ParentPositionID")]
        public virtual OrgPosition? Parent { get; set; }

        public virtual ObservableCollection<OrgPosition> Children { get; set; }
        public virtual ObservableCollection<OrgDelegation> DelegationsFrom { get; set; }
        public virtual ObservableCollection<OrgDelegation> DelegationsTo { get; set; }
        public virtual ObservableCollection<OrgPositionDepartment> PositionDepartments { get; set; }
    }
}
