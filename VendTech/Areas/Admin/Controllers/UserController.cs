using System.Collections.Generic;
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
    public class UserController : AdminBaseV2Controller
    {
        #region Variable Declaration
        private readonly IUserManager _userManager;
        private readonly IEmailTemplateManager _templateManager;
        #endregion

        public UserController(IUserManager userManager, IErrorLogManager errorLogManager, IEmailTemplateManager templateManager)
            : base(errorLogManager)
        {
            _userManager = userManager;
            _templateManager = templateManager;
        }

        #region User Management

        [HttpGet]
        public ActionResult ManageUsers()
        {
            ViewBag.SelectedTab = SelectedAdminTab.Users;
            var users = _userManager.GetUserPagedList(PagingModel.DefaultModel("CreatedAt", "Desc"));
            return View(users);
        }

        [AjaxOnly, HttpPost]
        public JsonResult GetUsersPagingList(PagingModel model)
        {
            ViewBag.SelectedTab = SelectedAdminTab.Users;
            var modal = _userManager.GetUserPagedList(model);
            List<string> resultString = new List<string>();
            resultString.Add(RenderRazorViewToString("Partials/_userListing", modal));
            resultString.Add(modal.TotalCount.ToString());
            return JsonResult(resultString);
        }

        public ActionResult AddUser()
        {
            ViewBag.UserTypes = _userManager.GetUserRolesSelectList();
            ViewBag.SelectedTab = SelectedAdminTab.Users;
            var model = new AddUserModel();
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
            var result = _userManager.AddUserDetails(model);
            if (result.Status == ActionStatus.Successfull)
            {
                var emailTemplate = _templateManager.GetEmailTemplateByTemplateType(TemplateTypes.NewCMSUser);
                string body = emailTemplate.TemplateContent;
                body = body.Replace("%UserName%", model.Email);
                body = body.Replace("%Password%", model.Password);
                body = body.Replace("%WebLink%", WebConfigurationManager.AppSettings["BaseUrl"].ToString() + "Admin");
                Utilities.SendEmail(model.Email, emailTemplate.EmailSubject, body);
            }
            return JsonResult(result);
        }

        public ActionResult EditUser(long userId)
        {
            ViewBag.UserTypes = _userManager.GetUserRolesSelectList();
            ViewBag.SelectedTab = SelectedAdminTab.Users;
            var userModel = new AddUserModel();
            userModel = _userManager.GetAppUserDetailsByUserId(userId);
            userModel.ModuleList = _userManager.GetAllModules(userId);
            userModel.PlatformList = _userManager.GetAllPlatforms(userId);
            userModel.WidgetList = _userManager.GetAllWidgets(userId);
            return View(userModel);
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
            return JsonResult(_userManager.UpdateUserDetails(model));
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

        [AjaxOnly, HttpPost]
        public JsonResult GetVendorName(int posId)
        {
            return Json(_userManager.GetVendorNamePOSNumber(posId));
        }
        #endregion
    }
}