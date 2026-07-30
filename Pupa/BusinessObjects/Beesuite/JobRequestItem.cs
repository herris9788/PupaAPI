using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pupa.BusinessObjects.Beesuite
{
    /// <summary>
    /// Lookup table mapping a Job Request configuration (Group / Category /
    /// Position / Brand / Type / PK / Service Type) to the inventory item that
    /// should be used for it (ItemCode / ItemName / ItemID).
    /// </summary>
    [Table("JobRequestItem")]
    public class JobRequestItem : BaseEntity
    {
        private int _id;
        private string? _group;
        private string? _category;
        private string? _position;
        private string? _brand;
        private string? _type;
        private string? _pk;
        private string? _serviceType;
        private string? _itemCode;
        private string? _itemName;
        private int? _itemId;
        private DateTime _createdAt = DateTime.UtcNow;
        private DateTime _updatedAt = DateTime.UtcNow;

        [Key]
        [Column("ID")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public virtual int ID
        {
            get => _id;
            set { if (_id == value) return; OnPropertyChanging(); _id = value; OnPropertyChanged(); }
        }

        [StringLength(100)]
        [Column("Group")]
        public virtual string? Group
        {
            get => _group;
            set { if (_group == value) return; OnPropertyChanging(); _group = value; OnPropertyChanged(); }
        }

        [StringLength(150)]
        [Column("Category")]
        public virtual string? Category
        {
            get => _category;
            set { if (_category == value) return; OnPropertyChanging(); _category = value; OnPropertyChanged(); }
        }

        [StringLength(150)]
        [Column("Position")]
        public virtual string? Position
        {
            get => _position;
            set { if (_position == value) return; OnPropertyChanging(); _position = value; OnPropertyChanged(); }
        }

        [StringLength(100)]
        [Column("Brand")]
        public virtual string? Brand
        {
            get => _brand;
            set { if (_brand == value) return; OnPropertyChanging(); _brand = value; OnPropertyChanged(); }
        }

        [StringLength(150)]
        [Column("Type")]
        public virtual string? Type
        {
            get => _type;
            set { if (_type == value) return; OnPropertyChanging(); _type = value; OnPropertyChanged(); }
        }

        [StringLength(50)]
        [Column("PK")]
        public virtual string? PK
        {
            get => _pk;
            set { if (_pk == value) return; OnPropertyChanging(); _pk = value; OnPropertyChanged(); }
        }

        [StringLength(255)]
        [Column("ServiceType")]
        public virtual string? ServiceType
        {
            get => _serviceType;
            set { if (_serviceType == value) return; OnPropertyChanging(); _serviceType = value; OnPropertyChanged(); }
        }

        [StringLength(100)]
        [Column("ItemCode")]
        public virtual string? ItemCode
        {
            get => _itemCode;
            set { if (_itemCode == value) return; OnPropertyChanging(); _itemCode = value; OnPropertyChanged(); }
        }

        [StringLength(255)]
        [Column("ItemName")]
        public virtual string? ItemName
        {
            get => _itemName;
            set { if (_itemName == value) return; OnPropertyChanging(); _itemName = value; OnPropertyChanged(); }
        }

        [Column("ItemID")]
        public virtual int? ItemID
        {
            get => _itemId;
            set { if (_itemId == value) return; OnPropertyChanging(); _itemId = value; OnPropertyChanged(); }
        }

        [Column("CreatedAt")]
        public virtual DateTime CreatedAt
        {
            get => _createdAt;
            set { if (_createdAt == value) return; OnPropertyChanging(); _createdAt = value; OnPropertyChanged(); }
        }

        [Column("UpdatedAt")]
        public virtual DateTime UpdatedAt
        {
            get => _updatedAt;
            set { if (_updatedAt == value) return; OnPropertyChanging(); _updatedAt = value; OnPropertyChanged(); }
        }
    }
}
