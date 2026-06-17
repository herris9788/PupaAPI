using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pupa.BusinessObjects.Beesuite
{
    [Table("RequisitionEngineNumber")]
    public class RequisitionEngineNumber : BaseEntity
    {
        private int? _RequisitionID;
        private string? _EngineNumber;
        

        public RequisitionEngineNumber()
        {
            CylinderNumbers = new ObservableCollection<RequisitionCylinderNumber>();
        }

        [Key]
        public virtual int ID
        {
            get; set;
        }

        public virtual int? RequisitionID
        {
            get => _RequisitionID;
            set
            {
                if (_RequisitionID == value) return;
                OnPropertyChanging();
                _RequisitionID = value;
                OnPropertyChanged();
            }
        }
        public virtual string? EngineNumber
        {
            get => _EngineNumber;
            set
            {
                if (_EngineNumber == value) return;
                OnPropertyChanging();
                _EngineNumber = value;
                OnPropertyChanged();
            }
        }
        public virtual ObservableCollection<RequisitionCylinderNumber> CylinderNumbers { get; set; }

    }
}

