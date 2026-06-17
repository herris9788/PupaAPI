using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pupa.BusinessObjects.Beesuite
{
    [Table("UserV2")]
    public class UserV2: BaseEntity
    {

        public UserV2()
        {
        }
        private int _id;
        private string? _username;
        private string? _fullname;
        private string? _secret = string.Empty;
        private string? _encoding = "base32";
        private string? _role;
        private string? _description;
        private string? _refreshToken;
        private DateTime? _refreshTokenExpiryUtc;

        [Key]
        public virtual int ID
        {
            get => _id;
            set { OnPropertyChanging(); _id = value; OnPropertyChanged(); }
        }

        public virtual string? Username
        {
            get => _username;
            set
            {
                if (_username == value) return;
                OnPropertyChanging();
                _username = value;
                OnPropertyChanged();
            }
        }
        public virtual string? FullName
        {
            get => _fullname;
            set
            {
                if (_fullname == value) return;
                OnPropertyChanging();
                _fullname = value;
                OnPropertyChanged();
            }
        }

        public virtual string? Secret
        {
            get => _secret;
            set
            {
                if (_secret == value) return;
                OnPropertyChanging();
                _secret = value;
                OnPropertyChanged();
            }
        }

        public virtual string? Encoding
        {
            get => _encoding;
            set
            {
                if (_encoding == value) return;
                OnPropertyChanging();
                _encoding = value;
                OnPropertyChanged();
            }
        }

        public virtual string? Role
        {
            get => _role;
            set
            {
                if (_role == value) return;
                OnPropertyChanging();
                _role = value;
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

        public virtual string? RefreshToken
        {
            get => _refreshToken;
            set
            {
                if (_refreshToken == value) return;
                OnPropertyChanging();
                _refreshToken = value;
                OnPropertyChanged();
            }
        }

        public virtual DateTime? RefreshTokenExpiryUtc
        {
            get => _refreshTokenExpiryUtc;
            set
            {
                if (_refreshTokenExpiryUtc == value) return;
                OnPropertyChanging();
                _refreshTokenExpiryUtc = value;
                OnPropertyChanged();
            }
        }
        private bool? _RequiredChangePassword { get; set; }
        public virtual bool? RequiredChangePassword
        {
            get => _RequiredChangePassword;
            set
            {
                if (_RequiredChangePassword == value) return;
                OnPropertyChanging();
                _RequiredChangePassword = value;
                OnPropertyChanged();
            }
        }
        private bool? _IsActive { get; set; }
        public virtual bool? IsActive
        {
            get => _IsActive;
            set
            {
                if (_IsActive == value) return;
                OnPropertyChanging();
                _IsActive = value;
                OnPropertyChanged();
            }
        }
        private int? _No { get; set; }
        public virtual int? No
        {
            get => _No;
            set
            {
                if (_No == value) return;
                OnPropertyChanging();
                _No = value;
                OnPropertyChanged();
            }
        }
        private int? _FailedLoginCount { get; set; }
        public virtual int? FailedLoginCount
        {
            get => _FailedLoginCount;
            set
            {
                if (_FailedLoginCount == value) return;
                OnPropertyChanging();
                _FailedLoginCount = value;
                OnPropertyChanged();
            }
        }
        private DateTime? _SuspendedUntil { get; set; }
        public virtual DateTime? SuspendedUntil
        {
            get => _SuspendedUntil;
            set
            {
                if (_SuspendedUntil == value) return;
                OnPropertyChanging();
                _SuspendedUntil = value;
                OnPropertyChanged();
            }
        }

    }
}