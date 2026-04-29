namespace FileVault.Web.Services;

public interface IEmailService
{
    Task SendEmailConfirmationAsync(string email, string name, string token);
    Task SendPasswordResetAsync(string email, string name, string token);
}

public class ConsoleEmailService : IEmailService
{
    private readonly ILogger<ConsoleEmailService> _logger;

    public ConsoleEmailService(ILogger<ConsoleEmailService> logger)
    {
        _logger = logger;
    }

    public Task SendEmailConfirmationAsync(string email, string name, string token)
    {
        _logger.LogInformation(
            "📧 EMAIL CONFIRMATION for {Name} ({Email}): /Auth/ConfirmEmail?token={Token}",
            name, email, token);
        return Task.CompletedTask;
    }

    public Task SendPasswordResetAsync(string email, string name, string token)
    {
        _logger.LogInformation(
            "📧 PASSWORD RESET for {Name} ({Email}): /Auth/ResetPassword?token={Token}",
            name, email, token);
        return Task.CompletedTask;
    }
}
