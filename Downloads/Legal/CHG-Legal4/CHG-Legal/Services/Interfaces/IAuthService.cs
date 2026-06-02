using CHG_Legal.Models.Entities;

namespace CHG_Legal.Services.Interfaces
{
    public interface IAuthService
    {
        Task<User?> ValidateUserAsync(string username, string password);
        Task<string?> GetUserRoleAsync(int userId);
        Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword);
    }
}