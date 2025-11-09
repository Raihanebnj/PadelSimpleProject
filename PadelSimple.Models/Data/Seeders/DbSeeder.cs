using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using PadelSimple.Models.Domain;
using PadelSimple.Models.Identity;
using System.Threading.Tasks;

namespace PadelSimple.Models.Data.Seeders
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(
            AppDbContext db,
            RoleManager<AppRole> roleManager,
            UserManager<AppUser> userManager,
            IConfiguration config)
        {
            await db.Database.EnsureCreatedAsync();

            // Ensure Roles
            if (!await roleManager.RoleExistsAsync("Admin"))
                await roleManager.CreateAsync(new AppRole { Name = "Admin" });

            if (!await roleManager.RoleExistsAsync("Staff"))
                await roleManager.CreateAsync(new AppRole { Name = "Staff" });

            if (!await roleManager.RoleExistsAsync("Member"))
                await roleManager.CreateAsync(new AppRole { Name = "Member" });

            // Ensure Admin User
            var adminEmail = "admin@club.com";
            var adminPass = config["Identity:AdminPassword"]; // from secrets.json

            if (string.IsNullOrWhiteSpace(adminPass))
                throw new System.Exception("Missing secret Identity:AdminPassword");

            var admin = await userManager.FindByEmailAsync(adminEmail);

            if (admin == null)
            {
                admin = new AppUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    IsMember = false,
                    IsDeleted = false
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