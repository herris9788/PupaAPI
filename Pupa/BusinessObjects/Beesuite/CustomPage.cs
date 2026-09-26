using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pupa.BusinessObjects.Beesuite
{
    /// <summary>
    /// A page uploaded from the web app's Settings &gt; Custom Pages: the source of a single Vue SFC
    /// (.vue) that the frontend compiles and renders at /p/{Slug} at runtime, so a new page needs no
    /// build/deploy. "MenuID" is the Menu row created for it (Route = /p/{Slug}); access is then
    /// controlled by the normal Menu/UserPermission system. Only SuperUser can write these from the UI.
    /// </summary>
    [Table("CustomPage")]
    public class CustomPage : BaseEntity
    {
        private int _id;
        private string? _slug;
        private string? _title;
        private string? _source;
        private bool _isPublished = true;
        private int? _menuId;
        private string? _createdBy;
        private DateTime? _createdAt;
        private string? _updatedBy;
        private DateTime? _updatedAt;

        [Key]
        [Column("ID")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public virtual int ID
        {
            get => _id;
            set { if (_id == value) return; OnPropertyChanging(); _id = value; OnPropertyChanged(); }
        }

        /// <summary>URL segment, lowercase letters/digits/dashes; unique.</summary>
        [StringLength(100)]
        [Column("Slug")]
        public virtual string? Slug
        {
            get => _slug;
            set { if (_slug == value) return; OnPropertyChanging(); _slug = value; OnPropertyChanged(); }
        }

        [StringLength(255)]
        [Column("Title")]
        public virtual string? Title
        {
            get => _title;
            set { if (_title == value) return; OnPropertyChanging(); _title = value; OnPropertyChanged(); }
        }

        /// <summary>Full text of the uploaded .vue file.</summary>
        [Column("Source")]
        public virtual string? Source
        {
            get => _source;
            set { if (_source == value) return; OnPropertyChanging(); _source = value; OnPropertyChanged(); }
        }

        [Column("IsPublished")]
        public virtual bool IsPublished
        {
            get => _isPublished;
            set { if (_isPublished == value) return; OnPropertyChanging(); _isPublished = value; OnPropertyChanged(); }
        }

        /// <summary>Menu.ID of the menu entry that exposes this page (null when none was created).</summary>
        [Column("MenuID")]
        public virtual int? MenuID
        {
            get => _menuId;
            set { if (_menuId == value) return; OnPropertyChanging(); _menuId = value; OnPropertyChanged(); }
        }

        [StringLength(100)]
        [Column("CreatedBy")]
        public virtual string? CreatedBy
        {
            get => _createdBy;
            set { if (_createdBy == value) return; OnPropertyChanging(); _createdBy = value; OnPropertyChanged(); }
        }

        [Column("CreatedAt")]
        public virtual DateTime? CreatedAt
        {
            get => _createdAt;
            set { if (_createdAt == value) return; OnPropertyChanging(); _createdAt = value; OnPropertyChanged(); }
        }

        [StringLength(100)]
        [Column("UpdatedBy")]
        public virtual string? UpdatedBy
        {
            get => _updatedBy;
            set { if (_updatedBy == value) return; OnPropertyChanging(); _updatedBy = value; OnPropertyChanged(); }
        }

        [Column("UpdatedAt")]
        public virtual DateTime? UpdatedAt
        {
            get => _updatedAt;
            set { if (_updatedAt == value) return; OnPropertyChanging(); _updatedAt = value; OnPropertyChanged(); }
        }
    }
}
