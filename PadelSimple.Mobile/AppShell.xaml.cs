using Microsoft.Extensions.DependencyInjection;
using PadelSimple.Mobile.Views;

namespace PadelSimple.Mobile;

public partial class AppShell : Shell
{
    public AppShell(IServiceProvider services)
    {
        InitializeComponent();

       
        var login = new ShellContent
        {
            Route = "login",
            Content = services.GetRequiredService<LoginPage>()
        };

        
        var tabBar = new TabBar { Route = "main" };

        tabBar.Items.Add(new ShellContent
        {
            Title = "Terreinen",
            Route = "courts",
            Content = services.GetRequiredService<CourtsPage>()
        });

        tabBar.Items.Add(new ShellContent
        {
            Title = "Materiaal",
            Route = "equipment",
            Content = services.GetRequiredService<EquipmentPage>()
        });

        tabBar.Items.Add(new ShellContent
        {
            Title = "Reservaties",
            Route = "reservations",
            Content = services.GetRequiredService<ReservationsPage>()
        });

        Items.Add(login);
        Items.Add(tabBar);

        
        CurrentItem = login;
    }
}
