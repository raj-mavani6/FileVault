using MongoDB.Driver;
using FileVault.Web.Models.Domain;

namespace FileVault.Web.Data.Repositories;

public class FolderRepository : IFolderRepository
{
    private readonly IMongoCollection<Folder> _folders;

    public FolderRepository(MongoDbContext context)
    {
        _folders = context.Folders;
    }

    public async Task<Folder?> GetByIdAsync(string id)
        => await _folders.Find(f => f.Id == id).FirstOrDefaultAsync();

    public async Task<List<Folder>> GetByParentAsync(string userId, string? parentId, bool includeDeleted = false)
    {
        var builder = Builders<Folder>.Filter;
        var filter = builder.Eq(f => f.OwnerUserId, userId);

        if (parentId != null)
            filter &= builder.Eq(f => f.ParentFolderId, parentId);
        else
            filter &= builder.Eq(f => f.ParentFolderId, null);

        if (!includeDeleted)
            filter &= builder.Eq(f => f.IsDeleted, false);

        return await _folders.Find(filter)
            .SortBy(f => f.Name)
            .ToListAsync();
    }

    public async Task<List<Folder>> GetDeletedByUserAsync(string userId)
        => await _folders.Find(f => f.OwnerUserId == userId && f.IsDeleted)
            .SortByDescending(f => f.DeletedAt)
            .ToListAsync();

    public async Task<long> CountByUserAsync(string userId, bool includeDeleted = false)
    {
        if (includeDeleted)
            return await _folders.CountDocumentsAsync(f => f.OwnerUserId == userId);
        return await _folders.CountDocumentsAsync(f => f.OwnerUserId == userId && !f.IsDeleted);
    }

    public async Task CreateAsync(Folder folder)
        => await _folders.InsertOneAsync(folder);

    public async Task UpdateAsync(Folder folder)
    {
        folder.UpdatedAt = DateTime.UtcNow;
        await _folders.ReplaceOneAsync(f => f.Id == folder.Id, folder);
    }

    public async Task DeletePermanentAsync(string id)
        => await _folders.DeleteOneAsync(f => f.Id == id);

    public async Task<List<Folder>> GetAncestorsAsync(string folderId, string userId)
    {
        var ancestors = new List<Folder>();
        var currentId = folderId;

        while (currentId != null)
        {
            var folder = await _folders.Find(f => f.Id == currentId && f.OwnerUserId == userId)
                .FirstOrDefaultAsync();
            if (folder == null) break;
            ancestors.Insert(0, folder);
            currentId = folder.ParentFolderId;
        }

        return ancestors;
    }
}
