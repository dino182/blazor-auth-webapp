using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;

namespace BlazorAuth.Client.Services;

public class CookieAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly AppHttpClient _httpClient;

    public CookieAuthenticationStateProvider(AppHttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var claims = await _httpClient.GetAuthenticationState();
        return new AuthenticationState(
            new ClaimsPrincipal(
                new ClaimsIdentity(claims.Select(c => new Claim(c.Type, c.Value)), nameof(CookieAuthenticationStateProvider), "name", null)
            )
        );
    }
}