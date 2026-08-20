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
    /// <summary>
    /// ViewModel voor het hoofdvenster. Beheert reservaties, terreinen, materialen en gebruikers.
    /// </summary>
    public partial class HoofdViewModel : ObservableObject
    {
        private readonly GegevensService _gegevensService;
        private readonly AuthenticatieService _authenticatieService;

        public ObservableCollection<Terrein> Terreinen { get; } = new();
        public ObservableCollection<Materiaal> MaterialenLijst { get; } = new();
        public ObservableCollection<Reservatie> Reservaties { get; } = new();
        public ObservableCollection<Reservatie> MijnReservaties { get; } = new();
        public ObservableCollection<AppGebruiker> Gebruikers { get; } = new();

        [ObservableProperty] private Terrein? geselecteerdTerrein;
        [ObservableProperty] private Materiaal? geselecteerdMateriaal;
        [ObservableProperty] private AppGebruiker? geselecteerdeGebruiker;
        [ObservableProperty] private Reservatie? geselecteerdeReservatie;
        [ObservableProperty] private Reservatie? geselecteerdeMijnReservatie;

        [ObservableProperty] private DateTime geselecteerdeDatum = DateTime.Today;

        public bool IsAdmin => _authenticatieService.IsAdmin;
        public string WelkomTekst => _authenticatieService.HuidigeGebruiker != null
            ? $"Welkom, {_authenticatieService.HuidigeGebruiker.Voornaam} {_authenticatieService.HuidigeGebruiker.Achternaam}"
            : "Welkom";

        public HoofdViewModel(GegevensService gegevensService, AuthenticatieService authenticatieService)
        {
            _gegevensService = gegevensService;
            _authenticatieService = authenticatieService;
        }

        // ================================================================
        //  DATA LADEN
        // ================================================================

        [RelayCommand]
        public async Task LaadData()
        {
            try
            {
                Terreinen.Clear();
                foreach (var t in await _gegevensService.GetTerreinen())
                    Terreinen.Add(t);

                MaterialenLijst.Clear();
                foreach (var m in await _gegevensService.GetMaterialen())
                    MaterialenLijst.Add(m);

                await LaadReservaties();

                if (_authenticatieService.IsAdmin)
                {
                    Gebruikers.Clear();
                    foreach (var u in await _authenticatieService.HaalAlleGebruikersOpAsync())
                        Gebruikers.Add(u);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fout bij laden van gegevens:\n{ex.Message}",
                    "Laadfout", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        public async Task LaadReservaties()
        {
            try
            {
                Reservaties.Clear();
                MijnReservaties.Clear();

                // 1. "Reservaties" tab:
                // - Admin ziet ALLE reservaties van alle klanten voor de geselecteerde datum.
                // - Klant ziet ENKEL zijn eigen reservaties.
                List<Reservatie> lijst;
                if (_authenticatieService.IsAdmin)
                {
                    lijst = await _gegevensService.GetReservaties(GeselecteerdeDatum);
                }
                else if (_authenticatieService.HuidigeGebruiker != null)
                {
                    var alleOpDatum = await _gegevensService.GetReservaties(GeselecteerdeDatum);
                    lijst = alleOpDatum.Where(r => r.GebruikerId == _authenticatieService.HuidigeGebruiker.Id).ToList();
                }
                else
                {
                    lijst = new List<Reservatie>();
                }

                foreach (var r in lijst)
                    Reservaties.Add(r);

                // 2. "Mijn Account" tab: toont ENKEL de eigen reservaties van de ingelogde gebruiker (Admin of Klant)
                if (_authenticatieService.HuidigeGebruiker != null)
                {
                    var mijnLijst = await _gegevensService.GetReservatiesVanGebruiker(_authenticatieService.HuidigeGebruiker.Id);
                    foreach (var r in mijnLijst)
                        MijnReservaties.Add(r);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fout bij laden van reservaties:\n{ex.Message}",
                    "Fout", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ================================================================
        //  RESERVATIES – CRUD
        // ================================================================

        [RelayCommand]
        public async Task NieuweReservatie()
        {
            if (_authenticatieService.HuidigeGebruiker == null)
            {
                MessageBox.Show("Je moet ingelogd zijn om een reservatie te maken.",
                    "Niet aangemeld", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var dialoog = App.GetService<ReservatieDialoog>();
            if (dialoog.DataContext is ReservatieDialoogViewModel vm)
                await vm.InitialiseerAsync(GeselecteerdeDatum, null);

            dialoog.Owner = Application.Current.MainWindow;
            if (dialoog.ShowDialog() == true)
                await LaadReservaties();
        }

        [RelayCommand]
        public async Task BewerkReservatie()
        {
            if (GeselecteerdeReservatie == null) return;

            var dialoog = App.GetService<ReservatieDialoog>();
            if (dialoog.DataContext is ReservatieDialoogViewModel vm)
                await vm.InitialiseerAsync(GeselecteerdeReservatie.Datum, GeselecteerdeReservatie);

            dialoog.Owner = Application.Current.MainWindow;
            if (dialoog.ShowDialog() == true)
                await LaadReservaties();
        }

        [RelayCommand]
        public async Task VerwijderReservatie()
        {
            if (GeselecteerdeReservatie == null) return;

            var bevestig = MessageBox.Show(
                $"Wilt u de reservatie voor {GeselecteerdeReservatie.Terrein?.Naam} op " +
                $"{GeselecteerdeReservatie.Datum:dd/MM/yyyy} om {GeselecteerdeReservatie.StartUur} verwijderen?",
                "Bevestig verwijdering", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (bevestig != MessageBoxResult.Yes) return;

            try
            {
                await _gegevensService.VerwijderReservatieZachtAsync(GeselecteerdeReservatie.Id);
                await LaadReservaties();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fout bij verwijderen:\n{ex.Message}",
                    "Fout", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ================================================================
        //  TERREINEN – CRUD
        // ================================================================

        [RelayCommand]
        private async Task SlaTerreinenOp()
        {
            try
            {
                await _gegevensService.SlaTerreinenOpAsync(Terreinen);
                await LaadData();
                MessageBox.Show("Terreinen opgeslagen.", "Succes",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Fout", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private void NieuwTerrein()
        {
            var terrein = new Terrein { Naam = "Nieuw terrein", Capaciteit = 4, Uurtarief = 15m };
            Terreinen.Add(terrein);
            GeselecteerdTerrein = terrein;
        }

        [RelayCommand]
        private async Task VerwijderTerrein()
        {
            if (GeselecteerdTerrein == null) return;
            var bev = MessageBox.Show(
                $"Terrein '{GeselecteerdTerrein.Naam}' verwijderen?",
                "Bevestig", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (bev != MessageBoxResult.Yes) return;
            try
            {
                await _gegevensService.VerwijderTerreinAsync(GeselecteerdTerrein.Id);
                await LaadData();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Fout", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ================================================================
        //  MATERIALEN – CRUD
        // ================================================================

        [RelayCommand]
        private async Task SlaMaterialenOp()
        {
            try
            {
                await _gegevensService.SlaMaterialenOpAsync(MaterialenLijst);
                await LaadData();
                MessageBox.Show("Materialen opgeslagen.", "Succes",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Fout", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private void NieuwMateriaal()
        {
            var mat = new Materiaal { Naam = "Nieuw materiaal", AantalInInventaris = 1, Huurprijs = 2.50m, IsActief = true };
            MaterialenLijst.Add(mat);
            GeselecteerdMateriaal = mat;
        }

        [RelayCommand]
        private async Task VerwijderMateriaal()
        {
            if (GeselecteerdMateriaal == null) return;
            var bev = MessageBox.Show(
                $"Materiaal '{GeselecteerdMateriaal.Naam}' verwijderen?",
                "Bevestig", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (bev != MessageBoxResult.Yes) return;
            try
            {
                await _gegevensService.VerwijderMateriaalAsync(GeselecteerdMateriaal.Id);
                await LaadData();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Fout", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ================================================================
        //  GEBRUIKERSBEHEER (Admin)
        // ================================================================

        [RelayCommand]
        private async Task MaakAdmin()
        {
            if (GeselecteerdeGebruiker == null) return;
            try
            {
                await _authenticatieService.VoegRolToeAsync(GeselecteerdeGebruiker, "Admin");
                await LaadData();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Fout", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private async Task VerwijderAdmin()
        {
            if (GeselecteerdeGebruiker == null) return;
            try
            {
                await _authenticatieService.VerwijderRolAsync(GeselecteerdeGebruiker, "Admin");
                await LaadData();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Fout", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private async Task BlokkeerGebruiker()
        {
            if (GeselecteerdeGebruiker == null) return;
            try
            {
                await _authenticatieService.StelGeblokkeerdAsync(GeselecteerdeGebruiker, true);
                await LaadData();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Fout", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private async Task DeblokkeerGebruiker()
        {
            if (GeselecteerdeGebruiker == null) return;
            try
            {
                await _authenticatieService.StelGeblokkeerdAsync(GeselecteerdeGebruiker, false);
                await LaadData();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Fout", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ================================================================
        //  UITLOGGEN
        // ================================================================

        [RelayCommand]
        private void MeldAf()
        {
            _authenticatieService.MeldAf();
            var aanmeldenVenster = App.GetService<AanmeldenVenster>();
            aanmeldenVenster.Show();
            foreach (Window w in Application.Current.Windows.Cast<Window>().ToList())
                if (w != aanmeldenVenster) w.Close();
        }
    }
}
