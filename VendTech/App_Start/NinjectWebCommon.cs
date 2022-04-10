[assembly: WebActivatorEx.PreApplicationStartMethod(typeof(VendTech.App_Start.NinjectWebCommon), "Start")]
[assembly: WebActivatorEx.ApplicationShutdownMethodAttribute(typeof(VendTech.App_Start.NinjectWebCommon), "Stop")]

namespace VendTech.App_Start
{
    using System;
    using System.Web;
    using Microsoft.Web.Infrastructure.DynamicModuleHelper;
    using Ninject;
    using Ninject.Web.Common;
    using Ninject.Extensions.Conventions;
    using System.Reflection;
    using Ninject.Web.Common.WebHost;
    using VendTech.BLL;

    public static class NinjectWebCommon
    {
        private static readonly Bootstrapper bootstrapper = new Bootstrapper();
        
        /// <summary>
        /// Starts the application
        /// </summary>
        public static void Start()
        {
            DynamicModuleUtility.RegisterModule(typeof(OnePerRequestHttpModule));
            DynamicModuleUtility.RegisterModule(typeof(NinjectHttpModule));
            
            bootstrapper.Initialize(CreateKernel);
        }

        /// <summary>
        /// Stops the application.
        /// </summary>
        public static void Stop()
        {
            bootstrapper.ShutDown();
        }

        /// <summary>
        /// Creates the kernel that will manage your application.
        /// </summary>
        /// <returns>The created kernel.</returns>
        private static IKernel CreateKernel()
        {
            var kernel = new StandardKernel();
            try
            {
                kernel.Bind<Func<IKernel>>().ToMethod(ctx => () => new Bootstrapper().Kernel).InRequestScope();
                kernel.Bind<IHttpModule>().To<HttpApplicationInitializationHttpModule>().InRequestScope();
                kernel.Bind<DbContext>().ToSelf().InRequestScope();
                RegisterServices(kernel);
                return kernel;
            }
            catch
            {
                kernel.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Load your modules or register your services here!
        /// </summary>
        /// <param name="kernel">The kernel.</param>
        private static void RegisterServices(IKernel kernel)
        {
            //System.Web.Http.GlobalConfiguration.Configuration.DependencyResolver = new Ninject.WebApi.DependencyResolver.NinjectDependencyResolver(kernel);

            kernel.Load(Assembly.GetExecutingAssembly());

            kernel.Bind(x => x
            .FromAssembliesMatching("VendTech.BLL.dll")
            .SelectAllClasses()
            .BindDefaultInterface());



            // In case of Multiple interface implementations
            //   kernel.Bind<IAmAnInterface>().To<UserManager>().Named("Strong");
            // kernel.Bind<IAmAnInterface>().To<AmAnInterface>().Named("Weak");
        }
    }
}
