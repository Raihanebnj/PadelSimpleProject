using System.Net;
using System.Net.Mail;

namespace PadelSimple.Web.Services;

public class SmtpEmailSender : IEmailSender
{
    private readonly IConfiguration _cfg;
    private readonly ILogger<SmtpEmailSender> _logger;

    public SmtpEmailSender(IConfiguration cfg, ILogger<SmtpEmailSender> logger)
    {
        _cfg = cfg;
        _logger = logger;
    }

    public async Task SendAsync(string toEmail, string subject, string htmlBody)
    {
        // secrets:
        // Email:SmtpHost, Email:SmtpPort, Email:User, Email:Pass, Email:From
        var host = _cfg["Email:SmtpHost"];
        var port = int.Parse(_cfg["Email:SmtpPort"] ?? "587");
        var user = _cfg["Email:User"];
        var pass = _cfg["Email:Pass"];
        var from = _cfg["Email:From"] ?? user;

        if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(user) || string.IsNullOrWhiteSpace(pass))
            throw new InvalidOperationException("E-mail settings ontbreken in User Secrets.");

        using var client = new SmtpClient(host, port)
        {
            Credentials = new NetworkCredential(user, pass),
            EnableSsl = true
        };

        var msg = new MailMessage(from!, toEmail, subject, htmlBody) { IsBodyHtml = true };
        await client.SendMailAsync(msg);

        _logger.LogInformation("Sent email to {To}", toEmail);
    }
}
