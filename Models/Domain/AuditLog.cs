using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace FileVault.Web.Models.Domain;

public class AuditLog
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = null!;

    [BsonElement("userId")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? UserId { get; set; }

    [BsonElement("userEmail")]
    public string? UserEmail { get; set; }

    [BsonElement("action")]
    public string Action { get; set; } = null!;

    [BsonElement("targetType")]
    public string? TargetType { get; set; }

    [BsonElement("targetId")]
    public string? TargetId { get; set; }

    [BsonElement("metadata")]
    public Dictionary<string, string>? Metadata { get; set; }

    [BsonElement("ipAddress")]
    public string? IpAddress { get; set; }

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
