using CHG_Legal.Models;
using CHG_Legal.Models.Entities;
using CHG_Legal.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CHG_Legal.Services.Implementations
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;

        public AuthService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<User?> ValidateUserAsync(string username, string password)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.User_Name == username && u.Password == password && u.Active);

            return user;
        }

        public async Task<string?> GetUserRoleAsync(int userId)
        {
            var userRole = await _context.UserRole
                .Include(ur => ur.Role)
                .FirstOrDefaultAsync(ur => ur.UserID == userId);

            return userRole?.Role?.RoleName;
        }

        public async Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword)
        {
            try
            {

                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    return false;
                }


                if (user.Password != currentPassword)
                {
                    return false;
                }

                user.Password = newPassword;
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}