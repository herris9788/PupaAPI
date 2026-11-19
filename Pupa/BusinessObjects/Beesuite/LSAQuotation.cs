using System;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pupa.BusinessObjects.Beesuite
{
    /// <summary>Quotation supplier yang masuk dari n8n. Tabel Postgres N8N."LSAQuotation"; rinciannya di LSAQuotationDetail (QuotationID).</summary>
    [Table("LSAQuotation", Schema = "N8N")]
    public class LSAQuotation : BaseEntity
    {
        private long _ID;
        private string? _SupplierCode;
        private string? _InquiryNumber;
        private string? _Database;
        private string? _Remarks;
        private string? _Attachment;
        private bool _IsFinal;
        private bool _Proceed;
        private bool _NoOffer;
        private int _DeliveryDays;
        private bool _TaxInclusive;
        private DateTime _CreatedAt = DateTime.Now;

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("ID")]
        public virtual long ID
        {
            get { return _ID; }
            set { OnPropertyChanging(); _ID = value; OnPropertyChanged(); }
        }

        [Column("SupplierCode")]
        public virtual string? SupplierCode
        {
            get { return _SupplierCode; }
            set { OnPropertyChanging(); _SupplierCode = value; OnPropertyChanged(); }
        }

        [Column("InquiryNumber")]
        public virtual string? InquiryNumber
        {
            get { return _InquiryNumber; }
            set { OnPropertyChanging(); _InquiryNumber = value; OnPropertyChanged(); }
        }

        [Column("Database")]
        public virtual string? Database
        {
            get { return _Database; }
            set { OnPropertyChanging(); _Database = value; OnPropertyChanged(); }
        }

        [Column("Remarks")]
        public virtual string? Remarks
        {
            get { return _Remarks; }
            set { OnPropertyChanging(); _Remarks = value; OnPropertyChanged(); }
        }

        [Column("Attachment")]
        public virtual string? Attachment
        {
            get { return _Attachment; }
            set { OnPropertyChanging(); _Attachment = value; OnPropertyChanged(); }
        }

        /// <summary>true = baris Final Quotation (duplikat submitan terbaru, file final di Attachment).</summary>
        [Column("IsFinal")]
        public virtual bool IsFinal
        {
            get { return _IsFinal; }
            set { OnPropertyChanging(); _IsFinal = value; OnPropertyChanged(); }
        }

        /// <summary>true = baris ini sudah diproses oleh N8N (default false).</summary>
        [Column("Proceed")]
        public virtual bool Proceed
        {
            get { return _Proceed; }
            set { OnPropertyChanging(); _Proceed = value; OnPropertyChanged(); }
        }

        /// <summary>Supplier menjawab No Offer di Vendor Portal (tanpa file, tanpa harga).</summary>
        [Column("NoOffer")]
        public virtual bool NoOffer
        {
            get { return _NoOffer; }
            set { OnPropertyChanging(); _NoOffer = value; OnPropertyChanged(); }
        }

        /// <summary>Lead time seluruh quotation, hari setelah SPK (Vendor Portal).</summary>
        [Column("DeliveryDays")]
        public virtual int DeliveryDays
        {
            get { return _DeliveryDays; }
            set { OnPropertyChanging(); _DeliveryDays = value; OnPropertyChanged(); }
        }

        /// <summary>Harga sudah termasuk pajak.</summary>
        [Column("TaxInclusive")]
        public virtual bool TaxInclusive
        {
            get { return _TaxInclusive; }
            set { OnPropertyChanging(); _TaxInclusive = value; OnPropertyChanged(); }
        }

        [Column("CreatedAt")]
        public virtual DateTime CreatedAt
        {
            get { return _CreatedAt; }
            set { OnPropertyChanging(); _CreatedAt = value; OnPropertyChanged(); }
        }

        /// <summary>Rincian item milik quotation ini (LSAQuotationDetail.QuotationID). Ambil lewat $expand=Details.</summary>
        public virtual ObservableCollection<LSAQuotationDetail>? Details { get; set; }
    }
}
