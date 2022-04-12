using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Web.Mvc;
using VendTech.Areas.Admin.Controllers;
using VendTech.Attributes;
using VendTech.BLL.Common;
using VendTech.BLL.Interfaces;
using VendTech.BLL.Managers;
using VendTech.BLL.Models;

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
        #endregion

        public AgentsController(IAgencyManager agencyManager, IErrorLogManager errorLogManager, IEmailTemplateManager templateManager, ICommissionManager commissionManager, IPOSManager pOSManager, IVendorManager vendorManager)
            : base(errorLogManager)
        {
            _agencyManager = agencyManager;
            _templateManager = templateManager;
            _commissionManager = commissionManager;
            _posManager = pOSManager;
            _vendorManager = vendorManager;
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
        public ActionResult FetchVendors(FetchItemsModel request)
        {
            var vendorList = _agencyManager.GetAgentsPagedList(PagingModel.DefaultModel("User.Agency.AgencyName", "Desc"), LOGGEDIN_USER.AgencyId);
     
            return PartialView("Transfer/_vendorsListing", vendorList);
        }

        #endregion
    }
}