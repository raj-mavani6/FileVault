using MongoDB.Driver;
using FileVault.Web.Models.Domain;

namespace FileVault.Web.Data.Repositories;

public class AuditLogRepository : IAuditLogRepository
{
    private readonly IMongoCollection<AuditLog> _logs;

    public AuditLogRepository(MongoDbContext context)
    {
        _logs = context.AuditLogs;
    }

    public async Task CreateAsync(AuditLog log)
        => await _logs.InsertOneAsync(log);

    public async Task<List<AuditLog>> GetRecentAsync(int count = 50)
        => await _logs.Find(_ => true)
            .SortByDescending(l => l.CreatedAt)
            .Limit(count)
            .ToListAsync();

    public async Task<List<AuditLog>> GetByUserAsync(string userId, int page = 1, int pageSize = 50)
        => await _logs.Find(l => l.UserId == userId)
            .SortByDescending(l => l.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync();

    public async Task<List<AuditLog>> GetAllAsync(int page = 1, int pageSize = 50,
        string? action = null, string? userId = null)
    {
        var builder = Builders<AuditLog>.Filter;
        var filter = builder.Empty;

        if (!string.IsNullOrWhiteSpace(action))
            filter &= builder.Eq(l => l.Action, action);

        if (!string.IsNullOrWhiteSpace(userId))
            filter &= builder.Eq(l => l.UserId, userId);

        return await _logs.Find(filter)
            .SortByDescending(l => l.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync();
    }

    public async Task<long> CountAllAsync(string? action = null, string? userId = null)
    {
        var builder = Builders<AuditLog>.Filter;
        var filter = builder.Empty;

        if (!string.IsNullOrWhiteSpace(action))
            filter &= builder.Eq(l => l.Action, action);

        if (!string.IsNullOrWhiteSpace(userId))
            filter &= builder.Eq(l => l.UserId, userId);

        return await _logs.CountDocumentsAsync(filter);
    }
}
