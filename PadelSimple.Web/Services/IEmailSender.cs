namespace PadelSimple.Web.Services;

public interface IAppEmailSender
{
    Task SendAsync(string to, string subject, string htmlBody);
}
