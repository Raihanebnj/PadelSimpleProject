using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PadelSimple.Models.Data;

namespace PadelSimple.Web.Controllers.Api;

[Route("api/courts")]
[ApiController]
public class CourtsController : ControllerBase
{
    private readonly AppDbContext _db;
    public CourtsController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> Get() =>
        Ok(await _db.Courts.OrderBy(c => c.Name).ToListAsync());

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] object dto)
    {
        // maak proper DTO later
        return BadRequest("TODO");
    }
}
