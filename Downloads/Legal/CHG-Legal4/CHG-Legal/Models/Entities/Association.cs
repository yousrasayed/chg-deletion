using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CHG_Legal.Models.Entities
{
    [Table("Association")]
    public class Association
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        [MaxLength(150)]
        public string? AssociationType { get; set; }

        [MaxLength(150)]
        public string? VotingMechanism { get; set; }

        public double? ValidityValue { get; set; }  

        public virtual ICollection<AssociationPlace> AssociationPlaces { get; set; } = new List<AssociationPlace>();
    }
}