using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace PadelSimple.Mobile.Services;

public class ApiClient
{
    private readonly HttpClient _http;

    public ApiClient(HttpClient http)
    {
        _http = http;
        _http.BaseAddress = new Uri(ApiConfig.BaseUrl);
    }

    public void SetBearer(string? token)
    {
        _http.DefaultRequestHeaders.Authorization = null;
        if (!string.IsNullOrWhiteSpace(token))
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    private async Task EnsureBearerTokenAsync()
    {
        var token = AuthService.CurrentToken;
        if (string.IsNullOrWhiteSpace(token))
        {
            token = await SecureStorage.GetAsync("auth_token");
            if (!string.IsNullOrWhiteSpace(token))
            {
                AuthService.CurrentToken = token;
            }
        }

        if (!string.IsNullOrWhiteSpace(token))
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
        else
        {
            _http.DefaultRequestHeaders.Authorization = null;
        }
    }

    public async Task<T?> GetAsync<T>(string url, CancellationToken ct = default)
    {
        await EnsureBearerTokenAsync();
        using var res = await _http.GetAsync(url, ct);
        var json = await res.Content.ReadAsStringAsync(ct);

        if (!res.IsSuccessStatusCode)
        {
            if (!string.IsNullOrWhiteSpace(json) && !json.TrimStart().StartsWith("<"))
            {
                try
                {
                    using var doc = JsonDocument.Parse(json);
                    if (doc.RootElement.TryGetProperty("message", out var msgElement) && !string.IsNullOrWhiteSpace(msgElement.GetString()))
                    {
                        throw new Exception(msgElement.GetString());
                    }
                }
                catch (JsonException) { }
            }

            if (res.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                if (url.Contains("/api/auth"))
                {
                    throw new Exception("Fout e-mailadres of wachtwoord ingegeven.");
                }
                throw new Exception("Je bent niet ingelogd of je sessie is verlopen. Ga naar de Login pagina om in te loggen.");
            }
            res.EnsureSuccessStatusCode();
        }

        if (json.TrimStart().StartsWith("<"))
        {
            throw new Exception("Server retourneerde HTML in plaats van JSON. Ben je ingelogd en draait de Web API op de juiste poort?");
        }
        return JsonSerializer.Deserialize<T>(json, JsonOptions());
    }

    public async Task<TOut?> PostAsync<TIn, TOut>(string url, TIn body, CancellationToken ct = default)
    {
        await EnsureBearerTokenAsync();
        var jsonBody = JsonSerializer.Serialize(body, JsonOptions());
        using var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
        using var res = await _http.PostAsync(url, content, ct);
        var json = await res.Content.ReadAsStringAsync(ct);

        if (!res.IsSuccessStatusCode)
        {
            if (!string.IsNullOrWhiteSpace(json) && !json.TrimStart().StartsWith("<"))
            {
                try
                {
                    using var doc = JsonDocument.Parse(json);
                    if (doc.RootElement.TryGetProperty("message", out var msgElement) && !string.IsNullOrWhiteSpace(msgElement.GetString()))
                    {
                        throw new Exception(msgElement.GetString());
                    }
                }
                catch (JsonException) { }
            }

            if (res.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                if (url.Contains("/api/auth"))
                {
                    throw new Exception("Fout e-mailadres of wachtwoord ingegeven.");
                }
                throw new Exception("Je bent niet ingelogd of je sessie is verlopen. Ga naar de Login pagina om in te loggen.");
            }
            res.EnsureSuccessStatusCode();
        }

        if (json.TrimStart().StartsWith("<"))
        {
            throw new Exception("Server retourneerde HTML in plaats van JSON. Ben je ingelogd en draait de Web API op de juiste poort?");
        }
        return JsonSerializer.Deserialize<TOut>(json, JsonOptions());
    }

    public async Task DeleteAsync(string url, CancellationToken ct = default)
    {
        await EnsureBearerTokenAsync();
        using var res = await _http.DeleteAsync(url, ct);
        res.EnsureSuccessStatusCode();
    }

    private static JsonSerializerOptions JsonOptions() => new()
    {
        PropertyNameCaseInsensitive = true
    };
}
