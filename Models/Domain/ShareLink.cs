using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace FileVault.Web.Models.Domain;

[BsonIgnoreExtraElements]
public class ShareLink
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = null!;

    [BsonElement("fileId")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string FileId { get; set; } = null!;

    [BsonElement("ownerUserId")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string OwnerUserId { get; set; } = null!;

    [BsonElement("token")]
    public string Token { get; set; } = null!;

    [BsonElement("passwordHash")]
    public string? PasswordHash { get; set; }

    [BsonElement("expiresAt")]
    public DateTime? ExpiresAt { get; set; }

    [BsonElement("allowDownload")]
    public bool AllowDownload { get; set; } = true;

    [BsonElement("accessCount")]
    public int AccessCount { get; set; }

    [BsonElement("lastAccessedAt")]
    public DateTime? LastAccessedAt { get; set; }

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("isRevoked")]
    public bool IsRevoked { get; set; }
}
