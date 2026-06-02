using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace CHG_Legal.Models.Entities
{
    [Table("Board")]
    public class Board
    {
        [Key]
        [Column("ID")]
        public int ID { get; set; }

        [Required]
        [Column("BoardDate")]
        public DateTime BoardDate { get; set; }

        [StringLength(500)]
        [Column("Subject")]
        public string? Subject { get; set; }

        [StringLength(700)]
        [Column("Attendees")]
        public string? Attendees { get; set; }

        [StringLength(700)]
        [Column("BoardDescsions")]
        public string? BoardDecisionsText { get; set; }

        [ForeignKey("User")]
        [Column("UserID")]
        public int UserID { get; set; }

        [StringLength(100)]
        [Column("CHGParty")]
        public string? CHGParty { get; set; }

        [StringLength(500)]
        [Column("Notes")]
        public string? Notes { get; set; }

        [StringLength(100)]
        [Column("MeetingType")]
        public string? MeetingType { get; set; }

        [StringLength(100)]
        [Column("MeetingStatus")]
        public string? MeetingStatus { get; set; }

        [ForeignKey("BoardType")]
        [Column("BoardTypeID")]
        public int BoardTypeID { get; set; }

       
        public virtual User? User { get; set; }
        public virtual BoardType? BoardType { get; set; }

        [JsonIgnore]
        public virtual ICollection<BoardAttendee> BoardAttendees { get; set; } = new List<BoardAttendee>();

        [JsonIgnore]
        public virtual ICollection<Approval> Approvals { get; set; } = new List<Approval>();

        [JsonIgnore]
        public virtual ICollection<BoardAttachment> BoardAttachments { get; set; } = new List<BoardAttachment>();

        [JsonIgnore]
        public virtual ICollection<BoardDecision> BoardDecisions { get; set; } = new List<BoardDecision>();
    }
}