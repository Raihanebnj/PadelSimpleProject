using PadelSimple.Models.Dtos;

namespace PadelSimple.Mobile.Services;

public class EquipmentService
{
    private readonly ApiClient _api;
    private readonly LocalDb _localDb;

    public EquipmentService(ApiClient api, LocalDb localDb)
    {
        _api = api;
        _localDb = localDb;
    }

    public async Task<List<EquipmentDto>> GetEquipmentAsync()
    {
        if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
        {
            try
            {
                var data = await _api.GetAsync<List<EquipmentDto>>("/api/materiaal");
                if (data != null)
                {
                    await _localDb.ReplaceEquipmentAsync(data);
                    return data;
                }
            }
            catch
            {
            }
        }

        return await _localDb.GetCachedEquipmentAsync();
    }
}
