using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using VendTech.Attributes;
using VendTech.BLL.Common;
using VendTech.BLL.Interfaces;
using VendTech.BLL.Models;
using VendTech.BLL.Models.CurrencyModel;
using static VendTech.Controllers.MeterController;

namespace VendTech.Areas.Admin.Controllers
{
    public class CurrencyController : AdminBaseController
    {
        #region Variable Declaration
        private readonly IPOSManager _posManager;
        private readonly IVendorManager _vendorManager;
        private readonly IEmailTemplateManager _templateManager;
        private readonly IUserManager _userManager;
        private readonly ICommissionManager _commissionManager;
        private readonly IMeterManager _meterManager;
        private readonly ICurrencyManager _currencyManager;
        #endregion

        public CurrencyController(IPOSManager posManager,
            IErrorLogManager errorLogManager,
            IEmailTemplateManager templateManager,
            IVendorManager vendorManager,
            IUserManager userManager,
            ICommissionManager commissionManager,
            IMeterManager meterManager,
            ICurrencyManager currencyManager)
            : base(errorLogManager)
        {
            _posManager = posManager;
            _templateManager = templateManager;
            _vendorManager = vendorManager;
            _userManager = userManager;
            _commissionManager = commissionManager;
            _meterManager = meterManager;
            _currencyManager = currencyManager;
        }

        #region Currency Management

        [HttpGet]
        public ActionResult ManageCurrency()
        {
            ViewBag.SelectedTab = SelectedAdminTab.Platforms;
            var users = _currencyManager.GetCurrencyPagedList(PagingModel.DefaultModel("Name", "Desc"));
            return View(users);
        }


       
       
        [AjaxOnly, HttpPost]
        public JsonResult GetCurrencyPagingList(PagingModel model)
        {
            ViewBag.SelectedTab = SelectedAdminTab.POS;
            var modal = _currencyManager.GetCurrencyPagedList(model);
            List<string> resultString = new List<string>();
            resultString.Add(RenderRazorViewToString("Partials/_currencyListing", modal));
            resultString.Add(modal.TotalCount.ToString());
            return JsonResult(resultString);
        }
        [HttpPost]
        public ActionResult AddEditCurrency(SaveCurrencyModel model)
        {
            ViewBag.SelectedTab = SelectedAdminTab.Currency;
            
            return JsonResult(_currencyManager.SaveCurrency(model));
        }

     
        [HttpGet]
        public ActionResult AddEditCurrency(string id = "")
        {
            ViewBag.SelectedTab = SelectedAdminTab.Currency;
            var model = new SaveCurrencyModel();
            var readonlyStr = "readonly";
            ViewBag.read = "";

            if (!string.IsNullOrEmpty(id))
            {
                ViewBag.read = readonlyStr;
                model = _currencyManager.GetSingle(id);
            }
            else
            {
                //model.PosSms = true;
                //model.WebPrint = true;
                //model.SMSNotificationDeposit = true;
                //model.SMSNotificationDeposit = true;
            }

            return View(model);
        }

        //[AjaxOnly, HttpPost]
        //public JsonResult DeleteCurrency(long Id)
        //{
        //    ViewBag.SelectedTab = SelectedAdminTab.Agents;
        //    return JsonResult(_posManager.DeletePos(Id));
        //}

        [AjaxOnly, HttpPost]
        public JsonResult EnableCurrency(string id)
        {
            return JsonResult(_currencyManager.ChangeCurrencyStatus(id, false));
        }

        [AjaxOnly, HttpPost]
        public JsonResult DisableCurrency(string id)
        {
            return JsonResult(_currencyManager.ChangeCurrencyStatus(id, true));
        }


        #endregion
    }
}