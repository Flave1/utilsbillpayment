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


        #endregion

        public DepositController(IUserManager userManager, IErrorLogManager errorLogManager, IAuthenticateManager authenticateManager, ICMSManager cmsManager, IDepositManager depositManager, IMeterManager meterManager, IVendorManager vendorManager, IBankAccountManager bankAccountManager, IPOSManager posManager, IEmailTemplateManager templateManager)
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
        }

        /// <summary>
        /// Index View 
        /// </summary>
        /// <returns></returns>
        public ActionResult Index(long posId = 0)
        {
            var model = new DepositModel();
            ViewBag.IsPlatformAssigned = _platformManager.GetUserAssignedPlatforms(LOGGEDIN_USER.UserID).Count > 0;
            ViewBag.DepositTypes = Utilities.EnumToList(typeof(DepositPaymentTypeEnum));

            var history_model = new ReportSearchModel
            {
                SortBy = "CreatedAt",
                SortOrder = "Desc",
                VendorId = LOGGEDIN_USER.UserID
            };

            var deposits = new PagingResult<DepositListingModel>();
            deposits = _depositManager.GetReportsPagedHistoryList(history_model);
            ViewBag.WalletHistory = deposits.List;

            ViewBag.ChkBankName = new SelectList(_bankAccountManager.GetBankNames_API().ToList(), "BankName", "BankName");
            var posList = _posManager.GetPOSSelectList(LOGGEDIN_USER.UserID);
            ViewBag.userPos = posList;
            if (posId == 0 && posList.Count > 0)
            {
                posId = Convert.ToInt64(posList[0].Value);
                ViewBag.posId = posId;
            }
            if (posId > 0)
            {
                ViewBag.Percentage = _posManager.GetPosCommissionPercentage(posId);
                ViewBag.balance = _posManager.GetPosBalance(posId);
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
            model.UserId = LOGGEDIN_USER.UserID;

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
            // return JsonResult(result);
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
    }
}