using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pupa.BusinessObjects.Beesuite
{
    [Table("ItemGroupMapping")]
    public class ItemGroupMapping : BaseEntity
    {
        private int _id;
        [Key]
        public virtual int ID { get => _id; set { OnPropertyChanging(); _id = value; OnPropertyChanged(); } }

        private string _coaCode = string.Empty;
        public virtual string COACode { get => _coaCode; set { OnPropertyChanging(); _coaCode = value; OnPropertyChanged(); } }

        private string _groupName = string.Empty;
        public virtual string GroupName { get => _groupName; set { OnPropertyChanging(); _groupName = value; OnPropertyChanged(); } }

        private int _stockCategoryID;
        public virtual int StockCategoryID { get => _stockCategoryID; set { OnPropertyChanging(); _stockCategoryID = value; OnPropertyChanged(); } }

        private string _categoryName = string.Empty;
        public virtual string CategoryName { get => _categoryName; set { OnPropertyChanging(); _categoryName = value; OnPropertyChanged(); } }

        private int _familyID;
        public virtual int FamilyID { get => _familyID; set { OnPropertyChanging(); _familyID = value; OnPropertyChanged(); } }

        private string _familyCode = string.Empty;
        public virtual string FamilyCode { get => _familyCode; set { OnPropertyChanging(); _familyCode = value; OnPropertyChanged(); } }

        private string _familyName = string.Empty;
        public virtual string FamilyName { get => _familyName; set { OnPropertyChanging(); _familyName = value; OnPropertyChanged(); } }

        // "Approval Matrix" (confirmed) or "Proposed" (guessed, needs review) --
        // mirrors the Status column used in the ApprovalV2.xlsx working file.
        private string _source = "Approval Matrix";
        public virtual string Source { get => _source; set { OnPropertyChanging(); _source = value; OnPropertyChanged(); } }

        private DateTime? _createdAt;
        public virtual DateTime? CreatedAt { get => _createdAt; set { OnPropertyChanging(); _createdAt = value; OnPropertyChanged(); } }
    }
}
