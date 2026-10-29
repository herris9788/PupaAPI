using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Pupa;

namespace Pupa.BusinessObjects.Vendor
{
    [Table("PaymentDocument", Schema = "Vendor")]
    public class PaymentDocument : BaseEntity
    {
        [Key]
        [Column("DocumentID")]
        public virtual long DocumentID { get; set; }

        [Column("DBName"), StringLength(50)]
        public virtual string? DBName { get; set; }

        [Column("OwnerType"), StringLength(50)]
        public virtual string? OwnerType { get; set; }

        [Column("OwnerID"), StringLength(50)]
        public virtual string? OwnerID { get; set; }

        [Column("POID")]
        public virtual long? POID { get; set; }

        [Column("PONumber"), StringLength(100)]
        public virtual string? PONumber { get; set; }

        [Column("PurchaseID")]
        public virtual long? PurchaseID { get; set; }

        [Column("PurchaseNumber"), StringLength(100)]
        public virtual string? PurchaseNumber { get; set; }

        [Column("DocumentName"), StringLength(255)]
        public virtual string? DocumentName { get; set; }

        [Column("Extension"), StringLength(20)]
        public virtual string? Extension { get; set; }

        [Column("Bytes")]
        public virtual string? Bytes { get; set; }

        [Column("Description"), StringLength(100)]
        public virtual string? Description { get; set; }

        [Column("Status"), StringLength(30)]
        public virtual string? Status { get; set; }

        [Column("Exported")]
        public virtual bool? Exported { get; set; }

        [Column("CreatedBy"), StringLength(100)]
        public virtual string? CreatedBy { get; set; }

        [Column("CreatedDate")]
        public virtual DateTimeOffset? CreatedDate { get; set; }

        [Column("LastModBy"), StringLength(100)]
        public virtual string? LastModBy { get; set; }

        [Column("LastMod")]
        public virtual DateTimeOffset? LastMod { get; set; }

        [Column("NetTotal", TypeName = "numeric(18,2)")]
        public virtual decimal? NetTotal { get; set; }

        [Column("CurrencyCode"), StringLength(10)]
        public virtual string? CurrencyCode { get; set; }

        [Column("ReferenceNo"), StringLength(100)]
        public virtual string? ReferenceNo { get; set; }
    }
}
