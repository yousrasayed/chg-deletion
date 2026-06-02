using CHG_Legal.Models.ViewModels;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CHG_Legal.Services.Interfaces
{
    public interface IAttachmentService
    {
        // Company Attachments
        Task<IEnumerable<AttachmentViewModel>> GetCompanyAttachmentsAsync(int companyId);
        Task<AttachmentViewModel> AddCompanyAttachmentAsync(int companyId, IFormFile file, string description, int userId);
        Task<bool> DeleteCompanyAttachmentAsync(int attachmentId);
        Task<(byte[] FileData, string FileName, string ContentType)> DownloadCompanyAttachmentAsync(int attachmentId);

        // Shareholder Attachments
        Task<IEnumerable<AttachmentViewModel>> GetShareholderAttachmentsAsync(int shareholderId);
        Task<AttachmentViewModel> AddShareholderAttachmentAsync(int shareholderId, IFormFile file, string description, int userId);
        Task<bool> DeleteShareholderAttachmentAsync(int attachmentId);
        Task<(byte[] FileData, string FileName, string ContentType)> DownloadShareholderAttachmentAsync(int attachmentId);

        // Branch Attachments
        Task<IEnumerable<AttachmentViewModel>> GetBranchAttachmentsAsync(int branchId);
        Task<AttachmentViewModel> AddBranchAttachmentAsync(int branchId, IFormFile file, string description, int userId);
        Task<bool> DeleteBranchAttachmentAsync(int attachmentId);
        Task<(byte[] FileData, string FileName, string ContentType)> DownloadBranchAttachmentAsync(int attachmentId);

        // Board Member Attachments
        Task<IEnumerable<AttachmentViewModel>> GetBoardMemberAttachmentsAsync(int memberId);
        Task<AttachmentViewModel> AddBoardMemberAttachmentAsync(int memberId, IFormFile file, string description, int userId);
        Task<bool> DeleteBoardMemberAttachmentAsync(int attachmentId);
        Task<(byte[] FileData, string FileName, string ContentType)> DownloadBoardMemberAttachmentAsync(int attachmentId);

        // Banking Attachments (for Bank Signatures - per company)
        Task<IEnumerable<AttachmentViewModel>> GetBankingAttachmentsAsync(int companyId);
        Task<AttachmentViewModel> AddBankingAttachmentAsync(int companyId, IFormFile file, string description, int userId);
        Task<bool> DeleteBankingAttachmentAsync(int attachmentId);
        Task<(byte[] FileData, string FileName, string ContentType)> DownloadBankingAttachmentAsync(int attachmentId);

        // Non-Banking Attachments (for Non-Bank Signatures - per company)
        Task<IEnumerable<AttachmentViewModel>> GetNonBankingAttachmentsAsync(int companyId);
        Task<AttachmentViewModel> AddNonBankingAttachmentAsync(int companyId, IFormFile file, string description, int userId);
        Task<bool> DeleteNonBankingAttachmentAsync(int attachmentId);
        Task<(byte[] FileData, string FileName, string ContentType)> DownloadNonBankingAttachmentAsync(int attachmentId);
    }
}