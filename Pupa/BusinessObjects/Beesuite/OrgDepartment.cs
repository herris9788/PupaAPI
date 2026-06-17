using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pupa.BusinessObjects.Beesuite
{
    [Table("OrgDepartment")]
    public class OrgDepartment : BaseEntity
    {
        private int _id;
        private string _name = string.Empty;
        private string? _code;
        private string? _description;
        private int? _parentID;
        private bool? _isActive = true;
        private DateTime? _createdAt;

        public OrgDepartment()
        {
            Children = new ObservableCollection<OrgDepartment>();
            PositionDepartments = new ObservableCollection<OrgPositionDepartment>();
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

        [Column("Code")]
        [MaxLength(50)]
        public virtual string? Code
        {
            get => _code;
            set { OnPropertyChanging(); _code = value; OnPropertyChanged(); }
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

        [Column("ParentID")]
        public virtual int? ParentID
        {
            get => _parentID;
            set { OnPropertyChanging(); _parentID = value; OnPropertyChanged(); }
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
        [ForeignKey("ParentID")]
        public virtual OrgDepartment? Parent { get; set; }

        public virtual ObservableCollection<OrgDepartment> Children { get; set; }
        public virtual ObservableCollection<OrgPositionDepartment> PositionDepartments { get; set; }
        public virtual ObservableCollection<OrgDelegation> DelegationsFrom { get; set; }
        public virtual ObservableCollection<OrgDelegation> DelegationsTo { get; set; }
    }
}
