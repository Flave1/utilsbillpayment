using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http.Description;
using System.Web.Mvc;
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
        private readonly ISMSManager _smsManager;
        #endregion

        public TransferController(IErrorLogManager errorLogManager, ITransferManager transferManager, IDepositManager depositManager, IPOSManager posManager, IEmailTemplateManager templateManager, ISMSManager smsManager)
            : base(errorLogManager)
        {
            _transferManager = transferManager;
            _depositManager = depositManager;
            _posManager = posManager;
            _templateManager = templateManager;
            _smsManager = smsManager;
        }

        #region User Management

        public ActionResult Index()
        {
            ViewBag.SelectedTab = SelectedAdminTab.Transfer;
            if(LOGGEDIN_USER == null)
            {
                SignOut();
                return View("Index", "Home");
            }
            var agencyPos = _posManager.ReturnAgencyAdminPOS(LOGGEDIN_USER.UserID);
            if(ModulesModel.Any())
            {
                return View(new TransferViewModel 
                {
                    CanTranferToOwnVendors = ModulesModel.Any(s => s.ControllerName.Contains("32")),
                    CanTranferToOtherVendors = ModulesModel.Any(s => s.ControllerName.Contains("33")),
                    Vendor = LOGGEDIN_USER?.AgencyName,
                    AdminBalance = Utilities.FormatAmount(agencyPos.Balance),
                    AdminName = LOGGEDIN_USER?.AgencyName, //+ " - " + agencyPos.SerialNumber, //agencyPos.User.Name + " " + agencyPos.User.SurName,
                    AdminPos = agencyPos.SerialNumber,
                    AdminPosId = agencyPos.POSId
                });
            }

            return View(new TransferViewModel());

        }

        [AjaxOnly, HttpPost]
        public JsonResult GetAllAgencyAdminVendors(FetchItemsModel request)
        {
            var vendorList = _transferManager
                .GetAllAgencyAdminVendors(
                PagingModel.DefaultModel("User.Agency.AgencyName", "Desc"), 
                LOGGEDIN_USER.AgencyId, LOGGEDIN_USER.UserID);
            return Json(new { result = JsonConvert.SerializeObject(vendorList.List) });
        }

        [AjaxOnly, HttpPost]
        public JsonResult GetAllOtherVendors(FetchItemsModel request)
        {
            var vendorList = _transferManager.GetOtherVendors(PagingModel.DefaultModel("User.Agency.AgencyName", "Desc"), LOGGEDIN_USER.AgencyId);
            return Json(new { result = JsonConvert.SerializeObject(vendorList.List) });
        }


        [AjaxOnly, HttpPost]
        public  JsonResult TransferCash(CashTransferModel request)
        {
            try
            {
                if (LOGGEDIN_USER.UserID == 0 || LOGGEDIN_USER == null)
                {
                    SignOut();
                }
                var reference = Utilities.GenerateByAnyLength(6).ToUpper();
                var frompos = _posManager.ReturnAgencyAdminPOS(LOGGEDIN_USER.UserID);
                var depositDr = new Deposit
                {
                    Amount = Decimal.Negate(request.Amount),
                    POSId = frompos.POSId,
                    BankAccountId = 0,
                    CreatedAt = DateTime.UtcNow,
                    PercentageAmount = Decimal.Negate(request.Amount),
                    CheckNumberOrSlipId = reference,
                    ValueDate = DateTime.UtcNow.ToString(),
                    ValueDateStamp = DateTime.UtcNow
                };

                var result1 = _depositManager.CreateDepositDebitTransfer(depositDr, LOGGEDIN_USER.UserID, request.otp, request.ToPosId, frompos);

                
                if(result1.Status == ActionStatus.Successfull)
                {
                    var depositCr = new Deposit
                    {
                        Amount = request.Amount,
                        POSId = request.ToPosId,
                        BankAccountId = 0,
                        CreatedAt = DateTime.UtcNow,
                        PercentageAmount = request.Amount,
                        CheckNumberOrSlipId = reference,
                        ValueDate = DateTime.UtcNow.ToString(),
                        ValueDateStamp = DateTime.UtcNow
                    };
                    var result2 = _depositManager.CreateDepositCreditTransfer(depositCr, LOGGEDIN_USER.UserID, frompos, request.otp);

                    if (result2.Status == ActionStatus.Successfull)
                    {
                        //SendEmailOnDeposit(request.FromPosId, request.ToPosId);
                        //await SendSmsOnDeposit(request.FromPosId, request.ToPosId, request.Amount);
                    }
                    return JsonResult(new ActionOutput { Message = result2.Message, Status = result2.Status });
                }
                else
                {
                    return JsonResult(new ActionOutput { Message = result1.Message, Status = result1.Status});
                }
              

               
            }
            catch (Exception e)
            {
                return JsonResult(new ActionOutput { Message = "Error Occurred!!! please contact administrator", Status = ActionStatus.Error });
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
        private async Task SendSmsOnDeposit(long fromPos, long toPosId, decimal amt)
        {

            var frmPos = _posManager.GetSinglePos(fromPos);
            var toPos = _posManager.GetSinglePos(toPosId);

            if (frmPos != null & frmPos.SMSNotificationDeposit ?? true)
            {
                var requestmsg = new SendSMSRequest
                {
                    Recipient = "232" + frmPos.Phone,
                    Payload = $"Greetings {frmPos.User.Name} \n" +
                   $"Your wallet has been credited of NLe: {Utilities.FormatAmount(amt)}.\n" +
                   "Please confirm the amount transferred reflects in your wallet.\n" +
                   "VENDTECH"
                };

                await _smsManager.SendSmsAsync(requestmsg);
            }

            if (toPos != null & toPos.SMSNotificationDeposit ?? true)
            {
                var requestmsg = new SendSMSRequest
                {
                    Recipient = "232" + toPos.Phone,
                    Payload = $"Greetings {toPos.User.Name} \n" +
                   $"Your wallet has been debited of NLe: {Utilities.FormatAmount(amt)}.\n" +
                   "VENDTECH"
                };

                await _smsManager.SendSmsAsync(requestmsg);
            }
        }


        [AjaxOnly, HttpPost]
        public async Task<JsonResult> SendOTP()
        {

            //return JsonResult(new ActionOutput { Message = "Please try again later", Status = ActionStatus.Error });
            if (LOGGEDIN_USER.UserID == 0 || LOGGEDIN_USER == null)
            {
                SignOut();
            }
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
                    //Utilities.SendEmail(User.Identity.Name, emailTemplate.EmailSubject, body);
                }

                var user = _userManager.GetAppUserProfile(LOGGEDIN_USER.UserID);
                if(user != null)
                {
                    var requestmsg = new SendSMSRequest
                    {
                        Recipient = "232" + user.Phone,
                        Payload = $"Greetings {user.Name} \n" +
                  $"To Approve deposits, please use the following OTP (One Time Passcode). {result.Object}\n" +
                  "VENDTECH"
                    };
                    await _smsManager.SendSmsAsync(requestmsg);
                }
                
            }
            return JsonResult(new ActionOutput { Message = result.Message, Status = result.Status });
        }
        #endregion

    }


}
