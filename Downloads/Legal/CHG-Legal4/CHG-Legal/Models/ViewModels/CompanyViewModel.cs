using System;
using System.Collections.Generic;

namespace CHG_Legal.Models.ViewModels
{
    public class CompanyViewModel
    {
        public int CompanyId { get; set; }
        public string Name { get; set; }
        public string HeadOfficeAddress { get; set; }
        public string CommercialRegNo { get; set; }
        public DateTime? RegistrationExpiry { get; set; }
        public int? RegistrationNotificationPeriod { get; set; }
        public DateTime? RegistrationDate { get; set; }
        public string TaxCardNumber { get; set; }
        public DateTime? CardRenewalDate { get; set; }
        public int? CardRenewalDue { get; set; }
        public double? VAT { get; set; }  // double? 
        public string AccountingAuditor { get; set; }
        public DateTime? AccountingAuditorHiringDate { get; set; }
        public decimal? AuthorizedCapital { get; set; }
        public DateTime? AuthorizedCapitalDate { get; set; }
        public decimal? IssuedCapital { get; set; }
        public DateTime? IssuedCapitalDate { get; set; }
        public decimal? PaidUpCapital { get; set; }
        public DateTime? PaidUpCapitalDate { get; set; }
        public double? ShareValue { get; set; }  // double?

        // Collections
        public List<ShareholderViewModel> Shareholders { get; set; } = new List<ShareholderViewModel>();
        public List<BranchViewModel> Branches { get; set; } = new List<BranchViewModel>();
        public MeetingsViewModel Meetings { get; set; }
        public BoardSettingsViewModel BoardSettings { get; set; }
        public List<BoardMemberViewModel> BoardMembers { get; set; } = new List<BoardMemberViewModel>();
        public List<BankSignatureViewModel> BankSignatures { get; set; } = new List<BankSignatureViewModel>();
        public List<NonBankSignatureViewModel> NonBankSignatures { get; set; } = new List<NonBankSignatureViewModel>();
        public string BankSignaturesNotes { get; set; }
        public string NonBankSignaturesNotes { get; set; }
    }

    public class ShareholderViewModel
    {
        public int ShareholderId { get; set; }
        public string Name { get; set; }
        public string Role { get; set; }
        public string SharesPercentage { get; set; }
        public string FounderName { get; set; }
        public double? FounderShareCount { get; set; }      // double?
        public double? SubscribedShareCount { get; set; }   // double?
        public double? ExcellentShareCount { get; set; }    // double?
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public double? TotalShareCounts { get; set; }       // double?
    }

    public class BranchViewModel
    {
        public int BranchId { get; set; }
        public string Name { get; set; }
        public string RegistrationNumber { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string Address { get; set; }
        public int? RegistrationNotification { get; set; }
    }

    public class MeetingsViewModel
    {
        public AssociationViewModel OrdinaryMeeting { get; set; }
        public AssociationViewModel ExtraordinaryMeeting { get; set; }
        public AssociationViewModel BoardMeeting { get; set; }
    }

    public class AssociationViewModel
    {
        public int ID { get; set; }
        public string AssociationType { get; set; }
        public string VotingMechanism { get; set; }
        public double? ValidityValue { get; set; }  // double?
        public List<string> Places { get; set; } = new List<string>();
        public string Address { get; set; }
    }

    public class BoardSettingsViewModel
    {
        public int BoardId { get; set; }
        public string Duration { get; set; }
        public DateTime? StartDate { get; set; }
        public int? NotificationPeriod { get; set; }
    }

    public class BoardMemberViewModel
    {
        public int MemberId { get; set; }
        public string Name { get; set; }
        public string Position { get; set; }
    }

    public class BankSignatureViewModel
    {
        public int GroupId { get; set; }
        public string GroupName { get; set; }
        public List<string> Members { get; set; } = new List<string>();
        public string Note { get; set; }
    }

    public class NonBankSignatureViewModel
    {
        public int AuthorizationId { get; set; }
        public string Name { get; set; }
        public string IssuedTo { get; set; }
        public string AuthorizationDetails { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string Note { get; set; }
        public int? NotificationPerid { get; set; }
    }
}