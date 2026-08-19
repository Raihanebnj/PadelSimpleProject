using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PadelSimple.Models.Data;
using PadelSimple.Models.Domain;
using PadelSimple.Models.Dtos;
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
    public async Task<ActionResult<List<ReservationDto>>> GetAll([FromQuery] DateTime? date)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized(new { message = "Je bent niet ingelogd of je sessie is verlopen." });

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
        var dtos = lijst
            .OrderBy(r => r.Datum)
            .ThenBy(r => r.StartUur)
            .Select(r => new ReservationDto(
                r.Id,
                r.TerreinId,
                r.Terrein?.Naam ?? $"Terrein #{r.TerreinId}",
                r.Datum,
                r.StartUur,
                r.EindUur,
                r.AantalSpelers,
                r.MateriaalId,
                r.Materiaal?.Naam,
                r.AantalMateriaal
            ))
            .ToList();

        return Ok(dtos);
    }

    /// <summary>GET /api/reservaties/{id}: Ophalen van een specifieke reservatie</summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ReservationDto>> Get(int id)
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

        var dto = new ReservationDto(
            r.Id,
            r.TerreinId,
            r.Terrein?.Naam ?? $"Terrein #{r.TerreinId}",
            r.Datum,
            r.StartUur,
            r.EindUur,
            r.AantalSpelers,
            r.MateriaalId,
            r.Materiaal?.Naam,
            r.AantalMateriaal
        );

        return Ok(dto);
    }

    /// <summary>POST /api/reservaties: Nieuwe reservatie aanmaken met overlap en voorraad validatie</summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ReservationCreateDto model)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized(new { message = "Je bent niet ingelogd of je sessie is verlopen." });
        if (user.IsBlocked) return StatusCode(StatusCodes.Status403Forbidden, new { message = "Account is geblokkeerd." });

        int courtId = model.CourtId;
        DateTime datum = model.Date.Date;
        TimeSpan startUur = model.StartTime;
        TimeSpan eindUur = model.EndTime;

        if (courtId <= 0) return BadRequest(new { message = "Selecteer een geldig terrein." });
        if (startUur >= eindUur) return BadRequest(new { message = "Starttijd moet vóór eindtijd liggen." });

        // Overlap validatie
        var zelfdeTerrein = await _db.Reservaties
            .Where(r => r.TerreinId == courtId && r.Datum.Date == datum && !r.IsDeleted)
            .ToListAsync();

        bool overlap = zelfdeTerrein.Any(r => r.StartUur < eindUur && startUur < r.EindUur);
        if (overlap) return BadRequest(new { message = "Er bestaat al een reservatie voor dit terrein op dit tijdslot." });

        // Materiaal voorraad check
        int? matId = (model.EquipmentId.HasValue && model.EquipmentId.Value > 0) ? model.EquipmentId : null;
        int matAantal = matId.HasValue ? Math.Max(1, model.EquipmentQuantity ?? 1) : 0;

        if (matId.HasValue && matAantal > 0)
        {
            var mat = await _db.Materialen.FirstOrDefaultAsync(m => m.Id == matId.Value && !m.IsDeleted);
            if (mat == null) return BadRequest(new { message = "Geselecteerd materiaal niet gevonden." });
            if (mat.AvailableQuantity < matAantal) return BadRequest(new { message = "Niet genoeg materiaal beschikbaar." });

            mat.AvailableQuantity -= matAantal;
        }

        // Prijs berekenen
        var terrein = await _db.Terreinen.FindAsync(courtId);
        if (terrein == null) return BadRequest(new { message = "Geselecteerd terrein niet gevonden." });

        var duur = (decimal)(eindUur - startUur).TotalHours;
        decimal totalePrijs = terrein.Uurtarief * duur;

        if (matId.HasValue && matAantal > 0)
        {
            var mat = await _db.Materialen.FindAsync(matId.Value);
            if (mat != null) totalePrijs += mat.Huurprijs * matAantal;
        }

        var reservatie = new Reservation
        {
            UserId = user.Id,
            TerreinId = courtId,
            Datum = datum,
            StartUur = startUur,
            EindUur = eindUur,
            AantalSpelers = Math.Max(1, model.NumberOfPlayers),
            MateriaalId = matId,
            AantalMateriaal = matAantal,
            TotalePrijs = totalePrijs
        };

        _db.Reservaties.Add(reservatie);
        await _db.SaveChangesAsync();

        var resultDto = new ReservationDto(
            reservatie.Id,
            reservatie.TerreinId,
            terrein.Naam,
            reservatie.Datum,
            reservatie.StartUur,
            reservatie.EindUur,
            reservatie.AantalSpelers,
            reservatie.MateriaalId,
            matId.HasValue ? (await _db.Materialen.FindAsync(matId.Value))?.Naam : null,
            reservatie.AantalMateriaal
        );

        return CreatedAtAction(nameof(Get), new { id = reservatie.Id }, resultDto);
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
