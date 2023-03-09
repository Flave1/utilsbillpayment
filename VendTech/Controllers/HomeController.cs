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
using VendTech.Areas.Admin.Controllers;
using System.Threading.Tasks;
using System.Web.Http.Results;
using VendTech.DAL;
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
        private readonly ISMSManager _smsManager;


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
            IDashboardManager dashboardManager, 
            ISMSManager smsManager)
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
            _smsManager = smsManager;
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

        [Public]
        public ActionResult Error(string errorMessage)
        {
            ViewBag.ErrorMessage = errorMessage;
            return View();

        }

        [AjaxOnly, HttpPost, Public]
        public JsonResult Login(LoginAPIModel model)
        {
            //to do: Implement user login
            //var data = _userManager.AdminLogin(model);
            var data = new ActionOutput<UserDetails>();
            if (!_authenticateManager.IsUserAccountActive(model.Email, model.Password))
            {
                return Json(new ActionOutput { Message = "ACCOUNT BLOCKED <br/>Contact <br/><br/> VENDTECH MANAGEMENT <br/> 232 79 990990", Status = ActionStatus.Error }, JsonRequestBehavior.AllowGet);
            }
            if (_authenticateManager.IsUserPosEnabled(model.Email, model.Password))
            {
                return Json(new ActionOutput { Message = "POS IS DISABLED <br/>Contact <br/><br/> VENDTECH MANAGEMENT <br/> 232 79 990990", Status = ActionStatus.Error }, JsonRequestBehavior.AllowGet);
            }
            var userDetails = _authenticateManager.GetDetailsbyUser(model.Email, model.Password);
            if (userDetails != null)
            {

                data.Status = ActionStatus.Successfull;
                var userId = userDetails.UserId;
                data.Object = new UserDetails
                {
                    FirstName = userDetails.IsCompany ? userDetails.CompanyName : userDetails.FirstName,
                    LastName = userDetails.IsCompany ? "" : userDetails.LastName,
                    UserName = userDetails.Email,
                    ProfilePicPath = userDetails.ProfilePicUrl,
                    IsAuthenticated = true,
                    UserID = userId,
                    AgencyId = userDetails?.AgentId??0,
                    LastActivityTime = DateTime.UtcNow,
                    UserType = UserRoles.AppUser,
                    IsEmailVerified = userDetails.isemailverified,
                    Status = userDetails.Status,
                    VendorName = userDetails.Vendor,
                    AgencyName = userDetails.AgencyName,
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
                JustLoggedin = true;

                if (!userDetails.isemailverified)
                {
                    data = new ActionOutput<UserDetails>();
                    data.Status = ActionStatus.Successfull;
                    data.Message = "emailNotVerified";
                    data.Results = new List<string>();
                    data.Results.Add(userDetails.UserId.ToString());
                    return Json(data, JsonRequestBehavior.AllowGet);
                } 
                var PermissonAndDetailModel = new PermissonAndDetailModel();
                PermissonAndDetailModel.UserDetails = data.Object;
                PermissonAndDetailModel.ModulesModelList = _userManager.GetAllModulesAtAuthentication(data.Object.UserID);
                CreateCustomAuthorisationCookie(data.Object.UserName, false, new JavaScriptSerializer().Serialize(PermissonAndDetailModel));


            }
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        private void SendEmailOnLogin()
        {
            var emailTemplate = _templateManager.GetEmailTemplateByTemplateType(TemplateTypes.NewAppUser);
            if (emailTemplate.TemplateStatus)
            {
                string body = emailTemplate.TemplateContent;
                body = body.Replace("%PASSWORD%", "Flave");
                body = body.Replace("%USER%", "Flave");
                body = body.Replace("%POSID%", "Flave");
                string to = "favouremmanuel433@gmail.com";
                Utilities.SendEmail(to, emailTemplate.EmailSubject, body);
            }
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
            DashboardViewModel dashBoard = new DashboardViewModel();
            if (LOGGEDIN_USER.UserID == 0 || LOGGEDIN_USER == null)
            {
                SignOut();
                return RedirectToAction("Index", "Home");
            }
            ViewBag.walletBalance = _userManager.GetUserWalletBalance(LOGGEDIN_USER.UserID, LOGGEDIN_USER.AgencyId);
            ViewBag.Pos = _userManager.GetUserDetailsByUserId(LOGGEDIN_USER?.UserID ?? 0)?.POSNumber;
            ViewBag.ShowRevenueWidg = _dashboardManager.IsUserAnAgent(LOGGEDIN_USER.UserID);
            var model = new List<PlatformModel>();
            model = _platformManager.GetUserAssignedPlatforms(LOGGEDIN_USER.UserID);

            dashBoard.currentUser = new UserModel();
            dashBoard = _dashboardManager.getDashboardData(LOGGEDIN_USER.UserID, LOGGEDIN_USER.AgencyId);
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
        public async Task<JsonResult> ForgotPassword(string email)
        {
            if (!string.IsNullOrEmpty(email))
            {
                //var otp = Utilities.GenerateRandomNo();
                var result = _authenticateManager.GenerateNewPassword(email);
                if (result.Status == ActionStatus.Error)
                    return JsonResult(result);
                //var link = "<a style='background-color: #7bddff; color: #fff;text-decoration: none;padding: 10px 20px;border-radius: 30px;text-transform: uppercase;' href='" + WebConfigurationManager.AppSettings["BaseUrl"] + "Home/ResetPassword?userId=" + result.ID + "&token=" + otp + "'>Reset Now</a>";
               
                var user = _userManager.GetUserDetailByEmail(email);
                if (user != null)
                {
                    var emailTemplate = _templateManager.GetEmailTemplateByTemplateType(TemplateTypes.ForgetPassword);
                    if (emailTemplate.TemplateStatus)
                    {
                        string body = emailTemplate.TemplateContent;
                        body = body.Replace("%PASSWORD%", result.Message);
                        body = body.Replace("%USER%", user.Name);
                        body = body.Replace("%POSID%", user.POS.FirstOrDefault().SerialNumber);
                        string to = email;
                        Utilities.SendEmail(to, emailTemplate.EmailSubject, body);
                    }

                    var msg = new SendSMSRequest
                    {
                        Recipient = "232" + user.Phone,
                        Payload = $"Greetings {user.Name} \n" +
                          $"Please use this temporal password to login. {result.Message}\n" +
                          "VENDTECH"
                    };
                    await _smsManager.SendSmsAsync(msg);
                }

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
        public ActionResult FirstTimeLoginChangePassword(long userId = 0)
        {

            ViewBag.SelectedTab = SelectedAdminTab.Users;
            ViewBag.IsFirstTimeLogin = true; ;
            var model = new ResetPasswordModel();
            model.UserId = userId;
            return View(model);
        }

        [AjaxOnly, HttpPost, Public]
        public JsonResult FirstTimeLoginChangePassword(ResetPasswordModel model)
        { 
            var data = new ActionOutput<UserDetails>();

            var result = _authenticateManager.FirstTimeLoginChangePassword(model.UserId, model.OldPassword, model.Password);

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
            CMSPageViewModel model = _cmsManager.GetPageContentByPageIdforFront(2);
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
                return Json(new ActionOutput { Status = ActionStatus.Error, Message = "Name must not be empty" });
            }
            request.Agency = "20"; //20 is id for default vendtech agency ////// DO NOT CHANGE
            var result = _userManager.AddAppUserDetails(request);
            if (result.Status == ActionStatus.Successfull)
            { 
                sendEmailToRegisteredUser(request); 
                sendEmailToAdminUser(request); 
            }
            return JsonResult(result);
        }

        void sendEmailToAdminUser(RegisterAPIModel request)
        {
            var adminUser = _userManager.GetAllAdminUsersByAppUserPermission();
            foreach(var admin in adminUser)
            {
                var emailTemplate = _templateManager.GetEmailTemplateByTemplateType(TemplateTypes.NewUserEmailToAdmin);
                if (emailTemplate.TemplateStatus)
                {
                    string body = emailTemplate.TemplateContent;
                    body = body.Replace("%AdminUserName%", admin.Name);
                    body = body.Replace("%Name%", request.FirstName);
                    body = body.Replace("%Surname%", request.LastName);
                    body = body.Replace("%Vendor%", request.IsCompany ? request.CompanyName : request.FirstName + " " + request.LastName);
                    Utilities.SendEmail(admin.Email, emailTemplate.EmailSubject, body);
                }
                
            }        
        }
        void sendEmailToRegisteredUser(RegisterAPIModel request)
        {
            var emailTemplate = _templateManager.GetEmailTemplateByTemplateType(TemplateTypes.NewAppUserRegistration);
            string body = emailTemplate.TemplateContent;
            body = body.Replace("%USER%", request.FirstName);
            Utilities.SendEmail(request.Email, emailTemplate.EmailSubject, body);
        } 
    }
}