using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CHG_Legal.Models.Entities
{
    [Table("BoardDecisions")]
    public class BoardDecision
    {
        [Key]
        [Column("ID")]
        public int ID { get; set; }

        [ForeignKey("Board")]
        [Column("Board_ID")]
        public int Board_ID { get; set; }

        [Column("Decesion_Number")]
        public short Decision_Number { get; set; }

        [Required]
        [StringLength(700)]
        [Column("Decesion_Details")]
        public string Decision_Details { get; set; } = string.Empty;

        [StringLength(10)]
        [Column("IsExcuted")]
        public string? IsExecuted { get; set; } = "False";

        [StringLength(700)]
        [Column("Notes")]
        public string? Notes { get; set; }

        [Column("DateInserted")]
        public DateTime DateInserted { get; set; } = DateTime.Now;

        // Navigation Properties
        public virtual Board? Board { get; set; }
    }
}