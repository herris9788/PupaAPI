using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pupa.BusinessObjects.Beesuite
{
    /// <summary>Relasi LSAItem (header) ke LSAItemVendor (master): LSAItemID -> LSAItem.ID, VendorID -> LSAItemVendor.ID.</summary>
    [Table("LSAItemVendorLink", Schema = "public")]
    public class LSAItemVendorLink : BaseEntity
    {
        private int _ID;
        private int? _LSAItemID;
        private int? _VendorID;

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("ID")]
        public virtual int ID
        {
            get { return _ID; }
            set { OnPropertyChanging(); _ID = value; OnPropertyChanged(); }
        }

        [Column("LSAItemID")]
        public virtual int? LSAItemID
        {
            get { return _LSAItemID; }
            set { OnPropertyChanging(); _LSAItemID = value; OnPropertyChanged(); }
        }

        [Column("VendorID")]
        public virtual int? VendorID
        {
            get { return _VendorID; }
            set { OnPropertyChanging(); _VendorID = value; OnPropertyChanged(); }
        }
    }
}
