using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PadelSimple.Models.Data;
using PadelSimple.Models.Domain;

namespace PadelSimple.Web.Controllers.Api;

[ApiController]
[Route("api/equipment")]
public class EquipmentApiController : ControllerBase
{
    private readonly AppDbContext _db;
    public EquipmentApiController(AppDbContext db) => _db = db;

    [HttpGet, Authorize]
    public async Task<ActionResult<List<Equipment>>> GetAll()
        => await _db.Equipment.OrderBy(e => e.Name).ToListAsync();

    [HttpGet("{id:int}"), Authorize]
    public async Task<ActionResult<Equipment>> Get(int id)
    {
        var e = await _db.Equipment.FindAsync(id);
        return e == null ? NotFound() : Ok(e);
    }

    [HttpPost, Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> Create(Equipment model)
    {
        model.AvailableQuantity = Math.Min(model.AvailableQuantity, model.TotalQuantity);
        _db.Equipment.Add(model);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = model.Id }, model);
    }

    [HttpPut("{id:int}"), Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> Update(int id, Equipment model)
    {
        if (id != model.Id) return BadRequest();

        var existing = await _db.Equipment.FindAsync(id);
        if (existing == null) return NotFound();

        existing.Name = model.Name;
        existing.TotalQuantity = model.TotalQuantity;
        existing.AvailableQuantity = Math.Min(model.AvailableQuantity, model.TotalQuantity);
        existing.IsActive = model.IsActive;

        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}"), Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var existing = await _db.Equipment.FindAsync(id);
        if (existing == null) return NotFound();

        existing.IsDeleted = true;
        existing.DeletedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return NoContent();
    }
}
