using VendTech.Attributes;
using VendTech.BLL.Interfaces;
using VendTech.BLL.Models;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using System.Web.UI;
using System.IO;
using Newtonsoft.Json;
using VendTech.BLL.Common;
using System.Threading.Tasks;

namespace VendTech.Areas.Admin.Controllers
{
    public class RTSEDSAReportController : AdminBaseController
    {
        #region Variable Declaration
        private readonly IRTSEDSAManager manager;
        private readonly IVendorManager _vendorManager;
        private readonly IBankAccountManager _bankAccountManager;
        private readonly IAgencyManager _agencyManager;
        private readonly IPaymentTypeManager _paymentTypeManager;
        private readonly IDepositManager _depositManager;
        private readonly IMeterManager _meterManager;
        public RTSEDSAReportController(IErrorLogManager errorLogManager, IRTSEDSAManager manager, IVendorManager vendorManager, IBankAccountManager bankAccountManager, IAgencyManager agencyManager, IPaymentTypeManager paymentTypeManager, IDepositManager depositManager, IMeterManager meterManager) : base(errorLogManager)
        {
            this.manager = manager;
            _vendorManager = vendorManager;
            _bankAccountManager = bankAccountManager;
            _agencyManager = agencyManager;
            _paymentTypeManager = paymentTypeManager;
            _depositManager = depositManager;
            _meterManager = meterManager;
        }
        #endregion



        #region Report

        [HttpGet]
        public ActionResult Inquiry()
        {

            var vendors = _vendorManager.GetVendorsSelectList();
            ViewBag.vendors = vendors;
            ViewBag.SelectedParentTab = SelectedAdminTab.RTSEDSA;
            return View("Inquiry", new PagingResult<RtsedsaTransaction>());
        }
        //[HttpGet]
        //public ActionResult Inquiry(int type = 0, long vendorId = 0, long pos = 0, string meter = "", string transactionId = "", string from = null, string to = null, string source = "")
        //{
        //    ViewBag.SelectedTab = SelectedAdminTab.RTSInquiry;
        //    ViewBag.SelectedParentTab = SelectedAdminTab.RTSEDSA;

        //    ViewBag.Pritdatetime = BLL.Common.Utilities.GetLocalDateTime().ToString("dd/MM/yyyy hh:mm:ss tt");

        //    ViewBag.Vendors = _vendorManager.GetVendorsSelectList();
        //    ViewBag.PosId = _vendorManager.GetPosSelectList();
        //    ViewBag.Agencies = _agencyManager.GetAgentsSelectList();
        //    var assignedReportModule = _userManager.GetAssignedReportModules(LOGGEDIN_USER.UserID, LOGGEDIN_USER.UserType == UserRoles.Admin);

        //    ViewBag.DepositTypes = _paymentTypeManager.GetPaymentTypeSelectList();
        //    //ViewBag.SelectedTab = SelectedAdminTab.Reports;

        //    if (source == "dashboard")
        //    {
        //        from = DateTime.UtcNow.ToString();
        //        to = DateTime.UtcNow.ToString();
        //    }
        //    if (from == null)
        //    {
        //        from = DateTime.UtcNow.ToString();
        //    }
        //    if (to == null)
        //    {
        //        to = DateTime.UtcNow.ToString();
        //    }

        //    var model = new ReportSearchModel
        //    {
        //        SortBy = "CreatedAt",
        //        SortOrder = "Desc",
        //        PageNo = 1,
        //        VendorId = vendorId,
        //        PosId = pos,
        //        Meter = meter,
        //        TransactionId = transactionId,
        //        From = DateTime.Parse(from),
        //        To = DateTime.Parse(to)
        //    };

        //    var deposits = new PagingResult<DepositListingModel>();
        //    var depositAudit = new PagingResult<DepositAuditModel>();

        //    ViewBag.AssignedReports = assignedReportModule;
        //    var bankAccounts = _bankAccountManager.GetBankAccounts();
        //    ViewBag.Banks = bankAccounts.ToList().Select(p => new SelectListItem { Text = p.BankName, Value = p.BankAccountId.ToString() }).ToList();

        //    if (assignedReportModule.Count > 0 && (type > 0 ? assignedReportModule.Any(x => x.Value == type.ToString()) : true))
        //    {
        //        var val = assignedReportModule[0].Value;
        //        var rec = assignedReportModule.FirstOrDefault(p => p.Value == type.ToString());
        //        if (rec != null)
        //        {
        //            val = type.ToString();
        //            val = type.ToString();
        //        }
        //        /// This Is Used For Fetching DEPOSITS REPORT
        //        if (val == "17")
        //        {
        //            deposits = _depositManager.GetReportsPagedList(model, true);
        //            return View(deposits);
        //        }
        //        /// This Is Used For Fetching SALES REPORT
        //        if (val == "16")
        //        {
        //            model.IsInitialLoad = true;

        //            ViewBag.Products = new List<SelectListItem> {
        //                new SelectListItem { Value = "", Text = "SELECT PRODUCT" },
        //                new SelectListItem { Value = "EDSA", Text = "EDSA" },
        //                new SelectListItem { Value = "ORANGE", Text = "ORANGE" },
        //                new SelectListItem { Value = "AFRICELL", Text = "AFRICELL" }
        //            };

        //            var recharges = new PagingResult<MeterRechargeApiListingModel>();  // ??new PagingResult<MeterRechargeApiListingModel>();
        //            return View(recharges);
        //        }

        //    }
        //    return View(deposits);
        //}

        [HttpGet]
        public ActionResult Transactions()//SHIFT INQUIRY
        {
            ViewBag.SelectedParentTab = SelectedAdminTab.RTSEDSA;
            return View("Transactions", new PagingResult<RtsedsaTransaction>());
        }

        [AjaxOnly, HttpPost]
        public ActionResult GetMeterNumbers(long userid)
        {
            var meterNumbers = manager.MeterNumbers(userid);
            return Json(new { result = JsonConvert.SerializeObject(meterNumbers) });
        }


        [AjaxOnly, HttpPost]//SHIFT INQURY
        public async Task<ActionResult> GetTransactionsAsync(string date)
        {

            var model = new TransactionRequest
            {
                Date = date,
            };
            var respponse = await manager.GetTransactionsAsync(model);
            return Json(new {result = JsonConvert.SerializeObject(respponse.List) });
        }
        [AjaxOnly, HttpPost]
        public ActionResult GetSalesInquiry(string fromdate, string todate, string meterSerial)
        {
            ViewBag.SelectedTab = SelectedAdminTab.Deposits;

            var model = new InquiryRequest
            {
                FromDate = fromdate, //Utilities.ConvertDateToEpochDate(
                ToDate = todate, //Utilities.ConvertDateToEpochDate(,
                MeterSerial = meterSerial
            };
            var respponse = manager.GetSalesInquiry(model).Result;
            return Json(new { result = JsonConvert.SerializeObject(respponse.List) });
        }


        public async Task ExportRTSEDSATransactions(ReportSearchModel model2, string ExportType, string frmD, string PrintedDateServer)
        {

            var model = new TransactionRequest
            {
                Date = frmD,
            };
            var respponse = await manager.GetTransactionsAsync(model);
            var gv = new GridView
            {
                DataSource = respponse.List,
            };
            gv.DataBind();
            if (respponse.List.Count > 0)
            {
                gv.HeaderRow.Cells[0].Text = "ACCOUNT";
                gv.HeaderRow.Cells[1].Text = "CODEUSER";
                gv.HeaderRow.Cells[2].Text = "CUSTOMER"; 
                gv.HeaderRow.Cells[3].Text = "DATE TRANSACTION";
                gv.HeaderRow.Cells[4].Text = "DEBT PAYMENT";
                gv.HeaderRow.Cells[5].Text = "METER SERIAL";
                gv.HeaderRow.Cells[6].Text = "RECEIPT"; 
                gv.HeaderRow.Cells[7].Text = "AMOUNT"; 
                gv.HeaderRow.Cells[8].Text = "TRANS ID"; 
                gv.HeaderRow.Cells[9].Text = "UNIT";
                gv.HeaderRow.Cells[10].Text = "UNIT PAYMENT";
                gv.HeaderRow.Cells[11].Text = "UNIT TYPE";
            }

            if (ExportType == "Excel")
            {
                string filename = "RTSEDSA" + PrintedDateServer + ".xls";
                Response.ClearContent();
                Response.Buffer = true;
                Response.AddHeader("content-disposition", "attachment; filename=\"" + filename + "\"");
                Response.ContentType = "application/ms-excel";
                Response.Charset = "";
                StringWriter objStringWriter = new StringWriter();
                HtmlTextWriter objHtmlTextWriter = new HtmlTextWriter(objStringWriter);
                gv.RenderControl(objHtmlTextWriter);
                Response.Output.Write(objStringWriter.ToString());
                Response.Flush();
                Response.End();
            }

        }

        public async Task ExportRTSEDSASaleInquiry(ReportSearchModel model2, string ExportType, string frmD, string toD, string meterSerial, string PrintedDateServer)
        {



            var model = new InquiryRequest
            {
                FromDate = frmD,
                ToDate = toD,
                MeterSerial = meterSerial
            };
            var respponse = await manager.GetSalesInquiry(model);
            var gv = new GridView
            {
                DataSource = respponse.List,

            };
            gv.DataBind();
            if (respponse.List.Count > 0)
            {
                gv.HeaderRow.Cells[0].Text = "ACCOUNT";
                gv.HeaderRow.Cells[1].Text = "CODEUSER";
                gv.HeaderRow.Cells[2].Text = "CUSTOMER";
                gv.HeaderRow.Cells[3].Text = "DATE TRANSACTION";
                gv.HeaderRow.Cells[4].Text = "DEBT PAYMENT";
                gv.HeaderRow.Cells[5].Text = "METER SERIAL";
                gv.HeaderRow.Cells[6].Text = "RECEIPT";
                gv.HeaderRow.Cells[7].Text = "AMOUNT";
                gv.HeaderRow.Cells[8].Text = "TRANS ID";
                gv.HeaderRow.Cells[9].Text = "UNIT";
                gv.HeaderRow.Cells[10].Text = "UNIT PAYMENT";
                gv.HeaderRow.Cells[11].Text = "UNIT TYPE";
            }


            if (ExportType == "Excel")
            {
                string filename = "RTSEDSA" + PrintedDateServer + ".xls";
                Response.ClearContent();
                Response.Buffer = true;
                Response.AddHeader("content-disposition", "attachment; filename=\"" + filename + "\"");
                Response.ContentType = "application/ms-excel";
                Response.Charset = "";
                StringWriter objStringWriter = new StringWriter();
                HtmlTextWriter objHtmlTextWriter = new HtmlTextWriter(objStringWriter);
                gv.RenderControl(objHtmlTextWriter);
                Response.Output.Write(objStringWriter.ToString());
                Response.Flush();
                Response.End();

            }

        }

        #endregion


    }
}