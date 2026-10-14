using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pupa.BusinessObjects.Beesuite
{
    /// <summary>
    /// Supplier item codes per item. Maps to the existing table "Ascend"."IC_SupplierItemCodes" (no schema change). Class is not called SupplierItemCode because a member cannot share its type's name.
    /// </summary>
    [Table("IC_SupplierItemCodes", Schema = "Ascend")]
    public class ItemSupplierCode : BaseEntity
    {
        private int _SupplierItemCodeID;
        private int _ItemID;
        private int _SupplierID;
        private string _SupplierItemCode = "0";
        private string _SupplierItemDescription = string.Empty;
        private string _CoState = "New";
        private Guid _CoUID;
        private Guid? _CoPkgID;
        private Guid? _CoSourcePkgID;

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Column("SupplierItemCodeID")]
        public virtual int SupplierItemCodeID { get { return _SupplierItemCodeID; } set { OnPropertyChanging(); _SupplierItemCodeID = value; OnPropertyChanged(); } }

        [Column("ItemID")]
        public virtual int ItemID { get { return _ItemID; } set { OnPropertyChanging(); _ItemID = value; OnPropertyChanged(); } }

        [Column("SupplierID")]
        public virtual int SupplierID { get { return _SupplierID; } set { OnPropertyChanging(); _SupplierID = value; OnPropertyChanged(); } }

        [Required]
        [StringLength(50)]
        [Column("SupplierItemCode")]
        public virtual string SupplierItemCode { get { return _SupplierItemCode; } set { OnPropertyChanging(); _SupplierItemCode = value; OnPropertyChanged(); } }

        [Required]
        [StringLength(500)]
        [Column("SupplierItemDescription")]
        public virtual string SupplierItemDescription { get { return _SupplierItemDescription; } set { OnPropertyChanging(); _SupplierItemDescription = value; OnPropertyChanged(); } }

        [Required]
        [StringLength(10)]
        [Column("CoState")]
        public virtual string CoState { get { return _CoState; } set { OnPropertyChanging(); _CoState = value; OnPropertyChanged(); } }

        [Column("CoUID")]
        public virtual Guid CoUID { get { return _CoUID; } set { OnPropertyChanging(); _CoUID = value; OnPropertyChanged(); } }

        [Column("CoPkgID")]
        public virtual Guid? CoPkgID { get { return _CoPkgID; } set { OnPropertyChanging(); _CoPkgID = value; OnPropertyChanged(); } }

        [Column("CoSourcePkgID")]
        public virtual Guid? CoSourcePkgID { get { return _CoSourcePkgID; } set { OnPropertyChanging(); _CoSourcePkgID = value; OnPropertyChanged(); } }
    }
}
