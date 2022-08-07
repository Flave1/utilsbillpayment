using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using VendTech.Attributes;
using VendTech.BLL.Common;
using VendTech.BLL.Interfaces;
using VendTech.BLL.Models;

namespace VendTech.Areas.Admin.Controllers
{
    public class AppUserController : AdminBaseController
    {
        #region Variable Declaration
        private readonly IUserManager _userManager;
        private readonly IEmailTemplateManager _templateManager;
        private readonly IAgencyManager _agentManager;
        private readonly IVendorManager _vendorManager;
        private readonly IPOSManager _posManager;
        private readonly IAuthenticateManager _authenticateManager;
        #endregion

        public AppUserController(IUserManager userManager, 
            IAuthenticateManager authenticateManager, 
            IErrorLogManager errorLogManager, 
            IEmailTemplateManager templateManager, 
            IAgencyManager agentManager, 
            IVendorManager vendorManager, 
            IPOSManager posManager)
            : base(errorLogManager)
        {
            _userManager = userManager;
            _templateManager = templateManager;
            _agentManager = agentManager;
            _vendorManager = vendorManager;
            _posManager = posManager;
            _authenticateManager = authenticateManager;
        }

        #region User Management

        [HttpGet]
        public ActionResult ManageAppUsers(string status="")
        {
            ViewBag.SelectedTab = SelectedAdminTab.Users;
            var users = _userManager.GetUserPagedList(PagingModel.DefaultModel("Name", "Asc"), true,status);
            return View(users);
        }

        [AjaxOnly, HttpPost]
        public JsonResult GetAppUsersPagingList(PagingModel model)
        {
            ViewBag.SelectedTab = SelectedAdminTab.Users;
            var modal = _userManager.GetUserPagedList(model, true);
            List<string> resultString = new List<string>();
            resultString.Add(RenderRazorViewToString("Partials/_appUserListing", modal));
            resultString.Add(modal.TotalCount.ToString());
            return JsonResult(resultString);
        }

        public ActionResult AddUser()
        {
            ViewBag.UserTypes = _userManager.GetUserRolesSelectList();
            ViewBag.Vendors = _vendorManager.GetVendorsSelectList();
            ViewBag.Agencies = _agentManager.GetAgentsSelectList();
            ViewBag.Pos = _posManager.GetPOSSelectList();
            ViewBag.Roles = new List<SelectListItem> { new SelectListItem { Text = "AppUser", Value = "9" }, new SelectListItem { Text = "Vendor", Value = "17" } };
            ViewBag.SelectedTab = SelectedAdminTab.Users;
            var model = new AddUserModel();
            model.PlatformList = _userManager.GetAllPlatforms(0);
            model.ResetUserPassword = true;
            model.ModuleList = _userManager.GetAllModules(0);
            model.WidgetList = _userManager.GetAllWidgets(0);
            return View(model);
        }

        [AjaxOnly, HttpPost]
        public JsonResult AddUserDetails(AddUserModel model)
        {
            ViewBag.SelectedTab = SelectedAdminTab.Users;
            if (model.ImagefromWeb != null)
            {
                var file = model.ImagefromWeb;
                var constructorInfo = typeof(HttpPostedFile).GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance)[0];
                model.Image = (HttpPostedFile)constructorInfo
                           .Invoke(new object[] { file.FileName, file.ContentType, file.InputStream });
            }

           
            model.AgentId = 20; //20 is id for default vendtech agency ////// DO NOT CHANGE
            var result = _userManager.AddAppUserDetails(model);
            if (result.Status == ActionStatus.Successfull)
            {
                var emailTemplate = _templateManager.GetEmailTemplateByTemplateType(TemplateTypes.NewAppUserRegistration);
                if (emailTemplate.TemplateStatus)
                {
                    string body = emailTemplate.TemplateContent;
                    body = body.Replace("%USER%", model.FirstName);
                    Utilities.SendEmail(model.Email, emailTemplate.EmailSubject, body);
                }
                
            }
            return JsonResult(result);
        }

        void sendEmailToRegisteredUser(AddUserModel request, ActionOutput result)
        {
            var registered_user_password = _userManager.GetUserPasswordbyUserId(result.ID);
            var code = Utilities.GenerateRandomNo();
            //var saveToken = _authenticateManager.SaveAccountVerificationRequest(result.ID, code.ToString());
            var emailTemplate = _templateManager.GetEmailTemplateByTemplateType(TemplateTypes.NewAppUser);
            if (emailTemplate.TemplateStatus)
            {
                string body = emailTemplate.TemplateContent;
                body = body.Replace("%firstname%", request.FirstName);
                body = body.Replace("%lastname%", request.LastName);
                body = body.Replace("%code%", code.ToString());

                // new code apllied here 
                body = body.Replace("%USER%", request.FirstName);
                body = body.Replace("%UserName%", request.Email);
                body = body.Replace("%Password%", registered_user_password);
                var verifybutton = "<a style='background-color: #7bddff; color: #fff;text-decoration: none;padding: 5px 7px;border-radius: 30px;text-transform: uppercase;' href='" + WebConfigurationManager.AppSettings["BaseUrl"].ToString() + "/Admin/Home/OTPVerification/" + result.ID + "'>Verify Now</a>";

                body = body.Replace("%verifylink%", verifybutton);
                body = body.Replace("%AppLink%", WebConfigurationManager.AppSettings["AppLink"].ToString());
                body = body.Replace("%WebLink%", WebConfigurationManager.AppSettings["BaseUrl"].ToString());
                var link = "";
                var otp = Utilities.GenerateRandomNo();
                var result_ = _authenticateManager.ForgotPassword(request.Email, otp.ToString());
                if (result_.Status == ActionStatus.Successfull)
                {
                    link = "<a style='background-color: #7bddff; color: #fff;text-decoration: none;padding: 5px 7px;border-radius: 61px;text-transform: uppercase;' href='" + WebConfigurationManager.AppSettings["BaseUrl"] + "Admin/Home/ResetPassword?userId=" + result.ID + "&token=" + otp + "'>Reset Now</a>";
                }
                body = body.Replace("%passwordrestlink%", link);
                try
                {
                    Utilities.SendEmail(request.Email, emailTemplate.EmailSubject, body);
                }
                catch (Exception e)
                {
                    throw e;
                    //return Json(new ActionOutput { Status = ActionStatus.Error, Message = $"{e?.InnerException?.Message }{e?.Message} {e?.Source} {e?.StackTrace}" });
                }
            }
            
        }
        public ActionResult EditUser(long userId)
        {
            ViewBag.UserTypes = _userManager.GetUserRolesSelectList();
            ViewBag.Agencies = _agentManager.GetAgentsSelectList();
            ViewBag.SelectedTab = SelectedAdminTab.Users;
            var userModel = new AddUserModel();
            ViewBag.Roles = new List<SelectListItem> { new SelectListItem { Text = "AppUser", Value = "9" }, new SelectListItem { Text = "Vendor", Value = "17" } };
            ViewBag.Vendors = _vendorManager.GetVendorsSelectList();
            ViewBag.Pos = _posManager.GetPOSSelectList();
            userModel = _userManager.GetAppUserDetailsByUserId(userId);
            userModel.ModuleList = _userManager.GetAllModules(userId);
            userModel.PlatformList = _userManager.GetAllPlatforms(userId);
            userModel.WidgetList = _userManager.GetAllWidgets(userId);
            return View(userModel);
        }

        public ActionResult ViewUser(long userId)
        {
            ViewBag.UserTypes = _userManager.GetUserRolesSelectList();
            ViewBag.SelectedTab = SelectedAdminTab.Users;
            var userModel = new AddUserModel();
            ViewBag.Roles = new List<SelectListItem> { new SelectListItem { Text = "AppUser", Value = "9" }, new SelectListItem { Text = "Vendor", Value = "17" } };
            ViewBag.Vendors = _vendorManager.GetVendorsSelectList();
            ViewBag.Pos = _posManager.GetPOSSelectList();
            userModel = _userManager.GetAppUserDetailsByUserId(userId);
            userModel.ModuleList = _userManager.GetAllModules(userId);
            userModel.PlatformList = _userManager.GetAllPlatforms(userId);
            userModel.WidgetList = _userManager.GetAllWidgets(userId);

            return View(userModel);
        }


        [AjaxOnly, HttpPost]
        public JsonResult ReactivateUserDetails(AddUserModel model)
        { 
            ViewBag.SelectedTab = SelectedAdminTab.Users;
            if (model.ImagefromWeb != null)
            {
                var file = model.ImagefromWeb;
                var constructorInfo = typeof(HttpPostedFile).GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance)[0];
                model.Image = (HttpPostedFile)constructorInfo
                           .Invoke(new object[] { file.FileName, file.ContentType, file.InputStream });
            }
            model.IsRe_Approval = true;
            model.AgentId = 20; //20 is id for default vendtech agency ////// DO NOT CHANGE
            var result = _userManager.UpdateAppUserDetails(model);

            //send mail to activated user
            //var userEmailTemplate = _templateManager.GetEmailTemplateByTemplateType(TemplateTypes.UserAccountReactivation);
            //if(userEmailTemplate != null)
            //{
            //       string body = userEmailTemplate.TemplateContent; 
            //       body = body.Replace("%USER%", model.FirstName);  
            //       Utilities.SendEmail(model.Email, userEmailTemplate.EmailSubject, body);
            //}
            sendEmailToRegisteredUser(model, result);


            return JsonResult(result);
        }

        [AjaxOnly, HttpPost]
        public JsonResult UpdateUserDetails(AddUserModel model)
        {

            ViewBag.SelectedTab = SelectedAdminTab.Users;
            if (model.ImagefromWeb != null)
            {
                var file = model.ImagefromWeb;
                var constructorInfo = typeof(HttpPostedFile).GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance)[0];
                model.Image = (HttpPostedFile)constructorInfo
                           .Invoke(new object[] { file.FileName, file.ContentType, file.InputStream });
            }
            return JsonResult(_userManager.UpdateAppUserDetails(model));
        }

        [AjaxOnly, HttpPost]
        public JsonResult DeleteUser(long userId)
        {
            ViewBag.SelectedTab = SelectedAdminTab.Users;
            return JsonResult(_userManager.DeleteUser(userId));
        }
        [AjaxOnly, HttpPost]
        public JsonResult DeclineUser(long userId)
        {
            ViewBag.SelectedTab = SelectedAdminTab.Users;
            return JsonResult(_userManager.DeclineUser(userId));
        }
        [AjaxOnly, HttpPost]
        public JsonResult BlockUser(long userId)
        {
            ViewBag.SelectedTab = SelectedAdminTab.Users;
            return JsonResult(_userManager.ChangeUserStatus(userId, UserStatusEnum.Block));
        }
        [AjaxOnly, HttpPost]
        public JsonResult UnBlockUser(long userId)
        {
            ViewBag.SelectedTab = SelectedAdminTab.Users;
            return JsonResult(_userManager.ChangeUserStatus(userId, UserStatusEnum.Active));
        }
        #endregion
    }
}