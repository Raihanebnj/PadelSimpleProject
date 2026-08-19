using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PadelSimple.Mobile.Services;
using PadelSimple.Models.Dtos;
using System.Collections.ObjectModel;

namespace PadelSimple.Mobile.ViewModels;

public partial class ReservationsVm : BaseVm
{
    private readonly ReservationsService _reservations;
    private readonly AuthService _auth;

    public ObservableCollection<ReservationDto> Items { get; } = new();

    [ObservableProperty] private DateTime selectedDate = DateTime.Today;

    public string? UserEmail => _auth.Email;
    public bool IsLoggedIn => _auth.IsLoggedIn;

    public ReservationsVm(ReservationsService reservations, AuthService auth)
    {
        _reservations = reservations;
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
            var data = await _reservations.GetReservationsAsync(SelectedDate) ?? new List<ReservationDto>();
            foreach (var r in data.OrderBy(x => x.StartTime))
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
        await Shell.Current.GoToAsync("new_reservation");
    }

    [RelayCommand]
    public async Task LogoutAsync()
    {
        await _auth.LogoutAsync();
        await Shell.Current.GoToAsync("//login");
    }
}
