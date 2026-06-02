using CHG_Legal.Models;
using CHG_Legal.Models.ViewModels;
using CHG_Legal.Services;
using CHG_Legal.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CHG_Legal.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAuthService _authService;
        private readonly AppDbContext _context;

        public AccountController(IAuthService authService, AppDbContext context)
        {
            _authService = authService;
            _context = context;
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (HttpContext.Session.GetString("IsLoggedIn") == "true")
                return RedirectToAction("Index", "Dashboard");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _authService.ValidateUserAsync(model.Username, model.Password);
            if (user != null)
            {
                var role = await _authService.GetUserRoleAsync(user.User_ID);

                HttpContext.Session.SetString("IsLoggedIn", "true");
                HttpContext.Session.SetString("Username", user.User_Name);
                HttpContext.Session.SetString("UserId", user.User_ID.ToString());
                HttpContext.Session.SetString("UserRole", role ?? "User");

              
                if (role == "Admin")
                    return RedirectToAction("Index", "Dashboard");   //"Admin", new { area = "" }
                else
                    return RedirectToAction("Index", "Dashboard");
            }

            ModelState.AddModelError(string.Empty, "اسم المستخدم أو كلمة السر غير صحيحة");
            return View(model);
        }

        [HttpGet]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword([FromForm] string currentPassword, [FromForm] string newPassword)
        {
            try
            {
                

                if (string.IsNullOrEmpty(currentPassword))
                {
                    return BadRequest(new { message = "كلمة السر الحالية مطلوبة" });
                }

                // Validate new password strength
                if (!PasswordValidator.IsValid(newPassword, out string passwordError))
                {
                    return BadRequest(new { message = passwordError });
                }

                var userId = HttpContext.Session.GetString("UserId");

                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "يجب تسجيل الدخول أولاً" });
                }

                var userIdInt = int.Parse(userId);

                var success = await _authService.ChangePasswordAsync(userIdInt, currentPassword, newPassword);

                if (success)
                {
                    return Ok(new { message = "تم تغيير كلمة السر بنجاح" });
                }

                return BadRequest(new { message = "كلمة السر الحالية غير صحيحة" });
            }
            catch (Exception ex)
            {
              
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}