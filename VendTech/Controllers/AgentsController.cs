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
    public class AgentsController : AppUserBaseController
    {
        #region Variable Declaration
        private readonly IAgencyManager _agencyManager;
        private readonly ICommissionManager _commissionManager;
        private readonly IEmailTemplateManager _templateManager;
        private readonly IPOSManager _posManager;
        private readonly IVendorManager _vendorManager;
        private readonly IDepositManager _depositManager;
        #endregion

        public AgentsController(IAgencyManager agencyManager, IErrorLogManager errorLogManager, IEmailTemplateManager templateManager, ICommissionManager commissionManager, IPOSManager pOSManager, IVendorManager vendorManager, IDepositManager depositManager)
            : base(errorLogManager)
        {
            _agencyManager = agencyManager;
            _templateManager = templateManager;
            _commissionManager = commissionManager;
            _posManager = pOSManager;
            _vendorManager = vendorManager;
            _depositManager = depositManager;
        }

        #region User Management

        public ActionResult ManageAgents()
        {
            ViewBag.SelectedTab = SelectedAdminTab.Agents;
            if(LOGGEDIN_USER.AgencyId > 0)
            {
                var users = _agencyManager.GetAgentsPagedList(PagingModel.DefaultModel("User.Agency.AgencyName", "Desc"), LOGGEDIN_USER.AgencyId);
                return View(users);
            }
            return View(new PagingResult<AgentListingModel>());
        }

        [AjaxOnly, HttpPost]
        public JsonResult GetAgentsPagedList(PagingModel model)
        {
            ViewBag.SelectedTab = SelectedAdminTab.Agents;
            var modal = _agencyManager.GetAgentsPagedList(model, LOGGEDIN_USER.AgencyId);
            List<string> resultString = new List<string>();
            resultString.Add(RenderRazorViewToString("Partials/_agencyListing", modal));
            resultString.Add(modal.TotalCount.ToString());
            return JsonResult(resultString);
        }

        [HttpPost]
        public ActionResult AddAgent(SaveAgentModel model)
        {
            ViewBag.SelectedTab = SelectedAdminTab.Agents;
            return JsonResult(_agencyManager.AddAgent(model));
        }
        [HttpGet]
        public ActionResult AddAgent(long? id = null)
        {
            ViewBag.SelectedTab = SelectedAdminTab.Agents;
            var model = new SaveAgentModel();
            var commissions = _commissionManager.GetCommissions();
            var drpCommissions = new List<SelectListItem>();
            foreach (var item in commissions)
            {
                drpCommissions.Add(new SelectListItem { Text = item.Value.ToString(), Value = item.CommissionId.ToString() });
            }
            ViewBag.AgentTypes = Utilities.EnumToList(typeof(AgentTypeEnum));
            ViewBag.commissions = drpCommissions;
            if (id.HasValue && id > 0)
            {
                model = _agencyManager.GetAgentDetail(id.Value);
            } 
            return View(model);
        }
        public ActionResult GetAgentPercentage(long vendorId)
        {
            return Json(new { percentage = _agencyManager.GetAgentPercentage(vendorId) }, JsonRequestBehavior.AllowGet);
        }

        [AjaxOnly, HttpPost]
        public JsonResult DeleteAgent(long agentId)
        { 

            ViewBag.SelectedTab = SelectedAdminTab.Agents;
            return JsonResult(_agencyManager.DeleteAgency(agentId));
        }

        [AjaxOnly, HttpPost]
        public ActionResult GeneratePasscode(GeneratePasscode generatePasscode)
        { 
            try
            { 
                var name = _vendorManager.GetVendorDetailApi(Convert.ToInt64(generatePasscode.VendorId)).Name;
                var passCode = Utilities.GenerateFiveRandomNo();

                var result = _posManager.SavePasscodePos(new SavePassCodeModel
                {
                    CountryCode = "232",
                    POSId = generatePasscode.POSId,
                    Email = generatePasscode.Email,
                    Name = name,
                    PassCode = passCode.ToString(),
                    Phone = generatePasscode.Phone,
                    VendorId = generatePasscode.VendorId
                });

                if(result.Status == ActionStatus.Successfull)
                {
                    if (!string.IsNullOrEmpty(generatePasscode.Email))
                    {
                        if (!string.IsNullOrEmpty(name))
                        {
                            var emailTemplate = _templateManager.GetEmailTemplateByTemplateType(TemplateTypes.GeneratePasscode);
                            if (emailTemplate.TemplateStatus)
                            {
                                string body = emailTemplate.TemplateContent;
                                body = body.Replace("%UserName%", name);
                                body = body.Replace("%passcode%", passCode.ToString());
                                if (!string.IsNullOrEmpty(generatePasscode.Email))
                                {
                                    Utilities.SendEmail(generatePasscode.Email, emailTemplate.EmailSubject, body);
                                }
                            }

                        }
                    }
                    if (!string.IsNullOrEmpty(generatePasscode.Phone))
                    {
                        var requestmsg = new SendSMSRequest
                        {
                            Recipient = $"+232{generatePasscode.Phone}",
                            Payload = $"Greetings {name}\n" +
                                   $"A new login passcode was generated for your account.\n" +
                                   $"Please use the below 5 digits code to to login to the mobile app.\n" +
                                   $"{passCode}\n" +
                                   $"Thank you"
                        };

                        var json = JsonConvert.SerializeObject(requestmsg);

                        HttpClient client = new HttpClient();
                        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                        client.BaseAddress = new Uri("https://kwiktalk.io");
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                        HttpRequestMessage httpRequest = new HttpRequestMessage(HttpMethod.Post, "/api/v2/submit");
                        httpRequest.Content = new StringContent(json, Encoding.UTF8, "application/json");

                        var res = client.SendAsync(httpRequest).Result;
                        var stringResult = res.Content.ReadAsStringAsync().Result;

                        if (res.StatusCode != (HttpStatusCode)200) { }

                    }
                }
               
                return Json(result);
            }
            catch (Exception)
            {
                return Json(new ActionOutput
                {
                    Status = ActionStatus.Error,
                    Message = "Something went wrong!!"
                });
            }

             
        }

        [AjaxOnly, HttpPost, Public]
        public JsonResult FetchVendors(FetchItemsModel request)
        {
            var vendorList = _agencyManager.GetAgentsPagedList2(PagingModel.DefaultModel("User.Agency.AgencyName", "Desc"), LOGGEDIN_USER.AgencyId);
            return Json(new { result = JsonConvert.SerializeObject(vendorList.List) });
        }

        [AjaxOnly, HttpPost]
        public JsonResult TransferCash(CashTransferModel request)
        {
            try
            {
                var deposit = new Deposit
                {
                    Amount = request.Amount,
                    POSId = request.ToPosId,
                    BankAccountId = 0,
                    CreatedAt = DateTime.UtcNow,
                    PercentageAmount = request.Amount,
                };
                var result = _depositManager.CreateDepositTransfer(deposit, LOGGEDIN_USER.UserID, request.FromPosId);
                SendEmailOnDeposit(request.FromPosId, request.ToPosId);
                SendSmsOnDeposit(request.FromPosId, request.ToPosId, request.Amount);
                return Json(new { message = "You Successfuly transfered cash", currentFromVendorBalance = result.Keys.First(), currentToVendorBalance = result.Values.First() });
            }
            catch (Exception e)
            {
                return Json(new { error = "Error Occurred!!! please contact administrator"});
            }
        }

        private void SendEmailOnDeposit(long fromPos, long toPosId)
        {
            var frmPos = _posManager.GetSinglePos(fromPos);
            var toPos = _posManager.GetSinglePos(toPosId);

            if (frmPos != null & frmPos?.EmailNotificationDeposit ?? false)
            {
                var user = _userManager.GetUserDetailsByUserId(frmPos?.VendorId??0);
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