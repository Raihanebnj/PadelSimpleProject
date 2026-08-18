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
                        });

                        // ---- ASP.NET Identity ----
                        services.AddIdentityCore<AppUser>(options =>
                        {
                            options.Password.RequireDigit = false;
                            options.Password.RequireNonAlphanumeric = false;
                            options.Password.RequireUppercase = false;
                            options.Password.RequiredLength = 6;
                        })
                        .AddRoles<AppRole>()
                        .AddEntityFrameworkStores<AppDbContext>();

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

            // Migrations toepassen bij opstarten
            using (var scope = AppHost.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                await db.Database.MigrateAsync();
            }

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
