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
public class AuthenticatieService
{
    private readonly UserManager<AppGebruiker> _userManager;
    private readonly RoleManager<AppRol> _roleManager;

    public AppGebruiker? HuidigeGebruiker { get; private set; }
    public IList<string> HuidigeRollen { get; private set; } = new List<string>();

    public bool IsAdmin => HuidigeRollen.Contains("Admin");
    public bool IsKlant => HuidigeRollen.Contains("Klant");
    // Legacy alias
    public bool IsPersoneel => false;
    public bool IsLid => HuidigeGebruiker?.IsLid ?? false;

    public AuthenticatieService(UserManager<AppGebruiker> userManager, RoleManager<AppRol> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    /// <summary>Meldt een gebruiker aan op basis van e-mail of gebruikersnaam.</summary>
    public async Task<(bool Geslaagd, string Fout)> MeldAanAsync(string gebruikersnaamOfEmail, string wachtwoord)
    {
        try
        {
            // Zoek op gebruikersnaam, daarna op e-mail
            var gebruiker = await _userManager.FindByNameAsync(gebruikersnaamOfEmail)
                    ?? await _userManager.FindByEmailAsync(gebruikersnaamOfEmail);

            if (gebruiker == null)
                return (false, "Gebruiker niet gevonden.");

            if (gebruiker.IsGeblokkeerd)
                return (false, "Dit account is geblokkeerd. Neem contact op met de beheerder.");

            if (gebruiker.IsVerwijderd)
                return (false, "Dit account bestaat niet meer.");

            var ok = await _userManager.CheckPasswordAsync(gebruiker, wachtwoord);
            if (!ok)
                return (false, "Ongeldig wachtwoord.");

            HuidigeGebruiker = gebruiker;
            HuidigeRollen = await _userManager.GetRolesAsync(gebruiker);
            return (true, string.Empty);
        }
        catch (Exception ex)
        {
            return (false, $"Aanmeldingsfout: {ex.Message}");
        }
    }

    /// <summary>Meldt de huidige gebruiker af.</summary>
    public void MeldAf()
    {
        HuidigeGebruiker = null;
        HuidigeRollen = new List<string>();
    }

    /// <summary>
    /// Registreert een nieuwe gebruiker met optioneel lidmaatschap (Klant-rol).
    /// </summary>
    public async Task<(bool Geslaagd, string Fout)> RegistreerAsync(
        string email,
        string wachtwoord,
        string voornaam,
        string achternaam,
        string telefoon,
        bool isLid)
    {
        try
        {
            var bestaand = await _userManager.FindByEmailAsync(email);
            if (bestaand != null)
                return (false, "Er bestaat al een account met dit e-mailadres.");

            var gebruiker = new AppGebruiker
            {
                UserName = email,
                Email = email,
                Voornaam = voornaam,
                Achternaam = achternaam,
                Telefoonnummer = telefoon,
                IsLid = isLid,
                IsGeblokkeerd = false,
                IsVerwijderd = false
            };

            var resultaat = await _userManager.CreateAsync(gebruiker, wachtwoord);
            if (!resultaat.Succeeded)
            {
                var msg = string.Join(Environment.NewLine, resultaat.Errors.Select(e => e.Description));
                return (false, msg);
            }

            // Rol toewijzen: altijd Klant
            await _userManager.AddToRoleAsync(gebruiker, "Klant");
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

    public async Task<List<AppGebruiker>> HaalAlleGebruikersOpAsync()
    {
        return await _userManager.Users
            .OrderBy(u => u.Email)
            .ToListAsync();
    }

    public async Task<IList<string>> HaalGebruikersRollenOpAsync(AppGebruiker gebruiker)
        => await _userManager.GetRolesAsync(gebruiker);

    public async Task VoegRolToeAsync(AppGebruiker gebruiker, string rolNaam)
    {
        if (!await _roleManager.RoleExistsAsync(rolNaam))
            throw new InvalidOperationException($"Rol '{rolNaam}' bestaat niet.");

        if (!await _userManager.IsInRoleAsync(gebruiker, rolNaam))
            await _userManager.AddToRoleAsync(gebruiker, rolNaam);
    }

    public async Task VerwijderRolAsync(AppGebruiker gebruiker, string rolNaam)
    {
        if (await _userManager.IsInRoleAsync(gebruiker, rolNaam))
            await _userManager.RemoveFromRoleAsync(gebruiker, rolNaam);
    }

    public async Task StelGeblokkeerdAsync(AppGebruiker gebruiker, bool geblokkeerd)
    {
        gebruiker.IsGeblokkeerd = geblokkeerd;
        await _userManager.UpdateAsync(gebruiker);
    }

    public async Task VerwijderGebruikerAsync(AppGebruiker gebruiker)
    {
        gebruiker.IsVerwijderd = true;
        gebruiker.VerwijderdOp = DateTime.UtcNow;
        await _userManager.UpdateAsync(gebruiker);
    }
}
