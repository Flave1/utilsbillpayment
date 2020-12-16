using VendTech.Attributes;
using VendTech.BLL.Interfaces;
using VendTech.BLL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using VendTech.BLL.Common;

namespace VendTech.Areas.Admin.Controllers
{
    public class BankAccountController : AdminBaseController
    {
        #region Variable Declaration
        private readonly IBankAccountManager _bankAccountManager;
        #endregion

        public BankAccountController(IBankAccountManager bankAccountManager, IErrorLogManager errorLogManager, IEmailTemplateManager templateManager, IVendorManager vendorManager)
            : base(errorLogManager)
        {
            _bankAccountManager = bankAccountManager;
        }


        [HttpGet]
        public ActionResult ManageBankAccounts()
        {
            ViewBag.SelectedTab = SelectedAdminTab.Platforms;
            var bankAccounts = _bankAccountManager.GetBankAccounts();
            return View(bankAccounts);
        }

        [HttpGet]
        public ActionResult EditBankAccount(long id)
        {
            ViewBag.SelectedTab = SelectedAdminTab.Platforms;
            var bankAccount = _bankAccountManager.GetBankAccountDetail(id);
            return View(bankAccount);
        }
        public ActionResult AddBankAccount()
        {
            ViewBag.SelectedTab = SelectedAdminTab.Platforms;
            return View(new BankAccountModel());
        }

        [AjaxOnly, HttpPost]
        public JsonResult UpdateBankAccountDetails(BankAccountModel model)
        {
            return JsonResult(_bankAccountManager.SaveBankAccount(model));
        }
        [AjaxOnly, HttpPost]
        public JsonResult AddBankAccountDetails(BankAccountModel model)
        {
            return JsonResult(_bankAccountManager.SaveBankAccount(model));
        }
               [AjaxOnly, HttpPost]
        public JsonResult Delete(int id)
        {
            return JsonResult(_bankAccountManager.Delete(id));
        }
    }
}