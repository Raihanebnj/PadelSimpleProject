using Microsoft.Extensions.DependencyInjection;
using PadelSimple.Mobile.Views;

namespace PadelSimple.Mobile;

public partial class AppShell : Shell
{
    public AppShell(IServiceProvider services)
    {
        InitializeComponent();

        Routing.RegisterRoute("nieuwe_reservatie", typeof(NieuweReservatiePagina));

        var login = new ShellContent
        {
            Route = "login",
            Content = services.GetRequiredService<LoginPagina>()
        };

        var tabBar = new TabBar { Route = "main" };

        tabBar.Items.Add(new ShellContent
        {
            Title = "Terreinen",
            Route = "terreinen",
            Content = services.GetRequiredService<TerreinenPagina>()
        });

        tabBar.Items.Add(new ShellContent
        {
            Title = "Materiaal",
            Route = "materiaal",
            Content = services.GetRequiredService<MateriaalPagina>()
        });

        tabBar.Items.Add(new ShellContent
        {
            Title = "Reservaties",
            Route = "reservaties",
            Content = services.GetRequiredService<ReservatiesPagina>()
        });

        tabBar.Items.Add(new ShellContent
        {
            Title = "Nieuwe Reservatie",
            Route = "nieuwe_reservatie_tab",
            Content = services.GetRequiredService<NieuweReservatiePagina>()
        });

        Items.Add(login);
        Items.Add(tabBar);

        CurrentItem = login;
    }
}
