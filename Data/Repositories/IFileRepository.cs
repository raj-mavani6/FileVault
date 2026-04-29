using FileVault.Web.Models.Domain;

namespace FileVault.Web.Data.Repositories;

public interface IFileRepository
{
    Task<FileItem?> GetByIdAsync(string id);
    Task<List<FileItem>> GetByFolderAsync(string userId, string? folderId, bool includeDeleted = false,
        int page = 1, int pageSize = 50, string? sortBy = null, bool sortDesc = true,
        string? searchTerm = null, string? extension = null, string? tag = null);
    Task<long> CountByFolderAsync(string userId, string? folderId, bool includeDeleted = false,
        string? searchTerm = null, string? extension = null, string? tag = null);
    Task<List<FileItem>> GetDeletedByUserAsync(string userId, int page = 1, int pageSize = 50);
    Task<long> CountDeletedByUserAsync(string userId);
    Task<List<FileItem>> GetRecentByUserAsync(string userId, int count = 10);
    Task CreateAsync(FileItem file);
    Task UpdateAsync(FileItem file);
    Task DeletePermanentAsync(string id);
    Task<long> GetTotalSizeByUserAsync(string userId);
    Task<long> CountByUserAsync(string userId, bool includeDeleted = false);
    Task<List<FileItem>> GetAllAsync(int page = 1, int pageSize = 50);
    Task<long> CountAllAsync();
    Task<Dictionary<string, long>> GetExtensionStatsAsync(string userId);
}
