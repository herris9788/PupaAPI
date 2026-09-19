using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pupa.BusinessObjects.Beesuite
{
    // One customer company of the NetSuite Customer app. See
    // migration_CustomerLicense.sql for what each column is for.
    [Table("CustomerCompany", Schema = "License")]
    public class CustomerCompany : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public virtual int ID { get; set; }

        [Column("Code")]
        public virtual string Code { get; set; } = "";

        [Column("Name")]
        public virtual string Name { get; set; } = "";

        [Column("LogoUrl")]
        public virtual string? LogoUrl { get; set; }

        [Column("MenuPrefix")]
        public virtual string MenuPrefix { get; set; } = "";

        [Column("ApiBaseUrl")]
        public virtual string ApiBaseUrl { get; set; } = "";

        [Column("ApiDb")]
        public virtual string? ApiDb { get; set; }

        [Column("AuthBaseUrl")]
        public virtual string? AuthBaseUrl { get; set; }

        // JSON dashboard layout for the web /purchasing page's widgets (see
        // migration_CustomerLicense.sql); null = default. Unrelated to
        // HomeConfig below — different consumer, different page.
        [Column("DashboardConfig")]
        public virtual string? DashboardConfig { get; set; }

        // JSON content for the mobile app's own native Home screen (progress
        // cards + recent activity); null = the app's built-in placeholder.
        [Column("HomeConfig")]
        public virtual string? HomeConfig { get; set; }

        [Column("IsActive")]
        public virtual bool IsActive { get; set; } = true;

        [Column("CreatedAt")]
        public virtual DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("UpdatedAt")]
        public virtual DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
