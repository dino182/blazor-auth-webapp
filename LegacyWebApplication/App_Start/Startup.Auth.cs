using Owin;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.OWIN;
using Microsoft.Identity.Web.TokenCacheProviders.InMemory;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;

namespace LegacyWebApplication
{
    public partial class Startup
    {
        public void ConfigureAuth(IAppBuilder app)
        {
            app.SetDefaultSignInAsAuthenticationType(CookieAuthenticationDefaults.AuthenticationType);

            app.UseCookieAuthentication(new CookieAuthenticationOptions());

            // Get an TokenAcquirerFactory specialized for OWIN
            var owinTokenAcquirerFactory = TokenAcquirerFactory.GetDefaultInstance<OwinTokenAcquirerFactory>();

            // Configure the web app.
            app.AddMicrosoftIdentityWebApp(owinTokenAcquirerFactory, updateOptions: options => options.RedirectUri = owinTokenAcquirerFactory.Configuration["AzureAd:RedirectUri"]);

            // Add the services you need.
            owinTokenAcquirerFactory.Services
                .AddMicrosoftGraph()
                .AddInMemoryTokenCaches();

            owinTokenAcquirerFactory.Build();
        }
    }
}
