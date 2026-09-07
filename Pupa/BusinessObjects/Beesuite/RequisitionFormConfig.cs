using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pupa.BusinessObjects.Beesuite
{
    /// <summary>
    /// Konfigurasi form dinamis per ItemCode. Mode:
    ///   - "Template" : pakai RequisitionFormTemplate (TemplateID). SchemaJson
    ///                  opsional sebagai override parsial atas schema template.
    ///   - "Custom"   : schema sendiri sepenuhnya di SchemaJson.
    /// Effective schema dihitung di service layer.
    /// Referensi desain: Pupa/docs/dynamic-requisition-wizard-recommended.md
    /// </summary>
    [Table("RequisitionFormConfig")]
    public class RequisitionFormConfig : BaseEntity
    {
        private int _id;
        private string _itemCode = string.Empty;
        private string _entityType = "RequisitionDetail";
        private string _mode = "Template";
        private int? _templateId;
        private string? _schemaJson;
        private bool _isActive = true;
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
        public virtual string ItemCode
        {
            get => _itemCode;
            set { if (_itemCode == value) return; OnPropertyChanging(); _itemCode = value; OnPropertyChanged(); }
        }

        /// <summary>Requisition (header) | RequisitionDetail (per item).</summary>
        [Required, StringLength(30)]
        public virtual string EntityType
        {
            get => _entityType;
            set { if (_entityType == value) return; OnPropertyChanging(); _entityType = value; OnPropertyChanged(); }
        }

        /// <summary>Template | Custom.</summary>
        [Required, StringLength(20)]
        public virtual string Mode
        {
            get => _mode;
            set { if (_mode == value) return; OnPropertyChanging(); _mode = value; OnPropertyChanged(); }
        }

        public virtual int? TemplateID
        {
            get => _templateId;
            set { if (_templateId == value) return; OnPropertyChanging(); _templateId = value; OnPropertyChanged(); }
        }

        [Column(TypeName = "jsonb")]
        public virtual string? SchemaJson
        {
            get => _schemaJson;
            set { if (_schemaJson == value) return; OnPropertyChanging(); _schemaJson = value; OnPropertyChanged(); }
        }

        public virtual bool IsActive
        {
            get => _isActive;
            set { if (_isActive == value) return; OnPropertyChanging(); _isActive = value; OnPropertyChanged(); }
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

        [ForeignKey("TemplateID")]
        public virtual RequisitionFormTemplate? Template { get; set; }
    }
}
