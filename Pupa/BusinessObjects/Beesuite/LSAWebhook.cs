using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pupa.BusinessObjects.Beesuite
{
    /// <summary>Webhook masuk untuk LSA (diisi n8n). Tabel Postgres N8N."LSAWebhook"; lampirannya di LSAWebhookAttachment (WebhookID).</summary>
    [Table("LSAWebhook", Schema = "N8N")]
    public class LSAWebhook : BaseEntity
    {
        private long _ID;
        private string? _SupplierCode;
        private string? _SupplierName;
        private string? _Database;
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

        [Column("SupplierName")]
        public virtual string? SupplierName
        {
            get { return _SupplierName; }
            set { OnPropertyChanging(); _SupplierName = value; OnPropertyChanged(); }
        }

        [Column("Database")]
        public virtual string? Database
        {
            get { return _Database; }
            set { OnPropertyChanging(); _Database = value; OnPropertyChanged(); }
        }

        [Column("CreatedAt")]
        public virtual DateTime CreatedAt
        {
            get { return _CreatedAt; }
            set { OnPropertyChanging(); _CreatedAt = value; OnPropertyChanged(); }
        }
    }
}
