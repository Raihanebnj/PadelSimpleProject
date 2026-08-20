using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PadelSimple.Models.Identity;
using PadelSimple.Web.ViewModels.Auth;

namespace PadelSimple.Web.Controllers;

/// <summary>
/// Controller voor authenticatie: aanmelden, registreren, afmelden en profielbeheer.
/// </summary>
public class AccountController : Controller
{
    private readonly UserManager<AppGebruiker> _userManager;
    private readonly SignInManager<AppGebruiker> _signInManager;
    private readonly ILogger<AccountController> _logger;

    public AccountController(
        UserManager<AppGebruiker> userManager,
        SignInManager<AppGebruiker> signInManager,
        ILogger<AccountController> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _logger = logger;
    }

    // ==================== REGISTREREN ====================

    [HttpGet, AllowAnonymous]
    public IActionResult Registreren() => View(new RegisterVm());

    [HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
    public async Task<IActionResult> Registreren(RegisterVm vm)
    {
        if (!ModelState.IsValid) return View(vm);

        var bestaand = await _userManager.FindByEmailAsync(vm.Email);
        if (bestaand != null)
        {
            ModelState.AddModelError(nameof(vm.Email), "Dit e-mailadres is al in gebruik.");
            return View(vm);
        }

        var gebruiker = new AppGebruiker
        {
            UserName = vm.Email,
            Email = vm.Email,
            Voornaam = vm.Voornaam,
            Achternaam = vm.Achternaam,
            Telefoonnummer = vm.Telefoon,
            IsLid = vm.IsLid,
            IsGeblokkeerd = false
        };

        var resultaat = await _userManager.CreateAsync(gebruiker, vm.Wachtwoord);
        if (!resultaat.Succeeded)
        {
            foreach (var fout in resultaat.Errors)
                ModelState.AddModelError(string.Empty, fout.Description);
            return View(vm);
        }

        // Nieuwe gebruiker krijgt automatisch de rol 'Klant'
        await _userManager.AddToRoleAsync(gebruiker, "Klant");
        _logger.LogInformation("Nieuwe klant geregistreerd: {Email}.", vm.Email);

        await _signInManager.SignInAsync(gebruiker, isPersistent: false);
        TempData["Success"] = "Welkom bij PadelSimple! Uw account is aangemaakt.";
        return RedirectToAction("Overzicht", "Home");
    }

    // ==================== AANMELDEN ====================

    [HttpGet, AllowAnonymous]
    public IActionResult Inloggen(string? returnUrl = null)
    {
        ViewBag.ReturnUrl = returnUrl;
        return View(new LoginVm());
    }

    [HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
    public async Task<IActionResult> Inloggen(LoginVm vm, string? returnUrl = null)
    {
        if (!ModelState.IsValid) return View(vm);

        var gebruiker = await _userManager.FindByEmailAsync(vm.Email);
        if (gebruiker == null)
        {
            _logger.LogWarning("Mislukte aanmeldingspoging voor onbekend e-mailadres: {Email}.", vm.Email);
            ModelState.AddModelError(string.Empty, "Ongeldig e-mailadres of wachtwoord.");
            return View(vm);
        }

        if (gebruiker.IsGeblokkeerd)
        {
            _logger.LogWarning("Aanmelding geweigerd: account {Email} is geblokkeerd.", vm.Email);
            ModelState.AddModelError(string.Empty, "Uw account is geblokkeerd. Contacteer de beheerder.");
            return View(vm);
        }

        var resultaat = await _signInManager.PasswordSignInAsync(gebruiker, vm.Wachtwoord, vm.OnthoudMij, lockoutOnFailure: false);
        if (!resultaat.Succeeded)
        {
            _logger.LogWarning("Mislukte aanmeldingspoging voor {Email}.", vm.Email);
            ModelState.AddModelError(string.Empty, "Ongeldig e-mailadres of wachtwoord.");
            return View(vm);
        }

        _logger.LogInformation("Gebruiker {Email} aangemeld.", vm.Email);

        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);

        return RedirectToAction("Overzicht", "Home");
    }

    // ==================== AFMELDEN ====================

    [HttpPost, Authorize, ValidateAntiForgeryToken]
    public async Task<IActionResult> Uitloggen()
    {
        _logger.LogInformation("Gebruiker {Email} afgemeld.", User.Identity?.Name);
        await _signInManager.SignOutAsync();
        return RedirectToAction("Overzicht", "Home");
    }

    // ==================== PROFIEL ====================

    [HttpGet, Authorize]
    public async Task<IActionResult> Profiel()
    {
        var gebruiker = await _userManager.GetUserAsync(User);
        if (gebruiker == null) return Challenge();

        var vm = new ProfielVm
        {
            Voornaam = gebruiker.Voornaam,
            Achternaam = gebruiker.Achternaam,
            Email = gebruiker.Email ?? string.Empty,
            Telefoon = gebruiker.Telefoonnummer,
            IsLid = gebruiker.IsLid
        };

        return View(vm);
    }

    [HttpPost, Authorize, ValidateAntiForgeryToken]
    public async Task<IActionResult> Profiel(ProfielVm vm)
    {
        if (!ModelState.IsValid) return View(vm);

        var gebruiker = await _userManager.GetUserAsync(User);
        if (gebruiker == null) return Challenge();

        gebruiker.Voornaam = vm.Voornaam;
        gebruiker.Achternaam = vm.Achternaam;
        gebruiker.Telefoonnummer = vm.Telefoon;

        var resultaat = await _userManager.UpdateAsync(gebruiker);
        if (!resultaat.Succeeded)
        {
            foreach (var fout in resultaat.Errors)
                ModelState.AddModelError(string.Empty, fout.Description);
            return View(vm);
        }

        TempData["Success"] = "Profiel bijgewerkt.";
        _logger.LogInformation("Profiel bijgewerkt voor {Email}.", gebruiker.Email);
        return RedirectToAction(nameof(Profiel));
    }

    // ==================== TAAL WISSELEN ====================

    [HttpPost, AllowAnonymous]
    public IActionResult WisselTaal(string taal, string? terugUrl = null)
    {
        var ondersteund = new[] { "nl", "en", "fr" };
        if (!ondersteund.Contains(taal)) taal = "nl";

        Response.Cookies.Append("lang", taal, new CookieOptions
        {
            Expires = DateTimeOffset.UtcNow.AddYears(1),
            IsEssential = true,
            SameSite = SameSiteMode.Lax
        });

        if (!string.IsNullOrWhiteSpace(terugUrl) && Url.IsLocalUrl(terugUrl))
            return Redirect(terugUrl);

        return RedirectToAction("Overzicht", "Home");
    }

    // ==================== TOEGANG GEWEIGERD ====================

    public IActionResult ToegangGeweigerd() => View();
}
