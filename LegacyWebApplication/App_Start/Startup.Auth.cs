using System;
using System.Configuration;
using System.IO;
using Microsoft.AspNetCore.DataProtection;
using Owin;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.OWIN;
using Microsoft.Identity.Web.TokenCacheProviders.InMemory;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.Interop;

namespace LegacyWebApplication
{
    public partial class Startup
    {
        public void ConfigureAuth(IAppBuilder app)
        {
            app.SetDefaultSignInAsAuthenticationType(CookieAuthenticationDefaults.AuthenticationType);

            // Configure the authentication to use the same values as the main application so that authentication state is shared between the two
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                CookieName = ".Blazor.Auth",
                TicketDataFormat = new AspNetTicketDataFormat(
                    new DataProtectorShim(
                        DataProtectionProvider.Create(new DirectoryInfo(Path.IsPathRooted(ConfigurationManager.AppSettings["KeysLocation"]) ? ConfigurationManager.AppSettings["KeysLocation"] : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ConfigurationManager.AppSettings["KeysLocation"])),
                            builder => builder.SetApplicationName("BlazorAuth"))
                            .CreateProtector("Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationMiddleware", CookieAuthenticationDefaults.AuthenticationType, "v2")
                        )),
                CookieManager = new ChunkingCookieManager()
            });

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
