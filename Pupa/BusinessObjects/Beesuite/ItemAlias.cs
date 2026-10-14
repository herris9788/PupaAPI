using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pupa.BusinessObjects.Beesuite
{
    /// <summary>
    /// Item alias / barcode codes. Maps to the existing table "Ascend"."IC_Aliases" (no schema change). Named ItemAlias to match ItemAliasName (IC_AliasNames).
    /// </summary>
    [Table("IC_Aliases", Schema = "Ascend")]
    public class ItemAlias : BaseEntity
    {
        private int _AliasID;
        private int _ItemID;
        private string _AliasCode = string.Empty;
        private bool _IsPrimaryCode = true;
        private string _AliasType = string.Empty;
        private string? _CreatedBy;
        private DateTime? _CreateDate;
        private string? _LastModBy;
        private DateTime? _LastMod;
        private string _CoState = "New";
        private Guid _CoUID;
        private Guid? _CoPkgID;
        private Guid? _CoSourcePkgID;
        private string _Description = string.Empty;
        private short _UOMLevel;

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Column("AliasID")]
        public virtual int AliasID { get { return _AliasID; } set { OnPropertyChanging(); _AliasID = value; OnPropertyChanged(); } }

        [Column("ItemID")]
        public virtual int ItemID { get { return _ItemID; } set { OnPropertyChanging(); _ItemID = value; OnPropertyChanged(); } }

        [Required]
        [StringLength(100)]
        [Column("AliasCode")]
        public virtual string AliasCode { get { return _AliasCode; } set { OnPropertyChanging(); _AliasCode = value; OnPropertyChanged(); } }

        [Column("IsPrimaryCode")]
        public virtual bool IsPrimaryCode { get { return _IsPrimaryCode; } set { OnPropertyChanging(); _IsPrimaryCode = value; OnPropertyChanged(); } }

        [Required]
        [StringLength(30)]
        [Column("AliasType")]
        public virtual string AliasType { get { return _AliasType; } set { OnPropertyChanging(); _AliasType = value; OnPropertyChanged(); } }

        [StringLength(30)]
        [Column("CreatedBy")]
        public virtual string? CreatedBy { get { return _CreatedBy; } set { OnPropertyChanging(); _CreatedBy = value; OnPropertyChanged(); } }

        [Column("CreateDate")]
        public virtual DateTime? CreateDate { get { return _CreateDate; } set { OnPropertyChanging(); _CreateDate = value; OnPropertyChanged(); } }

        [StringLength(30)]
        [Column("LastModBy")]
        public virtual string? LastModBy { get { return _LastModBy; } set { OnPropertyChanging(); _LastModBy = value; OnPropertyChanged(); } }

        [Column("LastMod")]
        public virtual DateTime? LastMod { get { return _LastMod; } set { OnPropertyChanging(); _LastMod = value; OnPropertyChanged(); } }

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
        [StringLength(100)]
        [Column("Description")]
        public virtual string Description { get { return _Description; } set { OnPropertyChanging(); _Description = value; OnPropertyChanged(); } }

        [Column("UOMLevel")]
        public virtual short UOMLevel { get { return _UOMLevel; } set { OnPropertyChanging(); _UOMLevel = value; OnPropertyChanged(); } }
    }
}
