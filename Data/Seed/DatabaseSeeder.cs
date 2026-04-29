using FileVault.Web.Models.Domain;
using FileVault.Web.Data.Repositories;

namespace FileVault.Web.Data.Seed;

public class DatabaseSeeder
{
    private readonly IUserRepository _userRepo;
    private readonly MongoDbContext _context;
    private readonly ILogger<DatabaseSeeder> _logger;

    public DatabaseSeeder(IUserRepository userRepo, MongoDbContext context, ILogger<DatabaseSeeder> logger)
    {
        _userRepo = userRepo;
        _context = context;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        await _context.EnsureIndexesAsync();
        _logger.LogInformation("MongoDB indexes ensured");

        var adminEmail = "admin@filevault.local";
        var existingAdmin = await _userRepo.GetByEmailAsync(adminEmail);

        if (existingAdmin == null)
        {
            var admin = new AppUser
            {
                FullName = "System Admin",
                Email = adminEmail,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                EmailConfirmed = true,
                Roles = new List<string> { "Admin", "User" },
                IsActive = true
            };

            await _userRepo.CreateAsync(admin);
            _logger.LogInformation("Default admin user created: {Email} / Admin@123", adminEmail);
        }
        else
        {
            _logger.LogInformation("Admin user already exists");
        }
    }
}
