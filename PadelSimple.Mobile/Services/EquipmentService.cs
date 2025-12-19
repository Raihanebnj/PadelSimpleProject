using PadelSimple.Models.Dtos;

namespace PadelSimple.Mobile.Services;

public class EquipmentService
{
    private readonly ApiClient _api;
    public EquipmentService(ApiClient api) => _api = api;

    public Task<List<EquipmentDto>?> GetEquipmentAsync()
        => _api.GetAsync<List<EquipmentDto>>("/api/equipment");
}
