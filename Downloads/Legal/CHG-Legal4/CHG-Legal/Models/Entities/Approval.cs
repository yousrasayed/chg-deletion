using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CHG_Legal.Models.Entities
{
    [Table("Approves")]
    public class Approval
    {
        [Key]
        [Column("ApprovalID")]
        public int ApprovalID { get; set; }

        [StringLength(1000)]
        [Column("ApprovedBy")]
        public string? ApprovedBy { get; set; }

        [ForeignKey("Board")]
        [Column("BoardApprovalID")]
        public int BoardApprovalID { get; set; }

        [ForeignKey("Attendee")]
        [Column("Attendee_ID")]
        public int Attendee_ID { get; set; }

        // Navigation Properties
        public virtual Board? Board { get; set; }
        public virtual Attendee? Attendee { get; set; }
    }
}