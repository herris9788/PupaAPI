using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pupa.BusinessObjects.Beesuite
{
    [Table("JobFieldValue")]
    public class JobFieldValue : BaseEntity
    {
        private int _id;
        [Key]
        public virtual int ID
        {
            get => _id;
            set { OnPropertyChanging(); _id = value; OnPropertyChanged(); }
        }

        private string _entityType;
        public virtual string? EntityType
        {
            get => _entityType;
            set { OnPropertyChanging(); _entityType = value; OnPropertyChanged(); }
        }

        private int? _entityID;
        public virtual int? EntityID
        {
            get => _entityID;
            set { OnPropertyChanging(); _entityID = value; OnPropertyChanged(); }
        }

        private string? _fieldKey;
        public virtual string? FieldKey
        {
            get => _fieldKey;
            set { OnPropertyChanging(); _fieldKey = value; OnPropertyChanged(); }
        }

        private int? _fieldDefinitionID;
        public virtual int? FieldDefinitionID
        {
            get => _fieldDefinitionID;
            set { OnPropertyChanging(); _fieldDefinitionID = value; OnPropertyChanged(); }
        }

        private string? _valueType = "text";
        public virtual string? ValueType
        {
            get => _valueType;
            set { OnPropertyChanging(); _valueType = value; OnPropertyChanged(); }
        }

        private string? _valueText;
        public virtual string? ValueText
        {
            get => _valueText;
            set { OnPropertyChanging(); _valueText = value; OnPropertyChanged(); }
        }

        private decimal? _valueNumber;
        public virtual decimal? ValueNumber
        {
            get => _valueNumber;
            set { OnPropertyChanging(); _valueNumber = value; OnPropertyChanged(); }
        }

        private DateTime? _valueDate;
        public virtual DateTime? ValueDate
        {
            get => _valueDate;
            set { OnPropertyChanging(); _valueDate = value; OnPropertyChanged(); }
        }

        private bool? _valueBool;
        public virtual bool? ValueBool
        {
            get => _valueBool;
            set { OnPropertyChanging(); _valueBool = value; OnPropertyChanged(); }
        }

        private string? _valueJson;
        public virtual string? ValueJson
        {
            get => _valueJson;
            set { OnPropertyChanging(); _valueJson = value; OnPropertyChanged(); }
        }

        private DateTime? _createdAt = DateTime.UtcNow;
        public virtual DateTime? CreatedAt
        {
            get => _createdAt;
            set { OnPropertyChanging(); _createdAt = value; OnPropertyChanged(); }
        }

        private DateTime? _updatedAt = DateTime.UtcNow;
        public virtual DateTime? UpdatedAt
        {
            get => _updatedAt;
            set { OnPropertyChanging(); _updatedAt = value; OnPropertyChanged(); }
        }

        private string? _createdBy;
        public virtual string? CreatedBy
        {
            get => _createdBy;
            set { OnPropertyChanging(); _createdBy = value; OnPropertyChanged(); }
        }

        private string? _updatedBy;
        public virtual string? UpdatedBy
        {
            get => _updatedBy;
            set { OnPropertyChanging(); _updatedBy = value; OnPropertyChanged(); }
        }

        // Navigation property
        [ForeignKey("FieldDefinitionID")]
        public virtual JobFieldDefinition? FieldDefinition { get; set; }

        public virtual JobRequest? JobRequest { get; set; }
        public virtual Job? Job { get; set; }
    }
}