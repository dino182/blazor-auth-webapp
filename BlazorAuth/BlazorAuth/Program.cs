using BlazorAuth;
using BlazorAuth.Client;
using BlazorAuth.Components.Pages;
using BlazorAuth.Weather;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveWebAssemblyComponents();

builder.Services.AddScoped<WeatherForecaster>();

builder.Services.AddMicrosoftIdentityWebAppAuthentication(builder.Configuration);

// Add a fallback authorisation policy that will be invoked by the static assets middleware
builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

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

app.UseAuthentication();
app.UseAuthorization();

// Static files are served after authentication and authorisation so that they are not publicly accessible
// The authorisation fallback policy will prevent non-authenticated users accessing them
app.UseStaticFiles();
app.UseAntiforgery();

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

app.Run();
