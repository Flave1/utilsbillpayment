#region Default NameSpaces
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using System.Web.Security;
#endregion

#region Custom NameSpaces
using VendTech.Attributes;
using VendTech.BLL.Models;
using VendTech.BLL.Interfaces;
using VendTech.BLL.Common;
using VendTech.BLL.Managers;
using Ninject;
#endregion

namespace VendTech.Controllers
{
    [NoCache]
    public class BaseController : Controller
    {

        #region Variable Declaration
        private readonly IErrorLogManager _errorLogManager;

        [Inject]
        public IUserManager _userManager { get; set; }
        [Inject]
        public IPlatformManager _platformManager { get; set; }
        /// <summary>
        /// Contains Information for Logged In User
        /// </summary>
        protected UserDetails LOGGEDIN_USER { get; set; }
        protected IList<ModulesModel> ModulesModel { get; set; }
        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="errorLogManager"></param>
        public BaseController(IErrorLogManager errorLogManager)
        {
            _errorLogManager = errorLogManager;
        }

        /// <summary>
        /// This will check validation error on action executing
        /// </summary>
        /// <param name="filterContext"></param>
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (!ModelState.IsValid)
            {
                if (Request.IsAjaxRequest()) filterContext.Result = Json(new ActionOutput
                {
                    Status = ActionStatus.Error,
                    Message = "Validation Error"
                }, JsonRequestBehavior.AllowGet);
                //This needs to be changed to redirect the control to an error page.
                else filterContext.Result = null;
            }
        }

        /// <summary>
        /// This will be used to check user authorization
        /// </summary>
        /// <param name="filter_context"></param>
        protected override void OnAuthorization(AuthorizationContext filter_context)
        {

            HttpCookie auth_cookie = Request.Cookies[Cookies.AuthorizationCookie];
            HttpCookie admin_auth_cookie = Request.Cookies[Cookies.AdminAuthorizationCookie];
            IAuthenticateManager authenticateManager = new AuthenticateManager();
            var minutes = authenticateManager.GetLogoutTime();
            ViewBag.Minutes = minutes;
            #region If auth cookie is present
            if (auth_cookie != null)
            {
                #region If Logged User is null
                if (LOGGEDIN_USER == null)
                {
                    try
                    {
                        FormsAuthenticationTicket auth_ticket = FormsAuthentication.Decrypt(auth_cookie.Value);
                        LOGGEDIN_USER = new JavaScriptSerializer().Deserialize<UserDetails>(auth_ticket.UserData);
                        System.Web.HttpContext.Current.User = new System.Security.Principal.GenericPrincipal(new FormsIdentity(auth_ticket), null);
                    }
                    catch (Exception exc)
                    {
                        if (auth_cookie != null)
                        {
                            auth_cookie.Expires = DateTime.Now.AddDays(-30);
                            Response.Cookies.Add(auth_cookie);
                            filter_context.Result = RedirectToAction("index", "home");
                            LogExceptionToDatabase(exc);
                        }

                    }
                }
                if ((filter_context.ActionDescriptor.ActionName == "Index" || filter_context.ActionDescriptor.ActionName == "SignUp") && filter_context.ActionDescriptor.ControllerDescriptor.ControllerName == "Home")
                {
                    filter_context.Result = RedirectToAction("dashboard", "home");
                }


                #endregion

                ViewBag.LOGGEDIN_USER = LOGGEDIN_USER;
            }
            #endregion

            #region if authorization cookie is not present and the action method being called is not marked with the [Public] attribute
            else if (!filter_context.ActionDescriptor.GetCustomAttributes(typeof(Public), false).Any())
            {
                if (!Request.IsAjaxRequest()) filter_context.Result = RedirectToAction("index", "home", new { returnUrl = Server.UrlEncode(Request.RawUrl) });
                else filter_context.Result = Json(new ActionOutput
                {
                    Status = ActionStatus.Error,
                    Message = "Authentication Error"
                }, JsonRequestBehavior.AllowGet);
            }
            #endregion


            #region if authorization cookie is not present and the action method being called is marked with the [Public] attribute
            else
            {
                LOGGEDIN_USER = new UserDetails { IsAuthenticated = false };
                ViewBag.LOGGEDIN_USER = LOGGEDIN_USER;
            }
            #endregion
            if (LOGGEDIN_USER != null && filter_context.ActionDescriptor.ActionName != "VerifyChangePasswordOTP" && (filter_context.ActionDescriptor.ActionName != "ChangePassword") && (filter_context.ActionDescriptor.ActionName != "SignOut"))
            {
                if (LOGGEDIN_USER.UserID > 0)
                {
                    var user = _userManager.GetUserDetailsByUserId(LOGGEDIN_USER.UserID);
                    if (user.AccountStatus == (UserStatusEnum.PasswordNotReset).ToString())
                        filter_context.Result = RedirectToAction("ChangePassword", "Home");
                    else if (user.AccountStatus == (UserStatusEnum.Block).ToString())
                    {
                        HttpCookie val = Request.Cookies[Cookies.AuthorizationCookie];
                        val.Expires = DateTime.Now.AddDays(-30);
                        Response.Cookies.Add(val);
                        filter_context.Result = RedirectToAction("Index", "Home");
                        LOGGEDIN_USER = null;
                    }

                }
            }
            if (LOGGEDIN_USER != null && LOGGEDIN_USER.IsAuthenticated && LOGGEDIN_USER.LastActivityTime != null && LOGGEDIN_USER.LastActivityTime.Value.AddMinutes(minutes) < DateTime.UtcNow)
            {
                HttpCookie val = Request.Cookies[Cookies.AuthorizationCookie];
                val.Expires = DateTime.Now.AddDays(-30);
                Response.Cookies.Add(val);
                filter_context.Result = RedirectToAction("Index", "Home");
            }

            else if (LOGGEDIN_USER != null && LOGGEDIN_USER.IsAuthenticated)
            {
                string action = filter_context.ActionDescriptor.ActionName;
                string controller = filter_context.RouteData.Values["controller"].ToString();
                if (action.ToLower() != "autologout")
                {
                    LOGGEDIN_USER.LastActivityTime = DateTime.UtcNow;
                    var ckie = new JavaScriptSerializer().Serialize(LOGGEDIN_USER);
                    CreateCustomAuthorisationCookie(LOGGEDIN_USER.UserName, false, ckie);
                }
            }
            if (LOGGEDIN_USER == null)
            {
                filter_context.Result = RedirectToAction("Index", "Home");
            }
            SetActionName(filter_context.ActionDescriptor.ActionName, filter_context.ActionDescriptor.ControllerDescriptor.ControllerName);

        }

        /// <summary>
        /// This will be used to handle exceptions 
        /// </summary>
        /// <param name="filter_context"></param>
        protected override void OnException(ExceptionContext filter_context)
        {

            filter_context.ExceptionHandled = true;
            var error_id = "";

            //Check whether to log exception in database or not
            if (Config.LogExceptionInDatabase)
            {
                error_id = LogExceptionToDatabase(filter_context.Exception);//log exception in database
            }

            var msg = string.Empty;
            if (filter_context.Exception.GetType() == typeof(HttpRequestValidationException)) msg = "HTML tags or malicious characters are not allowed";


            //redirect control to ErrorResultJson action method if the request is an ajax request
            if (Request.IsAjaxRequest()) filter_context.Result = Json(new ActionOutput
            {
                Status = ActionStatus.Error,
                Message = (string.IsNullOrWhiteSpace(msg) ? "Error : " + error_id : msg) + ". Please contact the helpdesk."
            }, JsonRequestBehavior.AllowGet);

            //This needs to be changed to redirect the control to an error page.
            else filter_context.Result = null;

            base.OnException(filter_context);
        }

        /// <summary>
        /// log exception to database
        /// </summary>
        /// <param name="ex"></param>
        /// <returns></returns>
        private String LogExceptionToDatabase(Exception ex)
        {
            try
            {
                //Log exception in database
                var result = _errorLogManager.LogExceptionToDatabase(ex);

                //Log exception in file
                LogExceptionToFile("", ex.Message);

            }
            catch (Exception)
            {
                LogExceptionToFile("", ex.Message);
                return "0";
            }

            return null;
        }

        /// <summary>
        /// Log exception to text file on server
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="ex_message"></param>
        private void LogExceptionToFile(String ex, String ex_message)
        {
            System.IO.StreamWriter sw = new StreamWriter(Server.MapPath("~/ErrorLog.txt"), true);
            try
            {
                //sw = new StreamWriter(Server.MapPath("~/ErrorLog.txt"), true);
                sw.WriteLine(ex_message);
                //sw.WriteLine("http://jsonformat.com/");
                sw.WriteLine(ex); sw.WriteLine(""); sw.WriteLine("");
            }
            catch { }
            finally
            {
                sw.Close();
            }
        }

        /// <summary>
        /// used to create user authorization cookie after login
        /// </summary>
        /// <param name="user_name"></param>
        /// <param name="is_persistent"></param>
        /// <param name="custom_data"></param>
        protected virtual void CreateCustomAuthorisationCookie(String user_name, Boolean is_persistent, String custom_data)
        {
            FormsAuthenticationTicket auth_ticket =
                new FormsAuthenticationTicket(
                    1, user_name,
                    DateTime.Now,
                    DateTime.Now.AddDays(7),
                    is_persistent, custom_data, ""
                );
            String encrypted_ticket_ud = FormsAuthentication.Encrypt(auth_ticket);
            HttpCookie auth_cookie_ud = new HttpCookie(Cookies.AuthorizationCookie, encrypted_ticket_ud);
            if (is_persistent) auth_cookie_ud.Expires = auth_ticket.Expiration;
            System.Web.HttpContext.Current.Response.Cookies.Add(auth_cookie_ud);
        }

        /// <summary>
        /// used to update user authorization cookie after login
        /// </summary>
        /// <param name="user_name"></param>
        /// <param name="is_persistent"></param>
        /// <param name="custom_data"></param>
        protected virtual void UpdateCustomAuthorisationCookie(String custom_data)
        {
            var cookie = Request.Cookies[Cookies.AuthorizationCookie];
            FormsAuthenticationTicket authTicketExt = FormsAuthentication.Decrypt(cookie.Value);

            FormsAuthenticationTicket auth_ticket =
            new FormsAuthenticationTicket(
                1, authTicketExt.Name,
                authTicketExt.IssueDate,
                authTicketExt.Expiration,
                authTicketExt.IsPersistent, custom_data, String.Empty
            );
            String encryptedTicket = FormsAuthentication.Encrypt(auth_ticket);
            cookie = new HttpCookie(Cookies.AuthorizationCookie, encryptedTicket);
            if (authTicketExt.IsPersistent) cookie.Expires = auth_ticket.Expiration;
            System.Web.HttpContext.Current.Response.Cookies.Add(cookie);
        }

        /// <summary>
        /// this will be used to render a view as a string 
        /// </summary>
        /// <param name="view_name"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        protected string RenderRazorViewToString(string view_name, object model)
        {
            ViewData.Model = model;
            using (var sw = new StringWriter())
            {
                var viewResult = ViewEngines.Engines.FindPartialView(ControllerContext, view_name);
                var viewContext = new ViewContext(ControllerContext, viewResult.View, ViewData, TempData, sw);
                viewResult.View.Render(viewContext, sw);
                viewResult.ViewEngine.ReleaseView(ControllerContext, viewResult.View);
                return sw.GetStringBuilder().ToString();
            }
        }

        /// <summary>
        /// Will be used to logout from the application
        /// </summary>
        /// <returns></returns>
        [HttpGet, Public]
        public virtual ActionResult SignOut()
        {
            HttpCookie auth_cookie = Request.Cookies[Cookies.AuthorizationCookie];
            if (auth_cookie != null)
            {
                auth_cookie.Expires = DateTime.Now.AddDays(-30);
                Response.Cookies.Add(auth_cookie);
            }

            return Redirect(Url.Action("Index", "Home"));
        }

        /// <summary>
        /// This will be used to set action name
        /// </summary>
        /// <param name="actionName"></param>
        private void SetActionName(string actionName, string controllerName)
        {
            //ViewBag.ControllerActionName = controllerName + " " + actionName;
            ViewBag.ControllerName = controllerName;
        }

        /// <summary>
        /// This will dispose the context after request execution
        /// </summary>
        /// <param name="filterContext"></param>
        protected override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            new UserManager().Dispose();
        }

        #region Json Result Methods
        protected JsonResult JsonResult(List<string> resultString)
        {
            return Json(new ActionOutput { Status = ActionStatus.Successfull, Message = "", Results = resultString }, JsonRequestBehavior.AllowGet);
        }

        protected JsonResult JsonResult(ActionOutput actionOutput)
        {
            return Json(actionOutput, JsonRequestBehavior.AllowGet);
        }
        #endregion
    }
}