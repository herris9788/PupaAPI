using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pupa.BusinessObjects.Beesuite
{
    [Table("RequisitionNotaVerification")]
    public class RequisitionNotaVerification : BaseEntity
    {
        [Key]
        public virtual int ID { get; set; }

        public virtual int RequisitionID { get; set; }

        public virtual int? RequisitionAttachmentRelID { get; set; }

        public virtual int? AttachmentID { get; set; }

        [StringLength(255)]
        public virtual string? FileName { get; set; }

        [StringLength(30)]
        public virtual string ScanStatus { get; set; } = "PENDING";

        [StringLength(30)]
        public virtual string? VerificationStatus { get; set; }

        [Column(TypeName = "date")]
        public virtual DateTime? InvoiceDate { get; set; }

        [StringLength(255)]
        public virtual string? VendorName { get; set; }

        public virtual int? AgeInDays { get; set; }

        public virtual int? OverdueDays { get; set; }

        [Column(TypeName = "jsonb")]
        public virtual string? RawResponse { get; set; }

        public virtual string? ErrorMessage { get; set; }

        public virtual int RetryCount { get; set; }

        public virtual DateTime? LastAttemptAt { get; set; }

        public virtual DateTime? NextRetryAt { get; set; }

        public virtual DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public virtual DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("RequisitionID")]
        public virtual Requisition? Requisition { get; set; }

        [ForeignKey("RequisitionAttachmentRelID")]
        public virtual RequisitionAttachmentRel? RequisitionAttachmentRel { get; set; }

        [ForeignKey("AttachmentID")]
        public virtual Attachment? Attachment { get; set; }
    }
}
