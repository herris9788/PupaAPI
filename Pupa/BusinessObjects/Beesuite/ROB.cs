using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pupa.BusinessObjects.Beesuite
{
    [Table("ROB", Schema = "Ascend")]
    public class ROB : BaseEntity
    {
        public ROB() {
      
        }

        #region Private Fields
        private int _ID;
        private string? _DB;
        private string? _InventoryUserCode;
        private string? _VesselPost;
        private string? _ItemCode;
        private int? _ItemID;
        private decimal? _Qty;
        #endregion

        #region Public Properties
        [Key]
        [Column("ID")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public virtual int ID { get { return _ID; } set { OnPropertyChanging(); _ID = value; OnPropertyChanged(); } }

        [Column("DB")]
        public virtual string? DB { get { return _DB; } set { OnPropertyChanging(); _DB = value; OnPropertyChanged(); } }

        [Column("InventoryUserCode")]
        public virtual string? InventoryUserCode { get { return _InventoryUserCode; } set { OnPropertyChanging(); _InventoryUserCode = value; OnPropertyChanged(); } }

        [Column("VesselPost")]
        public virtual string? VesselPost { get { return _VesselPost; } set { OnPropertyChanging(); _VesselPost = value; OnPropertyChanged(); } }

        [Column("ItemCode")]
        public virtual string? ItemCode { get { return _ItemCode; } set { OnPropertyChanging(); _ItemCode = value; OnPropertyChanged(); } }

        [Column("ItemID")]
        public virtual int? ItemID { get { return _ItemID; } set { OnPropertyChanging(); _ItemID = value; OnPropertyChanged(); } }

        [Column("Qty")]
        public virtual decimal? Qty { get { return _Qty; } set { OnPropertyChanging(); _Qty = value; OnPropertyChanged(); } }
        #endregion

        public virtual Item? Item { get; set; }
        public virtual InventoryUser? InventoryUser { get; set; }
    }
}
