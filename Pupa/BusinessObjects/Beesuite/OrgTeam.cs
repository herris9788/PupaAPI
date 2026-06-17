using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pupa.BusinessObjects.Beesuite
{
    [Table("OrgTeam")]
    public class OrgTeam : BaseEntity
    {
        private int _id;
        private string _name = string.Empty;
        private string? _description;
        private bool? _isActive = true;
        private DateTime? _createdAt;
        public OrgTeam ()
        {
            Positions = new ObservableCollection<OrgPosition>();
            DelegationsFrom = new ObservableCollection<OrgDelegation>();
            DelegationsTo = new ObservableCollection<OrgDelegation>();

        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("ID")]
        public virtual int ID
        {
            get => _id;
            set { OnPropertyChanging(); _id = value; OnPropertyChanged(); }
        }

        [Column("Name")]
        [MaxLength(150)]
        [Required]
        public virtual string Name
        {
            get => _name;
            set { OnPropertyChanging(); _name = value; OnPropertyChanged(); }
        }

        [Column("Description")]
        [MaxLength(255)]
        public virtual string? Description
        {
            get => _description;
            set { OnPropertyChanging(); _description = value; OnPropertyChanged(); }
        }

        [Column("IsActive")]
        public virtual bool? IsActive
        {
            get => _isActive;
            set { OnPropertyChanging(); _isActive = value; OnPropertyChanged(); }
        }

        [Column("CreatedAt")]
        public virtual DateTime? CreatedAt
        {
            get => _createdAt;
            set { OnPropertyChanging(); _createdAt = value; OnPropertyChanged(); }
        }

        // ── Navigation ───────────────────────────────────────────────────────
        public virtual ObservableCollection<OrgPosition> Positions { get; set; }
        public virtual ObservableCollection<OrgDelegation> DelegationsFrom { get; set; }
        public virtual ObservableCollection<OrgDelegation> DelegationsTo { get; set; }
    }
}
