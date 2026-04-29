namespace FileVault.Web.Services;

public interface IVirusScanService
{
    Task<(bool IsSafe, string? Reason)> ScanAsync(Stream fileStream, string fileName);
}

public class NoOpVirusScanService : IVirusScanService
{
    private readonly ILogger<NoOpVirusScanService> _logger;

    public NoOpVirusScanService(ILogger<NoOpVirusScanService> logger)
    {
        _logger = logger;
    }

    public Task<(bool IsSafe, string? Reason)> ScanAsync(Stream fileStream, string fileName)
    {
        _logger.LogDebug("Virus scan skipped (NoOp) for: {FileName}", fileName);
        return Task.FromResult((true, (string?)null));
    }
}
