// Controllers/CompaniesController.cs
using CHG_Legal.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using CHG_Legal.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
namespace CHG_Legal.Controllers
{
    public class CompaniesController : Controller
    {
        private readonly ICompanyService _companyService;
        private readonly ILogger<CompaniesController> _logger;
        private readonly IAttachmentService _attachmentService;

        public CompaniesController(
     ICompanyService companyService,
     IAttachmentService attachmentService, 
     ILogger<CompaniesController> logger)
        {
            _companyService = companyService;
            _attachmentService = attachmentService;  
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var companies = await _companyService.GetAllCompaniesAsync();
                return View(companies);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Index");
                return View(new List<CompanyViewModel>());
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetCompanies()
        {
            try
            {
                var companies = await _companyService.GetAllCompaniesAsync();
                return Ok(companies);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting companies");
                return StatusCode(500, new { success = false, message = $"حدث خطأ: {ex.Message}" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetCompany(int id)
        {
            try
            {
                var company = await _companyService.GetCompanyByIdAsync(id);
                if (company == null)
                    return NotFound(new { success = false, message = "الشركة غير موجودة" });

                return Ok(company);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting company {Id}", id);
                return StatusCode(500, new { success = false, message = $"حدث خطأ: {ex.Message}" });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveCompany([FromBody] CompanyViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new { success = false, message = "بيانات غير صحيحة", errors = ModelState });

                CompanyViewModel result;
                if (model.CompanyId > 0)
                {
                    result = await _companyService.UpdateCompanyAsync(model);
                }
                else
                {
                    result = await _companyService.CreateCompanyAsync(model);
                }

                return Ok(new { success = true, message = "تم حفظ الشركة بنجاح", companyId = result.CompanyId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving company");
                return StatusCode(500, new { success = false, message = $"حدث خطأ أثناء حفظ الشركة: {ex.Message}" });
            }
        }

        [HttpDelete]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCompany(int id)
        {
            try
            {
                var result = await _companyService.DeleteCompanyAsync(id);
                if (!result)
                    return NotFound(new { success = false, message = "الشركة غير موجودة" });

                return Ok(new { success = true, message = "تم حذف الشركة بنجاح" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting company {Id}", id);
                return StatusCode(500, new { success = false, message = $"حدث خطأ أثناء حذف الشركة: {ex.Message}" });
            }
        }
        [HttpGet]
        public async Task<IActionResult> GetShareValue()
        {
            try
            {
                var shareValue = await _companyService.GetCurrentShareValueAsync();
                return Ok(new { shareValue = shareValue ?? 1.0 });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting share value");
                return StatusCode(500, new { success = false, message = $"حدث خطأ: {ex.Message}" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetBankGroups()
        {
            try
            {
                var groups = await _companyService.GetBankGroupsAsync();
                return Ok(groups);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting bank groups");
                return StatusCode(500, new { success = false, message = $"حدث خطأ: {ex.Message}" });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult PrintCompanyDetails(string companyName, string allDetailsHtml)
        {
            ViewBag.CompanyName = companyName;
            ViewBag.AllDetailsHtml = allDetailsHtml;
            return View("_PrintCompanyDetails");
        }


        // ========== COMPANY ATTACHMENTS ACTIONS ==========
        // Helper method to get current user ID
        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(userIdClaim, out int userId))
                return userId;
            return 1; 
        }

        // ========== COMPANY ATTACHMENTS ACTIONS ==========

        [HttpGet]
        public async Task<IActionResult> ManageAttachments(int id, string activeTab = "company")
        {
            var company = await _companyService.GetCompanyByIdAsync(id);
            if (company == null)
                return NotFound();

            ViewBag.CompanyId = id;
            ViewBag.CompanyName = company.Name;
            ViewBag.ActiveTab = activeTab;

            ViewBag.CommercialRegNo = company.CommercialRegNo ?? "-";
            ViewBag.AuthorizedCapital = company.AuthorizedCapital?.ToString("N2") ?? "0";
            ViewBag.RegistrationDate = company.RegistrationDate?.ToString("yyyy/MM/dd") ?? "-";

            // Load all shareholders for the dropdown
            var shareholders = company.Shareholders ?? new List<ShareholderViewModel>();
            ViewBag.Shareholders = shareholders;
            ViewBag.ActiveShareholdersCount = shareholders.Count(s => s.EndDate == null);
            ViewBag.ExitedShareholdersCount = shareholders.Count(s => s.EndDate != null);

            // Load all branches for the dropdown
            var branches = company.Branches ?? new List<BranchViewModel>();
            ViewBag.Branches = branches;

            // Load all board members for the dropdown
            var boardMembers = company.BoardMembers ?? new List<BoardMemberViewModel>();
            ViewBag.BoardMembers = boardMembers;

            if (company.BoardSettings != null)
            {
                ViewBag.BoardDuration = company.BoardSettings.Duration ?? "-";
                ViewBag.BoardStartDate = company.BoardSettings.StartDate?.ToString("yyyy/MM") ?? "-";
            }
            else
            {
                ViewBag.BoardDuration = "-";
                ViewBag.BoardStartDate = "-";
            }

            return View("ManageAttachments");
        }
        [HttpGet]
        public async Task<IActionResult> GetCompanyAttachments(int companyId)
        {
            try
            {
                var attachments = await _attachmentService.GetCompanyAttachmentsAsync(companyId);
                return Ok(attachments ?? new List<AttachmentViewModel>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting company attachments");
                return Ok(new List<AttachmentViewModel>());
            }
        }

        [HttpPost]
        public async Task<IActionResult> UploadCompanyAttachment(int companyId, IFormFile file, string description)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return BadRequest(new { success = false, message = "يرجى اختيار ملف" });

                var userId = GetCurrentUserId();
                var attachment = await _attachmentService.AddCompanyAttachmentAsync(companyId, file, description, userId);

                return Ok(new { success = true, message = "تم رفع المرفق بنجاح", attachment });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading company attachment");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteCompanyAttachment(int id)
        {
            try
            {
                var result = await _attachmentService.DeleteCompanyAttachmentAsync(id);
                if (!result)
                    return NotFound(new { success = false, message = "المرفق غير موجود" });

                return Ok(new { success = true, message = "تم حذف المرفق بنجاح" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting company attachment");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> DownloadCompanyAttachment(int id)
        {
            try
            {
                var (fileData, fileName, contentType) = await _attachmentService.DownloadCompanyAttachmentAsync(id);
                if (fileData == null)
                    return NotFound(new { success = false, message = "الملف غير موجود" });

                return File(fileData, contentType, fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading company attachment");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // ========== SHAREHOLDER ATTACHMENTS ACTIONS ==========

        [HttpGet]
        public async Task<IActionResult> GetShareholderAttachments(int shareholderId)
        {
            try
            {
                var attachments = await _attachmentService.GetShareholderAttachmentsAsync(shareholderId);
                return Ok(attachments ?? new List<AttachmentViewModel>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting shareholder attachments");
                return Ok(new List<AttachmentViewModel>());
            }
        }

        [HttpPost]
        public async Task<IActionResult> UploadShareholderAttachment(int shareholderId, IFormFile file, string description)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return BadRequest(new { success = false, message = "يرجى اختيار ملف" });

                var userId = GetCurrentUserId();
                var attachment = await _attachmentService.AddShareholderAttachmentAsync(shareholderId, file, description, userId);

                return Ok(new { success = true, message = "تم رفع المرفق بنجاح", attachment });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading shareholder attachment");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteShareholderAttachment(int id)
        {
            try
            {
                var result = await _attachmentService.DeleteShareholderAttachmentAsync(id);
                if (!result)
                    return NotFound(new { success = false, message = "المرفق غير موجود" });

                return Ok(new { success = true, message = "تم حذف المرفق بنجاح" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting shareholder attachment");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> DownloadShareholderAttachment(int id)
        {
            try
            {
                var (fileData, fileName, contentType) = await _attachmentService.DownloadShareholderAttachmentAsync(id);
                if (fileData == null)
                    return NotFound(new { success = false, message = "الملف غير موجود" });

                return File(fileData, contentType, fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading shareholder attachment");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // ========== BRANCH ATTACHMENTS ACTIONS ==========

        [HttpGet]
        public async Task<IActionResult> GetBranchAttachments(int branchId)
        {
            try
            {
                var attachments = await _attachmentService.GetBranchAttachmentsAsync(branchId);
                return Ok(attachments ?? new List<AttachmentViewModel>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting branch attachments");
                return Ok(new List<AttachmentViewModel>());
            }
        }

        [HttpPost]
        public async Task<IActionResult> UploadBranchAttachment(int branchId, IFormFile file, string description)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return BadRequest(new { success = false, message = "يرجى اختيار ملف" });

                var userId = GetCurrentUserId();
                var attachment = await _attachmentService.AddBranchAttachmentAsync(branchId, file, description, userId);

                return Ok(new { success = true, message = "تم رفع المرفق بنجاح", attachment });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading branch attachment");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteBranchAttachment(int id)
        {
            try
            {
                var result = await _attachmentService.DeleteBranchAttachmentAsync(id);
                if (!result)
                    return NotFound(new { success = false, message = "المرفق غير موجود" });

                return Ok(new { success = true, message = "تم حذف المرفق بنجاح" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting branch attachment");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> DownloadBranchAttachment(int id)
        {
            try
            {
                var (fileData, fileName, contentType) = await _attachmentService.DownloadBranchAttachmentAsync(id);
                if (fileData == null)
                    return NotFound(new { success = false, message = "الملف غير موجود" });

                return File(fileData, contentType, fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading branch attachment");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // ========== BOARD MEMBER ATTACHMENTS ACTIONS ==========

        [HttpGet]
        public async Task<IActionResult> GetBoardMemberAttachments(int memberId)
        {
            try
            {
                var attachments = await _attachmentService.GetBoardMemberAttachmentsAsync(memberId);
                return Ok(attachments ?? new List<AttachmentViewModel>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting board member attachments");
                return Ok(new List<AttachmentViewModel>());
            }
        }

        [HttpPost]
        public async Task<IActionResult> UploadBoardMemberAttachment(int memberId, IFormFile file, string description)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return BadRequest(new { success = false, message = "يرجى اختيار ملف" });

                var userId = GetCurrentUserId();
                var attachment = await _attachmentService.AddBoardMemberAttachmentAsync(memberId, file, description, userId);

                return Ok(new { success = true, message = "تم رفع المرفق بنجاح", attachment });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading board member attachment");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteBoardMemberAttachment(int id)
        {
            try
            {
                var result = await _attachmentService.DeleteBoardMemberAttachmentAsync(id);
                if (!result)
                    return NotFound(new { success = false, message = "المرفق غير موجود" });

                return Ok(new { success = true, message = "تم حذف المرفق بنجاح" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting board member attachment");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> DownloadBoardMemberAttachment(int id)
        {
            try
            {
                var (fileData, fileName, contentType) = await _attachmentService.DownloadBoardMemberAttachmentAsync(id);
                if (fileData == null)
                    return NotFound(new { success = false, message = "الملف غير موجود" });

                return File(fileData, contentType, fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading board member attachment");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // ========== BANKING ATTACHMENTS ACTIONS ==========

        [HttpGet]
        public async Task<IActionResult> GetBankingAttachments(int companyId)
        {
            try
            {
                var attachments = await _attachmentService.GetBankingAttachmentsAsync(companyId);
                return Ok(attachments ?? new List<AttachmentViewModel>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting banking attachments");
                return Ok(new List<AttachmentViewModel>());
            }
        }

        [HttpPost]
        public async Task<IActionResult> UploadBankingAttachment(int companyId, IFormFile file, string description)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return BadRequest(new { success = false, message = "يرجى اختيار ملف" });

                var userId = GetCurrentUserId();
                var attachment = await _attachmentService.AddBankingAttachmentAsync(companyId, file, description, userId);

                return Ok(new { success = true, message = "تم رفع المرفق بنجاح", attachment });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading banking attachment");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteBankingAttachment(int id)
        {
            try
            {
                var result = await _attachmentService.DeleteBankingAttachmentAsync(id);
                if (!result)
                    return NotFound(new { success = false, message = "المرفق غير موجود" });

                return Ok(new { success = true, message = "تم حذف المرفق بنجاح" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting banking attachment");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> DownloadBankingAttachment(int id)
        {
            try
            {
                var (fileData, fileName, contentType) = await _attachmentService.DownloadBankingAttachmentAsync(id);
                if (fileData == null)
                    return NotFound(new { success = false, message = "الملف غير موجود" });

                return File(fileData, contentType, fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading banking attachment");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // ========== NON-BANKING ATTACHMENTS ACTIONS ==========

        [HttpGet]
        public async Task<IActionResult> GetNonBankingAttachments(int companyId)
        {
            try
            {
                var attachments = await _attachmentService.GetNonBankingAttachmentsAsync(companyId);
                return Ok(attachments ?? new List<AttachmentViewModel>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting non-banking attachments");
                return Ok(new List<AttachmentViewModel>());
            }
        }

        [HttpPost]
        public async Task<IActionResult> UploadNonBankingAttachment(int companyId, IFormFile file, string description)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return BadRequest(new { success = false, message = "يرجى اختيار ملف" });

                var userId = GetCurrentUserId();
                var attachment = await _attachmentService.AddNonBankingAttachmentAsync(companyId, file, description, userId);

                return Ok(new { success = true, message = "تم رفع المرفق بنجاح", attachment });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading non-banking attachment");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteNonBankingAttachment(int id)
        {
            try
            {
                var result = await _attachmentService.DeleteNonBankingAttachmentAsync(id);
                if (!result)
                    return NotFound(new { success = false, message = "المرفق غير موجود" });

                return Ok(new { success = true, message = "تم حذف المرفق بنجاح" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting non-banking attachment");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> DownloadNonBankingAttachment(int id)
        {
            try
            {
                var (fileData, fileName, contentType) = await _attachmentService.DownloadNonBankingAttachmentAsync(id);
                if (fileData == null)
                    return NotFound(new { success = false, message = "الملف غير موجود" });

                return File(fileData, contentType, fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading non-banking attachment");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }
    }
}