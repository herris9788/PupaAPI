using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pupa.BusinessObjects.Beesuite
{
    [Table("Partbook", Schema = "Ascend")]
    public class PartBook : BaseEntity
    {
        public PartBook()
        {
        }

        #region Private Fields
        // Field bawaan yang sudah ada sebelumnya
        private int? _VesselID;
        private string? _VesselName;
        private string? _BookName;
        private string? _FilePath;
        private decimal? _Filesize;
       
        #endregion

        #region Public Properties
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public virtual int ID { get; set; }

        public virtual int? VesselID { get { return _VesselID; } set { OnPropertyChanging(); _VesselID = value; OnPropertyChanged(); } }
        public virtual string? VesselName { get { return _VesselName; } set { OnPropertyChanging(); _VesselName = value; OnPropertyChanged(); } }
        public virtual string? BookName { get { return _BookName; } set { OnPropertyChanging(); _BookName = value; OnPropertyChanged(); } }
        public virtual string? FilePath { get { return _FilePath; } set { OnPropertyChanging(); _FilePath = value; OnPropertyChanged(); } }
        public virtual decimal? Filesize { get { return _Filesize; } set { OnPropertyChanging(); _Filesize = value; OnPropertyChanged(); } }

        #endregion
    }
}