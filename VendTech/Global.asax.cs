using Quartz;
using Quartz.Impl;
using System;
using System.ComponentModel;
using System.Net.Http;
using System.Security.Cryptography;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using VendTech.App_Start;
using VendTech.BLL.Jobs;
using VendTech.BLL.Models;

namespace VendTech
{
    public class MvcApplication : HttpApplication
    {
        protected void Application_Start()
        {
            IScheduler scheduler = StdSchedulerFactory.GetDefaultScheduler();
            scheduler.Start();

            /////
            ITrigger firstTrigger = TriggerBuilder.Create().StartNow()
            .WithSimpleSchedule (s => s.WithIntervalInMinutes(1).RepeatForever()).Build();
            IJobDetail jobFirst = JobBuilder.Create<ApplicationNotUsedSchedulerJob>().Build();
            /////


            /////
            //ITrigger secondTrigger = TriggerBuilder.Create().StartNow()
            //.WithSimpleSchedule(s => s.WithIntervalInSeconds(30).RepeatForever()).Build();
            //IJobDetail jobSecond = JobBuilder.Create<PendingTransactionCheckJob>().Build();
            /////


            /////
            //ITrigger thirdTrigger = TriggerBuilder.Create().StartNow()
            //.WithSimpleSchedule(s => s.WithIntervalInHours(24).RepeatForever()).Build();
            //IJobDetail jobThird = JobBuilder.Create<BalanceLowSheduleJob>().Build();
            /////

            scheduler.ScheduleJob(jobFirst, firstTrigger);
            //scheduler.ScheduleJob(jobSecond, secondTrigger);
            //scheduler.ScheduleJob(jobThird, thirdTrigger);



            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
           
        }
        //protected void Application_Error(object sender, EventArgs e)
        //{
        //    var error = Server.GetLastError();
        //    var cryptoEx = error as CryptographicException;
        //    if (cryptoEx != null)
        //    {
        //        HttpCookie auth_cookie = Request.Cookies[Cookies.AdminAuthorizationCookie];
        //        if (auth_cookie != null)
        //        {
        //            auth_cookie.Expires = DateTime.Now.AddDays(-30);
        //            Response.Cookies.Add(auth_cookie);
        //        }
        //        Server.ClearError();
        //    }
        //}

        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            System.Globalization.CultureInfo newCulture =
                (System.Globalization.CultureInfo)System.Threading.Thread.CurrentThread.CurrentCulture.Clone();
            newCulture.DateTimeFormat.ShortDatePattern = "dd/MM/yyyy";
            newCulture.DateTimeFormat.DateSeparator = "/";
            System.Threading.Thread.CurrentThread.CurrentCulture = newCulture;
        }

        private void HandleSignOut(HttpContext httpContext)
        {
            HttpCookie auth_cookie = Request.Cookies[Cookies.AdminAuthorizationCookie];
            if (auth_cookie != null)
            {
                auth_cookie.Expires = DateTime.Now.AddDays(-30);
                Response.Cookies.Add(auth_cookie);
            }
            HttpCookie web_auth_cookie = Request.Cookies[Cookies.AuthorizationCookie];
            if (web_auth_cookie != null)
            {
                web_auth_cookie.Expires = DateTime.Now.AddDays(-30);
                Response.Cookies.Add(web_auth_cookie);
            }
            //httpContext.Response.Redirect("~/Home/Error?errorMessage="+ httpContext.AllErrors[0].Message+"");
            httpContext.Response.Redirect("~/Home/Index");
        }

        protected void Application_Error()
        {

            HttpContext httpContext = HttpContext.Current;
            if (httpContext != null)
            {
                try
                {
                    RequestContext requestContext = ((MvcHandler)httpContext.CurrentHandler).RequestContext;
                    /* When the request is ajax the system can automatically handle a mistake with a JSON response. 
                   Then overwrites the default response */
                    if (requestContext.HttpContext.Request.IsAjaxRequest())
                    {
                        httpContext.Response.Clear();
                        string controllerName = requestContext.RouteData.GetRequiredString("controller");
                        IControllerFactory factory = ControllerBuilder.Current.GetControllerFactory();
                        IController controller = factory.CreateController(requestContext, controllerName);
                        ControllerContext controllerContext = new ControllerContext(requestContext, (ControllerBase)controller);

                        JsonResult jsonResult = new JsonResult
                        {
                            Data = new { success = false, serverError = "500" },
                            JsonRequestBehavior = JsonRequestBehavior.AllowGet
                        };
                        jsonResult.ExecuteResult(controllerContext);
                        httpContext.Response.End();
                    }
                    else
                    {
                        HandleSignOut(httpContext);
                    }
                }
                catch (InvalidCastException)
                {
                    HandleSignOut(httpContext);
                }

                catch (CryptographicException)
                {
                    HandleSignOut(httpContext);
                }
            }
        }
    }
}
