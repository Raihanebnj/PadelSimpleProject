using System.Windows;
using PadelSimple.Desktop.ViewModels;

namespace PadelSimple.Desktop.Views;

public partial class MainWindow : Window
{
    public MainWindow(MainViewModel vm)
    {
        InitializeComponent();
        DataContext = vm;

        Loaded += async (_, __) =>
        {
            if (DataContext is MainViewModel mvm)
                await mvm.LoadDataCommand.ExecuteAsync(null);
        };
    }

    private void Exit_Click(object sender, RoutedEventArgs e)
    {
        Application.Current.Shutdown();
    }
}
