// CHG_Legal/Services/Implementations/DashboardService.cs
using CHG_Legal.Models;
using CHG_Legal.Models.ViewModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CHG_Legal.Services.Interfaces;

namespace CHG_Legal.Services.Implementations
{
    public class DashboardService : IDashboardService
    {
        private readonly AppDbContext _context;

        public DashboardService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<DashboardViewModel> GetDashboardDataAsync()
        {
            var viewModel = new DashboardViewModel();

            // Get all companies with their related data
            var companies = await _context.Companies
                .Include(c => c.Shareholders)
                .Include(c => c.Branches)
                .Include(c => c.BoardSettings)
                .Include(c => c.BankingGroups)
                .Include(c => c.NonBankingAuthorizations)
                .ToListAsync();

            var today = DateTime.Today;
            var thirtyDaysFromNow = today.AddDays(30);

            // ============ حساب المساهمين الحاليين فقط ============
            // لكل شركة، نحسب عدد المساهمين الحاليين (بدون تاريخ خروج أو تاريخ خروج > اليوم)
            foreach (var company in companies)
            {
                var currentShareholders = company.Shareholders
                    .Where(s => !s.EndDate.HasValue || s.EndDate.Value > today)
                    .ToList();

                // يمكن تخزين العدد في خاصية مؤقتة أو استخدامه مباشرة
                // سنستخدم LINQ في كل عملية حساب
            }

            // ============ STATS CARDS ============
            viewModel.TotalCompanies = companies.Count;

            viewModel.ActiveCompanies = companies.Count(c =>
                !c.RegistrationExpiry.HasValue || c.RegistrationExpiry.Value > today);

            viewModel.ExpiringCompanies = companies.Count(c =>
                c.RegistrationExpiry.HasValue &&
                c.RegistrationExpiry.Value > today &&
                c.RegistrationExpiry.Value <= thirtyDaysFromNow);

            viewModel.ExpiredCompanies = companies.Count(c =>
                c.RegistrationExpiry.HasValue &&
                c.RegistrationExpiry.Value <= today);

            // ============ COMPANIES BY SHAREHOLDER COUNT (الحاليين فقط) ============
            viewModel.CompaniesByShareholderCount = new Dictionary<string, int>();

            var companiesWithShareholdersCount = companies.Select(c => new
            {
                Company = c,
                CurrentShareholdersCount = c.Shareholders.Count(s => !s.EndDate.HasValue || s.EndDate.Value > today)
            }).ToList();

            var noShareholders = companiesWithShareholdersCount.Count(c => c.CurrentShareholdersCount == 0);
            var oneToThree = companiesWithShareholdersCount.Count(c => c.CurrentShareholdersCount >= 1 && c.CurrentShareholdersCount <= 3);
            var fourToSeven = companiesWithShareholdersCount.Count(c => c.CurrentShareholdersCount >= 4 && c.CurrentShareholdersCount <= 7);
            var eightToTwelve = companiesWithShareholdersCount.Count(c => c.CurrentShareholdersCount >= 8 && c.CurrentShareholdersCount <= 12);
            var moreThanTwelve = companiesWithShareholdersCount.Count(c => c.CurrentShareholdersCount > 12);

            if (noShareholders > 0) viewModel.CompaniesByShareholderCount["بدون مساهمين"] = noShareholders;
            if (oneToThree > 0) viewModel.CompaniesByShareholderCount["1-3 مساهمين"] = oneToThree;
            if (fourToSeven > 0) viewModel.CompaniesByShareholderCount["4-7 مساهمين"] = fourToSeven;
            if (eightToTwelve > 0) viewModel.CompaniesByShareholderCount["8-12 مساهمين"] = eightToTwelve;
            if (moreThanTwelve > 0) viewModel.CompaniesByShareholderCount["أكثر من 12"] = moreThanTwelve;

            // ============ COMPANIES BY BRANCH COUNT ============
            viewModel.CompaniesByBranchCount = new Dictionary<string, int>();

            var noBranches = companies.Count(c => !c.Branches.Any());
            var oneBranch = companies.Count(c => c.Branches.Count == 1);
            var twoToFour = companies.Count(c => c.Branches.Count >= 2 && c.Branches.Count <= 4);
            var fivePlus = companies.Count(c => c.Branches.Count >= 5);

            if (noBranches > 0) viewModel.CompaniesByBranchCount["بدون فروع"] = noBranches;
            if (oneBranch > 0) viewModel.CompaniesByBranchCount["فرع واحد"] = oneBranch;
            if (twoToFour > 0) viewModel.CompaniesByBranchCount["2-4 فروع"] = twoToFour;
            if (fivePlus > 0) viewModel.CompaniesByBranchCount["5+ فروع"] = fivePlus;

            // ============ MONTHLY REGISTRATIONS ============
            viewModel.MonthlyRegistrations = new Dictionary<string, int>();
            var currentYear = DateTime.Now.Year;

            var monthNames = new[] { "يناير", "فبراير", "مارس", "إبريل", "مايو", "يونيو",
                                     "يوليو", "أغسطس", "سبتمبر", "أكتوبر", "نوفمبر", "ديسمبر" };

            for (int i = 1; i <= 12; i++)
            {
                var count = companies.Count(c =>
                    c.RegistrationDate.HasValue &&
                    c.RegistrationDate.Value.Year == currentYear &&
                    c.RegistrationDate.Value.Month == i);

                viewModel.MonthlyRegistrations[monthNames[i - 1]] = count;
            }

            // ============ YEARLY REPORTS TABLE ============
            var years = companies
                .Where(c => c.RegistrationDate.HasValue)
                .Select(c => c.RegistrationDate.Value.Year)
                .Distinct()
                .OrderBy(y => y)
                .ToList();

            foreach (var year in years)
            {
                var companiesInYear = companies
                    .Where(c => c.RegistrationDate.HasValue && c.RegistrationDate.Value.Year == year)
                    .ToList();

                // حساب المساهمين الحاليين فقط لكل شركة في السنة
                var totalCurrentShareholders = companiesInYear.Sum(c =>
                    c.Shareholders.Count(s => !s.EndDate.HasValue || s.EndDate.Value > today));

                var totalBranches = companiesInYear.Sum(c => c.Branches.Count);

                var report = new YearlyReportViewModel
                {
                    Year = year,
                    TotalCompanies = companiesInYear.Count,
                    ActiveCompanies = companiesInYear.Count(c => !c.RegistrationExpiry.HasValue || c.RegistrationExpiry.Value > today),
                    ExpiredCompanies = companiesInYear.Count(c => c.RegistrationExpiry.HasValue && c.RegistrationExpiry.Value <= today),
                    TotalShareholders = totalCurrentShareholders,
                    TotalBranches = totalBranches,
                    AverageShareholdersPerCompany = companiesInYear.Count > 0 ? (double)totalCurrentShareholders / companiesInYear.Count : 0,
                    AverageBranchesPerCompany = companiesInYear.Count > 0 ? (double)totalBranches / companiesInYear.Count : 0
                };

                // Companies by size (حسب عدد المساهمين الحاليين)
                var companiesWithCurrentCount = companiesInYear.Select(c => new
                {
                    Company = c,
                    CurrentCount = c.Shareholders.Count(s => !s.EndDate.HasValue || s.EndDate.Value > today)
                }).ToList();

                report.CompaniesBySize["بدون مساهمين"] = companiesWithCurrentCount.Count(c => c.CurrentCount == 0);
                report.CompaniesBySize["صغيرة (1-3)"] = companiesWithCurrentCount.Count(c => c.CurrentCount >= 1 && c.CurrentCount <= 3);
                report.CompaniesBySize["متوسطة (4-7)"] = companiesWithCurrentCount.Count(c => c.CurrentCount >= 4 && c.CurrentCount <= 7);
                report.CompaniesBySize["كبيرة (8+)"] = companiesWithCurrentCount.Count(c => c.CurrentCount >= 8);

                // Remove empty categories
                report.CompaniesBySize = report.CompaniesBySize.Where(x => x.Value > 0).ToDictionary(x => x.Key, x => x.Value);

                if (report.CompaniesBySize.Count == 0)
                {
                    report.CompaniesBySize["لا توجد بيانات"] = report.TotalCompanies;
                }

                viewModel.YearlyReports.Add(report);
            }

            // ============ TOP COMPANIES BY CURRENT SHAREHOLDERS ============
            viewModel.TopCompaniesByShareholders = companies
                .Select(c => new
                {
                    Company = c,
                    CurrentShareholderCount = c.Shareholders.Count(s => !s.EndDate.HasValue || s.EndDate.Value > today)
                })
                .Where(c => c.CurrentShareholderCount > 0)
                .OrderByDescending(c => c.CurrentShareholderCount)
                .Take(10)
                .Select(c => new TopCompanyViewModel
                {
                    CompanyId = c.Company.CompanyId,
                    CompanyName = string.IsNullOrEmpty(c.Company.Name) ? "بدون اسم" : c.Company.Name,
                    Value = c.CurrentShareholderCount,
                    ShareholderCount = c.CurrentShareholderCount,
                    BranchCount = c.Company.Branches.Count,
                    RegistrationDate = c.Company.RegistrationDate
                })
                .ToList();

            // ============ TOP COMPANIES BY BRANCHES COUNT ============
            viewModel.TopCompaniesByBranches = companies
                .Where(c => c.Branches.Any())
                .OrderByDescending(c => c.Branches.Count)
                .Take(10)
                .Select(c => new TopCompanyViewModel
                {
                    CompanyId = c.CompanyId,
                    CompanyName = string.IsNullOrEmpty(c.Name) ? "بدون اسم" : c.Name,
                    Value = c.Branches.Count,
                    ShareholderCount = c.Shareholders.Count(s => !s.EndDate.HasValue || s.EndDate.Value > today),
                    BranchCount = c.Branches.Count,
                    RegistrationDate = c.RegistrationDate
                })
                .ToList();

            return viewModel;
        }
    }
}