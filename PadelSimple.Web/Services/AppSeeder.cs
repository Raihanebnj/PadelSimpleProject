using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PadelSimple.Models.Data;
using PadelSimple.Models.Domain;
using PadelSimple.Models.Identity;

namespace PadelSimple.Web.Services;

/// <summary>
/// Seeder die bij het opstarten de databank migreert en basisgegevens aanmaakt.
/// </summary>
public class AppSeeder
{
    private readonly AppDbContext _db;
    private readonly RoleManager<AppRole> _roleManager;
    private readonly UserManager<AppUser> _userManager;
    private readonly IConfiguration _config;
    private readonly ILogger<AppSeeder> _logger;

    public AppSeeder(
        AppDbContext db,
        RoleManager<AppRole> roleManager,
        UserManager<AppUser> userManager,
        IConfiguration config,
        ILogger<AppSeeder> logger)
    {
        _db = db;
        _roleManager = roleManager;
        _userManager = userManager;
        _config = config;
        _logger = logger;
    }

    /// <summary>
    /// Zorg dat de databank en alle tabellen bestaan en seeded rollen en gebruikers via Identity.
    /// </summary>
    public async Task SeedAsync()
    {
        // 1. Zorg dat de databank en alle tabellen bestaan
        await _db.Database.EnsureCreatedAsync();
        _logger.LogInformation("Database gecontroleerd/aangemaakt.");

        // 2. Maak de 3 vereiste rollen aan
        foreach (var rol in new[] { "Admin", "Medewerker", "Klant" })
        {
            if (!await _roleManager.RoleExistsAsync(rol))
            {
                await _roleManager.CreateAsync(new AppRole { Name = rol });
                _logger.LogInformation("Rol '{Rol}' aangemaakt.", rol);
            }
        }

        // 3. Seed standaard admin via user secrets / config
        var adminEmail = _config["SeedAdmin:Email"] ?? "admin@padel.local";
        var adminWachtwoord = _config["SeedAdmin:Password"] ?? "Admin123!";
        await MaakGebruikerAanAlsNietBestaatAsync(
            "USER_ADMIN_001", adminEmail, adminWachtwoord, "Administrator", "PadelSimple",
            "+32 498 00 00 01", isLid: true, rol: "Admin");

        // 4. Seed demo medewerker
        await MaakGebruikerAanAlsNietBestaatAsync(
            "USER_MEDEWERKER_001", "medewerker@padelsimple.be", "Medewerker123!", "Sara", "Maes",
            "+32 498 55 66 77", isLid: true, rol: "Medewerker");

        // 5. Seed demo klant
        await MaakGebruikerAanAlsNietBestaatAsync(
            "USER_KLANT_001", "klant@padelsimple.be", "Klant123!", "Jan", "Janssen",
            "+32 476 12 34 56", isLid: true, rol: "Klant");

        // 6. Seed terreinen als databank leeg is
        if (!await _db.Terreinen.AnyAsync())
        {
            _db.Terreinen.AddRange(
                new Terrein { Naam = "Terrein 1 (Overdekt)", Capaciteit = 4, IsIndoors = true, Uurtarief = 18.00m },
                new Terrein { Naam = "Terrein 2 (Buiten)", Capaciteit = 4, IsIndoors = false, Uurtarief = 12.00m },
                new Terrein { Naam = "Terrein 3 (Overdekt VIP)", Capaciteit = 4, IsIndoors = true, Uurtarief = 25.00m }
            );
            await _db.SaveChangesAsync();
            _logger.LogInformation("Terreinen geseed.");
        }

        // 7. Seed materialen als databank leeg is
        if (!await _db.Materialen.AnyAsync())
        {
            _db.Materialen.AddRange(
                new Materiaal { Naam = "Padelracket", AantalInInventaris = 20, AvailableQuantity = 20, Huurprijs = 5.00m, IsActief = true },
                new Materiaal { Naam = "Set Ballen", AantalInInventaris = 30, AvailableQuantity = 30, Huurprijs = 2.50m, IsActief = true },
                new Materiaal { Naam = "Beschermingsbril", AantalInInventaris = 15, AvailableQuantity = 15, Huurprijs = 1.50m, IsActief = true }
            );
            await _db.SaveChangesAsync();
            _logger.LogInformation("Materialen geseed.");
        }

        // 8. Seed reservaties als databank leeg is
        if (!await _db.Reservaties.AnyAsync())
        {
            var klantUser = await _userManager.FindByEmailAsync("klant@padelsimple.be");
            var adminUser = await _userManager.FindByEmailAsync(adminEmail);

            if (klantUser != null && adminUser != null)
            {
                _db.Reservaties.AddRange(
                    new Reservation
                    {
                        UserId = klantUser.Id,
                        TerreinId = 1,
                        MateriaalId = 1,
                        AantalMateriaal = 2,
                        Datum = DateTime.Today.AddDays(1),
                        StartUur = new TimeSpan(10, 0, 0),
                        EindUur = new TimeSpan(11, 0, 0),
                        TotalePrijs = 28.00m,
                        AantalSpelers = 4
                    },
                    new Reservation
                    {
                        UserId = klantUser.Id,
                        TerreinId = 2,
                        MateriaalId = null,
                        AantalMateriaal = 0,
                        Datum = DateTime.Today.AddDays(2),
                        StartUur = new TimeSpan(14, 0, 0),
                        EindUur = new TimeSpan(15, 0, 0),
                        TotalePrijs = 12.00m,
                        AantalSpelers = 2
                    },
                    new Reservation
                    {
                        UserId = adminUser.Id,
                        TerreinId = 3,
                        MateriaalId = 2,
                        AantalMateriaal = 1,
                        Datum = DateTime.Today.AddDays(3),
                        StartUur = new TimeSpan(18, 0, 0),
                        EindUur = new TimeSpan(19, 30, 0),
                        TotalePrijs = 40.00m,
                        AantalSpelers = 4
                    }
                );
                await _db.SaveChangesAsync();
                _logger.LogInformation("Reservaties geseed.");
            }
        }
    }

    /// <summary>
    /// Hulpfunctie: maakt een gebruiker aan via Identity met een geldige PasswordHash.
    /// </summary>
    private async Task MaakGebruikerAanAlsNietBestaatAsync(
        string fixedId, string email, string wachtwoord, string voornaam, string achternaam,
        string telefoon, bool isLid, string rol)
    {
        var gebruiker = await _userManager.FindByEmailAsync(email);
        if (gebruiker == null)
        {
            gebruiker = new AppUser
            {
                Id = fixedId,
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                Voornaam = voornaam,
                Achternaam = achternaam,
                Telefoonnummer = telefoon,
                IsLid = isLid,
                IsBlocked = false
            };

            var resultaat = await _userManager.CreateAsync(gebruiker, wachtwoord);
            if (!resultaat.Succeeded)
            {
                var fouten = string.Join(" | ", resultaat.Errors.Select(e => e.Description));
                _logger.LogError("Aanmaken van gebruiker '{Email}' mislukt: {Fouten}", email, fouten);
                return;
            }

            _logger.LogInformation("Gebruiker '{Email}' aangemaakt met ID {Id}.", email, fixedId);
        }
        else
        {
            var hasher = new PasswordHasher<AppUser>();
            gebruiker.PasswordHash = hasher.HashPassword(gebruiker, wachtwoord);
            gebruiker.SecurityStamp = Guid.NewGuid().ToString();
            await _userManager.UpdateAsync(gebruiker);
        }

        if (!await _userManager.IsInRoleAsync(gebruiker, rol))
        {
            await _userManager.AddToRoleAsync(gebruiker, rol);
            _logger.LogInformation("Rol '{Rol}' toegewezen aan '{Email}'.", rol, email);
        }
    }
}
