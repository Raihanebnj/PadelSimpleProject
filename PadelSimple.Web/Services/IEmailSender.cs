namespace PadelSimple.Web.Services;

/// <summary>
/// Interface voor het verzenden van e-mails vanuit de applicatie.
/// </summary>
public interface IEmailSender
{
    /// <summary>
    /// Verzendt een e-mail asynchroon.
    /// </summary>
    /// <param name="naar">Het e-mailadres van de ontvanger.</param>
    /// <param name="onderwerp">Het onderwerp van de e-mail.</param>
    /// <param name="htmlInhoud">De HTML-inhoud van de e-mail.</param>
    Task VerzendAsync(string naar, string onderwerp, string htmlInhoud);
}
