using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pupa.BusinessObjects.Beesuite
{
    [Table("EventParticipant", Schema = "HRM")]
    public class EventParticipant : BaseEntity
    {
        public EventParticipant()
        {
        }

        private int _id;
        private int? _eventId;
        private int? _userId;
        private DateTime? _joinedAt;
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
        [Column("JoinedAt")]
        public virtual DateTime? JoinedAt
        {
            get => _joinedAt;
            set
            {
                if (_joinedAt == value) return;
                OnPropertyChanging();
                _joinedAt = value;
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
    }
}