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

        // FK -> ApprovalGroup.ID (the real relational link; GroupName above is
        // kept as a denormalized display copy).
        private int _groupID;
        public virtual int GroupID { get => _groupID; set { OnPropertyChanging(); _groupID = value; OnPropertyChanged(); } }

        // Group-relative approval level, 1-based. The resolver offsets this by
        // the vessel's MandatoryLevelCutoff to get the real approval-chain
        // position: for a mandatory vessel with cutoff C, this Level L is the
        // real position C + L; for a non-mandatory vessel the Group tier
        // starts at position 1, so Level L is position L directly. The real
        // position's stored/matched label still shifts 7 -> 8 (see
        // GetRequisitionApprovedByName / V2LevelLabel).
        private short _level = 1;
        public virtual short Level { get => _level; set { OnPropertyChanging(); _level = value; OnPropertyChanged(); } }

        private DateTime? _createdAt;
        public virtual DateTime? CreatedAt { get => _createdAt; set { OnPropertyChanging(); _createdAt = value; OnPropertyChanged(); } }
    }
}
