using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PadelSimple.Mobile.Services;
using PadelSimple.Models.Dtos;
using System.Collections.ObjectModel;

namespace PadelSimple.Mobile.ViewModels;

public partial class NewReservationVm : BaseVm
{
    private readonly ReservationsService _reservations;
    private readonly CourtsService _courts;
    private readonly EquipmentService _equipment;
    private readonly LocalDb _localDb;
    private readonly SyncService _sync;

    private readonly AuthService _auth;

    public ObservableCollection<CourtDto> Courts { get; } = new();
    public ObservableCollection<EquipmentDto> Equipment { get; } = new();

    [ObservableProperty] private DateTime selectedDate = DateTime.Today;
    [ObservableProperty] private CourtDto? selectedCourt;
    [ObservableProperty] private EquipmentDto? selectedEquipment;

    [ObservableProperty] private string startTime = "18:00";
    [ObservableProperty] private string endTime = "19:00";

    [ObservableProperty] private int numberOfPlayers = 4;
    [ObservableProperty] private int equipmentQuantity = 1;

    public string? UserEmail => _auth.Email;
    public bool IsLoggedIn => _auth.IsLoggedIn;

    public NewReservationVm(
        ReservationsService reservations,
        CourtsService courts,
        EquipmentService equipment,
        LocalDb localDb,
        SyncService sync,
        AuthService auth)
    {
        _reservations = reservations;
        _courts = courts;
        _equipment = equipment;
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
        if (Courts.Count == 0)
        {
            try
            {
                var cs = await _courts.GetCourtsAsync() ?? new List<CourtDto>();
                Courts.Clear();
                foreach (var c in cs.OrderBy(x => x.Name)) Courts.Add(c);
                SelectedCourt = Courts.FirstOrDefault();
            }
            catch (Exception ex)
            {
                Error = ex.Message;
            }
        }

        if (Equipment.Count == 0)
        {
            try
            {
                var eq = await _equipment.GetEquipmentAsync() ?? new List<EquipmentDto>();
                Equipment.Clear();
                Equipment.Add(new EquipmentDto(0, "(Geen)", 0, 0, true));
                foreach (var e in eq.OrderBy(x => x.Name)) Equipment.Add(e);
                SelectedEquipment = Equipment.FirstOrDefault();
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

        if (SelectedCourt == null)
        {
            Error = "Selecteer een terrein.";
            return;
        }

        if (!TimeSpan.TryParse(StartTime, out var st) || !TimeSpan.TryParse(EndTime, out var et) || st >= et)
        {
            Error = "Start/Einde tijd ongeldig.";
            return;
        }

        int? equipmentId = null;
        int? equipmentQty = null;

        if (SelectedEquipment != null && SelectedEquipment.Id != 0)
        {
            equipmentId = SelectedEquipment.Id;
            equipmentQty = Math.Max(1, EquipmentQuantity);
        }

        var dto = new ReservationCreateDto(
            SelectedCourt.Id,
            SelectedDate.Date,
            st,
            et,
            Math.Max(1, NumberOfPlayers),
            equipmentId,
            equipmentQty
        );

        if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
        {
            try
            {
                var created = await _reservations.CreateReservationAsync(dto);
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
            await _localDb.InsertPendingAsync(new LocalReservationPending
            {
                CourtId = dto.CourtId,
                Date = dto.Date,
                StartTime = dto.StartTime.ToString(@"hh\:mm"),
                EndTime = dto.EndTime.ToString(@"hh\:mm"),
                NumberOfPlayers = dto.NumberOfPlayers,
                EquipmentId = dto.EquipmentId,
                EquipmentQuantity = dto.EquipmentQuantity
            });

            Info = "Offline: reservatie bewaard en wordt later gesynchroniseerd.";
            await Task.Delay(1200);
            await Shell.Current.GoToAsync("..");
        }

        await _sync.TrySyncAsync();
    }
}
