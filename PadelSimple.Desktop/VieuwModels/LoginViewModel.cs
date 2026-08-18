using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PadelSimple.Desktop.Services;
using PadelSimple.Desktop.Views;
using PadelSimple.Models.Domain;
using PadelSimple.Models.Identity;

namespace PadelSimple.Desktop.ViewModels
{
    public partial class LoginViewModel : ObservableObject
    {
        private readonly AuthService _authService;

        // ====== Login velden ======
        [ObservableProperty] private string userName = string.Empty;
        [ObservableProperty] private string password = string.Empty;
        [ObservableProperty] private string errorMessage = string.Empty;

        // ====== Registratie velden ======
        [ObservableProperty] private string registerEmail = string.Empty;
        [ObservableProperty] private string registerVoornaam = string.Empty;
        [ObservableProperty] private string registerAchternaam = string.Empty;
        [ObservableProperty] private string registerTelefoon = string.Empty;
        [ObservableProperty] private string registerPassword = string.Empty;
        [ObservableProperty] private string registerPasswordHerhaal = string.Empty;
        [ObservableProperty] private bool registerIsLid = false;
        [ObservableProperty] private string registerError = string.Empty;

        public LoginViewModel(AuthService authService)
        {
            _authService = authService;
        }

        [RelayCommand]
        private async Task Login(Window window)
        {
            ErrorMessage = string.Empty;
            try
            {
                var (ok, fout) = await _authService.LoginAsync(UserName, Password);
                if (!ok)
                {
                    ErrorMessage = fout;
                    return;
                }

                // Open MainWindow via DI
                var main = App.GetService<MainWindow>();
                main.Show();

                // Login venster sluiten
                window.Close();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Onverwachte fout: {ex.Message}";
            }
        }

        [RelayCommand]
        private async Task Register()
        {
            RegisterError = string.Empty;
            try
            {
                if (string.IsNullOrWhiteSpace(RegisterEmail))
                {
                    RegisterError = "E-mailadres is verplicht.";
                    return;
                }
                if (string.IsNullOrWhiteSpace(RegisterVoornaam))
                {
                    RegisterError = "Voornaam is verplicht.";
                    return;
                }
                if (string.IsNullOrWhiteSpace(RegisterAchternaam))
                {
                    RegisterError = "Achternaam is verplicht.";
                    return;
                }
                if (RegisterPassword != RegisterPasswordHerhaal)
                {
                    RegisterError = "Wachtwoorden komen niet overeen.";
                    return;
                }
                if (RegisterPassword.Length < 6)
                {
                    RegisterError = "Wachtwoord moet minstens 6 tekens bevatten.";
                    return;
                }

                var (ok, fout) = await _authService.RegisterAsync(
                    RegisterEmail,
                    RegisterPassword,
                    RegisterVoornaam,
                    RegisterAchternaam,
                    RegisterTelefoon,
                    RegisterIsLid);

                if (!ok)
                {
                    RegisterError = fout;
                    return;
                }

                MessageBox.Show(
                    "Account succesvol aangemaakt! U kunt nu inloggen.",
                    "Registratie geslaagd",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                // Velden wissen
                RegisterEmail = RegisterVoornaam = RegisterAchternaam =
                    RegisterTelefoon = RegisterPassword = RegisterPasswordHerhaal = string.Empty;
                RegisterIsLid = false;
            }
            catch (Exception ex)
            {
                RegisterError = $"Registratiefout: {ex.Message}";
            }
        }
    }
}
