using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pupa.BusinessObjects.Beesuite
{
    [Table("ItemEquivalent")]
    public class ItemEquivalent : BaseEntity
    {
        private int _id;
        [Key]
        public virtual int ID
        {
            get => _id;
            set { OnPropertyChanging(); _id = value; OnPropertyChanged(); }
        }

        // Items sharing the same GroupID are equivalents of one another
        // (e.g. the same lubricant sold under different brands).
        private int _groupID;
        public virtual int GroupID
        {
            get => _groupID;
            set { OnPropertyChanging(); _groupID = value; OnPropertyChanged(); }
        }

        private string _category = string.Empty;
        public virtual string Category
        {
            get => _category;
            set { OnPropertyChanging(); _category = value; OnPropertyChanged(); }
        }

        private string _groupName = string.Empty;
        public virtual string GroupName
        {
            get => _groupName;
            set { OnPropertyChanging(); _groupName = value; OnPropertyChanged(); }
        }

        private string _itemCode = string.Empty;
        public virtual string ItemCode
        {
            get => _itemCode;
            set { OnPropertyChanging(); _itemCode = value; OnPropertyChanged(); }
        }

        private string _brand = string.Empty;
        public virtual string Brand
        {
            get => _brand;
            set { OnPropertyChanging(); _brand = value; OnPropertyChanged(); }
        }

        private string? _description;
        public virtual string? Description
        {
            get => _description;
            set { OnPropertyChanging(); _description = value; OnPropertyChanged(); }
        }

        private DateTime? _createdAt;
        public virtual DateTime? CreatedAt
        {
            get => _createdAt;
            set { OnPropertyChanging(); _createdAt = value; OnPropertyChanged(); }
        }
    }
}
