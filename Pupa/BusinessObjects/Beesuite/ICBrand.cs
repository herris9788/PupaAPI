using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pupa.BusinessObjects.Beesuite
{
    /// <summary>
    /// Item brands synced from every branch database's dbo.IC_Brand (HS Cron: syncBrands).
    /// One row per (DB, BrandID); "ID" is the surrogate key. Maps to the existing table
    /// "Ascend"."IC_Brand" (no schema change). Not to be confused with <see cref="Brand"/> ("IC_Brands").
    /// </summary>
    [Table("IC_Brand", Schema = "Ascend")]
    public class ICBrand : BaseEntity
    {
        private int _ID;
        private string _DB = string.Empty;
        private int _BrandID;
        private string? _BrandCode;
        private string? _BrandName;
        private string? _Description;
        private bool? _Disabled;
        private string? _CreatedBy;
        private DateTime? _CreateDate;
        private string? _LastModBy;
        private DateTime? _LastMod;

        [Key]
        [Column("ID")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public virtual int ID { get { return _ID; } set { OnPropertyChanging(); _ID = value; OnPropertyChanged(); } }

        /// <summary>Source branch database, e.g. "AS_MJS".</summary>
        [Required]
        [StringLength(10)]
        [Column("DB")]
        public virtual string DB { get { return _DB; } set { OnPropertyChanging(); _DB = value; OnPropertyChanged(); } }

        [Column("BrandID")]
        public virtual int BrandID { get { return _BrandID; } set { OnPropertyChanging(); _BrandID = value; OnPropertyChanged(); } }

        [StringLength(20)]
        [Column("BrandCode")]
        public virtual string? BrandCode { get { return _BrandCode; } set { OnPropertyChanging(); _BrandCode = value; OnPropertyChanged(); } }

        [StringLength(100)]
        [Column("BrandName")]
        public virtual string? BrandName { get { return _BrandName; } set { OnPropertyChanging(); _BrandName = value; OnPropertyChanged(); } }

        [StringLength(255)]
        [Column("Description")]
        public virtual string? Description { get { return _Description; } set { OnPropertyChanging(); _Description = value; OnPropertyChanged(); } }

        [Column("Disabled")]
        public virtual bool? Disabled { get { return _Disabled; } set { OnPropertyChanging(); _Disabled = value; OnPropertyChanged(); } }

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
    }
}
