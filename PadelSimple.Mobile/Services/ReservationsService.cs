using PadelSimple.Models.Dtos;

namespace PadelSimple.Mobile.Services;

public class ReservationsService
{
    private readonly ApiClient _api;
    private readonly LocalDb _localDb;

    public ReservationsService(ApiClient api, LocalDb localDb)
    {
        _api = api;
        _localDb = localDb;
    }

    public async Task<List<ReservationDto>> GetReservationsAsync(DateTime date)
    {
        if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
        {
            try
            {
                var data = await _api.GetAsync<List<ReservationDto>>($"/api/reservaties?date={date:yyyy-MM-dd}");
                if (data != null)
                {
                    await _localDb.ReplaceReservationsAsync(date, data);
                    return data;
                }
            }
            catch
            {
            }
        }

        return await _localDb.GetCachedReservationsAsync(date);
    }

    public Task<ReservationDto?> CreateReservationAsync(ReservationCreateDto dto)
        => _api.PostAsync<ReservationCreateDto, ReservationDto>("/api/reservaties", dto);
}
