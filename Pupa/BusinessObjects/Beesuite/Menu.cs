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
        private bool _IsLogistic { get; set; } = false;
        [Column("IsLogistic")]
        public virtual bool IsLogistic
        {
            get => _IsLogistic;
            set { OnPropertyChanging(); _IsLogistic = value; OnPropertyChanged(); }
        }

        // BeeSuite-only: default "Coming Soon" state for this menu (only
        // meaningful for MenuCode rows prefixed "BS_" — BeeSuite's own menu
        // catalog, added alongside this table's existing unrelated rows).
        private bool _IsComingSoon { get; set; } = false;
        [Column("IsComingSoon")]
        public virtual bool IsComingSoon
        {
            get => _IsComingSoon;
            set { OnPropertyChanging(); _IsComingSoon = value; OnPropertyChanged(); }
        }

        // BeeSuite-only: which platform(s) this menu item is allowed to
        // appear on — drives both the web sidebar and the mobile Quick
        // Access grid, which otherwise both render off the same
        // Menu/LaunchPoint grant.
        private bool _AllowWeb { get; set; } = true;
        [Column("AllowWeb")]
        public virtual bool AllowWeb
        {
            get => _AllowWeb;
            set { OnPropertyChanging(); _AllowWeb = value; OnPropertyChanged(); }
        }

        private bool _AllowMobile { get; set; } = false;
        [Column("AllowMobile")]
        public virtual bool AllowMobile
        {
            get => _AllowMobile;
            set { OnPropertyChanging(); _AllowMobile = value; OnPropertyChanged(); }
        }

        // BeeSuite-only: when true, the mobile WebPage embedding this menu's
        // Route renders without its own AppBar (the embedded web page is
        // expected to provide its own header/back control instead).
        private bool _HideAppBar { get; set; } = false;
        [Column("HideAppBar")]
        public virtual bool HideAppBar
        {
            get => _HideAppBar;
            set { OnPropertyChanging(); _HideAppBar = value; OnPropertyChanged(); }
        }

        // BeeSuite-only: when true (and HideAppBar is false), the WebPage's
        // AppBar shows the BeeSuite logo instead of the menu's title text.
        private bool _AppBarShowLogo { get; set; } = false;
        [Column("AppBarShowLogo")]
        public virtual bool AppBarShowLogo
        {
            get => _AppBarShowLogo;
            set { OnPropertyChanging(); _AppBarShowLogo = value; OnPropertyChanged(); }
        }

        // BeeSuite-only: when true, the WebPage embedding this menu's Route
        // also renders the app's own floating bottom nav bar on top, so the
        // user can jump straight to Home/Activity/Scan/Me without leaving
        // via a back button first.
        private bool _ShowBottomNav { get; set; } = false;
        [Column("ShowBottomNav")]
        public virtual bool ShowBottomNav
        {
            get => _ShowBottomNav;
            set { OnPropertyChanging(); _ShowBottomNav = value; OnPropertyChanged(); }
        }
    }
}
