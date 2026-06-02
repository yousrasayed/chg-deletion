using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CHG_Legal.Models.Entities
{
    [Table("Shareholders", Schema = "Company")]
    public class Shareholder
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ShareholderId { get; set; }

        public int? CompanyId { get; set; }

        [MaxLength(200)]
        public string? ShareName { get; set; }

        [MaxLength(100)]
        public string? Role { get; set; }

        [MaxLength(50)]
        public string? SharesPercentage { get; set; }

        [MaxLength(200)]
        public string? FounderName { get; set; }

        public double? FounderShareCount { get; set; }

        public double? SubscribedShareCount { get; set; }

        public double? ExcellentShareCount { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public double? TotalShareCounts { get; set; }

        [ForeignKey("CompanyId")]
        public virtual Company? Company { get; set; }
    }
}