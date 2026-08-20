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
    public partial class MateriaalSelectieItemViewModel : ObservableObject
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
    public partial class ReservatieDialoogViewModel : ObservableObject
    {
        private readonly GegevensService _gegevensService;
        private readonly AuthenticatieService _authenticatieService;

        // Lijsten
        public ObservableCollection<Terrein> Terreinen { get; } = new();
        public ObservableCollection<MateriaalSelectieItemViewModel> MateriaalItems { get; } = new();

        // Geselecteerd terrein
        [ObservableProperty] private Terrein? geselecteerdTerrein;

        // Reservatievelden
        [ObservableProperty] private DateTime datum = DateTime.Today;
        [ObservableProperty] private string startTijdString = "10:00";
        [ObservableProperty] private string eindTijdString = "11:00";
        [ObservableProperty] private int aantalSpelers = 2;
        [ObservableProperty] private string foutBoodschap = string.Empty;
        [ObservableProperty] private bool isBewerken = false;
        [ObservableProperty] private string vensterTitel = "Nieuwe Reservatie";
        [ObservableProperty] private decimal geschatPrijs = 0m;

        private Reservatie? _bestaandeReservatie;

        public ReservatieDialoogViewModel(GegevensService gegevensService, AuthenticatieService authenticatieService)
        {
            _gegevensService = gegevensService;
            _authenticatieService = authenticatieService;
        }

        public async Task InitialiseerAsync(DateTime initiëleDatum, Reservatie? bestaand = null)
        {
            Datum = initiëleDatum;
            _bestaandeReservatie = bestaand;
            FoutBoodschap = string.Empty;

            Terreinen.Clear();
            foreach (var t in await _gegevensService.GetTerreinen())
                Terreinen.Add(t);

            MateriaalItems.Clear();
            var materialen = await _gegevensService.GetMaterialen();
            foreach (var m in materialen)
            {
                var item = new MateriaalSelectieItemViewModel { Materiaal = m };

                if (bestaand != null)
                {
                    var bestaandMat = bestaand.ReservatieMaterialen?.FirstOrDefault(rm => rm.MateriaalId == m.Id);
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
                MateriaalItems.Add(item);
            }

            if (bestaand != null)
            {
                IsBewerken = true;
                VensterTitel = "Reservatie bewerken";
                Datum = bestaand.Datum;
                StartTijdString = bestaand.StartUur.ToString(@"hh\:mm");
                EindTijdString = bestaand.EindUur.ToString(@"hh\:mm");
                AantalSpelers = bestaand.AantalSpelers;

                GeselecteerdTerrein = Terreinen.FirstOrDefault(t => t.Id == bestaand.TerreinId);
            }
            else
            {
                IsBewerken = false;
                VensterTitel = "Nieuwe Reservatie";
                if (Terreinen.Count > 0) GeselecteerdTerrein = Terreinen[0];
            }

            BerekenPrijs();
        }

        partial void OnGeselecteerdTerreinChanged(Terrein? value) => BerekenPrijs();
        partial void OnStartTijdStringChanged(string value) => BerekenPrijs();
        partial void OnEindTijdStringChanged(string value) => BerekenPrijs();

        private void BerekenPrijs()
        {
            if (GeselecteerdTerrein == null) { GeschatPrijs = 0; return; }
            if (!ProbeerTijdTePareren(StartTijdString, out var start) ||
                !ProbeerTijdTePareren(EindTijdString, out var eind) ||
                eind <= start) { GeschatPrijs = 0; return; }

            var duur = (decimal)(eind - start).TotalHours;
            decimal totaal = GeselecteerdTerrein.Uurtarief * duur;

            foreach (var item in MateriaalItems)
            {
                if (item.Aantal > 0)
                    totaal += item.Huurprijs * item.Aantal;
            }

            GeschatPrijs = totaal;
        }

        private static bool ProbeerTijdTePareren(string? invoer, out TimeSpan tijd)
        {
            tijd = default;
            var txt = (invoer ?? string.Empty).Trim()
                .Replace('.', ':')
                .Replace('u', ':')
                .Replace('U', ':');

            if (string.IsNullOrWhiteSpace(txt)) return false;

            var formats = new[] { @"h\:mm", @"hh\:mm", @"h\:mm\:ss", @"hh\:mm\:ss" };
            if (TimeSpan.TryParseExact(txt, formats, CultureInfo.InvariantCulture, out tijd))
                return true;

            return TimeSpan.TryParse(txt, CultureInfo.InvariantCulture, out tijd);
        }

        [RelayCommand]
        private async Task Opslaan(Window? venster)
        {
            FoutBoodschap = string.Empty;

            if (GeselecteerdTerrein == null)
            {
                FoutBoodschap = "Kies een terrein.";
                MessageBox.Show(FoutBoodschap, "Fout", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!ProbeerTijdTePareren(StartTijdString, out var start))
            {
                FoutBoodschap = "Starttijd ongeldig. Gebruik bv. 10:00";
                MessageBox.Show(FoutBoodschap, "Fout", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!ProbeerTijdTePareren(EindTijdString, out var eind))
            {
                FoutBoodschap = "Eindtijd ongeldig. Gebruik bv. 11:00";
                MessageBox.Show(FoutBoodschap, "Fout", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (eind <= start)
            {
                FoutBoodschap = "Eindtijd moet na starttijd liggen.";
                MessageBox.Show(FoutBoodschap, "Fout", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (_authenticatieService.HuidigeGebruiker == null)
            {
                FoutBoodschap = "Je moet ingelogd zijn.";
                MessageBox.Show(FoutBoodschap, "Fout", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Gekozen materialen ophalen
            var gekozenMaterialen = MateriaalItems
                .Where(i => i.Aantal > 0)
                .Select(i => (MateriaalId: i.Id, Aantal: i.Aantal))
                .ToList();

            // Valideer per materiaal dat aantal niet groter is dan totale inventaris
            foreach (var item in MateriaalItems)
            {
                if (item.Aantal > item.AantalInInventaris)
                {
                    FoutBoodschap = $"Niet genoeg voorraad van '{item.Naam}'. In inventaris: {item.AantalInInventaris}, gevraagd: {item.Aantal}.";
                    MessageBox.Show(FoutBoodschap, "Fout bij materiaalvoorraad", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }

            try
            {
                if (IsBewerken && _bestaandeReservatie != null)
                {
                    _bestaandeReservatie.TerreinId = GeselecteerdTerrein.Id;
                    _bestaandeReservatie.Datum = Datum.Date;
                    _bestaandeReservatie.StartUur = start;
                    _bestaandeReservatie.EindUur = eind;
                    _bestaandeReservatie.AantalSpelers = AantalSpelers;

                    await _gegevensService.WijzigReservatieAsync(_bestaandeReservatie, gekozenMaterialen);
                }
                else
                {
                    var reservatie = new Reservatie
                    {
                        TerreinId = GeselecteerdTerrein.Id,
                        GebruikerId = _authenticatieService.HuidigeGebruiker.Id,
                        Datum = Datum.Date,
                        StartUur = start,
                        EindUur = eind,
                        AantalSpelers = AantalSpelers
                    };
                    await _gegevensService.MaakReservatieAanAsync(reservatie, gekozenMaterialen);
                }

                if (venster != null)
                {
                    venster.DialogResult = true;
                }
            }
            catch (Exception ex)
            {
                FoutBoodschap = ex.Message;
                MessageBox.Show(ex.Message, "Fout bij reservatie", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private void Annuleren(Window? venster)
        {
            if (venster != null)
            {
                venster.DialogResult = false;
            }
        }
    }
}
