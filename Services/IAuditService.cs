using FileVault.Web.Models.Domain;

namespace FileVault.Web.Services;

public interface IAuditService
{
    Task LogAsync(string? userId, string? userEmail, string action,
        string? targetType = null, string? targetId = null,
        Dictionary<string, string>? metadata = null, string? ipAddress = null);
    Task<List<AuditLog>> GetRecentAsync(int count = 50);
    Task<List<AuditLog>> GetAllAsync(int page = 1, int pageSize = 50,
        string? action = null, string? userId = null);
    Task<long> CountAllAsync(string? action = null, string? userId = null);
}
