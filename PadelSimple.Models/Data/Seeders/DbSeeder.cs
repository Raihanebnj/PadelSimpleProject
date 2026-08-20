using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using PadelSimple.Models.Data;
using PadelSimple.Models.Identity;
using System.Threading.Tasks;

namespace PadelSimple.Models.Data.Seeders
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(
            AppDbContext db,
            RoleManager<AppRol> roleManager,
            UserManager<AppGebruiker> userManager,
            IConfiguration config)
        {
            await db.Database.EnsureCreatedAsync();

            // Ensure Roles
            if (!await roleManager.RoleExistsAsync("Admin"))
                await roleManager.CreateAsync(new AppRol { Name = "Admin" });

            if (!await roleManager.RoleExistsAsync("Medewerker"))
                await roleManager.CreateAsync(new AppRol { Name = "Medewerker" });

            if (!await roleManager.RoleExistsAsync("Klant"))
                await roleManager.CreateAsync(new AppRol { Name = "Klant" });

            // Ensure Admin User
            var adminEmail = "admin@club.com";
            var adminPass = config["Identity:AdminPassword"]; // from secrets.json

            if (string.IsNullOrWhiteSpace(adminPass))
                throw new System.Exception("Missing secret Identity:AdminPassword");

            var admin = await userManager.FindByEmailAsync(adminEmail);

            if (admin == null)
            {
                admin = new AppGebruiker
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    IsLid = false,
                    IsVerwijderd = false
                };

                var result = await userManager.CreateAsync(admin, adminPass);

                if (!result.Succeeded)
                    throw new System.Exception("Admin create failed: " +
                        string.Join(", ", result.Errors));

                await userManager.AddToRoleAsync(admin, "Admin");
            }
        }
    }
}
