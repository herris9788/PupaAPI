using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pupa.BusinessObjects.Beesuite
{
    // One row per calendar day: day-level reservation settings (the window
    // during which staff may reserve). The actual dish options for that day
    // live in LunchMenuItem (one row per dish, each with its own quota).
    [Table("LunchMenu", Schema = "Lunch")]
    public class LunchMenu : BaseEntity
    {
        private int _id;
        private DateTime _date;
        private TimeOnly? _openTime;
        private TimeOnly? _closeTime;
        private bool _isActive = true;

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public virtual int ID
        {
            get => _id;
            set { OnPropertyChanging(); _id = value; OnPropertyChanged(); }
        }

        [Column("Date", TypeName = "date")]
        public virtual DateTime Date
        {
            get => _date;
            set { OnPropertyChanging(); _date = value; OnPropertyChanged(); }
        }

        // Null = no restriction on that side (e.g. OpenTime null means
        // reservations are open from the very start of the day).
        [Column("OpenTime", TypeName = "time")]
        public virtual TimeOnly? OpenTime
        {
            get => _openTime;
            set { OnPropertyChanging(); _openTime = value; OnPropertyChanged(); }
        }

        [Column("CloseTime", TypeName = "time")]
        public virtual TimeOnly? CloseTime
        {
            get => _closeTime;
            set { OnPropertyChanging(); _closeTime = value; OnPropertyChanged(); }
        }

        [Column("IsActive")]
        public virtual bool IsActive
        {
            get => _isActive;
            set { OnPropertyChanging(); _isActive = value; OnPropertyChanged(); }
        }
    }
}
