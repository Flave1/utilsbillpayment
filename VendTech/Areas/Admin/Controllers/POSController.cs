using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using VendTech.Attributes;
using VendTech.BLL.Common;
using VendTech.BLL.Interfaces;
using VendTech.BLL.Models;

namespace VendTech.Areas.Admin.Controllers
{
    public class POSController : AdminBaseController
    {
        #region Variable Declaration
        private readonly IPOSManager _posManager;
        private readonly IVendorManager _vendorManager;
        private readonly IEmailTemplateManager _templateManager;
        private readonly IUserManager _userManager;
        private readonly ICommissionManager _commissionManager;
        #endregion

        public POSController(IPOSManager posManager, IErrorLogManager errorLogManager, IEmailTemplateManager templateManager, IVendorManager vendorManager, IUserManager userManager, ICommissionManager commissionManager)
            : base(errorLogManager)
        {
            _posManager = posManager;
            _templateManager = templateManager;
            _vendorManager = vendorManager;
            _userManager = userManager;
            _commissionManager = commissionManager;
        }

        #region User Management

        [HttpGet]
        public ActionResult ManagePOS()
        {
            ViewBag.SelectedTab = SelectedAdminTab.POS;
            var users = _posManager.GetPOSPagedList(PagingModel.DefaultModel("CreatedAt", "Desc"));
            return View(users);
        }

        [AjaxOnly, HttpPost]
        public JsonResult GetUsersPagingList(PagingModel model)
        {
            ViewBag.SelectedTab = SelectedAdminTab.POS;
            var modal = _posManager.GetPOSPagedList(model);
            List<string> resultString = new List<string>();
            resultString.Add(RenderRazorViewToString("Partials/_posListing", modal));
            resultString.Add(modal.TotalCount.ToString());
            return JsonResult(resultString);
        }
        [HttpPost]
        public ActionResult AddEditPos(SavePosModel model)
        {
            ViewBag.SelectedTab = SelectedAdminTab.POS;
            return JsonResult(_posManager.SavePos(model));
        }

        [AjaxOnly, HttpPost]
        public ActionResult SavePos(SavePassCodeModel savePassCodeModel)
        {
            if (!string.IsNullOrEmpty(savePassCodeModel.PassCode))
            {
                var name = _vendorManager.GetVendorDetail(Convert.ToInt64(savePassCodeModel.VendorId)).Name;
                var emailTemplate = _templateManager.GetEmailTemplateByTemplateType(TemplateTypes.GeneratePasscode);
                string body = emailTemplate.TemplateContent;
                body = body.Replace("%UserName%", name);
                body = body.Replace("%passcode%", savePassCodeModel.PassCode);
                if (!string.IsNullOrEmpty(savePassCodeModel.Email))
                {
                    //var to = new List<string>();
                    //to.Add(savePassCodeModel.Email);
                    //var emailSender = new EmailSender();
                    //emailSender.SendEmailAsync(to, emailTemplate.EmailSubject, body);
                    Utilities.SendEmail(savePassCodeModel.Email, emailTemplate.EmailSubject, body);
                }
                if (!string.IsNullOrEmpty(savePassCodeModel.Phone))
                {
                    String message = HttpUtility.UrlEncode("Hello " + name + ",%nPlease find the Passcode requested for login. " + savePassCodeModel.PassCode + " in Ventech account.");
                    //string msg = "This is a test message Your one time password for activating your Textlocal account is " + savePassCodeModel.PassCode;
                    using (var wb = new WebClient())
                    {
                        byte[] response = wb.UploadValues("https://api.textlocal.in/send/", new NameValueCollection()
                {
                {"apikey" , "3dmxGZ4kX6w-GheG39NELIgd6546OjfacESXqNOVY4"},
                {"numbers" , savePassCodeModel.CountryCode+savePassCodeModel.Phone},
                {"message" , message},
                {"sender" , "TXTLCL"}
                });
                        string result = System.Text.Encoding.UTF8.GetString(response);
                    }

                    //        string accountSid = "AC8f6fb0fee17e0c77f875cb7adbb85b9e"; //Environment.GetEnvironmentVariable(TWILIO_ACCOUNT_SID);
                    //        string authToken = "2d7452931f1f1cebfdd79220e3f5893f";// Environment.GetEnvironmentVariable(TWILIO_AUTH_TOKEN);

                    //        var client = new TwilioRestClient(accountSid, authToken);

                    //        var message = MessageResource.Create(
                    //            body: "Join Earth's mightiest heroes. Like Kevin Bacon.",
                    //            from: new Twilio.Types.PhoneNumber("+91 8000403703"),
                    //            to: new Twilio.Types.PhoneNumber("+91 807715964"),
                    //            client: client
                    //        );
                    //        var statusCode = TwilioClient.GetRestClient().HttpClient
                    //.LastResponse.StatusCode;

                }
            }
            ViewBag.SelectedTab = SelectedAdminTab.POS;
            return Json(_posManager.SavePasscodePos(savePassCodeModel));
        }

        [HttpGet]
        public ActionResult AddEditPos(long? id = null)
        {
            ViewBag.SelectedTab = SelectedAdminTab.POS;
            var model = new SavePosModel();
            var readonlyStr = "readonly";
            ViewBag.read = "";

            if (id > 0) ViewBag.read = readonlyStr;

            ViewBag.PosTypes = Utilities.EnumToList(typeof(PosTypeEnum));
            ViewBag.Vendors = _vendorManager.GetVendorsSelectList();
            var commissions = _commissionManager.GetCommissions();
            var drpCommissions = new List<SelectListItem>();
            foreach (var item in commissions)
            {
                drpCommissions.Add(new SelectListItem { Text = item.Value.ToString(), Value = item.CommissionId.ToString() });
            }
            ViewBag.commissions = drpCommissions;
            if (id.HasValue && id > 0)
            {
                model = _posManager.GetPosDetail(id.Value);
                model.PlatformList = _posManager.GetAllPlatforms(id.Value);

            }
            else
                model.PlatformList = _posManager.GetAllPlatforms(0);
            return View(model);
        }

        [AjaxOnly, HttpPost]
        public JsonResult DeletePos(long posId)
        {
            ViewBag.SelectedTab = SelectedAdminTab.Agents;
            return JsonResult(_posManager.DeletePos(posId));
        }

        [AjaxOnly, HttpPost]
        public JsonResult EnablePOS(int id)
        {
            return JsonResult(_posManager.ChangePOSStatus(id, true));
        }

        [AjaxOnly, HttpPost]
        public JsonResult DisablePOS(int id)
        {
            return JsonResult(_posManager.ChangePOSStatus(id, false));
        }

        #endregion
    }
}