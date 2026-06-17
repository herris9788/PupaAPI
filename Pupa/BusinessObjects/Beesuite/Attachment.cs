using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pupa.BusinessObjects.Beesuite
{
    [Table("Attachment")]
    public class Attachment : BaseEntity
    {

        public Attachment()
        {
        }

        [Key]
        public virtual int ID
        {
            get; set;
        }
        private string? _FileName { get; set; }

        public virtual string? FileName
        {
            get => _FileName;
            set
            {
                if (_FileName == value) return;
                OnPropertyChanging();
                _FileName = value;
                OnPropertyChanged();
            }
        }
        private string? _Base64 { get; set; }

        public virtual string? Base64
        {
            get => _Base64;
            set
            {
                if (_Base64 == value) return;
                OnPropertyChanging();
                _Base64 = value;
                OnPropertyChanged();
            }
        }
        private string? _Path { get; set; }

        public virtual string? Path
        {
            get => _Path;
            set
            {
                if (_Path == value) return;
                OnPropertyChanging();
                _Path = value;
                OnPropertyChanged();
            }
        }
        private string? _MimeType { get; set; }

        public virtual string? MimeType
        {
            get => _MimeType;
            set
            {
                if (_MimeType == value) return;
                OnPropertyChanging();
                _MimeType = value;
                OnPropertyChanged();
            }
        }
        private bool? _IsBinary { get; set; }

        public virtual bool? IsBinary
        {
            get => _IsBinary;
            set
            {
                if (_IsBinary == value) return;
                OnPropertyChanging();
                _IsBinary = value;
                OnPropertyChanged();
            }
        }
        private int? _Type { get; set; }

        public virtual int? Type
        {
            get => _Type;
            set
            {
                if (_Type == value) return;
                OnPropertyChanging();
                _Type = value;
                OnPropertyChanged();
            }
        }

    }
}