using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pupa.BusinessObjects.Beesuite
{
    // Konsep baru: setiap kolom scope adalah wildcard saat NULL (cocok ke semua
    // nilai pada dimensi itu). Tidak ada lagi sentinel -1 seperti di
    // UserApprovalScope lama (yang butuh -1 khusus untuk StockCategoryID /
    // StockFamilyID tapi NULL untuk kolom lain) — di sini semantiknya konsisten:
    // NULL = "berlaku untuk semua", terisi = "harus persis sama".
    //
    // Resolver disarankan memilih scope dengan Specificity (jumlah kolom terisi)
    // paling tinggi yang match dulu, baru fallback ke yang lebih general. Dengan
    // begitu kombinasi Group/Category/Family/Vessel/VesselGroup/Department/
    // SubDepartment/DB/Level tidak perlu di-hardcode satu-satu seperti cascade
    // 18 rule di UserController lama.
    [Table("UserApprovalScope2")]
    public class UserApprovalScope2 : BaseEntity
    {
        public UserApprovalScope2()
        {
        }

        #region Backing Fields
        private int _ID;
        private int? _UserID;
        private string? _CompanyDB;
        private string? _Group;
        private int? _StockCategoryID;
        private int? _StockFamilyID;
        private int? _VesselID;
        private int? _VesselGroupID;
        private string? _Department;
        private string? _SubDepartment;
        private short? _Level;
        private bool? _IsActive;
        private DateTime? _CreatedAt;
        private DateTime? _UpdatedAt;
        #endregion

        #region Public Properties
        [Key]
        [Column("ID")]
        public virtual int ID
        {
            get => _ID;
            set { if (_ID == value) return; OnPropertyChanging(); _ID = value; OnPropertyChanged(); }
        }

        /// <summary>Approver yang berlaku kalau scope ini yang paling cocok/spesifik.</summary>
        [Column("UserID")]
        public virtual int? UserID
        {
            get => _UserID;
            set { if (_UserID == value) return; OnPropertyChanging(); _UserID = value; OnPropertyChanged(); }
        }

        /// <summary>Company/DB context. NULL = berlaku di semua DB.</summary>
        [Column("CompanyDB")]
        [MaxLength(20)]
        public virtual string? CompanyDB
        {
            get => _CompanyDB;
            set { if (_CompanyDB == value) return; OnPropertyChanging(); _CompanyDB = value; OnPropertyChanged(); }
        }

        /// <summary>Level teratas hierarki stock (Group &gt; Category &gt; Family), disimpan sebagai nama/kode bebas. NULL = berlaku di semua Group.</summary>
        [Column("Group")]
        [MaxLength(50)]
        public virtual string? Group
        {
            get => _Group;
            set { if (_Group == value) return; OnPropertyChanging(); _Group = value; OnPropertyChanged(); }
        }

        [Column("StockCategoryID")]
        public virtual int? StockCategoryID
        {
            get => _StockCategoryID;
            set { if (_StockCategoryID == value) return; OnPropertyChanging(); _StockCategoryID = value; OnPropertyChanged(); }
        }

        [Column("StockFamilyID")]
        public virtual int? StockFamilyID
        {
            get => _StockFamilyID;
            set { if (_StockFamilyID == value) return; OnPropertyChanging(); _StockFamilyID = value; OnPropertyChanged(); }
        }

        /// <summary>Vessel spesifik (InventoryUser.ID). NULL = berlaku ke semua vessel dalam VesselGroup.</summary>
        [Column("VesselID")]
        public virtual int? VesselID
        {
            get => _VesselID;
            set { if (_VesselID == value) return; OnPropertyChanging(); _VesselID = value; OnPropertyChanged(); }
        }

        [Column("VesselGroupID")]
        public virtual int? VesselGroupID
        {
            get => _VesselGroupID;
            set { if (_VesselGroupID == value) return; OnPropertyChanging(); _VesselGroupID = value; OnPropertyChanged(); }
        }

        [Column("Department")]
        [MaxLength(10)]
        public virtual string? Department
        {
            get => _Department;
            set { if (_Department == value) return; OnPropertyChanging(); _Department = value; OnPropertyChanged(); }
        }

        [Column("SubDepartment")]
        public virtual string? SubDepartment
        {
            get => _SubDepartment;
            set { if (_SubDepartment == value) return; OnPropertyChanging(); _SubDepartment = value; OnPropertyChanged(); }
        }

        /// <summary>Urutan approval: 1 = approver pertama … 7 = ketujuh (selaras Requisition.ApprovedBy1..7). NULL = berlaku di semua level.</summary>
        [Column("Level")]
        [Range(1, 7)]
        public virtual short? Level
        {
            get => _Level;
            set { if (_Level == value) return; OnPropertyChanging(); _Level = value; OnPropertyChanged(); }
        }

        /// <summary>Soft-disable tanpa hapus baris. NULL/true = aktif.</summary>
        [Column("IsActive")]
        public virtual bool? IsActive
        {
            get => _IsActive;
            set { if (_IsActive == value) return; OnPropertyChanging(); _IsActive = value; OnPropertyChanged(); }
        }

        [Column("CreatedAt")]
        public virtual DateTime? CreatedAt
        {
            get => _CreatedAt;
            set { if (_CreatedAt == value) return; OnPropertyChanging(); _CreatedAt = value; OnPropertyChanged(); }
        }

        [Column("UpdatedAt")]
        public virtual DateTime? UpdatedAt
        {
            get => _UpdatedAt;
            set { if (_UpdatedAt == value) return; OnPropertyChanging(); _UpdatedAt = value; OnPropertyChanged(); }
        }
        #endregion

        #region Navigation Properties
        [ForeignKey(nameof(UserID))]
        public virtual User? User { get; set; }

        [ForeignKey(nameof(StockCategoryID))]
        public virtual StockCategory? StockCategory { get; set; }

        [ForeignKey(nameof(StockFamilyID))]
        public virtual StockFamily? StockFamily { get; set; }

        [ForeignKey(nameof(VesselID))]
        public virtual InventoryUser? Vessel { get; set; }

        [ForeignKey(nameof(VesselGroupID))]
        public virtual InventoryUserGroup? VesselGroup { get; set; }
        #endregion

        /// <summary>
        /// Jumlah dimensi scope yang diisi (non-NULL). Dipakai resolver untuk
        /// memilih scope paling spesifik saat lebih dari satu scope cocok pada
        /// Level yang sama. Tidak dipetakan ke kolom DB.
        /// </summary>
        [NotMapped]
        public virtual int Specificity =>
            (CompanyDB != null ? 1 : 0) +
            (Group != null ? 1 : 0) +
            (StockCategoryID != null ? 1 : 0) +
            (StockFamilyID != null ? 1 : 0) +
            (VesselID != null ? 1 : 0) +
            (VesselGroupID != null ? 1 : 0) +
            (Department != null ? 1 : 0) +
            (SubDepartment != null ? 1 : 0);
    }
}
