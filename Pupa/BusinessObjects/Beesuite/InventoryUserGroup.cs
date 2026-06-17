using System;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pupa.BusinessObjects.Beesuite
{
    [Table("IC_InventoryUserGroup", Schema = "Ascend")]
    public class InventoryUserGroup : BaseEntity
    {
        public InventoryUserGroup() {
            this.InventoryUsers = new ObservableCollection<InventoryUser>();
            this.Approvals = new ObservableCollection<Approval>();
        }
        [Key]
        public virtual int ID { get; set; }
        private string? _DB { get; set; }
        public virtual string? DB
        {
            get => _DB;
            set { OnPropertyChanging(); _DB = value; OnPropertyChanged(); }
        }
        private int? _GroupID { get; set; }
        public virtual int? GroupID
        {
            get => _GroupID;
            set { OnPropertyChanging(); _GroupID = value; OnPropertyChanged(); }
        }
        private string? _GroupCode { get; set; }
        public virtual string? GroupCode
        {
            get => _GroupCode;
            set { OnPropertyChanging(); _GroupCode = value; OnPropertyChanged(); }
        }
        private string? _GroupName { get; set; }
        public virtual string? GroupName
        {
            get => _GroupName;
            set { OnPropertyChanging(); _GroupName = value; OnPropertyChanged(); }
        }
        public virtual ObservableCollection<InventoryUser>? InventoryUsers { get; set; }
        public virtual ObservableCollection<Approval>? Approvals { get; set; }
    }
}