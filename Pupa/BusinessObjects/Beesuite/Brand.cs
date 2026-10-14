using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pupa.BusinessObjects.Beesuite
{
    /// <summary>Item brands. Maps to the existing table "Ascend"."IC_Brands" (no schema change).</summary>
    [Table("IC_Brands", Schema = "Ascend")]
    public class Brand : BaseEntity
    {
        private int _BrandID;
        private string? _BrandName;
        private string _CoState = "New";
        private Guid _CoUID = Guid.NewGuid();
        private Guid? _CoPkgID;
        private Guid? _CoSourcePkgID;
        private string _BrandCode = string.Empty;
        private string _BFlags = string.Empty;
        private int? _GroupID;

        [Key]
        [Column("BrandID")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public virtual int BrandID { get { return _BrandID; } set { OnPropertyChanging(); _BrandID = value; OnPropertyChanged(); } }

        [StringLength(50)]
        [Column("BrandName")]
        public virtual string? BrandName { get { return _BrandName; } set { OnPropertyChanging(); _BrandName = value; OnPropertyChanged(); } }

        [Required]
        [StringLength(10)]
        [Column("CoState")]
        public virtual string CoState { get { return _CoState; } set { OnPropertyChanging(); _CoState = value; OnPropertyChanged(); } }

        [Column("CoUID")]
        public virtual Guid CoUID { get { return _CoUID; } set { OnPropertyChanging(); _CoUID = value; OnPropertyChanged(); } }

        [Column("CoPkgID")]
        public virtual Guid? CoPkgID { get { return _CoPkgID; } set { OnPropertyChanging(); _CoPkgID = value; OnPropertyChanged(); } }

        [Column("CoSourcePkgID")]
        public virtual Guid? CoSourcePkgID { get { return _CoSourcePkgID; } set { OnPropertyChanging(); _CoSourcePkgID = value; OnPropertyChanged(); } }

        [Required]
        [StringLength(10)]
        [Column("BrandCode")]
        public virtual string BrandCode { get { return _BrandCode; } set { OnPropertyChanging(); _BrandCode = value; OnPropertyChanged(); } }

        [Required]
        [StringLength(100)]
        [Column("BFlags")]
        public virtual string BFlags { get { return _BFlags; } set { OnPropertyChanging(); _BFlags = value; OnPropertyChanged(); } }

        [Column("GroupID")]
        public virtual int? GroupID { get { return _GroupID; } set { OnPropertyChanging(); _GroupID = value; OnPropertyChanged(); } }
    }
}
