using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace CHG_Legal.Models.Validation
{
    public class StrongPasswordAttribute : ValidationAttribute
    {
        private static readonly Regex PasswordRegex = new Regex(
            @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).{8,}$",
            RegexOptions.Compiled
        );

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
            {
                return new ValidationResult("كلمة السر مطلوبة");
            }

            var password = value.ToString()!;

            if (password.Length < 8)
            {
                return new ValidationResult("كلمة السر يجب أن تكون 8 أحرف على الأقل");
            }

            if (!Regex.IsMatch(password, @"[a-z]"))
            {
                return new ValidationResult("كلمة السر يجب أن تحتوي على حرف صغير (a-z) على الأقل");
            }

            if (!Regex.IsMatch(password, @"[A-Z]"))
            {
                return new ValidationResult("كلمة السر يجب أن تحتوي على حرف كبير (A-Z) على الأقل");
            }

            if (!Regex.IsMatch(password, @"\d"))
            {
                return new ValidationResult("كلمة السر يجب أن تحتوي على رقم (0-9) على الأقل");
            }

            if (!Regex.IsMatch(password, @"[\W_]"))
            {
                return new ValidationResult("كلمة السر يجب أن تحتوي على رمز خاص (!@#$%^&* etc.) على الأقل");
            }

            return ValidationResult.Success;
        }
    }
}