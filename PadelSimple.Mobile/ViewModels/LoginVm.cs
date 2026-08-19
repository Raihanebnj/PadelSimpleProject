using CommunityToolkit.Mvvm.Input;
using PadelSimple.Mobile.Services;

namespace PadelSimple.Mobile.ViewModels;

public partial class LoginVm : BaseVm
{
    private readonly AuthService _auth;
    private readonly SyncService _sync;

    public string Email { get; set; } = "";
    public string Password { get; set; } = "";

    public string? UserEmail => _auth.Email;
    public bool IsLoggedIn => _auth.IsLoggedIn;
    public bool IsNotLoggedIn => !_auth.IsLoggedIn;

    public LoginVm(AuthService auth, SyncService sync)
    {
        _auth = auth;
        _sync = sync;
        _auth.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(AuthService.Email) || e.PropertyName == nameof(AuthService.IsLoggedIn))
            {
                OnPropertyChanged(nameof(UserEmail));
                OnPropertyChanged(nameof(IsLoggedIn));
                OnPropertyChanged(nameof(IsNotLoggedIn));
            }
        };
    }

    [RelayCommand]
    private async Task RestoreAsync()
    {
        if (IsBusy) return;
        IsBusy = true;

        try
        {
            var ok = await _auth.TryRestoreAsync();
            if (ok)
            {
                await _sync.TrySyncAsync();
                await Shell.Current.GoToAsync("//main/courts");
            }
        }
        catch
        {
            // Geen geldige sessie: blijf op de loginpagina.
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task LoginAsync()
    {
        if (IsBusy) return;
        IsBusy = true;
        Error = null;
        Info = null;

        try
        {
            var ok = await _auth.LoginAsync(Email, Password);
            if (!ok)
            {
                Error = "Login mislukt. Controleer e-mail en wachtwoord.";
                return;
            }

            OnPropertyChanged(nameof(UserEmail));
            OnPropertyChanged(nameof(IsLoggedIn));
            await _sync.TrySyncAsync();
            await Shell.Current.GoToAsync("//main/courts");
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
    private async Task LogoutAsync()
    {
        await _auth.LogoutAsync();
        Email = "";
        Password = "";
        Info = "Succesvol uitgelogd.";
        OnPropertyChanged(nameof(UserEmail));
        OnPropertyChanged(nameof(IsLoggedIn));
    }
}
