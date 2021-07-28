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
using VendTech.BLL.Models;
using System.Web.UI.WebControls;
using System.IO;
using System.Web.UI;
using iTextSharp.text;
using iTextSharp.text.html.simpleparser;
using iTextSharp.text.pdf;
using System.Globalization;
using VendTech.Areas.Admin.Controllers;
#endregion

namespace VendTech.Controllers
{
    /// <summary>
    /// Home Controller 
    /// Created On: 10/04/2015
    /// </summary>
    public class ReportController : AppUserBaseController
    {
        #region Variable Declaration
        //   private readonly IUserManager _userManager;
        private readonly IAuthenticateManager _authenticateManager;
        private readonly IVendorManager _vendorManager;
        private readonly ICMSManager _cmsManager;
        private readonly IDepositManager _depositManager;
        private readonly IMeterManager _meterManager;
        private readonly IPOSManager _posManager;
        private readonly IBankAccountManager _bankAccountMananger;


        #endregion

        public ReportController(IUserManager userManager, IErrorLogManager errorLogManager, IAuthenticateManager authenticateManager, ICMSManager cmsManager, IDepositManager depositManager, IMeterManager meterManager, IVendorManager vendorManager, IPOSManager posManager, IBankAccountManager bankAccountManager)
            : base(errorLogManager)
        {
            _userManager = userManager;
            _authenticateManager = authenticateManager;
            _cmsManager = cmsManager;
            _depositManager = depositManager;
            _meterManager = meterManager;
            _vendorManager = vendorManager;
            _posManager = posManager;
            _bankAccountMananger = bankAccountManager;
        }

        /// <summary>
        /// Index View 
        /// </summary>
        /// <returns></returns>
        public ActionResult DepositReport()
        {
            ViewBag.Pritdatetime = BLL.Common.Utilities.GetLocalDateTime().ToString("dd/MM/yyyy hh:mm:ss tt");
            var model = new ReportSearchModel
            {
                SortBy = "CreatedAt",
                SortOrder = "Desc",
                VendorId = LOGGEDIN_USER.UserID,
                IsInitialLoad = true,
            };



            var posList = _posManager.GetPOSSelectList(LOGGEDIN_USER.UserID);
            ViewBag.userPos = posList;
            var deposits = new PagingResult<DepositListingModel>();
            deposits = _depositManager.GetReportsPagedList(model);
            ViewBag.SelectedTab = SelectedAdminTab.Reports;
            ViewBag.DepositTypes = BLL.Common.Utilities.EnumToList(typeof(DepositPaymentTypeEnum));
            var bankAccounts = _bankAccountMananger.GetBankAccounts();
            ViewBag.Banks = bankAccounts.ToList().Select(p => new SelectListItem { Text = p.BankName, Value = p.BankAccountId.ToString() }).ToList();
            return View(deposits);

        }
        [AjaxOnly, HttpPost]
        public JsonResult GetDepositReportPagingList(ReportSearchModel model)
        {
            ViewBag.SelectedTab = SelectedAdminTab.Deposits;
            //model.SortBy = "CreatedAt";
            //model.SortOrder = "Desc";
            model.VendorId = LOGGEDIN_USER.UserID;
            model.RecordsPerPage = 10;
            var modal = new PagingResult<DepositListingModel>();

            modal = _depositManager.GetReportsPagedList(model);
            //List<string> resultString = new List<string>();
            //resultString.Add(RenderRazorViewToString("Partials/_depositListing", modal));
            //resultString.Add(modal.TotalCount.ToString());



            var resultString = new List<string> {
                RenderRazorViewToString("Partials/_depositListing", modal),
                modal.TotalCount.ToString()
            };

            return JsonResult(resultString);
        }

        public void ExportDepositReportTo(ReportSearchModeluser model, string ExportType, string FromDate, string ToDate, string PrintedDateServer)
        {
            string fromdate = "";
            string Todate = "";
            CultureInfo provider = CultureInfo.InvariantCulture;
            if (!string.IsNullOrEmpty(FromDate))
            {
                model.FromDate = DateTime.ParseExact(FromDate, "dd/MM/yyyy", provider);
                fromdate = model.FromDate.Value.ToString("dd/MM/yyyy");
            }

            if (!string.IsNullOrEmpty(ToDate))
            {
                model.ToDate = DateTime.ParseExact(ToDate, "dd/MM/yyyy", provider);
                Todate = model.ToDate.Value.ToString("dd/MM/yyyy");
            }

            var newfilters = new ReportSearchModel
            {

                VendorId = LOGGEDIN_USER.UserID,
                RecordsPerPage = 500000,
                PosId = model.POS,
                Meter = model.Meter,
                DepositType = model.DepositType,
                Bank = model.BANK,
                From = model.FromDate,
                To = model.ToDate,
                TransactionId = model.TransactionId,
                SortBy = model.SortBy,
                SortOrder = model.SortOrder,
                RefNumber = model.refNumber,
                ReportType = model.ReportType,
                PageNo = model.PageNo,
            };


            var list = _depositManager.GetReportsExcelDeposituser(newfilters, false).List;
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
                string filename = "DepositReport_" + PrintedDateServer + ".xls";
                Response.ClearContent();
                Response.Buffer = true;
                Response.AddHeader("content-disposition", "attachment; filename=\"" + filename + "\"");
                Response.ContentType = "application/ms-excel";
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
                string filename = "DepositReport_" + PrintedDateServer + ".pdf";
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


        public ActionResult SalesReport()
        {
            ViewBag.Pritdatetime = BLL.Common.Utilities.GetLocalDateTime().ToString("dd/MM/yyyy hh:mm:ss tt");
            var model = new ReportSearchModel
            {
                SortBy = "CreatedAt",
                SortOrder = "Desc",
                VendorId = LOGGEDIN_USER.UserID,
                IsInitialLoad = true

            };
            var assignedReportModule = _userManager.GetAssignedReportModules(LOGGEDIN_USER.UserID, LOGGEDIN_USER.UserType == UserRoles.Admin);
            ViewBag.AssignedReports = assignedReportModule;
            var posList = _posManager.GetPOSSelectList(LOGGEDIN_USER.UserID);
            ViewBag.userPos = posList;
            var deposits = new PagingResult<MeterRechargeApiListingModel>();
            deposits = _meterManager.GetUserMeterRechargesReport(model);
            ViewBag.SelectedTab = SelectedAdminTab.Reports;
            return View(deposits);

        }
        [AjaxOnly, HttpPost]
        public JsonResult GetSalesReportPagingList(ReportSearchModel model)
        {
            ViewBag.SelectedTab = SelectedAdminTab.Reports;
            //model.SortBy = "CreatedAt";
            //model.SortOrder = "Desc";
            model.VendorId = LOGGEDIN_USER.UserID;
            model.RecordsPerPage = 100000000;
            var modal = new PagingResult<MeterRechargeApiListingModel>();

            modal = _meterManager.GetUserMeterRechargesReport(model);


            //List<string> resultString = new List<string>();
            //resultString.Add(RenderRazorViewToString("Partials/_salesListing", modal));
            //resultString.Add(modal.TotalCount.ToString());


            var resultString = new List<string> {
              RenderRazorViewToString("Partials/_salesListing", modal),
              modal.TotalCount.ToString()
          };

            return JsonResult(resultString);
        }


        public void ExportSalesReportTo(ReportSearchModeluser model, string ExportType, string FromDate, string ToDate, string PrintedDateServer)
        {
            PrintedDateServer = PrintedDateServer.TrimEnd(' ');
            string fromdate = "";
            string Todate = "";
            CultureInfo provider = CultureInfo.InvariantCulture;
            if (!string.IsNullOrEmpty(FromDate))
            {
                model.FromDate = DateTime.ParseExact(FromDate, "dd/MM/yyyy", provider);
                fromdate = model.FromDate.Value.ToString("dd/MM/yyyy");
            }

            if (!string.IsNullOrEmpty(ToDate))
            {
                model.ToDate = DateTime.ParseExact(ToDate, "dd/MM/yyyy", provider);
                Todate = model.ToDate.Value.ToString("dd/MM/yyyy");
            }


            var newfilters = new ReportSearchModel
            {

                VendorId = LOGGEDIN_USER.UserID,

                RecordsPerPage = 500000,
                PosId = model.POS,
                Meter = model.Meter,
                RechargeToken = model.RechargeToken,
                DepositType = model.DepositType,
                Bank = model.BANK,
                From = model.FromDate,
                To = model.ToDate,
                TransactionId = model.TransactionId,
                SortBy = model.SortBy,
                SortOrder = model.SortOrder,
                RefNumber = model.refNumber,
                ReportType = model.ReportType,
                PageNo = model.PageNo,
            };


            var list = _meterManager.GetSalesExcelReportData(newfilters, false).List;


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
                PrintedDateServer = PrintedDateServer.TrimEnd(' ');
                //string filename = "SalesReport_" + PrintedDateServer + ".xlsx";
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

                //string style = @"<style> .textmode { mso-number-format:\@; } </style>";
                //Response.Write(style);
                Response.Output.Write(objStringWriter.ToString());
                Response.Flush();
                Response.End();



                //string filepath = Server.MapPath(@"~/Content/StaticFileFormat/Generated/" + DateTime.Now.ToString("ddmmyyyyhhmmss") + ".xlsx");

                //WorkBook workbook = WorkBook.Load(Server.MapPath(@"~/Content/StaticFileFormat/TempSalesReport.xlsx"));
                //WorkSheet sheet = workbook.DefaultWorkSheet;
                //sheet.SetCellValue(1, 0, "FROM DATE:  " + fromdate);
                //sheet.SetCellValue(2, 0, "TO DATE:  " + Todate);
                //sheet.SetCellValue(3, 0, "  ");
                //sheet.SetCellValue(4, 0, "PRINT DATE:  " + PrintedDateServer);
                //sheet.SetCellValue(5, 0, "  ");
                //int row = 7;
                //foreach (var item in list)
                //{
                //    sheet.SetCellValue(row, 0, item.Date_TIME);
                //    sheet.SetCellValue(row, 1, item.PRODUCT_TYPE);
                //    sheet.SetCellValue(row, 2, item.AMOUNT);
                //    sheet.SetCellValue(row, 3, item.TRANSACTIONID);
                //    sheet.SetCellValue(row, 4, item.METER_NO);
                //    sheet.SetCellValue(row, 5, item.POSID);
                //    sheet.SetCellValue(row, 6, item.VENDORNAME);
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
                string filename = "SalesReport_" + PrintedDateServer + ".pdf";
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
        public ActionResult PrintDepositReport(ReportSearchModeluser model, string FromDate, string ToDate, string PrintedDateServer)
        {
            PrintedDateServer = PrintedDateServer.TrimEnd('\0');
            ViewBag.Pritdatetime = PrintedDateServer;
            CultureInfo provider = CultureInfo.InvariantCulture;
            if (!string.IsNullOrEmpty(FromDate))
            {
                model.FromDate = DateTime.ParseExact(FromDate, "dd/MM/yyyy", provider);
            }

            if (!string.IsNullOrEmpty(ToDate))
            {
                model.ToDate = DateTime.ParseExact(ToDate, "dd/MM/yyyy", provider);
            }


            var newfilters = new ReportSearchModel
            {

                VendorId = LOGGEDIN_USER.UserID,
                RecordsPerPage = 500000,
                PosId = model.POS,
                Meter = model.Meter,

                DepositType = model.DepositType,
                Bank = model.BANK,
                From = model.FromDate,
                To = model.ToDate,
                TransactionId = model.TransactionId,
                SortBy = model.SortBy,
                SortOrder = model.SortOrder,
                RefNumber = model.refNumber,
                ReportType = model.ReportType,
                PageNo = model.PageNo,
            };



            ViewBag.fromdate = newfilters.From == null ? "" : newfilters.From.Value.ToString("dd/MM/yyyy");
            ViewBag.Todate = newfilters.To == null ? "" : newfilters.To.Value.ToString("dd/MM/yyyy");


            var list = _depositManager.GetReportsExcelDeposituser(newfilters, false).List;
            return View(list);
        }

        [HttpGet]
        public ActionResult PrintSalesReport(ReportSearchModeluser model, string FromDate, string ToDate, string PrintedDateServer)
        {
            PrintedDateServer = PrintedDateServer.TrimEnd('\0');
            ViewBag.Pritdatetime = PrintedDateServer;
            CultureInfo provider = CultureInfo.InvariantCulture;
            if (!string.IsNullOrEmpty(FromDate))
            {
                model.FromDate = DateTime.ParseExact(FromDate, "dd/MM/yyyy", provider);
            }

            if (!string.IsNullOrEmpty(ToDate))
            {
                model.ToDate = DateTime.ParseExact(ToDate, "dd/MM/yyyy", provider);
            }


            var newfilters = new ReportSearchModel
            {
                VendorId = LOGGEDIN_USER.UserID,
                RecordsPerPage = 500000,
                PosId = model.POS,
                Meter = model.Meter,

                DepositType = model.DepositType,
                Bank = model.BANK,
                From = model.FromDate,
                To = model.ToDate,
                TransactionId = model.TransactionId,
                SortBy = model.SortBy,
                SortOrder = model.SortOrder,
                RefNumber = model.refNumber,
                ReportType = model.ReportType,
            };


            ViewBag.fromdate = newfilters.From == null ? "" : newfilters.From.Value.ToString("dd/MM/yyyy");
            ViewBag.Todate = newfilters.To == null ? "" : newfilters.To.Value.ToString("dd/MM/yyyy");

            var list = _meterManager.GetSalesExcelReportData(newfilters, false).List;
            return View(list);
        }

    }
}