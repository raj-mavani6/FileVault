using MongoDB.Driver;
using FileVault.Web.Models.Domain;

namespace FileVault.Web.Data.Repositories;

public class ShareLinkRepository : IShareLinkRepository
{
    private readonly IMongoCollection<ShareLink> _links;

    public ShareLinkRepository(MongoDbContext context)
    {
        _links = context.ShareLinks;
    }

    public async Task<ShareLink?> GetByIdAsync(string id)
        => await _links.Find(l => l.Id == id).FirstOrDefaultAsync();

    public async Task<ShareLink?> GetByTokenAsync(string token)
        => await _links.Find(l => l.Token == token).FirstOrDefaultAsync();

    public async Task<List<ShareLink>> GetByFileAsync(string fileId)
        => await _links.Find(l => l.FileId == fileId && !l.IsRevoked)
            .SortByDescending(l => l.CreatedAt)
            .ToListAsync();

    public async Task<List<ShareLink>> GetByUserAsync(string userId, int page = 1, int pageSize = 50)
        => await _links.Find(l => l.OwnerUserId == userId)
            .SortByDescending(l => l.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync();

    public async Task<long> CountByUserAsync(string userId)
        => await _links.CountDocumentsAsync(l => l.OwnerUserId == userId && !l.IsRevoked);

    public async Task CreateAsync(ShareLink link)
        => await _links.InsertOneAsync(link);

    public async Task UpdateAsync(ShareLink link)
        => await _links.ReplaceOneAsync(l => l.Id == link.Id, link);

    public async Task DeleteAsync(string id)
        => await _links.DeleteOneAsync(l => l.Id == id);
}
