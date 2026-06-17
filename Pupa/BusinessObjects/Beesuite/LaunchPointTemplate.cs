using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pupa.BusinessObjects.Beesuite
{
    [Table("LaunchPointTemplate")]
    public class LaunchPointTemplate : BaseEntity
    {
        private int _id;
        private string _templateName = string.Empty;
        private int? _menuID;
        private string? _name;
        private string? _icon;
        private int? _parentID;
        private int? _sortOrder = 0;
        private bool? _isActive = true;
        private DateTime? _createdAt;
        private DateTime? _updatedAt;

        public LaunchPointTemplate()
        {
            Children = new ObservableCollection<LaunchPointTemplate>();
        }

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
        [MaxLength(255)]
        [Required]
        public virtual string TemplateName
        {
            get => _templateName;
            set { OnPropertyChanging(); _templateName = value; OnPropertyChanged(); }
        }

        /// <summary>FK ke Menu.ID — NULL berarti ini adalah folder/grup, bukan menu item</summary>
        [Column("MenuID")]
        public virtual int? MenuID
        {
            get => _menuID;
            set { OnPropertyChanging(); _menuID = value; OnPropertyChanged(); }
        }

        /// <summary>Nama folder / label — diisi saat MenuID NULL</summary>
        [Column("Name")]
        [MaxLength(255)]
        public virtual string? Name
        {
            get => _name;
            set { OnPropertyChanging(); _name = value; OnPropertyChanged(); }
        }

        [Column("Icon")]
        [MaxLength(255)]
        public virtual string? Icon
        {
            get => _icon;
            set { OnPropertyChanging(); _icon = value; OnPropertyChanged(); }
        }

        /// <summary>Self-reference ke parent node — NULL atau 0 berarti root</summary>
        [Column("ParentID")]
        public virtual int? ParentID
        {
            get => _parentID;
            set { OnPropertyChanging(); _parentID = value; OnPropertyChanged(); }
        }

        [Column("SortOrder")]
        public virtual int? SortOrder
        {
            get => _sortOrder;
            set { OnPropertyChanging(); _sortOrder = value; OnPropertyChanged(); }
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

        // ── Navigation Properties ────────────────────────────────────────────

        /// <summary>Navigation ke Menu (jika MenuID tidak NULL)</summary>
        [ForeignKey("MenuID")]
        public virtual Menu? Menu { get; set; }

        /// <summary>Navigation ke parent node</summary>
        [ForeignKey("ParentID")]
        public virtual LaunchPointTemplate? Parent { get; set; }

        /// <summary>Navigation ke child nodes</summary>
        public virtual ObservableCollection<LaunchPointTemplate> Children { get; set; }
    }
}