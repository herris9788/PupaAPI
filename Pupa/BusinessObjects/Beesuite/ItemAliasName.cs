using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pupa.BusinessObjects.Beesuite
{
    /// <summary>
    /// Alias names of an inventory item. Maps to the existing table "Ascend"."IC_AliasNames"
    /// (no schema change). Named ItemAliasName because a member cannot share its type's name.
    /// </summary>
    [Table("IC_AliasNames", Schema = "Ascend")]
    public class ItemAliasName : BaseEntity
    {
        private int _NameID;
        private int _ItemID;
        private string _AliasName = string.Empty;
        private string _LastModBy = string.Empty;
        private DateTime _LastMod;

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Column("NameID")]
        public virtual int NameID { get { return _NameID; } set { OnPropertyChanging(); _NameID = value; OnPropertyChanged(); } }

        [Column("ItemID")]
        public virtual int ItemID { get { return _ItemID; } set { OnPropertyChanging(); _ItemID = value; OnPropertyChanged(); } }

        [Required]
        [StringLength(500)]
        [Column("AliasName")]
        public virtual string AliasName { get { return _AliasName; } set { OnPropertyChanging(); _AliasName = value; OnPropertyChanged(); } }

        [Required]
        [StringLength(30)]
        [Column("LastModBy")]
        public virtual string LastModBy { get { return _LastModBy; } set { OnPropertyChanging(); _LastModBy = value; OnPropertyChanged(); } }

        [Column("LastMod")]
        public virtual DateTime LastMod { get { return _LastMod; } set { OnPropertyChanging(); _LastMod = value; OnPropertyChanged(); } }
    }
}
