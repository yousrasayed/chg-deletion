using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CHG_Legal.Models.Entities
{
    [Table("BoardSettings", Schema = "Company")]
    public class BoardSetting
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int BoardId { get; set; }

        public int? CompanyId { get; set; }

        [MaxLength(100)]
        public string? Duration { get; set; }

        public DateTime? StartDate { get; set; }

        public int? NotificationPeriod { get; set; }

        [ForeignKey("CompanyId")]
        public virtual Company? Company { get; set; }

        public virtual ICollection<BoardMember> BoardMembers { get; set; } = new List<BoardMember>();
   
        public virtual ICollection<BoardMemberAttach> BoardMemberAttachments { get; set; } = new List<BoardMemberAttach>();
    }
}