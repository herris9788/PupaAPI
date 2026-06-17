using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pupa.BusinessObjects.Beesuite
{
    [Table("LogActivity")]
    public class LogActivity : BaseEntity
    {
        private long _id;
        private int? _documentId;
        private string _moduleName = string.Empty;
        private string _activityType = string.Empty;
        private string? _description;
        private string? _oldValue;
        private string? _newValue;
        private string _userName = string.Empty;
        private string? _userRole;
        private string? _ipAddress;
        private string? _userAgent;
        private DateTime? _createdAt = DateTime.Now;
        private string? _status = "SUCCESS";

        public LogActivity()
        {
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public virtual long ID
        {
            get => _id;
            set
            {
                if (_id == value) return;
                OnPropertyChanging();
                _id = value;
                OnPropertyChanged();
            }
        }

        public virtual int? DocumentID
        {
            get => _documentId;
            set
            {
                if (_documentId == value) return;
                OnPropertyChanging();
                _documentId = value;
                OnPropertyChanged();
            }
        }

        [Required]
        [StringLength(100)]
        public virtual string ModuleName
        {
            get => _moduleName;
            set
            {
                if (_moduleName == value) return;
                OnPropertyChanging();
                _moduleName = value;
                OnPropertyChanged();
            }
        }

        [Required]
        [StringLength(50)]
        public virtual string ActivityType
        {
            get => _activityType;
            set
            {
                if (_activityType == value) return;
                OnPropertyChanging();
                _activityType = value;
                OnPropertyChanged();
            }
        }

        public virtual string? Description
        {
            get => _description;
            set
            {
                if (_description == value) return;
                OnPropertyChanging();
                _description = value;
                OnPropertyChanged();
            }
        }

        [Column(TypeName = "jsonb")]
        public virtual string? OldValue
        {
            get => _oldValue;
            set
            {
                if (_oldValue == value) return;
                OnPropertyChanging();
                _oldValue = value;
                OnPropertyChanged();
            }
        }

        [Column(TypeName = "jsonb")]
        public virtual string? NewValue
        {
            get => _newValue;
            set
            {
                if (_newValue == value) return;
                OnPropertyChanging();
                _newValue = value;
                OnPropertyChanged();
            }
        }

        [Required]
        [StringLength(50)]
        public virtual string UserName
        {
            get => _userName;
            set
            {
                if (_userName == value) return;
                OnPropertyChanging();
                _userName = value;
                OnPropertyChanged();
            }
        }

        [StringLength(50)]
        public virtual string? UserRole
        {
            get => _userRole;
            set
            {
                if (_userRole == value) return;
                OnPropertyChanging();
                _userRole = value;
                OnPropertyChanged();
            }
        }

        [StringLength(45)]
        public virtual string? IPAddress
        {
            get => _ipAddress;
            set
            {
                if (_ipAddress == value) return;
                OnPropertyChanging();
                _ipAddress = value;
                OnPropertyChanged();
            }
        }

        public virtual string? UserAgent
        {
            get => _userAgent;
            set
            {
                if (_userAgent == value) return;
                OnPropertyChanging();
                _userAgent = value;
                OnPropertyChanged();
            }
        }

        public virtual DateTime? CreatedAt
        {
            get => _createdAt;
            set
            {
                if (_createdAt == value) return;
                OnPropertyChanging();
                _createdAt = value;
                OnPropertyChanged();
            }
        }

        [StringLength(20)]
        public virtual string? Status
        {
            get => _status;
            set
            {
                if (_status == value) return;
                OnPropertyChanging();
                _status = value;
                OnPropertyChanged();
            }
        }
    }
}