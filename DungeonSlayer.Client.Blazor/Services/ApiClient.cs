using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace DungeonRush.Client.Blazor.Services;

public class ApiClient
{
    private readonly HttpClient _http;
    private readonly AuthState _authState;

    public ApiClient(HttpClient http, AuthState authState)
    {
        _http = http;
        _authState = authState;
    }

    private HttpRequestMessage CreateRequest(HttpMethod method, string url, object? data = null)
    {
        var req = new HttpRequestMessage(method, url);
        if (_authState.IsAuthenticated)
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _authState.JwtToken);

        if (data != null)
        {
            var json = JsonSerializer.Serialize(data);
            req.Content = new StringContent(json, Encoding.UTF8, "application/json");
        }
        return req;
    }

    public async Task<HttpResponseMessage> SendAsync(HttpMethod method, string url, object? data = null)
    {
        var req = CreateRequest(method, url, data);
        return await _http.SendAsync(req);
    }

    public async Task<T?> GetAsync<T>(string url)
    {
        var response = await SendAsync(HttpMethod.Get, url);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<T>();
    }

    public async Task<T?> PostAsync<T>(string url, object data)
    {
        var response = await SendAsync(HttpMethod.Post, url, data);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<T>();
    }
}