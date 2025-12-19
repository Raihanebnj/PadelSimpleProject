using PadelSimple.Models.Dtos;

namespace PadelSimple.Mobile.Services;

public class CourtsService
{
    private readonly ApiClient _api;
    public CourtsService(ApiClient api) => _api = api;

    public Task<List<CourtDto>?> GetCourtsAsync()
        => _api.GetAsync<List<CourtDto>>("/api/courts");
}
