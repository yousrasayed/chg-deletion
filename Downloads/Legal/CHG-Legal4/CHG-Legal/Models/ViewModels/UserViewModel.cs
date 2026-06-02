using System.ComponentModel.DataAnnotations;
using CHG_Legal.Models.Validation;

namespace CHG_Legal.Models.ViewModels
{
    public class UserViewModel
    {
        public int? User_ID { get; set; }

        [Required(ErrorMessage = "اسم المستخدم مطلوب")]
        [StringLength(70, ErrorMessage = "اسم المستخدم لا يتجاوز 70 حرف")]
        public string User_Name { get; set; } = string.Empty;

        [StrongPassword]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "الدور مطلوب")]
        public int RoleID { get; set; }

        public bool Active { get; set; } = true;

        public string? RoleName { get; set; }
    }

    public class MeetingTypeViewModel
    {
        public int? ID { get; set; }

        [Required(ErrorMessage = "اسم نوع الاجتماع مطلوب")]
        [StringLength(100, ErrorMessage = "الاسم لا يتجاوز 100 حرف")]
        public string BoardTypeName { get; set; } = string.Empty;
    }
}