using CommunityToolkit.Mvvm.Input;
using PadelSimple.Mobile.Services;
using PadelSimple.Models.Dtos;
using System.Collections.ObjectModel;

namespace PadelSimple.Mobile.ViewModels;

public partial class EquipmentVm : BaseVm
{
    private readonly EquipmentService _equipment;
    private readonly AuthService _auth;

    public ObservableCollection<EquipmentDto> Items { get; } = new();

    public string? UserEmail => _auth.Email;
    public bool IsLoggedIn => _auth.IsLoggedIn;

    public EquipmentVm(EquipmentService equipment, AuthService auth)
    {
        _equipment = equipment;
        _auth = auth;
        _auth.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(AuthService.Email) || e.PropertyName == nameof(AuthService.IsLoggedIn))
            {
                OnPropertyChanged(nameof(UserEmail));
                OnPropertyChanged(nameof(IsLoggedIn));
            }
        };
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

    [RelayCommand]
    public async Task LogoutAsync()
    {
        await _auth.LogoutAsync();
        await Shell.Current.GoToAsync("//login");
    }
}
