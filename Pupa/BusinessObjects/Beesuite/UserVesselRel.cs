using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;

namespace Pupa.BusinessObjects.Beesuite
{
    [Table("UserVesselRel")]
    public class UserVesselRel : BaseEntity
    {
        // Fields privat untuk menyimpan data
        private int _id;
      
        public virtual int ID
        {
            get => _id;
            set { OnPropertyChanging(); _id = value; OnPropertyChanged(); }
        }

        private int _vesselID;

        public virtual int VesselID
        {
            get => _vesselID;
            set { OnPropertyChanging(); _vesselID = value; OnPropertyChanged(); }
        }

        private int _userID;

        public virtual int UserID
        {
            get => _userID;
            set { OnPropertyChanging(); _userID = value; OnPropertyChanged(); }
        }
        [ForeignKey(nameof(UserID))]
        public virtual User? User { get; set; }
        [ForeignKey(nameof(VesselID))]
        public virtual InventoryUser? Vessel { get; set; }
    }
}