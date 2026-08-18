using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PadelSimple.Models.Data;
using PadelSimple.Models.Domain;
using PadelSimple.Models.Identity;

namespace PadelSimple.Web.Controllers.Api;

/// <summary>
/// RESTful API controller voor het beheren van reservaties (CRUD met JSON responses).
/// </summary>
[ApiController]
[Route("api/reservaties")]
[Authorize(AuthenticationSchemes = "Bearer,Identity.Application")]
public class ReservationsApiController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly UserManager<AppUser> _userManager;

    public ReservationsApiController(AppDbContext db, UserManager<AppUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    /// <summary>GET /api/reservaties: Ophalen van reservaties (met optionele datum filter)</summary>
    [HttpGet]
    public async Task<ActionResult<List<Reservation>>> GetAll([FromQuery] DateTime? date)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var query = _db.Reservaties
            .Include(r => r.Terrein)
            .Include(r => r.Materiaal)
            .Include(r => r.User)
            .Where(r => !r.IsDeleted)
            .AsQueryable();

        // Klant ziet enkel eigen reservaties; Admin/Medewerker zien alles
        bool isBeheerder = User.IsInRole("Admin") || User.IsInRole("Medewerker");
        if (!isBeheerder)
        {
            query = query.Where(r => r.UserId == user.Id);
        }

        if (date.HasValue)
        {
            query = query.Where(r => r.Datum.Date == date.Value.Date);
        }

        var lijst = await query.ToListAsync();
        lijst = lijst.OrderBy(r => r.Datum).ThenBy(r => r.StartUur).ToList();
        return Ok(lijst);
    }

    /// <summary>GET /api/reservaties/{id}: Ophalen van een specifieke reservatie</summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<Reservation>> Get(int id)
    {
        var r = await _db.Reservaties
            .Include(x => x.Terrein)
            .Include(x => x.Materiaal)
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

        if (r == null) return NotFound(new { message = "Reservatie niet gevonden." });

        var user = await _userManager.GetUserAsync(User);
        bool isBeheerder = User.IsInRole("Admin") || User.IsInRole("Medewerker");
        if (!isBeheerder && r.UserId != user?.Id)
            return Forbid();

        return Ok(r);
    }

    /// <summary>POST /api/reservaties: Nieuwe reservatie aanmaken met overlap en voorraad validatie</summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Reservation model)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();
        if (user.IsBlocked) return StatusCode(StatusCodes.Status403Forbidden, new { message = "Account is geblokkeerd." });

        model.UserId = user.Id;
        model.Datum = model.Datum.Date;

        // Overlap validatie
        var zelfdeTerrein = await _db.Reservaties
            .Where(r => r.TerreinId == model.TerreinId && r.Datum.Date == model.Datum.Date && !r.IsDeleted)
            .ToListAsync();

        bool overlap = zelfdeTerrein.Any(r => r.StartUur < model.EindUur && model.StartUur < r.EindUur);
        if (overlap) return BadRequest(new { message = "Er bestaat al een reservatie voor dit terrein op dit tijdslot." });

        // Materiaal voorraad check
        if (model.MateriaalId.HasValue && model.AantalMateriaal > 0)
        {
            var mat = await _db.Materialen.FirstOrDefaultAsync(m => m.Id == model.MateriaalId.Value && !m.IsDeleted);
            if (mat == null) return BadRequest(new { message = "Materiaal niet gevonden." });
            if (mat.AvailableQuantity < model.AantalMateriaal) return BadRequest(new { message = "Niet genoeg materiaal beschikbaar." });

            mat.AvailableQuantity -= model.AantalMateriaal;
        }

        // Prijs berekenen
        var terrein = await _db.Terreinen.FindAsync(model.TerreinId);
        var duur = (decimal)(model.EindUur - model.StartUur).TotalHours;
        model.TotalePrijs = (terrein?.Uurtarief ?? 0m) * duur;

        if (model.MateriaalId.HasValue && model.AantalMateriaal > 0)
        {
            var mat = await _db.Materialen.FindAsync(model.MateriaalId.Value);
            if (mat != null) model.TotalePrijs += mat.Huurprijs * model.AantalMateriaal;
        }

        _db.Reservaties.Add(model);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(Get), new { id = model.Id }, model);
    }

    /// <summary>DELETE /api/reservaties/{id}: Reservatie annuleren/verwijderen</summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var r = await _db.Reservaties.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        if (r == null) return NotFound(new { message = "Reservatie niet gevonden." });

        var user = await _userManager.GetUserAsync(User);
        bool isBeheerder = User.IsInRole("Admin") || User.IsInRole("Medewerker");
        if (!isBeheerder && r.UserId != user?.Id)
            return Forbid();

        r.IsDeleted = true;
        r.DeletedAt = DateTime.UtcNow;

        if (r.MateriaalId.HasValue && r.AantalMateriaal > 0)
        {
            var mat = await _db.Materialen.FindAsync(r.MateriaalId.Value);
            if (mat != null)
                mat.AvailableQuantity = Math.Min(mat.AantalInInventaris, mat.AvailableQuantity + r.AantalMateriaal);
        }

        await _db.SaveChangesAsync();
        return NoContent();
    }
}
