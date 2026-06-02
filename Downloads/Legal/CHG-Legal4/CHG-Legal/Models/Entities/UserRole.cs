// Models/Entities/UserRole.cs
using CHG_Legal.Models.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class UserRole
{
    [Key]
    [Column("ID")]
    public int ID { get; set; }

    [ForeignKey("User")]
    [Column("UserID")]
    public int UserID { get; set; }

    [ForeignKey("Role")]
    [Column("RoleID")]
    public int RoleID { get; set; }  // int بدلاً من short

    public virtual User? User { get; set; }
    public virtual Role? Role { get; set; }
}