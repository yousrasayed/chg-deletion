using CHG_Legal.Models;
using CHG_Legal.Models.Entities;
using CHG_Legal.Models.ViewModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CHG_Legal.Services.Interfaces;

namespace CHG_Legal.Services.Implementations
{
    public class CompanyService : ICompanyService
    {
        private readonly AppDbContext _context;

        public CompanyService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<CompanyViewModel>> GetAllCompaniesAsync()
        {
            var companies = await _context.Companies
                .Include(c => c.Shareholders)
                .Include(c => c.Branches)
                .Include(c => c.BoardSettings)
                    .ThenInclude(bs => bs.BoardMembers)
                .Include(c => c.BankingGroups)
                    .ThenInclude(bg => bg.BankingGroupMembers)
                .Include(c => c.NonBankingAuthorizations)
                .ToListAsync();

           
            var shareValueEntity = await _context.ShareValues.FirstOrDefaultAsync();
            var currentShareValue = shareValueEntity?.Value ?? 1.0;

            var viewModels = companies.Select(c => MapToViewModel(c, currentShareValue)).ToList();

            return viewModels;
        }

        public async Task<CompanyViewModel> GetCompanyByIdAsync(int id)
        {
            var company = await _context.Companies
                .Include(c => c.Shareholders)
                .Include(c => c.Branches)
                .Include(c => c.BoardSettings)
                    .ThenInclude(bs => bs.BoardMembers)
                .Include(c => c.BankingGroups)
                    .ThenInclude(bg => bg.BankingGroupMembers)
                .Include(c => c.NonBankingAuthorizations)
                .FirstOrDefaultAsync(c => c.CompanyId == id);

            if (company == null)
                return null;

            var shareValueEntity = await _context.ShareValues.FirstOrDefaultAsync();
            var currentShareValue = shareValueEntity?.Value ?? 1.0;

            var viewModel = MapToViewModel(company, currentShareValue);

            // Load meeting data
            await LoadMeetingDataAsync(viewModel);

            return viewModel;
        }

        public async Task<CompanyViewModel> CreateCompanyAsync(CompanyViewModel model)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var company = new Company
                {
                    Name = model.Name,
                    HeadOfficeAddress = model.HeadOfficeAddress,
                    CommercialRegNo = model.CommercialRegNo,
                    RegistrationExpiry = model.RegistrationExpiry,
                    RegistrationNotificationPeriod = model.RegistrationNotificationPeriod,
                    RegistrationDate = model.RegistrationDate,
                    TaxCardNumber = model.TaxCardNumber,
                    CardRenewalDate = model.CardRenewalDate,
                    CardRenewalDue = model.CardRenewalDue,
                    VAT = model.VAT,
                    AccountingAuditor = model.AccountingAuditor,
                    AccountingAuditorHiringDate = model.AccountingAuditorHiringDate,
                    AuthorizedCapital = model.AuthorizedCapital,
                    AuthorizedCapitalDate = model.AuthorizedCapitalDate,
                    IssuedCapital = model.IssuedCapital,
                    IssuedCapitalDate = model.IssuedCapitalDate,
                    PaidUpCapital = model.PaidUpCapital,
                    PaidUpCapitalDate = model.PaidUpCapitalDate
                };

                _context.Companies.Add(company);
                await _context.SaveChangesAsync();

                // Save Shareholders
                if (model.Shareholders != null)
                {
                    foreach (var sh in model.Shareholders)
                    {
                        var shareholder = new Shareholder
                        {
                            CompanyId = company.CompanyId,
                            ShareName = sh.Name,
                            Role = sh.Role,
                            SharesPercentage = sh.SharesPercentage,
                            FounderName = sh.FounderName,
                            FounderShareCount = sh.FounderShareCount,
                            SubscribedShareCount = sh.SubscribedShareCount,
                            ExcellentShareCount = sh.ExcellentShareCount,
                            StartDate = sh.StartDate,
                            EndDate = sh.EndDate,
                            TotalShareCounts = sh.TotalShareCounts
                        };
                        _context.Shareholders.Add(shareholder);
                    }
                }

                // Save Branches
                if (model.Branches != null)
                {
                    foreach (var br in model.Branches)
                    {
                        var branch = new Branch
                        {
                            CompanyId = company.CompanyId,
                            Name = br.Name,
                            RegistrationNumber = br.RegistrationNumber,
                            ExpiryDate = br.ExpiryDate,
                            Address = br.Address,
                            RegistrationNotification = br.RegistrationNotification
                        };
                        _context.Branches.Add(branch);
                    }
                }

                // Save Board Settings and Members
                if (model.BoardSettings != null)
                {
                    var boardSetting = new BoardSetting
                    {
                        CompanyId = company.CompanyId,
                        Duration = model.BoardSettings.Duration,
                        StartDate = model.BoardSettings.StartDate,
                        NotificationPeriod = model.BoardSettings.NotificationPeriod
                    };
                    _context.BoardSettings.Add(boardSetting);
                    await _context.SaveChangesAsync();

                    if (model.BoardMembers != null)
                    {
                        foreach (var bm in model.BoardMembers)
                        {
                            var boardMember = new BoardMember
                            {
                                BoardId = boardSetting.BoardId,
                                Name = bm.Name,
                                Position = bm.Position
                            };
                            _context.BoardMembers.Add(boardMember);
                        }
                    }
                }

                // Save Banking Groups
                if (model.BankSignatures != null)
                {
                    foreach (var bg in model.BankSignatures)
                    {
                        var bankingGroup = new BankingGroup
                        {
                            CompanyId = company.CompanyId,
                            GroupName = bg.GroupName
                        };
                        _context.BankingGroups.Add(bankingGroup);
                        await _context.SaveChangesAsync();

                        foreach (var member in bg.Members)
                        {
                            var groupMember = new BankingGroupMember
                            {
                                GroupId = bankingGroup.GroupId,
                                MemberName = member,
                                Note = model.BankSignaturesNotes 
                            };
                            _context.BankingGroupMembers.Add(groupMember);
                        }
                    }
                }
               // Save Non-Banking Authorizations
if (model.NonBankSignatures != null)
{
    foreach (var nba in model.NonBankSignatures)
    {
        var authorization = new NonBankingAuthorization
        {
            CompanyId = company.CompanyId,
            Name = nba.Name,
            IssuedTo = nba.IssuedTo,
            AuthorizationDetails = nba.AuthorizationDetails,
            ExpiryDate = nba.ExpiryDate,
            Note = model.NonBankSignaturesNotes,  
            NotificationPerid = nba.NotificationPerid
        };
        _context.NonBankingAuthorizations.Add(authorization);
    }
}

                // Save Meeting Data
                await SaveMeetingDataAsync(company.CompanyId, model.Meetings);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                model.CompanyId = company.CompanyId;
                return model;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new Exception($"Error creating company: {ex.Message}", ex);
            }
        }

        public async Task<CompanyViewModel> UpdateCompanyAsync(CompanyViewModel model)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var company = await _context.Companies
                    .FirstOrDefaultAsync(c => c.CompanyId == model.CompanyId);

                if (company == null)
                    throw new Exception("Company not found");

                // Update company basic info
                company.Name = model.Name;
                company.HeadOfficeAddress = model.HeadOfficeAddress;
                company.CommercialRegNo = model.CommercialRegNo;
                company.RegistrationExpiry = model.RegistrationExpiry;
                company.RegistrationNotificationPeriod = model.RegistrationNotificationPeriod;
                company.RegistrationDate = model.RegistrationDate;
                company.TaxCardNumber = model.TaxCardNumber;
                company.CardRenewalDate = model.CardRenewalDate;
                company.CardRenewalDue = model.CardRenewalDue;
                company.VAT = model.VAT;
                company.AccountingAuditor = model.AccountingAuditor;
                company.AccountingAuditorHiringDate = model.AccountingAuditorHiringDate;
                company.AuthorizedCapital = model.AuthorizedCapital;
                company.AuthorizedCapitalDate = model.AuthorizedCapitalDate;
                company.IssuedCapital = model.IssuedCapital;
                company.IssuedCapitalDate = model.IssuedCapitalDate;
                company.PaidUpCapital = model.PaidUpCapital;
                company.PaidUpCapitalDate = model.PaidUpCapitalDate;

                _context.Companies.Update(company);

                // Update Shareholders
                _context.Shareholders.RemoveRange(_context.Shareholders.Where(s => s.CompanyId == model.CompanyId));
                if (model.Shareholders != null)
                {
                    foreach (var sh in model.Shareholders)
                    {
                        var shareholder = new Shareholder
                        {
                            CompanyId = model.CompanyId,
                            ShareName = sh.Name,
                            Role = sh.Role,
                            SharesPercentage = sh.SharesPercentage,
                            FounderName = sh.FounderName,
                            FounderShareCount = sh.FounderShareCount,
                            SubscribedShareCount = sh.SubscribedShareCount,
                            ExcellentShareCount = sh.ExcellentShareCount,
                            StartDate = sh.StartDate,
                            EndDate = sh.EndDate,
                            TotalShareCounts = sh.TotalShareCounts
                        };
                        _context.Shareholders.Add(shareholder);
                    }
                }

                // Update Branches
                _context.Branches.RemoveRange(_context.Branches.Where(b => b.CompanyId == model.CompanyId));
                if (model.Branches != null)
                {
                    foreach (var br in model.Branches)
                    {
                        var branch = new Branch
                        {
                            CompanyId = model.CompanyId,
                            Name = br.Name,
                            RegistrationNumber = br.RegistrationNumber,
                            ExpiryDate = br.ExpiryDate,
                            Address = br.Address,
                            RegistrationNotification = br.RegistrationNotification
                        };
                        _context.Branches.Add(branch);
                    }
                }

                // Update Board Settings and Members
                var existingBoardSettings = await _context.BoardSettings
                    .FirstOrDefaultAsync(bs => bs.CompanyId == model.CompanyId);

                if (existingBoardSettings != null)
                {
                    _context.BoardMembers.RemoveRange(_context.BoardMembers.Where(bm => bm.BoardId == existingBoardSettings.BoardId));
                    _context.BoardSettings.Remove(existingBoardSettings);
                }

                if (model.BoardSettings != null)
                {
                    var boardSetting = new BoardSetting
                    {
                        CompanyId = model.CompanyId,
                        Duration = model.BoardSettings.Duration,
                        StartDate = model.BoardSettings.StartDate,
                        NotificationPeriod = model.BoardSettings.NotificationPeriod
                    };
                    _context.BoardSettings.Add(boardSetting);
                    await _context.SaveChangesAsync();

                    if (model.BoardMembers != null)
                    {
                        foreach (var bm in model.BoardMembers)
                        {
                            var boardMember = new BoardMember
                            {
                                BoardId = boardSetting.BoardId,
                                Name = bm.Name,
                                Position = bm.Position
                            };
                            _context.BoardMembers.Add(boardMember);
                        }
                    }
                }

                // Update Banking Groups
                var existingBankingGroups = await _context.BankingGroups
                    .Include(bg => bg.BankingGroupMembers)
                    .Where(bg => bg.CompanyId == model.CompanyId)
                    .ToListAsync();

                foreach (var bg in existingBankingGroups)
                {
                    _context.BankingGroupMembers.RemoveRange(bg.BankingGroupMembers);
                    _context.BankingGroups.Remove(bg);
                }

                if (model.BankSignatures != null)
                {
                    foreach (var bg in model.BankSignatures)
                    {
                        var bankingGroup = new BankingGroup
                        {
                            CompanyId = model.CompanyId,
                            GroupName = bg.GroupName
                        };
                        _context.BankingGroups.Add(bankingGroup);
                        await _context.SaveChangesAsync();

                        foreach (var member in bg.Members)
                        {
                            var groupMember = new BankingGroupMember
                            {
                                GroupId = bankingGroup.GroupId,
                                MemberName = member,
                                Note = model.BankSignaturesNotes  
                            };
                            _context.BankingGroupMembers.Add(groupMember);
                        }
                    }
                }
                // Update Non-Banking Authorizations
                _context.NonBankingAuthorizations.RemoveRange(_context.NonBankingAuthorizations.Where(nba => nba.CompanyId == model.CompanyId));
                if (model.NonBankSignatures != null)
                {
                    foreach (var nba in model.NonBankSignatures)
                    {
                        var authorization = new NonBankingAuthorization
                        {
                            CompanyId = model.CompanyId,
                            Name = nba.Name,
                            IssuedTo = nba.IssuedTo,
                            AuthorizationDetails = nba.AuthorizationDetails,
                            ExpiryDate = nba.ExpiryDate,
                            Note = model.NonBankSignaturesNotes,  
                            NotificationPerid = nba.NotificationPerid
                        };
                        _context.NonBankingAuthorizations.Add(authorization);
                    }
                }

                // Update Meeting Data
                await UpdateMeetingDataAsync(model.CompanyId, model.Meetings);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return model;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new Exception($"Error updating company: {ex.Message}", ex);
            }
        }

        public async Task<bool> DeleteCompanyAsync(int id)
        {
            var company = await _context.Companies
                .Include(c => c.Shareholders)
                .Include(c => c.Branches)
                .Include(c => c.BoardSettings)
                    .ThenInclude(bs => bs.BoardMembers)
                .Include(c => c.BankingGroups)
                    .ThenInclude(bg => bg.BankingGroupMembers)
                .Include(c => c.NonBankingAuthorizations)
                .FirstOrDefaultAsync(c => c.CompanyId == id);

            if (company == null)
                return false;

            _context.Companies.Remove(company);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<double?> GetCurrentShareValueAsync()
        {
            var shareValue = await _context.ShareValues.FirstOrDefaultAsync();
            return shareValue?.Value;
        }

        public async Task<List<string>> GetBankGroupsAsync()
        {
            return await _context.Groups.Select(g => g.GroupName ?? string.Empty).ToListAsync();
        }

        private CompanyViewModel MapToViewModel(Company company, double? currentShareValue = null)
        {
            if (company == null) return null;

            var viewModel = new CompanyViewModel
            {
                CompanyId = company.CompanyId,
                Name = company.Name ?? string.Empty,
                HeadOfficeAddress = company.HeadOfficeAddress ?? string.Empty,
                CommercialRegNo = company.CommercialRegNo ?? string.Empty,
                RegistrationExpiry = company.RegistrationExpiry,
                RegistrationNotificationPeriod = company.RegistrationNotificationPeriod,
                RegistrationDate = company.RegistrationDate,
                TaxCardNumber = company.TaxCardNumber ?? string.Empty,
                CardRenewalDate = company.CardRenewalDate,
                CardRenewalDue = company.CardRenewalDue,
                VAT = company.VAT,
                AccountingAuditor = company.AccountingAuditor ?? string.Empty,
                AccountingAuditorHiringDate = company.AccountingAuditorHiringDate,
                AuthorizedCapital = company.AuthorizedCapital,
                AuthorizedCapitalDate = company.AuthorizedCapitalDate,
                IssuedCapital = company.IssuedCapital,
                IssuedCapitalDate = company.IssuedCapitalDate,
                PaidUpCapital = company.PaidUpCapital,
                PaidUpCapitalDate = company.PaidUpCapitalDate,
                ShareValue = currentShareValue ?? 1.0,
                Shareholders = company.Shareholders?.Select(s => new ShareholderViewModel
                {
                    ShareholderId = s.ShareholderId,
                    Name = s.ShareName ?? string.Empty,
                    Role = s.Role ?? string.Empty,
                    SharesPercentage = s.SharesPercentage ?? "0",
                    FounderName = s.FounderName ?? string.Empty,
                    FounderShareCount = s.FounderShareCount ?? 0,
                    SubscribedShareCount = s.SubscribedShareCount ?? 0,
                    ExcellentShareCount = s.ExcellentShareCount ?? 0,
                    StartDate = s.StartDate,
                    EndDate = s.EndDate,
                    TotalShareCounts = s.TotalShareCounts ?? 0
                }).ToList() ?? new List<ShareholderViewModel>(),
                Branches = company.Branches?.Select(b => new BranchViewModel
                {
                    BranchId = b.BranchId,
                    Name = b.Name ?? string.Empty,
                    RegistrationNumber = b.RegistrationNumber ?? string.Empty,
                    ExpiryDate = b.ExpiryDate,
                    Address = b.Address ?? string.Empty,
                    RegistrationNotification = b.RegistrationNotification
                }).ToList() ?? new List<BranchViewModel>()
            };

            // Map Board Settings
            var boardSetting = company.BoardSettings?.FirstOrDefault();
            if (boardSetting != null)
            {
                viewModel.BoardSettings = new BoardSettingsViewModel
                {
                    BoardId = boardSetting.BoardId,
                    Duration = boardSetting.Duration ?? string.Empty,
                    StartDate = boardSetting.StartDate,
                    NotificationPeriod = boardSetting.NotificationPeriod
                };
                viewModel.BoardMembers = boardSetting.BoardMembers?.Select(bm => new BoardMemberViewModel
                {
                    MemberId = bm.MemberId,
                    Name = bm.Name ?? string.Empty,
                    Position = bm.Position ?? string.Empty
                }).ToList() ?? new List<BoardMemberViewModel>();
            }
            else
            {
                viewModel.BoardSettings = new BoardSettingsViewModel();
                viewModel.BoardMembers = new List<BoardMemberViewModel>();
            }

            // Map Banking Groups
            if (company.BankingGroups != null && company.BankingGroups.Any())
            {
                viewModel.BankSignatures = company.BankingGroups.Select(bg => new BankSignatureViewModel
                {
                    GroupId = bg.GroupId,
                    GroupName = bg.GroupName ?? string.Empty,
                    Members = bg.BankingGroupMembers?.Select(m => m.MemberName ?? string.Empty).ToList() ?? new List<string>(),
                    Note = bg.BankingGroupMembers?.FirstOrDefault()?.Note ?? string.Empty  
                }).ToList();
            }
            else
            {
                viewModel.BankSignatures = new List<BankSignatureViewModel>();
            }

            // Map Non-Banking Authorizations
            if (company.NonBankingAuthorizations != null && company.NonBankingAuthorizations.Any())
            {
                viewModel.NonBankSignatures = company.NonBankingAuthorizations.Select(nba => new NonBankSignatureViewModel
                {
                    AuthorizationId = nba.AuthorizationId,
                    Name = nba.Name ?? string.Empty,
                    IssuedTo = nba.IssuedTo ?? string.Empty,
                    AuthorizationDetails = nba.AuthorizationDetails ?? string.Empty,
                    ExpiryDate = nba.ExpiryDate,
                    Note = "",  
                    NotificationPerid = nba.NotificationPerid
                }).ToList();
            }
            else
            {
                viewModel.NonBankSignatures = new List<NonBankSignatureViewModel>();
            }

          
            viewModel.NonBankSignaturesNotes = company.NonBankingAuthorizations?.FirstOrDefault()?.Note ?? "";

            return viewModel;
        }

        private async Task LoadMeetingDataAsync(CompanyViewModel viewModel)
        {
            var associationTypes = new[] { "ordinary", "extraordinary", "board" };
            var meetings = new MeetingsViewModel();

            foreach (var type in associationTypes)
            {
                var association = await _context.Associations
                    .Include(a => a.AssociationPlaces)
                    .FirstOrDefaultAsync(a => a.AssociationType == type);

                if (association != null)
                {
                    var meetingVM = new AssociationViewModel
                    {
                        ID = association.ID,
                        AssociationType = association.AssociationType ?? string.Empty,
                        VotingMechanism = association.VotingMechanism ?? string.Empty,
                        ValidityValue = association.ValidityValue,
                        Places = association.AssociationPlaces?.Select(p => p.AsscoiationPlace ?? string.Empty).ToList() ?? new List<string>(),
                        Address = association.AssociationPlaces?.FirstOrDefault()?.associationPlaceDecr ?? string.Empty
                    };

                    if (type == "ordinary")
                        meetings.OrdinaryMeeting = meetingVM;
                    else if (type == "extraordinary")
                        meetings.ExtraordinaryMeeting = meetingVM;
                    else if (type == "board")
                        meetings.BoardMeeting = meetingVM;
                }
            }

            viewModel.Meetings = meetings;
        }

        private async Task SaveMeetingDataAsync(int companyId, MeetingsViewModel meetings)
        {
            if (meetings == null) return;

            var meetingsToSave = new[]
            {
                meetings.OrdinaryMeeting,
                meetings.ExtraordinaryMeeting,
                meetings.BoardMeeting
            };

            foreach (var meeting in meetingsToSave)
            {
                if (meeting != null)
                {
                    var association = await _context.Associations
                        .FirstOrDefaultAsync(a => a.AssociationType == meeting.AssociationType);

                    if (association == null)
                    {
                        association = new Association
                        {
                            AssociationType = meeting.AssociationType,
                            VotingMechanism = meeting.VotingMechanism,
                            ValidityValue = meeting.ValidityValue
                        };
                        _context.Associations.Add(association);
                        await _context.SaveChangesAsync();
                    }
                    else
                    {
                        association.VotingMechanism = meeting.VotingMechanism;
                        association.ValidityValue = meeting.ValidityValue;
                        _context.Associations.Update(association);
                        await _context.SaveChangesAsync();
                    }

                    // Update places
                    var existingPlaces = _context.AssociationPlaces.Where(p => p.associationID == association.ID);
                    _context.AssociationPlaces.RemoveRange(existingPlaces);

                    foreach (var place in meeting.Places)
                    {
                        var associationPlace = new AssociationPlace
                        {
                            associationID = association.ID,
                            AsscoiationPlace = place,
                            associationPlaceDecr = meeting.Address
                        };
                        _context.AssociationPlaces.Add(associationPlace);
                    }
                }
            }
        }

        private async Task UpdateMeetingDataAsync(int companyId, MeetingsViewModel meetings)
        {
            await SaveMeetingDataAsync(companyId, meetings);
        }


    }
}