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

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        try
        {
            // Host maar één keer bouwen
            if (AppHost == null)
            {
                AppHost = Host.CreateDefaultBuilder()
                    .ConfigureServices((context, services) =>
                    {
                        // DB – vaste locatie in AppData
                        services.AddDbContextFactory<AppDbContext>(options =>
                        {
                            var folder = Path.Combine(
                                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                                "PadelSimple");

                            Directory.CreateDirectory(folder);

                            var dbPath = Path.Combine(folder, "padelsimple.db");
                            options.UseSqlite($"Data Source={dbPath}");
                        });

                        // Identity
                        services.AddIdentityCore<AppUser>(options =>
                        {
                            options.Password.RequireDigit = false;
                            options.Password.RequireNonAlphanumeric = false;
                            options.Password.RequireUppercase = false;
                            options.Password.RequiredLength = 6;
                        })
                            .AddRoles<AppRole>()
                            .AddEntityFrameworkStores<AppDbContext>();

                        // Services
                        services.AddScoped<AuthService>();
                        services.AddScoped<DataService>();

                        // ViewModels
                        services.AddTransient<LoginViewModel>();
                        services.AddTransient<MainViewModel>();
                        services.AddTransient<ReservationDialogViewModel>();

                        // Windows
                        services.AddTransient<LoginWindow>();
                        services.AddTransient<MainWindow>();
                        services.AddTransient<ReservationDialog>();
                    })
                    .Build();
            }

            await AppHost.StartAsync();
            await SeedDataAsync(AppHost.Services);

            var login = AppHost.Services.GetRequiredService<LoginWindow>();
            MainWindow = login;
            login.Show();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Er is een fout opgetreden bij het starten van de applicatie:\n{ex.Message}",
                "Opstartfout",
                MessageBoxButton.OK,
                MessageBoxImage.Error);

            Shutdown(-1);
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
            throw new InvalidOperationException("AppHost is nog niet geïnitialiseerd. OnStartup is nog niet uitgevoerd.");

        return AppHost.Services.GetRequiredService<T>();
    }

    private static async Task SeedDataAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var ctx = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        // Migrations doe je via Update-Database, niet hier.

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<AppRole>>();

        // Rollen
        foreach (var roleName in new[] { "Admin", "Staff", "Member" })
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new AppRole
                {
                    Name = roleName,
                    NormalizedName = roleName.ToUpper()
                });
            }
        }

        // Admin user
        var admin = await userManager.FindByNameAsync("admin");
        if (admin == null)
        {
            admin = new AppUser
            {
                UserName = "admin",
                Email = "admin@padel.local",
                IsMember = true
            };

            var result = await userManager.CreateAsync(admin, "Admin123!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(admin, "Admin");
            }
            // eventueel else: logging / MessageBox als aanmaken mislukt
        }
    }
}
