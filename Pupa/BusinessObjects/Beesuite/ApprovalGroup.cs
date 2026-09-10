using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pupa.BusinessObjects.Beesuite
{
    /// <summary>
    /// Master list of the business Group names used by the Approval Matrix
    /// (Approval Rule V2 prep). ItemGroupMapping and UserApprovalGroup both
    /// carry a GroupID foreign key into this table.
    /// </summary>
    [Table("ApprovalGroup")]
    public class ApprovalGroup : BaseEntity
    {
        private int _id;
        [Key]
        public virtual int ID { get => _id; set { OnPropertyChanging(); _id = value; OnPropertyChanged(); } }

        private string _groupName = string.Empty;
        public virtual string GroupName { get => _groupName; set { OnPropertyChanging(); _groupName = value; OnPropertyChanged(); } }

        private DateTime? _createdAt;
        public virtual DateTime? CreatedAt { get => _createdAt; set { OnPropertyChanging(); _createdAt = value; OnPropertyChanged(); } }
    }
}
