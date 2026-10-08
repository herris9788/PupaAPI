using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pupa.BusinessObjects.Beesuite
{
    /// <summary>Item + supplier mapping untuk LSA. Tabel lokal Postgres (public."LSAItem"), lihat
    /// Migrations/migration_LSAItem.sql. Tabel tidak punya primary key, jadi key gabungan
    /// (ItemID, SupplierID) hanya didefinisikan di level model EF (BeesuiteDbContext).</summary>
    [Table("LSAItem", Schema = "public")]
    public class LSAItem : BaseEntity
    {
        private int _ItemID;
        private string? _ItemCode;
        private string? _Type;
        private int _SupplierID;
        private string? _SupplierName;

        [Column("ItemID")]
        public virtual int ItemID
        {
            get { return _ItemID; }
            set { OnPropertyChanging(); _ItemID = value; OnPropertyChanged(); }
        }

        [Column("ItemCode")]
        public virtual string? ItemCode
        {
            get { return _ItemCode; }
            set { OnPropertyChanging(); _ItemCode = value; OnPropertyChanged(); }
        }

        [Column("Type")]
        public virtual string? Type
        {
            get { return _Type; }
            set { OnPropertyChanging(); _Type = value; OnPropertyChanged(); }
        }

        [Column("SupplierID")]
        public virtual int SupplierID
        {
            get { return _SupplierID; }
            set { OnPropertyChanging(); _SupplierID = value; OnPropertyChanged(); }
        }

        [Column("SupplierName")]
        public virtual string? SupplierName
        {
            get { return _SupplierName; }
            set { OnPropertyChanging(); _SupplierName = value; OnPropertyChanged(); }
        }
    }
}
