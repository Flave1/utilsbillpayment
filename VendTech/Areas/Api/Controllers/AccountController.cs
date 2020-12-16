using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Configuration;
using System.Web.Http;
using System.Web.Http.Description;
using System.Web.WebPages.Html;
using VendTech.Attributes;
using VendTech.BLL.Common;
using VendTech.BLL.Interfaces;
using VendTech.BLL.Models;
using VendTech.Framework.Api;

namespace VendTech.Areas.Api.Controllers
{
    public class AccountController : BaseAPIController
    {
        private readonly IUserManager _userManager;
        private readonly IAuthenticateManager _authenticateManager;
        private readonly IEmailTemplateManager _templateManager;
        private readonly IVendorManager _vendorManager;
        private readonly ICMSManager _cmsManager;
        private readonly IBankAccountManager _bankAccountManager;
        public AccountController(IUserManager userManager,IBankAccountManager bankAccountManager, IErrorLogManager errorLogManager, IEmailTemplateManager templateManager, IAuthenticateManager authenticateManager, ICMSManager cmsManager,IVendorManager vendorManager)
            : base(errorLogManager)
        {
            _userManager = userManager;
            _authenticateManager = authenticateManager;
            _templateManager = templateManager;
            _vendorManager = vendorManager;
            _cmsManager = cmsManager;
            _bankAccountManager = bankAccountManager;
        }
        [HttpPost, CheckAuthorizationAttribute.SkipAuthentication, CheckAuthorizationAttribute.SkipAuthorization]
        [ResponseType(typeof(ResponseBase))]
        public HttpResponseMessage Test()
        {
            var aa = _userManager.GetWelcomeMessage();
            return new JsonContent("OTP sent successfully", Status.Success).ConvertToHttpResponseOK();
        }

        [HttpPost, CheckAuthorizationAttribute.SkipAuthentication, CheckAuthorizationAttribute.SkipAuthorization]
        [ResponseType(typeof(ResponseBase))]
        [ActionName("SignIn")]
        public HttpResponseMessage SignIn(LoginAPIModel model)
        {
            if (!ModelState.IsValid)
                return new JsonContent("Email and password is required.", Status.Failed).ConvertToHttpResponseOK();
            else
            {
                //if (!_authenticateManager.IsEmailExist(model.Email))
                //    return new JsonContent("Email is not registered with us.", Status.Failed).ConvertToHttpResponseOK();


                var userDetails = _authenticateManager.GetDetailsbyUser(model.Email, model.Password);
                if (userDetails == null)
                    return new JsonContent("Invalid Credentials.", Status.Failed).ConvertToHttpResponseOK();
                else
                {
                    userDetails.Percentage = _vendorManager.GetVendorPercentage(userDetails.UserId);
                    _authenticateManager.AddTokenDevice(model);
                    if (_authenticateManager.IsTokenAlreadyExists(userDetails.UserId))
                    {
                        _authenticateManager.DeleteGenerateToken(userDetails.UserId);
                        return GenerateandSaveToken(userDetails,model);
                    }
                    else
                    return GenerateandSaveToken(userDetails, model);
                    //var code = Utilities.GenerateRandomNumber();
                    ////Send login code on Email
                    //var saveToken = _authenticateManager.SaveLoginCode(userDetails.UserId, code);
                    //var emailTemplate = _templateManager.GetEmailTemplateByTemplateType(TemplateTypes.LoginCodeEmail);
                    //string body = emailTemplate.TemplateContent;
                    //body = body.Replace("%code%", code.ToString());
                    //Utilities.SendEmail(model.Email, emailTemplate.EmailSubject, body);
                    ////_authenticateManager.FirstTimeLogin(userDetails.UserId);
                    //return new JsonContent("Login code sent to your email.", Status.Success, userDetails).ConvertToHttpResponseOK();


                }
            }
        }
        [HttpPost]

        public HttpResponseMessage Logout()
        {
            var token = Request.Headers.GetValues("Token").FirstOrDefault();
            var res = _authenticateManager.Logout(LOGGEDIN_USER.UserId, token);
            return new JsonContent(res.Message, res.Status == ActionStatus.Successfull ? Status.Success : Status.Failed).ConvertToHttpResponseOK();
        }

        [NonAction]

        private HttpResponseMessage GenerateandSaveToken(UserModel user, LoginAPIModel model)
        {
            var IssuedOn = DateTime.UtcNow;
            var newToken = _authenticateManager.GenerateToken(user, IssuedOn);
            user.Token = newToken;
            TokenModel token = new TokenModel();
            token.TokenKey = newToken;
            token.UserId = user.UserId;
            token.ExpiresOn = DateTime.MaxValue;
            token.DeviceToken = model.DeviceToken;
            token.AppType = model.AppType;
            //  token.ExpiresOn = DateTime.Now.AddMinutes(Convert.ToInt32(ConfigurationManager.AppSettings["TokenExpiry"]));
            token.CreatedOn = DateTime.UtcNow;
            var result = _authenticateManager.InsertToken(token);

            if (result == 1)
            {
                //HttpResponseMessage response = new HttpResponseMessage();
                //response = Request.CreateResponse(HttpStatusCode.OK, user);
                //response.Headers.Add("Token", newToken);
                //// response.Headers.Add("TokenExpiry", ConfigurationManager.AppSettings["TokenExpiry"]);
                //response.Headers.Add("Access-Control-Expose-Headers", "Token");
                return new JsonContent("You have successfully logged in.", Status.Success, user).ConvertToHttpResponseOK();
            }
            else
            {
                return new JsonContent("Error in Creating Token", Status.Failed, user).ConvertToHttpResponseOK();
            }
        }

        [HttpPost, CheckAuthorizationAttribute.SkipAuthentication, CheckAuthorizationAttribute.SkipAuthorization]
        [ResponseType(typeof(ResponseBase))]
        public HttpResponseMessage SignUp(SignUpModel model)
        {
            var result = _authenticateManager.SignUp(model);
            UserModel user = null;
            if (result.Status == ActionStatus.Successfull)
            {
                var code = Utilities.GenerateRandomNo();
                var saveToken = _authenticateManager.SaveAccountVerificationRequest(result.Object, code.ToString());
                var emailTemplate = _templateManager.GetEmailTemplateByTemplateType(TemplateTypes.AccountVerification);
                string body = emailTemplate.TemplateContent;
                body = body.Replace("%firstname%", model.FirstName);
                body = body.Replace("%lastname%", model.LastName);
                body = body.Replace("%code%", code.ToString());

                // new code apllied here 
                body = body.Replace("%USER%", model.FirstName);
                body = body.Replace("%UserName%", model.Email);
                body = body.Replace("%Password%", model.Password);
                var verifybutton = "<a style='background-color: #7bddff; color: #fff;text-decoration: none;padding: 5px 7px;border-radius: 30px;text-transform: uppercase;' href='" + WebConfigurationManager.AppSettings["BaseUrl"].ToString() + "/Admin/Home/OTPVerification/" + result.Object+ "'>Verify Now</a>";

                body = body.Replace("%verifylink%", verifybutton);
                body = body.Replace("%AppLink%", WebConfigurationManager.AppSettings["AppLink"].ToString());
               body = body.Replace("%WebLink%", WebConfigurationManager.AppSettings["BaseUrl"].ToString());
                var link = "";
                var otp = Utilities.GenerateRandomNo();
                var result_ = _authenticateManager.ForgotPassword(model.Email, otp.ToString());
                if (result_.Status == ActionStatus.Successfull)
                {
                     link = "<a style='background-color: #7bddff; color: #fff;text-decoration: none;padding: 5px 7px;border-radius: 61px;text-transform: uppercase;' href='" + WebConfigurationManager.AppSettings["BaseUrl"] + "Admin/Home/ResetPassword?userId=" + result.Object + "&token=" + otp + "'>Reset Now</a>";
                }
                body = body.Replace("%passwordrestlink%", link);
                Utilities.SendEmail(model.Email, emailTemplate.EmailSubject, body);

                //password sent
                //var emailTemplate_ = _templateManager.GetEmailTemplateByTemplateType(TemplateTypes.NewAppUser);
                //string body_ = emailTemplate_.TemplateContent;
                //body_ = body_.Replace("%UserName%", model.Email);
                //body_ = body_.Replace("%Password%", model.Password);
                //body_ = body_.Replace("%AppLink%", WebConfigurationManager.AppSettings["AppLink"].ToString());
                //body_ = body_.Replace("%WebLink%", WebConfigurationManager.AppSettings["BaseUrl"].ToString());
                //Utilities.SendEmail(model.Email, emailTemplate_.EmailSubject, body_);

              


                user = new UserModel();
                user = _userManager.GetUserDetailsByUserId(result.Object);
            }
            return new JsonContent(result.Message, result.Status == ActionStatus.Successfull ? Status.Success : Status.Failed, user).ConvertToHttpResponseOK();
        }
        [HttpPost, CheckAuthorizationAttribute.SkipAuthentication, CheckAuthorizationAttribute.SkipAuthorization]
        [ResponseType(typeof(ResponseBase))]
        public HttpResponseMessage ResendAccountVerificationOtp(long userId)
        {
            var code = Utilities.GenerateRandomNo();

            //var token = Guid.NewGuid();
            //var link = "<a href='" + WebConfigurationManager.AppSettings["BaseUrl"] + "/Admin/Home/ConfirmEmail?userId=" + result.Object + "&token=" + token + "'>Click here</a>";
            var user = _userManager.GetUserDetailsByUserId(userId);
            if (user == null)
                return new JsonContent("User not exist.", Status.Failed, user).ConvertToHttpResponseOK();
            var saveToken = _authenticateManager.SaveAccountVerificationRequest(userId, code.ToString());
            var emailTemplate = _templateManager.GetEmailTemplateByTemplateType(TemplateTypes.AccountVerification);
            string body = emailTemplate.TemplateContent;
            body = body.Replace("%code%", code.ToString());
            Utilities.SendEmail(user.Email, emailTemplate.EmailSubject, body);
            return new JsonContent("OTP sent successfully.", Status.Success, user).ConvertToHttpResponseOK();
        }
        [HttpPost, CheckAuthorizationAttribute.SkipAuthentication, CheckAuthorizationAttribute.SkipAuthorization]
        public HttpResponseMessage ForgotPassword(string email)
        {
            if (!string.IsNullOrEmpty(email))
            {
                var otp = Utilities.GenerateRandomNo();
                var result = _authenticateManager.ForgotPassword(email, otp.ToString());
                if (result.Status == ActionStatus.Error)
                    return new JsonContent(result.Message, Status.Failed).ConvertToHttpResponseOK();
                var link = "<a style='background-color: #7bddff; color: #fff;text-decoration: none;padding: 10px 20px;border-radius: 30px;text-transform: uppercase;' href='" + WebConfigurationManager.AppSettings["BaseUrl"] + "Admin/Home/ResetPassword?userId=" + result.ID + "&token=" + otp + "'>Reset Now</a>";
                var emailTemplate = _templateManager.GetEmailTemplateByTemplateType(TemplateTypes.ForgetPassword);
                string body = emailTemplate.TemplateContent;
                body = body.Replace("%link%", link);
                string to = email;
                Utilities.SendEmail(to, emailTemplate.EmailSubject, body);
                return new JsonContent("Password reset link has been sent to your email.", Status.Success).ConvertToHttpResponseOK();
            }
            return new JsonContent("Email is required.", Status.Failed).ConvertToHttpResponseOK();
        }


        [HttpPost, CheckAuthorizationAttribute.SkipAuthentication, CheckAuthorizationAttribute.SkipAuthorization]
        [ResponseType(typeof(ResponseBase))]
        public HttpResponseMessage VerifyAccountVerificationCode(VerifyAccountVerificationCodeMOdel model)
        {
            var result = _authenticateManager.VerifyAccountVerificationCode(model);
            return new JsonContent(result.Message, result.Status == ActionStatus.Successfull ? Status.Success : Status.Failed).ConvertToHttpResponseOK();
        }



        [HttpGet, CheckAuthorizationAttribute.SkipAuthentication, CheckAuthorizationAttribute.SkipAuthorization]
        [ResponseType(typeof(ResponseBase))]
        public HttpResponseMessage GetCountries()
        {
            var result = _authenticateManager.GetCountries();
            return new JsonContent("Countries fetched successfully.", Status.Success, result).ConvertToHttpResponseOK();
        }
        [HttpGet, CheckAuthorizationAttribute.SkipAuthentication, CheckAuthorizationAttribute.SkipAuthorization]
        [ResponseType(typeof(ResponseBase))]
        public HttpResponseMessage GetCities(int countryId)
        {
            var result = _authenticateManager.GetCities(countryId);
            return new JsonContent("Cities fetched successfully.", Status.Success, result).ConvertToHttpResponseOK();
        }
        [HttpGet, CheckAuthorizationAttribute.SkipAuthentication, CheckAuthorizationAttribute.SkipAuthorization]
        [ResponseType(typeof(ResponseBase))]
        public HttpResponseMessage GetAppUserTypes()
        {
            var result = Utilities.EnumToList(typeof(AppUserTypeEnum));
            return new JsonContent("App User Types fetched successfully.", Status.Success, result).ConvertToHttpResponseOK();
        }

        [HttpGet, CheckAuthorizationAttribute.SkipAuthentication, CheckAuthorizationAttribute.SkipAuthorization]
        [ResponseType(typeof(ResponseBase))]
        public HttpResponseMessage GetTermsAndConditions()
        {
            CMSPageViewModel model = _cmsManager.GetPageContentByPageIdforFront(1);
            return new JsonContent("Terms and conditions fetched successfully.", Status.Success, new { html = model.PageContent }).ConvertToHttpResponseOK();
        }
        [HttpGet, CheckAuthorizationAttribute.SkipAuthentication, CheckAuthorizationAttribute.SkipAuthorization]
        [ResponseType(typeof(ResponseBase))]
        public HttpResponseMessage GetPrivacyPolicy()
        {
            //Client wants these two combined so we did this way
            CMSPageViewModel privacyPolicy = _cmsManager.GetPageContentByPageIdforFront(6);
            CMSPageViewModel terms = _cmsManager.GetPageContentByPageIdforFront(1);
            return new JsonContent("Privacy policy fetched successfully.", Status.Success, new { privacyPolicyHtml = privacyPolicy.PageContent,termsHtml=terms.PageContent }).ConvertToHttpResponseOK();
        }
        [HttpGet, CheckAuthorizationAttribute.SkipAuthentication, CheckAuthorizationAttribute.SkipAuthorization]
        [ResponseType(typeof(ResponseBase))]
        public HttpResponseMessage TestPush()
        {
            var obj = new PushNotificationModel();
            obj.DeviceToken = "Test_7C8H3QQFVYPMbKW3R4fYGVOPyNsZjDOeMms1F5Bj5PliLELHDMmcnazBjgoiLVuAlyNpHoasbmtQ6Adxkt8CCONqReaNjAtpXdQfTCqjAtsRgCscOKNHebc7sTtHwecr+Rxz1Y8234dpz+MRZbrlYzkQ9ivxxYBt/MHxvi72yzY=";
            obj.DeviceType = (int)AppTypeEnum.Android;
            obj.Message = "This is test message.";
            obj.Title = "Test Title";
            obj.NotificationType = NotificationTypeEnum.DepositStatusChange;
            obj.UserId = 1;
            PushNotification.SendNotification(obj);
            return new JsonContent("Privacy policy fetched successfully.", Status.Success).ConvertToHttpResponseOK();
        }

        [HttpGet, CheckAuthorizationAttribute.SkipAuthentication, CheckAuthorizationAttribute.SkipAuthorization]
        [ResponseType(typeof(ResponseBase))]
        public HttpResponseMessage IsUserNameExists(string userName)
        {
            if (!_authenticateManager.IsUserNameExists(userName))
                return new JsonContent("User with this user name not exist", Status.Success).ConvertToHttpResponseOK();
            return new JsonContent("This Username has already been taken.", Status.Failed).ConvertToHttpResponseOK();
        }

        [HttpGet]
        [ResponseType(typeof(ResponseBase))]
        public HttpResponseMessage GetBankAccounts()
        {
            var result = _bankAccountManager.GetBankAccounts();
            return new JsonContent("Bank accounts fetched successfully.", Status.Success,result).ConvertToHttpResponseOK();
        }

        [HttpGet]
        [ResponseType(typeof(ResponseBase))]
        public HttpResponseMessage GetBankNamesForCheque()
        {
            var result = _bankAccountManager.GetBankNames_API().ToList();
            var data = result.ToList().Select(p => new SelectListItem { Text = p.BankName, Value = p.BankName }).ToList();
            return new JsonContent("Banks  fetched successfully.", Status.Success, data).ConvertToHttpResponseOK();
        }
        [HttpGet]
        [ResponseType(typeof(ResponseBase))]
        public HttpResponseMessage GetBankAccountsSelectList()
        {
            var bankAccounts = _bankAccountManager.GetBankAccounts();
            var data = bankAccounts.ToList().Select(p => new SelectListItem { Text = "(" + p.BankName + " - " + Utilities.FormatBankAccount(p.AccountNumber) + ")", Value = p.BankAccountId.ToString() }).ToList();
            return new JsonContent("Bank accounts fetched successfully.", Status.Success, data).ConvertToHttpResponseOK();
        }
    }
}
