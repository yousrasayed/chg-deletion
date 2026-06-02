using System.Text.RegularExpressions;

namespace CHG_Legal.Services
{
    public static class PasswordValidator
    {
        // Regex: At least one lowercase, one uppercase, one digit, one special character, min 8 chars
        private static readonly Regex PasswordRegex = new Regex(
            @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).{8,}$",
            RegexOptions.Compiled
        );

        public static bool IsValid(string password, out string errorMessage)
        {
            errorMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(password))
            {
                errorMessage = "كلمة السر مطلوبة";
                return false;
            }

            if (password.Length < 8)
            {
                errorMessage = "كلمة السر يجب أن تكون 8 أحرف على الأقل";
                return false;
            }

            if (password.Length > 70)
            {
                errorMessage = "كلمة السر يجب ألا تتجاوز 70 حرف";
                return false;
            }

            if (!Regex.IsMatch(password, @"[a-z]"))
            {
                errorMessage = "كلمة السر يجب أن تحتوي على حرف صغير (a-z) على الأقل";
                return false;
            }

            if (!Regex.IsMatch(password, @"[A-Z]"))
            {
                errorMessage = "كلمة السر يجب أن تحتوي على حرف كبير (A-Z) على الأقل";
                return false;
            }

            if (!Regex.IsMatch(password, @"\d"))
            {
                errorMessage = "كلمة السر يجب أن تحتوي على رقم (0-9) على الأقل";
                return false;
            }

            if (!Regex.IsMatch(password, @"[\W_]"))
            {
                errorMessage = "كلمة السر يجب أن تحتوي على رمز خاص (!@#$%^&* etc.) على الأقل";
                return false;
            }

            return true;
        }

        public static string GetRules()
        {
            return "• كلمة السر يجب أن تكون 8 أحرف على الأقل\n" +
                   "• حرف كبير (A-Z) على الأقل\n" +
                   "• حرف صغير (a-z) على الأقل\n" +
                   "• رقم (0-9) على الأقل\n" +
                   "• رمز خاص (!@#$%^&*) على الأقل";
        }
    }
}