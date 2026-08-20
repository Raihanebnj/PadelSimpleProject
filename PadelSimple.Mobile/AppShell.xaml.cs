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
            Title = "Login",
            Route = "login",
            ContentTemplate = new DataTemplate(() => services.GetRequiredService<LoginPagina>())
        };

        var tabBar = new TabBar { Route = "main" };

        tabBar.Items.Add(new ShellContent
        {
            Title = "Terreinen",
            Route = "terreinen",
            ContentTemplate = new DataTemplate(() => services.GetRequiredService<TerreinenPagina>())
        });

        tabBar.Items.Add(new ShellContent
        {
            Title = "Materiaal",
            Route = "materiaal",
            ContentTemplate = new DataTemplate(() => services.GetRequiredService<MateriaalPagina>())
        });

        tabBar.Items.Add(new ShellContent
        {
            Title = "Reservaties",
            Route = "reservaties",
            ContentTemplate = new DataTemplate(() => services.GetRequiredService<ReservatiesPagina>())
        });

        tabBar.Items.Add(new ShellContent
        {
            Title = "Nieuwe Reservatie",
            Route = "nieuwe_reservatie_tab",
            ContentTemplate = new DataTemplate(() => services.GetRequiredService<NieuweReservatiePagina>())
        });

        Items.Add(login);
        Items.Add(tabBar);

        CurrentItem = login;
    }
}
