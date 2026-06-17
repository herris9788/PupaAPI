using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pupa.BusinessObjects.Beesuite
{
    [Table("EventHamperItem", Schema = "HRM")]
    public class EventHamperItem : BaseEntity
    {
        public EventHamperItem()
        {
            this.EventUserSpecificItems = new ObservableCollection<EventUserSpecificItem>();
        }

        private int _id;
        private int? _eventId;
        private string? _itemName = string.Empty;
        private Event? _event;

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("ID")]
        public virtual int ID
        {
            get => _id;
            set { OnPropertyChanging(); _id = value; OnPropertyChanged(); }
        }

        [Column("EventID")]
        public virtual int? EventID
        {
            get => _eventId;
            set
            {
                if (_eventId == value) return;
                OnPropertyChanging();
                _eventId = value;
                OnPropertyChanged();
            }
        }

        [Column("ItemName")]
        [MaxLength(255)]
        public virtual string? ItemName
        {
            get => _itemName;
            set
            {
                if (_itemName == value) return;
                OnPropertyChanging();
                _itemName = value;
                OnPropertyChanged();
            }
        }

        // Navigation Property ke tabel Event
        [ForeignKey("EventID")]
        public virtual Event? Event
        {
            get => _event;
            set
            {
                if (_event == value) return;
                OnPropertyChanging();
                _event = value;
                OnPropertyChanged();
            }
        }
        public virtual ObservableCollection<EventUserSpecificItem> EventUserSpecificItems { get; set; }
    }
}