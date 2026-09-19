using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pupa.BusinessObjects.Beesuite
{
    // A license key for one CustomerCompany. ExpiresAt == null means "no expiry".
    // All timestamps are UTC (timestamptz).
    [Table("CustomerLicense", Schema = "License")]
    public class CustomerLicense : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public virtual int ID { get; set; }

        [Column("CompanyID")]
        public virtual int CompanyID { get; set; }

        [Column("LicenseKey")]
        public virtual string LicenseKey { get; set; } = "";

        [Column("IssuedAt")]
        public virtual DateTime IssuedAt { get; set; } = DateTime.UtcNow;

        [Column("ExpiresAt")]
        public virtual DateTime? ExpiresAt { get; set; }

        [Column("IsRevoked")]
        public virtual bool IsRevoked { get; set; }

        [Column("Note")]
        public virtual string? Note { get; set; }

        [Column("LastActivatedAt")]
        public virtual DateTime? LastActivatedAt { get; set; }

        [Column("LastDeviceID")]
        public virtual string? LastDeviceID { get; set; }

        [Column("ActivationCount")]
        public virtual int ActivationCount { get; set; }
    }
}
