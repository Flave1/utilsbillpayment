#region Default Namespaces
using System;
using System.Linq;
using System.Web.Mvc;
#endregion

#region Custom Namespaces
using VendTech.Attributes;
using VendTech.BLL.Interfaces;
using VendTech.BLL.Models;
using VendTech.BLL.Common;
using static VendTech.Controllers.MeterController;
using VendTech.DAL;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Net;
using System.Text;
using System.Threading.Tasks;
#endregion

namespace VendTech.Controllers
{
    /// <summary>
    /// Home Controller 
    /// Created On: 10/04/2015
    /// </summary>
    public class DepositController : AppUserBaseController
    {
        #region Variable Declaration
        private readonly IUserManager _userManager;
        private readonly IAuthenticateManager _authenticateManager;
        private readonly IVendorManager _vendorManager;
        private readonly ICMSManager _cmsManager;
        private readonly IDepositManager _depositManager;
        private readonly IMeterManager _meterManager;
        private readonly IBankAccountManager _bankAccountManager;
        private readonly IPOSManager _posManager;
        private readonly IEmailTemplateManager _templateManager;
        private readonly IPaymentTypeManager _paymentTypeManager;


        #endregion

        public DepositController(IUserManager userManager, IErrorLogManager errorLogManager, IAuthenticateManager authenticateManager, ICMSManager cmsManager, IDepositManager depositManager, IMeterManager meterManager, IVendorManager vendorManager, IBankAccountManager bankAccountManager, IPOSManager posManager, IEmailTemplateManager templateManager, IPaymentTypeManager paymentTypeManager)
            : base(errorLogManager)
        {
            _userManager = userManager;
            _authenticateManager = authenticateManager;
            _cmsManager = cmsManager;
            _depositManager = depositManager;
            _meterManager = meterManager;
            _vendorManager = vendorManager;
            _bankAccountManager = bankAccountManager;
            _posManager = posManager;
            _templateManager = templateManager;
            _paymentTypeManager = paymentTypeManager;
        }

        /// <summary>
        /// Index View 
        /// </summary>
        /// <returns></returns>
        public ActionResult Index(string posId = "")
        {
            ViewBag.SelectedTab = SelectedAdminTab.Deposits;
            var model = new DepositModel();
            ViewBag.UserId = LOGGEDIN_USER.UserID;
            ViewBag.IsPlatformAssigned = _platformManager.GetUserAssignedPlatforms(LOGGEDIN_USER.UserID).Count > 0;
            ViewBag.DepositTypes = _paymentTypeManager.GetPaymentTypeSelectList();

            var history_model = new ReportSearchModel
            {
                SortBy = "CreatedAt",
                SortOrder = "Desc",
                VendorId = LOGGEDIN_USER.UserID
            };

            var deposits = new PagingResult<DepositListingModel>();
            deposits = _depositManager.GetReportsPagedHistoryList(history_model, false, LOGGEDIN_USER.AgencyId);
            var depositsPendLIst  = _depositManager.GetPendingDepositForCustomer(LOGGEDIN_USER.UserID, LOGGEDIN_USER.AgencyId);
            ViewBag.WalletHistory = depositsPendLIst.Concat(deposits.List).ToList();

            ViewBag.ChkBankName = new SelectList(_bankAccountManager.GetBankNames_API().ToList(), "BankName", "BankName");
            var posList = _posManager.GetPOSWithNameSelectList(LOGGEDIN_USER.UserID, LOGGEDIN_USER.AgencyId);
            ViewBag.userPos = posList;
            if (string.IsNullOrEmpty(posId) && posList.Count > 0)
            {
                //posId = Convert.ToInt64(posList[0].Value);
                ViewBag.posId = posId;
            }
            if (!string.IsNullOrEmpty(posId))
            {
                ViewBag.Percentage = _posManager.GetPosCommissionPercentage(long.Parse(posId));
                ViewBag.balance = _posManager.GetPosBalance(long.Parse(posId));
            }
            else
            {
                ViewBag.Percentage = 0;
                ViewBag.balance = 0;
            }
            var bankAccounts = _bankAccountManager.GetBankAccounts();

            ViewBag.bankAccounts = bankAccounts.ToList().Select(p => new SelectListItem { Text = "(" + p.BankName + " - " + Utilities.FormatBankAccount(p.AccountNumber) + ")", Value = p.BankAccountId.ToString() }).ToList();

            ViewBag.walletBalance = _userManager.GetUserWalletBalance(LOGGEDIN_USER.UserID, LOGGEDIN_USER.AgencyId);
            return View(model);
        }
    
        [AjaxOnly, HttpPost]
        public JsonResult AddDeposit(DepositModel model)
        {
            ActionOutput<PendingDeposit> pd = null;
            if (model.PosId == 0)
            {
                return JsonResult(new ActionOutput { Message = "POS Required", Status = ActionStatus.Error });
            }

            if (model.ContinueDepoit == 0)
            {
                var pendingDeposits = _depositManager.ReturnPendingDepositsTotalAmount(model);
                if (pendingDeposits > 0)
                {
                    return JsonResult(new ActionOutput { Message = Utilities.FormatAmount(pendingDeposits), Status = ActionStatus.Successfull });
                }
            }
            pd = _depositManager.SaveDepositRequest(model);
            string mesg = pd.Message;
            if (pd.Object.User.AutoApprove.Value)
            {
                ActionOutput result = _depositManager.ChangeDepositStatus(pd.Object.PendingDepositId, DepositPaymentStatusEnum.Released, true);

                var deposit = _depositManager.GetDeposit(pd.Object.PendingDepositId);
                SendEmailOnDepositApproval(deposit);
                SendEmailToAdminOnDepositApproval(deposit, result.ID);
                SendSmsOnDepositApproval(deposit);

                _depositManager.DeletePendingDeposits(deposit);
            }
            else
            {
                var adminUsers = _userManager.GetAllAdminUsersByDepositRelease();

                var pos = _posManager.GetSinglePos(pd.Object.POSId);
                if (pos != null)
                {
                    foreach (var admin in adminUsers)
                    {
                        var emailTemplate = _templateManager.GetEmailTemplateByTemplateType(TemplateTypes.DepositRequestNotification);
                        if (emailTemplate != null)
                        {
                            if (emailTemplate.TemplateStatus)
                            {
                                string body = emailTemplate.TemplateContent;
                                body = body.Replace("%AdminUserName%", admin.Name);
                                body = body.Replace("%VendorName%", pos.User.Vendor);
                                body = body.Replace("%POSID%", pos.SerialNumber);
                                body = body.Replace("%REF%", pd.Object.CheckNumberOrSlipId);
                                body = body.Replace("%Amount%", Utilities.FormatAmount(pd.Object.Amount));
                                body = body.Replace("%CurrencyCode%", Utilities.GetCountry().CurrencyCode);
                                Utilities.SendEmail(admin.Email, emailTemplate.EmailSubject, body);
                                Utilities.SendEmail("vblell@gmail.com", emailTemplate.EmailSubject, body);
                            }

                        }
                    }
                }
            }

            
            return JsonResult(new ActionOutput { Message = mesg, Status = pd.Status });
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

            if (dep.POS!= null)
            {
                foreach (var admin in adminUsers)
                {
                    string body =$"<p>Greetings {admin.Name}, </p>" +
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
                    Recipient = Utilities.GetCountry().CountryCode  + deposit.POS.Phone,
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

        [AjaxOnly]
        public JsonResult GetBankAccountDetail(int bankAccountId)
        {
            return Json(new { bankAccount = _bankAccountManager.GetBankAccountDetail(bankAccountId) }, JsonRequestBehavior.AllowGet);
        }

        [AjaxOnly, HttpPost, Public]
        public ActionResult GetDepositDetails(RequestObject tokenobject)
        {
            var result = _depositManager.GetDepositDetail(Convert.ToInt64(tokenobject.token_string));
            if (result.Object == null)
                return Json(new { Success = false, Code = 302, Msg = result.Message });
            return PartialView("_depositReceipt", result.Object);
        }
        [AjaxOnly, HttpPost, Public]
        public ActionResult GetPendingDepositDetails(RequestObject tokenobject)
        {
            var result = _depositManager.GetPendingDepositDetail(Convert.ToInt64(tokenobject.token_string));
            if (result.Object == null)
                return Json(new { Success = false, Code = 302, Msg = result.Message });
            return PartialView("_depositReceipt", result.Object);
        }

        string LogExceptionToDatabase(Exception exc)
        {
            var context = new VendtechEntities();
            ErrorLog errorObj = new ErrorLog();
            errorObj.Message = exc.Message;
            errorObj.StackTrace = exc.StackTrace;
            errorObj.InnerException = exc.InnerException == null ? "" : exc.InnerException.Message;
            errorObj.LoggedInDetails = "";
            errorObj.LoggedAt = DateTime.UtcNow;
            errorObj.UserId = 0;
            context.ErrorLogs.Add(errorObj);
            // To do
            context.SaveChanges();
            return errorObj.ErrorLogID.ToString();
        }
    }
}