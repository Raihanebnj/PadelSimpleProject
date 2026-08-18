using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PadelSimple.Desktop.Services;
using PadelSimple.Desktop.ViewModels;
using PadelSimple.Desktop.Views;
using PadelSimple.Models.Data;
using PadelSimple.Models.Identity;

namespace PadelSimple.Desktop;

public partial class App : Application
{
    public static IHost? AppHost { get; private set; }

    public App()
    {
        ShutdownMode = ShutdownMode.OnLastWindowClose;
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        try
        {
            if (AppHost == null)
            {
                AppHost = Host.CreateDefaultBuilder()
                    .ConfigureServices((context, services) =>
                    {
                        // ---- Database ----
                        services.AddDbContextFactory<AppDbContext>(options =>
                        {
                            var folder = Path.Combine(
                                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                                "PadelSimple");
                            Directory.CreateDirectory(folder);
                            var dbPath = Path.Combine(folder, "padelsimple.db");
                            options.UseSqlite($"Data Source={dbPath}");
                            options.ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
                        });

                        // ---- ASP.NET Identity & Data Protection ----
                        services.AddDataProtection();
                        services.AddIdentityCore<AppUser>(options =>
                        {
                            options.Password.RequireDigit = false;
                            options.Password.RequireNonAlphanumeric = false;
                            options.Password.RequireUppercase = false;
                            options.Password.RequiredLength = 6;
                        })
                        .AddRoles<AppRole>()
                        .AddEntityFrameworkStores<AppDbContext>()
                        .AddDefaultTokenProviders();

                        // ---- Services ----
                        services.AddScoped<AuthService>();
                        services.AddScoped<DataService>();

                        // ---- ViewModels ----
                        services.AddTransient<LoginViewModel>();
                        services.AddTransient<MainViewModel>();
                        services.AddTransient<ReservationDialogViewModel>();

                        // ---- Windows ----
                        services.AddTransient<LoginWindow>();
                        services.AddTransient<MainWindow>();
                        services.AddTransient<ReservationDialog>();
                    })
                    .Build();
            }

            await AppHost.StartAsync();

            // Databank en seed-data initialiseren bij opstarten
            await SeedDataAsync(AppHost.Services);

            var login = AppHost.Services.GetRequiredService<LoginWindow>();
            MainWindow = login;
            login.Show();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Er is een fout opgetreden bij het starten van de applicatie:\n\n{ex.Message}",
                "Opstartfout",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            Shutdown(-1);
        }
    }

    private static async Task SeedDataAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.EnsureCreatedAsync();

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<AppRole>>();
        var hasher = new PasswordHasher<AppUser>();

        // Rollen garanderen
        foreach (var roleName in new[] { "Admin", "Klant" })
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new AppRole(roleName));
            }
        }

        // Admin gebruiker garanderen & wachtwoord instellen
        var admin = await userManager.FindByEmailAsync("admin@padelsimple.be");
        if (admin == null)
        {
            admin = new AppUser
            {
                UserName = "admin@padelsimple.be",
                Email = "admin@padelsimple.be",
                Voornaam = "Administrator",
                Achternaam = "PadelSimple",
                Telefoonnummer = "+32 498 00 00 01",
                IsLid = true,
                EmailConfirmed = true
            };
            var res = await userManager.CreateAsync(admin, "Admin123!");
            if (res.Succeeded)
            {
                await userManager.AddToRoleAsync(admin, "Admin");
            }
        }
        else
        {
            admin.PasswordHash = hasher.HashPassword(admin, "Admin123!");
            admin.IsBlocked = false;
            admin.IsDeleted = false;
            await userManager.UpdateAsync(admin);

            if (!await userManager.IsInRoleAsync(admin, "Admin"))
            {
                await userManager.AddToRoleAsync(admin, "Admin");
            }
        }

        // Klant gebruiker garanderen & wachtwoord instellen
        var klant = await userManager.FindByEmailAsync("klant@padelsimple.be");
        if (klant == null)
        {
            klant = new AppUser
            {
                UserName = "klant@padelsimple.be",
                Email = "klant@padelsimple.be",
                Voornaam = "Jan",
                Achternaam = "Janssen",
                Telefoonnummer = "+32 476 12 34 56",
                IsLid = true,
                EmailConfirmed = true
            };
            var res = await userManager.CreateAsync(klant, "Klant123!");
            if (res.Succeeded)
            {
                await userManager.AddToRoleAsync(klant, "Klant");
            }
        }
        else
        {
            klant.PasswordHash = hasher.HashPassword(klant, "Klant123!");
            klant.IsBlocked = false;
            klant.IsDeleted = false;
            await userManager.UpdateAsync(klant);

            if (!await userManager.IsInRoleAsync(klant, "Klant"))
            {
                await userManager.AddToRoleAsync(klant, "Klant");
            }
        }
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        if (AppHost != null)
        {
            await AppHost.StopAsync();
            AppHost.Dispose();
        }
        base.OnExit(e);
    }

    public static T GetService<T>() where T : class
    {
        if (AppHost == null)
            throw new InvalidOperationException("AppHost is nog niet geïnitialiseerd.");
        return AppHost.Services.GetRequiredService<T>();
    }
}
