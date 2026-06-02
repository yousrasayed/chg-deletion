using System.ComponentModel.DataAnnotations;
using CHG_Legal.Models.Validation;

namespace CHG_Legal.Models.ViewModels
{
    public class ChangePasswordViewModel
    {
        [Required(ErrorMessage = "كلمة السر الحالية مطلوبة")]
        [DataType(DataType.Password)]
        public string CurrentPassword { get; set; } = string.Empty;

        [StrongPassword]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "تأكيد كلمة السر مطلوب")]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "كلمة السر غير متطابقة")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}