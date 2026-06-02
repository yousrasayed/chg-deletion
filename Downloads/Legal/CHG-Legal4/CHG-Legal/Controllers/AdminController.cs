using CHG_Legal.Models;
using CHG_Legal.Models.Entities;
using CHG_Legal.Models.ViewModels;
using CHG_Legal.Services;
using CHG_Legal.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CHG_Legal.Controllers
{
    public class AdminController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IAuthService _authService;

        public AdminController(IAuthService authService, AppDbContext context)
        {
            _authService = authService;
            _context = context;
        }

        // Check if user is admin
        private bool IsAdmin()
        {
            var role = HttpContext.Session.GetString("UserRole");
            return role == "Admin";
        }

        // Admin Dashboard (placeholder)
        public IActionResult Index()
        {
            if (HttpContext.Session.GetString("IsLoggedIn") != "true" || !IsAdmin())
                return RedirectToAction("Login", "Account");

            return RedirectToAction("Users");
        }

        // ==================== USERS MANAGEMENT ====================

        [HttpGet]
        public async Task<IActionResult> Users()
        {
            if (HttpContext.Session.GetString("IsLoggedIn") != "true" || !IsAdmin())
                return RedirectToAction("Login", "Account");

            var users = await _context.Users
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .Select(u => new UserViewModel
                {
                    User_ID = u.User_ID,
                    User_Name = u.User_Name,
                    Active = u.Active,
                    RoleID = u.UserRoles.FirstOrDefault() != null ? u.UserRoles.FirstOrDefault().RoleID : 2,
                    RoleName = u.UserRoles.FirstOrDefault() != null ? u.UserRoles.FirstOrDefault().Role.RoleName : "User"
                })
                .ToListAsync();

            ViewBag.Roles = await _context.Roles.ToListAsync();
            return View(users);
        }

        [HttpGet]
        public async Task<IActionResult> GetUser(int id)
        {
            var user = await _context.Users
                .Include(u => u.UserRoles)
                .FirstOrDefaultAsync(u => u.User_ID == id);

            if (user == null)
                return NotFound();

            var userVm = new UserViewModel
            {
                User_ID = user.User_ID,
                User_Name = user.User_Name,
                Active = user.Active,
                RoleID = user.UserRoles.FirstOrDefault()?.RoleID ?? 2
            };

            return Json(userVm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddUser([FromBody] UserViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new { errors = GetModelStateErrors() });

                if (string.IsNullOrWhiteSpace(model.User_Name))
                    return BadRequest(new { message = "اسم المستخدم مطلوب" });

                // Validate password strength
                if (!PasswordValidator.IsValid(model.Password, out string passwordError))
                {
                    return BadRequest(new { message = passwordError });
                }

                // Check if username exists
                var existingUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.User_Name == model.User_Name);

                if (existingUser != null)
                    return BadRequest(new { message = "اسم المستخدم موجود بالفعل" });

                var user = new User
                {
                    User_Name = model.User_Name,
                    Password = model.Password,
                    Active = model.Active
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                // Add UserRole
                var userRole = new UserRole
                {
                    UserID = user.User_ID,
                    RoleID = model.RoleID
                };
                _context.UserRole.Add(userRole);
                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "تم إضافة المستخدم بنجاح" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateUser([FromBody] UserViewModel model)
        {
            try
            {
                if (!model.User_ID.HasValue)
                    return BadRequest(new { message = "معرف المستخدم مطلوب" });

                var user = await _context.Users
                    .Include(u => u.UserRoles)
                    .FirstOrDefaultAsync(u => u.User_ID == model.User_ID);

                if (user == null)
                    return NotFound(new { message = "المستخدم غير موجود" });

                // Check if username exists for another user
                var existingUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.User_Name == model.User_Name && u.User_ID != model.User_ID);

                if (existingUser != null)
                    return BadRequest(new { message = "اسم المستخدم موجود بالفعل" });

                user.User_Name = model.User_Name;
                user.Active = model.Active;

                if (!string.IsNullOrEmpty(model.Password))
                {
                    // Validate password strength if provided
                    if (!PasswordValidator.IsValid(model.Password, out string passwordError))
                    {
                        return BadRequest(new { message = passwordError });
                    }
                    user.Password = model.Password;
                }

                // Update role
                var userRole = user.UserRoles.FirstOrDefault();
                if (userRole != null)
                {
                    userRole.RoleID = model.RoleID;
                }

                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "تم تحديث المستخدم بنجاح" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(int id)
        {
            try
            {
                var user = await _context.Users
                    .Include(u => u.UserRoles)
                    .FirstOrDefaultAsync(u => u.User_ID == id);

                if (user == null)
                    return NotFound(new { message = "المستخدم غير موجود" });

                // Don't allow deleting current user
                var currentUserId = int.Parse(HttpContext.Session.GetString("UserId") ?? "0");
                if (user.User_ID == currentUserId)
                    return BadRequest(new { message = "لا يمكن حذف المستخدم الحالي" });

                _context.UserRole.RemoveRange(user.UserRoles);
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "تم حذف المستخدم بنجاح" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // ==================== MEETING TYPES MANAGEMENT ====================

        [HttpGet]
        public async Task<IActionResult> MeetingTypes()
        {
            if (HttpContext.Session.GetString("IsLoggedIn") != "true" || !IsAdmin())
                return RedirectToAction("Login", "Account");

            var types = await _context.BoardTypes.ToListAsync();
            return View(types);
        }

        [HttpGet]
        public async Task<IActionResult> GetMeetingType(int id)
        {
            var type = await _context.BoardTypes.FindAsync(id);
            if (type == null)
                return NotFound();

            return Json(type);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddMeetingType([FromBody] MeetingTypeViewModel model)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(model.BoardTypeName))
                    return BadRequest(new { message = "اسم نوع الاجتماع مطلوب" });

                var existingType = await _context.BoardTypes
                    .FirstOrDefaultAsync(t => t.BoardTypeName == model.BoardTypeName);

                if (existingType != null)
                    return BadRequest(new { message = "نوع الاجتماع موجود بالفعل" });

                var type = new BoardType
                {
                    BoardTypeName = model.BoardTypeName
                };

                _context.BoardTypes.Add(type);
                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "تم إضافة نوع الاجتماع بنجاح" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateMeetingType([FromBody] MeetingTypeViewModel model)
        {
            try
            {
                if (!model.ID.HasValue)
                    return BadRequest(new { message = "المعرف مطلوب" });

                if (string.IsNullOrWhiteSpace(model.BoardTypeName))
                    return BadRequest(new { message = "اسم نوع الاجتماع مطلوب" });

                var type = await _context.BoardTypes.FindAsync(model.ID.Value);
                if (type == null)
                    return NotFound(new { message = "نوع الاجتماع غير موجود" });

                // Check if name exists for another type
                var existingType = await _context.BoardTypes
                    .FirstOrDefaultAsync(t => t.BoardTypeName == model.BoardTypeName && t.ID != model.ID);

                if (existingType != null)
                    return BadRequest(new { message = "نوع الاجتماع موجود بالفعل" });

                type.BoardTypeName = model.BoardTypeName;
                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "تم تحديث نوع الاجتماع بنجاح" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteMeetingType(int id)
        {
            try
            {
                var type = await _context.BoardTypes.FindAsync(id);
                if (type == null)
                    return NotFound(new { message = "نوع الاجتماع غير موجود" });

                // Check if type is used in any meeting
                var isUsed = await _context.Boards.AnyAsync(b => b.BoardTypeID == id);
                if (isUsed)
                    return BadRequest(new { message = "لا يمكن حذف نوع الاجتماع المستخدم في اجتماعات" });

                _context.BoardTypes.Remove(type);
                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "تم حذف نوع الاجتماع بنجاح" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        private List<string> GetModelStateErrors()
        {
            return ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();
        }
    }
}