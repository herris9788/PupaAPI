using System;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pupa.BusinessObjects.Beesuite
{
    [Table("ServiceCategory", Schema = "Ascend")]
    public class ServiceCategory : BaseEntity
    {
        public ServiceCategory()
        {
           
        }

        private int? _I;
        private string? _Name;
        private string? _Description;
        private int? _FamilyID;
        

        [Key]
        public virtual int ID { get; set; }
        public virtual int? FamilyID { get { return _FamilyID; } set { OnPropertyChanging(); _FamilyID = value; OnPropertyChanged(); } }
        public virtual string? Name { get { return _Name; } set { OnPropertyChanging(); _Name = value; OnPropertyChanged(); } }
        public virtual string? Description { get { return _Description; } set { OnPropertyChanging(); _Description = value; OnPropertyChanged(); } }
        [ForeignKey("FamilyID")]
        public virtual StockFamily? StockFamily { get; set; }


    }
}