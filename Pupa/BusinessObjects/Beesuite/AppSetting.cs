using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pupa.BusinessObjects.Beesuite
{
    [Table("AppSetting")]
    public class AppSetting : BaseEntity
    {
        #region Private Fields
        private int _Id;
        private string? _Category;
        private string? _Key;
        private string? _ScopeType;
        private string? _ScopeValue;
        private string? _Value;
        private string? _ValueSchema;
        private string? _Description;
        private bool _IsActive;
        private string? _CreatedBy;
        private DateTimeOffset _CreatedAt;
        private string? _UpdatedBy;
        private DateTimeOffset _UpdatedAt;
        #endregion

        #region Public Properties
        [Key]
        [Column("ID")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public virtual int ID { get { return _Id; } set { OnPropertyChanging(); _Id = value; OnPropertyChanged(); } }

        [Column("Category")]
        public virtual string? Category { get { return _Category; } set { OnPropertyChanging(); _Category = value; OnPropertyChanged(); } }

        [Column("Key")]
        public virtual string? Key { get { return _Key; } set { OnPropertyChanging(); _Key = value; OnPropertyChanged(); } }

        [Column("ScopeType")]
        public virtual string? ScopeType { get { return _ScopeType; } set { OnPropertyChanging(); _ScopeType = value; OnPropertyChanged(); } }

        [Column("ScopeValue")]
        public virtual string? ScopeValue { get { return _ScopeValue; } set { OnPropertyChanging(); _ScopeValue = value; OnPropertyChanged(); } }

        [Column("Value", TypeName = "jsonb")]
        public virtual string? Value { get { return _Value; } set { OnPropertyChanging(); _Value = value; OnPropertyChanged(); } }

        [Column("ValueSchema", TypeName = "jsonb")]
        public virtual string? ValueSchema { get { return _ValueSchema; } set { OnPropertyChanging(); _ValueSchema = value; OnPropertyChanged(); } }

        [Column("Description")]
        public virtual string? Description { get { return _Description; } set { OnPropertyChanging(); _Description = value; OnPropertyChanged(); } }

        [Column("IsActive")]
        public virtual bool IsActive { get { return _IsActive; } set { OnPropertyChanging(); _IsActive = value; OnPropertyChanged(); } }

        [Column("CreatedBy")]
        public virtual string? CreatedBy { get { return _CreatedBy; } set { OnPropertyChanging(); _CreatedBy = value; OnPropertyChanged(); } }

        [Column("CreatedAt")]
        public virtual DateTimeOffset CreatedAt { get { return _CreatedAt; } set { OnPropertyChanging(); _CreatedAt = value; OnPropertyChanged(); } }

        [Column("UpdatedBy")]
        public virtual string? UpdatedBy { get { return _UpdatedBy; } set { OnPropertyChanging(); _UpdatedBy = value; OnPropertyChanged(); } }

        [Column("UpdatedAt")]
        public virtual DateTimeOffset UpdatedAt { get { return _UpdatedAt; } set { OnPropertyChanging(); _UpdatedAt = value; OnPropertyChanged(); } }
        #endregion
    }
}
