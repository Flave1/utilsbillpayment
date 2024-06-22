using Microsoft.Owin;
using Owin;
using Vendtech.Test;

[assembly: OwinStartupAttribute(typeof(VendTech.App_Start.Startup))]
namespace VendTech.App_Start
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.Use(typeof(LogMiddleware));
        }
    }
}