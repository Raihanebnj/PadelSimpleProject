using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PadelSimple.Models.Data;
using PadelSimple.Models.Data.Seeders;
using PadelSimple.Models.Identity;
using System.Configuration;
using System.Data;
using System.Windows;

namespace PadelSimple.Desktop;

public partial class App : Application
{
    public static IHost AppHost { get; private set; } = null!;

    public App()
    {

        AppHost = Host.CreateDefaultBuilder()
        .ConfigureAppConfiguration((ctx, cfg) =>
        {
            cfg.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
               .AddUserSecrets<App>(optional: true)
               .AddEnvironmentVariables();
        })
        .ConfigureServices((ctx, services) =>
        {
            var conn = ctx.Configuration.GetConnectionString("Default")!;
            services.AddDbContext<AppDbContext>(opt => opt.UseSqlite(conn));

            services.AddIdentity<AppUser, AppRole>(options =>
            {
                options.Password.RequiredLength = 6;
                options.Password.RequireDigit = false;
                options.Password.RequireUppercase = false;
            })
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders()
            .AddSignInManager();

            services.AddAuthentication();
            services.AddSingleton<IAuthenticationSchemeProvider, AuthenticationSchemeProvider>();

            services.AddSingleton<Services.AuthService>();
            services.AddScoped<Services.DataService>();

            services.AddTransient<Views.LoginWindow>();
            services.AddTransient<Views.MainWindow>();

            services.AddTransient<ViewModels.LoginViewModel>();
            services.AddTransient<ViewModels.MainViewModel>();
        })
        .Build();
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        await AppHost.StartAsync();

        // Seed DB
        using var scope = AppHost.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var roleMgr = scope.ServiceProvider.GetRequiredService<RoleManager<AppRole>>();
        var userMgr = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        var cfg = scope.ServiceProvider.GetRequiredService<IConfiguration>();

        try
        {
            await DbSeeder.SeedAsync(db, roleMgr, userMgr, cfg);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Database seeding error: {ex.Message}", "Init error");
            Shutdown(-1);
            return;
        }

        // Show login
        var login = AppHost.Services.GetRequiredService<Views.LoginWindow>();
        login.Show();

        base.OnStartup(e);
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        await AppHost.StopAsync();
        AppHost.Dispose();
        base.OnExit(e);
    }
}