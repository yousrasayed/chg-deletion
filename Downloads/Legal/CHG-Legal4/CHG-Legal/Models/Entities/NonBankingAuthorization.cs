using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CHG_Legal.Models.Entities
{
    [Table("NonBankingAuthorizations", Schema = "Company")]
    public class NonBankingAuthorization
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int AuthorizationId { get; set; }

        public int? CompanyId { get; set; }

        [MaxLength(200)]
        public string? Name { get; set; }

        [MaxLength(200)]
        public string? IssuedTo { get; set; }

        [MaxLength(300)]
        public string? AuthorizationDetails { get; set; }

        public DateTime? ExpiryDate { get; set; }

        [MaxLength(450)]
        public string? Note { get; set; }

        public int? NotificationPerid { get; set; }

        [ForeignKey("CompanyId")]
        public virtual Company? Company { get; set; }
        // Models/Entities/NonBankingAuthorization.cs - أضف هذه الخاصية
        public virtual ICollection<NonBankingAttach> NonBankingAttachments { get; set; } = new List<NonBankingAttach>();
    }
}