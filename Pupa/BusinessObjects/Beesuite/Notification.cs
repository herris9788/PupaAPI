using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;

namespace Pupa.BusinessObjects.Beesuite
{
    [Table("Notification")]
    public class Notification : BaseEntity
    {
        private int _id;
        [Key]
        public virtual int ID
        {
            get => _id;
            set { OnPropertyChanging(); _id = value; OnPropertyChanged(); }
        }
        private string? _topic;
        public virtual string? Topic
        {
            get => _topic;
            set { OnPropertyChanging(); _topic = value; OnPropertyChanged(); }
        }
        private string? _title;
        public virtual string? Title
        {
            get => _title;
            set { OnPropertyChanging(); _title = value; OnPropertyChanged(); }
        }
        private string? _Content;
        public virtual string? Content
        {
            get => _Content;
            set { OnPropertyChanging(); _Content = value; OnPropertyChanged(); }
        }
        private string? _Event;
        public virtual string? Event
        {
            get => _Event;
            set { OnPropertyChanging(); _Event = value; OnPropertyChanged(); }
        }
        private int? _DocumentID;
        public virtual int? DocumentID
        {
            get => _DocumentID;
            set { OnPropertyChanging(); _DocumentID = value; OnPropertyChanged(); }
        }
        private int? _AppID;
        public virtual int? AppID
        {
            get => _AppID;
            set { OnPropertyChanging(); _AppID = value; OnPropertyChanged(); }
        }
        private int? _ModuleID;
        public virtual int? ModuleID
        {
            get => _ModuleID;
            set { OnPropertyChanging(); _ModuleID = value; OnPropertyChanged(); }
        }
        private string? _Sender;
        public virtual string? Sender
        {
            get => _Sender;
            set { OnPropertyChanging(); _Sender = value; OnPropertyChanged(); }
        }
        private string? _SenderName;
        public virtual string? SenderName
        {
            get => _SenderName;
            set { OnPropertyChanging(); _SenderName = value; OnPropertyChanged(); }
        }
        private string? _Receiver;
        public virtual string? Receiver
        {
            get => _Receiver;
            set { OnPropertyChanging(); _Receiver = value; OnPropertyChanged(); }
        }
        private string? _ReceiverName;
        public virtual string? ReceiverName
        {
            get => _ReceiverName;
            set { OnPropertyChanging(); _ReceiverName = value; OnPropertyChanged(); }
        }
        private bool? _Read;
        public virtual bool? Read
        {
            get => _Read;
            set { OnPropertyChanging(); _Read = value; OnPropertyChanged(); }
        }
        private bool? _Public;
        public virtual bool? Public
        {
            get => _Public;
            set { OnPropertyChanging(); _Public = value; OnPropertyChanged(); }
        }
        private DateTime? _CreatedTime;
        public virtual DateTime? CreatedTime
        {
            get => _CreatedTime;
            set { OnPropertyChanging(); _CreatedTime = value; OnPropertyChanged(); }
        }
        private string? _DB;
        public virtual string? DB
        {
            get => _DB;
            set { OnPropertyChanging(); _DB = value; OnPropertyChanged(); }
        }
        public virtual ObservableCollection<NotificationRead> Reads { get; set; }

    }
}