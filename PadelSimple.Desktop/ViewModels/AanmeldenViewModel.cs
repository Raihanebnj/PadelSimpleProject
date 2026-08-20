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
    public partial class AanmeldenViewModel : ObservableObject
    {
        private readonly AuthenticatieService _authenticatieService;

        // ====== Aanmeldvelden ======
        [ObservableProperty] private string gebruikersnaam = string.Empty;
        [ObservableProperty] private string wachtwoord = string.Empty;
        [ObservableProperty] private string foutBoodschap = string.Empty;

        // ====== Registratievelden ======
        [ObservableProperty] private string registratieEmail = string.Empty;
        [ObservableProperty] private string registratieVoornaam = string.Empty;
        [ObservableProperty] private string registratieAchternaam = string.Empty;
        [ObservableProperty] private string registratieTelefoon = string.Empty;
        [ObservableProperty] private string registratieWachtwoord = string.Empty;
        [ObservableProperty] private string registratieWachtwoordHerhaal = string.Empty;
        [ObservableProperty] private bool registratieIsLid = false;
        [ObservableProperty] private string registratieFout = string.Empty;

        public AanmeldenViewModel(AuthenticatieService authenticatieService)
        {
            _authenticatieService = authenticatieService;
        }

        [RelayCommand]
        private async Task MeldAan(Window venster)
        {
            FoutBoodschap = string.Empty;
            try
            {
                var (geslaagd, fout) = await _authenticatieService.MeldAanAsync(Gebruikersnaam, Wachtwoord);
                if (!geslaagd)
                {
                    FoutBoodschap = fout;
                    return;
                }

                // Open Hoofdvenster via DI
                var hoofdvenster = App.GetService<Hoofdvenster>();
                hoofdvenster.Show();

                // Aanmeldvenster sluiten
                venster.Close();
            }
            catch (Exception ex)
            {
                FoutBoodschap = $"Onverwachte fout: {ex.Message}";
            }
        }

        [RelayCommand]
        private async Task Registreer()
        {
            RegistratieFout = string.Empty;
            try
            {
                if (string.IsNullOrWhiteSpace(RegistratieEmail))
                {
                    RegistratieFout = "E-mailadres is verplicht.";
                    return;
                }
                if (string.IsNullOrWhiteSpace(RegistratieVoornaam))
                {
                    RegistratieFout = "Voornaam is verplicht.";
                    return;
                }
                if (string.IsNullOrWhiteSpace(RegistratieAchternaam))
                {
                    RegistratieFout = "Achternaam is verplicht.";
                    return;
                }
                if (RegistratieWachtwoord != RegistratieWachtwoordHerhaal)
                {
                    RegistratieFout = "Wachtwoorden komen niet overeen.";
                    return;
                }
                if (RegistratieWachtwoord.Length < 6)
                {
                    RegistratieFout = "Wachtwoord moet minstens 6 tekens bevatten.";
                    return;
                }

                var (geslaagd, fout) = await _authenticatieService.RegistreerAsync(
                    RegistratieEmail,
                    RegistratieWachtwoord,
                    RegistratieVoornaam,
                    RegistratieAchternaam,
                    RegistratieTelefoon,
                    RegistratieIsLid);

                if (!geslaagd)
                {
                    RegistratieFout = fout;
                    return;
                }

                MessageBox.Show(
                    "Account succesvol aangemaakt! U kunt nu inloggen.",
                    "Registratie geslaagd",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                // Velden wissen
                RegistratieEmail = RegistratieVoornaam = RegistratieAchternaam =
                    RegistratieTelefoon = RegistratieWachtwoord = RegistratieWachtwoordHerhaal = string.Empty;
                RegistratieIsLid = false;
            }
            catch (Exception ex)
            {
                RegistratieFout = $"Registratiefout: {ex.Message}";
            }
        }
    }
}
