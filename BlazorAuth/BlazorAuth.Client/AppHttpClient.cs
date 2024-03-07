using System.Net.Http.Json;
using BlazorAuth.Client.Weather;

namespace BlazorAuth.Client;

public class AppHttpClient
{
    private readonly HttpClient _httpClient;

    public AppHttpClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IEnumerable<UserClaim>> GetAuthenticationState() =>
        await _httpClient.GetFromJsonAsync<IEnumerable<UserClaim>>("authentication/profile") ??
        throw new IOException("No authentication profile");

    public async Task<IEnumerable<WeatherForecast>> GetWeatherForecast() =>
        await _httpClient.GetFromJsonAsync<WeatherForecast[]>("/weather-forecast") ??
        throw new IOException("No weather forecast data");
}