using PadelSimple.Models.Dtos;

namespace PadelSimple.Mobile.Services;

public class TerreinenService
{
    private readonly ApiKlant _api;
    private readonly LokaleDb _localDb;

    public TerreinenService(ApiKlant api, LokaleDb localDb)
    {
        _api = api;
        _localDb = localDb;
    }

    public async Task<List<TerreinDto>> GetTerreinenAsync()
    {
        if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
        {
            try
            {
                var data = await _api.GetAsync<List<TerreinDto>>("/api/terreinen");
                if (data != null)
                {
                    await _localDb.ReplaceTerreinenAsync(data);
                    return data;
                }
            }
            catch
            {
            }
        }

        return await _localDb.GetGecachteTerreinenAsync();
    }
}
