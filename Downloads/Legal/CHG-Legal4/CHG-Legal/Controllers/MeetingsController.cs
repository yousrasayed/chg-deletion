using CHG_Legal.Models.Entities;
using CHG_Legal.Models.ViewModels;
using CHG_Legal.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CHG_Legal.Controllers
{
    public class MeetingsController : Controller
    {
        private readonly IMeetingService _meetingService;

        public MeetingsController(IMeetingService meetingService)
        {
            _meetingService = meetingService;
        }

        public async Task<IActionResult> Index(int pageNumber = 1, int pageSize = 5, string type = "", string about = "", string subject = "", string date = "")
        {
            if (HttpContext.Session.GetString("IsLoggedIn") != "true")
                return RedirectToAction("Login", "Account");

          

            ViewBag.Hospitals = await _meetingService.GetAllHospitalsAsync();
            ViewBag.BoardTypes = await _meetingService.GetAllBoardTypesAsync();
            ViewBag.AllAttendees = await _meetingService.GetAllAttendeesAsync();

            // Store filter values in ViewBag
            ViewBag.FilterType = type;
            ViewBag.FilterAbout = about;
            ViewBag.FilterSubject = subject;
            ViewBag.FilterDate = date;

            // Set selected values for dropdowns
            ViewBag.SelectedType = type;
            ViewBag.SelectedAbout = about;
            ViewBag.SelectedSubject = subject;
            ViewBag.SelectedDate = date;

            // Get filtered and paginated meetings
            var paginatedResult = await _meetingService.GetFilteredPaginatedMeetingsAsync(pageNumber, pageSize, type, about, subject, date);

            ViewBag.Meetings = paginatedResult.Items;
            ViewBag.CurrentPage = paginatedResult.CurrentPage;
            ViewBag.TotalPages = paginatedResult.TotalPages;
            ViewBag.TotalItems = paginatedResult.TotalItems;
            ViewBag.PageSize = pageSize;

            return View();
        }
        [HttpGet]
        public async Task<IActionResult> GetMeetings()
        {
            try
            {
                var meetings = await _meetingService.GetAllMeetingsAsync();

                var meetingDTOs = meetings.Select(m => new
                {
                    m.ID,
                    m.BoardDate,
                    m.Subject,
                    m.CHGParty,
                    m.MeetingType,
                    m.MeetingStatus,
                    m.BoardTypeID,
                    m.Notes,
                    BoardType = m.BoardType == null ? null : new
                    {
                        m.BoardType.ID,
                        m.BoardType.BoardTypeName
                    },
                    User = m.User == null ? null : new
                    {
                        m.User.User_ID,
                        m.User.User_Name
                    },
                    BoardAttendees = m.BoardAttendees?.Select(ba => new
                    {
                        ba.ID,
                        ba.BoardID,
                        ba.Attendee_ID,
                        Attendee = ba.Attendee == null ? null : new
                        {
                            ba.Attendee.ID,
                            ba.Attendee.Name
                        }
                    }),
                    Approvals = m.Approvals?.Select(a => new
                    {
                        a.ApprovalID,
                        a.BoardApprovalID,
                        a.Attendee_ID,
                        Attendee = a.Attendee == null ? null : new
                        {
                            a.Attendee.ID,
                            a.Attendee.Name
                        }
                    }),
                    BoardAttachments = m.BoardAttachments?.Select(ba => new
                    {
                        ba.ID,
                        ba.file_name,
                        ba.file_path,
                        ba.file_type,
                        ba.file_size
                    }),
                    BoardDecisions = m.BoardDecisions?.Select(bd => new
                    {
                        bd.ID,
                        bd.Decision_Number,
                        bd.Decision_Details,
                        bd.Notes,
                        bd.IsExecuted,
                        bd.DateInserted
                    })
                }).ToList();

                return Json(meetingDTOs);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ERROR: {ex.Message}");
                return Json(new List<object>());
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetMeetingsPaginated(int pageNumber = 1, int pageSize = 5, string type = "", string about = "", string subject = "", string date = "")
        {
            try
            {
                var paginatedResult = await _meetingService.GetFilteredPaginatedMeetingsAsync(pageNumber, pageSize, type, about, subject, date);

                var meetingDTOs = paginatedResult.Items.Select(m => new
                {
                    m.ID,
                    m.BoardDate,
                    m.Subject,
                    m.CHGParty,
                    m.MeetingType,
                    m.MeetingStatus,
                    m.BoardTypeID,
                    m.Notes,
                    BoardType = m.BoardType == null ? null : new
                    {
                        m.BoardType.ID,
                        m.BoardType.BoardTypeName
                    },
                    User = m.User == null ? null : new
                    {
                        m.User.User_ID,
                        m.User.User_Name
                    },
                    BoardAttendees = m.BoardAttendees?.Select(ba => new
                    {
                        ba.ID,
                        ba.BoardID,
                        ba.Attendee_ID,
                        Attendee = ba.Attendee == null ? null : new
                        {
                            ba.Attendee.ID,
                            ba.Attendee.Name
                        }
                    }),
                    Approvals = m.Approvals?.Select(a => new
                    {
                        a.ApprovalID,
                        a.BoardApprovalID,
                        a.Attendee_ID,
                        Attendee = a.Attendee == null ? null : new
                        {
                            a.Attendee.ID,
                            a.Attendee.Name
                        }
                    }),
                    BoardAttachments = m.BoardAttachments?.Select(ba => new
                    {
                        ba.ID,
                        ba.file_name,
                        ba.file_path,
                        ba.file_type,
                        ba.file_size
                    }),
                    BoardDecisions = m.BoardDecisions?.Select(bd => new
                    {
                        bd.ID,
                        bd.Decision_Number,
                        bd.Decision_Details,
                        bd.Notes,
                        bd.IsExecuted,
                        bd.DateInserted
                    })
                }).ToList();

                var result = new
                {
                    meetings = meetingDTOs,
                    currentPage = paginatedResult.CurrentPage,
                    totalPages = paginatedResult.TotalPages,
                    totalItems = paginatedResult.TotalItems,
                    pageSize = paginatedResult.PageSize,
                    hasPreviousPage = paginatedResult.HasPreviousPage,
                    hasNextPage = paginatedResult.HasNextPage
                };

                return Json(result);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ERROR: {ex.Message}");
                return Json(new { error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAttendeesByHospitals(string hospitalIds)
        {
            var ids = hospitalIds.Split(',').Select(int.Parse).ToList();
            var attendees = await _meetingService.GetAttendeesByHospitalsAsync(ids);
            return Json(attendees);
        }

        [HttpGet]
        public async Task<IActionResult> GetMeetingForEdit(int id)
        {
            var meeting = await _meetingService.GetMeetingByIdAsync(id);
            if (meeting == null) return NotFound();

            var meetingDto = new
            {
                meeting.ID,
                meeting.BoardDate,
                meeting.BoardTypeID,
                meeting.Subject,
                meeting.MeetingStatus,
                meeting.MeetingType,
                meeting.CHGParty,
                meeting.Notes,
                AttendeeIds = meeting.BoardAttendees?.Select(ba => ba.Attendee_ID).ToList() ?? new List<int>(),
                ApprovalIds = meeting.Approvals?.Select(a => a.Attendee_ID).ToList() ?? new List<int>(),
                Decisions = meeting.BoardDecisions?.Select(bd => (dynamic)new
                {
                    bd.Decision_Number,
                    bd.Decision_Details,
                    bd.Notes,
                    bd.IsExecuted
                }).ToList() ?? new List<dynamic>(),
                ExistingAttachments = meeting.BoardAttachments?.Select(a => (dynamic)new
                {
                    a.ID,
                    a.file_name,
                    a.file_path,
                    a.file_size,
                    a.description,
                    a.uploaded_at
                }).ToList() ?? new List<dynamic>()
            };

            return Json(meetingDto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddMeeting([FromForm] MeetingViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    return BadRequest(new { errors = errors });
                }

                var userId = int.Parse(HttpContext.Session.GetString("UserId") ?? "0");
                if (userId == 0)
                    return Unauthorized(new { error = "User not logged in" });

                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var meeting = await _meetingService.AddMeetingAsync(model, uploadsFolder, userId);
                return Ok(new { id = meeting.ID, message = "Meeting added successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateMeeting([FromForm] MeetingViewModel model)
        {
            try
            {
                if (!ModelState.IsValid || !model.Id.HasValue)
                    return BadRequest(ModelState);

                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                var success = await _meetingService.UpdateMeetingAsync(model, uploadsFolder);

                if (success)
                {
                    var updatedMeeting = await _meetingService.GetMeetingByIdAsync(model.Id.Value);
                    return Ok(updatedMeeting);
                }

                return NotFound(new { error = "Meeting not found" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAttachment(int id)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"=== DeleteAttachment called for ID: {id} ===");

                var success = await _meetingService.DeleteAttachmentAsync(id);
                if (success)
                {
                    System.Diagnostics.Debug.WriteLine($"Attachment {id} deleted successfully");
                    return Ok(new { success = true });
                }

                System.Diagnostics.Debug.WriteLine($"Attachment {id} not found");
                return NotFound(new { error = "Attachment not found" });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in DeleteAttachment: {ex.Message}");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> DownloadAttachments(int id)
        {
            var zipBytes = await _meetingService.GetMeetingAttachmentsZipAsync(id);
            if (zipBytes == null)
                return NotFound();

            return File(zipBytes, "application/zip", $"Meeting_{id}_Attachments.zip");
        }

        [HttpGet]
        public async Task<IActionResult> PrintAttachments(int id)
        {
            var meeting = await _meetingService.GetMeetingByIdAsync(id);
            if (meeting == null)
                return NotFound();

            return View(meeting);
        }

        [HttpGet]
        public async Task<IActionResult> DeleteMeeting(int id)
        {
            try
            {
                var success = await _meetingService.DeleteMeetingAsync(id);
                if (success)
                    return Ok(new { success = true, message = "Meeting deleted successfully" });
                return NotFound(new { success = false, message = "Meeting not found" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }
    }
}