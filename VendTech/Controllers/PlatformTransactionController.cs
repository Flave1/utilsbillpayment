using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using VendTech.BLL.Interfaces;
using VendTech.BLL.Managers;
using VendTech.BLL.Models;
using VendTech.BLL.PlatformApi;
using PagedList;
using VendTech.DAL;

namespace VendTech.Controllers
{
    public class PlatformTransactionController : AppUserBaseController
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
            int? status)
        {
            int PlatformId = pid ?? 0;
            int PageNumber = page ?? 1;
            int Status = status ?? -1;

            DateTime searchFromDate = ( ! string.IsNullOrEmpty(fromDate)) ? DateTime.Parse(fromDate) : new DateTime(2000, 1, 1);
            DateTime searchToDate = (!string.IsNullOrEmpty(toDate))
                ? DateTime.Parse(toDate) : DateTime.Today.AddDays(1).AddTicks(-1);

            //If it was selected in the search form, then we must set
            //the seconds and milliseconds to the max because the
            //Datetime picker only allows hours and minutes to be selected.
            if ( ! string.IsNullOrEmpty(toDate))
            {
                searchToDate = searchToDate.AddSeconds(59).AddMilliseconds(999);
            }

            DataQueryModel QueryModel = new DataQueryModel
            {
                PlatformId = PlatformId,
                UserId = LOGGEDIN_USER.UserID,
                PageSize = DataQueryModel.DEFAULT_PAGE_SIZE,
                Page = PageNumber,
                Reference = reference,
                Beneficiary = beneficiary,
                FromDate = searchFromDate, 
                ToDate = searchToDate,
                Status = Status,
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

            ViewBag.PlatformList = _platformManager.GetPlatforms();
            ViewBag.StatusList = ModelUtils.GetTransactionStatusEnumSelectItemList();
            List<SelectListItem> productsSelectItems = PlatformModel.ConvertToSelectListItems(_platformManager.GetUserAssignedPlatforms(LOGGEDIN_USER.UserID));
            ViewBag.Products = productsSelectItems;

            ViewBag.QueryModel = QueryModel;

            string searchForm = Request.Form["_searchForm"] as string;
            ViewBag.IsSearchForm = ! string.IsNullOrEmpty(searchForm) && searchForm == "yes";

            return View(result.PagedList);
        }

        [HttpGet]
        public ActionResult Execute(int? pid)
        {
            int PlatformId = pid ?? 0;
            PlatformTransactionModel transactionModel = new PlatformTransactionModel { PlatformId = PlatformId };
            return DisplayTransactionExecutionView(transactionModel);
        }

        [HttpPost]
        public ActionResult Execute(PlatformTransactionModel model)
        {
            PlatformModel platform = _platformManager.GetPlatformById(model.PlatformId);

            bool hasError = false;
            if (ModelState.IsValidField("Amount"))
            {
                if (model.Amount <= 0)
                {
                    hasError = true;
                    //TODO - get the minimum and max values for the platform product
                    ModelState.AddModelError("Amount", "Amount must be greater than 0");
                }
            }

            //TODO - Validate phone number / Meter Number

            if (hasError)
            {
                return DisplayTransactionExecutionView(model);
            }

            //TODO - how is the currency gotten for a user?
            string currency = "SLE";

            PlatformModel pm = _platformManager.GetPlatformById(model.PlatformId);

            PlatformTransactionModel newTranx = _platformTransactionManager.New(
                LOGGEDIN_USER.UserID, model.PlatformId, model.PosId ?? 0, model.Amount, model.Beneficiary, currency, pm.PlatformApiConnId);

            //Check balance
            //deduct from balance

            //execute transaction
            _platformTransactionManager.ProcessTransactionViaApi(newTranx.Id);
            

            //redirect to same page with details


            Session.Add("_platformTranx_" + newTranx.Id, "xxxx");
            return RedirectToAction("ExecuteOutcome", new { id = newTranx.Id });
        }

        private ActionResult DisplayTransactionExecutionView(PlatformTransactionModel transactionModel)
        {
            List<SelectListItem> productsSelectItems = PlatformModel.ConvertToSelectListItems(_platformManager.GetUserAssignedPlatforms(LOGGEDIN_USER.UserID));
            ViewBag.Products = productsSelectItems;

            //Get the POS
            var posList = _posManager.GetPOSSelectList(LOGGEDIN_USER.UserID, LOGGEDIN_USER.AgencyId);
            ViewBag.UserPosList = posList;

            return View(transactionModel);
        }

        [HttpGet]
        public ActionResult ExecuteOutcome(long id)
        {
            PlatformTransactionModel tranx = 
                _platformTransactionManager.GetPlatformTransactionById(new DataQueryModel{ UserId = LOGGEDIN_USER.UserID }, id);

            if (tranx.IsNew()) return RedirectToAction("Index");

            string sessVariable = "_platformTranx_" + id;
            string inSess = Session[sessVariable] as string;

            if (string.IsNullOrWhiteSpace(inSess)) return RedirectToAction("Index");

            Session.Remove(sessVariable);

            return View(tranx);
        }
    }
}