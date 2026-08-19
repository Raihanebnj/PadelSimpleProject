using PadelSimple.Models.Dtos;

namespace PadelSimple.Mobile.Services;

public class AuthService
{
    private readonly ApiClient _api;

    public AuthService(ApiClient api) => _api = api;

    public string? Token { get; private set; }
    public string? Email { get; private set; }

    public bool IsLoggedIn => !string.IsNullOrWhiteSpace(Token);

    public async Task<bool> LoginAsync(string email, string password)
    {
        var response = await _api.PostAsync<LoginRequest, LoginResponse>(
            "/api/auth/login",
            new LoginRequest(email, password));

        if (response == null || string.IsNullOrWhiteSpace(response.Token))
            return false;

        Token = response.Token;
        Email = email;

        _api.SetBearer(Token);

        await SecureStorage.SetAsync("auth_token", Token);
        await SecureStorage.SetAsync("auth_email", Email);

        return true;
    }

    public async Task LogoutAsync()
    {
        Token = null;
        Email = null;
        _api.SetBearer(null);
        SecureStorage.Remove("auth_token");
        SecureStorage.Remove("auth_email");
        await Task.CompletedTask;
    }

    public async Task<bool> TryRestoreAsync()
    {
        Token = await SecureStorage.GetAsync("auth_token");
        Email = await SecureStorage.GetAsync("auth_email");
        if (!string.IsNullOrWhiteSpace(Token))
        {
            _api.SetBearer(Token);
            return true;
        }
        return false;
    }
}
