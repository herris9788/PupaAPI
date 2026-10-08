using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pupa.BusinessObjects.Beesuite
{
    /// <summary>Item + supplier mapping untuk LSA. Tabel lokal Postgres (public."LSAItem"), lihat
    /// Migrations/migration_LSAItem_v2.sql. Primary key: ID (identity). Item hanya disimpan lewat ItemCode,
    /// supplier lewat SupplierCode + SupplierName.</summary>
    [Table("LSAItem", Schema = "public")]
    public class LSAItem : BaseEntity
    {
        private int _ID;
        private string? _ItemCode;
        private string? _Type;
        private string? _SupplierCode;
        private string? _SupplierName;

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("ID")]
        public virtual int ID
        {
            get { return _ID; }
            set { OnPropertyChanging(); _ID = value; OnPropertyChanged(); }
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

        [Column("SupplierCode")]
        public virtual string? SupplierCode
        {
            get { return _SupplierCode; }
            set { OnPropertyChanging(); _SupplierCode = value; OnPropertyChanged(); }
        }

        [Column("SupplierName")]
        public virtual string? SupplierName
        {
            get { return _SupplierName; }
            set { OnPropertyChanging(); _SupplierName = value; OnPropertyChanged(); }
        }
    }
}
