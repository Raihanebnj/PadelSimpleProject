using PadelSimple.Models.Dtos;

namespace PadelSimple.Mobile.Services;

public class SyncService
{
    private readonly LocalDb _localDb;
    private readonly ReservationsService _reservations;

    public SyncService(LocalDb localDb, ReservationsService reservations)
    {
        _localDb = localDb;
        _reservations = reservations;
    }

    public async Task TrySyncAsync()
    {
        if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
            return;

        var pending = await _localDb.GetPendingAsync();

        foreach (var p in pending)
        {
            if (!TimeSpan.TryParse(p.StartTime, out var st)) continue;
            if (!TimeSpan.TryParse(p.EndTime, out var et)) continue;

            var dto = new ReservationCreateDto(
                p.CourtId,
                p.Date.Date,
                st,
                et,
                p.NumberOfPlayers,
                p.EquipmentId,
                p.EquipmentQuantity
            );

            try
            {
                var created = await _reservations.CreateReservationAsync(dto);
                if (created != null)
                    await _localDb.DeletePendingAsync(p.Id);
            }
            catch
            {
                // keep pending (bv overlap of auth)
            }
        }
    }
}
