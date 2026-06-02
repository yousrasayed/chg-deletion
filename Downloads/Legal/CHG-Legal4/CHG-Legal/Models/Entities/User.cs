using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace CHG_Legal.Models.Entities
{
    [Table("Users")]
    public class User
    {
        [Key]
        [Column("User_ID")]
        public int User_ID { get; set; }

        [Required]
        [StringLength(70)]
        [Column("User_Name")]
        public string User_Name { get; set; } = string.Empty;

        [Required]
        [StringLength(70)]
        [Column("Password")]
        public string Password { get; set; } = string.Empty;

        [Column("Active")]
        public bool Active { get; set; } = true;

        // Navigation Properties - Add JsonIgnore to break cycles
        [JsonIgnore]
        public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();

        [JsonIgnore]
        public virtual ICollection<Board> Boards { get; set; } = new List<Board>();

        [JsonIgnore]
        public virtual ICollection<BoardAttachment> BoardAttachments { get; set; } = new List<BoardAttachment>();
    }
}