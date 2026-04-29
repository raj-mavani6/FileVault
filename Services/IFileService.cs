using FileVault.Web.Models.Domain;

namespace FileVault.Web.Services;

public interface IFileService
{
    Task<FileItem?> GetByIdAsync(string id, string userId);
    Task<(List<FileItem> Files, long Total)> GetFilesAsync(string userId, string? folderId,
        int page = 1, int pageSize = 50, string? sortBy = null, bool sortDesc = true,
        string? search = null, string? extension = null, string? tag = null);
    Task<(List<FileItem> Files, long Total)> GetTrashAsync(string userId, int page = 1, int pageSize = 50);
    Task<List<FileItem>> GetRecentAsync(string userId, int count = 10);
    Task<FileItem> CreateFileRecordAsync(string userId, string? folderId, string originalFileName,
        string contentType, long sizeBytes, string gridFsId, string? hash);
    Task UpdateAsync(FileItem file, string userId);
    Task SoftDeleteAsync(string id, string userId);
    Task RestoreAsync(string id, string userId);
    Task PermanentDeleteAsync(string id, string userId);
    Task EmptyTrashAsync(string userId);
    Task<long> GetUserStorageAsync(string userId);
    Task<long> GetUserFileCountAsync(string userId);
    Task<Dictionary<string, long>> GetExtensionStatsAsync(string userId);
    Task<Stream> DownloadFileAsync(string fileId, string userId);
    Task<(Stream Stream, string ContentType, string FileName, long Size)> GetFileStreamAsync(string fileId, string userId);
}
