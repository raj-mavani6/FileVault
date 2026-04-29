using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace FileVault.Web.Models.Domain;

public class FileItem
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = null!;

    [BsonElement("ownerUserId")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string OwnerUserId { get; set; } = null!;

    [BsonElement("folderId")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? FolderId { get; set; }

    [BsonElement("fileName")]
    public string FileName { get; set; } = null!;

    [BsonElement("originalFileName")]
    public string OriginalFileName { get; set; } = null!;

    [BsonElement("extension")]
    public string Extension { get; set; } = string.Empty;

    [BsonElement("contentType")]
    public string ContentType { get; set; } = "application/octet-stream";

    [BsonElement("sizeBytes")]
    public long SizeBytes { get; set; }

    [BsonElement("gridFsFileId")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string GridFsFileId { get; set; } = null!;

    [BsonElement("hashSha256")]
    public string? HashSha256 { get; set; }

    [BsonElement("tags")]
    public List<string> Tags { get; set; } = new();

    [BsonElement("description")]
    public string? Description { get; set; }

    [BsonElement("isDeleted")]
    public bool IsDeleted { get; set; }

    [BsonElement("deletedAt")]
    public DateTime? DeletedAt { get; set; }

    [BsonElement("versionNumber")]
    public int VersionNumber { get; set; } = 1;

    [BsonElement("previousVersionId")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? PreviousVersionId { get; set; }

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("updatedAt")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
