using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PadelSimple.Desktop.Services;
using PadelSimple.Models.Domain;
using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows;

namespace PadelSimple.Desktop.ViewModels
{
    /// <summary>
    /// ViewModel voor het dialoogvenster om een reservatie aan te maken of te bewerken.
    /// </summary>
    public partial class ReservationDialogViewModel : ObservableObject
    {
        private readonly DataService _dataService;
        private readonly AuthService _authService;

        // Lijsten voor ComboBoxen
        public ObservableCollection<Terrein> Courts { get; } = new();
        public ObservableCollection<Materiaal> Equipment { get; } = new();
        public ObservableCollection<Materiaal> EquipmentList => Equipment;

        // Geselecteerde items
        [ObservableProperty] private Terrein? selectedCourt;
        [ObservableProperty] private Materiaal? selectedEquipment;

        // Reservatie-velden
        [ObservableProperty] private DateTime date = DateTime.Today;
        [ObservableProperty] private string startTimeString = "10:00";
        [ObservableProperty] private string endTimeString = "11:00";
        [ObservableProperty] private int numberOfPlayers = 2;
        [ObservableProperty] private int equipmentQuantity = 0;
        [ObservableProperty] private string errorMessage = string.Empty;
        [ObservableProperty] private bool isEdit = false;
        [ObservableProperty] private string windowTitle = "Nieuwe Reservatie";
        [ObservableProperty] private decimal geschatPrijs = 0m;

        // De reservatie die we bewerken (null = nieuw)
        private Reservation? _bestaandeReservatie;

        public ReservationDialogViewModel(DataService dataService, AuthService authService)
        {
            _dataService = dataService;
            _authService = authService;
        }

        /// <summary>Laadt courts, materialen en vult de velden in (nieuw of bestaand).</summary>
        public async Task InitializeAsync(DateTime initialDate, Reservation? bestaand = null)
        {
            Date = initialDate;
            _bestaandeReservatie = bestaand;

            Courts.Clear();
            foreach (var t in await _dataService.GetTerreinen())
                Courts.Add(t);

            Equipment.Clear();
            // Voeg een lege optie toe voor "Geen materiaal"
            foreach (var m in await _dataService.GetMaterialen())
                Equipment.Add(m);

            if (bestaand != null)
            {
                IsEdit = true;
                WindowTitle = "Reservatie bewerken";
                Date = bestaand.Datum;
                StartTimeString = bestaand.StartUur.ToString(@"hh\:mm");
                EndTimeString = bestaand.EindUur.ToString(@"hh\:mm");
                NumberOfPlayers = bestaand.AantalSpelers;
                EquipmentQuantity = bestaand.AantalMateriaal;

                SelectedCourt = Courts.FirstOrDefault(t => t.Id == bestaand.TerreinId);
                SelectedEquipment = bestaand.MateriaalId.HasValue
                    ? Equipment.FirstOrDefault(m => m.Id == bestaand.MateriaalId.Value)
                    : null;
            }
            else
            {
                IsEdit = false;
                WindowTitle = "Nieuwe Reservatie";
                if (Courts.Count > 0) SelectedCourt = Courts[0];
            }

            BerekenPrijs();
        }

        partial void OnSelectedCourtChanged(Terrein? value) => BerekenPrijs();
        partial void OnStartTimeStringChanged(string value) => BerekenPrijs();
        partial void OnEndTimeStringChanged(string value) => BerekenPrijs();
        partial void OnSelectedEquipmentChanged(Materiaal? value) => BerekenPrijs();
        partial void OnEquipmentQuantityChanged(int value) => BerekenPrijs();

        private void BerekenPrijs()
        {
            if (SelectedCourt == null) { GeschatPrijs = 0; return; }
            if (!TryParseTime(StartTimeString, out var start) ||
                !TryParseTime(EndTimeString, out var end) ||
                end <= start) { GeschatPrijs = 0; return; }

            var duur = (decimal)(end - start).TotalHours;
            GeschatPrijs = SelectedCourt.Uurtarief * duur
                + (SelectedEquipment?.Huurprijs ?? 0m) * EquipmentQuantity;
        }

        private static bool TryParseTime(string? input, out TimeSpan time)
        {
            time = default;
            var txt = (input ?? string.Empty).Trim()
                .Replace('.', ':')
                .Replace('u', ':')
                .Replace('U', ':');

            if (string.IsNullOrWhiteSpace(txt)) return false;

            var formats = new[] { @"h\:mm", @"hh\:mm", @"h\:mm\:ss", @"hh\:mm\:ss" };
            if (TimeSpan.TryParseExact(txt, formats, CultureInfo.InvariantCulture, out time))
                return true;

            return TimeSpan.TryParse(txt, CultureInfo.InvariantCulture, out time);
        }

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

            if (!TryParseTime(StartTimeString, out var start))
            {
                ErrorMessage = "Starttijd ongeldig. Gebruik bv. 10:00";
                MessageBox.Show(ErrorMessage, "Fout", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!TryParseTime(EndTimeString, out var end))
            {
                ErrorMessage = "Eindtijd ongeldig. Gebruik bv. 11:00";
                MessageBox.Show(ErrorMessage, "Fout", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (end <= start)
            {
                ErrorMessage = "Eindtijd moet na starttijd liggen.";
                MessageBox.Show(ErrorMessage, "Fout", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (_authService.CurrentUser == null)
            {
                ErrorMessage = "Je moet ingelogd zijn.";
                MessageBox.Show(ErrorMessage, "Fout", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                if (IsEdit && _bestaandeReservatie != null)
                {
                    _bestaandeReservatie.TerreinId = SelectedCourt.Id;
                    _bestaandeReservatie.MateriaalId = SelectedEquipment?.Id;
                    _bestaandeReservatie.AantalMateriaal = EquipmentQuantity;
                    _bestaandeReservatie.Datum = Date.Date;
                    _bestaandeReservatie.StartUur = start;
                    _bestaandeReservatie.EindUur = end;
                    _bestaandeReservatie.AantalSpelers = NumberOfPlayers;
                    await _dataService.WijzigReservatieAsync(_bestaandeReservatie);
                }
                else
                {
                    var reservatie = new Reservation
                    {
                        TerreinId = SelectedCourt.Id,
                        MateriaalId = SelectedEquipment?.Id,
                        AantalMateriaal = EquipmentQuantity,
                        UserId = _authService.CurrentUser.Id,
                        Datum = Date.Date,
                        StartUur = start,
                        EindUur = end,
                        AantalSpelers = NumberOfPlayers
                    };
                    await _dataService.MaakReservatieAanAsync(reservatie);
                }

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
