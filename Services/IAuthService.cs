using FileVault.Web.Models.Domain;

namespace FileVault.Web.Services;

public interface IAuthService
{
    Task<(bool Success, string Message, AppUser? User)> RegisterAsync(string fullName, string email, string password);
    Task<(bool Success, string Message, AppUser? User)> LoginAsync(string email, string password);
    Task<(bool Success, string Message)> ForgotPasswordAsync(string email);
    Task<(bool Success, string Message)> ResetPasswordAsync(string token, string newPassword);
    Task<(bool Success, string Message)> ConfirmEmailAsync(string token);
    Task<(bool Success, string Message)> ChangePasswordAsync(string userId, string currentPassword, string newPassword);
    Task<AppUser?> GetUserByIdAsync(string userId);
    Task UpdateProfileAsync(string userId, string fullName);
}
