using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;

namespace Pupa.BusinessObjects.Beesuite
{
    [Table("InventoryUserSpecs", Schema = "Ascend")]
    public class InventoryUserSpec : BaseEntity
    {
        public InventoryUserSpec()
        {
        }
        private int? _InventoryUserID;
        private string? _Type;
        private string? _Specification;
        [Key]
        [Column("ID")]
        public virtual int ID
        {
            get; set;
        }
        [Column("InventoryUserID")]
        public virtual int? InventoryUserID
        {
            get => _InventoryUserID;
            set { OnPropertyChanging(); _InventoryUserID = value; OnPropertyChanged(); }
        }
        [Column("Specification")]
        public virtual string? Specification
        {
            get => _Specification;
            set { OnPropertyChanging(); _Specification = value; OnPropertyChanged(); }
        }
        [Column("Type")]
        public virtual string? Type
        {
            get => _Type;
            set { OnPropertyChanging(); _Type = value; OnPropertyChanged(); }
        }

        [ForeignKey("InventoryUserID")]
        public virtual InventoryUser? InventoryUser { get; set; }
    }
}