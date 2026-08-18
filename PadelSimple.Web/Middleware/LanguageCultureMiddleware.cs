namespace PadelSimple.Web.Middleware;

/// <summary>
/// Aangepaste middleware die de gewenste taal/cultuur instelt op basis van een cookie.
/// De taal wordt opgeslagen als cookie 'lang' (bv. "nl", "en", "fr").
/// </summary>
public class LanguageCultureMiddleware
{
    private readonly RequestDelegate _volgende;
    private readonly string[] _ondersteundeCulturen;
    private readonly string _standaardCultuur;

    public LanguageCultureMiddleware(RequestDelegate volgende, IConfiguration configuratie)
    {
        _volgende = volgende;
        _ondersteundeCulturen = configuratie.GetSection("SupportedCultures").Get<string[]>()
            ?? new[] { "nl", "en", "fr" };
        _standaardCultuur = configuratie["DefaultCulture"] ?? "nl";
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // 1. Lees de taalcookie
        var taal = context.Request.Cookies["lang"];

        // 2. Valideer — als ongeldig, gebruik de standaardtaal
        if (string.IsNullOrWhiteSpace(taal) || !_ondersteundeCulturen.Contains(taal))
        {
            taal = _standaardCultuur;
        }

        // 3. Stel de thread-cultuur in voor de huidige request
        var cultuurInfo = new System.Globalization.CultureInfo(taal);
        System.Threading.Thread.CurrentThread.CurrentCulture = cultuurInfo;
        System.Threading.Thread.CurrentThread.CurrentUICulture = cultuurInfo;

        // 4. Roep de volgende middleware aan
        await _volgende(context);
    }
}
