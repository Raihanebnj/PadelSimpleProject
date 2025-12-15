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
    public partial class MainViewModel : ObservableObject
    {
        private readonly DataService _dataService;
        private readonly AuthService _authService;

        public ObservableCollection<Court> Courts { get; } = new();
        public ObservableCollection<Equipment> EquipmentList { get; } = new();
        public ObservableCollection<Reservation> Reservations { get; } = new();
        public ObservableCollection<AppUser> Users { get; } = new();

        [ObservableProperty] private Court? selectedCourt;
        [ObservableProperty] private Equipment? selectedEquipment;
        [ObservableProperty] private AppUser? selectedUser;

        // Datum-filter voor de Overzicht-tab
        [ObservableProperty] private DateTime selectedDate = DateTime.Today;

        public bool IsAdmin => _authService.IsAdmin;

        public MainViewModel(DataService dataService, AuthService authService)
        {
            _dataService = dataService;
            _authService = authService;
        }

        // ======= INIT / LADEN =======

        [RelayCommand]
        public async Task LoadData()
        {
            // Terreinen
            Courts.Clear();
            foreach (var c in await _dataService.GetCourtsAsync())
                Courts.Add(c);

            // Materiaal
            EquipmentList.Clear();
            foreach (var e in await _dataService.GetEquipmentAsync())
                EquipmentList.Add(e);

            // Reservaties (voor geselecteerde datum)
            await LoadReservations();

            // Users (alleen als admin)
            if (_authService.IsAdmin)
            {
                Users.Clear();
                foreach (var u in await _authService.GetAllUsersAsync())
                    Users.Add(u);
            }
        }

        [RelayCommand]
        public async Task LoadReservations()
        {
            Reservations.Clear();
            var list = await _dataService.GetReservationsAsync(SelectedDate);
            foreach (var r in list)
                Reservations.Add(r);
        }

        // ======= NIEUWE RESERVATIE =======

        [RelayCommand]
        public async Task NewReservation()
        {
            if (_authService.CurrentUser == null)
            {
                MessageBox.Show("Je moet ingelogd zijn om een reservatie te maken.",
                    "Niet aangemeld", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Dialog via DI
            var dialog = App.GetService<ReservationDialog>();

            // ViewModel initialiseren: datum + courts + materiaal laden
            if (dialog.DataContext is ReservationDialogViewModel vm)
            {
                await vm.InitializeAsync(SelectedDate);
            }

            dialog.Owner = Application.Current.MainWindow;

            var result = dialog.ShowDialog();
            if (result == true)
            {
                // Na succesvolle reservatie opnieuw laden
                await LoadReservations();
            }
        }

        // ======= UITLOGGEN =======

        [RelayCommand]
        private void Logout()
        {
            _authService.Logout();

            var login = App.GetService<LoginWindow>();
            login.Show();

            // Alle andere vensters sluiten (incl. main)
            foreach (Window w in Application.Current.Windows.Cast<Window>().ToList())
            {
                if (w != login)
                    w.Close();
            }
        }

        // ======= COURT MANAGEMENT =======

        [RelayCommand]
        private async Task SaveCourt()
        {
            try
            {
                // alles opslaan, zodat edits in de grid zeker meegaan
                await _dataService.SaveCourtsAsync(Courts);
                await LoadData();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Fout", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private void NewCourt()
        {
            Courts.Add(new Court { Name = "Nieuw terrein", Capacity = 4 });
            SelectedCourt = Courts.Last();
        }

        // ======= EQUIPMENT MANAGEMENT =======

        [RelayCommand]
        private async Task SaveEquipment()
        {
            try
            {
                // 🔹 alles opslaan zodat grid-edits zeker meegaan
                await _dataService.SaveEquipmentAsync(EquipmentList);
                await LoadData();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Fout", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        [RelayCommand]
        private void NewEquipment()
        {
            EquipmentList.Add(new Equipment
            {
                Name = "Nieuw materiaal",
                TotalQuantity = 1,
                AvailableQuantity = 1,
                IsActive = true
            });
            SelectedEquipment = EquipmentList.Last();
        }

        // ======= USER MANAGEMENT =======

        [RelayCommand]
        private async Task MakeAdmin()
        {
            if (SelectedUser == null) return;
            await _authService.AddRoleAsync(SelectedUser, "Admin");
            await LoadData();
        }

        [RelayCommand]
        private async Task RemoveAdmin()
        {
            if (SelectedUser == null) return;
            await _authService.RemoveRoleAsync(SelectedUser, "Admin");
            await LoadData();
        }

        [RelayCommand]
        private async Task BlockUser()
        {
            if (SelectedUser == null) return;
            await _authService.SetBlockedAsync(SelectedUser, true);
            await LoadData();
        }

        [RelayCommand]
        private async Task UnblockUser()
        {
            if (SelectedUser == null) return;
            await _authService.SetBlockedAsync(SelectedUser, false);
            await LoadData();
        }
    }
}
