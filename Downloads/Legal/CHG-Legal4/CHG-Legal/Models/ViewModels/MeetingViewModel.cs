using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace CHG_Legal.Models.ViewModels
{
    public class MeetingViewModel
    {
        public int? Id { get; set; }

        [Required(ErrorMessage = "التاريخ مطلوب")]
        public DateTime BoardDate { get; set; }

        [Required(ErrorMessage = "نوع الاجتماع مطلوب")]
        public int BoardTypeID { get; set; }

        [Required(ErrorMessage = "الاجتماع بخصوص مطلوب")]
        public string CHGParty { get; set; } = string.Empty;

        [Required(ErrorMessage = "موضوع الاجتماع مطلوب")]
        public string Subject { get; set; } = string.Empty;

        [Required(ErrorMessage = "حالة الاجتماع مطلوبة")]
        public string MeetingStatus { get; set; } = string.Empty;

        public string MeetingType { get; set; } = string.Empty;

        public string? Notes { get; set; }

        public int UserID { get; set; }

        public List<int> AttendeeIds { get; set; } = new List<int>();
        public List<int> ApprovalIds { get; set; } = new List<int>();
        public List<IFormFile> MeetingAttachments { get; set; } = new List<IFormFile>();
        public List<DecisionInput> Decisions { get; set; } = new List<DecisionInput>();
    }

    public class DecisionInput
    {
        public short Decision_Number { get; set; }
        public string Decision_Details { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public string IsExecuted { get; set; } = "False";
        public List<IFormFile> Attachments { get; set; } = new List<IFormFile>();
    }
}