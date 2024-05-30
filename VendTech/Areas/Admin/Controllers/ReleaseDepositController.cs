using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using VendTech.Attributes;
using VendTech.BLL.Common;
using VendTech.BLL.Interfaces;
using VendTech.BLL.Models;
using VendTech.DAL;

namespace VendTech.Areas.Admin.Controllers
{
    public class ReleaseDepositController : AdminBaseV2Controller
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

        [HttpGet, Public]
        public ActionResult ManageDepositRelease(string status = "", string depositids = "", string otp = "", string taskId = "")
        {
            try
            {
                if (!User.Identity.IsAuthenticated)
                {
                    var user = _userManager.BackgroundAdminLogin(Convert.ToInt64(taskId));
                    if (user == null)
                        SignOut();
                    else
                    {
                        JustLoggedin = true;
                        var PermissonAndDetailModel = new PermissonAndDetailModelForAdmin();
                        PermissonAndDetailModel.UserDetails = new UserDetailForAdmin(user);
                        PermissonAndDetailModel.ModulesModelList = _userManager.GetAllModulesAtAuthentication(user.UserID)
                            .Where(e => e.ControllerName != "19" && e.ControllerName != "20" && e.ControllerName != "1" && e.ControllerName != "23" && e.ControllerName != "18" && e.ControllerName != "27").ToList();
                        CreateCustomAuthorisationCookie(user.UserName, false, new JavaScriptSerializer().Serialize(PermissonAndDetailModel));

                        ViewBag.LOGGEDIN_USER = user;
                        ViewBag.Data = _userManager.GetNotificationUsersCount(user.UserID);
                        ViewBag.USER_PERMISSONS = PermissonAndDetailModel.ModulesModelList;
                    }
                }
                ViewBag.SelectedTab = SelectedAdminTab.Deposits;
                ViewBag.Balance = _depositManager.GetPendingDepositTotal();
                var deposits = _depositManager.GetAllPendingDepositPagedList(PagingModel.DefaultModel("CreatedAt", "Desc"), true, 0, status);
                return View( deposits);
            }
            catch (Exception)
            {
                SignOut();
                return View("ManageDepositRelease2", new PagingResult<DepositListingModel>());
            }
        }

        [HttpGet, Public]
        public ActionResult Autoreleased(string status = "", string depositids = "", string otp = "", string taskId = "")
        {
            try
            {
                if (!_depositManager.IsOtpValid(otp))
                {
                    return View(new AutoRelease { Success = false, Message = $"Deposit was not approved \n Invalid Otp" });
                }
                var user = _userManager.BackgroundAdminLogin(Convert.ToInt64(taskId));
                if (user == null)
                {
                    return View(new AutoRelease { Success = false, Message = $"Deposit was not approved \n Invalid admin account" });
                }
                var result  = _depositManager.ChangeDepositStatus(Convert.ToInt64(depositids.Split(',')[0]), DepositPaymentStatusEnum.Released, false);
                if (result.Status == ActionStatus.Successfull)
                {
                    var depIds = depositids.Split(',').Select(long.Parse).ToList();
                    SendEmailOnDeposit(depIds);
                    SendSmsOnDeposit(depIds);

                    return View(new AutoRelease { Success = true, Message = $"{result.Message}" });
                }
                return View(new AutoRelease { Success = false, Message = "Deposit wasnot approved" });
                
            }
            catch (Exception)
            {
                return View(new AutoRelease { Success = false, Message = "Error Occured trying to approve deposit" });
            }
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
            var result = _depositManager.ChangeDepositStatus(depositId, DepositPaymentStatusEnum.Released, false);
            return JsonResult(result);
        }
        [AjaxOnly, HttpPost]
        public JsonResult RejectReleaseDeposit(long depositId)
        {
            ViewBag.SelectedTab = SelectedAdminTab.Deposits;
            return JsonResult(_depositManager.ChangeDepositStatus(depositId, DepositPaymentStatusEnum.Rejected, false));
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
                    //Utilities.SendEmail(admin.Email, emailTemplate.EmailSubject, body);
                    Utilities.SendEmail("vblell@gmail.com", emailTemplate.EmailSubject, body);
                }
            }
            return JsonResult(new ActionOutput { Message = result.Message, Status = result.Status });
        }


        [AjaxOnly, HttpPost]
        public JsonResult SendOTP2(ReleaseDepositModel2 model)
        {
            ViewBag.SelectedTab = SelectedAdminTab.Deposits;
            var result = _depositManager.SendOTP();
            if (result.Status == ActionStatus.Successfull)
            {
                var emailTemplate = _templateManager.GetEmailTemplateByTemplateType(TemplateTypes.DepositOTP);
                if (emailTemplate.TemplateStatus)
                {
                    var releaseBtn = $"<a href='{Utilities.DomainUrl}/Admin/ReleaseDeposit/ManageDepositRelease?depositids={string.Join(",", model.ReleaseDepositIds)}&otp={result.Object}&taskId={LOGGEDIN_USER.UserID}'>Release deposit</a>";
                    string body = emailTemplate.TemplateContent;
                    body = body.Replace("%otp%", result.Object);
                    body = body.Replace("%RELEASEOTP%", releaseBtn);
                    body = body.Replace("%USER%", LOGGEDIN_USER.FirstName);
                    var currentUser = LOGGEDIN_USER.UserID; 
                    Utilities.SendEmail(User.Identity.Name, emailTemplate.EmailSubject, body);
                    //Utilities.SendEmail("vblell@gmail.com", emailTemplate.EmailSubject, body);
                }
            }
            return JsonResult(new ActionOutput { Message = result.Message, Status = result.Status });
        }
        [AjaxOnly, HttpPost]
        public JsonResult ChangeDepositStatus(ReleaseDepositModel model)
        {
            ViewBag.SelectedTab = SelectedAdminTab.Deposits;
            var result = _depositManager.ChangeMultipleDepositStatus(model, LOGGEDIN_USER.UserID);

            if (result.Status == ActionStatus.Error)
            {
                return JsonResult(new ActionOutput { Message = result.Message, Status = result.Status });
            } 

            if (model.ReleaseDepositIds != null && model.ReleaseDepositIds.Any())
            {
                SendEmailOnDeposit(model.ReleaseDepositIds);
                SendSmsOnDeposit(model.ReleaseDepositIds);
            }
            return JsonResult(new ActionOutput { Message = result.Message, Status = result.Status });
        }

        [AjaxOnly, HttpPost]
        public JsonResult CancelDeposit(CancelDepositModel model)
        {
            ViewBag.SelectedTab = SelectedAdminTab.Deposits;
            var result = _depositManager.CancelDeposit(model);
            if (result.Status == ActionStatus.Error)
            {
                return JsonResult(new ActionOutput { Message = result.Message, Status = result.Status });
            }
            return JsonResult(new ActionOutput { Message = result.Message, Status = result.Status });
        }


        [AjaxOnly, HttpPost]
        public JsonResult AutoRelease(ReleaseDepositModel2 model)
        {
            try
            {
                var pds = _depositManager.GetPendingDeposits(model.ReleaseDepositIds);
                for (int i = 0; i < pds.Count; i++)
                {
                    ActionOutput result = _depositManager.ChangeDepositStatus(pds[i].PendingDepositId, DepositPaymentStatusEnum.Released, true);
                    var deposit = _depositManager.GetDeposit(pds[i].PendingDepositId);
                    SendEmailOnDepositApproval(deposit);
                    SendEmailToAdminOnDepositApproval(deposit, result.ID);
                    SendSmsOnDepositApproval(deposit);

                    _depositManager.DeletePendingDeposits(deposit);
                }


                return JsonResult(new ActionOutput { Message = $"{model.ReleaseDepositIds.Count} DEPOSIT (S) RELEASED AUTOMATICALLY", Status = ActionStatus.Successfull });
            }
            catch (ArgumentException ex)
            {
                return JsonResult(new ActionOutput { Message = ex.Message, Status = ActionStatus.Error });
            }
            catch (Exception ex)
            {
                return JsonResult(new ActionOutput { Message = ex.Message, Status = ActionStatus.Error });
            }
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
                               "Your last deposit has been approved\n" +
                               "Please confirm the amount deposited reflects in your wallet correctly.\n" +
                               $"{Utilities.GetCountry().CurrencyCode}: {Utilities.FormatAmount(deposit.Amount)} \n" +
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
                    _depositManager.DeletePendingDeposits(deposits);
                }
            } 
        }
        private void SendEmailOnDepositApproval(PendingDeposit deposit)
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
        private void SendEmailToAdminOnDepositApproval(PendingDeposit dep, long trxId)
        {
            var adminUsers = _userManager.GetAllAdminUsersByDepositRelease();

            if (dep.POS != null)
            {
                foreach (var admin in adminUsers)
                {
                    string body = $"<p>Greetings {admin.Name}, </p>" +
                                 $"<b>This is to inform you that a deposit has been AUTO APPROVED for</b> </br>" +
                                 "</br>" +
                                 $"Vendor Name: <b>{dep.POS.User.Vendor}</b> </br></br>" +
                                 $"POSID: <b>{dep.POS.SerialNumber}</b>  </br></br>" +
                                 $"DEPOSIT ID: <b>{trxId}</b> </br></br>" +
                                 $"REF#: <b>{dep.CheckNumberOrSlipId}</b> </br></br>" +
                                 $"Amount: <b>{Utilities.GetCountry().CurrencyCode} {Utilities.FormatAmount(dep.Amount)}</b> </br>" +
                                 $"</br>" +
                                 $"Thank You" +
                                 $"<br/>" +
                                 $"<p>{Utilities.EMAILFOOTERTEMPLATE}</p>";

                    Utilities.SendEmail(admin.Email, "VENDTECH SUPPORT | DEPOSIT AUTO APPROVAL EMAIL", body);
                }
            }
        }
        private bool SendSmsOnDepositApproval(PendingDeposit deposit)
        {
            if (deposit.POS.SMSNotificationDeposit ?? true)
            {
                var requestmsg = new SendSMSRequest
                {
                    Recipient = Utilities.GetCountry().CountryCode + deposit.POS.Phone,
                    Payload = $"Greetings {deposit.POS.User.Name} \n" +
                   "Your last deposit has been approved\n" +
                   "Please confirm the amount deposited reflects in your wallet correctly.\n" +
                   $"{Utilities.GetCountry().CurrencyCode}: {Utilities.FormatAmount(deposit.Amount)} \n" +
                   "VENDTECH"
                };
                return Utilities.SendSms(requestmsg);
            }
            return false;
        }



        #endregion
    }
}