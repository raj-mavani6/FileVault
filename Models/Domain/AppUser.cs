using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace FileVault.Web.Models.Domain;

public class AppUser
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = null!;

    [BsonElement("fullName")]
    public string FullName { get; set; } = null!;

    [BsonElement("email")]
    public string Email { get; set; } = null!;

    [BsonElement("passwordHash")]
    public string PasswordHash { get; set; } = null!;

    [BsonElement("emailConfirmed")]
    public bool EmailConfirmed { get; set; }

    [BsonElement("emailConfirmToken")]
    public string? EmailConfirmToken { get; set; }

    [BsonElement("passwordResetToken")]
    public string? PasswordResetToken { get; set; }

    [BsonElement("passwordResetExpiry")]
    public DateTime? PasswordResetExpiry { get; set; }

    [BsonElement("roles")]
    public List<string> Roles { get; set; } = new() { "User" };

    [BsonElement("isActive")]
    public bool IsActive { get; set; } = true;

    [BsonElement("avatarGridFsId")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? AvatarGridFsId { get; set; }

    [BsonElement("storageUsedBytes")]
    public long StorageUsedBytes { get; set; }

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("updatedAt")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
