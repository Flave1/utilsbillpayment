#region Default Namespaces
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
#endregion

#region Custom Namespaces
using VendTech.Attributes;
using VendTech.BLL.Interfaces;
using Ninject;
using VendTech.BLL.Models;
using System.Web.Script.Serialization;
#endregion

namespace VendTech.Areas.Agent.Controllers
{
    /// <summary>
    /// Home Controller 
    /// Created On: 10/04/2015
    /// </summary>
    public class HomeController : BaseAgentController
    {
        #region Variable Declaration
        private readonly IUserManager _userManager;
    
        #endregion

        public HomeController(IUserManager userManager, IErrorLogManager errorLogManager)
            : base(errorLogManager)
        {
            _userManager = userManager;
          
        }

        /// <summary>
        /// Index View 
        /// </summary>
        /// <returns></returns>
        [HttpGet, Public]
        public ActionResult Index()
        {
            return View(new LoginModal());
        }
        [AjaxOnly, HttpPost, Public]
        public JsonResult Login(LoginModal model)
        {
            //to do: Implement user login
            //var data = _userManager.AdminLogin(model);
            var data = _userManager.AgentLogin(model);
            if (data != null)
            {
                data.Status = ActionStatus.Successfull;
                var userId = data.Object.UserID;
                data.Object = new UserDetails
                {
                    FirstName = model.UserName,
                    UserName = model.UserName,
                    IsAuthenticated = true,
                    UserID = userId,
                    LastActivityTime=DateTime.UtcNow
                };
            }
            else
            {
                data = new ActionOutput<UserDetails>();
                data.Status = ActionStatus.Error;
                data.Message = "Invalid Credentials.";
            }
            if (data.Status == ActionStatus.Successfull)
            {
                var user_data = data.Object;
                CreateAgentAuthorisationCookie(model.UserName, false, new JavaScriptSerializer().Serialize(user_data));
            }
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// About Us Page
        /// </summary>
        /// <returns></returns>
        [Public]
        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        /// <summary>
        /// Contact Us Page
        /// </summary>
        /// <returns></returns>
        [Public]
        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}