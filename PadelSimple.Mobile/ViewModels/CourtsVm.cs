using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PadelSimple.Mobile.Services;
using PadelSimple.Models.Dtos;
using System.Collections.ObjectModel;

namespace PadelSimple.Mobile.ViewModels;

public partial class CourtsVm : BaseVm
{
    private readonly CourtsService _courts;

    public ObservableCollection<CourtDto> Items { get; } = new();

    public CourtsVm(CourtsService courts)
    {
        _courts = courts;
    }

    [RelayCommand]
    public async Task LoadAsync()
    {
        if (IsBusy) return;
        IsBusy = true;
        Error = null;
        Info = null;

        try
        {
            Items.Clear();
            var data = await _courts.GetCourtsAsync() ?? new List<CourtDto>();
            foreach (var c in data.OrderBy(x => x.Name))
                Items.Add(c);
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
