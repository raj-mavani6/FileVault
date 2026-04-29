using MongoDB.Bson;
using FileVault.Web.Models.Domain;
using FileVault.Web.Data.Repositories;
using FileVault.Web.Data.GridFs;
using FileVault.Web.Helpers;

namespace FileVault.Web.Services;

public class FileService : IFileService
{
    private readonly IFileRepository _fileRepo;
    private readonly IGridFsService _gridFs;
    private readonly IAuditService _auditService;
    private readonly ILogger<FileService> _logger;

    public FileService(IFileRepository fileRepo, IGridFsService gridFs,
        IAuditService auditService, ILogger<FileService> logger)
    {
        _fileRepo = fileRepo;
        _gridFs = gridFs;
        _auditService = auditService;
        _logger = logger;
    }

    public async Task<FileItem?> GetByIdAsync(string id, string userId)
    {
        var file = await _fileRepo.GetByIdAsync(id);
        if (file == null || file.OwnerUserId != userId) return null;
        return file;
    }

    public async Task<(List<FileItem> Files, long Total)> GetFilesAsync(string userId, string? folderId,
        int page, int pageSize, string? sortBy, bool sortDesc,
        string? search, string? extension, string? tag)
    {
        var files = await _fileRepo.GetByFolderAsync(userId, folderId, false,
            page, pageSize, sortBy, sortDesc, search, extension, tag);
        var total = await _fileRepo.CountByFolderAsync(userId, folderId, false,
            search, extension, tag);
        return (files, total);
    }

    public async Task<(List<FileItem> Files, long Total)> GetTrashAsync(string userId, int page, int pageSize)
    {
        var files = await _fileRepo.GetDeletedByUserAsync(userId, page, pageSize);
        var total = await _fileRepo.CountDeletedByUserAsync(userId);
        return (files, total);
    }

    public async Task<List<FileItem>> GetRecentAsync(string userId, int count)
        => await _fileRepo.GetRecentByUserAsync(userId, count);

    public async Task<FileItem> CreateFileRecordAsync(string userId, string? folderId,
        string originalFileName, string contentType, long sizeBytes, string gridFsId, string? hash)
    {
        var sanitized = FileHelpers.SanitizeFileName(originalFileName);
        var ext = Path.GetExtension(sanitized).ToLowerInvariant();

        var file = new FileItem
        {
            OwnerUserId = userId,
            FolderId = folderId,
            FileName = sanitized,
            OriginalFileName = originalFileName,
            Extension = ext,
            ContentType = contentType,
            SizeBytes = sizeBytes,
            GridFsFileId = gridFsId,
            HashSha256 = hash
        };

        await _fileRepo.CreateAsync(file);
        await _auditService.LogAsync(userId, null, "FileUploaded", "File", file.Id,
            new Dictionary<string, string> { { "fileName", sanitized }, { "size", sizeBytes.ToString() } });

        _logger.LogInformation("File created: {FileName} ({Size} bytes) by user {UserId}", sanitized, sizeBytes, userId);
        return file;
    }

    public async Task UpdateAsync(FileItem file, string userId)
    {
        if (file.OwnerUserId != userId) return;
        await _fileRepo.UpdateAsync(file);
        await _auditService.LogAsync(userId, null, "FileUpdated", "File", file.Id);
    }

    public async Task SoftDeleteAsync(string id, string userId)
    {
        var file = await _fileRepo.GetByIdAsync(id);
        if (file == null || file.OwnerUserId != userId) return;

        file.IsDeleted = true;
        file.DeletedAt = DateTime.UtcNow;
        await _fileRepo.UpdateAsync(file);
        await _auditService.LogAsync(userId, null, "FileTrashed", "File", id);
    }

    public async Task RestoreAsync(string id, string userId)
    {
        var file = await _fileRepo.GetByIdAsync(id);
        if (file == null || file.OwnerUserId != userId) return;

        file.IsDeleted = false;
        file.DeletedAt = null;
        await _fileRepo.UpdateAsync(file);
        await _auditService.LogAsync(userId, null, "FileRestored", "File", id);
    }

    public async Task PermanentDeleteAsync(string id, string userId)
    {
        var file = await _fileRepo.GetByIdAsync(id);
        if (file == null || file.OwnerUserId != userId) return;

        if (ObjectId.TryParse(file.GridFsFileId, out var gridFsId))
        {
            try { await _gridFs.DeleteAsync(gridFsId); }
            catch (Exception ex) { _logger.LogWarning(ex, "Failed to delete GridFS file {Id}", file.GridFsFileId); }
        }

        await _fileRepo.DeletePermanentAsync(id);
        await _auditService.LogAsync(userId, null, "FileDeletedPermanently", "File", id);
    }

    public async Task EmptyTrashAsync(string userId)
    {
        var trashed = await _fileRepo.GetDeletedByUserAsync(userId, 1, 10000);
        foreach (var file in trashed)
        {
            if (ObjectId.TryParse(file.GridFsFileId, out var gridFsId))
            {
                try { await _gridFs.DeleteAsync(gridFsId); }
                catch (Exception ex) { _logger.LogWarning(ex, "Failed to delete GridFS file {Id}", file.GridFsFileId); }
            }
            await _fileRepo.DeletePermanentAsync(file.Id);
        }
        await _auditService.LogAsync(userId, null, "TrashEmptied", "File", null);
    }

    public async Task<long> GetUserStorageAsync(string userId)
        => await _fileRepo.GetTotalSizeByUserAsync(userId);

    public async Task<long> GetUserFileCountAsync(string userId)
        => await _fileRepo.CountByUserAsync(userId);

    public async Task<Dictionary<string, long>> GetExtensionStatsAsync(string userId)
        => await _fileRepo.GetExtensionStatsAsync(userId);

    public async Task<Stream> DownloadFileAsync(string fileId, string userId)
    {
        var file = await _fileRepo.GetByIdAsync(fileId);
        if (file == null || file.OwnerUserId != userId)
            throw new UnauthorizedAccessException("File not found or access denied.");

        var gridFsId = ObjectId.Parse(file.GridFsFileId);
        return await _gridFs.OpenDownloadStreamAsync(gridFsId);
    }

    public async Task<(Stream Stream, string ContentType, string FileName, long Size)> GetFileStreamAsync(
        string fileId, string userId)
    {
        var file = await _fileRepo.GetByIdAsync(fileId);
        if (file == null || file.OwnerUserId != userId)
            throw new UnauthorizedAccessException("File not found or access denied.");

        var gridFsId = ObjectId.Parse(file.GridFsFileId);
        var stream = await _gridFs.OpenDownloadStreamAsync(gridFsId);
        return (stream, file.ContentType, file.FileName, file.SizeBytes);
    }
}
