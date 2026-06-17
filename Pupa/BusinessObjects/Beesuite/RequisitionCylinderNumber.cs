using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pupa.BusinessObjects.Beesuite
{
    [Table("RequisitionCylinderNumber")]
    public class RequisitionCylinderNumber : BaseEntity
    {
        private int? _RequisitionEngineNumberID;
        private string? _CylinderNumber;
        

        public RequisitionCylinderNumber()
        {
        }

        [Key]
        public virtual int ID
        {
            get; set;
        }

        public virtual int? RequisitionEngineNumberID
        {
            get => _RequisitionEngineNumberID;
            set
            {
                if (_RequisitionEngineNumberID == value) return;
                OnPropertyChanging();
                _RequisitionEngineNumberID = value;
                OnPropertyChanged();
            }
        }
        public virtual string? CylinderNumber
        {
            get => _CylinderNumber;
            set
            {
                if (_CylinderNumber == value) return;
                OnPropertyChanging();
                _CylinderNumber = value;
                OnPropertyChanged();
            }
        }

    }
}

