using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;

namespace Pupa.BusinessObjects.Beesuite
{
    [Table("WhatsappDevice")]
    public class WhatsappDevice : BaseEntity
    {
        public WhatsappDevice() {
            this.WhatsappDeviceGroups = new ObservableCollection<WhatsappDeviceGroup>();
        }
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
        private string? _Number;
        public virtual string? Number
        {
            get => _Number;
            set { OnPropertyChanging(); _Number = value; OnPropertyChanged(); }
        }
        private string? _Token;
        public virtual string? Token
        {
            get => _Token;
            set { OnPropertyChanging(); _Token = value; OnPropertyChanged(); }
        }
        private string? _Domain;
        public virtual string? Domain
        {
            get => _Domain;
            set { OnPropertyChanging(); _Domain = value; OnPropertyChanged(); }
        }
        private string? _Status;
        public virtual string? Status
        {
            get => _Status;
            set { OnPropertyChanging(); _Status = value; OnPropertyChanged(); }
        }
        public virtual ObservableCollection<WhatsappDeviceGroup> WhatsappDeviceGroups { get; set; }
    }
}