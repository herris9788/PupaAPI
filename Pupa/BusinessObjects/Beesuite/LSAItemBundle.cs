using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pupa.BusinessObjects.Beesuite
{
    /// <summary>Item isi (komponen) dari baris LSAItem bertipe Bundle. Tabel lokal Postgres
    /// (public."LSAItemBundle"), lihat Migrations/migration_LSAItemBundle.sql. LSAItemID mengarah ke LSAItem.ID.</summary>
    [Table("LSAItemBundle", Schema = "public")]
    public class LSAItemBundle : BaseEntity
    {
        private int _ID;
        private int? _LSAItemID;
        private string? _ItemCode;
        private decimal? _Qty;

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

        [Column("ItemCode")]
        public virtual string? ItemCode
        {
            get { return _ItemCode; }
            set { OnPropertyChanging(); _ItemCode = value; OnPropertyChanged(); }
        }

        [Column("Qty")]
        public virtual decimal? Qty
        {
            get { return _Qty; }
            set { OnPropertyChanging(); _Qty = value; OnPropertyChanged(); }
        }
    }
}
