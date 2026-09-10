using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pupa.BusinessObjects.Beesuite
{
    [Table("UserApprovalGroup")]
    public class UserApprovalGroup : BaseEntity
    {
        private int _id;
        [Key]
        public virtual int ID { get => _id; set { OnPropertyChanging(); _id = value; OnPropertyChanged(); } }

        private string _username = string.Empty;
        public virtual string Username { get => _username; set { OnPropertyChanging(); _username = value; OnPropertyChanged(); } }

        private string _groupName = string.Empty;
        public virtual string GroupName { get => _groupName; set { OnPropertyChanging(); _groupName = value; OnPropertyChanged(); } }

        private DateTime? _createdAt;
        public virtual DateTime? CreatedAt { get => _createdAt; set { OnPropertyChanging(); _createdAt = value; OnPropertyChanged(); } }
    }
}
