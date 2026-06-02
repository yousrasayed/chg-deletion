using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CHG_Legal.Models.Entities
{
    [Table("Hospitals")]
    public class Hospital
    {
        [Key]
        [Column("ID")]
        public int ID { get; set; }

        [Required]
        [StringLength(100)]
        [Column("Hospital")]
        public string HospitalName { get; set; } = string.Empty;

        // Navigation Properties
        public virtual ICollection<HospitalAttendee> HospitalAttendees { get; set; } = new List<HospitalAttendee>();
    }
}