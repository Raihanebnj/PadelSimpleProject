using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PadelSimple.Models.Data;
using PadelSimple.Models.Identity;
using PadelSimple.Web.ViewModels.Gebruikers;

namespace PadelSimple.Web.Controllers;

/// <summary>
/// Controller voor gebruikersbeheer uitsluitend voor Admin.
/// </summary>
[Authorize(Roles = "Admin")]
public class GebruikersController : Controller
{
    private readonly UserManager<AppGebruiker> _userManager;
    private readonly RoleManager<AppRol> _roleManager;
    private readonly AppDbContext _db;
    private readonly ILogger<GebruikersController> _logger;

    public GebruikersController(
        UserManager<AppGebruiker> userManager,
        RoleManager<AppRol> roleManager,
        AppDbContext db,
        ILogger<GebruikersController> logger)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _db = db;
        _logger = logger;
    }

    // ==================== OVERZICHT ====================

    public async Task<IActionResult> Overzicht(string? zoek = null, string? rolFilter = null)
    {
        var alleGebruikers = await _userManager.Users
            .Where(u => !u.IsVerwijderd)
            .OrderBy(u => u.Achternaam)
            .ThenBy(u => u.Voornaam)
            .ToListAsync();

        var vmLijst = new List<GebruikerRijVm>();

        foreach (var gebruiker in alleGebruikers)
        {
            var rollen = await _userManager.GetRolesAsync(gebruiker);

            // Filter op rolnaam indien opgegeven
            if (!string.IsNullOrWhiteSpace(rolFilter) && !rollen.Contains(rolFilter))
                continue;

            // Filter op naam of e-mail indien opgegeven
            if (!string.IsNullOrWhiteSpace(zoek))
            {
                var zoekLower = zoek.ToLower();
                if (!gebruiker.Email!.ToLower().Contains(zoekLower) &&
                    !gebruiker.Voornaam.ToLower().Contains(zoekLower) &&
                    !gebruiker.Achternaam.ToLower().Contains(zoekLower))
                    continue;
            }

            vmLijst.Add(new GebruikerRijVm
            {
                Id = gebruiker.Id,
                VolledigeNaam = gebruiker.VolledigeNaam,
                Email = gebruiker.Email ?? string.Empty,
                IsLid = gebruiker.IsLid,
                IsGeblokkeerd = gebruiker.IsGeblokkeerd,
                EmailBevestigd = gebruiker.EmailConfirmed,
                Rollen = rollen.ToList()
            });
        }

        var vm = new GebruikerIndexVm
        {
            Gebruikers = vmLijst,
            ZoekTerm = zoek,
            RolFilter = rolFilter,
            BeschikbareRollen = (await _roleManager.Roles.Select(r => r.Name!).ToListAsync())
        };

        return View(vm);
    }

    // ==================== DETAILS ====================

    public async Task<IActionResult> Details(string id)
    {
        var gebruiker = await _userManager.FindByIdAsync(id);
        if (gebruiker == null) return NotFound();

        var rollen = await _userManager.GetRolesAsync(gebruiker);
        var reservaties = await _db.Reservaties
            .Include(r => r.Terrein)
            .Where(r => r.GebruikerId == id)
            .OrderByDescending(r => r.Datum)
            .ToListAsync();

        var vm = new GebruikerDetailsVm
        {
            Id = gebruiker.Id,
            VolledigeNaam = gebruiker.VolledigeNaam,
            Email = gebruiker.Email ?? string.Empty,
            Telefoon = gebruiker.Telefoonnummer,
            IsLid = gebruiker.IsLid,
            IsGeblokkeerd = gebruiker.IsGeblokkeerd,
            EmailBevestigd = gebruiker.EmailConfirmed,
            Rollen = rollen.ToList(),
            Reservaties = reservaties.Select(r => new ReservatieRijVm
            {
                Id = r.Id,
                Datum = r.Datum,
                StartUur = r.StartUur,
                EindUur = r.EindUur,
                TerreinNaam = r.Terrein?.Naam ?? "–",
                TotalePrijs = r.TotalePrijs
            }).ToList(),
            BeschikbareRollen = (await _roleManager.Roles.Select(r => r.Name!).ToListAsync())
        };

        return View(vm);
    }

    // ==================== BLOKKEREN ====================

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Blokkeer(string id)
    {
        var gebruiker = await _userManager.FindByIdAsync(id);
        if (gebruiker == null) return NotFound();

        gebruiker.IsGeblokkeerd = true;
        await _userManager.UpdateAsync(gebruiker);

        _logger.LogInformation("Gebruiker {Email} geblokkeerd door {Door}.", gebruiker.Email, User.Identity?.Name);
        TempData["Success"] = $"{gebruiker.VolledigeNaam} is geblokkeerd.";
        return RedirectToAction(nameof(Overzicht));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Deblokkeer(string id)
    {
        var gebruiker = await _userManager.FindByIdAsync(id);
        if (gebruiker == null) return NotFound();

        gebruiker.IsGeblokkeerd = false;
        await _userManager.UpdateAsync(gebruiker);

        _logger.LogInformation("Gebruiker {Email} gedeblokkeerd door {Door}.", gebruiker.Email, User.Identity?.Name);
        TempData["Success"] = $"{gebruiker.VolledigeNaam} is gedeblokkeerd.";
        return RedirectToAction(nameof(Overzicht));
    }

    // ==================== ROLLEN BEHEER ====================

    [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = "Admin")]
    public async Task<IActionResult> VoegRolToe(string id, string rol)
    {
        var gebruiker = await _userManager.FindByIdAsync(id);
        if (gebruiker == null) return NotFound();

        if (!await _roleManager.RoleExistsAsync(rol))
        {
            TempData["Error"] = $"Rol '{rol}' bestaat niet.";
            return RedirectToAction(nameof(Details), new { id });
        }

        if (!await _userManager.IsInRoleAsync(gebruiker, rol))
        {
            await _userManager.AddToRoleAsync(gebruiker, rol);
            _logger.LogInformation("Rol '{Rol}' toegevoegd aan {Email}.", rol, gebruiker.Email);
            TempData["Success"] = $"Rol '{rol}' toegewezen aan {gebruiker.VolledigeNaam}.";
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = "Admin")]
    public async Task<IActionResult> VerwijderRol(string id, string rol)
    {
        var gebruiker = await _userManager.FindByIdAsync(id);
        if (gebruiker == null) return NotFound();

        if (await _userManager.IsInRoleAsync(gebruiker, rol))
        {
            await _userManager.RemoveFromRoleAsync(gebruiker, rol);
            _logger.LogInformation("Rol '{Rol}' verwijderd van {Email}.", rol, gebruiker.Email);
            TempData["Success"] = $"Rol '{rol}' verwijderd van {gebruiker.VolledigeNaam}.";
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    // ==================== E-MAIL VERIFICATIE ====================

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> BevestigEmail(string id)
    {
        var gebruiker = await _userManager.FindByIdAsync(id);
        if (gebruiker == null) return NotFound();

        gebruiker.EmailConfirmed = true;
        await _userManager.UpdateAsync(gebruiker);

        _logger.LogInformation("E-mail handmatig bevestigd voor {Email}.", gebruiker.Email);
        TempData["Success"] = $"E-mailadres van {gebruiker.VolledigeNaam} is bevestigd.";
        return RedirectToAction(nameof(Details), new { id });
    }
}
