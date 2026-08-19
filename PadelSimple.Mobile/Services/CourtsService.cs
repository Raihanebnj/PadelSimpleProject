using PadelSimple.Models.Dtos;

namespace PadelSimple.Mobile.Services;

public class CourtsService
{
    private readonly ApiClient _api;
    private readonly LocalDb _localDb;

    public CourtsService(ApiClient api, LocalDb localDb)
    {
        _api = api;
        _localDb = localDb;
    }

    public async Task<List<CourtDto>> GetCourtsAsync()
    {
        if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
        {
            try
            {
                var data = await _api.GetAsync<List<CourtDto>>("/api/terreinen");
                if (data != null)
                {
                    await _localDb.ReplaceCourtsAsync(data);
                    return data;
                }
            }
            catch
            {
            }
        }

        return await _localDb.GetCachedCourtsAsync();
    }
}
