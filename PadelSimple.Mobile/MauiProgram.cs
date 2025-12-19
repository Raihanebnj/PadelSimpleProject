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

       
        builder.Services.AddHttpClient<ApiClient>(client =>
        {
            client.BaseAddress = new Uri(ApiConfig.BaseUrl);
        });

      
        builder.Services.AddSingleton<App>();

     
        builder.Services.AddSingleton<AppShell>();

  
        builder.Services.AddSingleton<LocalDb>();
        builder.Services.AddSingleton<AuthService>();
        builder.Services.AddSingleton<CourtsService>();
        builder.Services.AddSingleton<EquipmentService>();
        builder.Services.AddSingleton<ReservationsService>();
        builder.Services.AddSingleton<SyncService>();

     
        builder.Services.AddTransient<LoginVm>();
        builder.Services.AddTransient<CourtsVm>();
        builder.Services.AddTransient<EquipmentVm>();
        builder.Services.AddTransient<ReservationsVm>();

   
        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<CourtsPage>();
        builder.Services.AddTransient<EquipmentPage>();
        builder.Services.AddTransient<ReservationsPage>();

        return builder.Build();
    }
}
