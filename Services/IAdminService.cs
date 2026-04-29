using FileVault.Web.Models.Domain;

namespace FileVault.Web.Services;

public interface IAdminService
{
    Task<List<AppUser>> GetUsersAsync(int page = 1, int pageSize = 20);
    Task<long> CountUsersAsync();
    Task<bool> DisableUserAsync(string userId);
    Task<bool> EnableUserAsync(string userId);
    Task<(long TotalFiles, long TotalSize, long TotalUsers, long ActiveShares)> GetSystemStatsAsync();
}
