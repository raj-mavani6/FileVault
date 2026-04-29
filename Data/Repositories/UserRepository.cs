using MongoDB.Driver;
using FileVault.Web.Models.Domain;

namespace FileVault.Web.Data.Repositories;

public class UserRepository : IUserRepository
{
    private readonly IMongoCollection<AppUser> _users;

    public UserRepository(MongoDbContext context)
    {
        _users = context.Users;
    }

    public async Task<AppUser?> GetByIdAsync(string id)
        => await _users.Find(u => u.Id == id).FirstOrDefaultAsync();

    public async Task<AppUser?> GetByEmailAsync(string email)
        => await _users.Find(u => u.Email == email.ToLowerInvariant()).FirstOrDefaultAsync();

    public async Task<List<AppUser>> GetAllAsync(int page = 1, int pageSize = 20)
        => await _users.Find(_ => true)
            .SortByDescending(u => u.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync();

    public async Task<long> CountAsync()
        => await _users.CountDocumentsAsync(_ => true);

    public async Task CreateAsync(AppUser user)
    {
        user.Email = user.Email.ToLowerInvariant();
        await _users.InsertOneAsync(user);
    }

    public async Task UpdateAsync(AppUser user)
    {
        user.UpdatedAt = DateTime.UtcNow;
        await _users.ReplaceOneAsync(u => u.Id == user.Id, user);
    }

    public async Task DeleteAsync(string id)
        => await _users.DeleteOneAsync(u => u.Id == id);

    public async Task<AppUser?> GetByResetTokenAsync(string token)
        => await _users.Find(u => u.PasswordResetToken == token).FirstOrDefaultAsync();

    public async Task<AppUser?> GetByConfirmTokenAsync(string token)
        => await _users.Find(u => u.EmailConfirmToken == token).FirstOrDefaultAsync();
}
