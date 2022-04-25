using Ninject;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using System.Web.Mvc;
using VendTech.BLL.Interfaces;
using VendTech.BLL.Managers;
using VendTech.BLL.Models;
using VendTech.Framework.Api;
using VendTech.Areas.Api.Controllers;

namespace VendTech.Attributes
{
    public class AuthenticateUser : Attribute { }

    public class Public : Attribute { }

    public class MemberAccess : Attribute { }

    public class AjaxOnlyAttribute : ActionMethodSelectorAttribute
    {
        public override bool IsValidForRequest(ControllerContext controllerContext, System.Reflection.MethodInfo methodInfo)
        {
            return controllerContext.RequestContext.HttpContext.Request.IsAjaxRequest();
        }
    }

    public class ModuleAccessAttribute : ActionMethodSelectorAttribute
    {
        private int id { get; set; }

        public ModuleAccessAttribute(int id)
        {
            this.id = id;
        }

        public override bool IsValidForRequest(ControllerContext controllerContext, System.Reflection.MethodInfo methodInfo)
        {
            return controllerContext.RequestContext.HttpContext.Request.IsAjaxRequest();
        }
    }
    public class HandelExceptionAttribute : System.Web.Http.Filters.ExceptionFilterAttribute
    {
        [Inject]
        public IErrorLogManager _errorLogManager { get; set; }
        public override void OnException(HttpActionExecutedContext actionExecutedContext)
        {
            var ex = actionExecutedContext.Exception;
            try
            {
                _errorLogManager.LogExceptionToDatabase(ex);
            }
            catch (Exception)
            {
                System.IO.StreamWriter sw = null;
                try
                {
                    sw = new StreamWriter(System.Web.HttpContext.Current.Server.MapPath("~/ErrorLog.txt"), true);
                    sw.WriteLine(ex.Message);
                    sw.WriteLine(DateTime.UtcNow);
                    sw.WriteLine(ex); sw.WriteLine(""); sw.WriteLine("");
                }
                catch { }
                finally { sw.Close(); }
            }
            _errorLogManager.LogExceptionToDatabase(ex);
            actionExecutedContext.Response = new JsonContent("An Unexpected Error Has Occured!", Status.Failed).ConvertToHttpResponseOK();
        }
    }
    /// <summary>
    /// Checks if the incomming user is authorized at the time any function in about to execute
    /// </summary>
    public class CheckAuthorizationFilter : Attribute { }


    public class CheckAuthorizationAttribute : System.Web.Http.AuthorizeAttribute
    {
        [Inject]
        public IUserManager _userManager { get; set; }

        public override void OnAuthorization(HttpActionContext actionContext)
        {
            var baseController = (BaseAPIController)actionContext.ControllerContext.Controller;
            var skipAuthorization = actionContext.ActionDescriptor.GetCustomAttributes<SkipAuthorization>().Any();
            var skipAuthentication = actionContext.ActionDescriptor.GetCustomAttributes<SkipAuthentication>().Any();
            string sessionToken = "";
            if (!skipAuthorization)
            {
                try
                {
                    sessionToken = actionContext.Request.Headers.GetValues("Token").FirstOrDefault();
                }
                catch (Exception)
                {

                    actionContext.Response = new JsonContent("Token Or Passcode not present!", Status.Failed).ConvertToHttpResponseOK();
                    return;
                }


                UserModel loginSession = _userManager.ValidateUserSession(sessionToken);
                //Please do not cahnge message for invalid token, Because there is a check in app side based on this message
                if (loginSession == null)
                    actionContext.Response = new JsonContent("Request could not be authorized. Invalid token!", Status.Failed).ConvertToHttpResponseOK();
                else
                {
                    _userManager.UpdateUserLastAppUsedTime(loginSession.UserId);
                    baseController.LOGGEDIN_USER = new UserModel
                    {
                        UserId = loginSession.UserId,
                        Email = loginSession.Email,
                        //SessionId = loginSession.SessionId,
                        //FirstName = loginSession.FirstName,
                        //LastName = loginSession.LastName,
                        //ProfileImageName = loginSession.ProfileImageName,
                        //DateOfBirth = loginSession.DateOfBirth,
                        //Address = loginSession.Address,
                        //Phone = loginSession.Phone,
                        //CompanyLogoName = loginSession.CompanyLogoName,
                        //CompanyName = loginSession.CompanyName,
                        //CompanyLocation = loginSession.CompanyLocation,
                        //DateCreated = loginSession.DateCreated,
                        //DateModified = loginSession.DateModified,
                        //IsDeleted = loginSession.IsDeleted
                    };
                }
            }
        }
        /// <summary>
        /// methods marked with this will not be checked for authorization
        /// </summary>
        public class SkipAuthorization : Attribute { }


        /// <summary>
        /// methods marked with this will not be checked for authentication
        /// </summary>
        public class SkipAuthentication : Attribute { }

        //    private void SetLoggedInUser(HttpActionContext actionContext, string sessionToken)
        //    {
        //        var baseController = (BaseAPIController)actionContext.ControllerContext.Controller;
        //        var secretKey = Config.ONACodeSecretKey;
        //        var clientHash = actionContext.Request.Headers.GetValues("ClientHash").FirstOrDefault();
        //        if (clientHash != "RT%U&*UGF")
        //        {
        //            var timeStamp = actionContext.Request.Headers.GetValues("Timestamp").FirstOrDefault();
        //            var validationHash = Utilities.HashCode(string.Format("{0}{1}{2}", sessionToken, timeStamp, secretKey));
        //            if (!validationHash.Equals(clientHash, StringComparison.InvariantCultureIgnoreCase))
        //            {
        //                actionContext.Response = new JsonContent("Request could not be authenticated. Invalid hash encountered!", Status.Failed, new UserSession() { sessionStatus = false, sessionStatusType = Sessionstatus.Expired }).ConvertToHttpResponseOK();
        //            }
        //        }

        //        var session = _userManager.ValidateSession(sessionToken);

        //        if (session.Status == ActionStatus.Error)
        //        {
        //            Sessionstatus sessionStatus = Sessionstatus.Present;
        //            if (session.Message == "Invalid Session")
        //                sessionStatus = Sessionstatus.Invalid;
        //            else if (session.Message == "Expired Session")
        //                sessionStatus = Sessionstatus.Expired;

        //            actionContext.Response = new JsonContent(session.Message, Status.Failed, new UserSession() { sessionStatus = false, sessionStatusType = sessionStatus }).ConvertToHttpResponseNAUTH();
        //        }
        //        else
        //        {
        //            baseController.LOGGED_IN_USER = new ApiUserModel
        //            {
        //                UserId = session.Object.UserId,
        //                Email = session.Object.Email,
        //                SessionId = session.Object.SessionId,
        //                FullName = session.Object.Name,
        //                UserType = session.Object.UserType
        //            };
        //        }
        //    }
        //}



        public class NoCacheAttribute : System.Web.Mvc.ActionFilterAttribute
        {
            public override void OnResultExecuting(ResultExecutingContext filterContext)
            {
                filterContext.HttpContext.Response.Cache.SetExpires(DateTime.UtcNow.AddDays(-1));
                filterContext.HttpContext.Response.Cache.SetValidUntilExpires(false);
                filterContext.HttpContext.Response.Cache.SetRevalidation(HttpCacheRevalidation.AllCaches);
                filterContext.HttpContext.Response.Cache.SetCacheability(HttpCacheability.NoCache);
                filterContext.HttpContext.Response.Cache.SetNoStore();
                base.OnResultExecuting(filterContext);
            }


        }

        /// <summary>
        /// validates the incomming model
        /// </summary>

        /// <summary>
        /// This will be used to skip model validations
        /// </summary>
        public class IgnoreModelErrorsAttribute : System.Web.Mvc.ActionFilterAttribute
        {
            private string keysString;

            public IgnoreModelErrorsAttribute(string keys)
                : base()
            {
                this.keysString = keys;
            }

            public override void OnActionExecuting(ActionExecutingContext filterContext)
            {
                ModelStateDictionary modelState = filterContext.Controller.ViewData.ModelState;
                string[] keyPatterns = keysString.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < keyPatterns.Length; i++)
                {
                    string keyPattern = keyPatterns[i]
                        .Trim()
                        .Replace(@".", @"\.")
                        .Replace(@"[", @"\[")
                        .Replace(@"]", @"\]")
                        .Replace(@"\[\]", @"\[[0-9]+\]")
                        .Replace(@"*", @"[A-Za-z0-9]+");
                    IEnumerable<string> matchingKeys = modelState.Keys.Where(x => Regex.IsMatch(x, keyPattern));
                    foreach (string matchingKey in matchingKeys)
                        modelState[matchingKey].Errors.Clear();
                }
            }

        }
    } /// <summary>
      /// This will be used to set No Cache For Controller Actions
      /// </summary>

    public class NoCacheAttribute : System.Web.Mvc.ActionFilterAttribute
    {
        public override void OnResultExecuting(ResultExecutingContext filterContext)
        {
            filterContext.HttpContext.Response.Cache.SetExpires(DateTime.UtcNow.AddDays(-1));
            filterContext.HttpContext.Response.Cache.SetValidUntilExpires(false);
            filterContext.HttpContext.Response.Cache.SetRevalidation(HttpCacheRevalidation.AllCaches);
            filterContext.HttpContext.Response.Cache.SetCacheability(HttpCacheability.NoCache);
            filterContext.HttpContext.Response.Cache.SetNoStore();
            base.OnResultExecuting(filterContext);
        }


    }
    public class ValidateModel : System.Web.Http.Filters.ActionFilterAttribute
    {
        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            if (!actionContext.ModelState.IsValid)
            {
                var message = actionContext.ModelState.Values.FirstOrDefault().Errors.FirstOrDefault().ErrorMessage;
                if (!string.IsNullOrEmpty(message))
                {
                    actionContext.Response = new JsonContent(message, Status.Failed).ConvertToHttpResponseOK();
                }
                else
                {
                    message = "Json is not in Proper format.";
                    actionContext.Response = new JsonContent(message, Status.Failed).ConvertToHttpResponseOK();

                }
            }
        }

        public override void OnActionExecuted(HttpActionExecutedContext actionContext)
        {
            new UserManager().Dispose();
        }
        public class SkipModelValidation : Attribute { }
    }
}