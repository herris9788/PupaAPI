using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;

namespace Pupa.BusinessObjects.Beesuite
{
    [Table("VesselSpecRel")]
    public class VesselSpecRel : BaseEntity
    {

        [Key]
        public virtual int ID
        {
            get;set;
        }
        private int? _SpecificationID;
        public virtual int? SpecificationID
        {
            get => _SpecificationID;
            set { OnPropertyChanging(); _SpecificationID = value; OnPropertyChanged(); }
        }
        private int? _VesselID;
        public virtual int? VesselID
        {
            get => _VesselID;
            set { OnPropertyChanging(); _VesselID = value; OnPropertyChanged(); }
        }
        [ForeignKey("SpecificationID")]
        public virtual Specification? Specification { get; set; }
        //[ForeignKey("VesselID")]
        //public virtual Vessel? Vessel { get; set; }
    }
}