using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PadelSimple.Models.Identity;

namespace PadelSimple.Web.Controllers;

[Authorize(Roles = "Admin")]
public class AdminGebruikersController : Controller
{
    private readonly UserManager<AppGebruiker> _userManager;

    public AdminGebruikersController(UserManager<AppGebruiker> userManager)
    {
        _userManager = userManager;
    }

    public async Task<IActionResult> Overzicht()
    {
        var gebruikers = await _userManager.Users.OrderBy(u => u.Email).ToListAsync();
        return View(gebruikers);
    }

    [HttpPost]
    public async Task<IActionResult> WisselBlokkering(string id)
    {
        var gebruiker = await _userManager.FindByIdAsync(id);
        if (gebruiker == null) return NotFound();

        gebruiker.IsGeblokkeerd = !gebruiker.IsGeblokkeerd;
        await _userManager.UpdateAsync(gebruiker);

        return RedirectToAction(nameof(Overzicht));
    }

    [HttpPost]
    public async Task<IActionResult> MaakAdmin(string id)
    {
        var gebruiker = await _userManager.FindByIdAsync(id);
        if (gebruiker == null) return NotFound();

        await _userManager.AddToRoleAsync(gebruiker, "Admin");
        return RedirectToAction(nameof(Overzicht));
    }

    [HttpPost]
    public async Task<IActionResult> VerwijderAdmin(string id)
    {
        var gebruiker = await _userManager.FindByIdAsync(id);
        if (gebruiker == null) return NotFound();

        await _userManager.RemoveFromRoleAsync(gebruiker, "Admin");
        return RedirectToAction(nameof(Overzicht));
    }
}
