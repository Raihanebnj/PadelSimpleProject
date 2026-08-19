using System.Globalization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PadelSimple.Models.Data;
using PadelSimple.Models.Domain;
using PadelSimple.Web.ViewModels.Courts;

namespace PadelSimple.Web.Controllers;

/// <summary>
/// Controller voor het bekijken en beheren van padelterreinen.
/// </summary>
[Authorize]
public class CourtsController : Controller
{
    private readonly AppDbContext _db;
    private readonly ILogger<CourtsController> _logger;

    public CourtsController(AppDbContext db, ILogger<CourtsController> logger)
    {
        _db = db;
        _logger = logger;
    }

    // ==================== OVERZICHT & BESCHIKBAARHEID ====================

    public async Task<IActionResult> Index(DateTime? date, string? start, string? end)
    {
        var vm = new CourtsIndexVm
        {
            Date = date?.Date ?? DateTime.Today,
            Start = start,
            End = end
        };

        var startOk = TryParseTime(start, out var startTs);
        var endOk = TryParseTime(end, out var endTs);
        var hasSlot = startOk && endOk && startTs < endTs;

        var courts = await _db.Terreinen
            .Where(t => !t.IsDeleted)
            .OrderBy(t => t.Naam)
            .ToListAsync();

        var reservations = await _db.Reservaties
            .Where(r => r.Datum.Date == vm.Date.Date && !r.IsDeleted)
            .Select(r => new { r.TerreinId, r.StartUur, r.EindUur })
            .ToListAsync();

        vm.Courts = courts.Select(c =>
        {
            var courtRes = reservations
                .Where(r => r.TerreinId == c.Id)
                .OrderBy(r => r.StartUur)
                .ToList();

            bool available;
            string? freeFrom = null;

            if (hasSlot)
            {
                var overlaps = courtRes
                    .Where(r => r.StartUur < endTs && startTs < r.EindUur)
                    .ToList();

                available = overlaps.Count == 0;

                if (!available)
                {
                    var lastEnd = overlaps.Max(r => r.EindUur);
                    freeFrom = lastEnd.ToString(@"hh\:mm");
                }
            }
            else
            {
                if (!courtRes.Any())
                {
                    available = true;
                }
                else
                {
                    available = false;
                    freeFrom = courtRes.Last().EindUur.ToString(@"hh\:mm");
                }
            }

            return new CourtRowVm
            {
                Id = c.Id,
                Name = c.Naam,
                Capacity = c.Capaciteit,
                IsIndoor = c.IsIndoors,
                Uurtarief = c.Uurtarief,
                IsAvailable = available,
                FreeFrom = freeFrom
            };
        }).ToList();

        return View(vm);
    }

    // ==================== TOEVOEGEN (Admin / Medewerker) ====================

    [Authorize(Roles = "Admin")]
    public IActionResult Create() => View(new TerreinEditVm());

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(TerreinEditVm vm)
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
        return RedirectToAction(nameof(Index));
    }

    // ==================== BEWERKEN (Admin / Medewerker) ====================

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(int id)
    {
        var terrein = await _db.Terreinen.FindAsync(id);
        if (terrein == null || terrein.IsDeleted) return NotFound();

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
    public async Task<IActionResult> Edit(TerreinEditVm vm)
    {
        if (!ModelState.IsValid) return View(vm);

        var terrein = await _db.Terreinen.FindAsync(vm.Id);
        if (terrein == null || terrein.IsDeleted) return NotFound();

        terrein.Naam = vm.Naam;
        terrein.Capaciteit = vm.Capaciteit;
        terrein.IsIndoors = vm.IsIndoors;
        terrein.Uurtarief = vm.Uurtarief;

        await _db.SaveChangesAsync();

        _logger.LogInformation("Terrein #{Id} bijgewerkt door {User}.", terrein.Id, User.Identity?.Name);
        TempData["Success"] = $"Terrein '{terrein.Naam}' bijgewerkt.";
        return RedirectToAction(nameof(Index));
    }

    // ==================== VERWIJDEREN (Admin) ====================

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var terrein = await _db.Terreinen.FindAsync(id);
        if (terrein == null || terrein.IsDeleted) return NotFound();

        return View(terrein);
    }

    [HttpPost, ActionName("Delete")]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var terrein = await _db.Terreinen.FindAsync(id);
        if (terrein != null)
        {
            terrein.IsDeleted = true;
            terrein.DeletedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            _logger.LogInformation("Terrein #{Id} soft-deleted door {User}.", id, User.Identity?.Name);
            TempData["Success"] = "Terrein verwijderd.";
        }
        return RedirectToAction(nameof(Index));
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
