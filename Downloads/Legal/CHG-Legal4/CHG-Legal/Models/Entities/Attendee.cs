using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace CHG_Legal.Models.Entities
{
    [Table("Attendees")]
    public class Attendee
    {
        [Key]
        [Column("ID")]
        public int ID { get; set; }

        [Required]
        [StringLength(100)]
        [Column("Attendee")]
        public string Name { get; set; } = string.Empty;

        [StringLength(400)]
        [Column("Job_Describtion")]
        public string? JobDescription { get; set; }

        // Navigation Properties
        [JsonIgnore]
        public virtual ICollection<HospitalAttendee> HospitalAttendees { get; set; } = new List<HospitalAttendee>();

        [JsonIgnore]
        public virtual ICollection<BoardAttendee> BoardAttendees { get; set; } = new List<BoardAttendee>();

        [JsonIgnore]
        public virtual ICollection<Approval> Approvals { get; set; } = new List<Approval>();
    }
}