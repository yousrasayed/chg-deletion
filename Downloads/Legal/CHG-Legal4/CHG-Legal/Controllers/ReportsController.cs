//using CHG_Legal.Services.Interfaces;
//using Microsoft.AspNetCore.Mvc;

//namespace CHG_Legal.Controllers
//{
//    public class ReportsController : Controller
//    {
//        private readonly IMeetingService _meetingService;

//        public ReportsController(IMeetingService meetingService)
//        {
//            _meetingService = meetingService;
//        }

//        public async Task<IActionResult> Index()
//        {
//            if (HttpContext.Session.GetString("IsLoggedIn") != "true")
//                return RedirectToAction("Login", "Account");

//            var meetings = await _meetingService.GetAllMeetingsAsync();
//            return View(meetings);
//        }
//    }
//}
using CHG_Legal.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CHG_Legal.Controllers
{
    public class ReportsController : Controller
    {
        private readonly ICompanyService _companyService;

        public ReportsController(ICompanyService companyService)
        {
            _companyService = companyService;
        }

        public async Task<IActionResult> Index()
        {
            if (HttpContext.Session.GetString("IsLoggedIn") != "true")
                return RedirectToAction("Login", "Account");

            var companies = await _companyService.GetAllCompaniesAsync();
            return View(companies);
        }
    }
}