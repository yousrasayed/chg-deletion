//using CHG_Legal.Services.Interfaces;
//using Microsoft.AspNetCore.Mvc;

//namespace CHG_Legal.Controllers
//{
//    public class DashboardController : Controller
//    {
//        private readonly IMeetingService _meetingService;

//        public DashboardController(IMeetingService meetingService)
//        {
//            _meetingService = meetingService;
//        }

//        public async Task<IActionResult> Index()
//        {
//            if (HttpContext.Session.GetString("IsLoggedIn") != "true")
//                return RedirectToAction("Login", "Account");

//            var viewModel = await _meetingService.GetDashboardDataAsync();

//            // Get all meetings for hospital details in expandable rows
//            var allMeetings = await _meetingService.GetAllMeetingsAsync();

//            // Build hospital yearly data for JavaScript
//            var hospitalYearlyData = new Dictionary<string, Dictionary<int, int>>();
//            var hospitalYearlyTypeData = new Dictionary<string, Dictionary<string, int>>();

//            foreach (var meeting in allMeetings)
//            {
//                if (!string.IsNullOrEmpty(meeting.CHGParty) && meeting.BoardDate != null)
//                {
//                    var year = meeting.BoardDate.Year;
//                    var hospitals = meeting.CHGParty.Split(',', StringSplitOptions.RemoveEmptyEntries);
//                    var meetingType = meeting.MeetingType ?? "غير محدد";

//                    foreach (var hospital in hospitals)
//                    {
//                        var hospitalName = hospital.Trim();
//                        var key = $"{year}_{hospitalName}";
//                        var typeKey = $"{year}_{hospitalName}_{meetingType}";

//                        // For count of meetings
//                        if (!hospitalYearlyData.ContainsKey(key))
//                        {
//                            hospitalYearlyData[key] = new Dictionary<int, int>();
//                        }
//                        if (!hospitalYearlyData[key].ContainsKey(year))
//                        {
//                            hospitalYearlyData[key][year] = 0;
//                        }
//                        hospitalYearlyData[key][year]++;

//                        // For breakdown by type
//                        if (!hospitalYearlyTypeData.ContainsKey(key))
//                        {
//                            hospitalYearlyTypeData[key] = new Dictionary<string, int>();
//                        }
//                        if (!hospitalYearlyTypeData[key].ContainsKey(meetingType))
//                        {
//                            hospitalYearlyTypeData[key][meetingType] = 0;
//                        }
//                        hospitalYearlyTypeData[key][meetingType]++;
//                    }
//                }
//            }

//            ViewBag.HospitalYearlyData = hospitalYearlyData;
//            ViewBag.HospitalYearlyTypeData = hospitalYearlyTypeData;

//            return View(viewModel);
//        }
//    }
//}
// CHG_Legal/Controllers/DashboardController.cs
using CHG_Legal.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CHG_Legal.Controllers
{
    public class DashboardController : Controller
    {
        private readonly IDashboardService _dashboardService;

        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        public async Task<IActionResult> Index()
        {
            if (HttpContext.Session.GetString("IsLoggedIn") != "true")
                return RedirectToAction("Login", "Account");

            var viewModel = await _dashboardService.GetDashboardDataAsync();
            return View(viewModel);
        }
    }
}