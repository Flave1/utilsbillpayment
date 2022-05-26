using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Web.Http.Description;
using System.Web.Mvc;
using VendTech.Areas.Admin.Controllers;
using VendTech.Attributes;
using VendTech.BLL.Common;
using VendTech.BLL.Interfaces;
using VendTech.BLL.Managers;
using VendTech.BLL.Models;
using VendTech.DAL;
using VendTech.Framework.Api;

namespace VendTech.Controllers
{
    public class TransferController : AppUserBaseController
    {
        #region Variable Declaration
        private readonly ITransferManager _transferManager;
        private readonly IDepositManager _depositManager;
        private readonly IPOSManager _posManager;
        private readonly IEmailTemplateManager _templateManager;
        #endregion

        public TransferController(IErrorLogManager errorLogManager, ITransferManager transferManager, IDepositManager depositManager, IPOSManager posManager, IEmailTemplateManager templateManager)
            : base(errorLogManager)
        {
            _transferManager = transferManager;
            _depositManager = depositManager;
            _posManager = posManager;
            _templateManager = templateManager;
        }

        #region User Management

        public ActionResult Index()
        {
            ViewBag.SelectedTab = SelectedAdminTab.Transfer;

            var agencyPos = _posManager.ReturnAgencyAdminPOS(LOGGEDIN_USER.UserID);
            if(ModulesModel.Any())
            {
                return View(new TransferViewModel 
                {
                    CanTranferToOwnVendors = ModulesModel.Any(s => s.ControllerName.Contains("32")),
                    CanTranferToOtherVendors = ModulesModel.Any(s => s.ControllerName.Contains("33")),
                    Vendor = LOGGEDIN_USER?.AgencyName,
                    AdminBalance = string.Format("{0:N0}", agencyPos.Balance),
                    AdminName = agencyPos.User.Name + " " + agencyPos.User.SurName,
                    AdminPos = agencyPos.SerialNumber
                });
            }

            return View(new TransferViewModel());

        }

        [AjaxOnly, HttpPost]
        public JsonResult GetAllAgencyAdminVendors(FetchItemsModel request)
        {
            var vendorList = _transferManager.GetAllAgencyAdminVendors(PagingModel.DefaultModel("User.Agency.AgencyName", "Desc"), LOGGEDIN_USER.AgencyId);
            return Json(new { result = JsonConvert.SerializeObject(vendorList.List) });
        }

        [AjaxOnly, HttpPost]
        public JsonResult GetAllOtherVendors(FetchItemsModel request)
        {
            var vendorList = _transferManager.GetOtherVendors(PagingModel.DefaultModel("User.Agency.AgencyName", "Desc"), LOGGEDIN_USER.AgencyId);
            return Json(new { result = JsonConvert.SerializeObject(vendorList.List) });
        }


        [AjaxOnly, HttpPost]
        public JsonResult TransferCash(CashTransferModel request)
        {
            try
            {

                //return null;
                var reference = Utilities.GenerateByAnyLength(6).ToUpper();
                var depositCr = new Deposit
                {
                    Amount = request.Amount,
                    POSId = request.ToPosId,
                    BankAccountId = 0,
                    CreatedAt = DateTime.UtcNow,
                    PercentageAmount = request.Amount,
                    CheckNumberOrSlipId = reference
                };
                var to = _depositManager.CreateDepositCreditTransfer(depositCr, LOGGEDIN_USER.UserID, request.FromPosId);
                var depositDr = new Deposit
                {
                    Amount = Decimal.Negate(request.Amount),
                    POSId = request.FromPosId,
                    BankAccountId = 0,
                    CreatedAt = DateTime.UtcNow,
                    PercentageAmount = Decimal.Negate(request.Amount),
                    CheckNumberOrSlipId = reference
                };
                var from = _depositManager.CreateDepositDebitTransfer(depositDr, LOGGEDIN_USER.UserID);

                //SendEmailOnDeposit(request.FromPosId, request.ToPosId);
                //SendSmsOnDeposit(request.FromPosId, request.ToPosId, request.Amount);
                return Json(new { message = "TRANSFER SUCCESSFUL", currentFromVendorBalance = from, currentToVendorBalance = to });
            }
            catch (Exception e)
            {
                return Json(new { error = "Error Occurred!!! please contact administrator" });
            }
        }


        private void SendEmailOnDeposit(long fromPos, long toPosId)
        {
            var frmPos = _posManager.GetSinglePos(fromPos);
            var toPos = _posManager.GetSinglePos(toPosId);

            if (frmPos != null & frmPos?.EmailNotificationDeposit ?? false)
            {
                var user = _userManager.GetUserDetailsByUserId(frmPos?.VendorId ?? 0);
                if (user != null)
                {
                    var emailTemplate = _templateManager.GetEmailTemplateByTemplateType(TemplateTypes.TransferFromNotification);
                    if (emailTemplate.TemplateStatus)
                    {
                        string body = emailTemplate.TemplateContent;
                        body = body.Replace("%USER%", user.FirstName);
                        Utilities.SendEmail(user.Email, emailTemplate.EmailSubject, body);
                    }
                }
            }

            if (toPos != null & toPos?.EmailNotificationDeposit ?? false)
            {
                var user = _userManager.GetUserDetailsByUserId(toPos?.VendorId ?? 0);
                if (user != null)
                {
                    var emailTemplate = _templateManager.GetEmailTemplateByTemplateType(TemplateTypes.TransferToNotification);
                    if (emailTemplate.TemplateStatus)
                    {
                        string body = emailTemplate.TemplateContent;
                        body = body.Replace("%USER%", user.FirstName);
                        Utilities.SendEmail(user.Email, emailTemplate.EmailSubject, body);
                    }
                }
            }
        }
        private void SendSmsOnDeposit(long fromPos, long toPosId, decimal amt)
        {

            var frmPos = _posManager.GetSinglePos(fromPos);
            var toPos = _posManager.GetSinglePos(toPosId);

            if (frmPos != null & frmPos.SMSNotificationDeposit ?? true)
            {
                var requestmsg = new SendSMSRequest
                {
                    Recipient = "232" + frmPos.Phone,
                    Payload = $"Greetings {frmPos.User.Name} \n" +
                   $"Your wallet has been credited of SLL: {string.Format("{0:N0}", amt)}.\n" +
                   "Please confirm the amount transferred reflects in your wallet.\n" +
                   "VENDTECH"
                };

                var json = JsonConvert.SerializeObject(requestmsg);

                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                HttpClient client = new HttpClient();
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                client.BaseAddress = new Uri("https://kwiktalk.io");
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                HttpRequestMessage httpRequest = new HttpRequestMessage(HttpMethod.Post, "/api/v2/submit");
                httpRequest.Content = new StringContent(json, Encoding.UTF8, "application/json");

                var res = client.SendAsync(httpRequest).Result;
                var stringResult = res.Content.ReadAsStringAsync().Result;
            }

            if (toPos != null & toPos.SMSNotificationDeposit ?? true)
            {
                var requestmsg = new SendSMSRequest
                {
                    Recipient = "232" + toPos.Phone,
                    Payload = $"Greetings {toPos.User.Name} \n" +
                   $"Your wallet has been debited of SLL: {string.Format("{0:N0}", amt)}.\n" +
                   "VENDTECH"
                };

                var json = JsonConvert.SerializeObject(requestmsg);

                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                HttpClient client = new HttpClient();
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                client.BaseAddress = new Uri("https://kwiktalk.io");
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                HttpRequestMessage httpRequest = new HttpRequestMessage(HttpMethod.Post, "/api/v2/submit");
                httpRequest.Content = new StringContent(json, Encoding.UTF8, "application/json");

                var res = client.SendAsync(httpRequest).Result;
                var stringResult = res.Content.ReadAsStringAsync().Result;
            }
        }

        #endregion

    }


}
