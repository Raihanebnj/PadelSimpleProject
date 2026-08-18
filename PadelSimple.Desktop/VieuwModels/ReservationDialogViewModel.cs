using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PadelSimple.Desktop.Services;
using PadelSimple.Models.Domain;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace PadelSimple.Desktop.ViewModels
{
    /// <summary>
    /// Wrapper ViewModel voor een enkel materiaalitem in het selectieoverzicht.
    /// </summary>
    public partial class EquipmentSelectionItemViewModel : ObservableObject
    {
        public Materiaal Materiaal { get; set; } = null!;
        public int Id => Materiaal.Id;
        public string Naam => Materiaal.Naam;
        public decimal Huurprijs => Materiaal.Huurprijs;
        public int AantalInInventaris => Materiaal.AantalInInventaris;

        [ObservableProperty] private int aantal = 0;
    }

    /// <summary>
    /// ViewModel voor het dialoogvenster om een reservatie aan te maken of te bewerken.
    /// Ondersteunt het selecteren van meerdere materialen en berekent de prijs dynamisch.
    /// </summary>
    public partial class ReservationDialogViewModel : ObservableObject
    {
        private readonly DataService _dataService;
        private readonly AuthService _authService;

        // Lijsten
        public ObservableCollection<Terrein> Courts { get; } = new();
        public ObservableCollection<EquipmentSelectionItemViewModel> EquipmentItems { get; } = new();

        // Geselecteerde terrein
        [ObservableProperty] private Terrein? selectedCourt;

        // Reservatie-velden
        [ObservableProperty] private DateTime date = DateTime.Today;
        [ObservableProperty] private string startTimeString = "10:00";
        [ObservableProperty] private string endTimeString = "11:00";
        [ObservableProperty] private int numberOfPlayers = 2;
        [ObservableProperty] private string errorMessage = string.Empty;
        [ObservableProperty] private bool isEdit = false;
        [ObservableProperty] private string windowTitle = "Nieuwe Reservatie";
        [ObservableProperty] private decimal geschatPrijs = 0m;

        private Reservation? _bestaandeReservatie;

        public ReservationDialogViewModel(DataService dataService, AuthService authService)
        {
            _dataService = dataService;
            _authService = authService;
        }

        public async Task InitializeAsync(DateTime initialDate, Reservation? bestaand = null)
        {
            Date = initialDate;
            _bestaandeReservatie = bestaand;
            ErrorMessage = string.Empty;

            Courts.Clear();
            foreach (var t in await _dataService.GetTerreinen())
                Courts.Add(t);

            EquipmentItems.Clear();
            var materialen = await _dataService.GetMaterialen();
            foreach (var m in materialen)
            {
                var item = new EquipmentSelectionItemViewModel { Materiaal = m };

                if (bestaand != null)
                {
                    var bestaandMat = bestaand.ReservationMaterialen?.FirstOrDefault(rm => rm.MateriaalId == m.Id);
                    if (bestaandMat != null)
                    {
                        item.Aantal = bestaandMat.Aantal;
                    }
                    else if (bestaand.MateriaalId == m.Id)
                    {
                        item.Aantal = bestaand.AantalMateriaal;
                    }
                }

                item.PropertyChanged += (_, _) => BerekenPrijs();
                EquipmentItems.Add(item);
            }

            if (bestaand != null)
            {
                IsEdit = true;
                WindowTitle = "Reservatie bewerken";
                Date = bestaand.Datum;
                StartTimeString = bestaand.StartUur.ToString(@"hh\:mm");
                EndTimeString = bestaand.EindUur.ToString(@"hh\:mm");
                NumberOfPlayers = bestaand.AantalSpelers;

                SelectedCourt = Courts.FirstOrDefault(t => t.Id == bestaand.TerreinId);
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

        private void BerekenPrijs()
        {
            if (SelectedCourt == null) { GeschatPrijs = 0; return; }
            if (!TryParseTime(StartTimeString, out var start) ||
                !TryParseTime(EndTimeString, out var end) ||
                end <= start) { GeschatPrijs = 0; return; }

            var duur = (decimal)(end - start).TotalHours;
            decimal totaal = SelectedCourt.Uurtarief * duur;

            foreach (var item in EquipmentItems)
            {
                if (item.Aantal > 0)
                    totaal += item.Huurprijs * item.Aantal;
            }

            GeschatPrijs = totaal;
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
        private async Task Save(Window? window)
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

            // Gekozen materialen ophalen
            var gekozenMaterialen = EquipmentItems
                .Where(i => i.Aantal > 0)
                .Select(i => (MateriaalId: i.Id, Aantal: i.Aantal))
                .ToList();

            // Valideer per materiaal dat aantal niet groter is dan totale inventaris
            foreach (var item in EquipmentItems)
            {
                if (item.Aantal > item.AantalInInventaris)
                {
                    ErrorMessage = $"Niet genoeg voorraad van '{item.Naam}'. In inventaris: {item.AantalInInventaris}, gevraagd: {item.Aantal}.";
                    MessageBox.Show(ErrorMessage, "Fout bij materiaalvoorraad", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }

            try
            {
                if (IsEdit && _bestaandeReservatie != null)
                {
                    _bestaandeReservatie.TerreinId = SelectedCourt.Id;
                    _bestaandeReservatie.Datum = Date.Date;
                    _bestaandeReservatie.StartUur = start;
                    _bestaandeReservatie.EindUur = end;
                    _bestaandeReservatie.AantalSpelers = NumberOfPlayers;

                    await _dataService.WijzigReservatieAsync(_bestaandeReservatie, gekozenMaterialen);
                }
                else
                {
                    var reservatie = new Reservation
                    {
                        TerreinId = SelectedCourt.Id,
                        UserId = _authService.CurrentUser.Id,
                        Datum = Date.Date,
                        StartUur = start,
                        EindUur = end,
                        AantalSpelers = NumberOfPlayers
                    };
                    await _dataService.MaakReservatieAanAsync(reservatie, gekozenMaterialen);
                }

                if (window != null)
                {
                    window.DialogResult = true;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
                MessageBox.Show(ex.Message, "Fout bij reservatie", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private void Cancel(Window? window)
        {
            if (window != null)
            {
                window.DialogResult = false;
            }
        }
    }
}
