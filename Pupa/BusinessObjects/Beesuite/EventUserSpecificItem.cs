using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pupa.BusinessObjects.Beesuite
{
    [Table("EventUserSpecificItem", Schema = "HRM")]
    public class EventUserSpecificItem : BaseEntity
    {
        public EventUserSpecificItem()
        {
        }

        private int _id;
        private int? _eventId;
        private int? _userId;
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

        [Column("UserID")]
        public virtual int? UserID
        {
            get => _userId;
            set
            {
                if (_userId == value) return;
                OnPropertyChanging();
                _userId = value;
                OnPropertyChanged();
            }
        }
        [ForeignKey("UserID")]
        public virtual UserV2? User { get; set; }

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
        private int? _EventHamperItemID { get; set; }
        public virtual int? EventHamperItemID
        {
            get => _EventHamperItemID;
            set
            {
                if (_EventHamperItemID == value) return;
                OnPropertyChanging();
                _EventHamperItemID = value;
                OnPropertyChanged();
            }
        }
        [ForeignKey("EventHamperItemID")]
        public virtual EventHamperItem? EventHamperItem { get; set; }
    }
}