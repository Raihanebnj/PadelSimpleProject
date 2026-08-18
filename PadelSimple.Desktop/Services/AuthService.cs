using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PadelSimple.Models.Identity;

namespace PadelSimple.Desktop.Services;

/// <summary>
/// Service voor authenticatie, registratie en gebruikersbeheer.
/// </summary>
public class AuthService
{
    private readonly UserManager<AppUser> _userManager;
    private readonly RoleManager<AppRole> _roleManager;

    public AppUser? CurrentUser { get; private set; }
    public IList<string> CurrentRoles { get; private set; } = new List<string>();

    public bool IsAdmin => CurrentRoles.Contains("Admin");
    public bool IsKlant => CurrentRoles.Contains("Klant");
    // Legacy alias
    public bool IsStaff => false;
    public bool IsMember => CurrentUser?.IsLid ?? false;

    public AuthService(UserManager<AppUser> userManager, RoleManager<AppRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    /// <summary>Meldt een gebruiker aan op basis van e-mail of gebruikersnaam.</summary>
    public async Task<(bool Succeeded, string Error)> LoginAsync(string userNameOrEmail, string password)
    {
        try
        {
            // Zoek op gebruikersnaam, daarna op e-mail
            var user = await _userManager.FindByNameAsync(userNameOrEmail)
                    ?? await _userManager.FindByEmailAsync(userNameOrEmail);

            if (user == null)
                return (false, "Gebruiker niet gevonden.");

            if (user.IsBlocked)
                return (false, "Dit account is geblokkeerd. Neem contact op met de beheerder.");

            if (user.IsDeleted)
                return (false, "Dit account bestaat niet meer.");

            var ok = await _userManager.CheckPasswordAsync(user, password);
            if (!ok)
                return (false, "Ongeldig wachtwoord.");

            CurrentUser = user;
            CurrentRoles = await _userManager.GetRolesAsync(user);
            return (true, string.Empty);
        }
        catch (Exception ex)
        {
            return (false, $"Aanmeldfout: {ex.Message}");
        }
    }

    /// <summary>Meldt de huidige gebruiker af.</summary>
    public void Logout()
    {
        CurrentUser = null;
        CurrentRoles = new List<string>();
    }

    /// <summary>
    /// Registreert een nieuwe gebruiker met optioneel lidmaatschap (Klant-rol).
    /// </summary>
    public async Task<(bool Succeeded, string Error)> RegisterAsync(
        string email,
        string password,
        string voornaam,
        string achternaam,
        string telefoon,
        bool isLid)
    {
        try
        {
            var existing = await _userManager.FindByEmailAsync(email);
            if (existing != null)
                return (false, "Er bestaat al een account met dit e-mailadres.");

            var user = new AppUser
            {
                UserName = email,
                Email = email,
                Voornaam = voornaam,
                Achternaam = achternaam,
                Telefoonnummer = telefoon,
                IsLid = isLid,
                IsBlocked = false,
                IsDeleted = false
            };

            var result = await _userManager.CreateAsync(user, password);
            if (!result.Succeeded)
            {
                var msg = string.Join(Environment.NewLine, result.Errors.Select(e => e.Description));
                return (false, msg);
            }

            // Rol toewijzen: altijd Klant
            await _userManager.AddToRoleAsync(user, "Klant");
            return (true, string.Empty);
        }
        catch (Exception ex)
        {
            return (false, $"Registratiefout: {ex.Message}");
        }
    }

    // ================================================================
    //  GEBRUIKERSBEHEER (Admin)
    // ================================================================

    public async Task<List<AppUser>> GetAllUsersAsync()
    {
        return await _userManager.Users
            .OrderBy(u => u.Email)
            .ToListAsync();
    }

    public async Task<IList<string>> GetUserRolesAsync(AppUser user)
        => await _userManager.GetRolesAsync(user);

    public async Task AddRoleAsync(AppUser user, string roleName)
    {
        if (!await _roleManager.RoleExistsAsync(roleName))
            throw new InvalidOperationException($"Rol '{roleName}' bestaat niet.");

        if (!await _userManager.IsInRoleAsync(user, roleName))
            await _userManager.AddToRoleAsync(user, roleName);
    }

    public async Task RemoveRoleAsync(AppUser user, string roleName)
    {
        if (await _userManager.IsInRoleAsync(user, roleName))
            await _userManager.RemoveFromRoleAsync(user, roleName);
    }

    public async Task SetBlockedAsync(AppUser user, bool geblokkeerd)
    {
        user.IsBlocked = geblokkeerd;
        await _userManager.UpdateAsync(user);
    }

    public async Task VerwijderGebruikerAsync(AppUser user)
    {
        user.IsDeleted = true;
        user.DeletedAt = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);
    }
}
