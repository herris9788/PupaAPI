using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pupa.BusinessObjects.Beesuite
{
    [Table("JobAttachment", Schema = "public")]
    public class JobAttachment : BaseEntity
    {
        private int _id;
        [Key]
        public virtual int ID
        {
            get => _id;
            set { OnPropertyChanging(); _id = value; OnPropertyChanged(); }
        }

        private int? _jobID;
        public virtual int? JobID
        {
            get => _jobID;
            set { OnPropertyChanging(); _jobID = value; OnPropertyChanged(); }
        }

        private string? _name;
        public virtual string? Name
        {
            get => _name;
            set { OnPropertyChanging(); _name = value; OnPropertyChanged(); }
        }

        private string? _mimeType;
        public virtual string? MimeType
        {
            get => _mimeType;
            set { OnPropertyChanging(); _mimeType = value; OnPropertyChanged(); }
        }

        private string? _kind;
        public virtual string? Kind
        {
            get => _kind;
            set { OnPropertyChanging(); _kind = value; OnPropertyChanged(); }
        }

        private string? _storageProvider;
        public virtual string? StorageProvider
        {
            get => _storageProvider;
            set { OnPropertyChanging(); _storageProvider = value; OnPropertyChanged(); }
        }

        private string? _bucketName;
        public virtual string? BucketName
        {
            get => _bucketName;
            set { OnPropertyChanging(); _bucketName = value; OnPropertyChanged(); }
        }

        private string? _storageKey;
        public virtual string? StorageKey
        {
            get => _storageKey;
            set { OnPropertyChanging(); _storageKey = value; OnPropertyChanged(); }
        }

        private string? _previewUrl;
        public virtual string? PreviewUrl
        {
            get => _previewUrl;
            set { OnPropertyChanging(); _previewUrl = value; OnPropertyChanged(); }
        }

        private int? _fileSize;
        public virtual int? FileSize
        {
            get => _fileSize;
            set { OnPropertyChanging(); _fileSize = value; OnPropertyChanged(); }
        }

        private string? _fileExtension;
        public virtual string? FileExtension
        {
            get => _fileExtension;
            set { OnPropertyChanging(); _fileExtension = value; OnPropertyChanged(); }
        }

        private int? _sortOrder;
        public virtual int? SortOrder
        {
            get => _sortOrder;
            set { OnPropertyChanging(); _sortOrder = value; OnPropertyChanged(); }
        }

        private DateTime? _createdAt;
        public virtual DateTime? CreatedAt
        {
            get => _createdAt;
            set { OnPropertyChanging(); _createdAt = value; OnPropertyChanged(); }
        }

        private DateTime? _updatedAt;
        public virtual DateTime? UpdatedAt
        {
            get => _updatedAt;
            set { OnPropertyChanging(); _updatedAt = value; OnPropertyChanged(); }
        }

        // Navigation property

        [ForeignKey("JobID")]
        public virtual Job? Job { get; set; }
    }
}