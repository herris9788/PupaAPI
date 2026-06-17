using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pupa.BusinessObjects.Beesuite
{
    [Table("AppConfig")]
    public class AppConfig : BaseEntity
    {
        public AppConfig()
        {
        }

        #region Private Fields
        private int _Id;
        private string? _Key;
        private string? _Namespace;
        private string? _DataType;
        private string? _ValText;
        private int? _ValInteger;
        private long? _ValBigInt;
        private decimal? _ValNumeric;
        private bool? _ValBoolean;
        private DateTimeOffset? _ValTimestamp;
        private DateOnly? _ValDate;
        private TimeSpan? _ValInterval;
        private Guid? _ValUuid;
        private string? _ValJson;
        private byte[]? _ValBytea;
        private string? _Description;
        private bool _IsSecret;
        private bool _IsReadonly;
        private DateTimeOffset _CreatedAt;
        private DateTimeOffset _UpdatedAt;
        private string? _CreatedBy;
        #endregion

        #region Public Properties
        [Key]
        [Column("ID")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public virtual int ID { get { return _Id; } set { OnPropertyChanging(); _Id = value; OnPropertyChanged(); } }

        [Column("Key")]
        public virtual string? Key { get { return _Key; } set { OnPropertyChanging(); _Key = value; OnPropertyChanged(); } }

        [Column("Namespace")]
        public virtual string? Namespace { get { return _Namespace; } set { OnPropertyChanging(); _Namespace = value; OnPropertyChanged(); } }

        [Column("DataType")]
        public virtual string? DataType { get { return _DataType; } set { OnPropertyChanging(); _DataType = value; OnPropertyChanged(); } }

        [Column("ValText")]
        public virtual string? ValText { get { return _ValText; } set { OnPropertyChanging(); _ValText = value; OnPropertyChanged(); } }

        [Column("ValInteger")]
        public virtual int? ValInteger { get { return _ValInteger; } set { OnPropertyChanging(); _ValInteger = value; OnPropertyChanged(); } }

        [Column("ValBigInt")]
        public virtual long? ValBigInt { get { return _ValBigInt; } set { OnPropertyChanging(); _ValBigInt = value; OnPropertyChanged(); } }

        [Column("ValNumeric")]
        public virtual decimal? ValNumeric { get { return _ValNumeric; } set { OnPropertyChanging(); _ValNumeric = value; OnPropertyChanged(); } }

        [Column("ValBoolean")]
        public virtual bool? ValBoolean { get { return _ValBoolean; } set { OnPropertyChanging(); _ValBoolean = value; OnPropertyChanged(); } }

        [Column("ValTimestamp")]
        public virtual DateTimeOffset? ValTimestamp { get { return _ValTimestamp; } set { OnPropertyChanging(); _ValTimestamp = value; OnPropertyChanged(); } }

        [Column("ValDate")]
        public virtual DateOnly? ValDate { get { return _ValDate; } set { OnPropertyChanging(); _ValDate = value; OnPropertyChanged(); } }

        [Column("ValInterval")]
        public virtual TimeSpan? ValInterval { get { return _ValInterval; } set { OnPropertyChanging(); _ValInterval = value; OnPropertyChanged(); } }

        [Column("ValUuid")]
        public virtual Guid? ValUuid { get { return _ValUuid; } set { OnPropertyChanging(); _ValUuid = value; OnPropertyChanged(); } }

        [Column("ValJson", TypeName = "jsonb")]
        public virtual string? ValJson { get { return _ValJson; } set { OnPropertyChanging(); _ValJson = value; OnPropertyChanged(); } }

        [Column("ValBytea")]
        public virtual byte[]? ValBytea { get { return _ValBytea; } set { OnPropertyChanging(); _ValBytea = value; OnPropertyChanged(); } }

        [Column("Description")]
        public virtual string? Description { get { return _Description; } set { OnPropertyChanging(); _Description = value; OnPropertyChanged(); } }

        [Column("IsSecret")]
        public virtual bool IsSecret { get { return _IsSecret; } set { OnPropertyChanging(); _IsSecret = value; OnPropertyChanged(); } }

        [Column("IsReadonly")]
        public virtual bool IsReadonly { get { return _IsReadonly; } set { OnPropertyChanging(); _IsReadonly = value; OnPropertyChanged(); } }

        [Column("CreatedAt")]
        public virtual DateTimeOffset CreatedAt { get { return _CreatedAt; } set { OnPropertyChanging(); _CreatedAt = value; OnPropertyChanged(); } }

        [Column("UpdatedAt")]
        public virtual DateTimeOffset UpdatedAt { get { return _UpdatedAt; } set { OnPropertyChanging(); _UpdatedAt = value; OnPropertyChanged(); } }

        [Column("CreatedBy")]
        public virtual string? CreatedBy { get { return _CreatedBy; } set { OnPropertyChanging(); _CreatedBy = value; OnPropertyChanged(); } }
        #endregion
    }
}