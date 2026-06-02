using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CHG_Legal.Models.Entities
{
    [Table("BankingGroupMembers", Schema = "Company")]
    public class BankingGroupMember
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int? GroupId { get; set; }

        [MaxLength(200)]
        public string? MemberName { get; set; }

        [MaxLength(350)]
        public string? Note { get; set; }

        [ForeignKey("GroupId")]
        public virtual BankingGroup? BankingGroup { get; set; }
    }
}