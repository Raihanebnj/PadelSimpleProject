namespace PadelSimple.Web.Services;

public class SmtpEmailSender : IAppEmailSender
{
    private readonly ILogger<SmtpEmailSender> _logger;

    public SmtpEmailSender(ILogger<SmtpEmailSender> logger)
    {
        _logger = logger;
    }

    public Task SendAsync(string to, string subject, string htmlBody)
    {
        _logger.LogInformation("MAIL to={to} subject={subject}", to, subject);
        return Task.CompletedTask;
    }
}
