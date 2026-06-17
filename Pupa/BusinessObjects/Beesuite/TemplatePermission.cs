using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pupa.BusinessObjects.Beesuite
{
    [Table("TemplatePermission")]
    public class TemplatePermission : BaseEntity
    {
        private int _id;
        private string _templateName = string.Empty;
        private int _menuID;
        private bool? _canView = false;
        private bool? _canRead = false;
        private bool? _canEdit = false;
        private bool? _canDelete = false;
        private bool? _isActive = true;

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("ID")]
        public virtual int ID
        {
            get => _id;
            set { OnPropertyChanging(); _id = value; OnPropertyChanged(); }
        }

        /// <summary>Nama template sidebar (misal: "Template Admin", "Template Crew")</summary>
        [Column("TemplateName")]
        [MaxLength(150)]
        [Required]
        public virtual string TemplateName
        {
            get => _templateName;
            set { OnPropertyChanging(); _templateName = value; OnPropertyChanged(); }
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
        private string? _featureCode;
        [Column("FeatureCode")]
        [MaxLength(80)]
        public virtual string? FeatureCode
        {
            get => _featureCode;
            set { OnPropertyChanging(); _featureCode = value; OnPropertyChanged(); }
        }
        // ── Navigation Properties ────────────────────────────────────────────

        /// <summary>Navigation ke Menu</summary>
        [ForeignKey("MenuID")]
        public virtual Menu? Menu { get; set; }
    }
}