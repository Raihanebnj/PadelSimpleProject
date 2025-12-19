namespace PadelSimple.Mobile.Services;

public class AuthService
{
    public string? Token { get; private set; }
    public string? Email { get; private set; }

    public bool IsLoggedIn => !string.IsNullOrWhiteSpace(Token);

    public async Task<bool> LoginAsync(string email, string password)
    {
        
        await Task.Delay(100);

       
        Token = Guid.NewGuid().ToString("N");
        Email = email;

       
        await SecureStorage.SetAsync("auth_token", Token);
        await SecureStorage.SetAsync("auth_email", Email);

        return true;
    }

    public async Task LogoutAsync()
    {
        Token = null;
        Email = null;
        SecureStorage.Remove("auth_token");
        SecureStorage.Remove("auth_email");
        await Task.CompletedTask;
    }

    public async Task TryRestoreAsync()
    {
        Token = await SecureStorage.GetAsync("auth_token");
        Email = await SecureStorage.GetAsync("auth_email");
    }
}
