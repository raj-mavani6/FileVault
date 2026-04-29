using FileVault.Web.Models.Domain;

namespace FileVault.Web.Services;

public interface IUploadService
{
    Task<UploadSession> InitiateAsync(string userId, string fileName, string contentType,
        long totalSize, int chunkSize, string? folderId);
    Task<bool> UploadChunkAsync(string sessionId, int chunkIndex, Stream chunkData, string userId);
    Task<FileItem?> CompleteAsync(string sessionId, string userId);
    Task AbortAsync(string sessionId, string userId);
    Task<UploadSession?> GetSessionAsync(string sessionId, string userId);
    Task<List<UploadSession>> GetActiveSessionsAsync(string userId);
    Task CleanupExpiredAsync();
}
