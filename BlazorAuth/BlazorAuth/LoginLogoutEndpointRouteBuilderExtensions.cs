using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;

namespace BlazorAuth;

internal static class LoginLogoutEndpointRouteBuilderExtensions
{
    internal static RouteGroupBuilder MapLoginAndLogout(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/authentication");

        // Only anonymous endpoint in entire application
        // Since there is no publicly available endpoint, redirect to the root page which will trigger authentication
        group.MapGet("/login", () => Results.Redirect("/"))
            .AllowAnonymous();

        // Sign out of the Cookie and OIDC handlers. If you do not sign out with the OIDC handler,
        // the user will automatically be signed back in the next time they visit a page that requires authentication
        // without being able to choose another account.
        group.MapPost("/logout", () => TypedResults.SignOut(new AuthenticationProperties { RedirectUri = "/" }, [CookieAuthenticationDefaults.AuthenticationScheme, OpenIdConnectDefaults.AuthenticationScheme]));

        // Endpoint to demonstrate supplying user claims to SPA frontend
        group.MapGet("/profile", (ClaimsPrincipal user) => user.Claims.ToDictionary(x => x.Type, x => x.Value));

        return group;
    }
}
