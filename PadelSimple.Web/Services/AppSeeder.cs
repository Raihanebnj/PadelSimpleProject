using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using PadelSimple.Models.Data;
using PadelSimple.Models.Identity;

namespace PadelSimple.Web.Services;

public class AppSeeder
{
    private readonly AppDbContext _db;
    private readonly RoleManager<AppRole> _roleManager;
    private readonly UserManager<AppUser> _userManager;
    private readonly IConfiguration _config;

    public AppSeeder(
        AppDbContext db,
        RoleManager<AppRole> roleManager,
        UserManager<AppUser> userManager,
        IConfiguration config)
    {
        _db = db;
        _roleManager = roleManager;
        _userManager = userManager;
        _config = config;
    }

    public async Task SeedAsync()
    {
        await _db.Database.MigrateAsync();

        // Rollen
        foreach (var role in new[] { "Admin", "Staff", "Member" })
        {
            if (!await _roleManager.RoleExistsAsync(role))
            {
                await _roleManager.CreateAsync(new AppRole { Name = role });
            }
        }

        // Admin credentials uit secrets / config
        var adminEmail = _config["SeedAdmin:Email"];
        var adminPassword = _config["SeedAdmin:Password"];

        // Veilig: seeding overslaan als secrets ontbreken
        if (string.IsNullOrWhiteSpace(adminEmail) || string.IsNullOrWhiteSpace(adminPassword))
            return;

        var admin = await _userManager.FindByEmailAsync(adminEmail);

        if (admin == null)
        {
            admin = new AppUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true,
                IsMember = true,
                IsBlocked = false
            };

            var result = await _userManager.CreateAsync(admin, adminPassword);
            if (!result.Succeeded)
            {
                throw new Exception(string.Join(" | ", result.Errors.Select(e => e.Description)));
            }
        }

        if (!await _userManager.IsInRoleAsync(admin, "Admin"))
        {
            await _userManager.AddToRoleAsync(admin, "Admin");
        }
    }
}
