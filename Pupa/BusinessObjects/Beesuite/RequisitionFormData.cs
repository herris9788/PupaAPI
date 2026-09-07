using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pupa.BusinessObjects.Beesuite
{
    /// <summary>
    /// Jawaban form dinamis milik satu Requisition (RequisitionDetailID NULL =
    /// form header) atau satu RequisitionDetail.
    ///
    /// SchemaSnapshot menyimpan effective schema MENTAH saat submit — jadi data
    /// ini tetap bisa dirender & divalidasi walau Config/Template berubah atau
    /// dihapus di kemudian hari. ConfigID/TemplateID hanya referensi lunak
    /// (ON DELETE SET NULL).
    /// Referensi desain: Pupa/docs/dynamic-requisition-wizard-recommended.md
    /// </summary>
    [Table("RequisitionFormData")]
    public class RequisitionFormData : BaseEntity
    {
        private int _id;
        private int _requisitionId;
        private int? _requisitionDetailId;
        private string? _itemCode;
        private int? _configId;
        private int? _templateId;
        private string _schemaSnapshot = "{}";
        private string _values = "{}";
        private string _status = "Draft";
        private DateTime? _createdAt = DateTime.Now;
        private DateTime? _updatedAt = DateTime.Now;
        private string? _createdBy;
        private string? _updatedBy;

        [Key]
        public virtual int ID
        {
            get => _id;
            set { if (_id == value) return; OnPropertyChanging(); _id = value; OnPropertyChanged(); }
        }

        public virtual int RequisitionID
        {
            get => _requisitionId;
            set { if (_requisitionId == value) return; OnPropertyChanging(); _requisitionId = value; OnPropertyChanged(); }
        }

        /// <summary>NULL = form milik header Requisition.</summary>
        public virtual int? RequisitionDetailID
        {
            get => _requisitionDetailId;
            set { if (_requisitionDetailId == value) return; OnPropertyChanging(); _requisitionDetailId = value; OnPropertyChanged(); }
        }

        [StringLength(100)]
        public virtual string? ItemCode
        {
            get => _itemCode;
            set { if (_itemCode == value) return; OnPropertyChanging(); _itemCode = value; OnPropertyChanged(); }
        }

        public virtual int? ConfigID
        {
            get => _configId;
            set { if (_configId == value) return; OnPropertyChanging(); _configId = value; OnPropertyChanged(); }
        }

        public virtual int? TemplateID
        {
            get => _templateId;
            set { if (_templateId == value) return; OnPropertyChanging(); _templateId = value; OnPropertyChanged(); }
        }

        /// <summary>Effective schema mentah saat data ini dibuat/diubah.</summary>
        [Required]
        [Column(TypeName = "jsonb")]
        public virtual string SchemaSnapshot
        {
            get => _schemaSnapshot;
            set { if (_schemaSnapshot == value) return; OnPropertyChanging(); _schemaSnapshot = value; OnPropertyChanged(); }
        }

        /// <summary>Jawaban: { "FieldKey": &lt;nilai&gt;, ... }.</summary>
        [Required]
        [Column(TypeName = "jsonb")]
        public virtual string Values
        {
            get => _values;
            set { if (_values == value) return; OnPropertyChanging(); _values = value; OnPropertyChanged(); }
        }

        /// <summary>Draft | Completed.</summary>
        [Required, StringLength(20)]
        public virtual string Status
        {
            get => _status;
            set { if (_status == value) return; OnPropertyChanging(); _status = value; OnPropertyChanged(); }
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

        [StringLength(100)]
        public virtual string? UpdatedBy
        {
            get => _updatedBy;
            set { if (_updatedBy == value) return; OnPropertyChanging(); _updatedBy = value; OnPropertyChanged(); }
        }

        [ForeignKey("RequisitionID")]
        public virtual Requisition? Requisition { get; set; }

        [ForeignKey("RequisitionDetailID")]
        public virtual RequisitionDetail? RequisitionDetail { get; set; }

        [ForeignKey("ConfigID")]
        public virtual RequisitionFormConfig? Config { get; set; }

        [ForeignKey("TemplateID")]
        public virtual RequisitionFormTemplate? Template { get; set; }
    }
}
