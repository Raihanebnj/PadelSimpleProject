using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using PadelSimple.Models.Identity;

namespace PadelSimple.Web.Controllers.Api;

/// <summary>
/// RESTful API Controller voor authenticatie via JWT Tokens.
/// </summary>
[ApiController]
[Route("api/auth")]
public class AuthenticatieApiController : ControllerBase
{
    private readonly UserManager<AppGebruiker> _userManager;
    private readonly SignInManager<AppGebruiker> _signInManager;
    private readonly IConfiguration _config;
    private readonly ILogger<AuthenticatieApiController> _logger;

    public AuthenticatieApiController(
        UserManager<AppGebruiker> userManager,
        SignInManager<AppGebruiker> signInManager,
        IConfiguration config,
        ILogger<AuthenticatieApiController> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _config = config;
        _logger = logger;
    }

    /// <summary>
    /// POST /api/auth/inloggen: Authenticeert een gebruiker en retourneert een JWT Bearer token.
    /// </summary>
    [HttpPost("inloggen")]
    public async Task<IActionResult> Inloggen([FromBody] ApiLoginAanvraag model)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var gebruiker = await _userManager.FindByEmailAsync(model.Email);
        if (gebruiker == null)
        {
            _logger.LogWarning("API Login mislukt: onbekend e-mailadres {Email}.", model.Email);
            return Unauthorized(new { message = "Ongeldig e-mailadres of wachtwoord." });
        }

        if (gebruiker.IsGeblokkeerd)
        {
            _logger.LogWarning("API Login geweigerd: account {Email} geblokkeerd.", model.Email);
            return StatusCode(StatusCodes.Status403Forbidden, new { message = "Account is geblokkeerd." });
        }

        var resultaat = await _signInManager.CheckPasswordSignInAsync(gebruiker, model.Wachtwoord, false);
        if (!resultaat.Succeeded)
        {
            _logger.LogWarning("API Login mislukt: verkeerd wachtwoord voor {Email}.", model.Email);
            return Unauthorized(new { message = "Ongeldig e-mailadres of wachtwoord." });
        }

        var tokenString = await GenereerJwtTokenAsync(gebruiker);
        _logger.LogInformation("JWT token gegenereerd voor API gebruiker {Email}.", model.Email);

        return Ok(new
        {
            token = tokenString,
            email = gebruiker.Email,
            voornaam = gebruiker.Voornaam,
            achternaam = gebruiker.Achternaam,
            isLid = gebruiker.IsLid
        });
    }

    /// <summary>
    /// POST /api/auth/registreren: Registreert een nieuwe klant via de API en retourneert direct een JWT Bearer token.
    /// </summary>
    [HttpPost("registreren")]
    public async Task<IActionResult> Registreren([FromBody] ApiRegistratieAanvraag model)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var bestaand = await _userManager.FindByEmailAsync(model.Email);
        if (bestaand != null)
        {
            return BadRequest(new { message = "E-mailadres is al in gebruik." });
        }

        var gebruiker = new AppGebruiker
        {
            UserName = model.Email,
            Email = model.Email,
            Voornaam = model.Voornaam,
            Achternaam = model.Achternaam,
            Telefoonnummer = model.Telefoon ?? string.Empty,
            IsLid = model.IsLid,
            IsGeblokkeerd = false
        };

        var resultaat = await _userManager.CreateAsync(gebruiker, model.Wachtwoord);
        if (!resultaat.Succeeded)
        {
            return BadRequest(new { errors = resultaat.Errors.Select(e => e.Description) });
        }

        await _userManager.AddToRoleAsync(gebruiker, "Klant");
        _logger.LogInformation("Nieuwe gebruiker {Email} geregistreerd via API.", model.Email);

        var tokenString = await GenereerJwtTokenAsync(gebruiker);

        return Ok(new
        {
            token = tokenString,
            email = gebruiker.Email,
            voornaam = gebruiker.Voornaam,
            achternaam = gebruiker.Achternaam
        });
    }

    private async Task<string> GenereerJwtTokenAsync(AppGebruiker gebruiker)
    {
        var rollen = await _userManager.GetRolesAsync(gebruiker);
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, gebruiker.Id),
            new Claim(ClaimTypes.Name, gebruiker.Email ?? string.Empty),
            new Claim(ClaimTypes.Email, gebruiker.Email ?? string.Empty),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        foreach (var rol in rollen)
        {
            claims.Add(new Claim(ClaimTypes.Role, rol));
        }

        var sleutelStr = _config["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key niet ingesteld.");
        var sleutel = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(sleutelStr));
        var creds = new SigningCredentials(sleutel, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(double.Parse(_config["Jwt:ExpiresInMinutes"] ?? "120")),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

public class ApiLoginAanvraag
{
    public string Email { get; set; } = string.Empty;
    public string Wachtwoord { get; set; } = string.Empty;
}

public class ApiRegistratieAanvraag
{
    public string Email { get; set; } = string.Empty;
    public string Wachtwoord { get; set; } = string.Empty;
    public string Voornaam { get; set; } = string.Empty;
    public string Achternaam { get; set; } = string.Empty;
    public string? Telefoon { get; set; }
    public bool IsLid { get; set; }
}
