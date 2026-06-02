using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CHG_Legal.Models.Entities
{
    [Table("Companies", Schema = "Company")]
    public class Company
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CompanyId { get; set; }

        [MaxLength(200)]
        public string? Name { get; set; }

        [MaxLength(300)]
        public string? HeadOfficeAddress { get; set; }

        [MaxLength(100)]
        public string? CommercialRegNo { get; set; }

        public DateTime? RegistrationExpiry { get; set; }

        [MaxLength(100)]
        public string? TaxCardNumber { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? AuthorizedCapital { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? IssuedCapital { get; set; }

        [MaxLength(200)]
        public string? AuditorName { get; set; }

        public DateTime? CardRenewalDate { get; set; }

        public int? CardRenewalDue { get; set; }

        [MaxLength(150)]
        public string? AccountingAuditor { get; set; }

        public DateTime? AccountingAuditorHiringDate { get; set; }

        public DateTime? AuthorizedCapitalDate { get; set; }

        public DateTime? IssuedCapitalDate { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? PaidUpCapital { get; set; }

        public DateTime? PaidUpCapitalDate { get; set; }

        public DateTime? RegistrationDate { get; set; }

        public int? RegistrationNotificationPeriod { get; set; }

        public double? VAT { get; set; }

        // Navigation properties
        public virtual ICollection<Shareholder> Shareholders { get; set; } = new List<Shareholder>();
        public virtual ICollection<Branch> Branches { get; set; } = new List<Branch>();
        public virtual ICollection<BoardSetting> BoardSettings { get; set; } = new List<BoardSetting>();
        public virtual ICollection<BankingGroup> BankingGroups { get; set; } = new List<BankingGroup>();
        public virtual ICollection<NonBankingAuthorization> NonBankingAuthorizations { get; set; } = new List<NonBankingAuthorization>();

        // Attachment collections
        public virtual ICollection<CompanyAttachment> CompanyAttachments { get; set; } = new List<CompanyAttachment>();
        public virtual ICollection<BranchAttachment> BranchAttachments { get; set; } = new List<BranchAttachment>();
        public virtual ICollection<NonBankingAttach> NonBankingAttachments { get; set; } = new List<NonBankingAttach>();
        public virtual ICollection<BankingAttachment> BankingAttachments { get; set; } = new List<BankingAttachment>();
    }
}