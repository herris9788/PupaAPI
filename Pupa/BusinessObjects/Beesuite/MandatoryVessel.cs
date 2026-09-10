using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pupa.BusinessObjects.Beesuite
{
    /// <summary>
    /// Marks a vessel as "wajib" (mandatory) for Approval Rule V2: levels
    /// 1..MandatoryLevelCutoff must be approved by the specific named users in
    /// MandatoryVesselApprover; levels beyond the cutoff fall through to the
    /// generic Group-based lookup (UserApprovalGroup). Vessels not listed here
    /// use the Group-based lookup from level 1 onward.
    /// </summary>
    [Table("MandatoryVessel")]
    public class MandatoryVessel : BaseEntity
    {
        private int _id;
        [Key]
        public virtual int ID { get => _id; set { OnPropertyChanging(); _id = value; OnPropertyChanged(); } }

        private int _vesselID;
        public virtual int VesselID { get => _vesselID; set { OnPropertyChanging(); _vesselID = value; OnPropertyChanged(); } }

        private string _companyDB = string.Empty;
        public virtual string CompanyDB { get => _companyDB; set { OnPropertyChanging(); _companyDB = value; OnPropertyChanged(); } }

        private short _mandatoryLevelCutoff;
        public virtual short MandatoryLevelCutoff { get => _mandatoryLevelCutoff; set { OnPropertyChanging(); _mandatoryLevelCutoff = value; OnPropertyChanged(); } }

        private DateTime? _createdAt;
        public virtual DateTime? CreatedAt { get => _createdAt; set { OnPropertyChanging(); _createdAt = value; OnPropertyChanged(); } }
    }
}
