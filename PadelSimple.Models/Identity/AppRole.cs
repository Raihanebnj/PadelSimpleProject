using Microsoft.AspNetCore.Identity;

namespace PadelSimple.Models.Identity;

/// <summary>
/// Applicatierol, gebaseerd op ASP.NET Core Identity.
/// </summary>
public class AppRole : IdentityRole
{
    public AppRole() : base() { }
    public AppRole(string roleName) : base(roleName) { }
}
