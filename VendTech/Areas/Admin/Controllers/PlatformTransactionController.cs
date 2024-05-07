using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using VendTech.Attributes;
using VendTech.BLL.Interfaces;
using VendTech.BLL.Models;

namespace VendTech.Areas.Admin.Controllers
{
    public class PlatformTransactionController : AdminBaseV2Controller
    {
        private IPlatformApiManager _platformApiManager;
        private IPOSManager _posManager;
        private IPlatformTransactionManager _platformTransactionManager;

        public PlatformTransactionController(
            IUserManager userManager,
            IPlatformManager platformManager,
            IErrorLogManager errorLogManager,
            IPlatformApiManager platformApiManager,
            IPOSManager posManager,
            IPlatformTransactionManager platformTransactionManager)

           : base(errorLogManager)
        {
            _platformApiManager = platformApiManager;
            _platformManager = platformManager;
            _posManager = posManager;
            _platformTransactionManager = platformTransactionManager;
        }
        // GET: PlatformTransaction table
        public ActionResult Index(
            int? pid,
            string reference,
            string beneficiary,
            int? page,
            string fromDate,
            string toDate,
            int? status,
            int? apiConnId)
        {
            int PlatformId = pid ?? 0;
            int PageNumber = page ?? 1;
            int Status = status ?? -1;
            int ApiConnId = apiConnId ?? 0;

            DateTime searchFromDate = (!string.IsNullOrEmpty(fromDate)) ? DateTime.Parse(fromDate) : new DateTime(2000, 1, 1);

            //If it was selected in the search form, then we must set
            //the seconds and milliseconds to the max because the
            //Datetime picker only allows hours and minutes to be selected.
            DateTime searchToDate = (!string.IsNullOrEmpty(toDate))
                ? DateTime.Parse(toDate).AddSeconds(59).AddMilliseconds(999) : DateTime.Today.AddDays(1).AddTicks(-1);

            DataQueryModel QueryModel = new DataQueryModel
            {
                PlatformId = PlatformId,
                PageSize = DataQueryModel.DEFAULT_PAGE_SIZE,
                Page = PageNumber,
                Reference = reference,
                Beneficiary = beneficiary,
                FromDate = searchFromDate,
                ToDate = searchToDate,
                Status = Status,
                IsAdmin = true,
                ApiConnId = ApiConnId
            };

            DataTableResultModel<PlatformTransactionModel> result =
                _platformTransactionManager.GetPlatformTransactionsForDataTable(QueryModel);

            ViewBag.MainPageHeader = "Transactions";
            if (PlatformId > 0)
            {
                ViewBag.IsFilteredByPlatform = true;
                ViewBag.PlatformId = PlatformId;
                PlatformModel platform = _platformManager.GetPlatformById(PlatformId);
                ViewBag.MainPageHeader = platform.Title + " " + ViewBag.MainPageHeader;
            }

            ViewBag.PlatformIdStr = PlatformId.ToString();
            //ViewBag.PlatformList = _platformManager.GetPlatforms();
            List<SelectListItem> productsSelectItems = PlatformModel.ConvertToSelectListItems(_platformManager.GetPlatforms());
            ViewBag.Products = productsSelectItems;

            ViewBag.ApiConnList = _platformApiManager.GetAllPlatformApiConnectionsSelectList();
            ViewBag.StatusList = ModelUtils.GetTransactionStatusEnumSelectItemList();

            ViewBag.QueryModel = QueryModel;

            string searchForm = Request.Form["_searchForm"] as string;
            ViewBag.IsSearchForm = !string.IsNullOrEmpty(searchForm) && searchForm == "yes";

            return View(result.PagedList);
        }

        [AjaxOnly, HttpGet]
        public JsonResult GetTranxLogs(long tranxId)
        {
            var list = _platformTransactionManager.GetTransactionLogs(tranxId);
            return Json(list, JsonRequestBehavior.AllowGet);
        }
    }
}