using Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;



namespace Vendtech.Test
{
    public class OwinTestConf
    {
        public void Configuration(IAppBuilder app)
        {
            app.Use(typeof(LogMiddleware));
            HttpConfiguration config = new HttpConfiguration();
            config.Services.Replace(typeof(System.Web.Http.Dispatcher.IAssembliesResolver), new TestWebApiResolver());
            config.MapHttpAttributeRoutes();
            app.UseWebApi(config);
        }
    }
}
