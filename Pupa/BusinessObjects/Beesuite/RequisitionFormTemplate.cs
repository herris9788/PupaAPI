using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pupa.BusinessObjects.Beesuite
{
    /// <summary>
    /// Blueprint wizard dinamis yang bisa dipakai ulang (pustaka schema).
    /// Versioned: baris dengan IsPublished=true bersifat immutable — perubahan
    /// membuat Version baru, bukan meng-UPDATE baris lama.
    /// Referensi desain: Pupa/docs/dynamic-requisition-wizard-recommended.md
    /// </summary>
    [Table("RequisitionFormTemplate")]
    public class RequisitionFormTemplate : BaseEntity
    {
        private int _id;
        private string _code = string.Empty;
        private string _name = string.Empty;
        private int _version = 1;
        private string _schemaJson = "{}";
        private bool _isActive = true;
        private bool _isPublished;
        private DateTime? _createdAt = DateTime.Now;
        private DateTime? _updatedAt = DateTime.Now;
        private string? _createdBy;

        [Key]
        public virtual int ID
        {
            get => _id;
            set { if (_id == value) return; OnPropertyChanging(); _id = value; OnPropertyChanged(); }
        }

        [Required, StringLength(100)]
        public virtual string Code
        {
            get => _code;
            set { if (_code == value) return; OnPropertyChanging(); _code = value; OnPropertyChanged(); }
        }

        [Required, StringLength(255)]
        public virtual string Name
        {
            get => _name;
            set { if (_name == value) return; OnPropertyChanging(); _name = value; OnPropertyChanged(); }
        }

        public virtual int Version
        {
            get => _version;
            set { if (_version == value) return; OnPropertyChanging(); _version = value; OnPropertyChanged(); }
        }

        /// <summary>Definisi wizard: steps -> fields, kondisi, validasi, prefill.</summary>
        [Required]
        [Column(TypeName = "jsonb")]
        public virtual string SchemaJson
        {
            get => _schemaJson;
            set { if (_schemaJson == value) return; OnPropertyChanging(); _schemaJson = value; OnPropertyChanged(); }
        }

        public virtual bool IsActive
        {
            get => _isActive;
            set { if (_isActive == value) return; OnPropertyChanging(); _isActive = value; OnPropertyChanged(); }
        }

        public virtual bool IsPublished
        {
            get => _isPublished;
            set { if (_isPublished == value) return; OnPropertyChanging(); _isPublished = value; OnPropertyChanged(); }
        }

        public virtual DateTime? CreatedAt
        {
            get => _createdAt;
            set { if (_createdAt == value) return; OnPropertyChanging(); _createdAt = value; OnPropertyChanged(); }
        }

        public virtual DateTime? UpdatedAt
        {
            get => _updatedAt;
            set { if (_updatedAt == value) return; OnPropertyChanging(); _updatedAt = value; OnPropertyChanged(); }
        }

        [StringLength(100)]
        public virtual string? CreatedBy
        {
            get => _createdBy;
            set { if (_createdBy == value) return; OnPropertyChanging(); _createdBy = value; OnPropertyChanged(); }
        }
    }
}
