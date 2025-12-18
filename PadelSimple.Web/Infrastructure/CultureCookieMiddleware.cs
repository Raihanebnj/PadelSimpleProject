using Microsoft.AspNetCore.Localization;

namespace PadelSimple.Web.Infrastructure;

public class CultureCookieMiddleware
{
    private readonly RequestDelegate _next;

    public CultureCookieMiddleware(RequestDelegate next) => _next = next;

    public async Task Invoke(HttpContext ctx)
    {
        // Als ?lang=nl/en/fr -> zet cookie
        var lang = ctx.Request.Query["lang"].ToString();
        if (!string.IsNullOrWhiteSpace(lang))
        {
            var culture = new RequestCulture(lang);
            ctx.Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(culture),
                new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1) }
            );
        }

        await _next(ctx);
    }
}
