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

    public async Task<T?> GetAsync<T>(string url, CancellationToken ct = default)
    {
        using var res = await _http.GetAsync(url, ct);
        res.EnsureSuccessStatusCode();
        var json = await res.Content.ReadAsStringAsync(ct);
        return JsonSerializer.Deserialize<T>(json, JsonOptions());
    }

    public async Task<TOut?> PostAsync<TIn, TOut>(string url, TIn body, CancellationToken ct = default)
    {
        var jsonBody = JsonSerializer.Serialize(body, JsonOptions());
        using var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
        using var res = await _http.PostAsync(url, content, ct);
        res.EnsureSuccessStatusCode();
        var json = await res.Content.ReadAsStringAsync(ct);
        return JsonSerializer.Deserialize<TOut>(json, JsonOptions());
    }

    public async Task DeleteAsync(string url, CancellationToken ct = default)
    {
        using var res = await _http.DeleteAsync(url, ct);
        res.EnsureSuccessStatusCode();
    }

    private static JsonSerializerOptions JsonOptions() => new()
    {
        PropertyNameCaseInsensitive = true
    };
}
