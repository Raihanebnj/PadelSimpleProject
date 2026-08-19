using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PadelSimple.Mobile.Services;
using PadelSimple.Models.Dtos;
using System.Collections.ObjectModel;

namespace PadelSimple.Mobile.ViewModels;

public partial class CourtsVm : BaseVm
{
    private readonly CourtsService _courts;
    private readonly AuthService _auth;

    public ObservableCollection<CourtDto> Items { get; } = new();

    public string? UserEmail => _auth.Email;
    public bool IsLoggedIn => _auth.IsLoggedIn;

    public CourtsVm(CourtsService courts, AuthService auth)
    {
        _courts = courts;
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

    [RelayCommand]
    public async Task LogoutAsync()
    {
        await _auth.LogoutAsync();
        await Shell.Current.GoToAsync("//login");
    }
}
