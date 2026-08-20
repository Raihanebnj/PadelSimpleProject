using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PadelSimple.Mobile.Services;
using PadelSimple.Models.Dtos;
using System.Collections.ObjectModel;

namespace PadelSimple.Mobile.ViewModels;

public partial class NieuweReservatieVm : BasisVm
{
    private readonly ReservatiesService _reservaties;
    private readonly TerreinenService _terreinen;
    private readonly MateriaalService _materiaal;
    private readonly LokaleDb _localDb;
    private readonly SynchronisatieService _sync;

    private readonly AuthService _auth;

    public ObservableCollection<TerreinDto> Terreinen { get; } = new();
    public ObservableCollection<MateriaalDto> Materiaal { get; } = new();

    [ObservableProperty] private DateTime selectedDate = DateTime.Today;
    [ObservableProperty] private TerreinDto? selectedTerrein;
    [ObservableProperty] private MateriaalDto? selectedMateriaal;

    [ObservableProperty] private string startUur = "18:00";
    [ObservableProperty] private string eindUur = "19:00";

    [ObservableProperty] private int aantalSpelers = 4;
    [ObservableProperty] private int materiaalAantal = 1;

    public string? UserEmail => _auth.Email;
    public bool IsLoggedIn => _auth.IsLoggedIn;

    public NieuweReservatieVm(
        ReservatiesService reservaties,
        TerreinenService terreinen,
        MateriaalService materiaal,
        LokaleDb localDb,
        SynchronisatieService sync,
        AuthService auth)
    {
        _reservaties = reservaties;
        _terreinen = terreinen;
        _materiaal = materiaal;
        _localDb = localDb;
        _sync = sync;
        _auth = auth;
        _auth.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(AuthService.Email) || e.PropertyName == nameof(AuthService.IsLoggedIn))
            {
                OnPropertyChanged(nameof(UserEmail));
                OnPropertyChanged(nameof(IsLoggedIn));
            }
        };
    }

    [RelayCommand]
    public async Task LogoutAsync()
    {
        await _auth.LogoutAsync();
        await Shell.Current.GoToAsync("//login");
    }

    [RelayCommand]
    public async Task InitAsync()
    {
        if (Terreinen.Count == 0)
        {
            try
            {
                var cs = await _terreinen.GetTerreinenAsync() ?? new List<TerreinDto>();
                Terreinen.Clear();
                foreach (var c in cs.OrderBy(x => x.Naam)) Terreinen.Add(c);
                SelectedTerrein = Terreinen.FirstOrDefault();
            }
            catch (Exception ex)
            {
                Error = ex.Message;
            }
        }

        if (Materiaal.Count == 0)
        {
            try
            {
                var eq = await _materiaal.GetMateriaalAsync() ?? new List<MateriaalDto>();
                Materiaal.Clear();
                Materiaal.Add(new MateriaalDto(0, "(Geen)", 0, 0, true));
                foreach (var e in eq.OrderBy(x => x.Naam)) Materiaal.Add(e);
                SelectedMateriaal = Materiaal.FirstOrDefault();
            }
            catch (Exception ex)
            {
                Error = ex.Message;
            }
        }
    }

    [RelayCommand]
    public async Task CreateAsync()
    {
        Error = null;
        Info = null;

        if (SelectedTerrein == null)
        {
            Error = "Selecteer een terrein.";
            return;
        }

        if (!TimeSpan.TryParse(StartUur, out var st) || !TimeSpan.TryParse(EindUur, out var et) || st >= et)
        {
            Error = "Start/Einde tijd ongeldig.";
            return;
        }

        int? materiaalId = null;
        int? materiaalQty = null;

        if (SelectedMateriaal != null && SelectedMateriaal.Id != 0)
        {
            materiaalId = SelectedMateriaal.Id;
            materiaalQty = Math.Max(1, MateriaalAantal);
        }

        var dto = new ReservatieCreateDto(
            SelectedTerrein.Id,
            SelectedDate.Date,
            st,
            et,
            Math.Max(1, AantalSpelers),
            materiaalId,
            materiaalQty
        );

        if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
        {
            try
            {
                var created = await _reservaties.CreateReservatieAsync(dto);
                if (created != null)
                {
                    Info = "Reservatie succesvol aangemaakt!";
                    await Task.Delay(1000);
                    // Navigate back to overview page
                    await Shell.Current.GoToAsync("..");
                    return;
                }

                Error = "Reservatie kon niet aangemaakt worden.";
            }
            catch (Exception ex)
            {
                Error = ex.Message;
            }
        }
        else
        {
            await _localDb.InsertPendingAsync(new LokaleReservatieInWacht
            {
                TerreinId = dto.TerreinId,
                Datum = dto.Datum,
                StartUur = dto.StartUur.ToString(@"hh\:mm"),
                EindUur = dto.EindUur.ToString(@"hh\:mm"),
                AantalSpelers = dto.AantalSpelers,
                MateriaalId = dto.MateriaalId,
                MateriaalAantal = dto.MateriaalAantal
            });

            Info = "Offline: reservatie bewaard en wordt later gesynchroniseerd.";
            await Task.Delay(1200);
            await Shell.Current.GoToAsync("..");
        }

        await _sync.TrySyncAsync();
    }
}
