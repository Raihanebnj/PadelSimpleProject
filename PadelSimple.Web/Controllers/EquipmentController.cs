using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PadelSimple.Models.Data;
using PadelSimple.Models.Domain;
using PadelSimple.Web.ViewModels.Equipment;

namespace PadelSimple.Web.Controllers;

/// <summary>
/// Controller voor het beheren en bekijken van de materiaalinventaris.
/// </summary>
[Authorize]
public class EquipmentController : Controller
{
    private readonly AppDbContext _db;
    private readonly ILogger<EquipmentController> _logger;

    public EquipmentController(AppDbContext db, ILogger<EquipmentController> logger)
    {
        _db = db;
        _logger = logger;
    }

    // ==================== OVERZICHT ====================

    public async Task<IActionResult> Index()
    {
        var items = await _db.Materialen
            .Where(m => !m.IsDeleted)
            .OrderBy(m => m.Naam)
            .Select(m => new EquipmentRowVm
            {
                Id = m.Id,
                Name = m.Naam,
                TotalQuantity = m.AantalInInventaris,
                AvailableQuantity = m.AvailableQuantity,
                Huurprijs = m.Huurprijs,
                IsActive = m.IsActief
            })
            .ToListAsync();

        var vm = new EquipmentIndexVm { Items = items };
        return View(vm);
    }

    // ==================== TOEVOEGEN (Admin / Medewerker) ====================

    [Authorize(Roles = "Admin,Medewerker")]
    public IActionResult Create() => View(new MateriaalEditVm());

    [HttpPost]
    [Authorize(Roles = "Admin,Medewerker")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(MateriaalEditVm vm)
    {
        if (!ModelState.IsValid) return View(vm);

        var mat = new Materiaal
        {
            Naam = vm.Naam,
            AantalInInventaris = vm.Aantal,
            AvailableQuantity = vm.Aantal,
            Huurprijs = vm.Huurprijs,
            IsActief = vm.IsActief
        };

        _db.Materialen.Add(mat);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Materiaal '{Naam}' toegevoegd door {User}.", mat.Naam, User.Identity?.Name);
        TempData["Success"] = $"Materiaal '{mat.Naam}' is toegevoegd.";
        return RedirectToAction(nameof(Index));
    }

    // ==================== BEWERKEN (Admin / Medewerker) ====================

    [Authorize(Roles = "Admin,Medewerker")]
    public async Task<IActionResult> Edit(int id)
    {
        var mat = await _db.Materialen.FindAsync(id);
        if (mat == null || mat.IsDeleted) return NotFound();

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
    [Authorize(Roles = "Admin,Medewerker")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(MateriaalEditVm vm)
    {
        if (!ModelState.IsValid) return View(vm);

        var mat = await _db.Materialen.FindAsync(vm.Id);
        if (mat == null || mat.IsDeleted) return NotFound();

        var diff = vm.Aantal - mat.AantalInInventaris;
        mat.Naam = vm.Naam;
        mat.AantalInInventaris = vm.Aantal;
        mat.AvailableQuantity = Math.Max(0, mat.AvailableQuantity + diff);
        mat.Huurprijs = vm.Huurprijs;
        mat.IsActief = vm.IsActief;

        await _db.SaveChangesAsync();

        _logger.LogInformation("Materiaal #{Id} bijgewerkt door {User}.", mat.Id, User.Identity?.Name);
        TempData["Success"] = $"Materiaal '{mat.Naam}' bijgewerkt.";
        return RedirectToAction(nameof(Index));
    }

    // ==================== VERWIJDEREN (Admin) ====================

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var mat = await _db.Materialen.FindAsync(id);
        if (mat == null || mat.IsDeleted) return NotFound();

        return View(mat);
    }

    [HttpPost, ActionName("Delete")]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var mat = await _db.Materialen.FindAsync(id);
        if (mat != null)
        {
            mat.IsDeleted = true;
            mat.DeletedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            _logger.LogInformation("Materiaal #{Id} soft-deleted door {User}.", id, User.Identity?.Name);
            TempData["Success"] = "Materiaal verwijderd.";
        }
        return RedirectToAction(nameof(Index));
    }

    // ==================== AJAX ENDPOINT ====================

    /// <summary>
    /// AJAX-call: past het aantal van een materiaal live aan (zonder herladen van de pagina).
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin,Medewerker")]
    public async Task<IActionResult> UpdateStockAsync(int id, int nieuwAantal)
    {
        if (nieuwAantal < 0)
            return Json(new { success = false, message = "Aantal mag niet negatief zijn." });

        var mat = await _db.Materialen.FindAsync(id);
        if (mat == null || mat.IsDeleted)
            return Json(new { success = false, message = "Materiaal niet gevonden." });

        var verschil = nieuwAantal - mat.AantalInInventaris;
        mat.AantalInInventaris = nieuwAantal;
        mat.AvailableQuantity = Math.Max(0, mat.AvailableQuantity + verschil);

        await _db.SaveChangesAsync();

        _logger.LogInformation("Stock live bijgewerkt via AJAX voor materiaal #{Id}: nieuw aantal {Aantal}.", id, nieuwAantal);
        return Json(new { success = true, totaal = mat.AantalInInventaris, beschikbaar = mat.AvailableQuantity });
    }
}
