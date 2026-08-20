using PadelSimple.Models.Dtos;

namespace PadelSimple.Mobile.Services;

public class ReservatiesService
{
    private readonly ApiKlant _api;
    private readonly LokaleDb _localDb;

    public ReservatiesService(ApiKlant api, LokaleDb localDb)
    {
        _api = api;
        _localDb = localDb;
    }

    public async Task<List<ReservatieDto>> GetReservatiesAsync(DateTime date)
    {
        if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
        {
            try
            {
                var data = await _api.GetAsync<List<ReservatieDto>>($"/api/reservaties?datum={date:yyyy-MM-dd}");
                if (data != null)
                {
                    await _localDb.ReplaceReservatiesAsync(date, data);
                    return data;
                }
            }
            catch
            {
            }
        }

        return await _localDb.GetGecachteReservatiesAsync(date);
    }

    public Task<ReservatieDto?> CreateReservatieAsync(ReservatieCreateDto dto)
        => _api.PostAsync<ReservatieCreateDto, ReservatieDto>("/api/reservaties", dto);
}
