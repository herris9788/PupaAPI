using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;

namespace Pupa.BusinessObjects.Beesuite
{
    [Table("WhatsappDeviceGroup")]
    public class WhatsappDeviceGroup : BaseEntity
    {

        [Key]
        public virtual int ID
        {
            get;set;
        }
        private string? _Name;
        public virtual string? Name
        {
            get => _Name;
            set { OnPropertyChanging(); _Name = value; OnPropertyChanged(); }
        }
        private int? _WhatsappDeviceID;
        public virtual int? WhatsappDeviceID
        {
            get => _WhatsappDeviceID;
            set { OnPropertyChanging(); _WhatsappDeviceID = value; OnPropertyChanged(); }
        }
        private string? _GroupID;
        public virtual string? GroupID
        {
            get => _GroupID;
            set { OnPropertyChanging(); _GroupID = value; OnPropertyChanged(); }
        }
        [ForeignKey("WhatsappDeviceID")]
        public virtual WhatsappDevice? WhatsappDevice { get; set; }
    }
}