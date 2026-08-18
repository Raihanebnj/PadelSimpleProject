using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace PadelSimple.Web.Services;

/// <summary>
/// Concrete implementatie van IEmailSender via SMTP met MailKit.
/// Instellingen worden gelezen uit appsettings.json / user secrets.
/// </summary>
public class SmtpEmailSender : IEmailSender
{
    private readonly IConfiguration _config;
    private readonly ILogger<SmtpEmailSender> _logger;

    public SmtpEmailSender(IConfiguration config, ILogger<SmtpEmailSender> logger)
    {
        _config = config;
        _logger = logger;
    }

    /// <summary>
    /// Verzendt een HTML e-mail via de geconfigureerde SMTP-server.
    /// </summary>
    public async Task VerzendAsync(string naar, string onderwerp, string htmlInhoud)
    {
        try
        {
            var smtpHost = _config["Email:SmtpHost"] ?? "smtp.gmail.com";
            var smtpPort = int.Parse(_config["Email:SmtpPort"] ?? "587");
            var gebruikersnaam = _config["Email:Gebruikersnaam"];
            var wachtwoord = _config["Email:Wachtwoord"];
            var vanAdres = _config["Email:FromAddress"] ?? "noreply@padelsimple.be";
            var vanNaam = _config["Email:FromName"] ?? "PadelSimple";

            var bericht = new MimeMessage();
            bericht.From.Add(new MailboxAddress(vanNaam, vanAdres));
            bericht.To.Add(MailboxAddress.Parse(naar));
            bericht.Subject = onderwerp;

            var bodyBuilder = new BodyBuilder { HtmlBody = htmlInhoud };
            bericht.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();
            await client.ConnectAsync(smtpHost, smtpPort, SecureSocketOptions.StartTls);

            if (!string.IsNullOrWhiteSpace(gebruikersnaam) && !string.IsNullOrWhiteSpace(wachtwoord))
            {
                await client.AuthenticateAsync(gebruikersnaam, wachtwoord);
            }

            await client.SendAsync(bericht);
            await client.DisconnectAsync(true);

            _logger.LogInformation("E-mail verstuurd naar {Naar} met onderwerp '{Onderwerp}'.", naar, onderwerp);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fout bij verzenden van e-mail naar {Naar}.", naar);
            // Gooi de fout niet opnieuw — e-mail is optioneel en mag de applicatie niet blokkeren.
        }
    }
}
