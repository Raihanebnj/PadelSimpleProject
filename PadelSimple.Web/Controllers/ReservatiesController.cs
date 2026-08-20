using System.Globalization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PadelSimple.Models.Data;
using PadelSimple.Models.Domain;
using PadelSimple.Models.Identity;
using PadelSimple.Web.ViewModels;

namespace PadelSimple.Web.Controllers;

/// <summary>
/// Controller voor het beheren van reservaties van padelterreinen.
/// </summary>
[Authorize]
public class ReservatiesController : Controller
{
    private readonly AppDbContext _db;
    private readonly UserManager<AppGebruiker> _userMgr;
    private readonly ILogger<ReservatiesController> _logger;

    public ReservatiesController(
        AppDbContext db,
        UserManager<AppGebruiker> userMgr,
        ILogger<ReservatiesController> logger)
    {
        _db = db;
        _userMgr = userMgr;
        _logger = logger;
    }

    // ==================== OVERZICHT MET FILTERING & SORTERING ====================

    public async Task<IActionResult> Overzicht(DateTime? datum = null, int? terreinId = null, string? sortering = null)
    {
        var gebruiker = await _userMgr.GetUserAsync(User);
        if (gebruiker == null) return Challenge();

        var query = _db.Reservaties
            .Include(r => r.Terrein)
            .Include(r => r.Materiaal)
            .Include(r => r.ReservatieMaterialen)
                .ThenInclude(rm => rm.Materiaal)
            .Include(r => r.Gebruiker)
            .AsQueryable();

        // Klant ziet enkel eigen reservaties; Admin en Medewerker zien alle reservaties
        bool isBeheerder = User.IsInRole("Admin") || User.IsInRole("Medewerker");
        if (!isBeheerder)
        {
            query = query.Where(r => r.GebruikerId == gebruiker.Id);
        }

        // Filtering
        if (datum.HasValue)
        {
            query = query.Where(r => r.Datum.Date == datum.Value.Date);
        }

        if (terreinId.HasValue && terreinId.Value > 0)
        {
            query = query.Where(r => r.TerreinId == terreinId.Value);
        }

        var lijst = await query.ToListAsync();

        // Sortering (client-side in-memory voor TimeSpan compatibiliteit)
        lijst = sortering switch
        {
            "datum_asc" => lijst.OrderBy(r => r.Datum).ThenBy(r => r.StartUur).ToList(),
            "prijs_desc" => lijst.OrderByDescending(r => r.TotalePrijs).ToList(),
            "prijs_asc" => lijst.OrderBy(r => r.TotalePrijs).ToList(),
            _ => lijst.OrderByDescending(r => r.Datum).ThenBy(r => r.StartUur).ToList() // Standaard nieuwste eerst
        };

        ViewBag.GeselecteerdeDatum = datum?.ToString("yyyy-MM-dd");
        ViewBag.GeselecteerdTerreinId = terreinId;
        ViewBag.HuidigeSortering = sortering;
        ViewBag.Terreinen = await _db.Terreinen
            .OrderBy(t => t.Naam)
            .Select(t => new SelectListItem { Value = t.Id.ToString(), Text = t.Naam })
            .ToListAsync();

        return View(lijst);
    }

    // ==================== AANMAKEN ====================

    public async Task<IActionResult> Maak()
    {
        var vm = new ReservatieEditVm();
        await VulDropDownsIn(vm);
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Maak(ReservatieEditVm vm)
    {
        var gebruiker = await _userMgr.GetUserAsync(User);
        if (gebruiker == null) return Challenge();

        if (!TryParseTijden(vm, out var start, out var eind))
            ModelState.AddModelError("", "Start/Eindtijd ongeldig (gebruik bijv. 10:00).");

        if (eind <= start)
            ModelState.AddModelError("", "Eindtijd moet na starttijd liggen.");

        if (!ModelState.IsValid)
        {
            await VulDropDownsIn(vm);
            return View(vm);
        }

        // Overlap-check op hetzelfde terrein & datum
        var zelfdeTerrein = await _db.Reservaties
            .Where(r => r.TerreinId == vm.TerreinId && r.Datum.Date == vm.Datum.Date && !r.IsVerwijderd)
            .ToListAsync();

        bool overlap = zelfdeTerrein.Any(r => r.StartUur < eind && start < r.EindUur);
        if (overlap)
        {
            _logger.LogWarning("Reservatie overlap geweigerd voor Terrein {TerreinId} op {Datum}.", vm.TerreinId, vm.Datum);
            ModelState.AddModelError("", "Er bestaat al een reservatie voor dit terrein op dit tijdslot.");
            await VulDropDownsIn(vm);
            return View(vm);
        }

        // Optionele materiaal voorraad check
        if (vm.MateriaalId.HasValue && vm.AantalMateriaal > 0)
        {
            var mat = await _db.Materialen.FirstOrDefaultAsync(m => m.Id == vm.MateriaalId.Value && !m.IsVerwijderd);
            if (mat == null || !mat.IsActief || mat.BeschikbaarAantal < vm.AantalMateriaal)
            {
                ModelState.AddModelError("", "Niet genoeg materiaal beschikbaar.");
                await VulDropDownsIn(vm);
                return View(vm);
            }
            mat.BeschikbaarAantal -= vm.AantalMateriaal;
        }

        // Prijsberekening
        var terrein = await _db.Terreinen.FindAsync(vm.TerreinId);
        var duurUur = (decimal)(eind - start).TotalHours;
        var totalePrijs = (terrein?.Uurtarief ?? 0m) * duurUur;

        if (vm.MateriaalId.HasValue && vm.AantalMateriaal > 0)
        {
            var mat = await _db.Materialen.FindAsync(vm.MateriaalId.Value);
            if (mat != null) totalePrijs += mat.Huurprijs * vm.AantalMateriaal;
        }

        var reservatie = new Reservatie
        {
            Datum = vm.Datum.Date,
            StartUur = start,
            EindUur = eind,
            TerreinId = vm.TerreinId,
            MateriaalId = (vm.MateriaalId.HasValue && vm.AantalMateriaal > 0) ? vm.MateriaalId : null,
            AantalMateriaal = (vm.MateriaalId.HasValue && vm.AantalMateriaal > 0) ? vm.AantalMateriaal : 0,
            AantalSpelers = vm.AantalSpelers,
            TotalePrijs = totalePrijs,
            GebruikerId = gebruiker.Id
        };

        _db.Reservaties.Add(reservatie);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Nieuwe reservatie #{Id} aangemaakt door {Email}.", reservatie.Id, gebruiker.Email);
        TempData["Success"] = "Reservatie succesvol aangemaakt!";
        return RedirectToAction(nameof(Overzicht));
    }

    // ==================== VERWIJDEREN ====================

    public async Task<IActionResult> Verwijder(int id)
    {
        var gebruiker = await _userMgr.GetUserAsync(User);
        if (gebruiker == null) return Challenge();

        var res = await _db.Reservaties
            .Include(r => r.Terrein)
            .Include(r => r.Materiaal)
            .Include(r => r.Gebruiker)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (res == null) return NotFound();

        bool isBeheerder = User.IsInRole("Admin") || User.IsInRole("Medewerker");
        if (!isBeheerder && res.GebruikerId != gebruiker.Id)
            return Forbid();

        return View(res);
    }

    [HttpPost, ActionName("Verwijder")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> VerwijderBevestigd(int id)
    {
        var gebruiker = await _userMgr.GetUserAsync(User);
        if (gebruiker == null) return Challenge();

        var res = await _db.Reservaties.FirstOrDefaultAsync(r => r.Id == id);
        if (res == null) return RedirectToAction(nameof(Overzicht));

        bool isBeheerder = User.IsInRole("Admin") || User.IsInRole("Medewerker");
        if (!isBeheerder && res.GebruikerId != gebruiker.Id)
            return Forbid();

        // Voorraad herstellen
        if (res.MateriaalId.HasValue && res.AantalMateriaal > 0)
        {
            var mat = await _db.Materialen.FirstOrDefaultAsync(m => m.Id == res.MateriaalId.Value);
            if (mat != null)
            {
                mat.BeschikbaarAantal += res.AantalMateriaal;
                if (mat.BeschikbaarAantal > mat.AantalInInventaris)
                    mat.BeschikbaarAantal = mat.AantalInInventaris;
            }
        }

        res.IsVerwijderd = true;
        res.VerwijderdOp = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        _logger.LogInformation("Reservatie #{Id} geannuleerd/verwijderd door {Email}.", id, gebruiker.Email);
        TempData["Success"] = "Reservatie geannuleerd.";
        return RedirectToAction(nameof(Overzicht));
    }

    // ==================== AJAX ENDPOINT ====================

    /// <summary>
    /// AJAX-call: haalt bezette tijdslots op voor een specifiek terrein en datum (zonder pagina reload).
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> HaalBezetteTijdslotsOp(int terreinId, string datum)
    {
        if (!DateTime.TryParse(datum, out var d))
            return Json(new { success = false, message = "Ongeldige datum." });

        var reservaties = await _db.Reservaties
            .Where(r => r.TerreinId == terreinId && r.Datum.Date == d.Date && !r.IsVerwijderd)
            .Select(r => new
            {
                start = r.StartUur.ToString(@"hh\:mm"),
                einde = r.EindUur.ToString(@"hh\:mm")
            })
            .ToListAsync();

        return Json(new { success = true, bezet = reservaties });
    }

    // ==================== HULPFUNCTIES ====================

    private async Task VulDropDownsIn(ReservatieEditVm vm)
    {
        var terreinen = await _db.Terreinen.OrderBy(t => t.Naam).ToListAsync();
        vm.Terreinen = terreinen
            .Select(t => new SelectListItem { Value = t.Id.ToString(), Text = $"{t.Naam} (€ {t.Uurtarief:F2}/u)" })
            .ToList();

        var materialen = await _db.Materialen
            .Where(m => m.IsActief && !m.IsVerwijderd)
            .OrderBy(m => m.Naam)
            .ToListAsync();

        vm.Materialen = new List<SelectListItem>
        {
            new SelectListItem { Value = "", Text = "(Geen materiaal)" }
        };

        vm.Materialen.AddRange(materialen.Select(m =>
            new SelectListItem
            {
                Value = m.Id.ToString(),
                Text = $"{m.Naam} (€ {m.Huurprijs:F2} - beschikbaar: {m.BeschikbaarAantal})"
            }));
    }

    private bool TryParseTijden(ReservatieEditVm vm, out TimeSpan start, out TimeSpan eind)
    {
        start = default;
        eind = default;

        bool okStart = TimeSpan.TryParseExact(vm.StartUur, new[] { @"h\:mm", @"hh\:mm" },
            CultureInfo.InvariantCulture, out start);

        bool okEind = TimeSpan.TryParseExact(vm.EindUur, new[] { @"h\:mm", @"hh\:mm" },
            CultureInfo.InvariantCulture, out eind);

        return okStart && okEind;
    }
}
