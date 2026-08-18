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
    public partial class MainViewModel : ObservableObject
    {
        private readonly DataService _dataService;
        private readonly AuthService _authService;

        public ObservableCollection<Terrein> Courts { get; } = new();
        public ObservableCollection<Materiaal> EquipmentList { get; } = new();
        public ObservableCollection<Reservation> Reservations { get; } = new();
        public ObservableCollection<Reservation> MyReservations { get; } = new();
        public ObservableCollection<AppUser> Users { get; } = new();

        [ObservableProperty] private Terrein? selectedCourt;
        [ObservableProperty] private Materiaal? selectedEquipment;
        [ObservableProperty] private AppUser? selectedUser;
        [ObservableProperty] private Reservation? selectedReservation;
        [ObservableProperty] private Reservation? selectedMyReservation;

        [ObservableProperty] private DateTime selectedDate = DateTime.Today;

        public bool IsAdmin => _authService.IsAdmin;
        public string WelkomTekst => _authService.CurrentUser != null
            ? $"Welkom, {_authService.CurrentUser.Voornaam} {_authService.CurrentUser.Achternaam}"
            : "Welkom";

        public MainViewModel(DataService dataService, AuthService authService)
        {
            _dataService = dataService;
            _authService = authService;
        }

        // ================================================================
        //  DATA LADEN
        // ================================================================

        [RelayCommand]
        public async Task LoadData()
        {
            try
            {
                Courts.Clear();
                foreach (var t in await _dataService.GetTerreinen())
                    Courts.Add(t);

                EquipmentList.Clear();
                foreach (var m in await _dataService.GetMaterialen())
                    EquipmentList.Add(m);

                await LoadReservations();

                if (_authService.IsAdmin)
                {
                    Users.Clear();
                    foreach (var u in await _authService.GetAllUsersAsync())
                        Users.Add(u);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fout bij laden van gegevens:\n{ex.Message}",
                    "Laadifout", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        public async Task LoadReservations()
        {
            try
            {
                Reservations.Clear();
                MyReservations.Clear();

                // 1. "Reservaties" tab:
                // - Admin ziet ALLE reservaties van alle klanten voor de geselecteerde datum.
                // - Klant ziet ENKEL zijn eigen reservaties.
                List<Reservation> lijst;
                if (_authService.IsAdmin)
                {
                    lijst = await _dataService.GetReservaties(SelectedDate);
                }
                else if (_authService.CurrentUser != null)
                {
                    var alleOpDatum = await _dataService.GetReservaties(SelectedDate);
                    lijst = alleOpDatum.Where(r => r.UserId == _authService.CurrentUser.Id).ToList();
                }
                else
                {
                    lijst = new List<Reservation>();
                }

                foreach (var r in lijst)
                    Reservations.Add(r);

                // 2. "Mijn Account" tab: toont ENKEL de eigen reservaties van de ingelogde gebruiker (Admin of Klant)
                if (_authService.CurrentUser != null)
                {
                    var mijnLijst = await _dataService.GetReservatiesVanGebruiker(_authService.CurrentUser.Id);
                    foreach (var r in mijnLijst)
                        MyReservations.Add(r);
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
        public async Task NewReservation()
        {
            if (_authService.CurrentUser == null)
            {
                MessageBox.Show("Je moet ingelogd zijn om een reservatie te maken.",
                    "Niet aangemeld", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var dialog = App.GetService<ReservationDialog>();
            if (dialog.DataContext is ReservationDialogViewModel vm)
                await vm.InitializeAsync(SelectedDate, null);

            dialog.Owner = Application.Current.MainWindow;
            if (dialog.ShowDialog() == true)
                await LoadReservations();
        }

        [RelayCommand]
        public async Task EditReservation()
        {
            if (SelectedReservation == null) return;

            var dialog = App.GetService<ReservationDialog>();
            if (dialog.DataContext is ReservationDialogViewModel vm)
                await vm.InitializeAsync(SelectedReservation.Datum, SelectedReservation);

            dialog.Owner = Application.Current.MainWindow;
            if (dialog.ShowDialog() == true)
                await LoadReservations();
        }

        [RelayCommand]
        public async Task DeleteReservation()
        {
            if (SelectedReservation == null) return;

            var bevestig = MessageBox.Show(
                $"Wilt u de reservatie voor {SelectedReservation.Terrein?.Naam} op " +
                $"{SelectedReservation.Datum:dd/MM/yyyy} om {SelectedReservation.StartUur} verwijderen?",
                "Bevestig verwijdering", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (bevestig != MessageBoxResult.Yes) return;

            try
            {
                await _dataService.SoftDeleteReservatieAsync(SelectedReservation.Id);
                await LoadReservations();
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
        private async Task SaveCourt()
        {
            try
            {
                await _dataService.SaveCourtsAsync(Courts);
                await LoadData();
                MessageBox.Show("Terreinen opgeslagen.", "Succes",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Fout", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private void NewCourt()
        {
            var terrein = new Terrein { Naam = "Nieuw terrein", Capaciteit = 4, Uurtarief = 15m };
            Courts.Add(terrein);
            SelectedCourt = terrein;
        }

        [RelayCommand]
        private async Task DeleteCourt()
        {
            if (SelectedCourt == null) return;
            var bev = MessageBox.Show(
                $"Terrein '{SelectedCourt.Naam}' verwijderen?",
                "Bevestig", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (bev != MessageBoxResult.Yes) return;
            try
            {
                await _dataService.VerwijderTerreinAsync(SelectedCourt.Id);
                await LoadData();
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
        private async Task SaveEquipment()
        {
            try
            {
                await _dataService.SaveEquipmentAsync(EquipmentList);
                await LoadData();
                MessageBox.Show("Materialen opgeslagen.", "Succes",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Fout", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private void NewEquipment()
        {
            var mat = new Materiaal { Naam = "Nieuw materiaal", AantalInInventaris = 1, Huurprijs = 2.50m, IsActief = true };
            EquipmentList.Add(mat);
            SelectedEquipment = mat;
        }

        [RelayCommand]
        private async Task DeleteEquipment()
        {
            if (SelectedEquipment == null) return;
            var bev = MessageBox.Show(
                $"Materiaal '{SelectedEquipment.Naam}' verwijderen?",
                "Bevestig", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (bev != MessageBoxResult.Yes) return;
            try
            {
                await _dataService.VerwijderMateriaalAsync(SelectedEquipment.Id);
                await LoadData();
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
        private async Task MakeAdmin()
        {
            if (SelectedUser == null) return;
            try
            {
                await _authService.AddRoleAsync(SelectedUser, "Admin");
                await LoadData();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Fout", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private async Task RemoveAdmin()
        {
            if (SelectedUser == null) return;
            try
            {
                await _authService.RemoveRoleAsync(SelectedUser, "Admin");
                await LoadData();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Fout", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private async Task BlockUser()
        {
            if (SelectedUser == null) return;
            try
            {
                await _authService.SetBlockedAsync(SelectedUser, true);
                await LoadData();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Fout", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private async Task UnblockUser()
        {
            if (SelectedUser == null) return;
            try
            {
                await _authService.SetBlockedAsync(SelectedUser, false);
                await LoadData();
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
        private void Logout()
        {
            _authService.Logout();
            var login = App.GetService<LoginWindow>();
            login.Show();
            foreach (Window w in Application.Current.Windows.Cast<Window>().ToList())
                if (w != login) w.Close();
        }
    }
}
