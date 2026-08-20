using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PadelSimple.Models.Data;
using PadelSimple.Models.Domain;
using PadelSimple.Web.ViewModels.Materiaal;

namespace PadelSimple.Web.Controllers;

/// <summary>
/// Controller voor het beheren en bekijken van de materiaalinventaris.
/// </summary>
[Authorize]
public class MateriaalController : Controller
{
    private readonly AppDbContext _db;
    private readonly ILogger<MateriaalController> _logger;

    public MateriaalController(AppDbContext db, ILogger<MateriaalController> logger)
    {
        _db = db;
        _logger = logger;
    }

    // ==================== OVERZICHT ====================

    public async Task<IActionResult> Overzicht()
    {
        var items = await _db.Materialen
            .Where(m => !m.IsVerwijderd)
            .OrderBy(m => m.Naam)
            .Select(m => new MateriaalRijVm
            {
                Id = m.Id,
                Naam = m.Naam,
                AantalInInventaris = m.AantalInInventaris,
                BeschikbaarAantal = m.BeschikbaarAantal,
                Huurprijs = m.Huurprijs,
                IsActief = m.IsActief
            })
            .ToListAsync();

        var vm = new MateriaalOverzichtVm { Items = items };
        return View(vm);
    }

    // ==================== TOEVOEGEN (Admin / Medewerker) ====================

    [Authorize(Roles = "Admin")]
    public IActionResult Maak() => View(new MateriaalEditVm());

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Maak(MateriaalEditVm vm)
    {
        if (!ModelState.IsValid) return View(vm);

        var mat = new Materiaal
        {
            Naam = vm.Naam,
            AantalInInventaris = vm.Aantal,
            BeschikbaarAantal = vm.Aantal,
            Huurprijs = vm.Huurprijs,
            IsActief = vm.IsActief
        };

        _db.Materialen.Add(mat);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Materiaal '{Naam}' toegevoegd door {User}.", mat.Naam, User.Identity?.Name);
        TempData["Success"] = $"Materiaal '{mat.Naam}' is toegevoegd.";
        return RedirectToAction(nameof(Overzicht));
    }

    // ==================== BEWERKEN (Admin / Medewerker) ====================

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Bewerk(int id)
    {
        var mat = await _db.Materialen.FindAsync(id);
        if (mat == null || mat.IsVerwijderd) return NotFound();

        var vm = new MateriaalEditVm
        {
            Id = mat.Id,
            Naam = mat.Naam,
            Aantal = mat.AantalInInventaris,
            Huurprijs = mat.Huurprijs,
            IsActief = mat.IsActief
        };

        return View(vm);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Bewerk(MateriaalEditVm vm)
    {
        if (!ModelState.IsValid) return View(vm);

        var mat = await _db.Materialen.FindAsync(vm.Id);
        if (mat == null || mat.IsVerwijderd) return NotFound();

        var verschil = vm.Aantal - mat.AantalInInventaris;
        mat.Naam = vm.Naam;
        mat.AantalInInventaris = vm.Aantal;
        mat.BeschikbaarAantal = Math.Max(0, mat.BeschikbaarAantal + verschil);
        mat.Huurprijs = vm.Huurprijs;
        mat.IsActief = vm.IsActief;

        await _db.SaveChangesAsync();

        _logger.LogInformation("Materiaal #{Id} bijgewerkt door {User}.", mat.Id, User.Identity?.Name);
        TempData["Success"] = $"Materiaal '{mat.Naam}' bijgewerkt.";
        return RedirectToAction(nameof(Overzicht));
    }

    // ==================== VERWIJDEREN (Admin) ====================

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Verwijder(int id)
    {
        var mat = await _db.Materialen.FindAsync(id);
        if (mat == null || mat.IsVerwijderd) return NotFound();

        return View(mat);
    }

    [HttpPost, ActionName("Verwijder")]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> VerwijderBevestigd(int id)
    {
        var mat = await _db.Materialen.FindAsync(id);
        if (mat != null)
        {
            mat.IsVerwijderd = true;
            mat.VerwijderdOp = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            _logger.LogInformation("Materiaal #{Id} soft-deleted door {User}.", id, User.Identity?.Name);
            TempData["Success"] = "Materiaal verwijderd.";
        }
        return RedirectToAction(nameof(Overzicht));
    }

    // ==================== AJAX ENDPOINT ====================

    /// <summary>
    /// AJAX-call: past het aantal van een materiaal live aan (zonder herladen van de pagina).
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> WerkVoorraadBijAsync(int id, int nieuwAantal)
    {
        if (nieuwAantal < 0)
            return Json(new { success = false, message = "Aantal mag niet negatief zijn." });

        var mat = await _db.Materialen.FindAsync(id);
        if (mat == null || mat.IsVerwijderd)
            return Json(new { success = false, message = "Materiaal niet gevonden." });

        var verschil = nieuwAantal - mat.AantalInInventaris;
        mat.AantalInInventaris = nieuwAantal;
        mat.BeschikbaarAantal = Math.Max(0, mat.BeschikbaarAantal + verschil);

        await _db.SaveChangesAsync();

        _logger.LogInformation("Stock live bijgewerkt via AJAX voor materiaal #{Id}: nieuw aantal {Aantal}.", id, nieuwAantal);
        return Json(new { success = true, totaal = mat.AantalInInventaris, beschikbaar = mat.BeschikbaarAantal });
    }
}
