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
public class ReservationsController : Controller
{
    private readonly AppDbContext _db;
    private readonly UserManager<AppUser> _userMgr;
    private readonly ILogger<ReservationsController> _logger;

    public ReservationsController(
        AppDbContext db,
        UserManager<AppUser> userMgr,
        ILogger<ReservationsController> logger)
    {
        _db = db;
        _userMgr = userMgr;
        _logger = logger;
    }

    // ==================== OVERZICHT MET FILTERING & SORTERING ====================

    public async Task<IActionResult> Index(DateTime? datum = null, int? terreinId = null, string? sortering = null)
    {
        var gebruiker = await _userMgr.GetUserAsync(User);
        if (gebruiker == null) return Challenge();

        var query = _db.Reservaties
            .Include(r => r.Terrein)
            .Include(r => r.Materiaal)
            .Include(r => r.ReservationMaterialen)
                .ThenInclude(rm => rm.Materiaal)
            .Include(r => r.User)
            .AsQueryable();

        // Klant ziet enkel eigen reservaties; Admin en Medewerker zien alle reservaties
        bool isBeheerder = User.IsInRole("Admin") || User.IsInRole("Medewerker");
        if (!isBeheerder)
        {
            query = query.Where(r => r.UserId == gebruiker.Id);
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

    public async Task<IActionResult> Create()
    {
        var vm = new ReservationEditVm();
        await VulDropDownsIn(vm);
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ReservationEditVm vm)
    {
        var gebruiker = await _userMgr.GetUserAsync(User);
        if (gebruiker == null) return Challenge();

        if (!TryParseTijden(vm, out var start, out var end))
            ModelState.AddModelError("", "Start/Eindtijd ongeldig (gebruik bijv. 10:00).");

        if (end <= start)
            ModelState.AddModelError("", "Eindtijd moet na starttijd liggen.");

        if (!ModelState.IsValid)
        {
            await VulDropDownsIn(vm);
            return View(vm);
        }

        // Overlap-check op hetzelfde terrein & datum
        var zelfdeTerrein = await _db.Reservaties
            .Where(r => r.TerreinId == vm.CourtId && r.Datum.Date == vm.Date.Date && !r.IsDeleted)
            .ToListAsync();

        bool overlap = zelfdeTerrein.Any(r => r.StartUur < end && start < r.EindUur);
        if (overlap)
        {
            _logger.LogWarning("Reservatie overlap geweigerd voor Terrein {TerreinId} op {Datum}.", vm.CourtId, vm.Date);
            ModelState.AddModelError("", "Er bestaat al een reservatie voor dit terrein op dit tijdslot.");
            await VulDropDownsIn(vm);
            return View(vm);
        }

        // Optionele materiaal voorraad check
        if (vm.EquipmentId.HasValue && vm.EquipmentQuantity > 0)
        {
            var mat = await _db.Materialen.FirstOrDefaultAsync(m => m.Id == vm.EquipmentId.Value && !m.IsDeleted);
            if (mat == null || !mat.IsActief || mat.AvailableQuantity < vm.EquipmentQuantity)
            {
                ModelState.AddModelError("", "Niet genoeg materiaal beschikbaar.");
                await VulDropDownsIn(vm);
                return View(vm);
            }
            mat.AvailableQuantity -= vm.EquipmentQuantity;
        }

        // Prijsberekening
        var terrein = await _db.Terreinen.FindAsync(vm.CourtId);
        var duurUur = (decimal)(end - start).TotalHours;
        var totalePrijs = (terrein?.Uurtarief ?? 0m) * duurUur;

        if (vm.EquipmentId.HasValue && vm.EquipmentQuantity > 0)
        {
            var mat = await _db.Materialen.FindAsync(vm.EquipmentId.Value);
            if (mat != null) totalePrijs += mat.Huurprijs * vm.EquipmentQuantity;
        }

        var reservatie = new Reservation
        {
            Datum = vm.Date.Date,
            StartUur = start,
            EindUur = end,
            TerreinId = vm.CourtId,
            MateriaalId = (vm.EquipmentId.HasValue && vm.EquipmentQuantity > 0) ? vm.EquipmentId : null,
            AantalMateriaal = (vm.EquipmentId.HasValue && vm.EquipmentQuantity > 0) ? vm.EquipmentQuantity : 0,
            AantalSpelers = vm.NumberOfPlayers,
            TotalePrijs = totalePrijs,
            UserId = gebruiker.Id
        };

        _db.Reservaties.Add(reservatie);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Nieuwe reservatie #{Id} aangemaakt door {Email}.", reservatie.Id, gebruiker.Email);
        TempData["Success"] = "Reservatie succesvol aangemaakt!";
        return RedirectToAction(nameof(Index));
    }

    // ==================== VERWIJDEREN ====================

    public async Task<IActionResult> Delete(int id)
    {
        var gebruiker = await _userMgr.GetUserAsync(User);
        if (gebruiker == null) return Challenge();

        var res = await _db.Reservaties
            .Include(r => r.Terrein)
            .Include(r => r.Materiaal)
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (res == null) return NotFound();

        bool isBeheerder = User.IsInRole("Admin") || User.IsInRole("Medewerker");
        if (!isBeheerder && res.UserId != gebruiker.Id)
            return Forbid();

        return View(res);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var gebruiker = await _userMgr.GetUserAsync(User);
        if (gebruiker == null) return Challenge();

        var res = await _db.Reservaties.FirstOrDefaultAsync(r => r.Id == id);
        if (res == null) return RedirectToAction(nameof(Index));

        bool isBeheerder = User.IsInRole("Admin") || User.IsInRole("Medewerker");
        if (!isBeheerder && res.UserId != gebruiker.Id)
            return Forbid();

        // Voorraad herstellen
        if (res.MateriaalId.HasValue && res.AantalMateriaal > 0)
        {
            var mat = await _db.Materialen.FirstOrDefaultAsync(m => m.Id == res.MateriaalId.Value);
            if (mat != null)
            {
                mat.AvailableQuantity += res.AantalMateriaal;
                if (mat.AvailableQuantity > mat.AantalInInventaris)
                    mat.AvailableQuantity = mat.AantalInInventaris;
            }
        }

        res.IsDeleted = true;
        res.DeletedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        _logger.LogInformation("Reservatie #{Id} geannuleerd/verwijderd door {Email}.", id, gebruiker.Email);
        TempData["Success"] = "Reservatie geannuleerd.";
        return RedirectToAction(nameof(Index));
    }

    // ==================== AJAX ENDPOINT ====================

    /// <summary>
    /// AJAX-call: haalt bezette tijdslots op voor een specifiek terrein en datum (zonder pagina reload).
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetBezetteTijdslots(int terreinId, string datum)
    {
        if (!DateTime.TryParse(datum, out var d))
            return Json(new { success = false, message = "Ongeldige datum." });

        var reservaties = await _db.Reservaties
            .Where(r => r.TerreinId == terreinId && r.Datum.Date == d.Date && !r.IsDeleted)
            .Select(r => new
            {
                start = r.StartUur.ToString(@"hh\:mm"),
                einde = r.EindUur.ToString(@"hh\:mm")
            })
            .ToListAsync();

        return Json(new { success = true, bezet = reservaties });
    }

    // ==================== HULPFUNCTIES ====================

    private async Task VulDropDownsIn(ReservationEditVm vm)
    {
        var terreinen = await _db.Terreinen.OrderBy(t => t.Naam).ToListAsync();
        vm.Courts = terreinen
            .Select(t => new SelectListItem { Value = t.Id.ToString(), Text = $"{t.Naam} (€ {t.Uurtarief:F2}/u)" })
            .ToList();

        var materialen = await _db.Materialen
            .Where(m => m.IsActief && !m.IsDeleted)
            .OrderBy(m => m.Naam)
            .ToListAsync();

        vm.Equipment = new List<SelectListItem>
        {
            new SelectListItem { Value = "", Text = "(Geen materiaal)" }
        };

        vm.Equipment.AddRange(materialen.Select(m =>
            new SelectListItem
            {
                Value = m.Id.ToString(),
                Text = $"{m.Naam} (€ {m.Huurprijs:F2} - beschikbaar: {m.AvailableQuantity})"
            }));
    }

    private bool TryParseTijden(ReservationEditVm vm, out TimeSpan start, out TimeSpan end)
    {
        start = default;
        end = default;

        bool okStart = TimeSpan.TryParseExact(vm.StartTime, new[] { @"h\:mm", @"hh\:mm" },
            CultureInfo.InvariantCulture, out start);

        bool okEnd = TimeSpan.TryParseExact(vm.EndTime, new[] { @"h\:mm", @"hh\:mm" },
            CultureInfo.InvariantCulture, out end);

        return okStart && okEnd;
    }
}
