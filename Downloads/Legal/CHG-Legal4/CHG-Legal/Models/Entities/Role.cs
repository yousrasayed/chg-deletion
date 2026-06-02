using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CHG_Legal.Models.Entities
{
    [Table("Roles")]
    public class Role
    {
        [Key]
        [Column("RoleID")]
        public int RoleID { get; set; }

        [Required]
        [StringLength(50)]
        [Column("RoleName")]
        public string RoleName { get; set; } = string.Empty;

        // Navigation Properties
        public virtual ICollection<UserRole> UserRole { get; set; } = new List<UserRole>();
    }
}