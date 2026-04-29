using FileVault.Web.Models.Domain;
using FileVault.Web.Data.Repositories;

namespace FileVault.Web.Services;

public class FolderService : IFolderService
{
    private readonly IFolderRepository _folderRepo;
    private readonly IAuditService _auditService;
    private readonly ILogger<FolderService> _logger;
    private readonly IFileService _fileService;

    public FolderService(IFolderRepository folderRepo, IAuditService auditService, ILogger<FolderService> logger, IFileService fileService)
    {
        _folderRepo = folderRepo;
        _auditService = auditService;
        _logger = logger;
        _fileService = fileService;
    }

    public async Task<Folder?> GetByIdAsync(string id, string userId)
    {
        var folder = await _folderRepo.GetByIdAsync(id);
        if (folder == null || folder.OwnerUserId != userId) return null;
        return folder;
    }

    public async Task<List<Folder>> GetChildrenAsync(string userId, string? parentId)
        => await _folderRepo.GetByParentAsync(userId, parentId);

    public async Task<Folder> CreateAsync(string userId, string? parentId, string name)
    {
        var path = "/";
        if (parentId != null)
        {
            var parent = await _folderRepo.GetByIdAsync(parentId);
            if (parent != null)
                path = parent.Path.TrimEnd('/') + "/" + parent.Name;
        }

        var folder = new Folder
        {
            OwnerUserId = userId,
            ParentFolderId = parentId,
            Name = name,
            Path = path
        };

        await _folderRepo.CreateAsync(folder);
        await _auditService.LogAsync(userId, null, "FolderCreated", "Folder", folder.Id,
            new Dictionary<string, string> { { "name", name } });
        return folder;
    }

    public async Task RenameAsync(string id, string userId, string newName)
    {
        var folder = await _folderRepo.GetByIdAsync(id);
        if (folder == null || folder.OwnerUserId != userId) return;

        folder.Name = newName;
        await _folderRepo.UpdateAsync(folder);
        await _auditService.LogAsync(userId, null, "FolderRenamed", "Folder", id);
    }

    public async Task MoveAsync(string id, string userId, string? newParentId)
    {
        var folder = await _folderRepo.GetByIdAsync(id);
        if (folder == null || folder.OwnerUserId != userId) return;

        folder.ParentFolderId = newParentId;
        if (newParentId != null)
        {
            var parent = await _folderRepo.GetByIdAsync(newParentId);
            if (parent != null)
                folder.Path = parent.Path.TrimEnd('/') + "/" + parent.Name;
        }
        else
        {
            folder.Path = "/";
        }

        await _folderRepo.UpdateAsync(folder);
        await _auditService.LogAsync(userId, null, "FolderMoved", "Folder", id);
    }

    public async Task SoftDeleteAsync(string id, string userId)
    {
        var folder = await _folderRepo.GetByIdAsync(id);
        if (folder == null || folder.OwnerUserId != userId) return;

        folder.IsDeleted = true;
        folder.DeletedAt = DateTime.UtcNow;
        await _folderRepo.UpdateAsync(folder);
        await _auditService.LogAsync(userId, null, "FolderTrashed", "Folder", id);
    }

    public async Task RestoreAsync(string id, string userId)
    {
        var folder = await _folderRepo.GetByIdAsync(id);
        if (folder == null || folder.OwnerUserId != userId) return;

        folder.IsDeleted = false;
        folder.DeletedAt = null;
        await _folderRepo.UpdateAsync(folder);
        await _auditService.LogAsync(userId, null, "FolderRestored", "Folder", id);
    }


    public async Task PermanentDeleteAsync(string id, string userId)
    {
        var folder = await _folderRepo.GetByIdAsync(id);
        if (folder == null || folder.OwnerUserId != userId) return;

        // 1. Recursive delete for subfolders
        var subfolders = await _folderRepo.GetByParentAsync(userId, id);
        foreach (var sf in subfolders)
        {
            await PermanentDeleteAsync(sf.Id, userId);
        }

        // 2. Clear all files in THIS folder
        var (files, _) = await _fileService.GetFilesAsync(userId, id, 1, 10000, null, false, null, null, null);
        foreach (var file in files)
        {
            await _fileService.PermanentDeleteAsync(file.Id, userId);
        }

        // 3. Delete the folder itself
        await _folderRepo.DeletePermanentAsync(id);
        await _auditService.LogAsync(userId, null, "FolderDeletedPermanently", "Folder", id);
    }

    public async Task<List<Folder>> GetBreadcrumbsAsync(string folderId, string userId)
        => await _folderRepo.GetAncestorsAsync(folderId, userId);

    public async Task<List<Folder>> GetDeletedAsync(string userId)
        => await _folderRepo.GetDeletedByUserAsync(userId);

    public async Task<long> CountAsync(string userId)
        => await _folderRepo.CountByUserAsync(userId);
}
