using FileVault.Web.Models.Domain;
using FileVault.Web.Data.Repositories;

namespace FileVault.Web.Services;

public class AuditService : IAuditService
{
    private readonly IAuditLogRepository _auditRepo;
    private readonly ILogger<AuditService> _logger;

    public AuditService(IAuditLogRepository auditRepo, ILogger<AuditService> logger)
    {
        _auditRepo = auditRepo;
        _logger = logger;
    }

    public async Task LogAsync(string? userId, string? userEmail, string action,
        string? targetType = null, string? targetId = null,
        Dictionary<string, string>? metadata = null, string? ipAddress = null)
    {
        var log = new AuditLog
        {
            UserId = userId,
            UserEmail = userEmail,
            Action = action,
            TargetType = targetType,
            TargetId = targetId,
            Metadata = metadata,
            IpAddress = ipAddress
        };

        await _auditRepo.CreateAsync(log);
        _logger.LogDebug("Audit: {Action} by {Email} on {Type}/{Id}",
            action, userEmail, targetType, targetId);
    }

    public async Task<List<AuditLog>> GetRecentAsync(int count = 50)
        => await _auditRepo.GetRecentAsync(count);

    public async Task<List<AuditLog>> GetAllAsync(int page = 1, int pageSize = 50,
        string? action = null, string? userId = null)
        => await _auditRepo.GetAllAsync(page, pageSize, action, userId);

    public async Task<long> CountAllAsync(string? action = null, string? userId = null)
        => await _auditRepo.CountAllAsync(action, userId);
}
