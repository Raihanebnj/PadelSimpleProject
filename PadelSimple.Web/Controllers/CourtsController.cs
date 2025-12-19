using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PadelSimple.Models.Data;
using PadelSimple.Web.ViewModels.Courts;
using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace PadelSimple.Web.Controllers;

[Authorize]
public class CourtsController : Controller
{
    private readonly AppDbContext _db;

    public CourtsController(AppDbContext db)
    {
        _db = db;
    }

    
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

        // Courts ophalen
        var courts = await _db.Courts
            .OrderBy(c => c.Name)
            .ToListAsync();

        // Reservaties voor die dag 1x ophalen (in-memory verwerken => SQLite-safe)
        var reservations = await _db.Reservations
            .Where(r => r.Date.Date == vm.Date.Date)
            .Select(r => new { r.CourtId, r.StartTime, r.EndTime })
            .ToListAsync();

        vm.Courts = courts.Select(c =>
        {
            var courtRes = reservations
                .Where(r => r.CourtId == c.Id)
                .OrderBy(r => r.StartTime)
                .ToList();

            bool available;
            string? freeFrom = null;

            if (hasSlot)
            {
                // Overlap check met gekozen slot
                var overlaps = courtRes
                    .Where(r => r.StartTime < endTs && startTs < r.EndTime)
                    .ToList();

                available = overlaps.Count == 0;

                if (!available)
                {
                    // Vrij vanaf = laatste eindtijd van de overlappende reservaties
                    var lastEnd = overlaps.Max(r => r.EndTime);
                    freeFrom = lastEnd.ToString(@"hh\:mm");
                }
            }
            else
            {
                // Geen slot: vrij als er geen reservaties zijn op die dag
                if (!courtRes.Any())
                {
                    available = true;
                }
                else
                {
                    available = false;
                    // Vrij vanaf = eindtijd van de laatste reservatie die dag
                    freeFrom = courtRes.Last().EndTime.ToString(@"hh\:mm");
                }
            }

            return new CourtRowVm
            {
                Id = c.Id,
                Name = c.Name,
                Capacity = c.Capacity,
                IsIndoor = c.IsIndoor,
                IsAvailable = available,
                FreeFrom = freeFrom
            };
        }).ToList();

        return View(vm);
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
