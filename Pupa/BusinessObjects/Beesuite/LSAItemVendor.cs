using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pupa.BusinessObjects.Beesuite
{
    /// <summary>Master Item Vendor (teks bebas, unik). Tabel lokal Postgres (public."LSAItemVendor"), lihat Migrations/migration_LSAItem_v8.sql.</summary>
    [Table("LSAItemVendor", Schema = "public")]
    public class LSAItemVendor : BaseEntity
    {
        private int _ID;
        private string? _ItemVendor;

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("ID")]
        public virtual int ID
        {
            get { return _ID; }
            set { OnPropertyChanging(); _ID = value; OnPropertyChanged(); }
        }

        [Column("ItemVendor")]
        public virtual string? ItemVendor
        {
            get { return _ItemVendor; }
            set { OnPropertyChanging(); _ItemVendor = value; OnPropertyChanged(); }
        }
    }
}
