using FileVault.Web.Models.Domain;

namespace FileVault.Web.Data.Repositories;

public interface IShareLinkRepository
{
    Task<ShareLink?> GetByIdAsync(string id);
    Task<ShareLink?> GetByTokenAsync(string token);
    Task<List<ShareLink>> GetByFileAsync(string fileId);
    Task<List<ShareLink>> GetByUserAsync(string userId, int page = 1, int pageSize = 50);
    Task<long> CountByUserAsync(string userId);
    Task CreateAsync(ShareLink link);
    Task UpdateAsync(ShareLink link);
    Task DeleteAsync(string id);
}
