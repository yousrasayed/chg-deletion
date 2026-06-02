using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CHG_Legal.Models.Entities
{
    [Table("BoardAttendees")]
    public class BoardAttendee
    {
        [Key]
        [Column("ID")]
        public int ID { get; set; }

        [StringLength(1000)]
        [Column("ApprovedBy")]
        public string? ApprovedBy { get; set; }

        [ForeignKey("Board")]
        [Column("BoardID")]
        public int BoardID { get; set; }

        [ForeignKey("Attendee")]
        [Column("Attendee_ID")]
        public int Attendee_ID { get; set; }

        // Navigation Properties
        public virtual Board? Board { get; set; }
        public virtual Attendee? Attendee { get; set; }
    }
}