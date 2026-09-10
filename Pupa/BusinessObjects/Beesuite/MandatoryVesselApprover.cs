using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pupa.BusinessObjects.Beesuite
{
    /// <summary>
    /// The specific user(s) required to approve at a given Level (1..cutoff)
    /// for a MandatoryVessel. More than one Username can share the same
    /// (VesselID, CompanyDB, Level) -- same shape as UserApprovalScope.
    /// </summary>
    [Table("MandatoryVesselApprover")]
    public class MandatoryVesselApprover : BaseEntity
    {
        private int _id;
        [Key]
        public virtual int ID { get => _id; set { OnPropertyChanging(); _id = value; OnPropertyChanged(); } }

        private int _vesselID;
        public virtual int VesselID { get => _vesselID; set { OnPropertyChanging(); _vesselID = value; OnPropertyChanged(); } }

        private string _companyDB = string.Empty;
        public virtual string CompanyDB { get => _companyDB; set { OnPropertyChanging(); _companyDB = value; OnPropertyChanged(); } }

        private short _level;
        public virtual short Level { get => _level; set { OnPropertyChanging(); _level = value; OnPropertyChanged(); } }

        private string _username = string.Empty;
        public virtual string Username { get => _username; set { OnPropertyChanging(); _username = value; OnPropertyChanged(); } }

        private DateTime? _createdAt;
        public virtual DateTime? CreatedAt { get => _createdAt; set { OnPropertyChanging(); _createdAt = value; OnPropertyChanged(); } }
    }
}
