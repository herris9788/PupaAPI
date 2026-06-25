using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pupa.BusinessObjects.Beesuite
{
    /// <summary>
    /// A single line item on a <see cref="Usage"/> document. Item details are
    /// snapshotted as columns so each line stays self-contained for reporting.
    /// </summary>
    [Table("UsageDetail")]
    public class UsageDetail : BaseEntity
    {
        private int _ID;
        private int? _UsageID;
        private int? _ItemID;
        private string? _ItemCode;
        private string? _ItemName;
        private string? _Specification;
        private string? _PartNoEx;
        private string? _Category;
        private string? _Family;
        private string? _VesselPost;
        private string? _UOM;
        private decimal? _Qty;
        private string? _Remarks;

        [Key]
        [Column("ID")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public virtual int ID { get { return _ID; } set { OnPropertyChanging(); _ID = value; OnPropertyChanged(); } }

        /// <summary>Parent usage document.</summary>
        [Column("UsageID")]
        public virtual int? UsageID { get { return _UsageID; } set { OnPropertyChanging(); _UsageID = value; OnPropertyChanged(); } }

        [Column("ItemID")]
        public virtual int? ItemID { get { return _ItemID; } set { OnPropertyChanging(); _ItemID = value; OnPropertyChanged(); } }

        [Column("ItemCode")]
        public virtual string? ItemCode { get { return _ItemCode; } set { OnPropertyChanging(); _ItemCode = value; OnPropertyChanged(); } }

        [Column("ItemName")]
        public virtual string? ItemName { get { return _ItemName; } set { OnPropertyChanging(); _ItemName = value; OnPropertyChanged(); } }

        [Column("Specification")]
        public virtual string? Specification { get { return _Specification; } set { OnPropertyChanging(); _Specification = value; OnPropertyChanged(); } }

        [Column("PartNoEx")]
        public virtual string? PartNoEx { get { return _PartNoEx; } set { OnPropertyChanging(); _PartNoEx = value; OnPropertyChanged(); } }

        [Column("Category")]
        public virtual string? Category { get { return _Category; } set { OnPropertyChanging(); _Category = value; OnPropertyChanged(); } }

        [Column("Family")]
        public virtual string? Family { get { return _Family; } set { OnPropertyChanging(); _Family = value; OnPropertyChanged(); } }

        /// <summary>Deck / Engine (or other post the item was used at).</summary>
        [Column("VesselPost")]
        public virtual string? VesselPost { get { return _VesselPost; } set { OnPropertyChanging(); _VesselPost = value; OnPropertyChanged(); } }

        /// <summary>Unit of measure the quantity was recorded in.</summary>
        [Column("UOM")]
        public virtual string? UOM { get { return _UOM; } set { OnPropertyChanging(); _UOM = value; OnPropertyChanged(); } }

        /// <summary>Quantity consumed.</summary>
        [Column("Qty")]
        public virtual decimal? Qty { get { return _Qty; } set { OnPropertyChanging(); _Qty = value; OnPropertyChanged(); } }

        [Column("Remarks")]
        public virtual string? Remarks { get { return _Remarks; } set { OnPropertyChanging(); _Remarks = value; OnPropertyChanged(); } }

        [ForeignKey("UsageID")]
        public virtual Usage? Usage { get; set; }
    }
}
