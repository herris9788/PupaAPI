
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Net.Mail;

namespace Pupa.BusinessObjects.Beesuite
{
    [Table("RequisitionDetailAttachmentRel")]
    public class RequisitionDetailAttachmentRel : BaseEntity
    {
        public RequisitionDetailAttachmentRel()
        {
        }

        [Key]
        public virtual int ID
        {
            get; set;
        }
        private int? _RequisitionDetailID { get; set; }
        private int? _AttachmentID { get; set; }
        public virtual int? RequisitionDetailID
        {
            get => _RequisitionDetailID;
            set
            {
                if (_RequisitionDetailID == value) return;
                OnPropertyChanging();
                _RequisitionDetailID = value;
                OnPropertyChanged();
            }
        }
        public virtual int? AttachmentID
        {
            get => _AttachmentID;
            set
            {
                if (_AttachmentID == value) return;
                OnPropertyChanging();
                _AttachmentID = value;
                OnPropertyChanged();
            }
        }

        private string? _Type { get; set; }
        public virtual string? Type
        {
            get => _Type;
            set
            {
                if (_Type == value) return;
                OnPropertyChanging();
                _Type = value;
                OnPropertyChanged();
            }
        }
        [ForeignKey("RequisitionDetailID")]
        public virtual RequisitionDetail? RequisitionRequisitionDetail { get; set; }
        [ForeignKey("AttachmentID")]
        public virtual Attachment? Attachment { get; set; }


    }
}