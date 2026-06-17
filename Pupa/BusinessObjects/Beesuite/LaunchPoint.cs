using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pupa.BusinessObjects.Beesuite
{
    [Table("LaunchPoint")]
    public class LaunchPoint : BaseEntity
    {
        private int _id;
        private string? _userName;
        private string? _menuCode;
        private string? _menuName;
        private string? _icon;
        private string? _route;
        private int? _parentID;
        private int _sortOrder;
        private bool _isActive = true;
        private string? _category;
        private string? _badge;
        private int? _menuID;
        private string? _name;
        private string? _templateName;
        private int? _templateItemID;

        public LaunchPoint()
        {
            Children = new ObservableCollection<LaunchPoint>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("ID")]
        public virtual int ID
        {
            get => _id;
            set { OnPropertyChanging(); _id = value; OnPropertyChanged(); }
        }

        [Column("UserName")]
        [MaxLength(100)]
        public virtual string? UserName
        {
            get => _userName;
            set { OnPropertyChanging(); _userName = value; OnPropertyChanged(); }
        }

        [Column("MenuCode")]
        [MaxLength(100)]
        public virtual string? MenuCode
        {
            get => _menuCode;
            set { OnPropertyChanging(); _menuCode = value; OnPropertyChanged(); }
        }

        [Column("MenuName")]
        [MaxLength(255)]
        public virtual string? MenuName
        {
            get => _menuName;
            set { OnPropertyChanging(); _menuName = value; OnPropertyChanged(); }
        }

        [Column("Icon")]
        [MaxLength(255)]
        public virtual string? Icon
        {
            get => _icon;
            set { OnPropertyChanging(); _icon = value; OnPropertyChanged(); }
        }

        [Column("Route")]
        [MaxLength(500)]
        public virtual string? Route
        {
            get => _route;
            set { OnPropertyChanging(); _route = value; OnPropertyChanged(); }
        }

        /// <summary>null = root node, isi = child dari LaunchPoint lain</summary>
        [Column("ParentID")]
        public virtual int? ParentID
        {
            get => _parentID;
            set { OnPropertyChanging(); _parentID = value; OnPropertyChanged(); }
        }

        [Column("SortOrder")]
        public virtual int SortOrder
        {
            get => _sortOrder;
            set { OnPropertyChanging(); _sortOrder = value; OnPropertyChanged(); }
        }

        [Column("IsActive")]
        public virtual bool IsActive
        {
            get => _isActive;
            set { OnPropertyChanging(); _isActive = value; OnPropertyChanged(); }
        }

        [Column("Category")]
        [MaxLength(100)]
        public virtual string? Category
        {
            get => _category;
            set { OnPropertyChanging(); _category = value; OnPropertyChanged(); }
        }

        [Column("Badge")]
        [MaxLength(50)]
        public virtual string? Badge
        {
            get => _badge;
            set { OnPropertyChanging(); _badge = value; OnPropertyChanged(); }
        }

        /// <summary>FK ke Menu.ID — NULL berarti ini folder/grup, bukan menu item</summary>
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

        [Column("TemplateName")]
        [MaxLength(150)]
        public virtual string? TemplateName
        {
            get => _templateName;
            set { OnPropertyChanging(); _templateName = value; OnPropertyChanged(); }
        }

        [Column("TemplateItemID")]
        public virtual int? TemplateItemID
        {
            get => _templateItemID;
            set { OnPropertyChanging(); _templateItemID = value; OnPropertyChanged(); }
        }

        // ── Navigation Properties ────────────────────────────────────────

        [ForeignKey("ParentID")]
        public virtual LaunchPoint? Parent { get; set; }
        public virtual ObservableCollection<LaunchPoint> Children { get; set; }

        [ForeignKey("MenuID")]
        public virtual Menu? Menu { get; set; }
    }
}