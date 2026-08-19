using CommunityToolkit.Mvvm.ComponentModel;
using PadelSimple.Models.Dtos;

namespace PadelSimple.Mobile.Services;

public partial class AuthService : ObservableObject
{
    private readonly ApiClient _api;

    public static string? CurrentToken { get; set; }

    public AuthService(ApiClient api) => _api = api;

    [ObservableProperty] private string? token;
    [ObservableProperty] private string? email;

    public bool IsLoggedIn => !string.IsNullOrWhiteSpace(Token);

    public async Task<bool> LoginAsync(string emailInput, string password)
    {
        var response = await _api.PostAsync<LoginRequest, LoginResponse>(
            "/api/auth/login",
            new LoginRequest(emailInput, password));

        if (response == null || string.IsNullOrWhiteSpace(response.Token))
            return false;

        Token = response.Token;
        Email = emailInput;
        CurrentToken = response.Token;

        _api.SetBearer(Token);

        await SecureStorage.SetAsync("auth_token", Token);
        await SecureStorage.SetAsync("auth_email", Email ?? "");

        OnPropertyChanged(nameof(IsLoggedIn));
        return true;
    }

    public async Task LogoutAsync()
    {
        Token = null;
        Email = null;
        CurrentToken = null;
        _api.SetBearer(null);
        SecureStorage.Remove("auth_token");
        SecureStorage.Remove("auth_email");
        OnPropertyChanged(nameof(IsLoggedIn));
        await Task.CompletedTask;
    }

    public async Task<bool> TryRestoreAsync()
    {
        Token = await SecureStorage.GetAsync("auth_token");
        Email = await SecureStorage.GetAsync("auth_email");
        CurrentToken = Token;
        if (!string.IsNullOrWhiteSpace(Token))
        {
            _api.SetBearer(Token);
            OnPropertyChanged(nameof(IsLoggedIn));
            return true;
        }
        return false;
    }
}
