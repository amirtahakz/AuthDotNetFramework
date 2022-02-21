using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Ui.Client.Startup))]
namespace Ui.Client
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
