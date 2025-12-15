using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PadelSimple.Desktop.Services;
using PadelSimple.Models.Domain;

namespace PadelSimple.Desktop.ViewModels
{
    public partial class ReservationDialogViewModel : ObservableObject
    {
        private readonly DataService _dataService;
        private readonly AuthService _authService;

        // ====== Lijsten voor ComboBoxen ======
        public ObservableCollection<Court> Courts { get; } = new();

        // Hoofdnaam: Equipment
        public ObservableCollection<Equipment> Equipment { get; } = new();

        // Alias zodat zowel Equipment als EquipmentList in XAML kunnen werken
        public ObservableCollection<Equipment> EquipmentList => Equipment;

        // ====== Geselecteerde items ======
        private Court? _selectedCourt;
        public Court? SelectedCourt
        {
            get => _selectedCourt;
            set => SetProperty(ref _selectedCourt, value);
        }

        private Equipment? _selectedEquipment;
        public Equipment? SelectedEquipment
        {
            get => _selectedEquipment;
            set => SetProperty(ref _selectedEquipment, value);
        }

        // ====== Velden van de reservatie ======
        private DateTime _date = DateTime.Today;
        public DateTime Date
        {
            get => _date;
            set => SetProperty(ref _date, value);
        }

        // Deze twee worden in XAML als string gebruikt (bv "18:00")
        private string _startTimeString = string.Empty;
        public string StartTimeString
        {
            get => _startTimeString;
            set => SetProperty(ref _startTimeString, value);
        }

        private string _endTimeString = string.Empty;
        public string EndTimeString
        {
            get => _endTimeString;
            set => SetProperty(ref _endTimeString, value);
        }

        private int _numberOfPlayers = 2;
        public int NumberOfPlayers
        {
            get => _numberOfPlayers;
            set => SetProperty(ref _numberOfPlayers, value);
        }

        private int _equipmentQuantity = 0;
        public int EquipmentQuantity
        {
            get => _equipmentQuantity;
            set => SetProperty(ref _equipmentQuantity, value);
        }

        // Eventuele foutboodschap (optioneel in XAML binden)
        private string _errorMessage = string.Empty;
        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        public ReservationDialogViewModel(DataService dataService, AuthService authService)
        {
            _dataService = dataService;
            _authService = authService;
        }

        // ====== Wordt aangeroepen door MainViewModel ======
        public async Task InitializeAsync(DateTime initialDate)
        {
            Date = initialDate;

            Courts.Clear();
            foreach (var c in await _dataService.GetCourtsAsync())
                Courts.Add(c);

            Equipment.Clear();
            foreach (var e in await _dataService.GetEquipmentAsync())
                Equipment.Add(e);
        }

        // ====== Commands ======

        [RelayCommand]
        private async Task Save(Window window)
        {
            ErrorMessage = string.Empty;

            if (SelectedCourt == null)
            {
                ErrorMessage = "Kies een terrein.";
                MessageBox.Show(ErrorMessage, "Fout", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!TimeSpan.TryParse(StartTimeString, out var start))
            {
                ErrorMessage = "Starttijd ongeldig (bv. 18:00).";
                MessageBox.Show(ErrorMessage, "Fout", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!TimeSpan.TryParse(EndTimeString, out var end))
            {
                ErrorMessage = "Eindtijd ongeldig (bv. 19:00).";
                MessageBox.Show(ErrorMessage, "Fout", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (_authService.CurrentUser == null)
            {
                ErrorMessage = "Je moet ingelogd zijn om een reservatie te maken.";
                MessageBox.Show(ErrorMessage, "Fout", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var reservation = new Reservation
            {
                CourtId = SelectedCourt.Id,
                EquipmentId = SelectedEquipment?.Id,
                EquipmentQuantity = EquipmentQuantity,
                UserId = _authService.CurrentUser.Id,
                Date = Date,
                StartTime = start,
                EndTime = end,
                NumberOfPlayers = NumberOfPlayers
            };

            try
            {
                await _dataService.CreateReservationAsync(reservation);
                window.DialogResult = true;
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
                MessageBox.Show(ex.Message, "Fout", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private void Cancel(Window window)
        {
            window.DialogResult = false;
        }
    }
}
