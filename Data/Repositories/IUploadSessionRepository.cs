using FileVault.Web.Models.Domain;

namespace FileVault.Web.Data.Repositories;

public interface IUploadSessionRepository
{
    Task<UploadSession?> GetByIdAsync(string id);
    Task<List<UploadSession>> GetActiveByUserAsync(string userId);
    Task<List<UploadSession>> GetExpiredSessionsAsync();
    Task CreateAsync(UploadSession session);
    Task UpdateAsync(UploadSession session);
    Task DeleteAsync(string id);
}
