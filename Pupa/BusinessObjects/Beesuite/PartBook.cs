using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pupa.BusinessObjects.Beesuite
{
    [Table("Partbook", Schema = "Ascend")]
    public class PartBook : BaseEntity
    {
        public PartBook()
        {
        }

        #region Private Fields
        // Field bawaan yang sudah ada sebelumnya
        private int? _VesselID;
        private string? _VesselName;
        private string? _BookName;
        private string? _FilePath;
        private decimal? _Filesize;
        private int? _FileID;
        private string? _FileName;
        private string? _FileExt;
        private string? _Department;
        private string? _Section;
        private string? _S3Key;
        private DateTime? _CreatedAt;
        private string? _CreatedBy;
        private DateTime? _UpdatedAt;
        private string? _UpdatedBy;
       
        #endregion

        #region Public Properties
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public virtual int ID { get; set; }

        public virtual int? VesselID { get { return _VesselID; } set { OnPropertyChanging(); _VesselID = value; OnPropertyChanged(); } }
        public virtual string? VesselName { get { return _VesselName; } set { OnPropertyChanging(); _VesselName = value; OnPropertyChanged(); } }
        public virtual string? BookName { get { return _BookName; } set { OnPropertyChanging(); _BookName = value; OnPropertyChanged(); } }
        public virtual string? FilePath { get { return _FilePath; } set { OnPropertyChanging(); _FilePath = value; OnPropertyChanged(); } }
        public virtual decimal? Filesize { get { return _Filesize; } set { OnPropertyChanging(); _Filesize = value; OnPropertyChanged(); } }

        // Kolom hasil migrasi dari aplikasi lama (sudah ada di tabel)
        public virtual int? FileID { get { return _FileID; } set { OnPropertyChanging(); _FileID = value; OnPropertyChanged(); } }
        public virtual string? FileName { get { return _FileName; } set { OnPropertyChanging(); _FileName = value; OnPropertyChanged(); } }
        public virtual string? FileExt { get { return _FileExt; } set { OnPropertyChanging(); _FileExt = value; OnPropertyChanged(); } }
        // Baru (migration_PartBookManagement.sql)
        /// <summary>Department buku (mis. Deck / Engine), sesuai aplikasi lama.</summary>
        public virtual string? Department { get { return _Department; } set { OnPropertyChanging(); _Department = value; OnPropertyChanged(); } }
        /// <summary>Section di dalam department.</summary>
        public virtual string? Section { get { return _Section; } set { OnPropertyChanging(); _Section = value; OnPropertyChanged(); } }
        /// <summary>Key objek di S3 (s3-api.waruna-group.co.id). Kosong untuk buku lama yang masih memakai FilePath.</summary>
        public virtual string? S3Key { get { return _S3Key; } set { OnPropertyChanging(); _S3Key = value; OnPropertyChanged(); } }
        public virtual DateTime? CreatedAt { get { return _CreatedAt; } set { OnPropertyChanging(); _CreatedAt = value; OnPropertyChanged(); } }
        public virtual string? CreatedBy { get { return _CreatedBy; } set { OnPropertyChanging(); _CreatedBy = value; OnPropertyChanged(); } }
        public virtual DateTime? UpdatedAt { get { return _UpdatedAt; } set { OnPropertyChanging(); _UpdatedAt = value; OnPropertyChanged(); } }
        public virtual string? UpdatedBy { get { return _UpdatedBy; } set { OnPropertyChanging(); _UpdatedBy = value; OnPropertyChanged(); } }
        #endregion
    }
}