using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PadelSimple.Models.Data;
using PadelSimple.Models.Domain;

namespace PadelSimple.Web.Controllers.Api;

/// <summary>
/// RESTful API controller voor het beheren van materiaal.
/// </summary>
[ApiController]
[Route("api/materiaal")]
[Authorize(AuthenticationSchemes = "Bearer,Identity.Application")]
public class MateriaalApiController : ControllerBase
{
    private readonly AppDbContext _db;
    public MateriaalApiController(AppDbContext db) => _db = db;

    /// <summary>GET /api/materiaal: Ophalen van al het actieve materiaal</summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<List<Materiaal>>> GetAll()
        => await _db.Materialen.Where(m => !m.IsVerwijderd).OrderBy(m => m.Naam).ToListAsync();

    /// <summary>GET /api/materiaal/{id}: Ophalen van specifiek materiaal</summary>
    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<ActionResult<Materiaal>> Get(int id)
    {
        var m = await _db.Materialen.FirstOrDefaultAsync(x => x.Id == id && !x.IsVerwijderd);
        return m == null ? NotFound(new { message = "Materiaal niet gevonden." }) : Ok(m);
    }

    /// <summary>POST /api/materiaal: Nieuw materiaal toevoegen (Admin, Medewerker)</summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] Materiaal model)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        model.BeschikbaarAantal = Math.Min(model.BeschikbaarAantal, model.AantalInInventaris);
        _db.Materialen.Add(model);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = model.Id }, model);
    }

    /// <summary>PUT /api/materiaal/{id}: Materiaal bijwerken (Admin, Medewerker)</summary>
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, [FromBody] Materiaal model)
    {
        if (id != model.Id) return BadRequest(new { message = "ID mismatch." });

        var existing = await _db.Materialen.FirstOrDefaultAsync(x => x.Id == id && !x.IsVerwijderd);
        if (existing == null) return NotFound(new { message = "Materiaal niet gevonden." });

        existing.Naam = model.Naam;
        existing.AantalInInventaris = model.AantalInInventaris;
        existing.BeschikbaarAantal = Math.Min(model.BeschikbaarAantal, model.AantalInInventaris);
        existing.Huurprijs = model.Huurprijs;
        existing.IsActief = model.IsActief;

        await _db.SaveChangesAsync();
        return NoContent();
    }

    /// <summary>DELETE /api/materiaal/{id}: Materiaal soft-deleten (Admin)</summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var existing = await _db.Materialen.FirstOrDefaultAsync(x => x.Id == id && !x.IsVerwijderd);
        if (existing == null) return NotFound(new { message = "Materiaal niet gevonden." });

        existing.IsVerwijderd = true;
        existing.VerwijderdOp = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return NoContent();
    }
}
