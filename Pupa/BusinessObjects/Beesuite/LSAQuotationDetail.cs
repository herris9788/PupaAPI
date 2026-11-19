using System;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pupa.BusinessObjects.Beesuite
{
    /// <summary>Rincian item milik satu LSAQuotation (QuotationID -> LSAQuotation.ID). Tabel Postgres N8N."LSAQuotationDetail".</summary>
    [Table("LSAQuotationDetail", Schema = "N8N")]
    public class LSAQuotationDetail : BaseEntity
    {
        private long _ID;
        private long _QuotationID;
        private string? _ItemCode;
        private string? _ItemName;
        private decimal? _Quantity;
        private string? _UOM;
        private decimal? _Price;
        private decimal? _Amount;
        private decimal? _LastPrice;
        private string? _Note;
        private int _DeliveryDays;

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("ID")]
        public virtual long ID
        {
            get { return _ID; }
            set { OnPropertyChanging(); _ID = value; OnPropertyChanged(); }
        }

        [Column("QuotationID")]
        public virtual long QuotationID
        {
            get { return _QuotationID; }
            set { OnPropertyChanging(); _QuotationID = value; OnPropertyChanged(); }
        }

        [Column("ItemCode")]
        public virtual string? ItemCode
        {
            get { return _ItemCode; }
            set { OnPropertyChanging(); _ItemCode = value; OnPropertyChanged(); }
        }

        [Column("ItemName")]
        public virtual string? ItemName
        {
            get { return _ItemName; }
            set { OnPropertyChanging(); _ItemName = value; OnPropertyChanged(); }
        }

        [Column("Quantity")]
        public virtual decimal? Quantity
        {
            get { return _Quantity; }
            set { OnPropertyChanging(); _Quantity = value; OnPropertyChanged(); }
        }

        [Column("UOM")]
        public virtual string? UOM
        {
            get { return _UOM; }
            set { OnPropertyChanging(); _UOM = value; OnPropertyChanged(); }
        }

        /// <summary>Harga satuan (dari LSA job).</summary>
        [Column("Price")]
        public virtual decimal? Price
        {
            get { return _Price; }
            set { OnPropertyChanging(); _Price = value; OnPropertyChanged(); }
        }

        /// <summary>Quantity x Price.</summary>
        [Column("Amount")]
        public virtual decimal? Amount
        {
            get { return _Amount; }
            set { OnPropertyChanging(); _Amount = value; OnPropertyChanged(); }
        }

        /// <summary>Harga beli terakhir.</summary>
        [Column("LastPrice")]
        public virtual decimal? LastPrice
        {
            get { return _LastPrice; }
            set { OnPropertyChanging(); _LastPrice = value; OnPropertyChanged(); }
        }

        /// <summary>Catatan baris.</summary>
        [Column("Note")]
        public virtual string? Note
        {
            get { return _Note; }
            set { OnPropertyChanging(); _Note = value; OnPropertyChanged(); }
        }

        /// <summary>Lead time baris ini, hari setelah SPK.</summary>
        [Column("DeliveryDays")]
        public virtual int DeliveryDays
        {
            get { return _DeliveryDays; }
            set { OnPropertyChanging(); _DeliveryDays = value; OnPropertyChanged(); }
        }

        public virtual LSAQuotation? Quotation { get; set; }
    }
}
