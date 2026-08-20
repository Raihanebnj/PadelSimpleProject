using Microsoft.Extensions.Logging;
using PadelSimple.Mobile.Services;
using PadelSimple.Mobile.ViewModels;
using PadelSimple.Mobile.Views;

namespace PadelSimple.Mobile;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            });

#if DEBUG
        builder.Logging.AddDebug();
#endif

        builder.Services.AddHttpClient<ApiKlant>(client =>
        {
            client.BaseAddress = new Uri(ApiConfig.BaseUrl);
        });

        // App infrastructure
        builder.Services.AddSingleton<App>();
        builder.Services.AddSingleton<AppShell>();

        // Services (State & DB)
        builder.Services.AddSingleton<LokaleDb>();
        builder.Services.AddSingleton<AuthService>();
        builder.Services.AddSingleton<TerreinenService>();
        builder.Services.AddSingleton<MateriaalService>();
        builder.Services.AddSingleton<ReservatiesService>();
        builder.Services.AddSingleton<SynchronisatieService>();

        // ViewModels
        builder.Services.AddTransient<LoginVm>();
        builder.Services.AddTransient<TerreinenVm>();
        builder.Services.AddTransient<MateriaalVm>();
        builder.Services.AddTransient<ReservatiesVm>();
        builder.Services.AddTransient<NieuweReservatieVm>();

        // Pages
        builder.Services.AddTransient<LoginPagina>();
        builder.Services.AddTransient<TerreinenPagina>();
        builder.Services.AddTransient<MateriaalPagina>();
        builder.Services.AddTransient<ReservatiesPagina>();
        builder.Services.AddTransient<NieuweReservatiePagina>();

        return builder.Build();
    }
}
