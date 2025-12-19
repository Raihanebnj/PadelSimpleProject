namespace PadelSimple.Web.Infrastructure;

public class CultureCookieMiddleware
{
    private readonly RequestDelegate _next;

    public CultureCookieMiddleware(RequestDelegate next) => _next = next;

    public async Task Invoke(HttpContext context)
    {
       
        var lang = context.Request.Query["lang"].ToString();
        if (!string.IsNullOrWhiteSpace(lang))
        {
            context.Response.Cookies.Append(
                ".AspNetCore.Culture",
                $"c={lang}|uic={lang}",
                new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1) }
            );
        }

        await _next(context);
    }
}
