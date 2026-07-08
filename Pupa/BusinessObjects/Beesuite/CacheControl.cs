using Pupa;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BeeSuite.WebApi.BusinessObjects.Beesuite
{
    /// <summary>
    /// Server-driven "force clear cache" directive for BeeSuite Web. Clients poll
    /// the active row; when <see cref="CacheVersion"/> differs from the value a
    /// client last cleared to, that client is forced to clear its cache (reload)
    /// before continuing. Bump <see cref="CacheVersion"/> to force all open
    /// clients; set <see cref="IsActive"/> = false to disable.
    /// </summary>
    [Table("CacheControl", Schema = "Ascend")]
    public class CacheControl : BaseEntity
    {
        public CacheControl()
        {
        }

        #region Backing Fields
        private string? _CacheVersion = string.Empty;
        private bool? _IsActive = true;
        private string? _Platform;
        private string? _Title = "Update Required";
        private string? _Message =
            "A new version of BeeSuite is available. Please clear your cache to continue.";
        #endregion

        #region Public Properties
        [Key]
        [Column("ID")]
        public virtual int ID
        {
            get;
            set;
        }

        /// <summary>Bump to any new value to force clients to clear + reload.</summary>
        [Column("CacheVersion")]
        [MaxLength(50)]
        public virtual string? CacheVersion
        {
            get => _CacheVersion;
            set { OnPropertyChanging(); _CacheVersion = value; OnPropertyChanged(); }
        }

        /// <summary>Master switch; when false the directive is ignored (no pop-up).</summary>
        [Column("IsActive")]
        public virtual bool? IsActive
        {
            get => _IsActive;
            set { OnPropertyChanging(); _IsActive = value; OnPropertyChanged(); }
        }

        /// <summary>Optional platform scope: "web", "all", etc. Null = all.</summary>
        [Column("Platform")]
        [MaxLength(20)]
        public virtual string? Platform
        {
            get => _Platform;
            set { OnPropertyChanging(); _Platform = value; OnPropertyChanged(); }
        }

        [Column("Title")]
        [MaxLength(120)]
        public virtual string? Title
        {
            get => _Title;
            set { OnPropertyChanging(); _Title = value; OnPropertyChanged(); }
        }

        [Column("Message")]
        public virtual string? Message
        {
            get => _Message;
            set { OnPropertyChanging(); _Message = value; OnPropertyChanged(); }
        }

        // DatabaseGeneratedOption.None forces EF Core to send the C# value on
        // INSERT (the Postgres column has no DEFAULT wired for EF). Defaults to
        // local "now" — same convention as UserApprovalScope.
        [Column("CreatedAt")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public virtual DateTime? CreatedAt { get; set; } = DateTime.Now;

        [Column("UpdatedAt")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public virtual DateTime? UpdatedAt { get; set; } = DateTime.Now;
        #endregion
    }
}
