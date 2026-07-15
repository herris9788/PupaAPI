using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pupa.BusinessObjects.Beesuite
{
    /// <summary>
    /// A photo / document attached to a <see cref="CartItem"/>. Mirrors
    /// RequisitionDetailAttachmentRel so that processing a cart item can map its
    /// attachments 1:1 onto the resulting requisition detail. Store either the
    /// inline Base64 blob OR a StoragePath/URL.
    /// </summary>
    [Table("CartItemAttachment")]
    public class CartItemAttachment : BaseEntity
    {
        private int _id;
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("ID")]
        public virtual int ID
        {
            get => _id;
            set { OnPropertyChanging(); _id = value; OnPropertyChanged(); }
        }

        private int _cartItemID;
        [Column("CartItemID")]
        [Required]
        public virtual int CartItemID
        {
            get => _cartItemID;
            set { OnPropertyChanging(); _cartItemID = value; OnPropertyChanged(); }
        }

        /// <summary>e.g. DamageReport, ServiceReport, ValvePhoto.</summary>
        private string? _type;
        [Column("Type")]
        [MaxLength(100)]
        public virtual string? Type
        {
            get => _type;
            set { OnPropertyChanging(); _type = value; OnPropertyChanged(); }
        }

        private string? _fileName;
        [Column("FileName")]
        [MaxLength(255)]
        public virtual string? FileName
        {
            get => _fileName;
            set { OnPropertyChanging(); _fileName = value; OnPropertyChanged(); }
        }

        private string? _fileExtension;
        [Column("FileExtension")]
        [MaxLength(20)]
        public virtual string? FileExtension
        {
            get => _fileExtension;
            set { OnPropertyChanging(); _fileExtension = value; OnPropertyChanged(); }
        }

        private string? _mimeType;
        [Column("MimeType")]
        [MaxLength(100)]
        public virtual string? MimeType
        {
            get => _mimeType;
            set { OnPropertyChanging(); _mimeType = value; OnPropertyChanged(); }
        }

        /// <summary>Inline blob (optional — use this OR StoragePath).</summary>
        private string? _base64;
        [Column("Base64")]
        public virtual string? Base64
        {
            get => _base64;
            set { OnPropertyChanging(); _base64 = value; OnPropertyChanged(); }
        }

        /// <summary>Path / URL to the stored file (optional — use this OR Base64).</summary>
        private string? _storagePath;
        [Column("StoragePath")]
        [MaxLength(500)]
        public virtual string? StoragePath
        {
            get => _storagePath;
            set { OnPropertyChanging(); _storagePath = value; OnPropertyChanged(); }
        }

        private DateTime? _createdAt = DateTime.UtcNow;
        [Column("CreatedAt")]
        public virtual DateTime? CreatedAt
        {
            get => _createdAt;
            set { OnPropertyChanging(); _createdAt = value; OnPropertyChanged(); }
        }

        private string? _createdBy;
        [Column("CreatedBy")]
        [MaxLength(100)]
        public virtual string? CreatedBy
        {
            get => _createdBy;
            set { OnPropertyChanging(); _createdBy = value; OnPropertyChanged(); }
        }

        // ── Navigation ────────────────────────────────────────────────────────
        [ForeignKey("CartItemID")]
        public virtual CartItem? CartItem { get; set; }
    }
}
