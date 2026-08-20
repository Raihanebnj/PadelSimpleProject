using PadelSimple.Models.Dtos;

namespace PadelSimple.Mobile.Services;

public class SynchronisatieService
{
    private readonly LokaleDb _localDb;
    private readonly ReservatiesService _reservaties;

    public SynchronisatieService(LokaleDb localDb, ReservatiesService reservaties)
    {
        _localDb = localDb;
        _reservaties = reservaties;
    }

    public async Task TrySyncAsync()
    {
        if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
            return;

        var pending = await _localDb.GetPendingAsync();

        foreach (var p in pending)
        {
            if (!TimeSpan.TryParse(p.StartUur, out var st)) continue;
            if (!TimeSpan.TryParse(p.EindUur, out var et)) continue;

            var dto = new ReservatieCreateDto(
                p.TerreinId,
                p.Datum.Date,
                st,
                et,
                p.AantalSpelers,
                p.MateriaalId,
                p.MateriaalAantal
            );

            try
            {
                var created = await _reservaties.CreateReservatieAsync(dto);
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
