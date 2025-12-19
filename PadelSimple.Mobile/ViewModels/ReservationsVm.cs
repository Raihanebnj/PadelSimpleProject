using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PadelSimple.Mobile.Services;
using PadelSimple.Models.Dtos;
using System.Collections.ObjectModel;

namespace PadelSimple.Mobile.ViewModels;

public partial class ReservationsVm : BaseVm
{
    private readonly ReservationsService _reservations;
    private readonly CourtsService _courts;
    private readonly EquipmentService _equipment;
    private readonly LocalDb _localDb;
    private readonly SyncService _sync;

    public ObservableCollection<ReservationDto> Items { get; } = new();
    public ObservableCollection<CourtDto> Courts { get; } = new();
    public ObservableCollection<EquipmentDto> Equipment { get; } = new();

    [ObservableProperty] private DateTime selectedDate = DateTime.Today;

    [ObservableProperty] private CourtDto? selectedCourt;
    [ObservableProperty] private EquipmentDto? selectedEquipment;

    [ObservableProperty] private string startTime = "18:00";
    [ObservableProperty] private string endTime = "19:00";

    [ObservableProperty] private int numberOfPlayers = 4;
    [ObservableProperty] private int equipmentQuantity = 1;

    public ReservationsVm(
        ReservationsService reservations,
        CourtsService courts,
        EquipmentService equipment,
        LocalDb localDb,
        SyncService sync)
    {
        _reservations = reservations;
        _courts = courts;
        _equipment = equipment;
        _localDb = localDb;
        _sync = sync;
    }

    [RelayCommand]
    public async Task InitAsync()
    {
        // laad dropdown data 1x
        if (Courts.Count == 0)
        {
            var cs = await _courts.GetCourtsAsync() ?? new List<CourtDto>();
            Courts.Clear();
            foreach (var c in cs.OrderBy(x => x.Name)) Courts.Add(c);
            SelectedCourt = Courts.FirstOrDefault();
        }

        if (Equipment.Count == 0)
        {
            var eq = await _equipment.GetEquipmentAsync() ?? new List<EquipmentDto>();
            Equipment.Clear();
            Equipment.Add(new EquipmentDto(0, "(Geen)", 0, 0, true));
            foreach (var e in eq.OrderBy(x => x.Name)) Equipment.Add(e);
            SelectedEquipment = Equipment.FirstOrDefault();
        }

        await LoadAsync();
    }

    [RelayCommand]
    public async Task LoadAsync()
    {
        if (IsBusy) return;
        IsBusy = true;
        Error = null;
        Info = null;

        try
        {
            Items.Clear();
            var data = await _reservations.GetReservationsAsync(SelectedDate) ?? new List<ReservationDto>();
            foreach (var r in data.OrderBy(x => x.StartTime))
                Items.Add(r);
        }
        catch (Exception ex)
        {
            Error = ex.Message;
        }
        finally
        {
            IsBusy = false;
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

        // online?
        if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
        {
            try
            {
                var created = await _reservations.CreateReservationAsync(dto);
                if (created != null)
                {
                    Info = "Reservatie aangemaakt.";
                    await LoadAsync();
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
            // offline queue
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

            Info = "Offline: reservatie bewaard en later gesynchroniseerd.";
        }

        // probeer sync (als net terug online)
        await _sync.TrySyncAsync();
    }
}
