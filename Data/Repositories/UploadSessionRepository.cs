using MongoDB.Driver;
using FileVault.Web.Models.Domain;

namespace FileVault.Web.Data.Repositories;

public class UploadSessionRepository : IUploadSessionRepository
{
    private readonly IMongoCollection<UploadSession> _sessions;

    public UploadSessionRepository(MongoDbContext context)
    {
        _sessions = context.UploadSessions;
    }

    public async Task<UploadSession?> GetByIdAsync(string id)
        => await _sessions.Find(s => s.Id == id).FirstOrDefaultAsync();

    public async Task<List<UploadSession>> GetActiveByUserAsync(string userId)
        => await _sessions.Find(s => s.UserId == userId &&
            (s.Status == UploadStatus.Initiated || s.Status == UploadStatus.InProgress))
            .SortByDescending(s => s.StartedAt)
            .ToListAsync();

    public async Task<List<UploadSession>> GetExpiredSessionsAsync()
        => await _sessions.Find(s => s.ExpiresAt < DateTime.UtcNow &&
            s.Status != UploadStatus.Completed && s.Status != UploadStatus.Aborted)
            .ToListAsync();

    public async Task CreateAsync(UploadSession session)
        => await _sessions.InsertOneAsync(session);

    public async Task UpdateAsync(UploadSession session)
        => await _sessions.ReplaceOneAsync(s => s.Id == session.Id, session);

    public async Task DeleteAsync(string id)
        => await _sessions.DeleteOneAsync(s => s.Id == id);
}
