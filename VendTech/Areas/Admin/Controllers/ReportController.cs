using VendTech.Attributes;
using VendTech.BLL.Interfaces;
using VendTech.BLL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using VendTech.BLL.Common;
using System.IO;
using System.Web.UI;
using System.Web.UI.WebControls;
using iTextSharp.text;
using iTextSharp.text.html.simpleparser;
using iTextSharp.text.pdf;
using System.Globalization;
using IronXL;
using System.Data;
using Microsoft.Office.Interop.Excel;
using System.Runtime.InteropServices;
using ClosedXML.Excel;
using System.ComponentModel;
using Quartz.Util;

namespace VendTech.Areas.Admin.Controllers
{
    public class ReportController : AdminBaseController
    {
        #region Variable Declaration
        // private readonly IUserManager _userManager;
        private readonly IVendorManager _vendorManager;
        private readonly IAgencyManager _agencyManager;
        private readonly IDepositManager _depositManager;
        private readonly IMeterManager _meterManager;
        private readonly IBankAccountManager _bankAccountManager;
        private readonly IPOSManager _posManager;
        #endregion

        public ReportController(IUserManager userManager,
            IErrorLogManager errorLogManager,
            IVendorManager vendorManager,
            IAgencyManager agencyManager,
            IDepositManager depositManager,
            IMeterManager meterManager,
            IBankAccountManager bankManager,
            IPOSManager posManager)
            : base(errorLogManager)
        {
            _userManager = userManager;
            _vendorManager = vendorManager;
            _agencyManager = agencyManager;
            _depositManager = depositManager;
            _meterManager = meterManager;
            _bankAccountManager = bankManager;
            _posManager = posManager;
        }

        #region Report

        [HttpGet]
        public ActionResult ManageReports(int type = 0)
        {
            ViewBag.Pritdatetime = BLL.Common.Utilities.GetLocalDateTime().ToString("dd/MM/yyyy hh:mm:ss tt");

            ViewBag.Vendors = _vendorManager.GetVendorsSelectList();
            ViewBag.PosId = _vendorManager.GetPosSelectList();
            ViewBag.Agencies = _agencyManager.GetAgentsSelectList();
            var assignedReportModule = _userManager.GetAssignedReportModules(LOGGEDIN_USER.UserID, LOGGEDIN_USER.UserType == UserRoles.Admin);

            ViewBag.DepositTypes = BLL.Common.Utilities.EnumToList(typeof(DepositPaymentTypeEnum));
            ViewBag.SelectedTab = SelectedAdminTab.Reports;
            var model = new ReportSearchModel
            {
                SortBy = "CreatedAt",
                SortOrder = "Desc",
                PageNo = 1,
            };
            model.IsInitialLoad = true;
            var deposits = new PagingResult<DepositListingModel>();
            var depositAudit = new PagingResult<DepositAuditModel>();

            ViewBag.AssignedReports = assignedReportModule;
            var bankAccounts = _bankAccountManager.GetBankAccounts();
            ViewBag.Banks = bankAccounts.ToList().Select(p => new SelectListItem { Text = p.BankName, Value = p.BankAccountId.ToString() }).ToList();

            if (assignedReportModule.Count > 0 && (type > 0 ? assignedReportModule.Any(x => x.Value == type.ToString()) : true))
            {
                var val = assignedReportModule[0].Value;
                var rec = assignedReportModule.FirstOrDefault(p => p.Value == type.ToString());
                if (rec != null)
                {
                    val = type.ToString();
                    val = type.ToString();
                }
                /// This Is Used For Fetching DEPOSITS REPORT
                if (val == "17")
                {
                    deposits = _depositManager.GetReportsPagedList(model, true);
                    return View(deposits);
                }
                /// This Is Used For Fetching SALES REPORT
                if (val == "16")
                {
                    var recharges = _meterManager.GetUserMeterRechargesReport(model, true);
                    return View("ManageSalesReports", recharges);
                }
                /// This Is Used For Fetching DEPOSIT AUDIT REPORT
                if (val == "21")
                {
                    ViewBag.IssuingBank = new SelectList(_bankAccountManager.GetBankNames_API().ToList(), "BankName", "BankName");
                    ViewBag.Vendor = new SelectList(_userManager.GetVendorNames_API().ToList(), "VendorId", "VendorName");

                    depositAudit = _depositManager.GetDepositAuditReports(model, true);

                    return View("ManageDepositAuditReport", depositAudit);
                }
                if (val == "2011")
                {
                    ViewBag.Balance = _depositManager.GetPendingDepositTotal();
                    var data = _depositManager.GetDepositPagedList(PagingModel.DefaultModel("CreatedAt", "Desc"), true);
                    return View("DepositReleaseReport", data);
                }
            }
            return View(deposits);
        }

        [HttpGet]
        public ActionResult GetVendorPosSelectList(long userId)
        {
            var posList = _posManager.GetVendorPos(userId);
            return Json(new { posList }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetPosPercentage(long pos)
        {
            var percentage = _posManager.GetPosPercentage(pos);
            return Json(new { percentage }, JsonRequestBehavior.AllowGet);
        }

        [AjaxOnly, HttpPost]
        public JsonResult GetDepositReleaseReportList(PagingModel model)
        {
            ViewBag.SelectedTab = SelectedAdminTab.Deposits;
            var modal = _depositManager.GetDepositPagedList(model, true);
            //List<string> resultString = new List<string>();

            //resultString.Add(RenderRazorViewToString("Partials/_depositReleaseListing", modal));
            //resultString.Add(modal.TotalCount.ToString());
            //return JsonResult(resultString);           

            var resultString = new List<string>
            {
                RenderRazorViewToString("Partials/_depositReleaseListing", modal),
                modal.TotalCount.ToString()
            };
            //resultString.Add(RenderRazorViewToString("Partials/_depositReleaseListing", modal));
            //resultString.Add(modal.TotalCount.ToString());
            return JsonResult(resultString);
        }
        [AjaxOnly, HttpPost]
        public JsonResult GetReportsPagingList(ReportSearchModel model)
        {
            ViewBag.SelectedTab = SelectedAdminTab.Deposits;
            model.RecordsPerPage = 100000000;
            var modal = new PagingResult<DepositListingModel>();
            var depositAuditModel = new PagingResult<DepositAuditModel>();
            if (model.ReportType == "17")
            {
                modal = _depositManager.GetReportsPagedList(model, true);
            }
            if (model.ReportType == "21")
            {
                depositAuditModel = _depositManager.GetAuditReportsPagedList(model, true);
            }
            //List<string> resultString = new List<string>();
            //resultString.Add(RenderRazorViewToString("Partials/_reportListing", modal));
            //resultString.Add(modal.TotalCount.ToString());
            if (model.ReportType == "17")
            {
                var resultString = new List<string> {
               RenderRazorViewToString("Partials/_reportListing", modal),
               modal.TotalCount.ToString()
            };
                return JsonResult(resultString);
            }
            if (model.ReportType == "21")
            {
                var resultString = new List<string> {
               RenderRazorViewToString("Partials/_depositAuditListing",depositAuditModel),
               depositAuditModel.TotalCount.ToString()};
                return JsonResult(resultString);
            }
            return JsonResult(new List<string>() { });
        }
        [AjaxOnly, HttpPost]
        public JsonResult GetSalesReportsPagingList(ReportSearchModel model)
        {
            ViewBag.SelectedTab = SelectedAdminTab.Deposits;
            //model.SortBy = "CreatedAt";
            //model.SortOrder = "Desc";

            model.RecordsPerPage = 1000000000;
            var modal = _meterManager.GetUserMeterRechargesReport(model, true);

            //List<string> resultString = new List<string>();
            //resultString.Add(RenderRazorViewToString("Partials/_salesReportListing", modal));
            //resultString.Add(modal.TotalCount.ToString());


            var resultString = new List<string> { RenderRazorViewToString("Partials/_salesReportListing", modal), modal.TotalCount.ToString()
           };
            return JsonResult(resultString);
        }

        [AjaxOnly, HttpPost]
        public JsonResult GetVendorsPagingList(PagingModel model)
        {
            ViewBag.SelectedTab = SelectedAdminTab.Users;
            var modal = _vendorManager.GetVendorsPagedList(model);
            var resultString = new List<string> {
               RenderRazorViewToString("Partials/_vendorListing", modal),
               modal.TotalCount.ToString()
           };
            return JsonResult(resultString);
        }

        public void ExportSalesReportTo(ReportSearchModel model, string ExportType, string FromDate, string ToDate, string PrintedDateServer)
        {
            PrintedDateServer = PrintedDateServer.TrimEnd(' ');
            string fromdate = "";
            string Todate = "";
            CultureInfo provider = CultureInfo.InvariantCulture;
            if (!string.IsNullOrEmpty(FromDate))
            {
                model.From = DateTime.ParseExact(FromDate, "dd/MM/yyyy", provider);
                fromdate = model.From.Value.ToString("dd/MM/yyyy");
            }

            if (!string.IsNullOrEmpty(ToDate))
            {
                model.To = DateTime.ParseExact(ToDate, "dd/MM/yyyy", provider);
                Todate = model.To.Value.ToString("dd/MM/yyyy");
            }

            var list = _meterManager.GetSalesExcelReportData(model, true).List;
            var gv = new GridView
            {
                DataSource = list,

            };
            gv.DataBind();
            if (list.Count > 0)
            {

                GridViewRow forbr = new GridViewRow(0, 0, DataControlRowType.Header, DataControlRowState.Normal);
                var tecbr = new TableHeaderCell
                {
                    ColumnSpan = 9,
                    Text = null,
                    HorizontalAlign = HorizontalAlign.Left,
                    BorderStyle = BorderStyle.None
                };
                forbr.BorderStyle = BorderStyle.None;
                forbr.Controls.Add(tecbr);
                gv.HeaderRow.Parent.Controls.AddAt(0, forbr);


                GridViewRow row3 = new GridViewRow(0, 0, DataControlRowType.Header, DataControlRowState.Normal);
                //TableHeaderCell tec3 = new TableHeaderCell();
                var tec3 = new TableHeaderCell
                {
                    ColumnSpan = 9,
                    Text = "PRINT DATE:  " + PrintedDateServer,
                    HorizontalAlign = HorizontalAlign.Left,
                    BorderStyle = BorderStyle.None
                };
                row3.BorderStyle = BorderStyle.None;
                row3.Controls.Add(tec3);
                gv.HeaderRow.Parent.Controls.AddAt(0, row3);


                GridViewRow forbrafterdate = new GridViewRow(0, 0, DataControlRowType.Header, DataControlRowState.Normal);
                var tecbrafterdate = new TableHeaderCell
                {
                    ColumnSpan = 9,
                    Text = null,
                    HorizontalAlign = HorizontalAlign.Left,
                    BorderStyle = BorderStyle.None
                };
                forbrafterdate.BorderStyle = BorderStyle.None;
                forbrafterdate.Controls.Add(tecbrafterdate);
                gv.HeaderRow.Parent.Controls.AddAt(0, forbrafterdate);

                GridViewRow row2 = new GridViewRow(0, 0, DataControlRowType.Header, DataControlRowState.Normal);
                var tec2 = new TableHeaderCell
                {
                    ColumnSpan = 9,
                    Text = "TO DATE:  " + Todate,
                    HorizontalAlign = HorizontalAlign.Left,
                    BorderStyle = BorderStyle.None,
                };
                row2.BorderStyle = BorderStyle.None;
                row2.Controls.Add(tec2);
                gv.HeaderRow.Parent.Controls.AddAt(0, row2);

                GridViewRow row22 = new GridViewRow(0, 0, DataControlRowType.Header, DataControlRowState.Normal);
                var tec22 = new TableHeaderCell
                {
                    ColumnSpan = 9,
                    Text = "FROM DATE:  " + fromdate,
                    HorizontalAlign = HorizontalAlign.Left,
                    BorderStyle = BorderStyle.None,
                };
                row22.BorderStyle = BorderStyle.None;
                row22.Controls.Add(tec22);
                gv.HeaderRow.Parent.Controls.AddAt(0, row22);

                GridViewRow row1 = new GridViewRow(0, 0, DataControlRowType.Header, DataControlRowState.Normal);
                //TableHeaderCell tec1 = new TableHeaderCell();
                var tec1 = new TableHeaderCell
                {
                    ColumnSpan = 9,
                    Text = "VENDTECH SALES REPORTS",
                    HorizontalAlign = HorizontalAlign.Left,
                    BorderStyle = BorderStyle.None,
                    BorderWidth = Unit.Pixel(20),
                };

                row1.Controls.Add(tec1);
                row1.BorderStyle = BorderStyle.None;
                row1.Style.Add(HtmlTextWriterStyle.FontSize, "large");
                gv.HeaderRow.Parent.Controls.AddAt(0, row1);

                //img
                GridViewRow imgRow = new GridViewRow(0, 0, DataControlRowType.Header, DataControlRowState.Normal);
                var imgHeader = new TableHeaderCell
                {
                    ColumnSpan = 9,
                    Text = "<img src='http://vendtechsl.net/Content/images/ventech.png' width='60'  style='border:1px solid red; text-align:center; margin:auto;'/>",
                    HorizontalAlign = HorizontalAlign.NotSet,
                    BorderStyle = BorderStyle.None,
                    BorderWidth = Unit.Pixel(20),
                };
                imgRow.Controls.Add(imgHeader);
                imgRow.BorderStyle = BorderStyle.Dotted;
                imgRow.Style.Add(HtmlTextWriterStyle.FontSize, "large");
                gv.HeaderRow.Parent.Controls.AddAt(0, imgRow);

                gv.HeaderRow.Cells[0].Text = "DATE/TIME"; //DATE_TIME
                gv.HeaderRow.Cells[1].Text = "PRODUCT"; //PRODUCT_TYPE
                gv.HeaderRow.Cells[2].Text = "TRANSACTION ID"; //TRANSACTIONID
                gv.HeaderRow.Cells[3].Text = "METER #"; //METER_NO
                gv.HeaderRow.Cells[4].Text = "VENDOR NAME"; //VENDORNAME
                gv.HeaderRow.Cells[5].Text = "POS ID"; //POSID 
                gv.HeaderRow.Cells[6].Text = "TOKEN"; gv.HeaderRow.Cells[6].ColumnSpan = 2;  //PIN
                gv.HeaderRow.Cells[7].Text = "AMOUNT"; //AMOUNT

                foreach (GridViewRow row in gv.Rows)
                {
                    if (row.RowType == DataControlRowType.DataRow)
                    {

                        row.Cells[0].HorizontalAlign = HorizontalAlign.Right;
                        row.Cells[1].HorizontalAlign = HorizontalAlign.Left;
                        row.Cells[2].HorizontalAlign = HorizontalAlign.Right;
                        row.Cells[3].HorizontalAlign = HorizontalAlign.Right;
                        row.Cells[4].HorizontalAlign = HorizontalAlign.Left;
                        row.Cells[5].HorizontalAlign = HorizontalAlign.Right;
                        row.Cells[6].HorizontalAlign = HorizontalAlign.Left;
                        row.Cells[7].HorizontalAlign = HorizontalAlign.Right;
                        var token = row.Cells[6].Text.ToString();
                        row.Cells[6].Text = token != "&nbsp;" ? BLL.Common.Utilities.FormatThisToken(token) : string.Empty;
                        row.Cells[6].ColumnSpan = 2;
                    }
                }
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
            else if (ExportType == "PDF")
            {
                string filename = "SalesReport_" + PrintedDateServer + ".pdf";
                Response.ContentType = "application/pdf";
                Response.AddHeader("content-disposition", "attachment;filename=\"" + filename + "\"");

                Response.Cache.SetCacheability(HttpCacheability.NoCache);
                StringWriter sw = new StringWriter();
                HtmlTextWriter hw = new HtmlTextWriter(sw);
                gv.RenderControl(hw);
                StringReader sr = new StringReader(sw.ToString());
                Document pdfDoc = new Document(PageSize.A2, 7f, 7f, 7f, 0f);
                HTMLWorker htmlparser = new HTMLWorker(pdfDoc);
                PdfWriter.GetInstance(pdfDoc, Response.OutputStream);
                pdfDoc.Open();
                htmlparser.Parse(sr);
                pdfDoc.Close();
                Response.Write(pdfDoc);
                Response.End();
                gv.AllowPaging = true;

                //Response.Charset = "";
                //StringWriter objStringWriter = new StringWriter();
                //HtmlTextWriter objHtmlTextWriter = new HtmlTextWriter(objStringWriter);
                //gv.RenderControl(objHtmlTextWriter);
                //Response.Output.Write(objStringWriter.ToString());
                //Response.Flush();
                //Response.End();
            }

        }
        public void ExportDepositReportTo(ReportSearchModel model, string ExportType, string FromDate, string ToDate, string PrintedDateServer)
        {
            string fromdate = "";
            string Todate = "";
            CultureInfo provider = CultureInfo.InvariantCulture;
            if (!string.IsNullOrEmpty(FromDate))
            {
                model.From = DateTime.ParseExact(FromDate, "dd/MM/yyyy", provider);
                fromdate = model.From.Value.ToString("dd/MM/yyyy");
            }

            if (!string.IsNullOrEmpty(ToDate))
            {
                model.To = DateTime.ParseExact(ToDate, "dd/MM/yyyy", provider);
                Todate = model.To.Value.ToString("dd/MM/yyyy");
            }

            var list = _depositManager.GetReportExcelData(model).List;

            var gv = new GridView
            {
                DataSource = list
            };
            gv.DataBind();
            if (list.Count > 0)
            {
                GridViewRow forbr = new GridViewRow(0, 0, DataControlRowType.Header, DataControlRowState.Normal);
                var tecbr = new TableHeaderCell
                {
                    ColumnSpan = 13,
                    Text = null,
                    HorizontalAlign = HorizontalAlign.Left,
                    BorderStyle = BorderStyle.None
                };
                forbr.BorderStyle = BorderStyle.None;
                forbr.Controls.Add(tecbr);
                gv.HeaderRow.Parent.Controls.AddAt(0, forbr);


                GridViewRow row3 = new GridViewRow(0, 0, DataControlRowType.Header, DataControlRowState.Normal);
                //TableHeaderCell tec3 = new TableHeaderCell();
                var tec3 = new TableHeaderCell
                {
                    ColumnSpan = 13,
                    Text = "PRINT DATE:  " + PrintedDateServer,
                    HorizontalAlign = HorizontalAlign.Left,
                    BorderStyle = BorderStyle.None
                };
                row3.BorderStyle = BorderStyle.None;
                row3.Controls.Add(tec3);
                gv.HeaderRow.Parent.Controls.AddAt(0, row3);


                GridViewRow forbrafterdate = new GridViewRow(0, 0, DataControlRowType.Header, DataControlRowState.Normal);
                var tecbrafterdate = new TableHeaderCell
                {
                    ColumnSpan = 13,
                    Text = null,
                    HorizontalAlign = HorizontalAlign.Left,
                    BorderStyle = BorderStyle.None
                };
                forbrafterdate.BorderStyle = BorderStyle.None;
                forbrafterdate.Controls.Add(tecbrafterdate);
                gv.HeaderRow.Parent.Controls.AddAt(0, forbrafterdate);


                GridViewRow row2 = new GridViewRow(0, 0, DataControlRowType.Header, DataControlRowState.Normal);
                var tec2 = new TableHeaderCell
                {
                    ColumnSpan = 13,
                    Text = "TO DATE:  " + Todate,
                    HorizontalAlign = HorizontalAlign.Left,
                    BorderStyle = BorderStyle.None,
                };
                row2.BorderStyle = BorderStyle.None;
                row2.Controls.Add(tec2);
                gv.HeaderRow.Parent.Controls.AddAt(0, row2);

                GridViewRow row22 = new GridViewRow(0, 0, DataControlRowType.Header, DataControlRowState.Normal);
                var tec22 = new TableHeaderCell
                {
                    ColumnSpan = 13,
                    Text = "FROM DATE:  " + fromdate,
                    HorizontalAlign = HorizontalAlign.Left,
                    BorderStyle = BorderStyle.None,
                };
                row22.BorderStyle = BorderStyle.None;
                row22.Controls.Add(tec22);
                gv.HeaderRow.Parent.Controls.AddAt(0, row22);

                GridViewRow row1 = new GridViewRow(0, 0, DataControlRowType.Header, DataControlRowState.Normal);
                var tec1 = new TableHeaderCell
                {
                    ColumnSpan = 13,
                    Text = "VENDTECH DEPOSIT REPORTS",
                    HorizontalAlign = HorizontalAlign.Center,
                    BorderStyle = BorderStyle.None,
                    BorderWidth = Unit.Pixel(20),
                };
                row1.Controls.Add(tec1);
                row1.BorderStyle = BorderStyle.None;
                row1.Style.Add(HtmlTextWriterStyle.FontSize, "large");
                gv.HeaderRow.Parent.Controls.AddAt(0, row1);

                //img
                GridViewRow imgRow = new GridViewRow(0, 0, DataControlRowType.Header, DataControlRowState.Normal);
                var imgHeader = new TableHeaderCell
                {
                    ColumnSpan = 13,
                    Text = "<img src='http://vendtechsl.net/Content/images/ventech.png' width='60'  style='border:1px solid red; text-align:center; margin:auto;'/>",
                    HorizontalAlign = HorizontalAlign.NotSet,
                    BorderStyle = BorderStyle.None,
                    BorderWidth = Unit.Pixel(20),
                };
                imgRow.Controls.Add(imgHeader);
                imgRow.BorderStyle = BorderStyle.Dotted;
                imgRow.Style.Add(HtmlTextWriterStyle.FontSize, "large");
                gv.HeaderRow.Parent.Controls.AddAt(0, imgRow);


                gv.HeaderRow.Cells[0].Text = "DATE/TIME"; //DATE_TIME
                gv.HeaderRow.Cells[1].Text = "VALUE DATE"; //VALUE DATE
                gv.HeaderRow.Cells[2].Text = "POS ID"; //POSID
                gv.HeaderRow.Cells[3].Text = "VENDOR"; gv.HeaderRow.Cells[3].ColumnSpan = 2; //VENDOR
                gv.HeaderRow.Cells[4].Text = "APPROVER"; //USERNAME
                gv.HeaderRow.Cells[5].Text = "TYPE"; //DEPOSIT_TYPE
                gv.HeaderRow.Cells[6].Text = "BANK"; //BANK
                gv.HeaderRow.Cells[7].Text = "TRANS-ID"; //TRANSACTION ID
                gv.HeaderRow.Cells[8].Text = "REF #"; //DEPOSIT_REF_NO
                gv.HeaderRow.Cells[9].Text = "AMOUNT";
                gv.HeaderRow.Cells[10].Text = "%"; //PERCENT
                gv.HeaderRow.Cells[11].Text = "BALANCE"; //NEW_BALANCE


                foreach (GridViewRow row in gv.Rows)
                {
                    if (row.RowType == DataControlRowType.DataRow)
                    {
                        row.Cells[0].HorizontalAlign = HorizontalAlign.Right;
                        row.Cells[1].HorizontalAlign = HorizontalAlign.Right;
                        row.Cells[2].HorizontalAlign = HorizontalAlign.Right;
                        row.Cells[5].HorizontalAlign = HorizontalAlign.Left;
                        row.Cells[3].HorizontalAlign = HorizontalAlign.Left; row.Cells[3].ColumnSpan = 2;
                        row.Cells[4].HorizontalAlign = HorizontalAlign.Left;
                        row.Cells[6].HorizontalAlign = HorizontalAlign.Left;
                        row.Cells[7].HorizontalAlign = HorizontalAlign.Right;
                        row.Cells[8].HorizontalAlign = HorizontalAlign.Right;
                        row.Cells[9].HorizontalAlign = HorizontalAlign.Right;
                        row.Cells[10].HorizontalAlign = HorizontalAlign.Right;
                        row.Cells[11].HorizontalAlign = HorizontalAlign.Right;
                    }
                }
            }


            if (ExportType == "Excel")
            {
                // string filename = "DepositReport_" + PrintedDateServer + ".xlsx";
                string filename = "DepositReport_" + PrintedDateServer + ".xls";
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
                //string style = @"<style> .textmode { mso-number-format:\@; } </style>";
                //Response.Write(style);
                Response.Output.Write(objStringWriter.ToString());
                Response.Flush();
                Response.End();

                //WorkBook workbook = WorkBook.Load(Server.MapPath(@"~/Content/StaticFileFormat/TempDepositReport.xlsx"));
                //string filepath = Server.MapPath(@"~/Content/StaticFileFormat/Generated/" + DateTime.Now.ToString("ddmmyyyyhhmmss") + ".xlsx");


                //WorkSheet sheet = workbook.DefaultWorkSheet;
                //sheet.SetCellValue(1, 0, "FROM DATE:  " + fromdate);
                //sheet.SetCellValue(2, 0, "TO DATE:  " + Todate);
                //sheet.SetCellValue(3, 0, "  ");
                //sheet.SetCellValue(4, 0, "PRINT DATE:  " + PrintedDateServer);
                //sheet.SetCellValue(5, 0, "  ");

                //int row = 7;
                //foreach (var item in list)
                //{
                //    sheet.SetCellValue(row, 0, item.DATE_TIME);
                //    sheet.SetCellValue(row, 1, item.POSID);
                //    sheet.SetCellValue(row, 2, item.USERNAME);
                //    sheet.SetCellValue(row, 3, item.AMOUNT);
                //    sheet.SetCellValue(row, 4, item.PERCENT);
                //    sheet.SetCellValue(row, 5, item.DEPOSIT_TYPE);
                //    sheet.SetCellValue(row, 6, item.BANK);
                //    sheet.SetCellValue(row, 7, item.DEPOSIT_REF_NO);
                //    sheet.SetCellValue(row, 8, item.NEW_BALANCE);
                //    row++;
                //}
                //workbook.SaveAs(filepath);

                //Response.Clear();
                //Response.AppendHeader("content-disposition", "attachment; filename=" + filename);
                //Response.ContentType = "application/octet-stream";
                //Response.WriteFile(filepath);
                //Response.Flush();
                //Response.End();

                //if (System.IO.File.Exists(filepath))
                //{
                //    // If file found, delete it    
                //    System.IO.File.Delete(filepath);
                //    Console.WriteLine("File deleted.");
                //}
            }
            else if (ExportType == "PDF")
            {
                string filename = "DepositReport_" + PrintedDateServer + ".pdf";
                Response.ContentType = "application/pdf";
                Response.AddHeader("content-disposition", "attachment; filename=\"" + filename + "\"");
                Response.Cache.SetCacheability(HttpCacheability.NoCache);
                StringWriter sw = new StringWriter();
                HtmlTextWriter hw = new HtmlTextWriter(sw);
                gv.RenderControl(hw);
                gv.HeaderRow.Style.Add("width", "15%");
                gv.HeaderRow.Style.Add("font-size", "10px");
                gv.EditRowStyle.HorizontalAlign = HorizontalAlign.Right;
                gv.Style.Add("text-decoration", "none");
                gv.Style.Add("font-family", "Arial, Helvetica, sans-serif;");
                gv.Style.Add("font-size", "8px");
                gv.Style.Add("text-align", "right");

                StringReader sr = new StringReader(sw.ToString());
                // Document pdfDoc = new Document(PageSize.A4_LANDSCAPE, 10f, 10f, 10f, 0f);
                Document pdfDoc = new Document(PageSize.A2, 7f, 7f, 7f, 0f);


                HTMLWorker htmlparser = new HTMLWorker(pdfDoc);
                PdfWriter.GetInstance(pdfDoc, Response.OutputStream);
                pdfDoc.Open();
                htmlparser.Parse(sr);
                pdfDoc.Close();
                Response.Write(pdfDoc);
                Response.End();
                gv.AllowPaging = true;
            }
        }


        //DateTime? FormatThisStringDate(string date)
        //{
        //    if (!string.IsNullOrEmpty(date))
        //    {
        //        var splitted_date = date.Split('/');
        //        var formatted_date = $"{splitted_date[1]}/{splitted_date[0]}/{splitted_date[2]}";
        //        return Convert.ToDateTime(formatted_date);
        //    }
        //    return null;
        //}

        public void ExportDepositAuditReportTo(ReportSearchModel model, string ExportType, string FromDate, string ToDate, string PrintedDateServer)
        {
            string fromdate = "";
            string Todate = "";
            CultureInfo provider = new CultureInfo("en-US");
            if (!string.IsNullOrEmpty(FromDate))
            {
                model.From = DateTime.ParseExact(FromDate, "dd/MM/yyyy", provider);
                fromdate = model.From.Value.ToString("dd/MM/yyyy");
            }

            if (!string.IsNullOrEmpty(ToDate))
            {
                model.To = DateTime.ParseExact(ToDate, "dd/MM/yyyy", provider);
                Todate = model.To.Value.ToString("dd/MM/yyyy");
            }

            var list = _depositManager.GetAuditReportExcelData(model).List;

            
            var gv = new GridView
            {
                DataSource = list
            };
            gv.DataBind();
            if (list.Count > 0)
            {

                GridViewRow forbr = new GridViewRow(0, 0, DataControlRowType.Header, DataControlRowState.Normal);
                var tecbr = new TableHeaderCell
                {
                    ColumnSpan = 11,
                    Text = null,
                    HorizontalAlign = HorizontalAlign.Left,
                    BorderStyle = BorderStyle.None,

                };
                forbr.BorderStyle = BorderStyle.None;
                forbr.BorderWidth = Unit.Pixel(0);
                forbr.Controls.Add(tecbr);
                gv.HeaderRow.Parent.Controls.AddAt(0, forbr);


                GridViewRow row3 = new GridViewRow(0, 0, DataControlRowType.Header, DataControlRowState.Normal);
                //TableHeaderCell tec3 = new TableHeaderCell();
                var tec3 = new TableHeaderCell
                {
                    ColumnSpan = 11,
                    Text = "PRINT DATE:  " + PrintedDateServer,
                    HorizontalAlign = HorizontalAlign.Left,
                    BorderStyle = BorderStyle.None,

                };
                row3.BorderWidth = Unit.Pixel(0);
                row3.BorderStyle = BorderStyle.None;
                row3.Controls.Add(tec3);
                gv.HeaderRow.Parent.Controls.AddAt(0, row3);

                GridViewRow forbrafterdate = new GridViewRow(0, 0, DataControlRowType.Header, DataControlRowState.Normal);
                var tecbrafterdate = new TableHeaderCell
                {
                    ColumnSpan = 11,
                    Text = null,
                    HorizontalAlign = HorizontalAlign.Left,
                    BorderStyle = BorderStyle.None
                };
                forbrafterdate.BorderWidth = Unit.Pixel(0);
                forbrafterdate.BorderStyle = BorderStyle.None;
                forbrafterdate.Controls.Add(tecbrafterdate);
                gv.HeaderRow.Parent.Controls.AddAt(0, forbrafterdate);

                GridViewRow row2 = new GridViewRow(0, 0, DataControlRowType.Header, DataControlRowState.Normal);
                var tec2 = new TableHeaderCell
                {
                    ColumnSpan = 11,
                    Text = "TO DATE:  " + Todate,
                    HorizontalAlign = HorizontalAlign.Left,
                    BorderStyle = BorderStyle.None,
                };
                row2.BorderWidth = Unit.Pixel(0);
                row2.BorderStyle = BorderStyle.None;
                row2.Controls.Add(tec2);
                gv.HeaderRow.Parent.Controls.AddAt(0, row2);

                GridViewRow row22 = new GridViewRow(0, 0, DataControlRowType.Header, DataControlRowState.Normal);
                var tec22 = new TableHeaderCell
                {
                    ColumnSpan = 11,
                    Text = "FROM DATE:  " + fromdate,
                    HorizontalAlign = HorizontalAlign.Left,
                    BorderStyle = BorderStyle.None,
                };
                row22.BorderWidth = Unit.Pixel(0);
                row22.BorderStyle = BorderStyle.None;
                row22.Controls.Add(tec22);
                gv.HeaderRow.Parent.Controls.AddAt(0, row22);

                GridViewRow row1 = new GridViewRow(0, 0, DataControlRowType.Header, DataControlRowState.Normal);
                //TableHeaderCell tec1 = new TableHeaderCell();
                var tec1 = new TableHeaderCell
                {
                    ColumnSpan = 11,
                    Text = "DEPOSIT AUDIT REPORTS",
                    HorizontalAlign = HorizontalAlign.Center,
                    BorderStyle = BorderStyle.None,
                    BorderWidth = Unit.Pixel(20),
                };

                row1.Controls.Add(tec1);
                row1.BorderWidth = Unit.Pixel(0);
                row1.BorderStyle = BorderStyle.None;
                row1.Style.Add(HtmlTextWriterStyle.FontSize, "large");
                gv.HeaderRow.Parent.Controls.AddAt(0, row1);

                //img
                GridViewRow imgRow = new GridViewRow(0, 0, DataControlRowType.Header, DataControlRowState.Normal);
                var imgHeader = new TableHeaderCell
                {
                    ColumnSpan = 11,
                    Text = "<img src='https://vendtechsl.com/Content/images/ventech.png' width='60'  style='border:1px solid red; text-align:center; margin:auto;'/>",
                    HorizontalAlign = HorizontalAlign.NotSet,
                    BorderStyle = BorderStyle.None,
                    BorderWidth = Unit.Pixel(20),
                };
                imgRow.Controls.Add(imgHeader);
                imgRow.BorderStyle = BorderStyle.Dotted;
                imgRow.Style.Add(HtmlTextWriterStyle.FontSize, "large");
                gv.HeaderRow.Parent.Controls.AddAt(0, imgRow);


                gv.HeaderRow.Cells[0].Text = "DATE/TIME";
                gv.HeaderRow.Cells[1].Text = "VALUE DATE";
                gv.HeaderRow.Cells[2].Text = "POS ID";
                gv.HeaderRow.Cells[3].Text = "VENDOR";
                gv.HeaderRow.Cells[4].Text = "TYPE";
                gv.HeaderRow.Cells[5].Text = "PAYER";
                gv.HeaderRow.Cells[6].Text = "PAYER BANK";
                gv.HeaderRow.Cells[7].Text = "REF#";
                gv.HeaderRow.Cells[8].Text = "GTBANK#";
                gv.HeaderRow.Cells[9].Text = "AMOUNT";
                gv.HeaderRow.Cells[10].Text = "STATUS";


                foreach (GridViewRow row in gv.Rows)
                {
                    if (row.RowType == DataControlRowType.DataRow)
                    {
                        var data = row.Cells[1].Text;
                        row.Cells[1].Text = row.Cells[10].Text;
                        row.Cells[10].Text = row.Cells[9].Text;
                        row.Cells[9].Text = row.Cells[8].Text;

                        var swapPayer = row.Cells[5].Text;
                        row.Cells[5].Text = row.Cells[6].Text;
                        row.Cells[6].Text = swapPayer;

                        row.Cells[8].Text = row.Cells[2].Text;
                        row.Cells[2].Text = data;
                        row.Cells[0].HorizontalAlign = HorizontalAlign.Right;
                        row.Cells[3].HorizontalAlign = HorizontalAlign.Left;
                        row.Cells[4].HorizontalAlign = HorizontalAlign.Left;
                        row.Cells[5].HorizontalAlign = HorizontalAlign.Left;
                        row.Cells[6].HorizontalAlign = HorizontalAlign.Left;
                        row.Cells[7].HorizontalAlign = HorizontalAlign.Right;
                        row.Cells[1].HorizontalAlign = HorizontalAlign.Right;
                        row.Cells[2].HorizontalAlign = HorizontalAlign.Right;
                        row.Cells[8].HorizontalAlign = HorizontalAlign.Left;
                        row.Cells[9].HorizontalAlign = HorizontalAlign.Right;
                        row.Cells[10].HorizontalAlign = HorizontalAlign.Left;
                    }
                }
            }
            if (ExportType == "Excel")
            {
                // string filename = "DepositReport_" + PrintedDateServer + ".xlsx";
                string filename = "DepositAuditReport_" + PrintedDateServer + ".xls";
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
                //string style = @"<style> .textmode { mso-number-format:\@; } </style>";
                //Response.Write(style);
                Response.Output.Write(objStringWriter.ToString());
                Response.Flush();
                Response.End();
            }
            else if (ExportType == "PDF")
            {
                string filename = "DepositAuditReport_" + PrintedDateServer + ".pdf";
                Response.ContentType = "application/pdf";
                Response.AddHeader("content-disposition", "attachment; filename=\"" + filename + "\"");
                Response.Cache.SetCacheability(HttpCacheability.NoCache);
                StringWriter sw = new StringWriter();
                HtmlTextWriter hw = new HtmlTextWriter(sw);
                gv.RenderControl(hw);
                gv.EditRowStyle.HorizontalAlign = HorizontalAlign.Right;
                gv.Style.Add("text-decoration", "none");
                gv.Style.Add("font-family", "Arial, Helvetica, sans-serif;");
                gv.Style.Add("font-size", "8px");
                gv.Style.Add("text-align", "right");

                StringReader sr = new StringReader(sw.ToString());
                // Document pdfDoc = new Document(PageSize.A4_LANDSCAPE, 10f, 10f, 10f, 0f);
                Document pdfDoc = new Document(PageSize.A2, 7f, 7f, 7f, 0f);


                HTMLWorker htmlparser = new HTMLWorker(pdfDoc);
                PdfWriter.GetInstance(pdfDoc, Response.OutputStream);
                pdfDoc.Open();
                htmlparser.Parse(sr);
                pdfDoc.Close();
                Response.Write(pdfDoc);
                Response.End();
                gv.AllowPaging = true;
            }
        }

        public void ExportDepositReleaseReportTo(ReportSearchModel model, string ExportType, string FromDate, string ToDate, string PrintedDateServer)
        {
            CultureInfo provider = CultureInfo.InvariantCulture;
            if (!string.IsNullOrEmpty(FromDate))
            {
                model.From = DateTime.ParseExact(FromDate, "dd/MM/yyyy", provider);
            }

            if (!string.IsNullOrEmpty(ToDate))
            {
                model.To = DateTime.ParseExact(ToDate, "dd/MM/yyyy", provider);
            }

            if (ExportType == "Excel")
            {
                string filename = "DepositReport_" + PrintedDateServer + ".xls";
                var gv = new GridView();
                var list = _depositManager.GetDepositPagedList(PagingModel.DefaultModel("CreatedAt", "Desc"), true).List;
                gv.DataSource = list;
                gv.DataBind();


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
            else if (ExportType == "PDF")
            {
                string filename = "DepositReport_" + PrintedDateServer + ".pdf";
                var gv = new GridView();
                var list = _depositManager.GetDepositPagedList(PagingModel.DefaultModel("CreatedAt", "Desc"), true).List;
                gv.DataSource = list;
                gv.DataBind();
                for (int i = 0; i < list.Count; i++)
                {
                    gv.Rows[i].Attributes.CssStyle.Add(HtmlTextWriterStyle.TextAlign, "right");
                }

                //  gv.DataBind();
                Response.ContentType = "application/pdf";
                Response.AddHeader("content-disposition", "attachment; filename=\"" + filename + "\"");
                Response.Cache.SetCacheability(HttpCacheability.NoCache);
                StringWriter sw = new StringWriter();
                HtmlTextWriter hw = new HtmlTextWriter(sw);
                gv.RenderControl(hw);
                gv.HeaderRow.Style.Add("width", "15%");
                gv.HeaderRow.Style.Add("font-size", "10px");
                gv.Style.Add("text-decoration", "none");
                gv.Style.Add("font-family", "Arial, Helvetica, sans-serif;");
                gv.Style.Add("font-size", "8px");

                StringReader sr = new StringReader(sw.ToString());
                // Document pdfDoc = new Document(PageSize.A4_LANDSCAPE, 10f, 10f, 10f, 0f);
                Document pdfDoc = new Document(PageSize.A2, 7f, 7f, 7f, 0f);


                HTMLWorker htmlparser = new HTMLWorker(pdfDoc);
                PdfWriter.GetInstance(pdfDoc, Response.OutputStream);
                pdfDoc.Open();
                htmlparser.Parse(sr);
                pdfDoc.Close();
                Response.Write(pdfDoc);
                Response.End();
                gv.AllowPaging = true;
            }
        }

        [HttpGet]
        public ActionResult PrintSalesReport(ReportSearchModel model, string FromDate, string ToDate, string PrintedDateServer)
        {
            ViewBag.Pritdatetime = PrintedDateServer; //BLL.Common.Utilities.GetLocalDateTime().ToString("dd/MM/yyyy hh:mm:ss tt");
            CultureInfo provider = CultureInfo.InvariantCulture;
            if (!string.IsNullOrEmpty(FromDate))
            {
                model.From = DateTime.ParseExact(FromDate, "dd/MM/yyyy", provider);
            }

            if (!string.IsNullOrEmpty(ToDate))
            {
                model.To = DateTime.ParseExact(ToDate, "dd/MM/yyyy", provider);
            }

            ViewBag.fromdate = model.From == null ? "" : model.From.Value.ToString("dd/MM/yyyy");
            ViewBag.Todate = model.To == null ? "" : model.To.Value.ToString("dd/MM/yyyy");

            var list = _meterManager.GetSalesExcelReportData(model, true).List;
            return View(list);
        }

        [HttpGet]
        public ActionResult PrintDepositReport(ReportSearchModel model, string FromDate, string ToDate, string PrintedDateServer)
        {
            ViewBag.Pritdatetime = PrintedDateServer; //BLL.Common.Utilities.GetLocalDateTime().ToString("dd/MM/yyyy hh:mm:ss tt");
            CultureInfo provider = CultureInfo.InvariantCulture;
            if (!string.IsNullOrEmpty(FromDate))
            {
                model.From = DateTime.ParseExact(FromDate, "dd/MM/yyyy", provider);
            }

            if (!string.IsNullOrEmpty(ToDate))
            {
                model.To = DateTime.ParseExact(ToDate, "dd/MM/yyyy", provider);
            }

            ViewBag.fromdate = model.From == null ? "" : model.From.Value.ToString("dd/MM/yyyy");
            ViewBag.Todate = model.To == null ? "" : model.To.Value.ToString("dd/MM/yyyy");
            var list = _depositManager.GetReportExcelData(model).List;
            return View(list);
        }

        [HttpGet]
        public ActionResult PrintDepositAuditReport(ReportSearchModel model, string FromDate, string ToDate, string PrintedDateServer)
        {
            ViewBag.Pritdatetime = PrintedDateServer; //BLL.Common.Utilities.GetLocalDateTime().ToString("dd/MM/yyyy hh:mm:ss tt");
            CultureInfo provider = new CultureInfo("en-Us");
            if (!string.IsNullOrEmpty(FromDate))
            {
                model.From = DateTime.ParseExact(FromDate, "dd/MM/yyyy", provider);
            }

            if (!string.IsNullOrEmpty(ToDate))
            {
                model.To = DateTime.ParseExact(ToDate, "dd/MM/yyyy", provider);
            }

            ViewBag.fromdate = model.From == null ? "" : model.From.Value.ToString("dd/MM/yyyy");
            ViewBag.Todate = model.To == null ? "" : model.To.Value.ToString("dd/MM/yyyy");
            var list = _depositManager.GetAuditReportExcelData(model).List;
            return View(list);
        }

        [HttpGet]
        public ActionResult PrintDepositReleaseReport(ReportSearchModel model)
        {
            ViewBag.Pritdatetime = BLL.Common.Utilities.GetLocalDateTime().ToString("dd/MM/yyyy hh:mm:ss tt");
            ViewBag.fromdate = model.From;
            ViewBag.Todate = model.To;
            var list = _depositManager.GetDepositPagedList(PagingModel.DefaultModel("CreatedAt", "Desc"), true).List;
            return View(list);
        }
        #endregion

        [AjaxOnly, HttpPost]
        public JsonResult SaveDepositAudit(DepositAuditModel depositAuditModel)
        {
            return Json(_depositManager.SaveDepositAuditRequest(depositAuditModel));
        }

        [AjaxOnly, HttpPost]
        public JsonResult UpdatDepositAudit(DepositAuditModel depositAuditModel)
        {
            return Json(_depositManager.UpdateDepositAuditRequest(depositAuditModel));
        }
        public System.Data.DataTable ExportToExcel()
        {
            System.Data.DataTable table = new System.Data.DataTable();
            table.Columns.Add("ID", typeof(int));
            table.Columns.Add("Name", typeof(string));
            table.Columns.Add("Sex", typeof(string));
            table.Columns.Add("Subject1", typeof(int));
            table.Columns.Add("Subject2", typeof(int));
            table.Columns.Add("Subject3", typeof(int));
            table.Columns.Add("Subject4", typeof(int));
            table.Columns.Add("Subject5", typeof(int));
            table.Columns.Add("Subject6", typeof(int));
            table.Rows.Add(1, "Amar", "M", 78, 59, 72, 95, 83, 77);
            table.Rows.Add(2, "Mohit", "M", 76, 65, 85, 87, 72, 90);
            table.Rows.Add(3, "Garima", "F", 77, 73, 83, 64, 86, 63);
            table.Rows.Add(4, "jyoti", "F", 55, 77, 85, 69, 70, 86);
            table.Rows.Add(5, "Avinash", "M", 87, 73, 69, 75, 67, 81);
            table.Rows.Add(6, "Devesh", "M", 92, 87, 78, 73, 75, 72);
            return table;
        }
    }
}