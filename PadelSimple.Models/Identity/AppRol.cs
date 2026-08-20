using Microsoft.AspNetCore.Identity;

namespace PadelSimple.Models.Identity;

/// <summary>
/// Applicatierol, gebaseerd op ASP.NET Core Identity.
/// </summary>
public class AppRol : IdentityRole
{
    public AppRol() : base() { }
    public AppRol(string rolNaam) : base(rolNaam) { }
}
