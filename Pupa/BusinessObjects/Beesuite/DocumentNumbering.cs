using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pupa.BusinessObjects.Beesuite
{
    [Table("DocumentNumbering", Schema = "Ascend")]
    public class DocumentNumbering
    {
        [Key]
        public virtual int ID { get; set; }
        public virtual string? Vessel { get; set; }
        public virtual string? VesselCode { get; set; }
        public virtual string? Format { get; set; }
        public virtual string? Type { get; set; }
    }
}
