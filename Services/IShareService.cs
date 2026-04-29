using FileVault.Web.Models.Domain;

namespace FileVault.Web.Services;

public interface IShareService
{
    Task<ShareLink> CreateShareLinkAsync(string fileId, string userId, bool allowDownload,
        DateTime? expiresAt, string? password);
    Task<ShareLink?> GetByTokenAsync(string token);
    Task<(bool Valid, string? Message)> ValidateShareAccessAsync(string token, string? password);
    Task<List<ShareLink>> GetUserSharesAsync(string userId, int page = 1, int pageSize = 50);
    Task<long> CountUserSharesAsync(string userId);
    Task RevokeAsync(string id, string userId);
    Task RecordAccessAsync(string token);
}
