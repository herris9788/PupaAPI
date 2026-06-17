using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pupa.BusinessObjects.Beesuite
{
    [Table("OrgPositionDepartment")]
    public class OrgPositionDepartment : BaseEntity
    {
        private int _id;
        private int _positionID;
        private int _departmentID;

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("ID")]
        public virtual int ID
        {
            get => _id;
            set { OnPropertyChanging(); _id = value; OnPropertyChanged(); }
        }

        [Column("PositionID")]
        [Required]
        public virtual int PositionID
        {
            get => _positionID;
            set { OnPropertyChanging(); _positionID = value; OnPropertyChanged(); }
        }

        [Column("DepartmentID")]
        [Required]
        public virtual int DepartmentID
        {
            get => _departmentID;
            set { OnPropertyChanging(); _departmentID = value; OnPropertyChanged(); }
        }

        // ── Navigation ───────────────────────────────────────────────────────
        [ForeignKey("PositionID")]
        public virtual OrgPosition? Position { get; set; }

        [ForeignKey("DepartmentID")]
        public virtual OrgDepartment? Department { get; set; }
    }
}
