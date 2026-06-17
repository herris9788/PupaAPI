using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pupa.BusinessObjects.Beesuite
{
    [Table("OrgLevel")]
    public class OrgLevel : BaseEntity
    {
        private int _id;
        private string _name = string.Empty;
        private int _rank;
        private bool? _isActive = true;
        public OrgLevel() {
            Positions = new ObservableCollection<OrgPosition>();
        }
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("ID")]
        public virtual int ID
        {
            get => _id;
            set { OnPropertyChanging(); _id = value; OnPropertyChanged(); }
        }

        /// <summary>Nama jabatan (Director, Manager, SPV, dll)</summary>
        [Column("Name")]
        [MaxLength(100)]
        [Required]
        public virtual string Name
        {
            get => _name;
            set { OnPropertyChanging(); _name = value; OnPropertyChanged(); }
        }

        /// <summary>Makin kecil = makin tinggi (Director=1 ... Staff=7)</summary>
        [Column("Rank")]
        [Required]
        public virtual int Rank
        {
            get => _rank;
            set { OnPropertyChanging(); _rank = value; OnPropertyChanged(); }
        }

        [Column("IsActive")]
        public virtual bool? IsActive
        {
            get => _isActive;
            set { OnPropertyChanging(); _isActive = value; OnPropertyChanged(); }
        }

        // ── Navigation ───────────────────────────────────────────────────────
        public virtual ObservableCollection<OrgPosition> Positions { get; set; }
    }
}