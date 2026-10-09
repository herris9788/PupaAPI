using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pupa.BusinessObjects.Beesuite
{
    /// <summary>Pasangan Item Code (master) dan Old Item Code milik satu LSAItem. Tabel lokal Postgres (public."LSAItemCode"), lihat Migrations/migration_LSAItem_v7.sql.</summary>
    [Table("LSAItemCode", Schema = "public")]
    public class LSAItemCode : BaseEntity
    {
        private int _ID;
        private int? _LSAItemID;
        private string? _ItemCode;
        private string? _OldItemCode;

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

        [Column("OldItemCode")]
        public virtual string? OldItemCode
        {
            get { return _OldItemCode; }
            set { OnPropertyChanging(); _OldItemCode = value; OnPropertyChanged(); }
        }
    }
}
