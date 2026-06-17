using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pupa.BusinessObjects.Beesuite
{
    [Table("Menu")]
    public class Menu : BaseEntity
    {
        private int _menuID;
        private string? _menuCode;
        private string? _menuName;
        private string? _icon;
        private string? _route;
        private string? _category;
        private string? _description;
        private int _sortOrder;
        private bool _isActive = true;

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public virtual int ID
        {
            get => _menuID;
            set { OnPropertyChanging(); _menuID = value; OnPropertyChanged(); }
        }

        [Column("MenuCode")]
        public virtual string? MenuCode
        {
            get => _menuCode;
            set { OnPropertyChanging(); _menuCode = value; OnPropertyChanged(); }
        }

        [Column("MenuName")]
        public virtual string? MenuName
        {
            get => _menuName;
            set { OnPropertyChanging(); _menuName = value; OnPropertyChanged(); }
        }

        [Column("Icon")]
        public virtual string? Icon
        {
            get => _icon;
            set { OnPropertyChanging(); _icon = value; OnPropertyChanged(); }
        }

        [Column("Route")]
        public virtual string? Route
        {
            get => _route;
            set { OnPropertyChanging(); _route = value; OnPropertyChanged(); }
        }

        [Column("Category")]
        public virtual string? Category
        {
            get => _category;
            set { OnPropertyChanging(); _category = value; OnPropertyChanged(); }
        }

        [Column("Description")]
        public virtual string? Description
        {
            get => _description;
            set { OnPropertyChanging(); _description = value; OnPropertyChanged(); }
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
    }
}
