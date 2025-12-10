using System.Windows;
using PadelSimple.Desktop.ViewModels;

namespace PadelSimple.Desktop.Views;

public partial class MainWindow : Window
{
    public MainWindow(MainViewModel vm)
    {
        InitializeComponent();
        DataContext = vm;

        Loaded += async (_, _) => await vm.LoadDataAsync();
    }
}
