using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CHG_Legal.Models.Entities
{
    [Table("BoardMembers", Schema = "Company")]
    public class BoardMember
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MemberId { get; set; }

        public int? BoardId { get; set; }

        [MaxLength(200)]
        public string? Name { get; set; }

        [MaxLength(200)]
        public string? Position { get; set; }

        [ForeignKey("BoardId")]
        public virtual BoardSetting? BoardSetting { get; set; }
    }
}