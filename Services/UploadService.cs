using System.Security.Cryptography;
using MongoDB.Bson;
using FileVault.Web.Models.Domain;
using FileVault.Web.Models.Settings;
using FileVault.Web.Data.Repositories;
using FileVault.Web.Data.GridFs;
using FileVault.Web.Helpers;
using Microsoft.Extensions.Options;

namespace FileVault.Web.Services;

public class UploadService : IUploadService
{
    private readonly IUploadSessionRepository _sessionRepo;
    private readonly IFileRepository _fileRepo;
    private readonly IGridFsService _gridFs;
    private readonly IVirusScanService _virusScan;
    private readonly IAuditService _auditService;
    private readonly AppSettings _appSettings;
    private readonly ILogger<UploadService> _logger;
    private readonly string _tempPath;

    public UploadService(
        IUploadSessionRepository sessionRepo,
        IFileRepository fileRepo,
        IGridFsService gridFs,
        IVirusScanService virusScan,
        IAuditService auditService,
        IOptions<AppSettings> appSettings,
        ILogger<UploadService> logger)
    {
        _sessionRepo = sessionRepo;
        _fileRepo = fileRepo;
        _gridFs = gridFs;
        _virusScan = virusScan;
        _auditService = auditService;
        _appSettings = appSettings.Value;
        _logger = logger;
        _tempPath = Path.Combine(Directory.GetCurrentDirectory(), _appSettings.TempUploadPath);
        Directory.CreateDirectory(_tempPath);
    }

    public async Task<UploadSession> InitiateAsync(string userId, string fileName, string contentType,
        long totalSize, int chunkSize, string? folderId)
    {
        var sanitizedName = FileHelpers.SanitizeFileName(fileName);
        var ext = Path.GetExtension(sanitizedName).ToLowerInvariant();

        // Check blocked extensions
        var blocked = _appSettings.BlockedFileExtensions
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(e => e.Trim().ToLowerInvariant());
        if (blocked.Contains(ext))
            throw new InvalidOperationException($"File type '{ext}' is not allowed.");

        if (chunkSize <= 0)
            chunkSize = _appSettings.MaxChunkSizeBytes;

        var totalChunks = (int)Math.Ceiling((double)totalSize / chunkSize);

        var session = new UploadSession
        {
            UserId = userId,
            FileName = sanitizedName,
            ContentType = contentType,
            FolderId = folderId,
            TotalSize = totalSize,
            ChunkSize = chunkSize,
            TotalChunks = totalChunks,
            ExpiresAt = DateTime.UtcNow.AddMinutes(_appSettings.UploadSessionExpiryMinutes)
        };

        await _sessionRepo.CreateAsync(session);
        Directory.CreateDirectory(GetSessionPath(session.Id));

        _logger.LogInformation("Upload session initiated: {SessionId} for {FileName} ({Chunks} chunks)",
            session.Id, sanitizedName, totalChunks);
        return session;
    }

    public async Task<bool> UploadChunkAsync(string sessionId, int chunkIndex, Stream chunkData, string userId)
    {
        var session = await _sessionRepo.GetByIdAsync(sessionId);
        if (session == null || session.UserId != userId)
            throw new UnauthorizedAccessException("Upload session not found.");

        if (session.Status == UploadStatus.Completed || session.Status == UploadStatus.Aborted)
            throw new InvalidOperationException("Upload session is no longer active.");

        if (chunkIndex < 0 || chunkIndex >= session.TotalChunks)
            throw new ArgumentOutOfRangeException(nameof(chunkIndex));

        var chunkPath = GetChunkPath(sessionId, chunkIndex);
        await using (var fs = new FileStream(chunkPath, FileMode.Create, FileAccess.Write, FileShare.None, 81920))
        {
            await chunkData.CopyToAsync(fs);
        }

        session.UploadedChunks.Add(chunkIndex);
        session.Status = UploadStatus.InProgress;
        await _sessionRepo.UpdateAsync(session);

        _logger.LogDebug("Chunk {Index}/{Total} uploaded for session {SessionId}",
            chunkIndex + 1, session.TotalChunks, sessionId);

        return session.UploadedChunks.Count == session.TotalChunks;
    }

    public async Task<FileItem?> CompleteAsync(string sessionId, string userId)
    {
        var session = await _sessionRepo.GetByIdAsync(sessionId);
        if (session == null || session.UserId != userId)
            throw new UnauthorizedAccessException("Upload session not found.");

        if (session.UploadedChunks.Count != session.TotalChunks)
            throw new InvalidOperationException(
                $"Upload incomplete: {session.UploadedChunks.Count}/{session.TotalChunks} chunks received.");

        session.Status = UploadStatus.Completing;
        await _sessionRepo.UpdateAsync(session);

        try
        {
            // Assemble chunks and stream to GridFS
            var sessionPath = GetSessionPath(sessionId);
            using var sha256 = SHA256.Create();
            ObjectId gridFsId;

            // Create a combined stream that reads chunks sequentially
            using (var combinedStream = new CombinedChunkStream(sessionPath, session.TotalChunks))
            {
                // Compute hash while uploading
                using var hashStream = new CryptoStream(combinedStream, sha256, CryptoStreamMode.Read);
                gridFsId = await _gridFs.UploadFromStreamAsync(
                    session.FileName, hashStream, session.ContentType);
            }

            var hash = BitConverter.ToString(sha256.Hash!).Replace("-", "").ToLowerInvariant();

            // Create file record
            var file = new FileItem
            {
                OwnerUserId = userId,
                FolderId = session.FolderId,
                FileName = session.FileName,
                OriginalFileName = session.FileName,
                Extension = Path.GetExtension(session.FileName).ToLowerInvariant(),
                ContentType = session.ContentType,
                SizeBytes = session.TotalSize,
                GridFsFileId = gridFsId.ToString(),
                HashSha256 = hash
            };

            await _fileRepo.CreateAsync(file);

            session.Status = UploadStatus.Completed;
            session.CompletedAt = DateTime.UtcNow;
            session.GridFsFileId = gridFsId.ToString();
            session.HashSha256 = hash;
            await _sessionRepo.UpdateAsync(session);

            // Cleanup temp files
            CleanupSessionFiles(sessionId);

            await _auditService.LogAsync(userId, null, "FileUploaded", "File", file.Id,
                new Dictionary<string, string>
                {
                    { "fileName", file.FileName },
                    { "size", file.SizeBytes.ToString() }
                });

            _logger.LogInformation("Upload completed: {FileName} ({Size} bytes), GridFS: {GridFsId}",
                file.FileName, file.SizeBytes, gridFsId);

            return file;
        }
        catch (Exception ex)
        {
            session.Status = UploadStatus.Failed;
            await _sessionRepo.UpdateAsync(session);
            _logger.LogError(ex, "Upload completion failed for session {SessionId}", sessionId);
            throw;
        }
    }

    public async Task AbortAsync(string sessionId, string userId)
    {
        var session = await _sessionRepo.GetByIdAsync(sessionId);
        if (session == null || session.UserId != userId) return;

        session.Status = UploadStatus.Aborted;
        await _sessionRepo.UpdateAsync(session);
        CleanupSessionFiles(sessionId);

        _logger.LogInformation("Upload aborted: {SessionId}", sessionId);
    }

    public async Task<UploadSession?> GetSessionAsync(string sessionId, string userId)
    {
        var session = await _sessionRepo.GetByIdAsync(sessionId);
        if (session == null || session.UserId != userId) return null;
        return session;
    }

    public async Task<List<UploadSession>> GetActiveSessionsAsync(string userId)
        => await _sessionRepo.GetActiveByUserAsync(userId);

    public async Task CleanupExpiredAsync()
    {
        var expired = await _sessionRepo.GetExpiredSessionsAsync();
        foreach (var session in expired)
        {
            session.Status = UploadStatus.Aborted;
            await _sessionRepo.UpdateAsync(session);
            CleanupSessionFiles(session.Id);
        }

        if (expired.Count > 0)
            _logger.LogInformation("Cleaned up {Count} expired upload sessions", expired.Count);
    }

    private string GetSessionPath(string sessionId) => Path.Combine(_tempPath, sessionId);
    private string GetChunkPath(string sessionId, int index) => Path.Combine(GetSessionPath(sessionId), $"chunk_{index}");

    private void CleanupSessionFiles(string sessionId)
    {
        var path = GetSessionPath(sessionId);
        if (Directory.Exists(path))
        {
            try { Directory.Delete(path, true); }
            catch (Exception ex) { _logger.LogWarning(ex, "Failed to cleanup temp files for session {Id}", sessionId); }
        }
    }
}

/// <summary>
/// A stream that reads chunk files sequentially without loading all into memory.
/// </summary>
internal class CombinedChunkStream : Stream
{
    private readonly string _sessionPath;
    private readonly int _totalChunks;
    private int _currentChunk;
    private FileStream? _currentStream;

    public CombinedChunkStream(string sessionPath, int totalChunks)
    {
        _sessionPath = sessionPath;
        _totalChunks = totalChunks;
        _currentChunk = 0;
    }

    public override bool CanRead => true;
    public override bool CanSeek => false;
    public override bool CanWrite => false;
    public override long Length => throw new NotSupportedException();
    public override long Position
    {
        get => throw new NotSupportedException();
        set => throw new NotSupportedException();
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        int totalRead = 0;
        while (totalRead < count && _currentChunk < _totalChunks)
        {
            if (_currentStream == null)
            {
                var chunkPath = Path.Combine(_sessionPath, $"chunk_{_currentChunk}");
                _currentStream = new FileStream(chunkPath, FileMode.Open, FileAccess.Read,
                    FileShare.Read, 81920);
            }

            int bytesRead = _currentStream.Read(buffer, offset + totalRead, count - totalRead);
            if (bytesRead == 0)
            {
                _currentStream.Dispose();
                _currentStream = null;
                _currentChunk++;
            }
            else
            {
                totalRead += bytesRead;
            }
        }
        return totalRead;
    }

    public override void Flush() { }
    public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
    public override void SetLength(long value) => throw new NotSupportedException();
    public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

    protected override void Dispose(bool disposing)
    {
        _currentStream?.Dispose();
        base.Dispose(disposing);
    }
}
