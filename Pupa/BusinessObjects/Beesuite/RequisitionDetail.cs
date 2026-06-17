using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pupa.BusinessObjects.Beesuite
{
    [Table("RequisitionDetail")]
    public class RequisitionDetail : BaseEntity
    {
        private int _id;
        private int? _requisitionId;
        private int? _itemId;
        private decimal? _qtyRequest = 0;
        private decimal? _qtyApproved = 0;
        private decimal? _approvedQuantityByManTech = 0;
        private decimal? _approvedQuantityByPurchasing = 0;

        private DateTime? _createdAt = DateTime.Now;
        private DateTime? _updatedAt = DateTime.Now;
        private string? _partBookAttachmentPath;
        public RequisitionDetail() {
            this.RequisitionDetailAttachmentRels = new ObservableCollection<RequisitionDetailAttachmentRel>();
        }

        [Key]
        public virtual int ID
        {
            get => _id;
            set
            {
                if (_id == value) return;
                OnPropertyChanging();
                _id = value;
                OnPropertyChanged();
            }
        }

        public virtual int? RequisitionID
        {
            get => _requisitionId;
            set
            {
                if (_requisitionId == value) return;
                OnPropertyChanging();
                _requisitionId = value;
                OnPropertyChanged();
            }
        }

        public virtual int? ItemID
        {
            get => _itemId;
            set
            {
                if (_itemId == value) return;
                OnPropertyChanging();
                _itemId = value;
                OnPropertyChanged();
            }
        }

        public virtual decimal? QtyRequest
        {
            get => _qtyRequest;
            set
            {
                if (_qtyRequest == value) return;
                OnPropertyChanging();
                _qtyRequest = value;
                OnPropertyChanged();
            }
        }

        public virtual decimal? QtyApproved
        {
            get => _qtyApproved;
            set
            {
                if (_qtyApproved == value) return;
                OnPropertyChanging();
                _qtyApproved = value;
                OnPropertyChanged();
            }
        }
        private decimal? _QtyApproved1 { get; set; }
        public virtual decimal? QtyApproved1
        {
            get => _QtyApproved1;
            set
            {
                if (_QtyApproved1 == value) return;
                OnPropertyChanging();
                _QtyApproved1 = value;
                OnPropertyChanged();
            }
        }
        private decimal? _QtyApproved2 { get; set; }
        public virtual decimal? QtyApproved2
        {
            get => _QtyApproved2;
            set
            {
                if (_QtyApproved2 == value) return;
                OnPropertyChanging();
                _QtyApproved2 = value;
                OnPropertyChanged();
            }
        }
        private decimal? _QtyApproved3 { get; set; }
        public virtual decimal? QtyApproved3
        {
            get => _QtyApproved3;
            set
            {
                if (_QtyApproved3 == value) return;
                OnPropertyChanging();
                _QtyApproved3 = value;
                OnPropertyChanged();
            }
        }
        private decimal? _QtyApproved4 { get; set; }
        public virtual decimal? QtyApproved4
        {
            get => _QtyApproved4;
            set
            {
                if (_QtyApproved4 == value) return;
                OnPropertyChanging();
                _QtyApproved4 = value;
                OnPropertyChanged();
            }
        }
        private decimal? _QtyApproved5 { get; set; }
        public virtual decimal? QtyApproved5
        {
            get => _QtyApproved5;
            set
            {
                if (_QtyApproved5 == value) return;
                OnPropertyChanging();
                _QtyApproved5 = value;
                OnPropertyChanged();
            }
        }
        private decimal? _QtyApproved6 { get; set; }
        public virtual decimal? QtyApproved6
        {
            get => _QtyApproved6;
            set
            {
                if (_QtyApproved6 == value) return;
                OnPropertyChanging();
                _QtyApproved6 = value;
                OnPropertyChanged();
            }
        }
        private decimal? _QtyApproved7 { get; set; }
        public virtual decimal? QtyApproved7
        {
            get => _QtyApproved7;
            set
            {
                if (_QtyApproved7 == value) return;
                OnPropertyChanging();
                _QtyApproved7 = value;
                OnPropertyChanged();
            }
        }

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
        private string? _Remarks { get; set; }
        public virtual string? Remarks
        {
            get => _Remarks;
            set
            {
                if (_Remarks == value) return;
                OnPropertyChanging();
                _Remarks = value;
                OnPropertyChanged();
            }
        }

        [ForeignKey("RequisitionID")]
        public virtual Requisition? Requisition { get; set; }

        [ForeignKey("ItemID")]
        public virtual Item? Item { get; set; }

      
        public virtual decimal? QtyApprovedManTech
        {
            get => _approvedQuantityByManTech;
            set
            {
                if (_approvedQuantityByManTech == value) return;
                OnPropertyChanging();
                _approvedQuantityByManTech = value;
                OnPropertyChanged();
            }
        }
        public virtual decimal? QtyApprovedPurchasing
        {
            get => _approvedQuantityByPurchasing;
            set
            {
                if (_approvedQuantityByPurchasing == value) return;
                OnPropertyChanging();
                _approvedQuantityByPurchasing = value;
                OnPropertyChanged();
            }
        }
        private string? _ItemDescription { get; set; }
        public virtual string? ItemDescription
        {
            get => _ItemDescription;
            set
            {
                if (_ItemDescription == value) return;
                OnPropertyChanging();
                _ItemDescription = value;
                OnPropertyChanged();
            }
        }
        private string? _Purpose { get; set;  }
        public virtual string? Purpose
        {
            get => _Purpose;
            set
            {
                if (_Purpose == value) return;
                OnPropertyChanging();
                _Purpose = value;
                OnPropertyChanged();
            }
        }
        private string? _PlacementArea { get; set; }
        public virtual string? PlacementArea
        {
            get => _PlacementArea;
            set
            {
                if (_PlacementArea == value) return;
                OnPropertyChanging();
                _PlacementArea = value;
                OnPropertyChanged();
            }
        }
        private string? _Comments { get; set; }
        public virtual string? Comments
        {
            get => _Comments;
            set
            {
                if (_Comments == value) return;
                OnPropertyChanging();
                _Comments = value;
                OnPropertyChanged();
            }
        }
        private int? _UOMLevel { get; set; }
        public virtual int? UOMLevel
        {
            get => _UOMLevel;
            set
            {
                if (_UOMLevel == value) return;
                OnPropertyChanging();
                _UOMLevel = value;
                OnPropertyChanged();
            }
        }
        public virtual string? PartBookAttachmentPath
        {
            get => _partBookAttachmentPath;
            set
            {
                if (_partBookAttachmentPath == value) return;
                OnPropertyChanging();
                _partBookAttachmentPath = value;
                OnPropertyChanged();
            }
        }
        private string? _WarehouseCode { get; set; }
        public virtual string? WarehouseCode
        {
            get => _WarehouseCode;
            set
            {
                if (_WarehouseCode == value) return;
                OnPropertyChanging();
                _WarehouseCode = value;
                OnPropertyChanged();
            }
        }
        private int? _BookingID { get; set;  }
        public virtual int? BookingID
        {
            get => _BookingID;
            set
            {
                if (_BookingID == value) return;
                OnPropertyChanging();
                _BookingID = value;
                OnPropertyChanged();
            }
        }
        private decimal? _QtyBooked { get; set; }
        public virtual decimal? QtyBooked
        {
            get => _QtyBooked;
            set
            {
                if (_QtyBooked == value) return;
                OnPropertyChanging();
                _QtyBooked = value;
                OnPropertyChanged();
            }
        }
        private string? _Edition { get; set; }
        public virtual string? Edition
        {
            get => _Edition;
            set
            {
                if (_Edition == value) return;
                OnPropertyChanging();
                _Edition = value;
                OnPropertyChanged();
            }
        }
        private string? _Brand { get; set; }
        public virtual string? Brand
        {
            get => _Brand;
            set
            {
                if (_Brand == value) return;
                OnPropertyChanging();
                _Brand = value;
                OnPropertyChanged();
            }
        }
        private string? _Size { get; set; }
        public virtual string? Size
        {
            get => _Size;
            set
            {
                if (_Size == value) return;
                OnPropertyChanging();
                _Size = value;
                OnPropertyChanged();
            }
        }
        private string? _Model { get; set; }
        public virtual string? Model
        {
            get => _Model;
            set
            {
                if (_Model == value) return;
                OnPropertyChanging();
                _Model = value;
                OnPropertyChanged();
            }
        }
        public virtual ObservableCollection<RequisitionDetailAttachmentRel> RequisitionDetailAttachmentRels { get; set; }

    }
}