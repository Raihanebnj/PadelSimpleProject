using PadelSimple.Models.Dtos;

namespace PadelSimple.Mobile.Services;

public class ReservationsService
{
    private readonly ApiClient _api;
    public ReservationsService(ApiClient api) => _api = api;

    public Task<List<ReservationDto>?> GetReservationsAsync(DateTime date)
        => _api.GetAsync<List<ReservationDto>>($"/api/reservations?date={date:yyyy-MM-dd}");

    public Task<ReservationDto?> CreateReservationAsync(ReservationCreateDto dto)
        => _api.PostAsync<ReservationCreateDto, ReservationDto>("/api/reservations", dto);
}
