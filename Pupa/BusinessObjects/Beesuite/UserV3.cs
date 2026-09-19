using System.Collections.ObjectModel;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pupa.BusinessObjects.Beesuite
{
    [Table("UserV3")]
    public class UserV3 : BaseEntity
    {

        public UserV3()
        {
        }
        private int _id;
        private string _username = string.Empty;
        private string _secret = string.Empty;
        private string? _encoding = "base32";
        private string? _role;
        private string? _description;
        private string? _refreshToken;
        private DateTime? _refreshTokenExpiryUtc;
        private string? _position { get; set; }

        [Key]
        public virtual int ID
        {
            get => _id;
            set { OnPropertyChanging(); _id = value; OnPropertyChanged(); }
        }

        public virtual string Username
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

        public virtual string Secret
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
        // ── Web login migration (Ascend → UserV3) ────────────────────────────
        // Migrated = akun ini sudah pindah ke UserV3 sebagai sumber login. Saat
        // false, login web masih diverifikasi ke Ascend (lama); saat true,
        // password diverifikasi ke UserV3.Secret dan profil (DBNames/Roles/Buyer/
        // Dept/FullName) dibaca dari ProfileJson — tidak menyentuh Ascend lagi.
        private bool? _Migrated { get; set; }
        [Column("Migrated")]
        public virtual bool? Migrated
        {
            get => _Migrated;
            set
            {
                if (_Migrated == value) return;
                OnPropertyChanging();
                _Migrated = value;
                OnPropertyChanged();
            }
        }
        // Snapshot JSON profil Ascend saat migrasi: { Fullname, DBNames, Roles, Buyer, Dept }.
        private string? _ProfileJson { get; set; }
        [Column("ProfileJson")]
        public virtual string? ProfileJson
        {
            get => _ProfileJson;
            set
            {
                if (_ProfileJson == value) return;
                OnPropertyChanging();
                _ProfileJson = value;
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
        public virtual ObservableCollection<UserApprovalScope>? UserApprovalScopes { get; set; }

        // Employee/national ID — collected once, right after a user's first
        // successful login via UserV3 (auth/v4/login), and usable as an
        // alternate login key alongside Username from then on.
        private string? _NIK { get; set; }
        public virtual string? NIK
        {
            get => _NIK;
            set
            {
                if (_NIK == value) return;
                OnPropertyChanging();
                _NIK = value;
                OnPropertyChanged();
            }
        }
    }
}