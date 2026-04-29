using FileVault.Web.Models.Domain;

namespace FileVault.Web.Data.Repositories;

public interface IAuditLogRepository
{
    Task CreateAsync(AuditLog log);
    Task<List<AuditLog>> GetRecentAsync(int count = 50);
    Task<List<AuditLog>> GetByUserAsync(string userId, int page = 1, int pageSize = 50);
    Task<List<AuditLog>> GetAllAsync(int page = 1, int pageSize = 50, string? action = null, string? userId = null);
    Task<long> CountAllAsync(string? action = null, string? userId = null);
}
