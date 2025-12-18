using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PadelSimple.Models.Data;
using PadelSimple.Models.Domain;
using PadelSimple.Models.Identity;

namespace PadelSimple.Web.Services;

public class DbSeeder
{
    private readonly AppDbContext _db;
    private readonly UserManager<AppUser> _users;
    private readonly RoleManager<AppRole> _roles;
    private readonly ILogger<DbSeeder> _logger;

    public DbSeeder(AppDbContext db, UserManager<AppUser> users, RoleManager<AppRole> roles, ILogger<DbSeeder> logger)
    {
        _db = db;
        _users = users;
        _roles = roles;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        await _db.Database.MigrateAsync();

        foreach (var roleName in new[] { "Admin", "Staff", "Member" })
        {
            if (!await _roles.RoleExistsAsync(roleName))
                await _roles.CreateAsync(new AppRole { Name = roleName });
        }

        // Demo data (courts/equipment) als leeg
        if (!await _db.Courts.AnyAsync())
        {
            _db.Courts.AddRange(
                new Court { Name = "Court 1", Capacity = 4, IsIndoor = false },
                new Court { Name = "Court 2", Capacity = 4, IsIndoor = true }
            );
        }

        if (!await _db.Equipment.AnyAsync())
        {
            _db.Equipment.AddRange(
                new Equipment { Name = "Padelracket", TotalQuantity = 20, AvailableQuantity = 20, IsActive = true },
                new Equipment { Name = "Ballen set", TotalQuantity = 30, AvailableQuantity = 30, IsActive = true }
            );
        }

        await _db.SaveChangesAsync();

        // Admin user
        var adminEmail = "admin@padel.local";
        var admin = await _users.FindByEmailAsync(adminEmail);
        if (admin == null)
        {
            admin = new AppUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true,   // handig voor start
                IsMember = true,
                IsBlocked = false
            };

            var created = await _users.CreateAsync(admin, "Admin123!");
            if (!created.Succeeded)
            {
                _logger.LogError("Admin create failed: {Errors}", string.Join(", ", created.Errors.Select(e => e.Description)));
                return;
            }

            await _users.AddToRoleAsync(admin, "Admin");
        }
    }
}
