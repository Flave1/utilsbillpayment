using VendTech.Attributes;
using VendTech.BLL.Interfaces;
using VendTech.BLL.Models;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using System.Web.UI;
using System.IO;
using Newtonsoft.Json;
using System;
using System.Linq;
using Castle.Core.Internal;
using VendTech.BLL.Managers;

namespace VendTech.Areas.Admin.Controllers
{
    public class RTSEDSAReportController : AdminBaseController
    {
        #region Variable Declaration
        private readonly IRTSEDSAManager manager;
        private readonly IVendorManager _vendorManager;
        public RTSEDSAReportController(IErrorLogManager errorLogManager, IRTSEDSAManager manager, IVendorManager vendorManager) : base(errorLogManager)
        {
            this.manager = manager;
            _vendorManager = vendorManager;
        }
        #endregion



        #region Report

        [HttpGet]
        public ActionResult Inquiry()
        {

            var vendors = _vendorManager.GetVendorsSelectList();
            ViewBag.vendors = vendors;
            ViewBag.SelectedTab = SelectedAdminTab.RTSInquiry;
            ViewBag.SelectedParentTab = SelectedAdminTab.RTSEDSA;
            return View(new PagingResult<RtsedsaTransaction>());
        }
        [HttpGet]
        public ActionResult Transactions()
        {
            ViewBag.SelectedTab = SelectedAdminTab.RTSTransaction;
            ViewBag.SelectedParentTab = SelectedAdminTab.RTSEDSA;
            return View(new PagingResult<RtsedsaTransaction>());
        }

        [AjaxOnly, HttpPost]
        public ActionResult GetMeterNumbers(long userid)
        {
            var meterNumbers = manager.MeterNumbers(userid);
            return Json(new { result = JsonConvert.SerializeObject(meterNumbers) });
        }


        [AjaxOnly, HttpPost]
        public ActionResult GetTransactionsAsync(string date)
        {

            var model = new TransactionRequest
            {
                Date = Convert.ToInt64(date),
            };
            var respponse = manager.GetTransactionsAsync(model).Result;
            return Json(new {result = JsonConvert.SerializeObject(respponse.List) });
        }
        [AjaxOnly, HttpPost]
        public ActionResult GetSalesInquiry(string fromdate, string todate, string meterSerial)
        {
            ViewBag.SelectedTab = SelectedAdminTab.Deposits;

            var model = new InquiryRequest
            {
                FromDate = Convert.ToInt64(fromdate),
                ToDate = Convert.ToInt64(todate),
                MeterSerial = meterSerial
            };
            var respponse = manager.GetSalesInquiry(model).Result;
            return Json(new { result = JsonConvert.SerializeObject(respponse.List) });
        }


        public void ExportRTSEDSATransactions(ReportSearchModel model2, string ExportType, string frmD, string PrintedDateServer)
        {

            var model = new TransactionRequest
            {
                Date = Convert.ToInt64(frmD),
            };
            var respponse = manager.GetTransactionsAsync(model).Result;
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

        public void ExportRTSEDSASaleInquiry(ReportSearchModel model2, string ExportType, string frmD, string toD, string meterSerial, string PrintedDateServer)
        {



            var model = new InquiryRequest
            {
                FromDate = Convert.ToInt64(frmD),
                ToDate = Convert.ToInt64(toD),
                MeterSerial = meterSerial
            };
            var respponse = manager.GetSalesInquiry(model).Result;
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