using System.Globalization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PadelSimple.Models.Data;
using PadelSimple.Models.Domain;
using PadelSimple.Web.ViewModels.Terreinen;

namespace PadelSimple.Web.Controllers;

/// <summary>
/// Controller voor het bekijken en beheren van padelterreinen.
/// </summary>
[Authorize]
public class TerreinenController : Controller
{
    private readonly AppDbContext _db;
    private readonly ILogger<TerreinenController> _logger;

    public TerreinenController(AppDbContext db, ILogger<TerreinenController> logger)
    {
        _db = db;
        _logger = logger;
    }

    // ==================== OVERZICHT & BESCHIKBAARHEID ====================

    public async Task<IActionResult> Overzicht(DateTime? datum, string? start, string? einde)
    {
        var vm = new TerreinenOverzichtVm
        {
            Datum = datum?.Date ?? DateTime.Today,
            Start = start,
            Einde = einde
        };

        var startOk = TryParseTime(start, out var startTs);
        var eindOk = TryParseTime(einde, out var eindTs);
        var heeftSlot = startOk && eindOk && startTs < eindTs;

        var terreinen = await _db.Terreinen
            .Where(t => !t.IsVerwijderd)
            .OrderBy(t => t.Naam)
            .ToListAsync();

        var reservaties = await _db.Reservaties
            .Where(r => r.Datum.Date == vm.Datum.Date && !r.IsVerwijderd)
            .Select(r => new { r.TerreinId, r.StartUur, r.EindUur })
            .ToListAsync();

        vm.Terreinen = terreinen.Select(c =>
        {
            var terreinRes = reservaties
                .Where(r => r.TerreinId == c.Id)
                .OrderBy(r => r.StartUur)
                .ToList();

            bool beschikbaar;
            string? vrijVanaf = null;

            if (heeftSlot)
            {
                var overlap = terreinRes
                    .Where(r => r.StartUur < eindTs && startTs < r.EindUur)
                    .ToList();

                beschikbaar = overlap.Count == 0;

                if (!beschikbaar)
                {
                    var laatsteEinde = overlap.Max(r => r.EindUur);
                    vrijVanaf = laatsteEinde.ToString(@"hh\:mm");
                }
            }
            else
            {
                if (!terreinRes.Any())
                {
                    beschikbaar = true;
                }
                else
                {
                    beschikbaar = false;
                    vrijVanaf = terreinRes.Last().EindUur.ToString(@"hh\:mm");
                }
            }

            return new TerreinRijVm
            {
                Id = c.Id,
                Naam = c.Naam,
                Capaciteit = c.Capaciteit,
                IsIndoors = c.IsIndoors,
                Uurtarief = c.Uurtarief,
                IsBeschikbaar = beschikbaar,
                VrijVanaf = vrijVanaf
            };
        }).ToList();

        return View(vm);
    }

    // ==================== TOEVOEGEN (Admin / Medewerker) ====================

    [Authorize(Roles = "Admin")]
    public IActionResult Maak() => View(new TerreinEditVm());

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Maak(TerreinEditVm vm)
    {
        if (!ModelState.IsValid) return View(vm);

        var terrein = new Terrein
        {
            Naam = vm.Naam,
            Capaciteit = vm.Capaciteit,
            IsIndoors = vm.IsIndoors,
            Uurtarief = vm.Uurtarief
        };

        _db.Terreinen.Add(terrein);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Terrein '{Naam}' aangemaakt door {User}.", terrein.Naam, User.Identity?.Name);
        TempData["Success"] = $"Terrein '{terrein.Naam}' aangemaakt.";
        return RedirectToAction(nameof(Overzicht));
    }

    // ==================== BEWERKEN (Admin / Medewerker) ====================

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Bewerk(int id)
    {
        var terrein = await _db.Terreinen.FindAsync(id);
        if (terrein == null || terrein.IsVerwijderd) return NotFound();

        var vm = new TerreinEditVm
        {
            Id = terrein.Id,
            Naam = terrein.Naam,
            Capaciteit = terrein.Capaciteit,
            IsIndoors = terrein.IsIndoors,
            Uurtarief = terrein.Uurtarief
        };

        return View(vm);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Bewerk(TerreinEditVm vm)
    {
        if (!ModelState.IsValid) return View(vm);

        var terrein = await _db.Terreinen.FindAsync(vm.Id);
        if (terrein == null || terrein.IsVerwijderd) return NotFound();

        terrein.Naam = vm.Naam;
        terrein.Capaciteit = vm.Capaciteit;
        terrein.IsIndoors = vm.IsIndoors;
        terrein.Uurtarief = vm.Uurtarief;

        await _db.SaveChangesAsync();

        _logger.LogInformation("Terrein #{Id} bijgewerkt door {User}.", terrein.Id, User.Identity?.Name);
        TempData["Success"] = $"Terrein '{terrein.Naam}' bijgewerkt.";
        return RedirectToAction(nameof(Overzicht));
    }

    // ==================== VERWIJDEREN (Admin) ====================

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Verwijder(int id)
    {
        var terrein = await _db.Terreinen.FindAsync(id);
        if (terrein == null || terrein.IsVerwijderd) return NotFound();

        return View(terrein);
    }

    [HttpPost, ActionName("Verwijder")]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> VerwijderBevestigd(int id)
    {
        var terrein = await _db.Terreinen.FindAsync(id);
        if (terrein != null)
        {
            terrein.IsVerwijderd = true;
            terrein.VerwijderdOp = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            _logger.LogInformation("Terrein #{Id} soft-deleted door {User}.", id, User.Identity?.Name);
            TempData["Success"] = "Terrein verwijderd.";
        }
        return RedirectToAction(nameof(Overzicht));
    }

    private static bool TryParseTime(string? value, out TimeSpan time)
    {
        time = default;
        if (string.IsNullOrWhiteSpace(value)) return false;

        return TimeSpan.TryParseExact(
                   value.Trim(),
                   new[] { @"h\:mm", @"hh\:mm" },
                   CultureInfo.InvariantCulture,
                   out time)
               || TimeSpan.TryParse(value.Trim(), CultureInfo.InvariantCulture, out time);
    }
}
