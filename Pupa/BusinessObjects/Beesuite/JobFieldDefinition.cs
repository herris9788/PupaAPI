using Pupa.Types;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pupa.BusinessObjects.Beesuite
{
    [Table("JobFieldDefinition")]
    public class JobFieldDefinition : BaseEntity
    {
        private int _id;
        private string _formType = string.Empty;
        private string _fieldKey = string.Empty;
        private string _fieldLabel = string.Empty;
        private FieldType _fieldType = FieldType.Text;
        private bool _isRequired;
        private string? _options;
        private int _sortOrder;

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public virtual int ID
        {
            get => _id;
            set { if (_id == value) return; OnPropertyChanging(); _id = value; OnPropertyChanged(); }
        }

        [Required]
        [StringLength(50)]
        public virtual string FormType
        {
            get => _formType;
            set { if (_formType == value) return; OnPropertyChanging(); _formType = value; OnPropertyChanged(); }
        }

        [Required]
        [StringLength(100)]
        public virtual string FieldKey
        {
            get => _fieldKey;
            set { if (_fieldKey == value) return; OnPropertyChanging(); _fieldKey = value; OnPropertyChanged(); }
        }

        [Required]
        [StringLength(255)]
        public virtual string FieldLabel
        {
            get => _fieldLabel;
            set { if (_fieldLabel == value) return; OnPropertyChanging(); _fieldLabel = value; OnPropertyChanged(); }
        }

        [Required]
        public virtual FieldType FieldType
        {
            get => _fieldType;
            set { if (_fieldType == value) return; OnPropertyChanging(); _fieldType = value; OnPropertyChanged(); }
        }

        public virtual bool IsRequired
        {
            get => _isRequired;
            set { if (_isRequired == value) return; OnPropertyChanging(); _isRequired = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// Menyimpan array JSON pilihan dropdown/checkbox
        /// </summary>
        [Column(TypeName = "jsonb")]
        public virtual string? Options
        {
            get => _options;
            set { if (_options == value) return; OnPropertyChanging(); _options = value; OnPropertyChanged(); }
        }

        public virtual int SortOrder
        {
            get => _sortOrder;
            set { if (_sortOrder == value) return; OnPropertyChanging(); _sortOrder = value; OnPropertyChanged(); }
        }
    }
}
