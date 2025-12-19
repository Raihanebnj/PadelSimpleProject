using CommunityToolkit.Mvvm.Input;
using PadelSimple.Mobile.Services;

namespace PadelSimple.Mobile.ViewModels;

public partial class LoginVm : BaseVm
{
    private readonly AuthService _auth;
    private readonly SyncService _sync;

    public string Email { get; set; } = "";
    public string Password { get; set; } = "";

    public LoginVm(AuthService auth, SyncService sync)
    {
        _auth = auth;
        _sync = sync;
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
                Error = "Login mislukt.";
                return;
            }

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
}
