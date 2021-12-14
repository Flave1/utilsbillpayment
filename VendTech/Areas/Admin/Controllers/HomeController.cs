#region Default Namespaces
using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
#endregion

#region Custom Namespaces
using VendTech.Attributes;
using VendTech.BLL.Models;
using VendTech.BLL.Interfaces;
using VendTech.BLL.Common;
using System.Web.Configuration;
using System.Reflection;
using VendTech.DAL;
using Newtonsoft.Json;
using System.Linq;
#endregion

namespace VendTech.Areas.Admin.Controllers
{
    public class HomeController : AdminBaseController
    {
        #region Variable Declaration
        private readonly IUserManager _userManager;
        private readonly IEmailTemplateManager _templateManager;
        private readonly ICMSManager _cmsManager;
        private readonly IAuthenticateManager _authenticateManager;
        private IDashboardManager _dashboardManager;
        private readonly IMeterManager _meterManager;
        private readonly IDepositManager _depositManager;
        #endregion


        // /Admin/Home/OTPVerification/
        public HomeController(IDepositManager depositManager, IMeterManager meterManager, IUserManager userManager, IErrorLogManager errorLogManager, IEmailTemplateManager templateManager, ICMSManager cmsManager, IAuthenticateManager authenticateManager, IDashboardManager dashboardManager)
            : base(errorLogManager)
        {
            _userManager = userManager;
            _templateManager = templateManager;
            _cmsManager = cmsManager;
            _authenticateManager = authenticateManager;
            _dashboardManager = dashboardManager;
            _meterManager = meterManager;
            _depositManager = depositManager;
        }


        [HttpGet, Public]
        public ActionResult Index()
        {
            return View(new LoginModal());
        }

        [Public]
        public ActionResult Error(string errorMessage)
        {
            ViewBag.ErrorMessage = errorMessage;
            return View();

        }

        /// <summary>
        /// This will handle user login request
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [AjaxOnly, HttpPost, Public]
        public JsonResult Login(LoginModal model)
        {
            //to do: Implement user login
            //var data = _userManager.AdminLogin(model);
            var data = _userManager.AdminLogin(model);
            if (data != null)
            {
                data.Status = ActionStatus.Successfull;
                var userId = data.Object.UserID;
                data.Object = new UserDetailForAdmin
                {
                    FirstName = data.Object.FirstName,
                    LastName = data.Object.LastName,
                    UserName = model.UserName,
                    IsAuthenticated = true,
                    UserID = userId,
                    LastActivityTime = DateTime.UtcNow,
                    UserType = data.Object.UserType,
                    ProfilePicPath = data.Object.ProfilePicPath
                };
            }
            else
            {
                data = new ActionOutput<UserDetailForAdmin>();
                data.Status = ActionStatus.Error;
                data.Message = "Invalid Credentials.";
            }
            if (data.Status == ActionStatus.Successfull)
            {
                JustLoggedin = true;
                var PermissonAndDetailModel = new PermissonAndDetailModelForAdmin();
                PermissonAndDetailModel.UserDetails = data.Object; 
                PermissonAndDetailModel.ModulesModelList = _userManager.GetAllModulesAtAuthentication(data.Object.UserID).Where(e => e.ControllerName != "19" && e.ControllerName != "20" && e.ControllerName != "1" && e.ControllerName != "23" && e.ControllerName != "18").ToList(); 
                CreateCustomAuthorisationCookie(model.UserName, false, new JavaScriptSerializer().Serialize(PermissonAndDetailModel));
            }
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Dashboard Page
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult Dashboard()
        {
            if (LOGGEDIN_USER?.UserID == 0 || LOGGEDIN_USER == null)
            {
                SignOut();
                return RedirectToAction("Index", "Home", new { area = "Admin" });
            }
            var model = new List<PlatformModel>();
            model = _platformManager.GetUserAssignedPlatforms(LOGGEDIN_USER.UserID);

            DashboardViewModel dashBoard = new DashboardViewModel();
            dashBoard = _dashboardManager.getDashboardData(LOGGEDIN_USER.UserID);
            dashBoard.platFormModels = model;
            dashBoard.currentUser = _userManager.GetUserDetailsByUserId(LOGGEDIN_USER.UserID);
            return View(dashBoard);
        }
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
                var link = "<a style='background-color: #7bddff; color: #fff;text-decoration: none;padding: 10px 20px;border-radius: 30px;text-transform: uppercase;' href='" + WebConfigurationManager.AppSettings["BaseUrl"] + "Admin/Home/ResetPassword?userId=" + result.ID + "&token=" + otp + "'>Reset Now</a>";
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


        [HttpPost]
        [Public]
        public ActionResult ResetPassword(ResetPasswordModel model)
        {
            if (!ModelState.IsValid)
                return View(model);
            ViewBag.Message = "";

            if (_authenticateManager.ResetPassword(model).Status == ActionStatus.Successfull)

                ViewBag.Message = "Password reset successfully.";
            return View(model);
        }


        [HttpGet]
        [Public]
        public ActionResult OTPVerification(long ID)
        {
            ViewBag.Message = "";
            var data = new VerifyAccountVerificationCodeMOdel();
            var userdetails = _userManager.GetUserDetailsByUserId(ID);
            if (userdetails != null)
            {
                data.UserId = userdetails.UserId;
                if (userdetails.isemailverified == true)
                {
                    ViewBag.Message = "AlreadyVerified";
                }
            }
            else
            {
                data.UserId = 0;
            }
            return View(data);
        }


        [HttpPost]
        [Public]
        public ActionResult OTPVerification(VerifyAccountVerificationCodeMOdel model)
        {
            ViewBag.Message = "";
            if (!ModelState.IsValid)
                return View(model);
            var result = _authenticateManager.VerifyAccountVerificationCode(model);
            if (result.Status == ActionStatus.Successfull)
            {
                ViewBag.Message = "verified";
            }
            else
            {
                ViewBag.Message = "failed";
            }
            return View(model);
        }

        public ActionResult EditProfile()
        {
            ViewBag.SelectedTab = SelectedAdminTab.Profile;

            var userDetails = _userManager.GetAppUserProfile(LOGGEDIN_USER.UserID);
            var countries = _authenticateManager.GetCountries();
            var countryDrpData = new List<SelectListItem>();

            foreach (var item in countries)
            {
                countryDrpData.Add(new SelectListItem { Text = item.Name, Value = item.CountryId.ToString() });
            }
            ViewBag.countries = countryDrpData;
            ViewBag.Cities = _authenticateManager.GetCities();
            return View(userDetails);
        }

        [AjaxOnly, HttpPost]
        public ActionResult EditProfile(UpdateProfileModel model)
        {
            if (!ModelState.IsValid)
                return View(model);
            ViewBag.SelectedTab = SelectedAdminTab.Profile;
            if (model.ImagefromWeb != null)
            {
                var file = model.ImagefromWeb;
                var constructorInfo = typeof(HttpPostedFile).GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance)[0];
                model.Image = (HttpPostedFile)constructorInfo
                           .Invoke(new object[] { file.FileName, file.ContentType, file.InputStream });
            }
            return JsonResult(_userManager.UpdateUserProfile(LOGGEDIN_USER.UserID, model));
        }

        public ActionResult ChangePassword()
        {
            ViewBag.SelectedTab = SelectedAdminTab.Profile;
            var model = new ChangePasswordModel();
            return View(model);
        }
        [AjaxOnly, HttpPost]
        public JsonResult ChangePassword(ChangePasswordModel model)
        {
            model.UserId = LOGGEDIN_USER.UserID;
            return JsonResult(_authenticateManager.ChangePassword(model));
        }
        [Public]
        public ActionResult TermsAndConditions()
        {
            CMSPageViewModel model = _cmsManager.GetPageContentByPageIdforFront(1);
            return View(model);
        }

        [Public]
        public ActionResult AccesDeniedPage()
        {
            return View();
        }

        [Public]
        public ActionResult PrivacyPolicy()
        {
            CMSPageViewModel model = _cmsManager.GetPageContentByPageIdforFront(6);
            return View(model);
        }
        [Public]
        public ActionResult ContactUs()
        {
            CMSPageViewModel model = _cmsManager.GetPageContentByPageIdforFront(2);
            return View(model);
        }
        [Public]
        public ActionResult AboutUs()
        {
            CMSPageViewModel model = _cmsManager.GetPageContentByPageIdforFront(3);
            return View(model);
        }
        [Public]
        public ActionResult FAQ()
        {
            CMSPageViewModel model = _cmsManager.GetPageContentByPageIdforFront(5);
            return View(model);
        }
        [HttpPost]
        public JsonResult SaveLogoutTime(SaveLogoutTimeModel model)
        {
            return JsonResult(_authenticateManager.SaveLogoutTime(model));
        }
        [HttpGet, Public]
        public JsonResult AutoLogout()
        {
            var minutes = _authenticateManager.GetLogoutTime();
            bool result = true;
            if (LOGGEDIN_USER == null || LOGGEDIN_USER.UserID == 0)
                result = false;
            else if (LOGGEDIN_USER != null && LOGGEDIN_USER.LastActivityTime.Value.AddMinutes(minutes) < DateTime.UtcNow)
                result = false;
            return JsonResult(new ActionOutput { Status = result ? ActionStatus.Successfull : ActionStatus.Error });
        }
         

        [HttpGet, Public]
        public JsonResult ReturnDealerBalance()
        {
            try
            {
                var lastTransaction = _meterManager.GetLastTransaction();
                
                var result = new LastMeterTransaction
                { 
                    LastDealerBalance = string.Format("{0:N0}", lastTransaction.CurrentDealerBalance),
                    RequestDate = lastTransaction.RequestDate.ToString("dd/MM/yyyy"),  
                }; 

                return Json(new { result = result }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return null;
            }
            
        }

        [HttpGet, Public]
        public JsonResult CheckForUnClearedDeposits()
        {
            try
            {
                var uncleardDeposits = _depositManager.GetUnclearedDeposits();

                if (uncleardDeposits.Any())
                {
                    var emailTemplate = _templateManager.GetEmailTemplateByTemplateType(TemplateTypes.UnclearedDepositNotification);
                    if (emailTemplate.TemplateStatus)
                    {
                        foreach(var deposit in uncleardDeposits)
                        {
                            string body = emailTemplate.TemplateContent;
                            body = body.Replace("%USER%", deposit.POS.User.Name);
                            body = body.Replace("%AMOUNT%", deposit.Amount.ToString());
                            body = body.Replace("%DEPOSITAPPROVEDDATE%", deposit.DepositLogs.FirstOrDefault()?.CreatedAt.ToString("f"));
                            body = body.Replace("%TODAY%", DateTime.UtcNow.ToString("f"));
                            Utilities.SendEmail("vblell@gmail.com", emailTemplate.EmailSubject, body);
                            _depositManager.UpdateNextReminderDate(deposit);
                        }
                    }
                }
                return Json(new { result = "success" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return null;
            }

        }
    }
}