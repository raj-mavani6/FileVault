using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace FileVault.Web.Models.Domain;

public class UploadSession
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = null!;

    [BsonElement("userId")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string UserId { get; set; } = null!;

    [BsonElement("fileName")]
    public string FileName { get; set; } = null!;

    [BsonElement("contentType")]
    public string ContentType { get; set; } = "application/octet-stream";

    [BsonElement("folderId")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? FolderId { get; set; }

    [BsonElement("totalSize")]
    public long TotalSize { get; set; }

    [BsonElement("chunkSize")]
    public int ChunkSize { get; set; } = 5 * 1024 * 1024; // 5MB default

    [BsonElement("totalChunks")]
    public int TotalChunks { get; set; }

    [BsonElement("uploadedChunks")]
    public HashSet<int> UploadedChunks { get; set; } = new();

    [BsonElement("status")]
    public UploadStatus Status { get; set; } = UploadStatus.Initiated;

    [BsonElement("gridFsFileId")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? GridFsFileId { get; set; }

    [BsonElement("hashSha256")]
    public string? HashSha256 { get; set; }

    [BsonElement("startedAt")]
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("expiresAt")]
    public DateTime ExpiresAt { get; set; }

    [BsonElement("completedAt")]
    public DateTime? CompletedAt { get; set; }
}

public enum UploadStatus
{
    Initiated = 0,
    InProgress = 1,
    Completing = 2,
    Completed = 3,
    Failed = 4,
    Aborted = 5
}
