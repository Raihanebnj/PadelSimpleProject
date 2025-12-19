using CommunityToolkit.Mvvm.Input;
using PadelSimple.Mobile.Services;
using PadelSimple.Models.Dtos;
using System.Collections.ObjectModel;

namespace PadelSimple.Mobile.ViewModels;

public partial class EquipmentVm : BaseVm
{
    private readonly EquipmentService _equipment;

    public ObservableCollection<EquipmentDto> Items { get; } = new();

    public EquipmentVm(EquipmentService equipment)
    {
        _equipment = equipment;
    }

    [RelayCommand]
    public async Task LoadAsync()
    {
        if (IsBusy) return;
        IsBusy = true;
        Error = null;

        try
        {
            Items.Clear();
            var data = await _equipment.GetEquipmentAsync() ?? new List<EquipmentDto>();
            foreach (var e in data.OrderBy(x => x.Name))
                Items.Add(e);
        }
        catch (Exception ex)
        {
            Error = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }
}
