using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pupa.BusinessObjects.Beesuite
{
    [Table("IC_UOM", Schema = "Ascend")]
    public class UOM
    {
        [Key]
        public virtual int UOMID { get; set; }
        public virtual string? UOMCode { get; set; }
        public virtual int? Decimals { get; set; }

        [NotMapped]
        public virtual int UOMID_1 { get; set; }
        [NotMapped]
        public virtual string? UOMCode_1 { get; set; }
        [NotMapped]
        public virtual int UOMID_2 { get; set; }
        [NotMapped]
        public virtual string? UOMCode_2 { get; set; }


    }
}
