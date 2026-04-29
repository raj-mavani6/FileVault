using FileVault.Web.Models.Domain;

namespace FileVault.Web.Data.Repositories;

public interface IUserRepository
{
    Task<AppUser?> GetByIdAsync(string id);
    Task<AppUser?> GetByEmailAsync(string email);
    Task<List<AppUser>> GetAllAsync(int page = 1, int pageSize = 20);
    Task<long> CountAsync();
    Task CreateAsync(AppUser user);
    Task UpdateAsync(AppUser user);
    Task DeleteAsync(string id);
    Task<AppUser?> GetByResetTokenAsync(string token);
    Task<AppUser?> GetByConfirmTokenAsync(string token);
}
