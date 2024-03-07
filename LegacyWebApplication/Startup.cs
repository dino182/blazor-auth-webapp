using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(LegacyWebApplication.Startup))]

namespace LegacyWebApplication
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
