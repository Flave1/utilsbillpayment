#region Default Namespaces
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
#endregion

#region Custom Namespaces
using VendTech.Attributes;
using VendTech.BLL.Interfaces;
using Ninject;
using VendTech.BLL.Models;
using System.Web.Script.Serialization;
using VendTech.BLL.Common;
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

    
        #endregion

        public DepositController(IUserManager userManager, IErrorLogManager errorLogManager, IAuthenticateManager authenticateManager, ICMSManager cmsManager,IDepositManager depositManager,IMeterManager meterManager,IVendorManager vendorManager,IBankAccountManager bankAccountManager,IPOSManager posManager)
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
        }

        /// <summary>
        /// Index View 
        /// </summary>
        /// <returns></returns>
        public ActionResult Index(long posId=0)
        {
            var model = new DepositModel();
            ViewBag.IsPlatformAssigned = _platformManager.GetUserAssignedPlatforms(LOGGEDIN_USER.UserID).Count > 0;
            ViewBag.DepositTypes = Utilities.EnumToList(typeof(DepositPaymentTypeEnum));
            //ViewBag.Pos = _userManager.GetUserDetailsByUserId(LOGGEDIN_USER.UserID).POSNumber;
            //ViewBag.Percentage = _posManager.GetPosCommissionPercentageByUserId(LOGGEDIN_USER.UserID);
            //model.Deposits = _depositManager.GetUserDepositList(1, 10, LOGGEDIN_USER.UserID).List;
            //model.Recharges = _meterManager.GetUserMeterRecharges(LOGGEDIN_USER.UserID, 1, 10).List;


            ViewBag.ChkBankName = new SelectList(_bankAccountManager.GetBankNames_API().ToList(), "BankName", "BankName");
            var posList = _posManager.GetPOSSelectList(LOGGEDIN_USER.UserID);
            ViewBag.userPos = posList;
            if (posId==0 &&  posList.Count > 0)
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

            ViewBag.bankAccounts = bankAccounts.ToList().Select(p => new SelectListItem { Text = "(" + p.BankName+" - " + Utilities.FormatBankAccount(p.AccountNumber) + ")", Value = p.BankAccountId.ToString() }).ToList();
           
            return View(model);
        }
        [AjaxOnly, HttpPost]
        public JsonResult AddDeposit(DepositModel model)
        {
            model.UserId = LOGGEDIN_USER.UserID;

            return JsonResult(_depositManager.SaveDepositRequest(model));
        }
        [AjaxOnly]
        public JsonResult GetBankAccountDetail(int bankAccountId)
        {
            return Json(new { bankAccount = _bankAccountManager.GetBankAccountDetail(bankAccountId) }, JsonRequestBehavior.AllowGet);
        }
    }
}