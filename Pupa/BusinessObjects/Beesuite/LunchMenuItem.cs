using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pupa.BusinessObjects.Beesuite
{
    // One dish option offered on a given day — there can be several per day
    // per Choice (e.g. two Halal dishes, two Non-Halal dishes), each with
    // its own reservation quota.
    [Table("LunchMenuItem", Schema = "Lunch")]
    public class LunchMenuItem : BaseEntity
    {
        private int _id;
        private DateTime _date;
        private string? _name;
        private string? _description;
        private string _choice = "Halal";
        private int _quota;
        private int _sortOrder;
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

        [Column("Name")]
        public virtual string? Name
        {
            get => _name;
            set { OnPropertyChanging(); _name = value; OnPropertyChanged(); }
        }

        [Column("Description")]
        public virtual string? Description
        {
            get => _description;
            set { OnPropertyChanging(); _description = value; OnPropertyChanged(); }
        }

        // "Halal" or "NonHalal"
        [Column("Choice")]
        public virtual string Choice
        {
            get => _choice;
            set { OnPropertyChanging(); _choice = value; OnPropertyChanged(); }
        }

        // Max reservations for this specific dish. 0 = unlimited.
        [Column("Quota")]
        public virtual int Quota
        {
            get => _quota;
            set { OnPropertyChanging(); _quota = value; OnPropertyChanged(); }
        }

        [Column("SortOrder")]
        public virtual int SortOrder
        {
            get => _sortOrder;
            set { OnPropertyChanging(); _sortOrder = value; OnPropertyChanged(); }
        }

        [Column("IsActive")]
        public virtual bool IsActive
        {
            get => _isActive;
            set { OnPropertyChanging(); _isActive = value; OnPropertyChanged(); }
        }
    }
}
