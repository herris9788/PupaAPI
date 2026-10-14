using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pupa.BusinessObjects.Beesuite
{
    [Table("RunningStoreHistory", Schema = "Ascend")]
    public class RunningStoreHistory : BaseEntity
    {
        private int _ID;
        private int _inventoryUserID;
        private DateTime _requestDate;

        [Key]
        [Column("ID")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public virtual int ID
        {
            get => _ID;
            set { OnPropertyChanging(); _ID = value; OnPropertyChanged(); }
        }

        [Column("InventoryUserID")]
        public virtual int InventoryUserID
        {
            get => _inventoryUserID;
            set { OnPropertyChanging(); _inventoryUserID = value; OnPropertyChanged(); }
        }

        [Column("RequestDate")]
        public virtual DateTime RequestDate
        {
            get => _requestDate;
            set { OnPropertyChanging(); _requestDate = value; OnPropertyChanged(); }
        }

        [ForeignKey(nameof(InventoryUserID))]
        public virtual InventoryUser? InventoryUser { get; set; }
    }
}
