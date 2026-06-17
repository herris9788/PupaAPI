using System;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pupa.BusinessObjects.Beesuite
{
    [Table("IC_StockFamily_COA", Schema = "Ascend")]
    public class StockFamilyCOA : BaseEntity
    {
        public StockFamilyCOA()
        {
        }
        private int? _StockCategoryID;
        private int? _FamilyID;
        private string? _Code;
        private string? _Description;
        private string? _PurposeEx;
        [Key]
        public virtual int ID { get; set; }

        [Column("StockCategoryID")]
        public virtual int? StockCategoryID { get { return _StockCategoryID; } set { OnPropertyChanging(); _StockCategoryID = value; OnPropertyChanged(); } }

        [Column("FamilyID")]
        public virtual int? FamilyID { get { return _FamilyID; } set { OnPropertyChanging(); _FamilyID = value; OnPropertyChanged(); } }

        [Column("Code")]
        public virtual string? Code { get { return _Code; } set { OnPropertyChanging(); _Code = value; OnPropertyChanged(); } }
        [Column("Description")]
        public virtual string? Description { get { return _Description; } set { OnPropertyChanging(); _Description = value; OnPropertyChanged(); } }
        [Column("PurposeEx")]
        public virtual string? PurposeEx { get { return _PurposeEx; } set { OnPropertyChanging(); _PurposeEx = value; OnPropertyChanged(); } }
        [ForeignKey("FamilyID")]
        public virtual StockFamily? StockFamily { get; set; }
        [ForeignKey("StockCategoryID")]
        public virtual StockCategory? StockCategory { get; set; }

    }
}