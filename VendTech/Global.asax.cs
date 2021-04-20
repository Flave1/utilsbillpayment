using Quartz;
using Quartz.Impl;
using System;
using System.Collections.Generic;
using System.Linq;
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

        protected void Application_Error()
        {
            HttpContext httpContext = HttpContext.Current;
            if (httpContext != null)
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
                    httpContext.Response.Redirect("~/Home/Error?errorMessage="+ httpContext.AllErrors[0].Message+"");
                }
            }
        }
    }
}
