using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;

namespace Pupa.BusinessObjects.Beesuite
{
    [Table("AccessLog")]
    public class AccessLog : BaseEntity
    {

        #region Backing Fields
        private string _username = string.Empty;
        private string? _ipAddress;
        private string? _userAgent;
        private bool _success;
        private string? _failReason;
        private string? _endpoint;
        private DateTime _createdAt = DateTime.Now;
        #endregion

        #region Public Properties
        [Key]
        [Column("ID")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public virtual int ID { get; set; }

        [Column("Username")]
        public virtual string Username
        {
            get => _username;
            set { OnPropertyChanging(); _username = value; OnPropertyChanged(); }
        }

        [Column("IPAddress")]
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

        [Column("Success")]
        public virtual bool Success
        {
            get => _success;
            set { OnPropertyChanging(); _success = value; OnPropertyChanged(); }
        }

        [Column("FailReason")]
        public virtual string? FailReason
        {
            get => _failReason;
            set { OnPropertyChanging(); _failReason = value; OnPropertyChanged(); }
        }

        [Column("Endpoint")]
        public virtual string? Endpoint
        {
            get => _endpoint;
            set { OnPropertyChanging(); _endpoint = value; OnPropertyChanged(); }
        }

        [Column("CreatedAt")]
        public virtual DateTime CreatedAt
        {
            get => _createdAt;
            set { OnPropertyChanging(); _createdAt = value; OnPropertyChanged(); }
        }
        #endregion

    }
}