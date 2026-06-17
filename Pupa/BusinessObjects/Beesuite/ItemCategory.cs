using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pupa.BusinessObjects.Beesuite
{
    [Table("ItemCategory")]
    public class ItemCategory : BaseEntity
    {
        public ItemCategory()
        {
            //_CategoryVesselItemRels = new ObservableCollection<CategoryVesselItemRel>();
        }

        #region Backing Fields
        private int _ID;
        private int? _ParentID;
        private string? _Code;
        private string? _Name;
        private string? _Description;
        private DateTime? _CreatedAt;
        private DateTime? _UpdatedAt;
        //private ObservableCollection<CategoryVesselItemRel> _CategoryVesselItemRels;
        #endregion

        #region Public Properties
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("ID")]
        public virtual int ID
        {
            get { return _ID; }
            set { OnPropertyChanging(); _ID = value; OnPropertyChanged(); }
        }

        [Column("ParentID")]
        public virtual int? ParentID
        {
            get { return _ParentID; }
            set { OnPropertyChanging(); _ParentID = value; OnPropertyChanged(); }
        }

        [Column("Code")]
        [StringLength(255)]
        public virtual string? Code
        {
            get { return _Code; }
            set { OnPropertyChanging(); _Code = value; OnPropertyChanged(); }
        }

        [Column("Name")]
        [StringLength(255)]
        public virtual string? Name
        {
            get { return _Name; }
            set { OnPropertyChanging(); _Name = value; OnPropertyChanged(); }
        }

        [Column("Description")]
        [StringLength(500)]
        public virtual string? Description
        {
            get { return _Description; }
            set { OnPropertyChanging(); _Description = value; OnPropertyChanged(); }
        }

        [Column("CreatedAt")]
        public virtual DateTime? CreatedAt
        {
            get { return _CreatedAt; }
            set { OnPropertyChanging(); _CreatedAt = value; OnPropertyChanged(); }
        }

        [Column("UpdatedAt")]
        public virtual DateTime? UpdatedAt
        {
            get { return _UpdatedAt; }
            set { OnPropertyChanging(); _UpdatedAt = value; OnPropertyChanged(); }
        }

        // Relasi One-to-Many
        //public virtual ObservableCollection<CategoryVesselItemRel> CategoryVesselItemRels
        //{
        //    get { return _CategoryVesselItemRels; }
        //    set { OnPropertyChanging(); _CategoryVesselItemRels = value; OnPropertyChanged(); }
        //}
        #endregion
        [ForeignKey("ParentID")]
        public virtual ItemCategory? Category { get; set; }
    }
}