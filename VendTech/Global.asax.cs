using Quartz;
using Quartz.Impl;
using System;
using System.Security.Cryptography;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using VendTech.App_Start;
using VendTech.BLL.Models;

namespace VendTech
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {


            IScheduler scheduler = StdSchedulerFactory.GetDefaultScheduler();
            scheduler.Start();
            ITrigger firstTrigger = TriggerBuilder.Create().StartNow()
            .WithSimpleSchedule
              (s =>
                 s.WithIntervalInMinutes(2).RepeatForever()
              )
            .Build();
            IJobDetail jobFirst = JobBuilder.Create<VendTech.BLL.Models.ApplicationNotUsedSchedulerJob>().Build();
            scheduler.ScheduleJob(jobFirst, firstTrigger);



            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

        }
        protected void Application_Error(object sender, EventArgs e)
        {
            var error = Server.GetLastError();
            var cryptoEx = error as CryptographicException;
            if (cryptoEx != null)
            {
                HttpCookie auth_cookie = Request.Cookies[Cookies.AdminAuthorizationCookie];
                if (auth_cookie != null)
                {
                    auth_cookie.Expires = DateTime.Now.AddDays(-30);
                    Response.Cookies.Add(auth_cookie);
                }
                Server.ClearError();
            }
        }
    }
}