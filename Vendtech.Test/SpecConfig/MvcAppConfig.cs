using Microsoft.VisualStudio.TestTools.UnitTesting;
using SpecsFor.Mvc;
using VendTech;

namespace Vendtech.Test.SpecConfig
{
    [TestClass]
    class MvcAppConfig
    {
        private static SpecsForIntegrationHost integrationHost;

        [AssemblyInitialize()]
        public static void MyAssemblyInitialize(TestContext testContext)
        {
            var config = new SpecsForMvcConfig();

            config.UseIISExpress()
                .With(Project.Named("Vendtech"))
                .ApplyWebConfigTransformForConfig("Debug");

            config.BuildRoutesUsing(r => RouteConfig.RegisterRoutes(r));

            config.UseBrowser(BrowserDriver.Chrome); 
            integrationHost = new SpecsForIntegrationHost(config);
            integrationHost.Start();
        }

        [AssemblyCleanup()]
        public static void MyAssemblyCleanup()
        {
            integrationHost.Shutdown();
        }
    }
}