using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(CafDataVisu.Startup))]
namespace CafDataVisu
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
