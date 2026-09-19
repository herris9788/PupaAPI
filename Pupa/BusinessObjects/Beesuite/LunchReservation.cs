using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pupa.BusinessObjects.Beesuite
{
    // One row per person per day who reserved a lunch queue number. UserID
    // comes from the JWT "sub" claim (stable even if a display name changes);
    // Username is kept alongside purely for admin display. Rows created by
    // an admin on someone's behalf (the "manual reserve" flow) leave UserID
    // null since there's no token to read it from.
    [Table("LunchReservation", Schema = "Lunch")]
    public class LunchReservation : BaseEntity
    {
        private int _id;
        private DateTime _date;
        private string? _userID;
        private string? _username;
        private int? _menuItemID;
        private string? _menuItemName;
        private string? _choice;
        private int _queueNumber;
        private string _status = "Reserved";
        private DateTime _createdAt = DateTime.UtcNow;
        private DateTime? _pickedUpAt;
        private string? _qrToken;

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

        [Column("UserID")]
        public virtual string? UserID
        {
            get => _userID;
            set { OnPropertyChanging(); _userID = value; OnPropertyChanged(); }
        }

        [Column("Username")]
        public virtual string? Username
        {
            get => _username;
            set { OnPropertyChanging(); _username = value; OnPropertyChanged(); }
        }

        // Which specific dish (LunchMenuItem) was reserved. Null for legacy
        // rows created before menu items existed.
        [Column("MenuItemID")]
        public virtual int? MenuItemID
        {
            get => _menuItemID;
            set { OnPropertyChanging(); _menuItemID = value; OnPropertyChanged(); }
        }

        // Denormalized copies of the chosen LunchMenuItem's Name/Choice at
        // reservation time, so listing reservations doesn't need to join
        // LunchMenuItem (and still displays sensibly if that item is later
        // edited or removed).
        [Column("MenuItemName")]
        public virtual string? MenuItemName
        {
            get => _menuItemName;
            set { OnPropertyChanging(); _menuItemName = value; OnPropertyChanged(); }
        }

        // "Halal" or "NonHalal"
        [Column("Choice")]
        public virtual string? Choice
        {
            get => _choice;
            set { OnPropertyChanging(); _choice = value; OnPropertyChanged(); }
        }

        [Column("QueueNumber")]
        public virtual int QueueNumber
        {
            get => _queueNumber;
            set { OnPropertyChanging(); _queueNumber = value; OnPropertyChanged(); }
        }

        // "Reserved" | "PickedUp" | "Cancelled"
        [Column("Status")]
        public virtual string Status
        {
            get => _status;
            set { OnPropertyChanging(); _status = value; OnPropertyChanged(); }
        }

        [Column("CreatedAt")]
        public virtual DateTime CreatedAt
        {
            get => _createdAt;
            set { OnPropertyChanging(); _createdAt = value; OnPropertyChanged(); }
        }

        [Column("PickedUpAt")]
        public virtual DateTime? PickedUpAt
        {
            get => _pickedUpAt;
            set { OnPropertyChanging(); _pickedUpAt = value; OnPropertyChanged(); }
        }

        // Opaque, unguessable identifier encoded into the QR code shown on the
        // staff member's ticket — scanned at the counter to confirm pickup
        // without the admin having to find the right row in the list by hand.
        [Column("QrToken")]
        public virtual string? QrToken
        {
            get => _qrToken;
            set { OnPropertyChanging(); _qrToken = value; OnPropertyChanged(); }
        }
    }
}
