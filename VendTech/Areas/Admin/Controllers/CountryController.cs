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
    public class CountryController : AdminBaseV2Controller
    {
        #region Variable Declaration
        private readonly ICurrencyManager _currencyManager;
        #endregion

        public CountryController(
            IErrorLogManager errorLogManager,
            ICurrencyManager currencyManager)
            : base(errorLogManager)
        {
            _currencyManager = currencyManager;
        }

        #region Currency Management

        [HttpGet]
        public ActionResult Index()
        {
            ViewBag.SelectedTab = SelectedAdminTab.Platforms;
            var users = _currencyManager.GetCountryPagedList(PagingModel.DefaultModel("Name", "Desc"));
            return View(users);
        }

        [AjaxOnly, HttpPost]
        public JsonResult GetCountryPagingList(PagingModel model)
        {
            ViewBag.SelectedTab = SelectedAdminTab.POS;
            var modal = _currencyManager.GetCountryPagedList(model);
            List<string> resultString = new List<string>();
            resultString.Add(RenderRazorViewToString("Partials/_countryListing", modal));
            resultString.Add(modal.TotalCount.ToString());
            return JsonResult(resultString);
        }
        [HttpPost]
        public ActionResult AddEditCountry(SaveCountryModel model)
        {
            ViewBag.SelectedTab = SelectedAdminTab.Currency;
            
            return JsonResult(_currencyManager.SaveCountry(model));
        }

        [HttpGet]
        public ActionResult AddEdit(int id = 0)
        {
            ViewBag.SelectedTab = SelectedAdminTab.Currency;
            var model = new SaveCountryModel();
            var readonlyStr = "readonly";
            ViewBag.read = "";

            if (id > 1)
            {
                ViewBag.read = readonlyStr;
                model.CountryId = id;
                model = _currencyManager.GetSingle(id);
            }
            return View(model);
        }

        [AjaxOnly, HttpPost]
        public JsonResult EnableCountry(int id)
        {
            return JsonResult(_currencyManager.ChangeCountryStatus(id, false));
        }

        [AjaxOnly, HttpPost]
        public JsonResult DisableCountry(int id)
        {
            return JsonResult(_currencyManager.ChangeCountryStatus(id, true));
        }


        #endregion
    }
}