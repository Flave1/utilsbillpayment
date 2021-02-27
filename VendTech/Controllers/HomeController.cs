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
using VendTech.BLL.Common;
using System.Web.Configuration;
#endregion

namespace VendTech.Controllers
{
    /// <summary>
    /// Home Controller 
    /// Created On: 10/04/2015
    /// </summary>
    public class HomeController : AppUserBaseController
    {
        #region Variable Declaration
        //private readonly IUserManager _userManager;
        private readonly IAuthenticateManager _authenticateManager;
        private readonly ICMSManager _cmsManager;
        private readonly IEmailTemplateManager _templateManager;
      //  private readonly IPlatformManager _platformManager;
        private readonly IVendorManager _vendorManager;
        private readonly IDashboardManager _dashboardManager;
        private readonly IAgencyManager _agentManager;


        #endregion

        public HomeController(
            IUserManager userManager, 
            IErrorLogManager errorLogManager,
            IAuthenticateManager authenticateManager,
            ICMSManager cmsManager,
            IPlatformManager platformManager,
            IEmailTemplateManager templateManager,
            IVendorManager vendorManager,
            IAgencyManager agencyManager,
            IDashboardManager dashboardManager)
            : base(errorLogManager)
        {
            _agentManager = agencyManager;
            _userManager = userManager;
            _authenticateManager = authenticateManager;
            _templateManager = templateManager;
            _cmsManager = cmsManager;
            _platformManager = platformManager;
            _vendorManager = vendorManager;
            _dashboardManager = dashboardManager;
        }

        /// <summary>
        /// Index View 
        /// </summary>
        /// <returns></returns>
        [Public]
        public ActionResult Index()
        {
            return View(new LoginAPIModel());

        }
        [AjaxOnly, HttpPost, Public]
        public JsonResult Login(LoginAPIModel model)
        {
            //to do: Implement user login
            //var data = _userManager.AdminLogin(model);
        var   data = new ActionOutput<UserDetails>();
        if (!_authenticateManager.IsUserAccountActive(model.Email, model.Password))
        {
            return Json(new ActionOutput { Message = "ACCOUNT BLOCKED <br/>Contact <br/><br/> VENDTECH MANAGEMENT <br/> 232 79 990990", Status = ActionStatus.Error }, JsonRequestBehavior.AllowGet);
        }
            var userDetails = _authenticateManager.GetDetailsbyUser(model.Email, model.Password);
            if (userDetails != null)
            {
                
                data.Status = ActionStatus.Successfull;
                var userId = userDetails.UserId;
                data.Object = new UserDetails
                {
                    FirstName = userDetails.FirstName,
                    LastName = userDetails.LastName,
                    UserName = userDetails.Email,
                    ProfilePicPath = userDetails.ProfilePicUrl,
                    IsAuthenticated = true,
                    UserID = userId,
                    LastActivityTime = DateTime.UtcNow,
                    UserType = UserRoles.AppUser,
                    IsEmailVerified = userDetails.isemailverified,
                    Status = userDetails.Status
                };
            }
            else
            {
                data = new ActionOutput<UserDetails>();
                data.Status = ActionStatus.Error;
                data.Message = "Invalid Credentials.";
            }
            if (data!=null && data.Object!=null)
            {  
                var PermissonAndDetailModel = new PermissonAndDetailModel();
                PermissonAndDetailModel.UserDetails = data.Object;
                PermissonAndDetailModel.ModulesModelList = _userManager.GetAllModulesAtAuthentication(data.Object.UserID);
                CreateCustomAuthorisationCookie(model.Email, false, new JavaScriptSerializer().Serialize(PermissonAndDetailModel));
                //CreateCustomAuthorisationCookie(model.Email, false, new JavaScriptSerializer().Serialize(data.Object));
                if (!userDetails.isemailverified)
                {
                    data = new ActionOutput<UserDetails>();
                    data.Status = ActionStatus.Successfull;
                    data.Message = "emailNotVerified";
                    return Json(data, JsonRequestBehavior.AllowGet);
                }
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
        [HttpGet]
        public ActionResult Dashboard()
        {
            if (LOGGEDIN_USER.UserID == 0)
            { 
                SignOut(); 
            }
            ViewBag.walletBalance = _userManager.GetUserWalletBalance(LOGGEDIN_USER.UserID);
            ViewBag.Pos = _userManager.GetUserDetailsByUserId(LOGGEDIN_USER?.UserID??0)?.POSNumber;
            var model = new List<PlatformModel>();
            model = _platformManager.GetUserAssignedPlatforms(LOGGEDIN_USER.UserID);

            DashboardViewModel dashBoard = new DashboardViewModel();
            dashBoard.currentUser = new UserModel();
            dashBoard = _dashboardManager.getDashboardData(LOGGEDIN_USER.UserID);
            dashBoard.platFormModels = model;

            dashBoard.currentUser = _userManager.GetUserDetailsByUserId(LOGGEDIN_USER.UserID);
            return View(dashBoard);
        }
        /// <summary>
        /// Contact Us Page
        /// </summary>
        /// <returns></returns>
        [Public]
        public ActionResult ForgotPassword()
        {
            return View();
        }
        [HttpPost, Public]
        public JsonResult ForgotPassword(string email)
        {
            if (!string.IsNullOrEmpty(email))
            {
                var otp = Utilities.GenerateRandomNo();
                var result = _authenticateManager.ForgotPassword(email, otp.ToString());
                if (result.Status == ActionStatus.Error)
                    return JsonResult(result);
                var link = "<a style='background-color: #7bddff; color: #fff;text-decoration: none;padding: 10px 20px;border-radius: 30px;text-transform: uppercase;' href='" + WebConfigurationManager.AppSettings["BaseUrl"] + "Home/ResetPassword?userId=" + result.ID + "&token=" + otp + "'>Reset Now</a>";
                var emailTemplate = _templateManager.GetEmailTemplateByTemplateType(TemplateTypes.ForgetPassword);
                string body = emailTemplate.TemplateContent;
                body = body.Replace("%link%", link);
                string to = email;
                Utilities.SendEmail(to, emailTemplate.EmailSubject, body);
                return JsonResult(result);
            }
            return JsonResult(new ActionOutput { Message = "Email is required", Status = ActionStatus.Error });
        }
        [Public]
        public ActionResult ResetPassword(long userId, string token)
        {
            var model = new ResetPasswordModel();
            if (_authenticateManager.IsValidForgotRequest(userId, token))
                model.UserId = userId;
            else
                ViewBag.Message = "Invalid Token";
            return View(model);
        }
        [Public]
        public ActionResult ChangePassword()
        {

            ViewBag.SelectedTab = SelectedAdminTab.Users; 
            var model = new ResetPasswordModel();
            return View(model);
        }
       
        [AjaxOnly, HttpPost]
        public JsonResult ChangePassword(ResetPasswordModel model)
        {
            model.UserId = LOGGEDIN_USER.UserID;
            var code = Utilities.GenerateRandomNo();
            var result = _authenticateManager.SaveChangePasswordOTP(LOGGEDIN_USER.UserID, model.OldPassword, code.ToString());

            if (result.Status == ActionStatus.Successfull)
            {
                var emailTemplate = _templateManager.GetEmailTemplateByTemplateType(TemplateTypes.ChangePassword);
                string body = emailTemplate.TemplateContent;
                body = body.Replace("%OTP%", code.ToString());
                string to = result.Object;
                Utilities.SendEmail(to, emailTemplate.EmailSubject, body);
            }
            return JsonResult(new ActionOutput { Status = result.Status, Message = result.Message }); 
        }


        [Public]
        public ActionResult FirstTimeLoginChangePassword()
        {

            ViewBag.SelectedTab = SelectedAdminTab.Users;
            ViewBag.IsFirstTimeLogin = true; ;
            var model = new ResetPasswordModel();
            return View(model);
        }

        [AjaxOnly, HttpPost]
        public JsonResult FirstTimeLoginChangePassword(ResetPasswordModel model)
        {
            model.UserId = LOGGEDIN_USER.UserID;
            var data = new ActionOutput<UserDetails>();

            var result = _authenticateManager.FirstTimeLoginChangePassword(LOGGEDIN_USER.UserID, model.OldPassword, model.Password);

            if (result.Status != ActionStatus.Successfull)
                return JsonResult(new ActionOutput { Status = result.Status, Message = result.Message });


            var userDetails = _authenticateManager.GetDetailsbyUser(LOGGEDIN_USER.UserName, model.Password);
            if (userDetails != null)
            { 
                data.Status = ActionStatus.Successfull;
                var userId = userDetails.UserId;
                data.Object = new UserDetails
                {
                    FirstName = userDetails.FirstName,
                    LastName = userDetails.LastName,
                    UserName = userDetails.Email,
                    ProfilePicPath = userDetails.ProfilePicUrl,
                    IsAuthenticated = true,
                    UserID = userId,
                    LastActivityTime = DateTime.UtcNow,
                    UserType = UserRoles.AppUser,
                    IsEmailVerified = userDetails.isemailverified,
                };
            }
            else
            {
                data = new ActionOutput<UserDetails>();
                data.Status = ActionStatus.Error;
                data.Message = "Invalid Credentials.";
            }
            if (data != null && data.Object != null)
            {
                var PermissonAndDetailModel = new PermissonAndDetailModel();
                PermissonAndDetailModel.UserDetails = data.Object;
                PermissonAndDetailModel.ModulesModelList = _userManager.GetAllModulesAtAuthentication(data.Object.UserID);
                CreateCustomAuthorisationCookie(userDetails.Email, false, new JavaScriptSerializer().Serialize(PermissonAndDetailModel)); 
            }
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        [AjaxOnly, HttpPost]
        public JsonResult VerifyChangePasswordOTP(ResetPasswordModel model)
        {
            model.UserId = LOGGEDIN_USER.UserID;
            var result = _authenticateManager.VerifyChangePasswordOTP(model);
            return JsonResult(result);
        }
        public ActionResult PrivacyPolicy()
        {
            CMSPageViewModel model = _cmsManager.GetPageContentByPageIdforFront(6);
            return View(model);
        }

        public ActionResult TermsAndConditions()
        {
            CMSPageViewModel model = _cmsManager.GetPageContentByPageIdforFront(1);
            return View(model);
        }
        [HttpGet, Public]
        public JsonResult AutoLogout()
        {
            var secs = _authenticateManager.GetLogoutTime();
            bool result = true;
            if (LOGGEDIN_USER == null || LOGGEDIN_USER.UserID == 0)
                result = false;
            else if (LOGGEDIN_USER != null && LOGGEDIN_USER.LastActivityTime.Value.AddSeconds(secs) < DateTime.UtcNow)
                result = false;
            return JsonResult(new ActionOutput { Status = result ? ActionStatus.Successfull : ActionStatus.Error });
        }
   
        [Public]
        public ActionResult Register()
        {
            var countries = _authenticateManager.GetCountries();
            var countryDrpData = new List<SelectListItem>();

            ViewBag.Agencies = _agentManager.GetAgentsSelectList();

            foreach (var item in countries)
            {
                countryDrpData.Add(new SelectListItem { Text = item.Name, Value = item.CountryId.ToString() });
            }
            ViewBag.countries = countryDrpData;
            ViewBag.Cities = _authenticateManager.GetCities();

            return View(new RegisterAPIModel());
        }

        [AjaxOnly, Public, HttpPost]
        public JsonResult Submit_new_user_details(RegisterAPIModel request)
        { 
            if (string.IsNullOrEmpty(request.FirstName) && string.IsNullOrEmpty(request.CompanyName))
            {
                return Json(new ActionOutput { Status = ActionStatus.Error,  Message = "Name must not be empty" });
            }
            var result = _userManager.AddAppUserDetails(request);
            if (result.Status == ActionStatus.Successfull)
            {
                var registered_user_password = _userManager.GetUserPasswordbyUserId(result.ID);
                var code = Utilities.GenerateRandomNo();
                var saveToken = _authenticateManager.SaveAccountVerificationRequest(result.ID, code.ToString());
                var emailTemplate = _templateManager.GetEmailTemplateByTemplateType(TemplateTypes.AccountVerification);
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
                

                //password sent 
                //var emailTemplate = _templateManager.GetEmailTemplateByTemplateType(TemplateTypes.NewAppUser);
                //string body = emailTemplate.TemplateContent;
                //body = body.Replace("%UserName%", model.Email);
                //body = body.Replace("%Password%", model.Password);
                //body = body.Replace("%AppLink%", WebConfigurationManager.AppSettings["AppLink"].ToString());
                //body = body.Replace("%WebLink%", WebConfigurationManager.AppSettings["BaseUrl"].ToString());
                //Utilities.SendEmail(model.Email, emailTemplate.EmailSubject, body);
            }
            return JsonResult(result);
        }
    }
}