using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using MongoDB.Bson;

namespace FileVault.Web.Data.GridFs;

public interface IGridFsService
{
    Task<ObjectId> UploadFromStreamAsync(string fileName, Stream source, string contentType);
    Task<Stream> OpenDownloadStreamAsync(ObjectId fileId);
    Task DownloadToStreamAsync(ObjectId fileId, Stream destination);
    Task DeleteAsync(ObjectId fileId);
    Task<GridFSFileInfo?> GetFileInfoAsync(ObjectId fileId);
}

public class GridFsService : IGridFsService
{
    private readonly IGridFSBucket _bucket;

    public GridFsService(MongoDbContext context)
    {
        _bucket = context.GridFsBucket;
    }

    public async Task<ObjectId> UploadFromStreamAsync(string fileName, Stream source, string contentType)
    {
        var options = new GridFSUploadOptions
        {
            Metadata = new BsonDocument
            {
                { "contentType", contentType },
                { "uploadedAt", DateTime.UtcNow }
            }
        };
        return await _bucket.UploadFromStreamAsync(fileName, source, options);
    }

    public async Task<Stream> OpenDownloadStreamAsync(ObjectId fileId)
    {
        return await _bucket.OpenDownloadStreamAsync(fileId);
    }

    public async Task DownloadToStreamAsync(ObjectId fileId, Stream destination)
    {
        await _bucket.DownloadToStreamAsync(fileId, destination);
    }

    public async Task DeleteAsync(ObjectId fileId)
    {
        await _bucket.DeleteAsync(fileId);
    }

    public async Task<GridFSFileInfo?> GetFileInfoAsync(ObjectId fileId)
    {
        var filter = Builders<GridFSFileInfo>.Filter.Eq("_id", fileId);
        var cursor = await _bucket.FindAsync(filter);
        return await cursor.FirstOrDefaultAsync();
    }
}
