using CHG_Legal.Models;
using CHG_Legal.Models.Entities;
using CHG_Legal.Models.ViewModels;
using CHG_Legal.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CHG_Legal.Services.Implementations
{
    public class AttachmentService : IAttachmentService
    {
        private readonly AppDbContext _context;

        public AttachmentService(AppDbContext context)
        {
            _context = context;
        }

        // ========== MAP TO VIEW MODEL FUNCTIONS ==========

        private AttachmentViewModel MapToViewModel(CompanyAttachment attachment)
        {
            if (attachment == null) return null;
            return new AttachmentViewModel
            {
                ID = attachment.ID,
                FileName = attachment.file_name ?? "unknown",
                FileType = attachment.file_type ?? "unknown",
                FileSize = attachment.file_size ?? 0,
                MimeType = attachment.mime_type ?? "application/octet-stream",
                Description = attachment.description ?? string.Empty,
                UploadedAt = attachment.uploaded_at ?? DateTime.Now,
                UploadedBy = attachment.UserID?.ToString() ?? string.Empty
            };
        }

        private AttachmentViewModel MapToViewModel(ShareHolder_Attaches attachment)
        {
            if (attachment == null) return null;
            return new AttachmentViewModel
            {
                ID = attachment.ID,
                FileName = attachment.file_name ?? "unknown",
                FileType = attachment.file_type ?? "unknown",
                FileSize = attachment.file_size ?? 0,
                MimeType = attachment.mime_type ?? "application/octet-stream",
                Description = attachment.description ?? string.Empty,
                UploadedAt = attachment.uploaded_at ?? DateTime.Now,
                UploadedBy = attachment.UserID?.ToString() ?? string.Empty
            };
        }

        private AttachmentViewModel MapToViewModel(BranchAttachment attachment)
        {
            if (attachment == null) return null;
            return new AttachmentViewModel
            {
                ID = attachment.ID,
                FileName = attachment.file_name ?? "unknown",
                FileType = attachment.file_type ?? "unknown",
                FileSize = attachment.file_size ?? 0,
                MimeType = attachment.mime_type ?? "application/octet-stream",
                Description = attachment.description ?? string.Empty,
                UploadedAt = attachment.uploaded_at ?? DateTime.Now,
                UploadedBy = attachment.UserID?.ToString() ?? string.Empty
            };
        }

        private AttachmentViewModel MapToViewModel(BoardMemberAttach attachment)
        {
            if (attachment == null) return null;
            return new AttachmentViewModel
            {
                ID = attachment.ID,
                FileName = attachment.file_name ?? "unknown",
                FileType = attachment.file_type ?? "unknown",
                FileSize = attachment.file_size ?? 0,
                MimeType = attachment.mime_type ?? "application/octet-stream",
                Description = attachment.description ?? string.Empty,
                UploadedAt = attachment.uploaded_at ?? DateTime.Now,
                UploadedBy = attachment.UserID?.ToString() ?? string.Empty
            };
        }

        private AttachmentViewModel MapToViewModel(BankingAttachment attachment)
        {
            if (attachment == null) return null;
            return new AttachmentViewModel
            {
                ID = attachment.ID,
                FileName = attachment.file_name ?? "unknown",
                FileType = attachment.file_type ?? "unknown",
                FileSize = attachment.file_size ?? 0,
                MimeType = attachment.mime_type ?? "application/octet-stream",
                Description = attachment.description ?? string.Empty,
                UploadedAt = attachment.uploaded_at ?? DateTime.Now,
                UploadedBy = attachment.UserID?.ToString() ?? string.Empty
            };
        }

        private AttachmentViewModel MapToViewModel(NonBankingAttach attachment)
        {
            if (attachment == null) return null;
            return new AttachmentViewModel
            {
                ID = attachment.ID,
                FileName = attachment.file_name ?? "unknown",
                FileType = attachment.file_type ?? "unknown",
                FileSize = attachment.file_size ?? 0,
                MimeType = attachment.mime_type ?? "application/octet-stream",
                Description = attachment.description ?? string.Empty,
                UploadedAt = attachment.uploaded_at ?? DateTime.Now,
                UploadedBy = attachment.UserID?.ToString() ?? string.Empty
            };
        }

        // ========== COMPANY ATTACHMENTS ==========
        public async Task<IEnumerable<AttachmentViewModel>> GetCompanyAttachmentsAsync(int companyId)
        {
            var attachments = await _context.CompanyAttachments
                .Where(a => a.CompanyId == companyId)
                .OrderByDescending(a => a.uploaded_at)
                .ToListAsync();

            return attachments.Select(MapToViewModel).Where(vm => vm != null).ToList();
        }

        public async Task<AttachmentViewModel> AddCompanyAttachmentAsync(int companyId, IFormFile file, string description, int userId)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("الملف غير صالح");

            var company = await _context.Companies.FindAsync(companyId);
            if (company == null)
                throw new ArgumentException("الشركة غير موجودة");

            byte[] fileData;
            using (var memoryStream = new MemoryStream())
            {
                await file.CopyToAsync(memoryStream);
                fileData = memoryStream.ToArray();
            }

            var attachment = new CompanyAttachment
            {
                CompanyId = companyId,
                file_name = file.FileName,
                file_path = null,
                file_type = Path.GetExtension(file.FileName).TrimStart('.'),
                file_size = file.Length,
                mime_type = file.ContentType,
                description = description,
                uploaded_at = DateTime.Now,
                file_data = fileData,
                UserID = userId
            };

            _context.CompanyAttachments.Add(attachment);
            await _context.SaveChangesAsync();

            return MapToViewModel(attachment);
        }

        public async Task<bool> DeleteCompanyAttachmentAsync(int attachmentId)
        {
            var attachment = await _context.CompanyAttachments.FindAsync(attachmentId);
            if (attachment == null)
                return false;

            _context.CompanyAttachments.Remove(attachment);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<(byte[] FileData, string FileName, string ContentType)> DownloadCompanyAttachmentAsync(int attachmentId)
        {
            var attachment = await _context.CompanyAttachments.FindAsync(attachmentId);
            if (attachment == null)
            {
                return (null, null, null);
            }

            return (attachment.file_data, attachment.file_name, attachment.mime_type ?? "application/octet-stream");
        }

        // ========== SHAREHOLDER ATTACHMENTS  ==========
        public async Task<IEnumerable<AttachmentViewModel>> GetShareholderAttachmentsAsync(int shareholderId)
        {
            var attachments = await _context.ShareHolderAttaches
                .Where(a => a.ShareholderId == shareholderId)
                .OrderByDescending(a => a.uploaded_at)
                .ToListAsync();

            return attachments.Select(MapToViewModel).Where(vm => vm != null).ToList();
        }

        public async Task<AttachmentViewModel> AddShareholderAttachmentAsync(int shareholderId, IFormFile file, string description, int userId)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("الملف غير صالح");

            var shareholder = await _context.Shareholders.FindAsync(shareholderId);
            if (shareholder == null)
                throw new ArgumentException("المساهم غير موجود");

            byte[] fileData;
            using (var memoryStream = new MemoryStream())
            {
                await file.CopyToAsync(memoryStream);
                fileData = memoryStream.ToArray();
            }

            var attachment = new ShareHolder_Attaches
            {
                ShareholderId = shareholderId,
                CompanyId = shareholder.CompanyId,
                file_name = file.FileName,
                file_path = null,
                file_type = Path.GetExtension(file.FileName).TrimStart('.'),
                file_size = file.Length,
                mime_type = file.ContentType,
                description = description,
                uploaded_at = DateTime.Now,
                file_data = fileData,
                UserID = userId
            };

            _context.ShareHolderAttaches.Add(attachment);
            await _context.SaveChangesAsync();

            return MapToViewModel(attachment);
        }

        public async Task<bool> DeleteShareholderAttachmentAsync(int attachmentId)
        {
            var attachment = await _context.ShareHolderAttaches.FindAsync(attachmentId);
            if (attachment == null)
                return false;

            _context.ShareHolderAttaches.Remove(attachment);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<(byte[] FileData, string FileName, string ContentType)> DownloadShareholderAttachmentAsync(int attachmentId)
        {
            var attachment = await _context.ShareHolderAttaches.FindAsync(attachmentId);
            if (attachment == null)
            {
                return (null, null, null);
            }

            return (attachment.file_data, attachment.file_name, attachment.mime_type ?? "application/octet-stream");
        }

        // ========== BRANCH ATTACHMENTS ==========
        public async Task<IEnumerable<AttachmentViewModel>> GetBranchAttachmentsAsync(int branchId)
        {
            var attachments = await _context.BranchAttachments
                .Where(a => a.BranchId == branchId)
                .OrderByDescending(a => a.uploaded_at)
                .ToListAsync();

            return attachments.Select(MapToViewModel).Where(vm => vm != null).ToList();
        }

        public async Task<AttachmentViewModel> AddBranchAttachmentAsync(int branchId, IFormFile file, string description, int userId)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("الملف غير صالح");

            var branch = await _context.Branches.FindAsync(branchId);
            if (branch == null)
                throw new ArgumentException("الفرع غير موجود");

            byte[] fileData;
            using (var memoryStream = new MemoryStream())
            {
                await file.CopyToAsync(memoryStream);
                fileData = memoryStream.ToArray();
            }

            var attachment = new BranchAttachment
            {
                BranchId = branchId,
                CompanyId = branch.CompanyId,
                file_name = file.FileName,
                file_path = null,
                file_type = Path.GetExtension(file.FileName).TrimStart('.'),
                file_size = file.Length,
                mime_type = file.ContentType,
                description = description,
                uploaded_at = DateTime.Now,
                file_data = fileData,
                UserID = userId
            };

            _context.BranchAttachments.Add(attachment);
            await _context.SaveChangesAsync();

            return MapToViewModel(attachment);
        }

        public async Task<bool> DeleteBranchAttachmentAsync(int attachmentId)
        {
            var attachment = await _context.BranchAttachments.FindAsync(attachmentId);
            if (attachment == null)
                return false;

            _context.BranchAttachments.Remove(attachment);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<(byte[] FileData, string FileName, string ContentType)> DownloadBranchAttachmentAsync(int attachmentId)
        {
            var attachment = await _context.BranchAttachments.FindAsync(attachmentId);
            if (attachment == null)
            {
                return (null, null, null);
            }

            return (attachment.file_data, attachment.file_name, attachment.mime_type ?? "application/octet-stream");
        }

        // ========== BOARD MEMBER ATTACHMENTS  ==========
        public async Task<IEnumerable<AttachmentViewModel>> GetBoardMemberAttachmentsAsync(int memberId)
        {
            var attachments = await _context.BoardMemberAttaches
                .Where(a => a.MemberId == memberId)
                .OrderByDescending(a => a.uploaded_at)
                .ToListAsync();

            return attachments.Select(MapToViewModel).Where(vm => vm != null).ToList();
        }

        public async Task<AttachmentViewModel> AddBoardMemberAttachmentAsync(int memberId, IFormFile file, string description, int userId)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("الملف غير صالح");

            var member = await _context.BoardMembers.FindAsync(memberId);
            if (member == null)
                throw new ArgumentException("عضو مجلس الإدارة غير موجود");

            byte[] fileData;
            using (var memoryStream = new MemoryStream())
            {
                await file.CopyToAsync(memoryStream);
                fileData = memoryStream.ToArray();
            }

            var attachment = new BoardMemberAttach
            {
                MemberId = memberId,
                file_name = file.FileName,
                file_path = null,
                file_type = Path.GetExtension(file.FileName).TrimStart('.'),
                file_size = file.Length,
                mime_type = file.ContentType,
                description = description,
                uploaded_at = DateTime.Now,
                file_data = fileData,
                UserID = userId
            };

            _context.BoardMemberAttaches.Add(attachment);
            await _context.SaveChangesAsync();

            return MapToViewModel(attachment);
        }

        public async Task<bool> DeleteBoardMemberAttachmentAsync(int attachmentId)
        {
            var attachment = await _context.BoardMemberAttaches.FindAsync(attachmentId);
            if (attachment == null)
                return false;

            _context.BoardMemberAttaches.Remove(attachment);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<(byte[] FileData, string FileName, string ContentType)> DownloadBoardMemberAttachmentAsync(int attachmentId)
        {
            var attachment = await _context.BoardMemberAttaches.FindAsync(attachmentId);
            if (attachment == null)
            {
                return (null, null, null);
            }

            return (attachment.file_data, attachment.file_name, attachment.mime_type ?? "application/octet-stream");
        }

        // ========== BANKING ATTACHMENTS ==========
        public async Task<IEnumerable<AttachmentViewModel>> GetBankingAttachmentsAsync(int companyId)
        {
            var attachments = await _context.BankingAttachments
                .Where(a => a.CompanyId == companyId)
                .OrderByDescending(a => a.uploaded_at)
                .ToListAsync();

            return attachments.Select(MapToViewModel).Where(vm => vm != null).ToList();
        }

        public async Task<AttachmentViewModel> AddBankingAttachmentAsync(int companyId, IFormFile file, string description, int userId)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("الملف غير صالح");

            var company = await _context.Companies.FindAsync(companyId);
            if (company == null)
                throw new ArgumentException("الشركة غير موجودة");

            byte[] fileData;
            using (var memoryStream = new MemoryStream())
            {
                await file.CopyToAsync(memoryStream);
                fileData = memoryStream.ToArray();
            }

            var attachment = new BankingAttachment
            {
                CompanyId = companyId,
                MemberName = null,
                file_name = file.FileName,
                file_path = null,
                file_type = Path.GetExtension(file.FileName).TrimStart('.'),
                file_size = file.Length,
                mime_type = file.ContentType,
                description = description,
                uploaded_at = DateTime.Now,
                file_data = fileData,
                UserID = userId
            };

            _context.BankingAttachments.Add(attachment);
            await _context.SaveChangesAsync();

            return MapToViewModel(attachment);
        }

        public async Task<bool> DeleteBankingAttachmentAsync(int attachmentId)
        {
            var attachment = await _context.BankingAttachments.FindAsync(attachmentId);
            if (attachment == null)
                return false;

            _context.BankingAttachments.Remove(attachment);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<(byte[] FileData, string FileName, string ContentType)> DownloadBankingAttachmentAsync(int attachmentId)
        {
            var attachment = await _context.BankingAttachments.FindAsync(attachmentId);
            if (attachment == null)
            {
                return (null, null, null);
            }

            return (attachment.file_data, attachment.file_name, attachment.mime_type ?? "application/octet-stream");
        }

        // ========== NON-BANKING ATTACHMENTS  ==========
        public async Task<IEnumerable<AttachmentViewModel>> GetNonBankingAttachmentsAsync(int companyId)
        {
            var attachments = await _context.NonBankingAttaches
                .Where(a => a.CompanyId == companyId)
                .OrderByDescending(a => a.uploaded_at)
                .ToListAsync();

            return attachments.Select(MapToViewModel).Where(vm => vm != null).ToList();
        }

        public async Task<AttachmentViewModel> AddNonBankingAttachmentAsync(int companyId, IFormFile file, string description, int userId)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("الملف غير صالح");

            var company = await _context.Companies.FindAsync(companyId);
            if (company == null)
                throw new ArgumentException("الشركة غير موجودة");

            byte[] fileData;
            using (var memoryStream = new MemoryStream())
            {
                await file.CopyToAsync(memoryStream);
                fileData = memoryStream.ToArray();
            }

            var attachment = new NonBankingAttach
            {
                AuthorizationId = 0,
                CompanyId = companyId,
                file_name = file.FileName,
                file_path = null,
                file_type = Path.GetExtension(file.FileName).TrimStart('.'),
                file_size = file.Length,
                mime_type = file.ContentType,
                description = description,
                uploaded_at = DateTime.Now,
                file_data = fileData,
                UserID = userId
            };

            _context.NonBankingAttaches.Add(attachment);
            await _context.SaveChangesAsync();

            return MapToViewModel(attachment);
        }

        public async Task<bool> DeleteNonBankingAttachmentAsync(int attachmentId)
        {
            var attachment = await _context.NonBankingAttaches.FindAsync(attachmentId);
            if (attachment == null)
                return false;

            _context.NonBankingAttaches.Remove(attachment);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<(byte[] FileData, string FileName, string ContentType)> DownloadNonBankingAttachmentAsync(int attachmentId)
        {
            var attachment = await _context.NonBankingAttaches.FindAsync(attachmentId);
            if (attachment == null)
            {
                return (null, null, null);
            }

            return (attachment.file_data, attachment.file_name, attachment.mime_type ?? "application/octet-stream");
        }
    }
}