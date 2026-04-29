using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using FileVault.Web.Models.Domain;
using FileVault.Web.Models.Settings;
using Microsoft.Extensions.Options;

namespace FileVault.Web.Data;

public class MongoDbContext
{
    private readonly IMongoDatabase _database;
    public IGridFSBucket GridFsBucket { get; }

    public IMongoCollection<AppUser> Users => _database.GetCollection<AppUser>("users");
    public IMongoCollection<Folder> Folders => _database.GetCollection<Folder>("folders");
    public IMongoCollection<FileItem> Files => _database.GetCollection<FileItem>("files");
    public IMongoCollection<UploadSession> UploadSessions => _database.GetCollection<UploadSession>("uploadSessions");
    public IMongoCollection<ShareLink> ShareLinks => _database.GetCollection<ShareLink>("shareLinks");
    public IMongoCollection<AuditLog> AuditLogs => _database.GetCollection<AuditLog>("auditLogs");

    public MongoDbContext(IOptions<MongoDbSettings> settings)
    {
        var client = new MongoClient(settings.Value.ConnectionString);
        _database = client.GetDatabase(settings.Value.DatabaseName);
        GridFsBucket = new GridFSBucket(_database, new GridFSBucketOptions
        {
            BucketName = settings.Value.GridFsBucketName
        });
    }

    public async Task EnsureIndexesAsync()
    {
        // Users indexes
        await Users.Indexes.CreateManyAsync(new[]
        {
            new CreateIndexModel<AppUser>(
                Builders<AppUser>.IndexKeys.Ascending(u => u.Email),
                new CreateIndexOptions { Unique = true }),
            new CreateIndexModel<AppUser>(
                Builders<AppUser>.IndexKeys.Ascending(u => u.IsActive))
        });

        // Files indexes
        await Files.Indexes.CreateManyAsync(new[]
        {
            new CreateIndexModel<FileItem>(
                Builders<FileItem>.IndexKeys.Ascending(f => f.OwnerUserId)),
            new CreateIndexModel<FileItem>(
                Builders<FileItem>.IndexKeys.Ascending(f => f.FolderId)),
            new CreateIndexModel<FileItem>(
                Builders<FileItem>.IndexKeys.Ascending(f => f.FileName)),
            new CreateIndexModel<FileItem>(
                Builders<FileItem>.IndexKeys.Ascending(f => f.CreatedAt)),
            new CreateIndexModel<FileItem>(
                Builders<FileItem>.IndexKeys.Ascending(f => f.Tags)),
            new CreateIndexModel<FileItem>(
                Builders<FileItem>.IndexKeys.Ascending(f => f.IsDeleted)),
            new CreateIndexModel<FileItem>(
                Builders<FileItem>.IndexKeys.Ascending(f => f.Extension))
        });

        // Folders indexes
        await Folders.Indexes.CreateManyAsync(new[]
        {
            new CreateIndexModel<Folder>(
                Builders<Folder>.IndexKeys.Ascending(f => f.OwnerUserId)),
            new CreateIndexModel<Folder>(
                Builders<Folder>.IndexKeys.Ascending(f => f.ParentFolderId)),
            new CreateIndexModel<Folder>(
                Builders<Folder>.IndexKeys.Ascending(f => f.IsDeleted))
        });

        // Upload sessions indexes
        await UploadSessions.Indexes.CreateManyAsync(new[]
        {
            new CreateIndexModel<UploadSession>(
                Builders<UploadSession>.IndexKeys.Ascending(u => u.UserId)),
            new CreateIndexModel<UploadSession>(
                Builders<UploadSession>.IndexKeys.Ascending(u => u.Status)),
            new CreateIndexModel<UploadSession>(
                Builders<UploadSession>.IndexKeys.Ascending(u => u.ExpiresAt))
        });

        // Share links indexes
        await ShareLinks.Indexes.CreateManyAsync(new[]
        {
            new CreateIndexModel<ShareLink>(
                Builders<ShareLink>.IndexKeys.Ascending(s => s.Token),
                new CreateIndexOptions { Unique = true }),
            new CreateIndexModel<ShareLink>(
                Builders<ShareLink>.IndexKeys.Ascending(s => s.FileId)),
            new CreateIndexModel<ShareLink>(
                Builders<ShareLink>.IndexKeys.Ascending(s => s.OwnerUserId))
        });

        // Audit logs indexes
        await AuditLogs.Indexes.CreateManyAsync(new[]
        {
            new CreateIndexModel<AuditLog>(
                Builders<AuditLog>.IndexKeys.Ascending(a => a.UserId)),
            new CreateIndexModel<AuditLog>(
                Builders<AuditLog>.IndexKeys.Descending(a => a.CreatedAt))
        });
    }
}
