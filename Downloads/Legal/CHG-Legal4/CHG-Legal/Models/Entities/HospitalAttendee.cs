using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CHG_Legal.Models.Entities
{
    [Table("Hospital_Attendees")]
    public class HospitalAttendee
    {
        [Key]
        [Column("ID")]
        public int ID { get; set; }

        [ForeignKey("Attendee")]
        [Column("Attendee_ID")]
        public int Attendee_ID { get; set; }

        [ForeignKey("Hospital")]
        [Column("Hospital_ID")]
        public int Hospital_ID { get; set; }

        // Navigation Properties
        public virtual Attendee? Attendee { get; set; }
        public virtual Hospital? Hospital { get; set; }
    }
}