using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pupa.BusinessObjects.Beesuite
{
    /// <summary>
    /// Satu baris = satu perubahan pada sebuah RequisitionFormTemplate:
    /// Create, Update ("save" draft), Publish, Unpublish, Activate, Deactivate,
    /// NewVersion (revisi), Delete.
    ///
    /// TemplateID nullable (ON DELETE SET NULL) — baris "Delete" harus tetap
    /// ada walau template draft-nya sudah benar-benar dihapus dari DB; Code &
    /// Version didenormalisasi supaya histori tetap bisa ditelusuri tanpa FK.
    /// Referensi desain: Pupa/migration_RequisitionFormTemplateAudit.sql
    /// </summary>
    [Table("RequisitionFormTemplateAudit")]
    public class RequisitionFormTemplateAudit : BaseEntity
    {
        private int _id;
        private int? _templateId;
        private string _code = string.Empty;
        private int _version;
        private string _action = string.Empty;
        private string? _oldValue;
        private string? _newValue;
        private string? _remarks;
        private string? _createdBy;
        private DateTime _createdAt = DateTime.Now;

        [Key]
        public virtual int ID
        {
            get => _id;
            set { if (_id == value) return; OnPropertyChanging(); _id = value; OnPropertyChanged(); }
        }

        public virtual int? TemplateID
        {
            get => _templateId;
            set { if (_templateId == value) return; OnPropertyChanging(); _templateId = value; OnPropertyChanged(); }
        }

        [Required, StringLength(100)]
        public virtual string Code
        {
            get => _code;
            set { if (_code == value) return; OnPropertyChanging(); _code = value; OnPropertyChanged(); }
        }

        public virtual int Version
        {
            get => _version;
            set { if (_version == value) return; OnPropertyChanging(); _version = value; OnPropertyChanged(); }
        }

        /// <summary>Create | Update | Publish | Unpublish | Activate | Deactivate | NewVersion | Delete.</summary>
        [Required, StringLength(30)]
        public virtual string Action
        {
            get => _action;
            set { if (_action == value) return; OnPropertyChanging(); _action = value; OnPropertyChanged(); }
        }

        [Column(TypeName = "jsonb")]
        public virtual string? OldValue
        {
            get => _oldValue;
            set { if (_oldValue == value) return; OnPropertyChanging(); _oldValue = value; OnPropertyChanged(); }
        }

        [Column(TypeName = "jsonb")]
        public virtual string? NewValue
        {
            get => _newValue;
            set { if (_newValue == value) return; OnPropertyChanging(); _newValue = value; OnPropertyChanged(); }
        }

        public virtual string? Remarks
        {
            get => _remarks;
            set { if (_remarks == value) return; OnPropertyChanging(); _remarks = value; OnPropertyChanged(); }
        }

        [StringLength(100)]
        public virtual string? CreatedBy
        {
            get => _createdBy;
            set { if (_createdBy == value) return; OnPropertyChanging(); _createdBy = value; OnPropertyChanged(); }
        }

        public virtual DateTime CreatedAt
        {
            get => _createdAt;
            set { if (_createdAt == value) return; OnPropertyChanging(); _createdAt = value; OnPropertyChanged(); }
        }

        [ForeignKey("TemplateID")]
        public virtual RequisitionFormTemplate? Template { get; set; }
    }
}
