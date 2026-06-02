using System;

namespace CHG_Legal.Models.ViewModels
{
    public class AttachmentViewModel
    {
        public int ID { get; set; }
        public string FileName { get; set; }
        public string FileType { get; set; }
        public long? FileSize { get; set; }
        public string MimeType { get; set; }
        public string Description { get; set; }
        public DateTime? UploadedAt { get; set; }
        public string UploadedBy { get; set; }

        // For display
        public string FormattedFileSize => FileSize.HasValue ? GetFormattedFileSize(FileSize.Value) : "0 B";
        public string FormattedUploadDate => UploadedAt?.ToString("yyyy/MM/dd HH:mm") ?? "-";

        private string GetFormattedFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }
    }
}