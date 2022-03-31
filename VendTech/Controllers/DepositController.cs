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
using VendTech.Areas.Admin.Controllers;
using static VendTech.Controllers.MeterController;
using System.Threading.Tasks;
using System.Net.Http;
using System.Collections.Generic;
using System.Net;
using Newtonsoft.Json;
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
            ViewBag.WalletHistory = deposits.List;

            ViewBag.ChkBankName = new SelectList(_bankAccountManager.GetBankNames_API().ToList(), "BankName", "BankName");
            var posList = _posManager.GetPOSSelectList(LOGGEDIN_USER.UserID, LOGGEDIN_USER.AgencyId);
            ViewBag.userPos = posList;
            if (posId == "" && posList.Count > 0)
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

            return View(model);
        }
        [AjaxOnly, HttpPost]
        public JsonResult AddDeposit(DepositModel model)
        {
            // model.UserId = LOGGEDIN_USER.UserID; 
            if (model.PosId == 0)
            {
                return JsonResult(new ActionOutput { Message = "POS Required", Status = ActionStatus.Error });
            } 
                if (model.ContinueDepoit == 0)
            { 
                var pendingDeposits = _depositManager.ReturnPendingDepositsTotalAmount(model);
                if (pendingDeposits > 0)
                {
                    return JsonResult(new ActionOutput { Message = string.Format("{0:N0}", pendingDeposits), Status = ActionStatus.Successfull });
                }
            }
                 
            var result = _depositManager.SaveDepositRequest(model);

            var adminUsers = _userManager.GetAllAdminUsersByDepositRelease();

            var pos = _posManager.GetSinglePos(result.Object.POSId);
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
                            body = body.Replace("%REF%", result.Object.CheckNumberOrSlipId);
                            body = body.Replace("%Amount%", string.Format("{0:N0}", result.Object.Amount));
                            Utilities.SendEmail(admin.Email, emailTemplate.EmailSubject, body);
                        }

                    }
                }
            }
            return JsonResult(new ActionOutput { Message = result.Message, Status = result.Status });
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

        [AjaxOnly, HttpPost]
        public async Task<ActionResult> SmartIFrame(DepositModel model = null)
        {
            var pos = _posManager.GetPosDetail(model.PosId);
            var vendor = _vendorManager.GetVendorDetail(pos.VendorId??0);
            var apiClientID = 7;
            var serviceID = 33;
            var clientIDNumber =vendor.VendorId;
            var currency = "SLL";
            var billRefNumber = Utilities.GenerateByAnyLength(8).ToUpper();// "123ABC";
            var billDesc = $"{vendor.Name} 's deposit for a sum amount of {model.Amount}";
            var clientName = vendor.Name + " " + vendor.SurName;
            var key = "LUFMz+8Z/CMEGi+z";
            var secret = "0mXfFg1ueMwnZqY4ewPmbjZeJBmhGzjn";
            var clientMSISDN = DateTime.UtcNow.Year +""+ "" + DateTime.UtcNow.Day + "" + DateTime.UtcNow.Month + ""+ DateTime.UtcNow.Second + "" +DateTime.UtcNow.Minute;
            var clientEmail = vendor.Email;
            var callBackURLOnSuccess = "http://vendtechsl.net/api/Deposit/Smartkorpornotification";
            var notificationURL = callBackURLOnSuccess;

            var data_string = apiClientID + "" + model.Amount + "" + serviceID + "" + clientIDNumber + "" + currency + "" + billRefNumber + "" + billDesc + "" + clientName + "" + secret;
            var hash = Utilities.SHA256(data_string, key);
            var secureHash = Utilities.Base64Encode(hash);

            var formContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("apiClientID", apiClientID.ToString()),
                new KeyValuePair<string, string>("secureHash", secureHash),
                new KeyValuePair<string, string>("billDesc", billDesc),
                new KeyValuePair<string, string>("billRefNumber", billRefNumber),
                new KeyValuePair<string, string>("currency", currency),
                new KeyValuePair<string, string>("serviceID", serviceID.ToString()),
                new KeyValuePair<string, string>("clientMSISDN", clientMSISDN.ToString()),
                new KeyValuePair<string, string>("clientName", clientName),
                new KeyValuePair<string, string>("clientIDNumber", clientIDNumber.ToString()),
                new KeyValuePair<string, string>("clientEmail", clientEmail),
                new KeyValuePair<string, string>("callBackURLOnSuccess", callBackURLOnSuccess),
                new KeyValuePair<string, string>("notificationURL", notificationURL),
                new KeyValuePair<string, string>("amountExpected", model.Amount.ToString()),
            });

            try
            {


                var client = new HttpClient();
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                var response = await client.PostAsync("https://app.smartkorpor.com/PaymentAPI/invoice/checkout", formContent);

                var stringContent = await response.Content.ReadAsStringAsync();

                return JsonResult(new ActionOutput { Message = stringContent, Status = ActionStatus.Successfull });
            }
            catch (Exception ed)
            {
                throw;
            }
        }

      


    }
}