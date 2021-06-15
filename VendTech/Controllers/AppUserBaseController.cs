#region Default Namespaces
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using System.Web.Security;
#endregion

#region Custom Namespaces
using VendTech.Attributes;
using VendTech.Controllers;
using VendTech.BLL.Models;
using VendTech.BLL.Interfaces;
using VendTech.BLL.Managers;
using VendTech.BLL.Common;
using Newtonsoft.Json;
#endregion

namespace VendTech.Areas.Admin.Controllers
{
    /// <summary>
    /// This controller will work as a base controller for the admin section of the application
    /// </summary>
    [NoCache]
    public class AppUserBaseController : BaseController
    {


        public AppUserBaseController(IErrorLogManager errorLogManager)
            : base(errorLogManager)
        {

        }

        /// <summary>
        /// This will be used to chek  user authorization
        /// </summary>
        /// <param name="filter_context"></param>
        protected override void OnAuthorization(AuthorizationContext filter_context)
        {
            HttpCookie auth_cookie = Request.Cookies[Cookies.AuthorizationCookie]; 
            IAuthenticateManager authenticateManager = new AuthenticateManager();
            var minutes = authenticateManager.GetLogoutTime();
            var model = new PermissonAndDetailModel();
            ViewBag.Minutes = minutes;
            #region If auth cookie is present
            if (auth_cookie != null)
            {
                #region If LoggedInUser is null
                if (LOGGEDIN_USER == null)
                {
                    try
                    {
                        if (JustLoggedin)
                        {
                            FormsAuthenticationTicket auth_ticket = FormsAuthentication.Decrypt(auth_cookie.Value);
                            model = new JavaScriptSerializer().Deserialize<PermissonAndDetailModel>(auth_ticket.UserData);
                            LOGGEDIN_USER = model.UserDetails;
                            ModulesModel = model.ModulesModelList;
                            System.Web.HttpContext.Current.User = new System.Security.Principal.GenericPrincipal(new FormsIdentity(auth_ticket), null);
                        }
                        else
                        {
                            //SignOut();
                        }

                    }
                    catch (Exception ex)
                    {
                        if (auth_cookie != null)
                        {
                            auth_cookie.Expires = DateTime.Now.AddDays(-30);
                            Response.Cookies.Add(auth_cookie);
                            JustLoggedin = false;
                            filter_context.Result = RedirectToAction("index", "home"); 
                        }
                        Console.WriteLine(ex.ToString());
                    }
                }
                #endregion

                if (LOGGEDIN_USER != null)
                {
                    var isEnabled = _authManager.IsUserPosEnable(LOGGEDIN_USER.UserID);
                    if (!isEnabled)
                    {
                        HttpCookie val = Request.Cookies[Cookies.AuthorizationCookie];
                        val.Expires = DateTime.Now.AddDays(-30);
                        Response.Cookies.Add(val);
                        LOGGEDIN_USER = null;
                        filter_context.Result = RedirectToAction("Index", "Home");
                    }
                }
                ViewBag.LOGGEDIN_USER = LOGGEDIN_USER;
                ViewBag.USER_PERMISSONS = ModulesModel;
            }
            #endregion

            #region if authorization cookie is not present and the action method being called is not marked with the [Public] attribute
            else if (!filter_context.ActionDescriptor.GetCustomAttributes(typeof(Public), false).Any())
            {
                if (!Request.IsAjaxRequest()) filter_context.Result = RedirectToAction("Index", "Home", new { returnUrl = Server.UrlEncode(Request.RawUrl) });
                else filter_context.Result = Json(new ActionOutput
                {
                    Status = ActionStatus.Error,
                    Message = "Authentication Error"
                }, JsonRequestBehavior.AllowGet);
            }
            #endregion

            if (auth_cookie != null)
            {
                #region If Logged User is null
                if (LOGGEDIN_USER == null)
                {
                    FormsAuthenticationTicket auth_ticket = FormsAuthentication.Decrypt(auth_cookie.Value);
                    model = new JavaScriptSerializer().Deserialize<PermissonAndDetailModel>(auth_ticket.UserData);
                    LOGGEDIN_USER = model.UserDetails;
                    ModulesModel = model.ModulesModelList;
                    System.Web.HttpContext.Current.User = new System.Security.Principal.GenericPrincipal(new FormsIdentity(auth_ticket), null);
                }
                if (filter_context.ActionDescriptor.ActionName == "Index" && filter_context.ActionDescriptor.ControllerDescriptor.ControllerName == "Home")
                {
                    filter_context.Result = RedirectToAction("Dashboard", "Home");// new { area = "Admin" });
                }
                #endregion
                ViewBag.LOGGEDIN_USER = LOGGEDIN_USER;
                ViewBag.USER_PERMISSONS = ModulesModel;

            }



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
                ViewBag.USER_PERMISSONS = ModulesModel;

            }
            #endregion

            if (LOGGEDIN_USER != null && LOGGEDIN_USER.IsAuthenticated && LOGGEDIN_USER.LastActivityTime != null && LOGGEDIN_USER.LastActivityTime.Value.AddMinutes(minutes) < DateTime.UtcNow)
            {
                HttpCookie val = Request.Cookies[Cookies.AuthorizationCookie];
                val.Expires = DateTime.Now.AddDays(-30);
                Response.Cookies.Add(val);
                SignOut();
                LOGGEDIN_USER = null;
                JustLoggedin = false;
                filter_context.Result = RedirectToAction("Index", "Home"); 
            }

            else if (LOGGEDIN_USER != null && LOGGEDIN_USER.IsAuthenticated)
            {
                string action = filter_context.ActionDescriptor.ActionName;
                string controller = filter_context.RouteData.Values["controller"].ToString();
                if (action.ToLower() != "autologout")
                {
                    LOGGEDIN_USER.LastActivityTime = DateTime.UtcNow;
                    var ckie = new JavaScriptSerializer().Serialize(model);
                    CreateCustomAuthorisationCookie(LOGGEDIN_USER.UserName, false, ckie);
                }
            }
            //if (LOGGEDIN_USER.UserID == 0)
            //{
            //    SignOut();
            //    JustLoggedin = false;
            //    filter_context.Result = RedirectToAction("Index", "Home");
            //}

            SetActionName(filter_context.ActionDescriptor.ActionName, filter_context.ActionDescriptor.ControllerDescriptor.ControllerName);
        }

        /// <summary>
        /// this will be used to create admin user authentication cookie after login
        /// </summary>
        /// <param name="user_name"></param>
        /// <param name="is_persistent"></param>
        /// <param name="custom_data"></param>
        protected override void CreateCustomAuthorisationCookie(String user_name, Boolean is_persistent, String custom_data)
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
        /// this will be used to log out from the admin section
        /// </summary>
        /// <returns></returns>
        [HttpGet, Public]
        public override ActionResult SignOut()
        {
            HttpCookie auth_cookie = Request.Cookies[Cookies.AuthorizationCookie];
            if (auth_cookie != null)
            {
                JustLoggedin = false;
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
    }
}