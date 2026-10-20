using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pupa.BusinessObjects.Beesuite
{
    /// <summary>Lampiran milik satu LSAWebhook (WebhookID -> LSAWebhook.ID). Isi file disimpan sebagai Base64. Tabel Postgres N8N."LSAWebhookAttachment".</summary>
    [Table("LSAWebhookAttachment", Schema = "N8N")]
    public class LSAWebhookAttachment : BaseEntity
    {
        private long _ID;
        private long _WebhookID;
        private string? _Name;
        private string? _MimeType;
        private string? _Base64;
        private DateTime _CreatedAt = DateTime.Now;

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("ID")]
        public virtual long ID
        {
            get { return _ID; }
            set { OnPropertyChanging(); _ID = value; OnPropertyChanged(); }
        }

        [Column("WebhookID")]
        public virtual long WebhookID
        {
            get { return _WebhookID; }
            set { OnPropertyChanging(); _WebhookID = value; OnPropertyChanged(); }
        }

        [Column("Name")]
        public virtual string? Name
        {
            get { return _Name; }
            set { OnPropertyChanging(); _Name = value; OnPropertyChanged(); }
        }

        [Column("MimeType")]
        public virtual string? MimeType
        {
            get { return _MimeType; }
            set { OnPropertyChanging(); _MimeType = value; OnPropertyChanged(); }
        }

        [Column("Base64")]
        public virtual string? Base64
        {
            get { return _Base64; }
            set { OnPropertyChanging(); _Base64 = value; OnPropertyChanged(); }
        }

        [Column("CreatedAt")]
        public virtual DateTime CreatedAt
        {
            get { return _CreatedAt; }
            set { OnPropertyChanging(); _CreatedAt = value; OnPropertyChanged(); }
        }

        /// <summary>Webhook induk (WebhookID -> LSAWebhook.ID). Ambil lewat $expand=Webhook.</summary>
        public virtual LSAWebhook? Webhook { get; set; }
    }
}
