using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PadelSimple.Models.Identity;
using PadelSimple.Web.Models;
using PadelSimple.Web.Services;

namespace PadelSimple.Web.Controllers;

public class AccountController : Controller
{
    private readonly UserManager<AppUser> _users;
    private readonly SignInManager<AppUser> _signIn;
    private readonly IEmailSender _email;
    private readonly ILogger<AccountController> _logger;

    public AccountController(UserManager<AppUser> users, SignInManager<AppUser> signIn, IEmailSender email, ILogger<AccountController> logger)
    {
        _users = users;
        _signIn = signIn;
        _email = email;
        _logger = logger;
    }

    [HttpGet]
    public IActionResult Login() => View(new LoginVm());

    [HttpPost]
    public async Task<IActionResult> Login(LoginVm vm)
    {
        if (!ModelState.IsValid) return View(vm);

        // zoeken op email of username
        var user = await _users.FindByEmailAsync(vm.EmailOrUserName) ?? await _users.FindByNameAsync(vm.EmailOrUserName);
        if (user == null)
        {
            ModelState.AddModelError("", "Gebruiker niet gevonden.");
            return View(vm);
        }

        if (user.IsBlocked)
        {
            ModelState.AddModelError("", "Je account is geblokkeerd.");
            return View(vm);
        }

        if (!user.EmailConfirmed)
        {
            ModelState.AddModelError("", "Bevestig eerst je e-mail.");
            return View(vm);
        }

        var result = await _signIn.PasswordSignInAsync(user, vm.Password, vm.RememberMe, lockoutOnFailure: false);
        if (!result.Succeeded)
        {
            ModelState.AddModelError("", "Ongeldig wachtwoord.");
            return View(vm);
        }

        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult Register() => View(new RegisterVm());

    [HttpPost]
    public async Task<IActionResult> Register(RegisterVm vm)
    {
        if (!ModelState.IsValid) return View(vm);

        var user = new AppUser
        {
            UserName = vm.Email,
            Email = vm.Email,
            IsMember = true,
            IsBlocked = false
        };

        var created = await _users.CreateAsync(user, vm.Password);
        if (!created.Succeeded)
        {
            foreach (var e in created.Errors) ModelState.AddModelError("", e.Description);
            return View(vm);
        }

        // automatisch rol Member
        await _users.AddToRoleAsync(user, "Member");

        // email confirm link
        var token = await _users.GenerateEmailConfirmationTokenAsync(user);
        var url = Url.Action(nameof(ConfirmEmail), "Account",
            new { userId = user.Id, token }, Request.Scheme);

        await _email.SendAsync(vm.Email, "Bevestig je account",
            $"Klik op deze link om te bevestigen: <a href=\"{url}\">Bevestigen</a>");

        return RedirectToAction(nameof(RegisterDone));
    }

    [HttpGet]
    public IActionResult RegisterDone() => View();

    [HttpGet]
    public async Task<IActionResult> ConfirmEmail(string userId, string token)
    {
        var user = await _users.FindByIdAsync(userId);
        if (user == null) return NotFound();

        var ok = await _users.ConfirmEmailAsync(user, token);
        if (!ok.Succeeded)
        {
            _logger.LogWarning("Email confirm failed for {UserId}", userId);
            return View("ConfirmFailed");
        }

        return View("ConfirmOk");
    }

    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        await _signIn.SignOutAsync();
        return RedirectToAction(nameof(Login));
    }

    public IActionResult AccessDenied() => View();
}
