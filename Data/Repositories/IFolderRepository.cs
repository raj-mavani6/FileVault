using FileVault.Web.Models.Domain;

namespace FileVault.Web.Data.Repositories;

public interface IFolderRepository
{
    Task<Folder?> GetByIdAsync(string id);
    Task<List<Folder>> GetByParentAsync(string userId, string? parentId, bool includeDeleted = false);
    Task<List<Folder>> GetDeletedByUserAsync(string userId);
    Task<long> CountByUserAsync(string userId, bool includeDeleted = false);
    Task CreateAsync(Folder folder);
    Task UpdateAsync(Folder folder);
    Task DeletePermanentAsync(string id);
    Task<List<Folder>> GetAncestorsAsync(string folderId, string userId);
}
