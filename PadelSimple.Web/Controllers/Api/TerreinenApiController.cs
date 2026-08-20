using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PadelSimple.Models.Data;
using PadelSimple.Models.Domain;

namespace PadelSimple.Web.Controllers.Api;

/// <summary>
/// RESTful API controller voor het beheren van terreinen.
/// Ondersteunt JWT Bearer authenticatie en sessie cookies.
/// </summary>
[ApiController]
[Route("api/terreinen")]
[Authorize(AuthenticationSchemes = "Bearer,Identity.Application")]
public class TerreinenApiController : ControllerBase
{
    private readonly AppDbContext _db;
    public TerreinenApiController(AppDbContext db) => _db = db;

    /// <summary>GET /api/terreinen: Ophalen van alle actieve terreinen</summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<List<Terrein>>> GetAll()
        => await _db.Terreinen.Where(t => !t.IsVerwijderd).OrderBy(t => t.Naam).ToListAsync();

    /// <summary>GET /api/terreinen/{id}: Ophalen van specifiek terrein</summary>
    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<ActionResult<Terrein>> Get(int id)
    {
        var t = await _db.Terreinen.FirstOrDefaultAsync(x => x.Id == id && !x.IsVerwijderd);
        return t == null ? NotFound(new { message = "Terrein niet gevonden." }) : Ok(t);
    }

    /// <summary>POST /api/terreinen: Nieuw terrein toevoegen (Admin, Medewerker)</summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] Terrein model)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        _db.Terreinen.Add(model);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = model.Id }, model);
    }

    /// <summary>PUT /api/terreinen/{id}: Terrein bijwerken (Admin, Medewerker)</summary>
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, [FromBody] Terrein model)
    {
        if (id != model.Id) return BadRequest(new { message = "ID mismatch." });

        var existing = await _db.Terreinen.FirstOrDefaultAsync(x => x.Id == id && !x.IsVerwijderd);
        if (existing == null) return NotFound(new { message = "Terrein niet gevonden." });

        existing.Naam = model.Naam;
        existing.Capaciteit = model.Capaciteit;
        existing.IsIndoors = model.IsIndoors;
        existing.Uurtarief = model.Uurtarief;

        await _db.SaveChangesAsync();
        return NoContent();
    }

    /// <summary>DELETE /api/terreinen/{id}: Terrein soft-deleten (Admin)</summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var existing = await _db.Terreinen.FirstOrDefaultAsync(x => x.Id == id && !x.IsVerwijderd);
        if (existing == null) return NotFound(new { message = "Terrein niet gevonden." });

        existing.IsVerwijderd = true;
        existing.VerwijderdOp = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return NoContent();
    }
}
