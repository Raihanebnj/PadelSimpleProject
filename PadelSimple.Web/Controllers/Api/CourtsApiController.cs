using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PadelSimple.Models.Data;
using PadelSimple.Models.Domain;

namespace PadelSimple.Web.Controllers.Api;

[ApiController]
[Route("api/courts")]
public class CourtsApiController : ControllerBase
{
    private readonly AppDbContext _db;
    public CourtsApiController(AppDbContext db) => _db = db;

    [HttpGet, Authorize]
    public async Task<ActionResult<List<Court>>> GetAll()
        => await _db.Courts.OrderBy(c => c.Name).ToListAsync();

    [HttpGet("{id:int}"), Authorize]
    public async Task<ActionResult<Court>> Get(int id)
    {
        var c = await _db.Courts.FindAsync(id);
        return c == null ? NotFound() : Ok(c);
    }

    [HttpPost, Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> Create(Court model)
    {
        _db.Courts.Add(model);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = model.Id }, model);
    }

    [HttpPut("{id:int}"), Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> Update(int id, Court model)
    {
        if (id != model.Id) return BadRequest();

        var existing = await _db.Courts.FindAsync(id);
        if (existing == null) return NotFound();

        existing.Name = model.Name;
        existing.Capacity = model.Capacity;
        existing.IsIndoor = model.IsIndoor;

        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}"), Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var existing = await _db.Courts.FindAsync(id);
        if (existing == null) return NotFound();

        existing.IsDeleted = true;
        existing.DeletedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return NoContent();
    }
}
