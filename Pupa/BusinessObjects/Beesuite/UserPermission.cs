using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pupa.BusinessObjects.Beesuite
{
    [Table("UserPermission")]
    public class UserPermission : BaseEntity
    {
        private int _id;
        private int _userID;      // ← ganti dari string Username
        private int _menuID;
        private bool? _canView = false;
        private bool? _canRead = false;
        private bool? _canEdit = false;
        private bool? _canDelete = false;
        private bool? _isActive = true;
        private DateTime? _createdAt;
        private DateTime? _updatedAt;

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("ID")]
        public virtual int ID
        {
            get => _id;
            set { OnPropertyChanging(); _id = value; OnPropertyChanged(); }
        }

        /// <summary>FK ke UserV3.ID</summary>
        [Column("UserID")]
        [Required]
        public virtual int UserID
        {
            get => _userID;
            set { OnPropertyChanging(); _userID = value; OnPropertyChanged(); }
        }

        /// <summary>FK ke Menu.ID</summary>
        [Column("MenuID")]
        [Required]
        public virtual int MenuID
        {
            get => _menuID;
            set { OnPropertyChanging(); _menuID = value; OnPropertyChanged(); }
        }

        [Column("CanView")]
        public virtual bool? CanView
        {
            get => _canView;
            set { OnPropertyChanging(); _canView = value; OnPropertyChanged(); }
        }

        [Column("CanRead")]
        public virtual bool? CanRead
        {
            get => _canRead;
            set { OnPropertyChanging(); _canRead = value; OnPropertyChanged(); }
        }

        [Column("CanEdit")]
        public virtual bool? CanEdit
        {
            get => _canEdit;
            set { OnPropertyChanging(); _canEdit = value; OnPropertyChanged(); }
        }

        [Column("CanDelete")]
        public virtual bool? CanDelete
        {
            get => _canDelete;
            set { OnPropertyChanging(); _canDelete = value; OnPropertyChanged(); }
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
        private string? _featureCode;

        [Column("FeatureCode")]
        [MaxLength(80)]
        public virtual string? FeatureCode
        {
            get => _featureCode;
            set { OnPropertyChanging(); _featureCode = value; OnPropertyChanged(); }
        }

        // ── Navigation Properties ────────────────────────────────────────

        [ForeignKey("UserID")]
        public virtual UserV3? User { get; set; }

        [ForeignKey("MenuID")]
        public virtual Menu? Menu { get; set; }
    }
}