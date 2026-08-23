using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pupa.BusinessObjects.Beesuite
{
    [Table("User")]
    public class User : BaseEntity
    {

        public User()
        {
        }
        private int _id;
        private string? _username = string.Empty;
        private string? _secret = string.Empty;
        private string? _encoding = "base32";
        private string? _role;
        private string? _description;
        private string? _refreshToken;
        private DateTime? _refreshTokenExpiryUtc;
        private string? _position { get; set; }

        private string? _TwoFactorSecret { get; set; }
        private bool? _TwoFactorEnabled { get; set; } = false;

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
        private DateTime? _LastLoginAt { get; set; }
        public virtual DateTime? LastLoginAt
        {
            get => _LastLoginAt;
            set
            {
                if (_LastLoginAt == value) return;
                OnPropertyChanging();
                _LastLoginAt = value;
                OnPropertyChanged();
            }
        }
        private DateTime? _CreatedAt { get; set; }
        public virtual DateTime? CreatedAt
        {
            get => _CreatedAt;
            set
            {
                if (_CreatedAt == value) return;
                OnPropertyChanging();
                _CreatedAt = value;
                OnPropertyChanged();
            }
        }
        public virtual string? Position
        {
            get => _position;
            set
            {
                if (_position == value) return;
                OnPropertyChanging();
                _position = value;
                OnPropertyChanged();
            }
        }
        private string? _FullName { get; set; }
        public virtual string? FullName
        {
            get => _FullName;
            set
            {
                if (_FullName == value) return;
                OnPropertyChanging();
                _FullName = value;
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
        public virtual ObservableCollection<UserVesselRel>? UserVesselRels { get; set; }
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
        private string? _Email { get; set; }
        public virtual string? Email
        {
            get => _Email;
            set
            {
                if (_Email == value) return;
                OnPropertyChanging();
                _Email = value;
                OnPropertyChanged();
            }
        }
        private string? _Phone { get; set; }
        public virtual string? Phone
        {
            get => _Phone;
            set
            {
                if (_Phone == value) return;
                OnPropertyChanging();
                _Phone = value;
                OnPropertyChanged();
            }
        }
        private string? _ImagePath { get; set; }
        public virtual string? ImagePath
        {
            get => _ImagePath;
            set
            {
                if (_ImagePath == value) return;
                OnPropertyChanging();
                _ImagePath = value;
                OnPropertyChanged();
            }
        }
        private string? _ImageBase64 { get; set; }
        public virtual string? ImageBase64
        {
            get => _ImageBase64;
            set
            {
                if (_ImageBase64 == value) return;
                OnPropertyChanging();
                _ImageBase64 = value;
                OnPropertyChanged();
            }
        }
        private string? _ImageType { get; set; }
        public virtual string? ImageType
        {
            get => _ImageType;
            set
            {
                if (_ImageType == value) return;
                OnPropertyChanging();
                _ImageType = value;
                OnPropertyChanged();
            }
        }
        private int?  _FailedLoginCount{ get; set; }
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
        private bool? _CanManageRepair { get; set; }
        public virtual bool? CanManageRepair
        {
            get => _CanManageRepair;
            set
            {
                if (_CanManageRepair == value) return;
                OnPropertyChanging();
                _CanManageRepair = value;
                OnPropertyChanged();
            }
        }

        public virtual string? TwoFactorSecret
        {
            get => _TwoFactorSecret;
            set
            {
                if (_TwoFactorSecret == value) return;
                OnPropertyChanging();
                _TwoFactorSecret = value;
                OnPropertyChanged();
            }
        }
        public virtual bool? TwoFactorEnabled
        {
            get => _TwoFactorEnabled;
            set
            {
                if (_TwoFactorEnabled == value) return;
                OnPropertyChanging();
                _TwoFactorEnabled = value;
                OnPropertyChanged();
            }
        }
        private bool? _TwoFactorConfirmed { get; set; }
        public virtual bool? TwoFactorConfirmed
        {
            get => _TwoFactorConfirmed;
            set
            {
                if (_TwoFactorConfirmed == value) return;
                OnPropertyChanging();
                _TwoFactorConfirmed = value;
                OnPropertyChanged();
            }
        }
        private bool? _Disabled { get; set; }
        public virtual bool? Disabled
        {
            get => _Disabled;
            set
            {
                if (_Disabled == value) return;
                OnPropertyChanging();
                _Disabled = value;
                OnPropertyChanged();
            }
        }
        private string? _Updated { get; set; }
        public virtual string? Updated        {
            get => _Updated;
            set
            {
                if (_Updated == value) return;
                OnPropertyChanging();
                _Updated = value;
                OnPropertyChanged();
            }
        }
        public virtual ObservableCollection<UserApprovalScope>? UserApprovalScopes { get; set; }

        // ── Biometric login (stored device-token, single device per user) ──────
        // Biometric data never reaches the server: the device keeps a random
        // token in its secure storage behind the fingerprint/face prompt, and
        // only the SHA-256 hash of that token is stored here. Enrolling on a new
        // device overwrites these (one biometric device per user). Clear
        // BiometricTokenHash to revoke.
        private string? _BiometricTokenHash { get; set; }
        public virtual string? BiometricTokenHash
        {
            get => _BiometricTokenHash;
            set { if (_BiometricTokenHash == value) return; OnPropertyChanging(); _BiometricTokenHash = value; OnPropertyChanged(); }
        }
        private string? _BiometricDeviceID { get; set; }
        public virtual string? BiometricDeviceID
        {
            get => _BiometricDeviceID;
            set { if (_BiometricDeviceID == value) return; OnPropertyChanging(); _BiometricDeviceID = value; OnPropertyChanged(); }
        }
        private string? _BiometricDeviceName { get; set; }
        public virtual string? BiometricDeviceName
        {
            get => _BiometricDeviceName;
            set { if (_BiometricDeviceName == value) return; OnPropertyChanging(); _BiometricDeviceName = value; OnPropertyChanged(); }
        }
        private string? _BiometricPlatform { get; set; }
        public virtual string? BiometricPlatform
        {
            get => _BiometricPlatform;
            set { if (_BiometricPlatform == value) return; OnPropertyChanging(); _BiometricPlatform = value; OnPropertyChanged(); }
        }
        private DateTime? _BiometricEnabledAt { get; set; }
        public virtual DateTime? BiometricEnabledAt
        {
            get => _BiometricEnabledAt;
            set { if (_BiometricEnabledAt == value) return; OnPropertyChanging(); _BiometricEnabledAt = value; OnPropertyChanged(); }
        }
        private DateTime? _BiometricLastUsedAt { get; set; }
        public virtual DateTime? BiometricLastUsedAt
        {
            get => _BiometricLastUsedAt;
            set { if (_BiometricLastUsedAt == value) return; OnPropertyChanging(); _BiometricLastUsedAt = value; OnPropertyChanged(); }
        }
        private bool? _WA { get; set; }
        public virtual bool? WA
        {
            get => _WA;
            set { if (_WA == value) return; OnPropertyChanging(); _WA = value; OnPropertyChanged(); }
        }
    }
}