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
public class ReservatiesApiController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly UserManager<AppGebruiker> _userManager;

    public ReservatiesApiController(AppDbContext db, UserManager<AppGebruiker> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    /// <summary>GET /api/reservaties: Ophalen van reservaties (met optionele datum filter)</summary>
    [HttpGet]
    public async Task<ActionResult<List<ReservatieDto>>> GetAll([FromQuery] DateTime? datum)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized(new { message = "Je bent niet ingelogd of je sessie is verlopen." });

        var query = _db.Reservaties
            .Include(r => r.Terrein)
            .Include(r => r.Materiaal)
            .Include(r => r.Gebruiker)
            .Where(r => !r.IsVerwijderd)
            .AsQueryable();

        // Klant ziet enkel eigen reservaties; Admin/Medewerker zien alles
        bool isBeheerder = User.IsInRole("Admin") || User.IsInRole("Medewerker");
        if (!isBeheerder)
        {
            query = query.Where(r => r.GebruikerId == user.Id);
        }

        if (datum.HasValue)
        {
            query = query.Where(r => r.Datum.Date == datum.Value.Date);
        }

        var lijst = await query.ToListAsync();
        var dtos = lijst
            .OrderBy(r => r.Datum)
            .ThenBy(r => r.StartUur)
            .Select(r => new ReservatieDto(
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
    public async Task<ActionResult<ReservatieDto>> Get(int id)
    {
        var r = await _db.Reservaties
            .Include(x => x.Terrein)
            .Include(x => x.Materiaal)
            .Include(x => x.Gebruiker)
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsVerwijderd);

        if (r == null) return NotFound(new { message = "Reservatie niet gevonden." });

        var user = await _userManager.GetUserAsync(User);
        bool isBeheerder = User.IsInRole("Admin") || User.IsInRole("Medewerker");
        if (!isBeheerder && r.GebruikerId != user?.Id)
            return Forbid();

        var dto = new ReservatieDto(
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
    public async Task<IActionResult> Create([FromBody] ReservatieCreateDto model)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized(new { message = "Je bent niet ingelogd of je sessie is verlopen." });
        if (user.IsGeblokkeerd) return StatusCode(StatusCodes.Status403Forbidden, new { message = "Account is geblokkeerd." });

        int terreinId = model.TerreinId;
        DateTime datum = model.Datum.Date;
        TimeSpan startUur = model.StartUur;
        TimeSpan eindUur = model.EindUur;

        if (terreinId <= 0) return BadRequest(new { message = "Selecteer een geldig terrein." });
        if (startUur >= eindUur) return BadRequest(new { message = "Starttijd moet vóór eindtijd liggen." });

        // Overlap validatie
        var zelfdeTerrein = await _db.Reservaties
            .Where(r => r.TerreinId == terreinId && r.Datum.Date == datum && !r.IsVerwijderd)
            .ToListAsync();

        bool overlap = zelfdeTerrein.Any(r => r.StartUur < eindUur && startUur < r.EindUur);
        if (overlap) return BadRequest(new { message = "Er bestaat al een reservatie voor dit terrein op dit tijdslot." });

        // Materiaal voorraad check
        int? matId = (model.MateriaalId.HasValue && model.MateriaalId.Value > 0) ? model.MateriaalId : null;
        int matAantal = matId.HasValue ? Math.Max(1, model.MateriaalAantal ?? 1) : 0;

        if (matId.HasValue && matAantal > 0)
        {
            var mat = await _db.Materialen.FirstOrDefaultAsync(m => m.Id == matId.Value && !m.IsVerwijderd);
            if (mat == null) return BadRequest(new { message = "Geselecteerd materiaal niet gevonden." });
            if (mat.BeschikbaarAantal < matAantal) return BadRequest(new { message = "Niet genoeg materiaal beschikbaar." });

            mat.BeschikbaarAantal -= matAantal;
        }

        // Prijs berekenen
        var terrein = await _db.Terreinen.FindAsync(terreinId);
        if (terrein == null) return BadRequest(new { message = "Geselecteerd terrein niet gevonden." });

        var duur = (decimal)(eindUur - startUur).TotalHours;
        decimal totalePrijs = terrein.Uurtarief * duur;

        if (matId.HasValue && matAantal > 0)
        {
            var mat = await _db.Materialen.FindAsync(matId.Value);
            if (mat != null) totalePrijs += mat.Huurprijs * matAantal;
        }

        var reservatie = new Reservatie
        {
            GebruikerId = user.Id,
            TerreinId = terreinId,
            Datum = datum,
            StartUur = startUur,
            EindUur = eindUur,
            AantalSpelers = Math.Max(1, model.AantalSpelers),
            MateriaalId = matId,
            AantalMateriaal = matAantal,
            TotalePrijs = totalePrijs
        };

        _db.Reservaties.Add(reservatie);
        await _db.SaveChangesAsync();

        var resultDto = new ReservatieDto(
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
        var r = await _db.Reservaties.FirstOrDefaultAsync(x => x.Id == id && !x.IsVerwijderd);
        if (r == null) return NotFound(new { message = "Reservatie niet gevonden." });

        var user = await _userManager.GetUserAsync(User);
        bool isBeheerder = User.IsInRole("Admin") || User.IsInRole("Medewerker");
        if (!isBeheerder && r.GebruikerId != user?.Id)
            return Forbid();

        r.IsVerwijderd = true;
        r.VerwijderdOp = DateTime.UtcNow;

        if (r.MateriaalId.HasValue && r.AantalMateriaal > 0)
        {
            var mat = await _db.Materialen.FindAsync(r.MateriaalId.Value);
            if (mat != null)
                mat.BeschikbaarAantal = Math.Min(mat.AantalInInventaris, mat.BeschikbaarAantal + r.AantalMateriaal);
        }

        await _db.SaveChangesAsync();
        return NoContent();
    }
}
