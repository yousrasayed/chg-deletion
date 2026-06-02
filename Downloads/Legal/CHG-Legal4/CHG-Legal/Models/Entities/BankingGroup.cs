using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CHG_Legal.Models.Entities
{
    [Table("BankingGroups", Schema = "Company")]
    public class BankingGroup
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int GroupId { get; set; }

        public int? CompanyId { get; set; }

        [MaxLength(200)]
        public string? GroupName { get; set; }

        [ForeignKey("CompanyId")]
        public virtual Company? Company { get; set; }

        public virtual ICollection<BankingGroupMember> BankingGroupMembers { get; set; } = new List<BankingGroupMember>();
        // Models/Entities/BankingGroup.cs - أضف هذه الخاصية
        public virtual ICollection<BankingAttachment> BankingAttachments { get; set; } = new List<BankingAttachment>();
    }
}