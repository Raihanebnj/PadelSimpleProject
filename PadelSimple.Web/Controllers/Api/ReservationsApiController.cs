using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PadelSimple.Models.Data;
using PadelSimple.Models.Domain;
using PadelSimple.Models.Identity;

namespace PadelSimple.Web.Controllers.Api;

[ApiController]
[Route("api/reservations")]
public class ReservationsApiController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly UserManager<AppUser> _userManager;

    public ReservationsApiController(AppDbContext db, UserManager<AppUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    [HttpGet, Authorize]
    public async Task<ActionResult<List<Reservation>>> GetAll([FromQuery] DateTime? date)
    {
        var q = _db.Reservations
            .Include(r => r.Court)
            .Include(r => r.Equipment)
            .Include(r => r.User)
            .AsQueryable();

        if (date.HasValue)
            q = q.Where(r => r.Date.Date == date.Value.Date);

        return await q.OrderBy(r => r.Date).ThenBy(r => r.StartTime).ToListAsync();
    }

    [HttpGet("{id:int}"), Authorize]
    public async Task<ActionResult<Reservation>> Get(int id)
    {
        var r = await _db.Reservations
            .Include(x => x.Court)
            .Include(x => x.Equipment)
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.Id == id);

        return r == null ? NotFound() : Ok(r);
    }

    // Create: user moet ingelogd zijn
    [HttpPost, Authorize]
    public async Task<IActionResult> Create(Reservation model)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();
        if (user.IsBlocked) return Forbid();

        model.UserId = user.Id;
        model.Date = model.Date.Date;

        // overlap check
        var sameCourt = await _db.Reservations
            .Where(r => r.CourtId == model.CourtId && r.Date.Date == model.Date.Date)
            .ToListAsync();

        var overlap = sameCourt.Any(r => r.StartTime < model.EndTime && model.StartTime < r.EndTime);
        if (overlap) return BadRequest("Er bestaat al een reservatie voor dit terrein en tijdslot.");

        // equipment
        if (model.EquipmentId.HasValue && model.EquipmentQuantity.HasValue && model.EquipmentQuantity.Value > 0)
        {
            var eq = await _db.Equipment.FindAsync(model.EquipmentId.Value);
            if (eq == null) return BadRequest("Materiaal niet gevonden.");
            if (eq.AvailableQuantity < model.EquipmentQuantity.Value) return BadRequest("Niet genoeg materiaal beschikbaar.");

            eq.AvailableQuantity -= model.EquipmentQuantity.Value;
        }

        _db.Reservations.Add(model);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(Get), new { id = model.Id }, model);
    }

    [HttpDelete("{id:int}"), Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> Delete(int id)
    {
        var r = await _db.Reservations.FirstOrDefaultAsync(x => x.Id == id);
        if (r == null) return NotFound();

        r.IsDeleted = true;
        r.DeletedAt = DateTime.UtcNow;

        if (r.EquipmentId.HasValue && r.EquipmentQuantity.HasValue && r.EquipmentQuantity.Value > 0)
        {
            var eq = await _db.Equipment.FindAsync(r.EquipmentId.Value);
            if (eq != null)
                eq.AvailableQuantity = Math.Min(eq.TotalQuantity, eq.AvailableQuantity + r.EquipmentQuantity.Value);
        }

        await _db.SaveChangesAsync();
        return NoContent();
    }
}
