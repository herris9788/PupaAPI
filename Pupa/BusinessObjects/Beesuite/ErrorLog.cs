using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pupa.BusinessObjects.Beesuite
{
    [Table("ErrorLog")]
    public class ErrorLog : BaseEntity
    {
        private long _id;
        private string _level = "ERROR";
        private string _message = string.Empty;
        private string? _source;
        private string? _stackTrace;
        private string? _requestPath;
        private string? _requestMethod;
        private string? _requestBody;
        private string? _userName;
        private string? _ipAddress;
        private string? _userAgent;
        private int? _statusCode;
        private DateTime _createdAt = DateTime.UtcNow;

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("ID")]
        public virtual long ID
        {
            get => _id;
            set { OnPropertyChanging(); _id = value; OnPropertyChanged(); }
        }

        /// <summary>ERROR | WARN | FATAL | INFO</summary>
        [Column("Level")]
        [MaxLength(20)]
        [Required]
        public virtual string Level
        {
            get => _level;
            set { OnPropertyChanging(); _level = value; OnPropertyChanged(); }
        }

        [Column("Message")]
        [Required]
        public virtual string Message
        {
            get => _message;
            set { OnPropertyChanging(); _message = value; OnPropertyChanged(); }
        }

        /// <summary>Nama controller / service / method asal error</summary>
        [Column("Source")]
        [MaxLength(255)]
        public virtual string? Source
        {
            get => _source;
            set { OnPropertyChanging(); _source = value; OnPropertyChanged(); }
        }

        [Column("StackTrace")]
        public virtual string? StackTrace
        {
            get => _stackTrace;
            set { OnPropertyChanging(); _stackTrace = value; OnPropertyChanged(); }
        }

        [Column("RequestPath")]
        [MaxLength(500)]
        public virtual string? RequestPath
        {
            get => _requestPath;
            set { OnPropertyChanging(); _requestPath = value; OnPropertyChanged(); }
        }

        [Column("RequestMethod")]
        [MaxLength(10)]
        public virtual string? RequestMethod
        {
            get => _requestMethod;
            set { OnPropertyChanging(); _requestMethod = value; OnPropertyChanged(); }
        }

        [Column("RequestBody", TypeName = "jsonb")]
        public virtual string? RequestBody
        {
            get => _requestBody;
            set { OnPropertyChanging(); _requestBody = value; OnPropertyChanged(); }
        }

        [Column("UserName")]
        [MaxLength(150)]
        public virtual string? UserName
        {
            get => _userName;
            set { OnPropertyChanging(); _userName = value; OnPropertyChanged(); }
        }

        [Column("IPAddress")]
        [MaxLength(45)]
        public virtual string? IPAddress
        {
            get => _ipAddress;
            set { OnPropertyChanging(); _ipAddress = value; OnPropertyChanged(); }
        }

        [Column("UserAgent")]
        public virtual string? UserAgent
        {
            get => _userAgent;
            set { OnPropertyChanging(); _userAgent = value; OnPropertyChanged(); }
        }

        [Column("StatusCode")]
        public virtual int? StatusCode
        {
            get => _statusCode;
            set { OnPropertyChanging(); _statusCode = value; OnPropertyChanged(); }
        }

        [Column("CreatedAt")]
        public virtual DateTime CreatedAt
        {
            get => _createdAt;
            set { OnPropertyChanging(); _createdAt = value; OnPropertyChanged(); }
        }
    }
}
