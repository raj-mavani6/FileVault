using FileVault.Web.Models.Domain;

namespace FileVault.Web.Services;

public interface IFolderService
{
    Task<Folder?> GetByIdAsync(string id, string userId);
    Task<List<Folder>> GetChildrenAsync(string userId, string? parentId);
    Task<Folder> CreateAsync(string userId, string? parentId, string name);
    Task RenameAsync(string id, string userId, string newName);
    Task MoveAsync(string id, string userId, string? newParentId);
    Task SoftDeleteAsync(string id, string userId);
    Task RestoreAsync(string id, string userId);
    Task PermanentDeleteAsync(string id, string userId);
    Task<List<Folder>> GetBreadcrumbsAsync(string folderId, string userId);
    Task<List<Folder>> GetDeletedAsync(string userId);
    Task<long> CountAsync(string userId);
}
