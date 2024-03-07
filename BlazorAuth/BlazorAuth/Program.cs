using System.Net.Http.Headers;
using BlazorAuth;
using BlazorAuth.Client;
using BlazorAuth.Components.Pages;
using BlazorAuth.Weather;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;
using Yarp.ReverseProxy.Transforms;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveWebAssemblyComponents();

builder.Services.AddScoped<WeatherForecaster>();

builder.Services.AddMicrosoftIdentityWebAppAuthentication(builder.Configuration)
    .EnableTokenAcquisitionToCallDownstreamApi()
    .AddMicrosoftGraph()
    .AddInMemoryTokenCaches();

// Set the authentication cookie name the same value as the fallback application
builder.Services.Configure<CookieAuthenticationOptions>(CookieAuthenticationDefaults.AuthenticationScheme, options =>
{
    options.Cookie.Name = ".Blazor.Auth";
});

// Store data protection keys in a location accessible to both this application and the fallback application
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(builder.Configuration["KeysLocation"] ?? throw new InvalidOperationException("Missing KeysLocation configuration")))
    .SetApplicationName("BlazorAuth");

// Add a fallback authorisation policy that will be invoked by the static assets middleware
builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

// Add YARP services
builder.Services.AddHttpForwarder();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
// Static files in the web root will be publicly accessible
// This is required to prevent the reverse proxy endpoint routing attempting to serve these from the fallback application
app.UseStaticFiles();

// Configure the middleware in the correct order so that pipeline operates properly
app.UseRouting();
app.UseAuthentication(); // must follow UseRouting
app.UseAuthorization(); // must follow UseAuthentication
app.UseAntiforgery(); // must follow UseAuthorization

// Require authorisation by default on all endpoints

// Use static server rendering to generate the error page that is used by the app.UseExceptionHandler middleware
// This page is separate from regular Blazor routing
app.MapGet("/error", () => new RazorComponentResult<Error>())
    .RequireAuthorization();

// Set up Blazor endpoints
app.MapRazorComponents<App>()
    .AddInteractiveWebAssemblyRenderMode()
    .RequireAuthorization();

// Set up web API
app.MapGet("/weather-forecast", ([FromServices] WeatherForecaster weatherForecaster) => weatherForecaster.GetWeatherForecast())
    .RequireAuthorization();

// Set up authentication endpoints
app.MapLoginAndLogout()
    .RequireAuthorization();

// Forward Microsoft Graph Toolkit proxy requests to Microsoft Graph
app.MapForwarder("/api/mgt/{**catch-all}", "https://graph.microsoft.com", builderContext =>
    {
        // Trim the prefix from the request as this is just the frontend proxy configuration
        builderContext.AddPathRemovePrefix("/api/mgt");

        // Attach Bearer tokens to the request from the Microsoft.Identity.Web token acquisition service
        builderContext.AddRequestTransform(async transformContext =>
        {
            var tokenAcquisition = transformContext.HttpContext.RequestServices.GetRequiredService<ITokenAcquisition>();
            var accessToken = await tokenAcquisition.GetAccessTokenForUserAsync(new[] { "user.read" });

            transformContext.ProxyRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        });
    })
    .RequireAuthorization();

// Forward all other requests to the fallback application
app.MapForwarder("/{**catch-all}", app.Configuration["ProxyTo"] ?? throw new InvalidOperationException("Missing ProxyTo configuration"))
    .RequireAuthorization();

app.Run();
