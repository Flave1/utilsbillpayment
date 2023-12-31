using VendTech.Attributes;
using VendTech.BLL.Interfaces;
using VendTech.BLL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using System.Web.UI;
using System.Web.UI.WebControls;
using iTextSharp.text;
using iTextSharp.text.html.simpleparser;
using iTextSharp.text.pdf;
using System.Globalization;
using System.Data;
using System.Drawing;
using System.Web.Script.Serialization;
using Newtonsoft.Json;
using VendTech.Framework.Api;
using Twilio.Converters;
using Twilio.Http;
using VendTech.BLL.Common;
using System.Web.Http.Results;
using VendTech.BLL.Managers;

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
        private readonly IPaymentTypeManager _paymentTypeManager;
        private readonly IEmailTemplateManager _emailTemplateManager;
        #endregion

        public ReportController(IUserManager userManager,
            IErrorLogManager errorLogManager,
            IVendorManager vendorManager,
            IAgencyManager agencyManager,
            IDepositManager depositManager,
            IMeterManager meterManager,
            IBankAccountManager bankManager,
            IPOSManager posManager, IPaymentTypeManager paymentTypeManager, IEmailTemplateManager emailTemplateManager)
            : base(errorLogManager)
        {
            _userManager = userManager;
            _vendorManager = vendorManager;
            _agencyManager = agencyManager;
            _depositManager = depositManager;
            _meterManager = meterManager;
            _bankAccountManager = bankManager;
            _posManager = posManager;
            _paymentTypeManager = paymentTypeManager;
            _emailTemplateManager = emailTemplateManager;
        }

        #region Report

        [HttpGet]
        public ActionResult ManageReports(int type = 0, long vendorId = 0, long pos = 0, string meter = "", string transactionId = "", string from = null, string to = null, string source = "")
        {
            ViewBag.Pritdatetime = BLL.Common.Utilities.GetLocalDateTime().ToString("dd/MM/yyyy hh:mm:ss tt");

            ViewBag.Vendors = _vendorManager.GetVendorsSelectList();
            ViewBag.PosId = _vendorManager.GetPosSelectList();
            ViewBag.Agencies = _agencyManager.GetAgentsSelectList();
            var assignedReportModule = _userManager.GetAssignedReportModules(LOGGEDIN_USER.UserID, LOGGEDIN_USER.UserType == UserRoles.Admin);
 
            ViewBag.DepositTypes = _paymentTypeManager.GetPaymentTypeSelect2List();
            ViewBag.SelectedTab = SelectedAdminTab.Reports; 

            if(source == "dashboard")
            {
                from = DateTime.UtcNow.ToString();
                to = DateTime.UtcNow.ToString();
            }
            if(from == null)
            {
                from = DateTime.UtcNow.ToString();
            }
            if (to == null)
            {
                to = DateTime.UtcNow.ToString();
            }

            var model = new ReportSearchModel
            {
                SortBy = "CreatedAt",
                SortOrder = "Desc",
                PageNo = 1,
                VendorId = vendorId,
                PosId = pos,
                Meter = meter,
                TransactionId = transactionId,
                From = DateTime.Parse(from),
                To =DateTime.Parse(to)
            };

            var deposits = new PagingResult<DepositListingModel>();
            var depositAudit = new PagingResult<DepositAuditModel>();

            if (assignedReportModule.Any())
            {
                var rtsReport1 = new SelectListItem { Text = "SHIFT ENQUIRY", Value = "-1" };
                var rtsReport2 = new SelectListItem { Text = "CUSTOMER ENQUIRIES", Value = "-2" };
                var rtsRps = new List<SelectListItem>();
                rtsRps.Add(rtsReport1);
                rtsRps.Add(rtsReport2);
                assignedReportModule.AddRange(rtsRps);
            }

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

                if (val == "-1")
                {
                    return View("Inquiry", new PagingResult<RtsedsaTransaction>());
                }
                if(val == "-2")
                {
                    return View("Transactions", new PagingResult<RtsedsaTransaction>());
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
                    model.IsInitialLoad = true;

                    ViewBag.Products = _platformManager.GetActivePlatformsSelectList();

                    var recharges = _meterManager.GetUserMeterRechargesReportAsync(model, true);  // ??new PagingResult<MeterRechargeApiListingModel>();
                    return View("ManageSalesReports", recharges);
                }
                if (val == "27")
                {

                    var balanceSheet = new PagingResultWithDefaultAmount<BalanceSheetListingModel2>();
                    ViewBag.OpeningBalance = 0;
                    return View("BalanceSheetReports", balanceSheet);
                }
                if (val == "28")
                {
                    var recharges = _meterManager.GetUserGSTRechargesReport(model, true);
                    return View("ManageGSTSalesReports", recharges);
                }
                if (val == "29")
                {
                    ViewBag.Agencies = _agencyManager.GetAgentsSelectList();
                    var recharges = _depositManager.GetAgentRevenueReportsPagedList(model, true);
                    return View("ManageAgentsRevenueReports", recharges);
                }
                if (val == "34")
                {
                    ViewBag.Agencies = _agencyManager.GetAgentsSelectList();
                    var commissions = _depositManager.GetCommissionsReportsPagedList(model, true);
                    return View("ManageCommissionReports", commissions);
                }
                /// This Is Used For Fetching DEPOSIT AUDIT REPORT
                if (val == "21")
                {
                    ViewBag.IssuingBank = new SelectList(_bankAccountManager.GetBankNames_API().ToList(), "BankName", "BankName");
                    ViewBag.Vendor = new SelectList(_userManager.GetVendorNames_API().ToList(), "VendorId", "VendorName");

                    ViewBag.banked = new SelectList(_bankAccountManager.GetBankAccounts().ToList(), "BankName", "BankName");

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

            var deposits = _depositManager.GetAllPendingDepositPagedList(PagingModel.DefaultModel("CreatedAt", "Desc"), vendorId: userId > 0 ? userId : 0); 
            return Json(new { posList = posList, history = deposits }, JsonRequestBehavior.AllowGet); 
            //return Json(new { posList }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetAgencyPosSelectList(long userId)
        {
            var posList = _posManager.GetAgencyPos(userId);
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

            var resultString = new List<string>
            {
                RenderRazorViewToString("Partials/_depositReleaseListing", modal),
                modal.TotalCount.ToString()
            }; 
            return JsonResult(resultString);
        }
        [AjaxOnly, HttpPost]
        public JsonResult GetReportsPagingList(ReportSearchModel model)
        {
            ViewBag.SelectedTab = SelectedAdminTab.Deposits;
            model.RecordsPerPage = 100000000;
            var response = new PagingResult<DepositListingModel>();
            var depositAuditModel = new PagingResult<DepositAuditModel>();
            if (model.ReportType == "17")
            {
                response = _depositManager.GetReportsPagedList(model, true);
            }
            if (model.ReportType == "21")
            {
                depositAuditModel = _depositManager.GetAuditReportsPagedList(model, true);
            }
            if (model.ReportType == "17")
            {
                //var resultString = new List<string> {
                //   RenderRazorViewToString("Partials/_reportListing", modal),
                //   modal.TotalCount.ToString()
                //};
                //return JsonResult(resultString);

                var serializer = new JavaScriptSerializer();
                serializer.MaxJsonLength = 999999999;
                var stringVal = serializer.Serialize(response);
                return Json(new { Status = ActionStatus.Successfull, Message = "deposit", Results = stringVal });
            }

            if (model.ReportType == "21")
            {
                var resultString = new List<string> {
                RenderRazorViewToString("Partials/_depositAuditListing",depositAuditModel),
                depositAuditModel.TotalCount.ToString()};
                return Json(new ActionOutput { Status = ActionStatus.Successfull, Message = "audit", Results = resultString }, JsonRequestBehavior.AllowGet); ;
                //var serializer = new JavaScriptSerializer();
                //serializer.MaxJsonLength = 999999999;
                //var stringVal = serializer.Serialize(depositAuditModel);
                //return Json(new { Status = ActionStatus.Successfull, Message = "audit", Results = stringVal });
            }
            return JsonResult(new List<string>() { });
        }
        [AjaxOnly, HttpPost]
        public JsonResult GetSalesReportsPagingList(ReportSearchModel model)
        {
            ViewBag.SelectedTab = SelectedAdminTab.Deposits; 
            model.RecordsPerPage = 1000000000;
            var response = _meterManager.GetUserMeterRechargesReportAsync(model, true);


            //var sum = modal.List.Select(d => d.Amount).Sum();
            //var resultString = new List<string> { RenderRazorViewToString("Partials/_salesReportListing", modal), modal.TotalCount.ToString() };
            var serializer = new JavaScriptSerializer();
            serializer.MaxJsonLength = 999999999;
            var stringVal = serializer.Serialize(response);
            return  Json( new { Status = ActionStatus.Successfull, Message = "", Results = stringVal });
        }

        [AjaxOnly, HttpPost]
        public JsonResult GetAgentRevenueReportsPagedList(ReportSearchModel model)
        {
            ViewBag.SelectedTab = SelectedAdminTab.Deposits;
            model.RecordsPerPage = 1000000000;
            var result = _depositManager.GetAgentRevenueReportsPagedList(model, true);


            var sum = result.List.Select(d => d.Amount).Sum();
            var resultString = new List<string> { RenderRazorViewToString("Partials/_agentsRevenueReportListing", result), result.TotalCount.ToString()
           };
            return JsonResult(resultString);
        }


        [AjaxOnly, HttpPost]
        public JsonResult GetCommissionsReportsPagedList(ReportSearchModel model)
        {
            ViewBag.SelectedTab = SelectedAdminTab.Deposits;
            model.RecordsPerPage = 1000000000;
            var result = _depositManager.GetCommissionsReportsPagedList(model, true);


            var sum = result.List.Select(d => d.Amount).Sum();
            var resultString = new List<string> { RenderRazorViewToString("Partials/_agentsRevenueReportListing", result), result.TotalCount.ToString()
           };
            return JsonResult(resultString);
        }

        [AjaxOnly, HttpPost]
        public JsonResult GetBalanceSheetReportsPagingList(ReportSearchModel model)
        {
            ViewBag.SelectedTab = SelectedAdminTab.Deposits; 
            model.RecordsPerPage = 1000000000; 

            var balanceSheet = new PagingResultWithDefaultAmount<BalanceSheetListingModel2>();
            var depositsBS = _depositManager.GetBalanceSheetReportsPagedList(model, true, 0);
            var salesBS = _meterManager.GetBalanceSheetReportsPagedList(model, true, 0);

            var result = depositsBS.Concat(salesBS).OrderBy(d => d.DateTime).ToList();

            balanceSheet = _posManager.CalculateBalancesheet(result);

            balanceSheet.Status = ActionStatus.Successfull;
            balanceSheet.Message = "Balance Sheet List";
            balanceSheet.TotalCount = depositsBS.Concat(salesBS).Count();



            var serializer = new JavaScriptSerializer();
            serializer.MaxJsonLength = 999999999;
            var stringVal = serializer.Serialize(balanceSheet);
            return Json(new { Status = ActionStatus.Successfull, Message = "", Results = stringVal });

            //var resultString = new List<string> { RenderRazorViewToString("Partials/_balanceSheetReportListing", balanceSheet), balanceSheet.TotalCount.ToString() };
            //return JsonResult(resultString);
        }

       
        [AjaxOnly, HttpPost]
        public JsonResult GetGSTSalesReportsPagingList(ReportSearchModel model)
        {
            ViewBag.SelectedTab = SelectedAdminTab.Deposits; 
            model.RecordsPerPage = 1000000000;
            var modal = _meterManager.GetUserGSTRechargesReport(model, true);
              
            var resultString = new List<string> { RenderRazorViewToString("Partials/_gstSalesReportListing", modal), modal.TotalCount.ToString()
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
                    Text = "<img src='https://vendtechsl.com/Content/images/ventech.png' width='60'  style='border:1px solid red; text-align:center; margin:auto;'/>",
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
                    Text = "<img src='https://vendtechsl.com/Content/images/ventech.png' width='60'  style='border:1px solid red; text-align:center; margin:auto;'/>",
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

        public void ExportBalanceSheetReportTo(ReportSearchModel model, string ExportType, string FromDate, string ToDate, string PrintedDateServer)
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

            var balanceSheet = new PagingResultWithDefaultAmount<BalanceSheetListingModel2>();
            var depositsBS = _depositManager.GetBalanceSheetReportsPagedList(model, true, 0);
            var salesBS = _meterManager.GetBalanceSheetReportsPagedList(model, true, 0);


            KeyValuePair<string, string> GetVendorDetail = _posManager.GetVendorDetail(model.PosId ?? 0);

            var result  = depositsBS.Concat(salesBS).OrderBy(d => d.DateTime).ToList();

            balanceSheet = _posManager.CalculateBalancesheet(result);



            balanceSheet.TotalCount = depositsBS.Concat(salesBS).Count();

            var list = balanceSheet.List.Select(a => new BalanceSheetReportExcelModel
            { 
                BALANCE = a.Balance,
                DATE_TIME = a.DateTime,
                DEPOSITAMOUNT =  a.DepositAmount, 
                REFERENCE = a.Reference,
                SALEAMOUNT = a.SaleAmount,
                TRANSACTIONID = a.TransactionId,
                TYPE = a.TransactionType,
                BALANCEBEFORE = a.BalanceBefore,
                
            }).ToList();
             
            var gv = new GridView
            {
                DataSource = list, 

            };
            gv.DataBind();
            if (list.Count > 0)
            {

                //DETAILS
                GridViewRow detailRow = new GridViewRow(0, 0, DataControlRowType.Header, DataControlRowState.Normal); 
                gv.HeaderRow.Parent.Controls.AddAt(0, detailRow);
                var detail = new TableHeaderCell
                {
                    ColumnSpan = 4,
                    Text = "POS ID:  " + GetVendorDetail.Key +
                    "<br /><br />VENDOR:  " + GetVendorDetail.Value +
                    "<br /><br />FROM DATE:  " + fromdate +
                    "<br />TO DATE:  " + Todate +
                    "<br />PRINTED DATE:  " + PrintedDateServer,
                    HorizontalAlign = HorizontalAlign.Left,
                    BorderStyle = BorderStyle.None,
                    BorderWidth = Unit.Pixel(20),
                };
                detailRow.Controls.Add(detail);


                //IMAGE
                var imgHeader = new TableHeaderCell
                { 
                    ColumnSpan = 2,
                    Text = "<img src='https://vendtechsl.com/Content/images/ventech.png' width='110' height='110' />",
                    HorizontalAlign = HorizontalAlign.Right,
                    BorderStyle = BorderStyle.None,
                    BorderWidth = Unit.Pixel(20),
                };
                detailRow.Controls.Add(imgHeader);

                var openingBal = "OPENING BAL:  " + balanceSheet.Amount;
                var openingClosingHeader = new TableHeaderCell
                {
                    ColumnSpan = 2,
                    Text = "<br />"+
                    openingBal +
                     "<br />" + "",
                    HorizontalAlign = HorizontalAlign.Right,
                    BorderStyle = BorderStyle.None,
                    BorderWidth = Unit.Pixel(20),
                };
                detailRow.Controls.Add(openingClosingHeader);
                 

                GridViewRow emptyRow = new GridViewRow(0, 0, DataControlRowType.Header, DataControlRowState.Normal); 
                var space = new TableHeaderCell
                {
                    ColumnSpan = 8,
                    Text = "",
                    HorizontalAlign = HorizontalAlign.Center,
                    BorderStyle = BorderStyle.None,
                    BorderWidth = Unit.Pixel(20),
                    
                };
                emptyRow.Controls.Add(space);
                emptyRow.BorderStyle = BorderStyle.None; 
                gv.HeaderRow.Parent.Controls.AddAt(0, emptyRow);

                GridViewRow row1 = new GridViewRow(0, 0, DataControlRowType.Header, DataControlRowState.Normal);
                var tec1 = new TableHeaderCell
                {
                    ColumnSpan = 8,
                    Text = "VENDTECH BALANCE SHEET REPORTS",
                    HorizontalAlign = HorizontalAlign.Center,
                    BorderStyle = BorderStyle.None,
                    BorderWidth = Unit.Pixel(20),

                };
                row1.Controls.Add(tec1);
                row1.BorderStyle = BorderStyle.None;
                row1.Style.Add(HtmlTextWriterStyle.FontSize, "large");
                row1.Style.Add(HtmlTextWriterStyle.FontWeight, "bold");
                gv.HeaderRow.Parent.Controls.AddAt(0, row1);



                gv.HeaderRow.Cells[0].Text = "DATE/TIME"; //DATE_TIME
                gv.HeaderRow.Cells[1].Text = "TRANS ID"; //TRANSACTION ID
                gv.HeaderRow.Cells[2].Text = "TYPE"; //TRANS TYPE
                gv.HeaderRow.Cells[3].Text = "REFERENCE"; //REFERENCE
                gv.HeaderRow.Cells[4].Text = "BALANCE BEFORE"; //BEFORE BALANCE
                gv.HeaderRow.Cells[5].Text = "DEPOSIT"; //DEPOSIT AMOUNT
                gv.HeaderRow.Cells[6].Text = "SALES"; //SALES AMOUNT
                gv.HeaderRow.Cells[7].Text = "BALANCE"; //BALANCE

                foreach (GridViewRow row in gv.Rows)
                {
                 
                    if (row.RowType == DataControlRowType.DataRow)
                    {

                        row.Cells[0].HorizontalAlign = HorizontalAlign.Right;
                        row.Cells[1].HorizontalAlign = HorizontalAlign.Right;
                        row.Cells[2].HorizontalAlign = HorizontalAlign.Left;
                        row.Cells[3].HorizontalAlign = HorizontalAlign.Right;
                        row.Cells[4].HorizontalAlign = HorizontalAlign.Right;
                        row.Cells[5].HorizontalAlign = HorizontalAlign.Right;
                        row.Cells[6].HorizontalAlign = HorizontalAlign.Right;
                        row.Cells[7].HorizontalAlign = HorizontalAlign.Right;

                        //depositAmount = Convert.ToDecimal(row.Cells[5].Text);
                        //balance = balance + depositAmount - saleAmount;

                        //row.Cells[5].Text = BLL.Common.Utilities.FormatAmount(depositAmount);
                        //row.Cells[6].Text = BLL.Common.Utilities.FormatAmount(saleAmount);
                        //row.Cells[7].Text = BLL.Common.Utilities.FormatAmount(balance); 
                        if(row.Cells[2].Text == "Deposit")
                        {
                            row.Cells[0].BackColor = Color.LightGray;
                            row.Cells[1].BackColor = Color.LightGray;
                            row.Cells[2].BackColor = Color.LightGray;
                            row.Cells[3].BackColor = Color.LightGray;
                            row.Cells[4].BackColor = Color.LightGray;
                            row.Cells[5].BackColor = Color.LightGray;
                            row.Cells[6].BackColor = Color.LightGray;
                            row.Cells[7].BackColor = Color.LightGray;
                        }
                        else //(row.Cells[2].Text == "EDSA")
                        {
                            row.Cells[6].ForeColor = Color.Red;  
                        }

                        if (row.Cells[5].Text == "0.00")
                        {
                            row.Cells[5].Text = "";
                        }
                        if (row.Cells[6].Text == "0.00")
                        {
                            row.Cells[6].Text = "";
                        }
                    }
                }
            }


            if (ExportType == "Excel")
            {

                string filename = "BalanceSheetReport_" + PrintedDateServer + ".xls";
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
                string filename = "BalanceSheetReport_" + PrintedDateServer + ".pdf";
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

        public void ExportGSTSalesReportTo(ReportSearchModel model, string ExportType, string FromDate, string ToDate, string PrintedDateServer)
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

            var list = _meterManager.GetUserGSTRechargesReport(model, true).List;
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
                    ColumnSpan = 10,
                    Text = null,
                    HorizontalAlign = HorizontalAlign.Left,
                    BorderStyle = BorderStyle.None
                };
                forbr.BorderStyle = BorderStyle.None;
                forbr.Controls.Add(tecbr);
                gv.HeaderRow.Parent.Controls.AddAt(0, forbr);


                //DETAILS
                GridViewRow detailRow = new GridViewRow(0, 0, DataControlRowType.Header, DataControlRowState.Normal);
                gv.HeaderRow.Parent.Controls.AddAt(0, detailRow);
                var detail = new TableHeaderCell
                {
                    ColumnSpan = 7,
                    Text =  "FROM DATE:  " + fromdate +
                    "<br /> TO DATE:  " + Todate +
                    "<br /> PRINT DATE:  " + PrintedDateServer,
                    HorizontalAlign = HorizontalAlign.Left,
                    BorderStyle = BorderStyle.None,
                    BorderWidth = Unit.Pixel(20),
                };
                detailRow.Controls.Add(detail);


                //IMAGE
                var imgHeader = new TableHeaderCell
                {
                    ColumnSpan = 3,
                    Text = "<img src='https://vendtechsl.com/Content/images/ventech.png' width='80' height='80' />",
                    HorizontalAlign = HorizontalAlign.Right,
                    BorderStyle = BorderStyle.None,
                    BorderWidth = Unit.Pixel(20),
                };
                detailRow.Controls.Add(imgHeader);



                GridViewRow row1 = new GridViewRow(0, 0, DataControlRowType.Header, DataControlRowState.Normal);
                //TableHeaderCell tec1 = new TableHeaderCell();
                var tec1 = new TableHeaderCell
                {
                    ColumnSpan = 10,
                    Text = "VENDTECH SALES GST (15%) REPORT",
                    HorizontalAlign = HorizontalAlign.Left,
                    BorderStyle = BorderStyle.None,
                    BorderWidth = Unit.Pixel(20),
                };
                row1.Controls.Add(tec1); 

                row1.BorderStyle = BorderStyle.None;
                row1.Style.Add(HtmlTextWriterStyle.FontSize, "large");
                gv.HeaderRow.Parent.Controls.AddAt(0, row1);

             

                gv.HeaderRow.Cells[0].Text = "DATE/TIME"; //DATE_TIME
                gv.HeaderRow.Cells[1].Text = "TRANS ID"; //TRANSACTIONID
                gv.HeaderRow.Cells[2].Text = "RECEIPT"; //RECEIPT
                gv.HeaderRow.Cells[3].Text = "METER No"; //METER_NO
                gv.HeaderRow.Cells[4].Text = "AMOUNT"; //AMOUNT
                gv.HeaderRow.Cells[5].Text = "SERVICE CHARGE"; //SERVICE CHARGE
                gv.HeaderRow.Cells[6].Text = "GST (15)"; //GST (15) 
                gv.HeaderRow.Cells[7].Text = "UNITS COST"; //UNITS COST
                gv.HeaderRow.Cells[8].Text = "TARIFF"; //TARIFF
                gv.HeaderRow.Cells[9].Text = "UNITS"; //UNITS 

                foreach (GridViewRow row in gv.Rows)
                {
                    if (row.RowType == DataControlRowType.DataRow)
                    {

                        row.Cells[0].HorizontalAlign = HorizontalAlign.Right;
                        row.Cells[1].HorizontalAlign = HorizontalAlign.Right;
                        row.Cells[2].HorizontalAlign = HorizontalAlign.Right;
                        row.Cells[3].HorizontalAlign = HorizontalAlign.Right;
                        row.Cells[4].HorizontalAlign = HorizontalAlign.Right;
                        row.Cells[5].HorizontalAlign = HorizontalAlign.Right;
                        row.Cells[6].HorizontalAlign = HorizontalAlign.Right;
                        row.Cells[7].HorizontalAlign = HorizontalAlign.Right;
                        row.Cells[8].HorizontalAlign = HorizontalAlign.Right;
                        row.Cells[4].Text = BLL.Common.Utilities.FormatAmount(Convert.ToDecimal(row.Cells[4].Text));
                        row.Cells[5].Text = BLL.Common.Utilities.FormatAmount(Convert.ToDecimal(row.Cells[5].Text)); 
                        row.Cells[7].Text = BLL.Common.Utilities.FormatAmount(Convert.ToDecimal(row.Cells[7].Text));
                        row.Cells[8].Text = BLL.Common.Utilities.FormatAmount(Convert.ToDecimal(row.Cells[8].Text));
                        row.Cells[6].Text = BLL.Common.Utilities.FormatAmount(Convert.ToDecimal(row.Cells[6].Text));
                    }
                }
            }


            if (ExportType == "Excel")
            {

                string filename = "GSTSalesReport_" + PrintedDateServer + ".xls";
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
                string filename = "GSTSalesReport_" + PrintedDateServer + ".pdf";
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
         
        public void ExportDepositAuditReportTo(ReportSearchModel model, string ExportType, string FromDate, string ToDate, string PrintedDateServer)
        { 
            string fromdate = "";
            string Todate = "";
            CultureInfo provider = new CultureInfo("en-US");
            if (!string.IsNullOrEmpty(FromDate))
            {
                var fromDateSplited = FromDate.Split('/');
                if(fromDateSplited[0].Length == 1)
                {
                    FromDate = $"0{fromDateSplited[0]}/{fromDateSplited[1]}/{fromDateSplited[2]}";
                }
                if (fromDateSplited[1].Length == 1)
                {
                    FromDate = $"{fromDateSplited[0]}/0{fromDateSplited[1]}/{fromDateSplited[2]}";
                }
                model.From = DateTime.ParseExact(FromDate, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                fromdate = model.From.Value.ToString("dd/MM/yyyy");
            }

            if (!string.IsNullOrEmpty(ToDate))
            {
                var toDateSplited = ToDate.Split('/');
                if (toDateSplited[0].Length == 1)
                {
                    ToDate = $"0{toDateSplited[0]}/{toDateSplited[1]}/{toDateSplited[2]}";
                }
                if (toDateSplited[1].Length == 1)
                {
                    ToDate = $"{toDateSplited[0]}/0{toDateSplited[1]}/{toDateSplited[2]}";
                }
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
                gv.HeaderRow.Cells[2].Text = "TRANS ID";
                gv.HeaderRow.Cells[3].Text = "POS ID";
                gv.HeaderRow.Cells[4].Text = "VENDOR";
                gv.HeaderRow.Cells[5].Text = "TYPE";
                gv.HeaderRow.Cells[6].Text = "PAYER";
                gv.HeaderRow.Cells[7].Text = "PAYER BANK";
                gv.HeaderRow.Cells[8].Text = "REF#";
                gv.HeaderRow.Cells[9].Text = "GTBANK#";
                gv.HeaderRow.Cells[10].Text = "AMOUNT";
                gv.HeaderRow.Cells[11].Text = "STATUS";


                foreach (GridViewRow row in gv.Rows)
                {
                    if (row.RowType == DataControlRowType.DataRow)
                    {
                        //var data = row.Cells[2].Text;
                        //row.Cells[2].Text = row.Cells[11].Text;
                        //row.Cells[11].Text = row.Cells[10].Text;
                        //row.Cells[10].Text = row.Cells[9].Text;

                        //var swapPayer = row.Cells[6].Text;
                        //row.Cells[6].Text = row.Cells[7].Text;
                        //row.Cells[7].Text = swapPayer;

                        //row.Cells[9].Text = row.Cells[3].Text;
                        //row.Cells[3].Text = data;
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
                        row.Cells[11].HorizontalAlign = HorizontalAlign.Left;
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

        public void ExportAgentRevenueReportTo(ReportSearchModel model, string ExportType, string FromDate, string ToDate, string PrintedDateServer)
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


            var list = _depositManager.GetAgentRevenueReportsExcelDeposituser(model, true).List;
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
                    ColumnSpan = 10,
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
                    ColumnSpan = 10,
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
                    ColumnSpan = 10,
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
                    ColumnSpan = 10,
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
                    ColumnSpan = 10,
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
                    ColumnSpan = 10,
                    Text = "VENDTECH AGENCY REVENUE REPORTS",
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
                    ColumnSpan = 10,
                    Text = "<img src='https://vendtechsl.com/Content/images/ventech.png' width='60'  style='border:1px solid red; text-align:center; margin:auto;'/>",
                    HorizontalAlign = HorizontalAlign.NotSet,
                    BorderStyle = BorderStyle.None,
                    BorderWidth = Unit.Pixel(20),
                };
                imgRow.Controls.Add(imgHeader);
                imgRow.BorderStyle = BorderStyle.Dotted;
                imgRow.Style.Add(HtmlTextWriterStyle.FontSize, "large");
                gv.HeaderRow.Parent.Controls.AddAt(0, imgRow);


                gv.HeaderRow.Cells[0].Text = "DATE/TIME"; //DATE_TIME
                gv.HeaderRow.Cells[1].Text = "POS ID"; //POSID
                gv.HeaderRow.Cells[2].Text = "VENDOR"; gv.HeaderRow.Cells[2].ColumnSpan = 2; //VENDOR 
                gv.HeaderRow.Cells[3].Text = "TYPE"; //DEPOSIT_TYPE
                gv.HeaderRow.Cells[4].Text = "TRANS-ID"; //TRANSACTION ID
                gv.HeaderRow.Cells[5].Text = "REF #"; //DEPOSIT_REF_NO 
                gv.HeaderRow.Cells[6].Text = "AMOUNT"; //AMOUNT
                gv.HeaderRow.Cells[7].Text = "VENDOR @ 1%"; //VENDOR @ 1%
                gv.HeaderRow.Cells[8].Text = "AGENT @ 0.5%"; //AGENT @ 0.5%


                foreach (GridViewRow row in gv.Rows)
                {
                    if (row.RowType == DataControlRowType.DataRow)
                    {
                        row.Cells[0].HorizontalAlign = HorizontalAlign.Right;
                        row.Cells[1].HorizontalAlign = HorizontalAlign.Right;
                        row.Cells[2].HorizontalAlign = HorizontalAlign.Left;
                        row.Cells[2].ColumnSpan = 2;
                        row.Cells[3].HorizontalAlign = HorizontalAlign.Left;
                        row.Cells[4].HorizontalAlign = HorizontalAlign.Right;
                        row.Cells[5].HorizontalAlign = HorizontalAlign.Right;
                        row.Cells[6].HorizontalAlign = HorizontalAlign.Right;
                        row.Cells[7].HorizontalAlign = HorizontalAlign.Right;
                        row.Cells[8].HorizontalAlign = HorizontalAlign.Right;
                    }
                }
            }


            if (ExportType == "Excel")
            {
                string filename = "AgentReventReport_" + PrintedDateServer + ".xls";
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
                string filename = "AgentReventReport_" + PrintedDateServer + ".pdf";
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
        public ActionResult PrintGSTSalesReport(ReportSearchModel model, string FromDate, string ToDate, string PrintedDateServer)
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

            var list = _meterManager.GetUserGSTRechargesReport(model, true).List;
            return View(list);
        }

        public ActionResult PrintBalancesheetReport(ReportSearchModel model, string FromDate, string ToDate, string PrintedDateServer)
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
          
            var depositsBS = _depositManager.GetBalanceSheetReportsPagedList(model, true, 0);
            var salesBS = _meterManager.GetBalanceSheetReportsPagedList(model, true, 0);

            decimal balance = 0;
            KeyValuePair<string, string> GetVendorDetail = _posManager.GetVendorDetail(model.PosId ?? 0);
            ViewBag.pos = GetVendorDetail.Key;
            ViewBag.vendor = GetVendorDetail.Value;
         
           var list = depositsBS.Concat(salesBS).OrderBy(d => d.DateTime).ToList();
            ViewBag.openBal = BLL.Common.Utilities.FormatAmount(depositsBS.FirstOrDefault().DepositAmount);
            ViewBag.closeBal = BLL.Common.Utilities.FormatAmount(depositsBS.ToList().Select(d => d.DepositAmount).Sum() - salesBS.ToList().Select(d => d.SaleAmount).Sum());
            foreach (var item in list)
            {
                balance = balance + item.DepositAmount - item.SaleAmount;
                item.Balance = balance;
            }

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
        public ActionResult PrintAgencyRevenueReport(ReportSearchModel model, string FromDate, string ToDate, string PrintedDateServer)
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
            var list = _depositManager.GetAgentRevenueReportsExcelDeposituser(model).List;
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

        [AjaxOnly, HttpPost]
        public JsonResult GetMiniSalesReport(ReportSearchModel model)
        {
            model.RecordsPerPage = 1000000000;
            model.IsInitialLoad = true;
            var result = _meterManager.GetMiniSalesReport(model, true, 0, model.MiniSaleRpType);
            return Json(new { result = JsonConvert.SerializeObject(result.List) });
        }

        [AjaxOnly, HttpPost, Public]
        public JsonResult ReturnVoucher(RequestObject tokenobject)
        {

            var result = _meterManager.ReturnVoucherReceipt(tokenobject.token_string);
            if (result.ReceiptStatus.Status == "unsuccessful")
                return Json(new { Success = false, Code = 302, Msg = result.ReceiptStatus.Message });

            result.ShowEmailButtonOnWeb = true;
            return Json(new { Success = true, Code = 200, Msg = "Meter recharged successfully.", Data = result });
        }


        [AjaxOnly, HttpPost, Public]
        public JsonResult SendEmail(SendViaEmail request)
        {
            var _errorManager = DependencyResolver.Current.GetService<IErrorLogManager>();

            try
            {

                _errorManager.LogExceptionToDatabase(new Exception($"Enter"));
                var td = _depositManager.GetSingleTransaction(long.Parse(request.TransactionId));
                if (td == null)
                    return Json(new { message = "Not found", status = "success" });

                //var body = BLL.Common.Utilities.ReaFromTemplateFile("DepositPDF.html");

                var vendor = _userManager.GetUserDetailsByUserId(td.UserId);
                var emailTemplate = _emailTemplateManager.GetEmailTemplateByTemplateType(TemplateTypes.DepositReceiptTemplate);
                if (emailTemplate.TemplateStatus)
                {
                    var body = emailTemplate.TemplateContent;
                    body = body.Replace("%ValueDate%", td.ValueDate);
                    body = body.Replace("%CreatedAt%", td.CreatedAt.ToString("dd/MM/yyyy"));
                    body = body.Replace("%VendorName%", td.User.Vendor);
                    body = body.Replace("%PosNumber%", td.POS.SerialNumber);
                    body = body.Replace("%TransactionId%", td.TransactionId);
                    body = body.Replace("%Type%", td.PaymentType1.Name);
                    body = body.Replace("%VendorName%", td.NameOnCheque);
                    body = body.Replace("%Amount%", BLL.Common.Utilities.FormatAmount(td.Amount));
                    body = body.Replace("%IssuingBank%", td.ChequeBankName);
                    body = body.Replace("%ChkNoOrSlipId%", td.CheckNumberOrSlipId);
                    body = body.Replace("%Bank%", td.ChequeBankName);
                    body = body.Replace("%PercentageAmount%", BLL.Common.Utilities.FormatAmount(td.PercentageAmount));
                    body = body.Replace("%date%", td.CreatedAt.ToString("dd/MM/yyyy"));

                    var file = BLL.Common.Utilities.CreatePdf(body, td.TransactionId + "_invoice.pdf");
                    var subject = $"INV-{td.TransactionId} from VENDTECHSL for {vendor.Vendor}";
                    var content = $"Hi {vendor} \n" +
                        $"Here is INV-{td.TransactionId} for {BLL.Common.Utilities.GetCountry().CurrencyCode} {BLL.Common.Utilities.FormatAmount(td.Amount)} \n";
                    BLL.Common.Utilities.SendPDFEmail(request.Email, subject, content, file.FirstOrDefault().Value, td.TransactionId + "_invoice.pdf");
                }

             
                return Json(new { message = "Email successfully sent.", status = "success" });
            }
            catch (Exception ex)
            {
                _errorManager.LogExceptionToDatabase(new Exception($"Errror", ex));
                return Json(new { message = "Email not sent.", status = "failed" });
            }
        }

    }
}