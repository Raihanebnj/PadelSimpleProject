using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PadelSimple.Mobile.Services;
using PadelSimple.Models.Dtos;
using System.Collections.ObjectModel;

namespace PadelSimple.Mobile.ViewModels;

public partial class ReservatiesVm : BasisVm
{
    private readonly ReservatiesService _reservaties;
    private readonly AuthService _auth;

    public ObservableCollection<ReservatieDto> Items { get; } = new();

    [ObservableProperty] private DateTime selectedDate = DateTime.Today;

    public string? UserEmail => _auth.Email;
    public bool IsLoggedIn => _auth.IsLoggedIn;

    public ReservatiesVm(ReservatiesService reservaties, AuthService auth)
    {
        _reservaties = reservaties;
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
            var data = await _reservaties.GetReservatiesAsync(SelectedDate) ?? new List<ReservatieDto>();
            foreach (var r in data.OrderBy(x => x.StartUur))
                Items.Add(r);
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
    public async Task GoToNewReservationAsync()
    {
        await Shell.Current.GoToAsync("//nieuwe_reservatie_tab");
    }

    [RelayCommand]
    public async Task LogoutAsync()
    {
        await _auth.LogoutAsync();
        await Shell.Current.GoToAsync("//login");
    }
}
