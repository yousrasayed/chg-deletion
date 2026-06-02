using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CHG_Legal.Models.Entities
{
    [Table("AssociationPlace")]
    public class AssociationPlace
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        [MaxLength(150)]
        public string? AsscoiationPlace { get; set; }

        public int? associationID { get; set; }

        [MaxLength(350)]
        public string? associationPlaceDecr { get; set; }

        [ForeignKey("associationID")]
        public virtual Association? Association { get; set; }
    }
}