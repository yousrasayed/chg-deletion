using CHG_Legal.Models.Entities;
using CHG_Legal.Models.ViewModels;

namespace CHG_Legal.Services.Interfaces
{
    public interface IMeetingService
    {
        Task<List<Board>> GetAllMeetingsAsync();
        Task<Board?> GetMeetingByIdAsync(int id);
        Task<Board> AddMeetingAsync(MeetingViewModel model, string uploadsFolder, int userId);
        Task<bool> UpdateMeetingAsync(MeetingViewModel model, string uploadsFolder);
        Task<bool> DeleteMeetingAsync(int id);
        Task<byte[]?> GetMeetingAttachmentsZipAsync(int meetingId);
        Task<List<BoardAttachment>> GetMeetingAttachmentsAsync(int meetingId);
        //Task<DashboardViewModel> GetDashboardDataAsync();
        Task<List<Attendee>> GetAttendeesByHospitalsAsync(List<int> hospitalIds);
        Task<List<Hospital>> GetAllHospitalsAsync();
        Task<List<BoardType>> GetAllBoardTypesAsync();
        Task<List<Attendee>> GetAllAttendeesAsync();
        Task<bool> DeleteAttachmentAsync(int attachmentId);

        Task<PaginatedResult<Board>> GetFilteredPaginatedMeetingsAsync(int pageNumber, int pageSize, string type, string about, string subject, string date);
    }
}