using System;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pupa.BusinessObjects.Beesuite
{
    /// <summary>Rincian milik satu LSAJob (JobID -> LSAJob.ID). Tabel Postgres N8N."LSAJobDetail".</summary>
    [Table("LSAJobDetail", Schema = "N8N")]
    public class LSAJobDetail : BaseEntity
    {
        private long _ID;
        private long _JobID;
        private string? _Group;
        private string? _ItemCode;
        private string? _OldItemCode;
        private string? _ItemVendor;
        private string? _ItemVendors;
        private decimal? _Qty;
        private decimal? _Price;
        private decimal? _Amount;
        private decimal? _ProcessQty;
        private decimal? _ProcessPrice;
        private DateTime _CreatedAt = DateTime.Now;

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("ID")]
        public virtual long ID
        {
            get { return _ID; }
            set { OnPropertyChanging(); _ID = value; OnPropertyChanged(); }
        }

        [Column("JobID")]
        public virtual long JobID
        {
            get { return _JobID; }
            set { OnPropertyChanging(); _JobID = value; OnPropertyChanged(); }
        }

        /// <summary>Group item pada baris detail, mis. "Service &amp; Renew Certificate".</summary>
        [Column("Group")]
        public virtual string? Group
        {
            get { return _Group; }
            set { OnPropertyChanging(); _Group = value; OnPropertyChanged(); }
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

        [Column("ItemVendor")]
        public virtual string? ItemVendor
        {
            get { return _ItemVendor; }
            set { OnPropertyChanging(); _ItemVendor = value; OnPropertyChanged(); }
        }

        /// <summary>JSON bebas (jsonb), disimpan/dibaca sebagai string JSON.</summary>
        [Column("ItemVendors", TypeName = "jsonb")]
        public virtual string? ItemVendors
        {
            get { return _ItemVendors; }
            set { OnPropertyChanging(); _ItemVendors = value; OnPropertyChanged(); }
        }

        [Column("Qty")]
        public virtual decimal? Qty
        {
            get { return _Qty; }
            set { OnPropertyChanging(); _Qty = value; OnPropertyChanged(); }
        }

        [Column("Price")]
        public virtual decimal? Price
        {
            get { return _Price; }
            set { OnPropertyChanging(); _Price = value; OnPropertyChanged(); }
        }

        [Column("Amount")]
        public virtual decimal? Amount
        {
            get { return _Amount; }
            set { OnPropertyChanging(); _Amount = value; OnPropertyChanged(); }
        }

        [Column("ProcessQty")]
        public virtual decimal? ProcessQty
        {
            get { return _ProcessQty; }
            set { OnPropertyChanging(); _ProcessQty = value; OnPropertyChanged(); }
        }

        [Column("ProcessPrice")]
        public virtual decimal? ProcessPrice
        {
            get { return _ProcessPrice; }
            set { OnPropertyChanging(); _ProcessPrice = value; OnPropertyChanged(); }
        }

        [Column("CreatedAt")]
        public virtual DateTime CreatedAt
        {
            get { return _CreatedAt; }
            set { OnPropertyChanging(); _CreatedAt = value; OnPropertyChanged(); }
        }

        public virtual LSAJob? Job { get; set; }
    }
}
