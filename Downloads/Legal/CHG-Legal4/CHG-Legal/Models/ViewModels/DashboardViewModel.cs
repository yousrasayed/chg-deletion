//namespace CHG_Legal.Models.ViewModels
//{
//    public class DashboardViewModel
//    {
//        public int TotalMeetings { get; set; }
//        public int ApprovedMeetings { get; set; }
//        public int RejectedMeetings { get; set; }
//        public int PendingMeetings { get; set; }
//        public int WaitingMeetings { get; set; }

//        // Charts Data
//        public Dictionary<string, int> MeetingsByType { get; set; } = new();
//        public Dictionary<string, int> MeetingsByHospital { get; set; } = new();

//        // Yearly meetings by type and month (Year -> Type -> Month -> Count)
//        public Dictionary<int, Dictionary<string, Dictionary<int, int>>> YearlyMeetingsByType { get; set; } = new();

//        // Yearly report table
//        public List<YearlyReportViewModel> YearlyReports { get; set; } = new();
//    }

//    public class YearlyReportViewModel
//    {
//        public int Year { get; set; }
//        public Dictionary<string, int> MeetingsByType { get; set; } = new();
//        public int TotalMeetings { get; set; }
//        public int FullyApprovedMeetings { get; set; }
//        public int PendingApprovalMeetings { get; set; }
//        public int WaitingMeetings { get; set; }
//    }
//}
// CHG_Legal/Models/ViewModels/DashboardViewModel.cs
// CHG_Legal/Models/ViewModels/DashboardViewModel.cs
namespace CHG_Legal.Models.ViewModels
{
    public class DashboardViewModel
    {
        // Company Statistics
        public int TotalCompanies { get; set; }
        public int ActiveCompanies { get; set; }
        public int ExpiringCompanies { get; set; }
        public int ExpiredCompanies { get; set; }

        // Charts Data - بدون رأس المال
        public Dictionary<string, int> CompaniesByShareholderCount { get; set; } = new();
        public Dictionary<string, int> CompaniesByBranchCount { get; set; } = new();
        public Dictionary<string, int> MonthlyRegistrations { get; set; } = new();

        // Yearly report table
        public List<YearlyReportViewModel> YearlyReports { get; set; } = new();

        // Top companies (by shareholder count or branch count)
        public List<TopCompanyViewModel> TopCompaniesByShareholders { get; set; } = new();
        public List<TopCompanyViewModel> TopCompaniesByBranches { get; set; } = new();
    }

    public class YearlyReportViewModel
    {
        public int Year { get; set; }
        public int TotalCompanies { get; set; }
        public int ActiveCompanies { get; set; }
        public int ExpiredCompanies { get; set; }
        public int TotalShareholders { get; set; }
        public int TotalBranches { get; set; }
        public double AverageShareholdersPerCompany { get; set; }
        public double AverageBranchesPerCompany { get; set; }
        public Dictionary<string, int> CompaniesBySize { get; set; } = new(); // حسب عدد المساهمين
    }

    public class TopCompanyViewModel
    {
        public int CompanyId { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public int Value { get; set; } // عدد المساهمين أو عدد الفروع
        public int ShareholderCount { get; set; }
        public int BranchCount { get; set; }
        public DateTime? RegistrationDate { get; set; }
    }
}