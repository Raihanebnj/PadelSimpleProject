using System.Windows;
using System.Windows.Controls;
using PadelSimple.Desktop.ViewModels;

namespace PadelSimple.Desktop.Views;

public partial class AanmeldenVenster : Window
{
    public AanmeldenVenster(AanmeldenViewModel vm)
    {
        InitializeComponent();
        DataContext = vm;
    }

    private void WachtwoordVak_OnPasswordChanged(object sender, RoutedEventArgs e)
    {
        if (DataContext is AanmeldenViewModel vm && sender is PasswordBox pb)
            vm.Wachtwoord = pb.Password;
    }

    private void RegWachtwoord1_OnPasswordChanged(object sender, RoutedEventArgs e)
    {
        if (DataContext is AanmeldenViewModel vm && sender is PasswordBox pb)
            vm.RegistratieWachtwoord = pb.Password;
    }

    private void RegWachtwoord2_OnPasswordChanged(object sender, RoutedEventArgs e)
    {
        if (DataContext is AanmeldenViewModel vm && sender is PasswordBox pb)
            vm.RegistratieWachtwoordHerhaal = pb.Password;
    }
}
