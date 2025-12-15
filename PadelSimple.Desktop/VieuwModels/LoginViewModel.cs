using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PadelSimple.Desktop.Services;
using PadelSimple.Desktop.Views;

namespace PadelSimple.Desktop.ViewModels;

public partial class LoginViewModel : ObservableObject
{
    private readonly AuthService _authService;

    [ObservableProperty] private string _userName = string.Empty;
    [ObservableProperty] private string _password = string.Empty;

    [ObservableProperty] private string _registerEmail = string.Empty;
    [ObservableProperty] private string _registerPassword = string.Empty;
    [ObservableProperty] private string _registerPasswordRepeat = string.Empty;

    [ObservableProperty] private string _errorMessage = string.Empty;
    [ObservableProperty] private string _registerError = string.Empty;

    public LoginViewModel(AuthService authService)
    {
        _authService = authService;
    }

    [RelayCommand]
    private async Task Login(Window window)
    {
        ErrorMessage = string.Empty;

        var (ok, error) = await _authService.LoginAsync(UserName, Password);
        if (!ok)
        {
            ErrorMessage = error;
            return;
        }

        // 🔹 Nieuwe MainWindow ophalen via DI
        var main = App.GetService<MainWindow>();

        // 🔹 Zeg expliciet tegen WPF: dit is nu de MainWindow
        Application.Current.MainWindow = main;

        main.Show();

        // 🔹 Loginwindow sluiten
        window.Close();
    }

    [RelayCommand]
    private async Task Register()
    {
        RegisterError = string.Empty;

        if (string.IsNullOrWhiteSpace(RegisterEmail) ||
            string.IsNullOrWhiteSpace(RegisterPassword))
        {
            RegisterError = "Vul e-mail en wachtwoord in.";
            return;
        }

        if (RegisterPassword != RegisterPasswordRepeat)
        {
            RegisterError = "Wachtwoorden komen niet overeen.";
            return;
        }

        var (ok, error) = await _authService.RegisterAsync(RegisterEmail, RegisterPassword);
        if (!ok)
        {
            RegisterError = error;
            return;
        }

        RegisterError = "Account aangemaakt. Je kan nu inloggen.";
        RegisterEmail = string.Empty;
        RegisterPassword = string.Empty;
        RegisterPasswordRepeat = string.Empty;
    }
}
