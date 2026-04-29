using Serilog;
using AspNetCoreRateLimit;
using FileVault.Web.Data;
using FileVault.Web.Data.GridFs;
using FileVault.Web.Data.Repositories;
using FileVault.Web.Data.Seed;
using FileVault.Web.Middleware;
using FileVault.Web.Models.Settings;
using FileVault.Web.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Server.Kestrel.Core;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Serilog
    builder.Host.UseSerilog((ctx, lc) => lc.ReadFrom.Configuration(ctx.Configuration));

    // Configuration bindings
    builder.Services.Configure<MongoDbSettings>(builder.Configuration.GetSection("MongoDb"));
    builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("AppSettings"));

    // Remove request size limits for large file uploads
    builder.Services.Configure<KestrelServerOptions>(options =>
    {
        options.Limits.MaxRequestBodySize = null; // No limit
    });
    builder.Services.Configure<FormOptions>(options =>
    {
        options.MultipartBodyLengthLimit = long.MaxValue;
        options.ValueLengthLimit = int.MaxValue;
        options.MemoryBufferThreshold = int.MaxValue;
    });

    // MongoDB
    builder.Services.AddSingleton<MongoDbContext>();

    // Repositories
    builder.Services.AddScoped<IUserRepository, UserRepository>();
    builder.Services.AddScoped<IFileRepository, FileRepository>();
    builder.Services.AddScoped<IFolderRepository, FolderRepository>();
    builder.Services.AddScoped<IUploadSessionRepository, UploadSessionRepository>();
    builder.Services.AddScoped<IShareLinkRepository, ShareLinkRepository>();
    builder.Services.AddScoped<IAuditLogRepository, AuditLogRepository>();

    // GridFS
    builder.Services.AddScoped<IGridFsService, GridFsService>();

    // Services
    builder.Services.AddScoped<IAuthService, AuthService>();
    builder.Services.AddScoped<IFileService, FileService>();
    builder.Services.AddScoped<IFolderService, FolderService>();
    builder.Services.AddScoped<IUploadService, UploadService>();
    builder.Services.AddScoped<IShareService, ShareService>();
    builder.Services.AddScoped<IAuditService, AuditService>();
    builder.Services.AddScoped<IVirusScanService, NoOpVirusScanService>();
    builder.Services.AddScoped<IEmailService, ConsoleEmailService>();
    builder.Services.AddTransient<DatabaseSeeder>();

    // Authentication
    builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
        .AddCookie(options =>
        {
            options.LoginPath = "/Auth/Login";
            options.LogoutPath = "/Auth/Logout";
            options.AccessDeniedPath = "/Auth/Login";
            options.ExpireTimeSpan = TimeSpan.FromDays(7);
            options.SlidingExpiration = true;
            options.Cookie.HttpOnly = true;
            options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
            options.Cookie.SameSite = SameSiteMode.Lax;
            options.Cookie.Name = "FileVault.Auth";
        });

    builder.Services.AddAuthorization();

    // Rate Limiting
    builder.Services.AddMemoryCache();
    builder.Services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("IpRateLimiting"));
    builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
    builder.Services.AddInMemoryRateLimiting();

    // Anti-forgery
    builder.Services.AddAntiforgery(options =>
    {
        options.HeaderName = "X-CSRF-TOKEN";
        options.Cookie.Name = "FileVault.Antiforgery";
    });

    builder.Services.AddControllersWithViews()
        .AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        });

    var app = builder.Build();

    // Seed database
    using (var scope = app.Services.CreateScope())
    {
        var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();
        await seeder.SeedAsync();
    }

    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Home/Error");
        app.UseHsts();
    }

    app.UseMiddleware<SecurityHeadersMiddleware>();
    app.UseMiddleware<ExceptionHandlingMiddleware>();
    app.UseIpRateLimiting();

    app.UseHttpsRedirection();
    app.UseStaticFiles();
    app.UseRouting();

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllerRoute(
        name: "shareLink",
        pattern: "s/{token}",
        defaults: new { controller = "Share", action = "PublicView" });

    app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");

    Log.Information("FileVault is starting on {Urls}", string.Join(", ",
        app.Urls.Any() ? app.Urls : new[] { "https://localhost:5001" }));

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application startup failed");
}
finally
{
    Log.CloseAndFlush();
}
