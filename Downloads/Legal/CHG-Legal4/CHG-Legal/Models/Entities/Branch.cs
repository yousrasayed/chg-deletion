using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CHG_Legal.Models.Entities
{
    [Table("Branches", Schema = "Company")]
    public class Branch
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int BranchId { get; set; }

        public int? CompanyId { get; set; }

        [MaxLength(200)]
        public string? Name { get; set; }

        [MaxLength(100)]
        public string? RegistrationNumber { get; set; }

        public DateTime? ExpiryDate { get; set; }

        [MaxLength(300)]
        public string? Address { get; set; }

        public int? RegistrationNotification { get; set; }

        [ForeignKey("CompanyId")]
        public virtual Company? Company { get; set; }

        public virtual ICollection<BranchAttachment> BranchAttachments { get; set; } = new List<BranchAttachment>();
    }
}