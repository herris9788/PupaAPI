
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Net.Mail;

namespace Pupa.BusinessObjects.Beesuite
{
    [Table("RequisitionAttachmentRel")]
    public class RequisitionAttachmentRel : BaseEntity
    {
        public RequisitionAttachmentRel()
        {
        }

        [Key]
        public virtual int ID
        {
            get; set;
        }
        private int? _RequisitionID { get; set; }
        private int? _AttachmentID { get; set; }
        public virtual int? RequisitionID
        {
            get => _RequisitionID;
            set
            {
                if (_RequisitionID == value) return;
                OnPropertyChanging();
                _RequisitionID = value;
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
        [ForeignKey("RequisitionID")]
        public virtual Requisition? Requisition { get; set; }
        [ForeignKey("AttachmentID")]
        public virtual Attachment? Attachment { get; set; }


    }
}