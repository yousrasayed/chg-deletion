using CHG_Legal.Models;
using CHG_Legal.Models.Entities;
using CHG_Legal.Models.ViewModels;
using CHG_Legal.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.IO.Compression;

namespace CHG_Legal.Services.Implementations
{
    public class MeetingService : IMeetingService
    {
        private readonly AppDbContext _context;

        public MeetingService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<PaginatedResult<Board>> GetFilteredPaginatedMeetingsAsync(int pageNumber, int pageSize, string type, string about, string subject, string date)
        {
           

            // Start with base query
            var query = _context.Boards.AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(type))
            {
                query = query.Where(m => m.MeetingType != null && m.MeetingType == type);
            }

            if (!string.IsNullOrEmpty(about))
            {
                query = query.Where(m => m.CHGParty != null && m.CHGParty.Contains(about));
            }

            if (!string.IsNullOrEmpty(subject))
            {
                query = query.Where(m => m.Subject != null && m.Subject.Contains(subject));
            }

            // FIXED: Date filter - compare date only, ignore time
            if (!string.IsNullOrEmpty(date))
            {
                if (DateTime.TryParse(date, out DateTime filterDate))
                {
                    var startDate = filterDate.Date;
                    var endDate = startDate.AddDays(1);
                    query = query.Where(m => m.BoardDate >= startDate && m.BoardDate < endDate);
                }
            }

            // Get total count
            var totalItems = await query.CountAsync();

            // Calculate total pages
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            // Ensure page number is valid
            if (totalPages > 0 && pageNumber > totalPages)
                pageNumber = totalPages;
            if (pageNumber < 1) pageNumber = 1;

            // Get paginated data
            var items = await query
                .AsNoTracking()
                .AsSplitQuery()
                .Include(b => b.BoardType)
                .Include(b => b.User)
                .Include(b => b.BoardAttendees)
                    .ThenInclude(ba => ba.Attendee)
                .Include(b => b.Approvals)
                    .ThenInclude(a => a.Attendee)
                .Include(b => b.BoardAttachments)
                .Include(b => b.BoardDecisions)
                .OrderByDescending(b => b.BoardDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Handle NULL values
            foreach (var board in items)
            {
                board.Subject ??= "بدون موضوع";
                board.MeetingType ??= "مجلس إدارة";
                board.MeetingStatus ??= "pending";
                board.CHGParty ??= "";
                board.Notes ??= "";
                board.Attendees ??= "";
                board.BoardDecisionsText ??= "";

            }

            return new PaginatedResult<Board>
            {
                Items = items,
                CurrentPage = pageNumber,
                TotalPages = totalPages,
                TotalItems = totalItems,
                PageSize = pageSize
            };
        }        // -----------------------------
        public async Task<List<Board>> GetAllMeetingsAsync()
        {
            var boards = await _context.Boards
                .AsNoTracking()
                .AsSplitQuery()
                .Include(b => b.BoardType)
                .Include(b => b.User)
                .Include(b => b.BoardAttendees)
                    .ThenInclude(ba => ba.Attendee)
                .Include(b => b.Approvals)
                    .ThenInclude(a => a.Attendee)
                .Include(b => b.BoardAttachments)
                .Include(b => b.BoardDecisions)
                .OrderByDescending(b => b.BoardDate)
                .ToListAsync();

            // تعامل مع الـ NULL values
            foreach (var board in boards)
            {
                board.Subject ??= "بدون موضوع";
                board.MeetingType ??= "مجلس إدارة";
                board.MeetingStatus ??= "pending";
                board.CHGParty ??= "";
                board.Notes ??= "";
                board.Attendees ??= "";
                board.BoardDecisionsText ??= "";
            }

            return boards;
        }

        // -----------------------------
        // GET MEETING BY ID (FAST - with split query)
        // -----------------------------
        public async Task<Board?> GetMeetingByIdAsync(int id)
        {
            return await _context.Boards
                .AsSplitQuery()
                .Include(b => b.BoardType)
                .Include(b => b.User)
                .Include(b => b.BoardAttendees)
                    .ThenInclude(ba => ba.Attendee)
                .Include(b => b.Approvals)
                    .ThenInclude(a => a.Attendee)
                .Include(b => b.BoardAttachments)
                .Include(b => b.BoardDecisions)
                .FirstOrDefaultAsync(b => b.ID == id);
        }

        // -----------------------------
        // ADD MEETING (OPTIMIZED)
        // -----------------------------
        public async Task<Board> AddMeetingAsync(MeetingViewModel model, string uploadsFolder, int userId)
        {
            var board = new Board
            {
                BoardDate = model.BoardDate,
                BoardTypeID = model.BoardTypeID,
                Subject = model.Subject,
                MeetingStatus = model.MeetingStatus,
                MeetingType = model.MeetingType,
                CHGParty = model.CHGParty,
                Notes = model.Notes,
                UserID = userId
            };

            _context.Boards.Add(board);
            await _context.SaveChangesAsync();

            // Create folder for this meeting
            var meetingFolder = Path.Combine(uploadsFolder, "meetings", board.ID.ToString());
            if (!Directory.Exists(meetingFolder))
                Directory.CreateDirectory(meetingFolder);

            // Save meeting attachments (OPTIMIZED)
            if (model.MeetingAttachments != null && model.MeetingAttachments.Any())
            {
                foreach (var file in model.MeetingAttachments)
                {
                    if (file.Length == 0) continue;

                    var filePath = Path.Combine(meetingFolder, file.FileName);

                    byte[] fileBytes;
                    using (var memoryStream = new MemoryStream())
                    {
                        await file.CopyToAsync(memoryStream);
                        fileBytes = memoryStream.ToArray();
                    }

                    await File.WriteAllBytesAsync(filePath, fileBytes);

                    _context.BoardAttachments.Add(new BoardAttachment
                    {
                        Board_id = board.ID,
                        user_id = userId,
                        file_name = file.FileName,
                        file_path = $"/uploads/meetings/{board.ID}/{file.FileName}",
                        file_type = Path.GetExtension(file.FileName),
                        file_size = file.Length,
                        mime_type = file.ContentType,
                        file_data = fileBytes,
                        uploaded_at = DateTime.Now
                    });
                }
            }

            // Add attendees (OPTIMIZED - using AddRange)
            if (model.AttendeeIds != null && model.AttendeeIds.Any())
            {
                var attendees = model.AttendeeIds.Select(attendeeId => new BoardAttendee
                {
                    BoardID = board.ID,
                    Attendee_ID = attendeeId,
                    ApprovedBy = null
                });
                _context.BoardAttendees.AddRange(attendees);
            }

            // Add approvals (OPTIMIZED - using AddRange)
            if (model.ApprovalIds != null && model.ApprovalIds.Any())
            {
                var approvals = model.ApprovalIds.Select(approvalId => new Approval
                {
                    BoardApprovalID = board.ID,
                    Attendee_ID = approvalId,
                    ApprovedBy = "System"
                });
                _context.Approvals.AddRange(approvals);
            }

            // Add decisions
            if (model.Decisions != null && model.Decisions.Any())
            {
                foreach (var decisionInput in model.Decisions)
                {
                    if (string.IsNullOrEmpty(decisionInput.Decision_Details)) continue;

                    var decision = new BoardDecision
                    {
                        Board_ID = board.ID,
                        Decision_Number = decisionInput.Decision_Number,
                        Decision_Details = decisionInput.Decision_Details,
                        Notes = decisionInput.Notes,
                        IsExecuted = decisionInput.IsExecuted,
                        DateInserted = DateTime.Now
                    };

                    _context.BoardDecisions.Add(decision);
                    await _context.SaveChangesAsync(); // Need Save to get decision ID

                    // Save decision attachments
                    if (decisionInput.Attachments != null && decisionInput.Attachments.Any())
                    {
                        var decisionFolder = Path.Combine(meetingFolder, "decisions", decision.ID.ToString());
                        if (!Directory.Exists(decisionFolder))
                            Directory.CreateDirectory(decisionFolder);

                        foreach (var file in decisionInput.Attachments)
                        {
                            if (file.Length == 0) continue;

                            var filePath = Path.Combine(decisionFolder, file.FileName);

                            byte[] fileBytes;
                            using (var memoryStream = new MemoryStream())
                            {
                                await file.CopyToAsync(memoryStream);
                                fileBytes = memoryStream.ToArray();
                            }

                            await File.WriteAllBytesAsync(filePath, fileBytes);

                            _context.BoardAttachments.Add(new BoardAttachment
                            {
                                Board_id = board.ID,
                                user_id = userId,
                                file_name = file.FileName,
                                file_path = $"/uploads/meetings/{board.ID}/decisions/{decision.ID}/{file.FileName}",
                                file_type = Path.GetExtension(file.FileName),
                                file_size = file.Length,
                                mime_type = file.ContentType,
                                file_data = fileBytes,
                                description = $"Decision: {decision.Decision_Number}",
                                uploaded_at = DateTime.Now
                            });
                        }
                    }
                }
            }

            await _context.SaveChangesAsync();
            return board;
        }

        // -----------------------------
        // UPDATE MEETING
        // -----------------------------
        public async Task<bool> UpdateMeetingAsync(MeetingViewModel model, string uploadsFolder)
        {
            try
            {
               

                // Get existing meeting with all related data
                var existing = await _context.Boards
                    .Include(b => b.BoardAttendees)
                    .Include(b => b.Approvals)
                    .Include(b => b.BoardAttachments)
                    .Include(b => b.BoardDecisions)
                    .FirstOrDefaultAsync(b => b.ID == model.Id);

                if (existing == null) return false;

                // Update basic information
                existing.BoardDate = model.BoardDate;
                existing.BoardTypeID = model.BoardTypeID;
                existing.Subject = model.Subject;
                existing.MeetingStatus = model.MeetingStatus;
                existing.MeetingType = model.MeetingType;
                existing.CHGParty = model.CHGParty;
                existing.Notes = model.Notes;

                // Update attendees - Remove old and add new (OPTIMIZED)
                _context.BoardAttendees.RemoveRange(existing.BoardAttendees);
                if (model.AttendeeIds != null && model.AttendeeIds.Any())
                {
                    var attendees = model.AttendeeIds.Select(attendeeId => new BoardAttendee
                    {
                        BoardID = existing.ID,
                        Attendee_ID = attendeeId,
                        ApprovedBy = null
                    });
                    _context.BoardAttendees.AddRange(attendees);
                }

                // Update approvals - Remove old and add new (OPTIMIZED)
                _context.Approvals.RemoveRange(existing.Approvals);
                if (model.ApprovalIds != null && model.ApprovalIds.Any())
                {
                    var approvals = model.ApprovalIds.Select(approvalId => new Approval
                    {
                        BoardApprovalID = existing.ID,
                        Attendee_ID = approvalId,
                        ApprovedBy = "System"
                    });
                    _context.Approvals.AddRange(approvals);
                }

                // Handle new meeting attachments (add only, don't delete existing)
                if (model.MeetingAttachments != null && model.MeetingAttachments.Any())
                {
                    var meetingFolder = Path.Combine(uploadsFolder, "meetings", existing.ID.ToString());
                    if (!Directory.Exists(meetingFolder))
                        Directory.CreateDirectory(meetingFolder);

                    foreach (var file in model.MeetingAttachments)
                    {
                        if (file == null || file.Length == 0) continue;

                        var fileName = $"{DateTime.Now.Ticks}_{file.FileName}";
                        var filePath = Path.Combine(meetingFolder, fileName);

                        byte[] fileBytes;
                        using (var memoryStream = new MemoryStream())
                        {
                            await file.CopyToAsync(memoryStream);
                            fileBytes = memoryStream.ToArray();
                        }

                        await File.WriteAllBytesAsync(filePath, fileBytes);

                        _context.BoardAttachments.Add(new BoardAttachment
                        {
                            Board_id = existing.ID,
                            user_id = existing.UserID,
                            file_name = fileName,
                            file_path = $"/uploads/meetings/{existing.ID}/{fileName}",
                            file_type = Path.GetExtension(file.FileName),
                            file_size = file.Length,
                            mime_type = file.ContentType,
                            file_data = fileBytes,
                            uploaded_at = DateTime.Now
                        });
                    }
                }

                // Update decisions - Remove old and add new
                _context.BoardDecisions.RemoveRange(existing.BoardDecisions);
                if (model.Decisions != null && model.Decisions.Any())
                {
                    foreach (var decisionInput in model.Decisions)
                    {
                        if (string.IsNullOrEmpty(decisionInput.Decision_Details)) continue;

                        var decision = new BoardDecision
                        {
                            Board_ID = existing.ID,
                            Decision_Number = decisionInput.Decision_Number,
                            Decision_Details = decisionInput.Decision_Details,
                            Notes = decisionInput.Notes,
                            IsExecuted = decisionInput.IsExecuted,
                            DateInserted = DateTime.Now
                        };

                        _context.BoardDecisions.Add(decision);
                        await _context.SaveChangesAsync(); // Need Save to get decision ID

                        // Handle decision attachments
                        if (decisionInput.Attachments != null && decisionInput.Attachments.Any())
                        {
                            var meetingFolder = Path.Combine(uploadsFolder, "meetings", existing.ID.ToString());
                            var decisionFolder = Path.Combine(meetingFolder, "decisions", decision.ID.ToString());
                            if (!Directory.Exists(decisionFolder))
                                Directory.CreateDirectory(decisionFolder);

                            foreach (var file in decisionInput.Attachments)
                            {
                                if (file == null || file.Length == 0) continue;

                                var fileName = $"{DateTime.Now.Ticks}_{file.FileName}";
                                var filePath = Path.Combine(decisionFolder, fileName);

                                byte[] fileBytes;
                                using (var memoryStream = new MemoryStream())
                                {
                                    await file.CopyToAsync(memoryStream);
                                    fileBytes = memoryStream.ToArray();
                                }

                                await File.WriteAllBytesAsync(filePath, fileBytes);

                                _context.BoardAttachments.Add(new BoardAttachment
                                {
                                    Board_id = existing.ID,
                                    user_id = existing.UserID,
                                    file_name = fileName,
                                    file_path = $"/uploads/meetings/{existing.ID}/decisions/{decision.ID}/{fileName}",
                                    file_type = Path.GetExtension(file.FileName),
                                    file_size = file.Length,
                                    mime_type = file.ContentType,
                                    file_data = fileBytes,
                                    description = $"Decision: {decision.Decision_Number}",
                                    uploaded_at = DateTime.Now
                                });
                            }
                        }
                    }
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        // -----------------------------
        // DELETE MEETING (OPTIMIZED)
        // -----------------------------
        public async Task<bool> DeleteMeetingAsync(int id)
        {
            try
            {

                // Get the meeting with all related data
                var meeting = await _context.Boards
                    .Include(b => b.BoardAttendees)
                    .Include(b => b.Approvals)
                    .Include(b => b.BoardAttachments)
                    .Include(b => b.BoardDecisions)
                    .FirstOrDefaultAsync(b => b.ID == id);

                if (meeting == null)
                {
                    return false;
                }

                // Delete all related data (using RemoveRange for bulk delete)
                if (meeting.BoardAttendees != null && meeting.BoardAttendees.Any())
                    _context.BoardAttendees.RemoveRange(meeting.BoardAttendees);

                if (meeting.Approvals != null && meeting.Approvals.Any())
                    _context.Approvals.RemoveRange(meeting.Approvals);

                // Delete physical files from disk before removing from DB
                if (meeting.BoardAttachments != null && meeting.BoardAttachments.Any())
                {
                    foreach (var attachment in meeting.BoardAttachments)
                    {
                        if (!string.IsNullOrEmpty(attachment.file_path))
                        {
                            var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", attachment.file_path.TrimStart('/'));
                            if (File.Exists(fullPath))
                            {
                                File.Delete(fullPath);
                            }
                        }
                    }
                    _context.BoardAttachments.RemoveRange(meeting.BoardAttachments);
                }

                if (meeting.BoardDecisions != null && meeting.BoardDecisions.Any())
                    _context.BoardDecisions.RemoveRange(meeting.BoardDecisions);

                // Delete the meeting itself
                _context.Boards.Remove(meeting);

                // Save all changes
                await _context.SaveChangesAsync();

                // Delete the meeting folder
                var meetingFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "meetings", id.ToString());
                if (Directory.Exists(meetingFolder))
                {
                    Directory.Delete(meetingFolder, true);
                }

                return true;
            }
            catch (Exception ex)
            {
               
                return false;
            }
        }

        // -----------------------------
        // DOWNLOAD ATTACHMENTS ZIP (OPTIMIZED)
        // -----------------------------
        public async Task<byte[]?> GetMeetingAttachmentsZipAsync(int meetingId)
        {
            var attachments = await _context.BoardAttachments
                .Where(a => a.Board_id == meetingId)
                .ToListAsync();

            if (!attachments.Any()) return null;

            using var memoryStream = new MemoryStream();
            using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
            {
                foreach (var att in attachments)
                {
                    byte[] fileData;

                    if (att.file_data != null)
                    {
                        fileData = att.file_data;
                    }
                    else if (!string.IsNullOrEmpty(att.file_path))
                    {
                        var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", att.file_path.TrimStart('/'));
                        if (File.Exists(fullPath))
                        {
                            fileData = await File.ReadAllBytesAsync(fullPath);
                        }
                        else
                        {
                            continue;
                        }
                    }
                    else
                    {
                        continue;
                    }

                    var entry = archive.CreateEntry(att.file_name);
                    using (var entryStream = entry.Open())
                    {
                        await entryStream.WriteAsync(fileData, 0, fileData.Length);
                    }
                }
            }

            return memoryStream.ToArray();
        }

        // -----------------------------
        // GET MEETING ATTACHMENTS
        // -----------------------------
        public Task<List<BoardAttachment>> GetMeetingAttachmentsAsync(int meetingId)
        {
            return _context.BoardAttachments
                .AsNoTracking()
                .Where(a => a.Board_id == meetingId)
                .ToListAsync();
        }

        // -----------------------------
        // DELETE ATTACHMENT
        // -----------------------------
        public async Task<bool> DeleteAttachmentAsync(int attachmentId)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"=== DeleteAttachmentAsync called for ID: {attachmentId} ===");

                var attachment = await _context.BoardAttachments.FindAsync(attachmentId);
                if (attachment == null)
                {
                    System.Diagnostics.Debug.WriteLine($"Attachment {attachmentId} not found in database");
                    return false;
                }

                System.Diagnostics.Debug.WriteLine($"Attachment found: {attachment.file_name}, Path: {attachment.file_path}");

                // Delete physical file from disk
                if (!string.IsNullOrEmpty(attachment.file_path))
                {
                    var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", attachment.file_path.TrimStart('/'));
                    System.Diagnostics.Debug.WriteLine($"Full file path: {fullPath}");

                    if (File.Exists(fullPath))
                    {
                        File.Delete(fullPath);
                        System.Diagnostics.Debug.WriteLine($"Deleted physical file: {fullPath}");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"File not found: {fullPath}");
                    }
                }

                _context.BoardAttachments.Remove(attachment);
                await _context.SaveChangesAsync();

                System.Diagnostics.Debug.WriteLine($"Attachment {attachmentId} removed from database");
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in DeleteAttachmentAsync: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                return false;
            }
        }
        // -----------------------------
        // GET DASHBOARD DATA
        // -----------------------------
        //public async Task<DashboardViewModel> GetDashboardDataAsync()
        //{
        //    var meetings = await GetAllMeetingsAsync();

        //    // Get all BoardTypes from database
        //    var allBoardTypes = await _context.BoardTypes.ToListAsync();

        //    // Get all distinct years
        //    var years = meetings.Select(m => m.BoardDate.Year).Distinct().OrderBy(y => y).ToList();

        //    var viewModel = new DashboardViewModel
        //    {
        //        TotalMeetings = meetings.Count,
        //        ApprovedMeetings = meetings.Count(m => m.MeetingStatus == "approved"),
        //        RejectedMeetings = meetings.Count(m => m.MeetingStatus == "rejected"),
        //        PendingMeetings = meetings.Count(m => m.MeetingStatus == "pending"),
        //        WaitingMeetings = meetings.Count(m => m.MeetingStatus == "rejected" || m.MeetingStatus == "pending"),

        //        // Meetings by type (using BoardType names)
        //        MeetingsByType = meetings
        //            .Where(m => m.BoardType != null)
        //            .GroupBy(m => m.BoardType!.BoardTypeName)
        //            .ToDictionary(g => g.Key, g => g.Count()),

        //        // Meetings by hospital
        //        MeetingsByHospital = meetings
        //            .Where(m => !string.IsNullOrEmpty(m.CHGParty))
        //            .SelectMany(m => m.CHGParty!.Split(',', StringSplitOptions.RemoveEmptyEntries))
        //            .GroupBy(h => h.Trim())
        //            .ToDictionary(g => g.Key, g => g.Count()),

        //        // Yearly reports
        //        YearlyReports = new List<YearlyReportViewModel>(),

        //        // Yearly meetings by type and month
        //        YearlyMeetingsByType = new Dictionary<int, Dictionary<string, Dictionary<int, int>>>()
        //    };

        //    // Build yearly reports and monthly data
        //    foreach (var year in years)
        //    {
        //        var yearMeetings = meetings.Where(m => m.BoardDate.Year == year).ToList();

        //        // Create a dictionary for all BoardTypes (including those with 0 meetings)
        //        var meetingsByTypeForYear = new Dictionary<string, int>();
        //        foreach (var boardType in allBoardTypes)
        //        {
        //            var count = yearMeetings.Count(m => m.BoardType != null && m.BoardType.BoardTypeName == boardType.BoardTypeName);
        //            meetingsByTypeForYear[boardType.BoardTypeName] = count;
        //        }

        //        var yearlyReport = new YearlyReportViewModel
        //        {
        //            Year = year,
        //            MeetingsByType = meetingsByTypeForYear,
        //            TotalMeetings = yearMeetings.Count,
        //            FullyApprovedMeetings = yearMeetings.Count(m => IsFullyApproved(m)),
        //            PendingApprovalMeetings = yearMeetings.Count(m => !IsFullyApproved(m) && m.MeetingStatus != "rejected" && m.MeetingStatus != "approved"),
        //            WaitingMeetings = yearMeetings.Count(m => m.MeetingStatus == "rejected" || m.MeetingStatus == "pending")
        //        };

        //        viewModel.YearlyReports.Add(yearlyReport);

        //        // Build yearly meetings by type and month
        //        var yearlyTypeMonthData = new Dictionary<string, Dictionary<int, int>>();

        //        foreach (var boardType in allBoardTypes)
        //        {
        //            yearlyTypeMonthData[boardType.BoardTypeName] = new Dictionary<int, int>();
        //            for (int month = 1; month <= 12; month++)
        //            {
        //                yearlyTypeMonthData[boardType.BoardTypeName][month] = 0;
        //            }
        //        }

        //        foreach (var meeting in yearMeetings)
        //        {
        //            if (meeting.BoardType != null)
        //            {
        //                var month = meeting.BoardDate.Month;
        //                var type = meeting.BoardType.BoardTypeName;

        //                if (yearlyTypeMonthData.ContainsKey(type))
        //                {
        //                    if (yearlyTypeMonthData[type].ContainsKey(month))
        //                    {
        //                        yearlyTypeMonthData[type][month]++;
        //                    }
        //                }
        //            }
        //        }

        //        viewModel.YearlyMeetingsByType[year] = yearlyTypeMonthData;
        //    }

        //    return viewModel;
        //}

        // Helper function to check if a meeting is fully approved
        private bool IsFullyApproved(Board meeting)
        {
            var allAttendeeIds = meeting.BoardAttendees?.Select(ba => ba.Attendee_ID).ToList() ?? new List<int>();
            var approvedAttendeeIds = meeting.Approvals?.Select(a => a.Attendee_ID).ToList() ?? new List<int>();

            if (allAttendeeIds.Count == 0) return false;

            return allAttendeeIds.All(id => approvedAttendeeIds.Contains(id));
        }

        // -----------------------------
        // LOOKUPS (ALL WITH AsNoTracking)
        // -----------------------------
        public async Task<List<Hospital>> GetAllHospitalsAsync()
        {
            return await _context.Hospitals
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<List<BoardType>> GetAllBoardTypesAsync()
        {
            return await _context.BoardTypes
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<List<Attendee>> GetAllAttendeesAsync()
        {
            return await _context.Attendees
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<List<Attendee>> GetAttendeesByHospitalsAsync(List<int> hospitalIds)
        {
            return await _context.HospitalAttendees
                .Where(ha => hospitalIds.Contains(ha.Hospital_ID))
                .Select(ha => ha.Attendee)
                .Distinct()
                .AsNoTracking()
                .ToListAsync() ?? new List<Attendee>();
        }

        // Add this method to get meetings grouped by hospital and type
        public async Task<Dictionary<string, Dictionary<string, int>>> GetMeetingsByHospitalAndTypeAsync()
        {
            var meetings = await GetAllMeetingsAsync();
            var result = new Dictionary<string, Dictionary<string, int>>();

            foreach (var meeting in meetings)
            {
                if (!string.IsNullOrEmpty(meeting.CHGParty) && !string.IsNullOrEmpty(meeting.MeetingType))
                {
                    var hospitals = meeting.CHGParty.Split(',', StringSplitOptions.RemoveEmptyEntries);
                    var year = meeting.BoardDate.Year;

                    foreach (var hospital in hospitals)
                    {
                        var hospitalName = hospital.Trim();
                        var key = $"{year}_{hospitalName}";

                        if (!result.ContainsKey(key))
                        {
                            result[key] = new Dictionary<string, int>();
                        }

                        if (!result[key].ContainsKey(meeting.MeetingType))
                        {
                            result[key][meeting.MeetingType] = 0;
                        }

                        result[key][meeting.MeetingType]++;
                    }
                }
            }

            return result;
        }

    }

}