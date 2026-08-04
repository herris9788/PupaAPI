using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pupa.BusinessObjects.Beesuite
{
    /// <summary>
    /// Whitelist of items offered in Quick Order, one row per item. Replaces the
    /// list that used to be hard-coded in the Flutter page
    /// (QuickOrderPageWeb._categoryItemCodes) so it can be maintained in the DB.
    ///
    /// "Category"/"Family" are display snapshots; "CategoryID"/"FamilyID" are the
    /// ids the requisition groups by. "GroupName" is the item's provision
    /// sub-group (e.g. "VEGETABLES", "DRY STORES"). "MaxQty" caps the requestable
    /// quantity (null = unlimited).
    /// </summary>
    [Table("QuickOrderItem")]
    public class QuickOrderItem : BaseEntity
    {
        private int _id;
        private string? _itemCode;
        private string? _itemName;
        private string? _groupName;
        private string? _category;
        private string? _family;
        private int? _categoryId;
        private int? _familyId;
        private decimal? _maxQty;
        private string? _note;

        [Key]
        [Column("ID")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public virtual int ID
        {
            get => _id;
            set { if (_id == value) return; OnPropertyChanging(); _id = value; OnPropertyChanged(); }
        }

        [StringLength(50)]
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

        /// <summary>Provision sub-group, e.g. "VEGETABLES", "FRUITS", "DRY STORES".</summary>
        [Column("GroupName")]
        public virtual string? GroupName
        {
            get => _groupName;
            set { if (_groupName == value) return; OnPropertyChanging(); _groupName = value; OnPropertyChanged(); }
        }

        [StringLength(255)]
        [Column("Category")]
        public virtual string? Category
        {
            get => _category;
            set { if (_category == value) return; OnPropertyChanging(); _category = value; OnPropertyChanged(); }
        }

        [StringLength(255)]
        [Column("Family")]
        public virtual string? Family
        {
            get => _family;
            set { if (_family == value) return; OnPropertyChanging(); _family = value; OnPropertyChanged(); }
        }

        [Column("CategoryID")]
        public virtual int? CategoryID
        {
            get => _categoryId;
            set { if (_categoryId == value) return; OnPropertyChanging(); _categoryId = value; OnPropertyChanged(); }
        }

        [Column("FamilyID")]
        public virtual int? FamilyID
        {
            get => _familyId;
            set { if (_familyId == value) return; OnPropertyChanging(); _familyId = value; OnPropertyChanged(); }
        }

        /// <summary>Max requestable quantity; null = unlimited.</summary>
        [Column("MaxQty", TypeName = "numeric(18,2)")]
        public virtual decimal? MaxQty
        {
            get => _maxQty;
            set { if (_maxQty == value) return; OnPropertyChanging(); _maxQty = value; OnPropertyChanged(); }
        }

        [StringLength(1000)]
        [Column("Note")]
        public virtual string? Note
        {
            get => _note;
            set { if (_note == value) return; OnPropertyChanging(); _note = value; OnPropertyChanged(); }
        }
    }
}
