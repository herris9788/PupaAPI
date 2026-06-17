using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pupa.BusinessObjects.Beesuite
{
    [Table("Event", Schema = "HRM")]
    public class Event : BaseEntity
    {
        public Event()
        {
            this.Participants = new ObservableCollection<EventParticipant>();
            this.Items = new ObservableCollection<EventHamperItem>();
            this.SpecificationItems = new ObservableCollection<EventUserSpecificItem>();
        }

        private int _id;
        private string? _title = string.Empty;
        private string? _description;
        private DateTime? _date;
        private string? _type = string.Empty;
        private DateTime? _createdAt;
        private DateTime? _updatedAt;

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("ID")]
        public virtual int ID
        {
            get => _id;
            set { OnPropertyChanging(); _id = value; OnPropertyChanged(); }
        }

        [Required]
        [Column("Title")]
        [MaxLength(255)]
        public virtual string? Title
        {
            get => _title;
            set
            {
                if (_title == value) return;
                OnPropertyChanging();
                _title = value;
                OnPropertyChanged();
            }
        }

        [Column("Description")]
        public virtual string? Description
        {
            get => _description;
            set
            {
                if (_description == value) return;
                OnPropertyChanging();
                _description = value;
                OnPropertyChanged();
            }
        }

        [Required]
        [Column("Date")]
        public virtual DateTime? Date
        {
            get => _date;
            set
            {
                if (_date == value) return;
                OnPropertyChanging();
                _date = value;
                OnPropertyChanged();
            }
        }

        [Required]
        [Column("Type")]
        [MaxLength(50)]
        public virtual string? Type // 'Meeting' or 'CorporateHamper'
        {
            get => _type;
            set
            {
                if (_type == value) return;
                OnPropertyChanging();
                _type = value;
                OnPropertyChanged();
            }
        }

        [Column("CreatedAt")]
        public virtual DateTime? CreatedAt
        {
            get => _createdAt;
            set
            {
                if (_createdAt == value) return;
                OnPropertyChanging();
                _createdAt = value;
                OnPropertyChanged();
            }
        }

        [Column("UpdatedAt")]
        public virtual DateTime? UpdatedAt
        {
            get => _updatedAt;
            set
            {
                if (_updatedAt == value) return;
                OnPropertyChanging();
                _updatedAt = value;
                OnPropertyChanged();
            }
        }
        public virtual ObservableCollection<EventParticipant> Participants { get; set; }
        public virtual ObservableCollection<EventHamperItem> Items { get; set; }
        public virtual ObservableCollection<EventUserSpecificItem> SpecificationItems { get; set; }

    }
}