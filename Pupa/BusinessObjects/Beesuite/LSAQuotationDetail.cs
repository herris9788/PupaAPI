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

        public virtual LSAQuotation? Quotation { get; set; }
    }
}
