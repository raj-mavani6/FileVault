using FileVault.Web.Models.Domain;
using FileVault.Web.Data.Repositories;
using FileVault.Web.Helpers;
using FileVault.Web.Models.Settings;
using Microsoft.Extensions.Options;

namespace FileVault.Web.Services;

public class ShareService : IShareService
{
    private readonly IShareLinkRepository _shareRepo;
    private readonly IFileRepository _fileRepo;
    private readonly IAuditService _auditService;
    private readonly AppSettings _appSettings;
    private readonly ILogger<ShareService> _logger;

    public ShareService(IShareLinkRepository shareRepo, IFileRepository fileRepo,
        IAuditService auditService, IOptions<AppSettings> appSettings, ILogger<ShareService> logger)
    {
        _shareRepo = shareRepo;
        _fileRepo = fileRepo;
        _auditService = auditService;
        _appSettings = appSettings.Value;
        _logger = logger;
    }

    public async Task<ShareLink> CreateShareLinkAsync(string fileId, string userId,
        bool allowDownload, DateTime? expiresAt, string? password)
    {
        var file = await _fileRepo.GetByIdAsync(fileId);
        if (file == null || file.OwnerUserId != userId)
            throw new UnauthorizedAccessException("File not found or access denied.");

        var link = new ShareLink
        {
            FileId = fileId,
            OwnerUserId = userId,
            Token = HashHelper.GenerateToken(16),
            AllowDownload = allowDownload,
            ExpiresAt = expiresAt ?? DateTime.UtcNow.AddDays(_appSettings.ShareLinkDefaultExpiryDays),
            PasswordHash = password != null ? BCrypt.Net.BCrypt.HashPassword(password) : null
        };

        await _shareRepo.CreateAsync(link);
        await _auditService.LogAsync(userId, null, "ShareLinkCreated", "ShareLink", link.Id,
            new Dictionary<string, string> { { "fileId", fileId }, { "token", link.Token } });

        return link;
    }

    public async Task<ShareLink?> GetByTokenAsync(string token)
        => await _shareRepo.GetByTokenAsync(token);

    public async Task<(bool Valid, string? Message)> ValidateShareAccessAsync(string token, string? password)
    {
        var link = await _shareRepo.GetByTokenAsync(token);
        if (link == null)
            return (false, "Share link not found.");

        if (link.IsRevoked)
            return (false, "This share link has been revoked.");

        if (link.ExpiresAt.HasValue && link.ExpiresAt.Value < DateTime.UtcNow)
            return (false, "This share link has expired.");

        if (link.PasswordHash != null)
        {
            if (string.IsNullOrEmpty(password) || !BCrypt.Net.BCrypt.Verify(password, link.PasswordHash))
                return (false, "Invalid password.");
        }

        return (true, null);
    }

    public async Task<List<ShareLink>> GetUserSharesAsync(string userId, int page = 1, int pageSize = 50)
        => await _shareRepo.GetByUserAsync(userId, page, pageSize);

    public async Task<long> CountUserSharesAsync(string userId)
        => await _shareRepo.CountByUserAsync(userId);

    public async Task RevokeAsync(string id, string userId)
    {
        var link = await _shareRepo.GetByIdAsync(id);
        if (link == null || link.OwnerUserId != userId) return;

        link.IsRevoked = true;
        await _shareRepo.UpdateAsync(link);
        await _auditService.LogAsync(userId, null, "ShareLinkRevoked", "ShareLink", id);
    }

    public async Task RecordAccessAsync(string token)
    {
        var link = await _shareRepo.GetByTokenAsync(token);
        if (link == null) return;

        link.AccessCount++;
        link.LastAccessedAt = DateTime.UtcNow;
        await _shareRepo.UpdateAsync(link);
    }
}
