using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Web;
using System.Web.Configuration;
using System.Web.Http;
using System.Web.Http.Description;
using VendTech.Attributes;
using VendTech.BLL.Common;
using VendTech.BLL.Interfaces;
using VendTech.BLL.Models;
using VendTech.Framework.Api;

namespace VendTech.Areas.Api.Controllers
{
    public class UserController : BaseAPIController
    {
        private readonly IUserManager _userManager;
        private readonly IAuthenticateManager _authenticateManager;
        private readonly IEmailTemplateManager _templateManager;
        private readonly IPlatformManager _platformManager;
        private readonly IPOSManager _posManager;
        private readonly ISMSManager _sMSManager;
        private readonly IMeterManager _meterManager;
        public UserController(IUserManager userManager, IErrorLogManager errorLogManager, IEmailTemplateManager templateManager, IAuthenticateManager authenticateManager, IPlatformManager platformManager, IPOSManager posManager, ISMSManager sMSManager, IMeterManager meterManager)
            : base(errorLogManager)
        {
            _userManager = userManager;
            _authenticateManager = authenticateManager;
            _templateManager = templateManager;
            _platformManager = platformManager;
            _posManager = posManager;
            _sMSManager = sMSManager;
            _meterManager = meterManager;
        }
        [HttpPost, CheckAuthorizationAttribute.SkipAuthentication, CheckAuthorizationAttribute.SkipAuthorization]
        [ResponseType(typeof(ResponseBase))]
        public HttpResponseMessage Test()
        {
            var aa = _userManager.GetWelcomeMessage();
            return new JsonContent("OTP sent successfully", Status.Success).ConvertToHttpResponseOK();
        }
        [HttpGet]
        [ResponseType(typeof(ResponseBase))]
        public HttpResponseMessage GetUserProfile()
        {
            var result = _userManager.GetUserDetailsForApi(LOGGEDIN_USER.UserId);
            if (result.Status == ActionStatus.Error)
                return new JsonContent(result.Message, Status.Failed).ConvertToHttpResponseOK();

            return new JsonContent(result.Message, Status.Success, result.Object).ConvertToHttpResponseOK();
        }
        [HttpPost]
        [ResponseType(typeof(ResponseBase))]
        public HttpResponseMessage UpdateProfilePic()
        {
            //System.IO.StreamWriter sw = null;
            //try
            //{
            //    sw = new StreamWriter(System.Web.HttpContext.Current.Server.MapPath("~/ErrorLog.txt"), true);
            //    sw.WriteLine(HttpContext.Current.Request);
            //    sw.WriteLine(HttpContext.Current.Request.RequestContext); 
            //}
            //catch { }
            //finally { sw.Close(); }
            if (HttpContext.Current.Request.Files.Count == 0)
                return new JsonContent("Please select an image.", Status.Failed).ConvertToHttpResponseOK();
            var image = HttpContext.Current.Request.Files[0];
            var result = _userManager.UpdateProfilePic(LOGGEDIN_USER.UserId, image);
            if (result.Status == ActionStatus.Error)
                return new JsonContent(result.Message, Status.Failed).ConvertToHttpResponseOK();
            var userProfile = _userManager.GetUserDetailsForApi(LOGGEDIN_USER.UserId);

            return new JsonContent("Profile picture updated successfully.", Status.Success, new { user = userProfile.Object }).ConvertToHttpResponseOK();
        }
        [HttpPost]
        [ResponseType(typeof(ResponseBase))]
        public HttpResponseMessage UpdateUserProfile()
        {
              if (!Request.Content.IsMimeMultipartContent())
                throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
             var httpRequest = HttpContext.Current.Request;
            var form = httpRequest.Form;
            var model = new UpdateProfileModel();
            if(httpRequest.Files.Count>0)
            model.Image = HttpContext.Current.Request.Files[0];

            if (form["Name"] != null)
                model.Name = form["Name"].ToString();
            if (form["SurName"] != null)
                model.SurName = form["SurName"].ToString();
            if (form["DOB"] != null)
                model.DOB = Convert.ToDateTime(form["DOB"].ToString());
            if (form["City"] != null)
                model.City = Convert.ToInt32(form["City"]);
            if (form["Country"] != null)
                model.Country =Convert.ToInt32(form["Country"]);
            if (form["Phone"] != null)
                model.Phone = form["Phone"].ToString();
            if (form["Address"] != null)
                model.Address = form["Address"].ToString();
            var result = _userManager.UpdateUserProfile(LOGGEDIN_USER.UserId, model);
            if (result.Status == ActionStatus.Error)
                return new JsonContent(result.Message, Status.Failed).ConvertToHttpResponseOK();
            var userProfile = _userManager.GetUserDetailsForApi(LOGGEDIN_USER.UserId);
            return new JsonContent("Profile updated successfully.", Status.Success, new { user = userProfile.Object }).ConvertToHttpResponseOK();
        }

        [HttpGet]
        [ResponseType(typeof(ResponseBase))]
        public HttpResponseMessage GetNotifications(int pageNo, int pageSize)
        {

            var result = _userManager.GetUserNotifications(pageNo, pageSize, LOGGEDIN_USER.UserId);
            if (result.Status == ActionStatus.Error)
                return new JsonContent(result.Message, Status.Failed).ConvertToHttpResponseOK();
            return new JsonContent(result.TotalCount, result.Message, Status.Success, result.List).ConvertToHttpResponseOK();
        }

        [HttpGet]
        [ResponseType(typeof(ResponseBase))]
        public HttpResponseMessage GetUserNotificationsApi(int pageNo, int pageSize)
        {

            var result = _userManager.GetUserNotificationApi(pageNo, pageSize, LOGGEDIN_USER.UserId);
            if (result.Result3 == ActionStatus.Error)
                return new JsonContent("Error In Retrieving Data!!", Status.Failed).ConvertToHttpResponseOK();
            return new JsonContent(0, "Notification Received For this user!", Status.Success, result).ConvertToHttpResponseOK();
        }

        [HttpGet]
        [ResponseType(typeof(ResponseBase))]
        public HttpResponseMessage GetWalletBalance()
        {
            var userDetail = _userManager.GetUserDetailsByUserId(LOGGEDIN_USER.UserId);
            var balance = _userManager.GetUserWalletBalance(LOGGEDIN_USER.UserId);
            return new JsonContent(
                "User balance fetched successfully.", 
                Status.Success, 
                new {
                        balance = balance, 
                        unReadNotifications = _userManager.GetUnreadNotifications(LOGGEDIN_USER.UserId), 
                        accountStatus=userDetail.AccountStatus ,
                        stringBalance = string.Format("{0:N0}", balance)
        }
                ).ConvertToHttpResponseOK();
        }

        [HttpPost]
        [ResponseType(typeof(ResponseBase))]
        public HttpResponseMessage ChangePassword(ResetPasswordModel model)
        {
            model.UserId = LOGGEDIN_USER.UserId;
                var code = Utilities.GenerateRandomNo();
                var result = _authenticateManager.SaveChangePasswordOTP(LOGGEDIN_USER.UserId, model.OldPassword, code.ToString());
                if (result.Status == ActionStatus.Successfull)
                {
                    var emailTemplate = _templateManager.GetEmailTemplateByTemplateType(TemplateTypes.ChangePassword);
                    string body = emailTemplate.TemplateContent;
                    body = body.Replace("%OTP%", code.ToString());
                    string to = result.Object;
                    Utilities.SendEmail(to, emailTemplate.EmailSubject, body);
                }
            //var result = _authenticateManager.ResetPassword(model);
            return new JsonContent(result.Message, Status.Success, result.Status==ActionStatus.Successfull?Status.Success:Status.Failed).ConvertToHttpResponseOK();
        }
        [HttpPost]
        [ResponseType(typeof(ResponseBase))]
        public HttpResponseMessage VerifyChangePasswordOTP(ResetPasswordModel model)
        {
            model.UserId = LOGGEDIN_USER.UserId;
            var result = _authenticateManager.VerifyChangePasswordOTP(model);
            var userDetail = _userManager.GetUserDetailsByUserId(LOGGEDIN_USER.UserId);
            return new JsonContent(result.Message, result.Status == ActionStatus.Successfull ? Status.Success : Status.Failed, new {accountStatus=userDetail.AccountStatus }).ConvertToHttpResponseOK();
        }
        [HttpPost]
        [ResponseType(typeof(ResponseBase))]
        public HttpResponseMessage GenerateReferralCode()
        {
            var result = _userManager.SaveReferralCode(LOGGEDIN_USER.UserId);
            return new JsonContent(result.Message, result.Status == ActionStatus.Successfull ? Status.Success : Status.Failed, new { code = result.Object }).ConvertToHttpResponseOK();
        }
        [HttpGet]
        [ResponseType(typeof(ResponseBase))]
        public HttpResponseMessage GetUserAssignedPlatforms()
        {
            var result = _platformManager.GetUserAssignedPlatforms(LOGGEDIN_USER.UserId);
            return new JsonContent("Platforms fetched successfully.", Status.Success, result).ConvertToHttpResponseOK();
        }
        [HttpGet]
        [ResponseType(typeof(ResponseBase))]
        public HttpResponseMessage GetUserPos()
        {
            var result = _posManager.GetPOSSelectListForApi(LOGGEDIN_USER.UserId);
            return new JsonContent("Pos fetched successfully.", Status.Success, result).ConvertToHttpResponseOK();
        }
        [HttpGet]
        [ResponseType(typeof(ResponseBase))]
        public HttpResponseMessage GetUserPosPagingList(int pageNo, int pageSize)
        {
            var result = _posManager.GetUserPosPagingListForApp(pageNo,pageSize,LOGGEDIN_USER.UserId);
            return new JsonContent(result.TotalCount, result.Message, result.Status == ActionStatus.Successfull ? Status.Success : Status.Failed, result.List).ConvertToHttpResponseOK();
        }

    }
}
