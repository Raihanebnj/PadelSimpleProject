using PadelSimple.Models.Dtos;

namespace PadelSimple.Mobile.Services;

public class MateriaalService
{
    private readonly ApiKlant _api;
    private readonly LokaleDb _localDb;

    public MateriaalService(ApiKlant api, LokaleDb localDb)
    {
        _api = api;
        _localDb = localDb;
    }

    public async Task<List<MateriaalDto>> GetMateriaalAsync()
    {
        if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
        {
            try
            {
                var data = await _api.GetAsync<List<MateriaalDto>>("/api/materiaal");
                if (data != null)
                {
                    await _localDb.ReplaceMateriaalAsync(data);
                    return data;
                }
            }
            catch
            {
            }
        }

        return await _localDb.GetGecachtMateriaalAsync();
    }
}
