using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pupa.BusinessObjects.Beesuite
{
    [Table("MenuFeature")]
    public class MenuFeature : BaseEntity
    {
        private int _id;
        private int _menuID;
        private string _code = string.Empty;
        private string _label = string.Empty;
        private int? _sortOrder = 0;
        private bool? _isActive = true;

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("ID")]
        public virtual int ID
        {
            get => _id;
            set { OnPropertyChanging(); _id = value; OnPropertyChanged(); }
        }

        /// <summary>FK ke Menu.ID</summary>
        [Column("MenuID")]
        [Required]
        public virtual int MenuID
        {
            get => _menuID;
            set { OnPropertyChanging(); _menuID = value; OnPropertyChanged(); }
        }

        /// <summary>Kode fitur, mis. 'price', 'supplier'</summary>
        [Column("Code")]
        [MaxLength(80)]
        [Required]
        public virtual string Code
        {
            get => _code;
            set { OnPropertyChanging(); _code = value; OnPropertyChanged(); }
        }

        /// <summary>Label tampilan, mis. 'Harga Barang'</summary>
        [Column("Label")]
        [MaxLength(150)]
        [Required]
        public virtual string Label
        {
            get => _label;
            set { OnPropertyChanging(); _label = value; OnPropertyChanged(); }
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

        // ── Navigation Properties ────────────────────────────────────────────

        [ForeignKey("MenuID")]
        public virtual Menu? Menu { get; set; }
    }
}