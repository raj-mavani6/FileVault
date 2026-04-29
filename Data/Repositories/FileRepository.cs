using MongoDB.Driver;
using FileVault.Web.Models.Domain;

namespace FileVault.Web.Data.Repositories;

public class FileRepository : IFileRepository
{
    private readonly IMongoCollection<FileItem> _files;

    public FileRepository(MongoDbContext context)
    {
        _files = context.Files;
    }

    public async Task<FileItem?> GetByIdAsync(string id)
        => await _files.Find(f => f.Id == id).FirstOrDefaultAsync();

    public async Task<List<FileItem>> GetByFolderAsync(string userId, string? folderId,
        bool includeDeleted = false, int page = 1, int pageSize = 50,
        string? sortBy = null, bool sortDesc = true,
        string? searchTerm = null, string? extension = null, string? tag = null)
    {
        var builder = Builders<FileItem>.Filter;
        var filter = builder.Eq(f => f.OwnerUserId, userId);

        if (!includeDeleted)
            filter &= builder.Eq(f => f.IsDeleted, false);

        if (folderId != null)
            filter &= builder.Eq(f => f.FolderId, folderId);
        else
            filter &= builder.Eq(f => f.FolderId, null);

        if (!string.IsNullOrWhiteSpace(searchTerm))
            filter &= builder.Regex(f => f.FileName, new MongoDB.Bson.BsonRegularExpression(searchTerm, "i"));

        if (!string.IsNullOrWhiteSpace(extension))
            filter &= builder.Eq(f => f.Extension, extension.ToLowerInvariant());

        if (!string.IsNullOrWhiteSpace(tag))
            filter &= builder.AnyEq(f => f.Tags, tag);

        var sort = sortBy?.ToLower() switch
        {
            "name" => sortDesc
                ? Builders<FileItem>.Sort.Descending(f => f.FileName)
                : Builders<FileItem>.Sort.Ascending(f => f.FileName),
            "size" => sortDesc
                ? Builders<FileItem>.Sort.Descending(f => f.SizeBytes)
                : Builders<FileItem>.Sort.Ascending(f => f.SizeBytes),
            "type" => sortDesc
                ? Builders<FileItem>.Sort.Descending(f => f.Extension)
                : Builders<FileItem>.Sort.Ascending(f => f.Extension),
            _ => sortDesc
                ? Builders<FileItem>.Sort.Descending(f => f.CreatedAt)
                : Builders<FileItem>.Sort.Ascending(f => f.CreatedAt)
        };

        return await _files.Find(filter)
            .Sort(sort)
            .Skip((page - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync();
    }

    public async Task<long> CountByFolderAsync(string userId, string? folderId,
        bool includeDeleted = false, string? searchTerm = null,
        string? extension = null, string? tag = null)
    {
        var builder = Builders<FileItem>.Filter;
        var filter = builder.Eq(f => f.OwnerUserId, userId);

        if (!includeDeleted)
            filter &= builder.Eq(f => f.IsDeleted, false);

        if (folderId != null)
            filter &= builder.Eq(f => f.FolderId, folderId);
        else
            filter &= builder.Eq(f => f.FolderId, null);

        if (!string.IsNullOrWhiteSpace(searchTerm))
            filter &= builder.Regex(f => f.FileName, new MongoDB.Bson.BsonRegularExpression(searchTerm, "i"));

        if (!string.IsNullOrWhiteSpace(extension))
            filter &= builder.Eq(f => f.Extension, extension.ToLowerInvariant());

        if (!string.IsNullOrWhiteSpace(tag))
            filter &= builder.AnyEq(f => f.Tags, tag);

        return await _files.CountDocumentsAsync(filter);
    }

    public async Task<List<FileItem>> GetDeletedByUserAsync(string userId, int page = 1, int pageSize = 50)
        => await _files.Find(f => f.OwnerUserId == userId && f.IsDeleted)
            .SortByDescending(f => f.DeletedAt)
            .Skip((page - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync();

    public async Task<long> CountDeletedByUserAsync(string userId)
        => await _files.CountDocumentsAsync(f => f.OwnerUserId == userId && f.IsDeleted);

    public async Task<List<FileItem>> GetRecentByUserAsync(string userId, int count = 10)
        => await _files.Find(f => f.OwnerUserId == userId && !f.IsDeleted)
            .SortByDescending(f => f.CreatedAt)
            .Limit(count)
            .ToListAsync();

    public async Task CreateAsync(FileItem file)
        => await _files.InsertOneAsync(file);

    public async Task UpdateAsync(FileItem file)
    {
        file.UpdatedAt = DateTime.UtcNow;
        await _files.ReplaceOneAsync(f => f.Id == file.Id, file);
    }

    public async Task DeletePermanentAsync(string id)
        => await _files.DeleteOneAsync(f => f.Id == id);

    public async Task<long> GetTotalSizeByUserAsync(string userId)
    {
        var pipeline = new MongoDB.Bson.BsonDocument[]
        {
            new("$match", new MongoDB.Bson.BsonDocument
            {
                { "ownerUserId", new MongoDB.Bson.ObjectId(userId) },
                { "isDeleted", false }
            }),
            new("$group", new MongoDB.Bson.BsonDocument
            {
                { "_id", MongoDB.Bson.BsonNull.Value },
                { "totalSize", new MongoDB.Bson.BsonDocument("$sum", "$sizeBytes") }
            })
        };

        var cursor = await _files.AggregateAsync<MongoDB.Bson.BsonDocument>(pipeline);
        var result = await cursor.FirstOrDefaultAsync();
        if (result == null) return 0;
        
        var totalValue = result["totalSize"];
        if (totalValue.IsDouble) return (long)totalValue.AsDouble;
        if (totalValue.IsInt32) return (long)totalValue.AsInt32;
        return totalValue.AsInt64;
    }

    public async Task<long> CountByUserAsync(string userId, bool includeDeleted = false)
    {
        if (includeDeleted)
            return await _files.CountDocumentsAsync(f => f.OwnerUserId == userId);
        return await _files.CountDocumentsAsync(f => f.OwnerUserId == userId && !f.IsDeleted);
    }

    public async Task<List<FileItem>> GetAllAsync(int page = 1, int pageSize = 50)
        => await _files.Find(_ => true)
            .SortByDescending(f => f.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync();

    public async Task<long> CountAllAsync()
        => await _files.CountDocumentsAsync(_ => true);

    public async Task<Dictionary<string, long>> GetExtensionStatsAsync(string userId)
    {
        var pipeline = new MongoDB.Bson.BsonDocument[]
        {
            new("$match", new MongoDB.Bson.BsonDocument
            {
                { "ownerUserId", new MongoDB.Bson.ObjectId(userId) },
                { "isDeleted", false }
            }),
            new("$group", new MongoDB.Bson.BsonDocument
            {
                { "_id", "$extension" },
                { "count", new MongoDB.Bson.BsonDocument("$sum", 1) }
            })
        };

        var cursor = await _files.AggregateAsync<MongoDB.Bson.BsonDocument>(pipeline);
        var results = await cursor.ToListAsync();
        return results.ToDictionary(
            r => r["_id"].IsBsonNull ? "unknown" : r["_id"].AsString,
            r => r["count"].ToInt64());
    }
}
