using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PadelSimple.Models.Data;
using PadelSimple.Models.Domain;
using PadelSimple.Models.Identity;
using PadelSimple.Web.ViewModels;
using System.Globalization;

namespace PadelSimple.Web.Controllers;

[Authorize]
public class ReservationsController : Controller
{
    private readonly AppDbContext _db;
    private readonly UserManager<AppUser> _userMgr;

    public ReservationsController(AppDbContext db, UserManager<AppUser> userMgr)
    {
        _db = db;
        _userMgr = userMgr;
    }

    public async Task<IActionResult> Index(DateTime? date = null)
    {
        var user = await _userMgr.GetUserAsync(User);
        if (user == null) return Challenge();

        var q = _db.Reservations
            .Include(r => r.Court)
            .Include(r => r.Equipment)
            .Include(r => r.User)
            .AsQueryable();

        // User ziet eigen reservaties, Admin ziet alles
        if (!User.IsInRole("Admin"))
            q = q.Where(r => r.UserId == user.Id);

        if (date.HasValue)
            q = q.Where(r => r.Date.Date == date.Value.Date);

        
        var list = await q.ToListAsync();
        list = list
            .OrderBy(r => r.Date)
            .ThenBy(r => r.StartTime)
            .ToList();

        ViewBag.SelectedDate = date?.ToString("yyyy-MM-dd");
        return View(list);
    }

    public async Task<IActionResult> Create()
    {
        var vm = new ReservationEditVm();
        await FillDropDowns(vm);
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ReservationEditVm vm)
    {
        var user = await _userMgr.GetUserAsync(User);
        if (user == null) return Challenge();

        if (!TryParseTimes(vm, out var start, out var end))
            ModelState.AddModelError("", "Start/Eindtijd ongeldig (bv 18:00).");

        if (end <= start)
            ModelState.AddModelError("", "Eindtijd moet na starttijd liggen.");

        if (!ModelState.IsValid)
        {
            await FillDropDowns(vm);
            return View(vm);
        }

        // Overlap-check (zelfde court + datum)
        var sameCourt = await _db.Reservations
            .Where(r => r.CourtId == vm.CourtId && r.Date.Date == vm.Date.Date)
            .ToListAsync();

        bool overlap = sameCourt.Any(r => r.StartTime < end && start < r.EndTime);
        if (overlap)
        {
            ModelState.AddModelError("", "Er bestaat al een reservatie voor dit terrein en tijdslot.");
            await FillDropDowns(vm);
            return View(vm);
        }

        // Equipment stock
        if (vm.EquipmentId.HasValue && vm.EquipmentQuantity > 0)
        {
            var eq = await _db.Equipment.FirstOrDefaultAsync(e => e.Id == vm.EquipmentId.Value);
            if (eq == null)
            {
                ModelState.AddModelError("", "Materiaal niet gevonden.");
                await FillDropDowns(vm);
                return View(vm);
            }

            if (!eq.IsActive || eq.AvailableQuantity < vm.EquipmentQuantity)
            {
                ModelState.AddModelError("", "Niet genoeg materiaal beschikbaar.");
                await FillDropDowns(vm);
                return View(vm);
            }

            eq.AvailableQuantity -= vm.EquipmentQuantity;
        }

        var res = new Reservation
        {
            Date = vm.Date.Date,
            StartTime = start,
            EndTime = end,
            CourtId = vm.CourtId,
            EquipmentId = (vm.EquipmentId.HasValue && vm.EquipmentQuantity > 0) ? vm.EquipmentId : null,
            EquipmentQuantity = (vm.EquipmentId.HasValue && vm.EquipmentQuantity > 0) ? vm.EquipmentQuantity : null,
            NumberOfPlayers = vm.NumberOfPlayers,
            UserId = user.Id
        };

        _db.Reservations.Add(res);
        await _db.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    // Admin mag alles verwijderen; user mag enkel eigen verwijderen
    public async Task<IActionResult> Delete(int id)
    {
        var user = await _userMgr.GetUserAsync(User);
        if (user == null) return Challenge();

        var res = await _db.Reservations
            .Include(r => r.Court)
            .Include(r => r.Equipment)
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (res == null) return NotFound();

        if (!User.IsInRole("Admin") && res.UserId != user.Id)
            return Forbid();

        return View(res);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var user = await _userMgr.GetUserAsync(User);
        if (user == null) return Challenge();

        var res = await _db.Reservations.FirstOrDefaultAsync(r => r.Id == id);
        if (res == null) return RedirectToAction(nameof(Index));

        if (!User.IsInRole("Admin") && res.UserId != user.Id)
            return Forbid();

        // stock terug
        if (res.EquipmentId.HasValue && res.EquipmentQuantity.HasValue && res.EquipmentQuantity.Value > 0)
        {
            var eq = await _db.Equipment.FirstOrDefaultAsync(e => e.Id == res.EquipmentId.Value);
            if (eq != null)
            {
                eq.AvailableQuantity += res.EquipmentQuantity.Value;
                if (eq.AvailableQuantity > eq.TotalQuantity)
                    eq.AvailableQuantity = eq.TotalQuantity;
            }
        }

        _db.Reservations.Remove(res);
        await _db.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    private async Task FillDropDowns(ReservationEditVm vm)
    {
        var courts = await _db.Courts.OrderBy(c => c.Name).ToListAsync();
        vm.Courts = courts
            .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name })
            .ToList();

        var equipment = await _db.Equipment
            .Where(e => e.IsActive)
            .OrderBy(e => e.Name)
            .ToListAsync();

        vm.Equipment = new List<SelectListItem>
        {
            new SelectListItem { Value = "", Text = "(Geen)" }
        };

        vm.Equipment.AddRange(equipment.Select(e =>
            new SelectListItem
            {
                Value = e.Id.ToString(),
                Text = $"{e.Name} (beschikbaar: {e.AvailableQuantity})"
            }));
    }

    private bool TryParseTimes(ReservationEditVm vm, out TimeSpan start, out TimeSpan end)
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
