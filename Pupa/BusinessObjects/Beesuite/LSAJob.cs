using System;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pupa.BusinessObjects.Beesuite
{
    /// <summary>Job LSA yang diproses N8N. Tabel Postgres N8N."LSAJob"; rinciannya di LSAJobDetail (JobID, ON DELETE CASCADE).</summary>
    [Table("LSAJob", Schema = "N8N")]
    public class LSAJob : BaseEntity
    {
        private long _ID;
        private string? _DocumentNo;
        private string? _SupplierCode;
        private string? _Database;
        private string? _FileName;
        private string _Status = "Pending";
        private string? _Error;
        private DateTime _CreatedAt = DateTime.Now;
        private DateTime _UpdatedAt = DateTime.Now;

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("ID")]
        public virtual long ID
        {
            get { return _ID; }
            set { OnPropertyChanging(); _ID = value; OnPropertyChanged(); }
        }

        [Column("DocumentNo")]
        public virtual string? DocumentNo
        {
            get { return _DocumentNo; }
            set { OnPropertyChanging(); _DocumentNo = value; OnPropertyChanged(); }
        }

        [Column("SupplierCode")]
        public virtual string? SupplierCode
        {
            get { return _SupplierCode; }
            set { OnPropertyChanging(); _SupplierCode = value; OnPropertyChanged(); }
        }

        /// <summary>Database Ascend asal job (mis. AS_WNS).</summary>
        [Column("Database")]
        public virtual string? Database
        {
            get { return _Database; }
            set { OnPropertyChanging(); _Database = value; OnPropertyChanged(); }
        }

        [Column("FileName")]
        public virtual string? FileName
        {
            get { return _FileName; }
            set { OnPropertyChanging(); _FileName = value; OnPropertyChanged(); }
        }

        [Column("Status")]
        public virtual string Status
        {
            get { return _Status; }
            set { OnPropertyChanging(); _Status = value; OnPropertyChanged(); }
        }

        [Column("Error")]
        public virtual string? Error
        {
            get { return _Error; }
            set { OnPropertyChanging(); _Error = value; OnPropertyChanged(); }
        }

        [Column("CreatedAt")]
        public virtual DateTime CreatedAt
        {
            get { return _CreatedAt; }
            set { OnPropertyChanging(); _CreatedAt = value; OnPropertyChanged(); }
        }

        [Column("UpdatedAt")]
        public virtual DateTime UpdatedAt
        {
            get { return _UpdatedAt; }
            set { OnPropertyChanging(); _UpdatedAt = value; OnPropertyChanged(); }
        }

        /// <summary>Rincian milik job ini (LSAJobDetail.JobID). Ambil lewat $expand=Details.</summary>
        public virtual ObservableCollection<LSAJobDetail>? Details { get; set; }
    }
}
