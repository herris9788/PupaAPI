using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;

namespace Pupa.BusinessObjects.Beesuite
{
    [Table("NotificationRead")]
    public class NotificationRead : BaseEntity
    {
        private int _id;
        [Key]
        public virtual int ID
        {
            get => _id;
            set { OnPropertyChanging(); _id = value; OnPropertyChanged(); }
        }
        private string? _userName;
        public virtual string? UserName
        {
            get => _userName;
            set { OnPropertyChanging(); _userName = value; OnPropertyChanged(); }
        }
        private int? _NotificationID;
        public virtual int? NotificationID
        {
            get => _NotificationID;
            set { OnPropertyChanging(); _NotificationID = value; OnPropertyChanged(); }
        }
        public virtual Notification? Notification { get; set;  }
    }
}