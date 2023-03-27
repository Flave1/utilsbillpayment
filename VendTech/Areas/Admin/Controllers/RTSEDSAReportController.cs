using VendTech.Attributes;
using VendTech.BLL.Interfaces;
using VendTech.BLL.Models;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using System.Web.UI;
using System.IO;
using Newtonsoft.Json;

namespace VendTech.Areas.Admin.Controllers
{
    public class RTSEDSAReportController : AdminBaseController
    {
        #region Variable Declaration
        private readonly IRTSEDSAManager manager;
        public RTSEDSAReportController(IErrorLogManager errorLogManager, IRTSEDSAManager manager) : base(errorLogManager) => this.manager = manager;
        #endregion



        #region Report

        [HttpGet]
        public ActionResult Index(string date)
        {
            return View(new PagingResult<RtsedsaTransaction>());
        }

       
        [AjaxOnly, HttpPost]
        public ActionResult GetSalesReportsPagingList(string date)
        {
            ViewBag.SelectedTab = SelectedAdminTab.Deposits;

            var model = new UnixDateRequest
            {
                Date = date
            };
            var respponse = manager.GetTransactionsAsync(model).Result;
            //var resultString = new List<string> { RenderRazorViewToString("Partials/_salesListing", respponse), respponse.TotalCount.ToString() };
            return Json(new {result = JsonConvert.SerializeObject(respponse.List) });
        }


        public void ExportRTSEDSAReportTo(ReportSearchModel model2, string ExportType, string FromDate, string ToDate, string PrintedDateServer)
        {

            var model = new UnixDateRequest
            {
                Date = ""
            };
            var respponse = manager.GetTransactionsAsync(model).Result;
            var gv = new GridView
            {
                DataSource = respponse.List,

            };
            gv.DataBind();
            if (respponse.List.Count > 0)
            {

               

                gv.HeaderRow.Cells[0].Text = "ACCOUNT"; //DATE_TIME
                gv.HeaderRow.Cells[1].Text = "CUSTOMER"; //PRODUCT_TYPE
                gv.HeaderRow.Cells[2].Text = "DATE TRANSACTION"; //TRANSACTIONID
                gv.HeaderRow.Cells[3].Text = "DEBT PAYMENT"; //METER_NO
                gv.HeaderRow.Cells[4].Text = "METER SERIAL"; //VENDORNAME
                gv.HeaderRow.Cells[5].Text = "RECEIPT"; //POSID 
                gv.HeaderRow.Cells[6].Text = "AMOUNT"; //AMOUNT
                gv.HeaderRow.Cells[7].Text = "TRANS ID"; //AMOUNT
                gv.HeaderRow.Cells[8].Text = "UNIT"; //AMOUNT
                gv.HeaderRow.Cells[9].Text = "UNIT PAYMENT"; //AMOUNT
                gv.HeaderRow.Cells[10].Text = "UNIT TYPE"; //AMOUNT

                //foreach (GridViewRow row in gv.Rows)
                //{
                //    if (row.RowType == DataControlRowType.DataRow)
                //    {

                //        row.Cells[0].HorizontalAlign = HorizontalAlign.Right;
                //        row.Cells[1].HorizontalAlign = HorizontalAlign.Left;
                //        row.Cells[2].HorizontalAlign = HorizontalAlign.Right;
                //        row.Cells[3].HorizontalAlign = HorizontalAlign.Right;
                //        row.Cells[4].HorizontalAlign = HorizontalAlign.Left;
                //        row.Cells[5].HorizontalAlign = HorizontalAlign.Right;
                //        row.Cells[6].HorizontalAlign = HorizontalAlign.Left;
                //        row.Cells[7].HorizontalAlign = HorizontalAlign.Right;
                //        var token = row.Cells[6].Text.ToString();
                //        row.Cells[6].Text = token != "&nbsp;" ? BLL.Common.Utilities.FormatThisToken(token) : string.Empty;
                //        row.Cells[6].ColumnSpan = 2;
                //    }
                //}
            }


            if (ExportType == "Excel")
            {

                string filename = "SalesReport_" + PrintedDateServer + ".xls";
                Response.ClearContent();
                Response.Buffer = true;
                Response.AddHeader("content-disposition", "attachment; filename=\"" + filename + "\"");
                Response.ContentType = "application/ms-excel";
                //Response.ContentType = "application/application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                //Response.AppendHeader("content-disposition", "attachment; filename=\"" + filename + "\"");

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