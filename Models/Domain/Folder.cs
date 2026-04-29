using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace FileVault.Web.Models.Domain;

public class Folder
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = null!;

    [BsonElement("ownerUserId")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string OwnerUserId { get; set; } = null!;

    [BsonElement("parentFolderId")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? ParentFolderId { get; set; }

    [BsonElement("name")]
    public string Name { get; set; } = null!;

    [BsonElement("path")]
    public string Path { get; set; } = "/";

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("updatedAt")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("isDeleted")]
    public bool IsDeleted { get; set; }

    [BsonElement("deletedAt")]
    public DateTime? DeletedAt { get; set; }
}
