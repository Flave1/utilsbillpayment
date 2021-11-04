using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using VendTech.Attributes;
using VendTech.BLL.Common;
using VendTech.BLL.Interfaces;
using VendTech.BLL.Models;
using VendTech.DAL;

namespace VendTech.Areas.Admin.Controllers
{
    public class ReleaseDepositController : AdminBaseController
    {
        #region Variable Declaration
        private readonly IUserManager _userManager;
        private readonly IEmailTemplateManager _templateManager;
        private readonly IDepositManager _depositManager;
        #endregion

        public ReleaseDepositController(IUserManager userManager, IErrorLogManager errorLogManager, IEmailTemplateManager templateManager, IDepositManager depositManager)
            : base(errorLogManager)
        {
            _userManager = userManager;
            _templateManager = templateManager;
            _depositManager = depositManager;
        }

        #region User Management

        [HttpGet]
        public ActionResult ManageDepositRelease(string status = "")
        {
            ViewBag.SelectedTab = SelectedAdminTab.Deposits;
            ViewBag.Balance = _depositManager.GetPendingDepositTotal();
            var deposits = _depositManager.GetDepositPagedList(PagingModel.DefaultModel("CreatedAt", "Desc"), true, 0, status);
            return View(deposits);
        }

        [AjaxOnly, HttpPost]
        public JsonResult GetDepositReleasePagingList(PagingModel model)
        {
            ViewBag.SelectedTab = SelectedAdminTab.Deposits;
            var modal = _depositManager.GetDepositPagedList(model, true);
            List<string> resultString = new List<string>();
            resultString.Add(RenderRazorViewToString("Partials/_depositReleaseListing", modal));
            resultString.Add(modal.TotalCount.ToString());
            return JsonResult(resultString);
        }
        [AjaxOnly, HttpPost]
        public JsonResult ApproveReleaseDeposit(long depositId)
        {
            ViewBag.SelectedTab = SelectedAdminTab.Deposits;
            return JsonResult(_depositManager.ChangeDepositStatus(depositId, DepositPaymentStatusEnum.Released, LOGGEDIN_USER.UserID));
        }
        [AjaxOnly, HttpPost]
        public JsonResult RejectReleaseDeposit(long depositId)
        {
            ViewBag.SelectedTab = SelectedAdminTab.Deposits;
            return JsonResult(_depositManager.ChangeDepositStatus(depositId, DepositPaymentStatusEnum.Rejected, LOGGEDIN_USER.UserID));
        }
        [AjaxOnly, HttpPost]
        public JsonResult SendOTP()
        { 
            ViewBag.SelectedTab = SelectedAdminTab.Deposits;
            var result = _depositManager.SendOTP();
            if (result.Status == ActionStatus.Successfull)
            {
                var emailTemplate = _templateManager.GetEmailTemplateByTemplateType(TemplateTypes.DepositOTP);
                if (emailTemplate.TemplateStatus)
                {
                    string body = emailTemplate.TemplateContent;
                    body = body.Replace("%otp%", result.Object);
                    body = body.Replace("%USER%", LOGGEDIN_USER.FirstName);
                    var currentUser = LOGGEDIN_USER.UserID;
                    Utilities.SendEmail(User.Identity.Name, emailTemplate.EmailSubject, body);
                }
            }
            return JsonResult(new ActionOutput { Message = result.Message, Status = result.Status });
        }
        [AjaxOnly, HttpPost]
        public JsonResult ChangeDepositStatus(ReleaseDepositModel model)
        {
            ViewBag.SelectedTab = SelectedAdminTab.Deposits;
            var result = _depositManager.ChangeMultipleDepositStatus(model, LOGGEDIN_USER.UserID);
            if(result.Status == ActionStatus.Error)
            {
                return JsonResult(new ActionOutput { Message = result.Message, Status = result.Status });
            }
            if (model.ReleaseDepositIds.Any())
            {
                SendEmailOnDeposit(model.ReleaseDepositIds);
                SendSmsOnDeposit(model.ReleaseDepositIds);
            }
            return JsonResult(new ActionOutput { Message = result.Message, Status = result.Status });
        }

        private void  SendEmailOnDeposit(List<long> depositIds)
        {
            var deposits = _depositManager.GetListOfDeposits(depositIds);
            if (deposits.Any())
            {
                foreach (var deposit in deposits)
                {
                    if (deposit.POS.EmailNotificationDeposit ?? true)
                    {
                        var user = _userManager.GetUserDetailsByUserId(deposit.UserId);
                        if (user != null)
                        {
                            var emailTemplate = _templateManager.GetEmailTemplateByTemplateType(TemplateTypes.DepositApprovedNotification);
                            if (emailTemplate.TemplateStatus)
                            {
                                string body = emailTemplate.TemplateContent;
                                body = body.Replace("%USER%", user.FirstName);
                                Utilities.SendEmail(user.Email, emailTemplate.EmailSubject, body);
                            }
                        }
                    }
                }
            }
        }
        private void SendSmsOnDeposit(List<long> depositIds)
        {
            if (depositIds.Any())
            {
                var deposits = _depositManager.GetListOfDeposits(depositIds);
                if (deposits.Any())
                {
                    foreach(var deposit in deposits)
                    {
                        if (deposit.POS.SMSNotificationDeposit ?? true)
                        {
                            var requestmsg = new SendSMSRequest
                            {
                                Recipient = "232" + deposit.POS.Phone,
                                Payload = $"Greetings {deposit.User.Name} \n" +
                               $"Your deposit of SLL: {string.Format("{0:N0}", deposit.Amount)} has been approved.\n" +
                               "Please confirm the amount deposited reflects in your wallet.\n" +
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

                            var res =  client.SendAsync(httpRequest).Result;
                            var stringResult = res.Content.ReadAsStringAsync().Result;
                        }
                    }
                }
            } 
        }
        #endregion
    }
}