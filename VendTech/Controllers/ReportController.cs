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
using System.Drawing;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Web.Http.Results;
using VendTech.BLL.Managers;
using VendTech.Framework.Api;
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
        private readonly IPaymentTypeManager _paymentTypeManager;
        private readonly IEmailTemplateManager _emailTemplateManager;


        #endregion

        public ReportController(IUserManager userManager, IErrorLogManager errorLogManager, IAuthenticateManager authenticateManager, ICMSManager cmsManager, IDepositManager depositManager, IMeterManager meterManager, IVendorManager vendorManager, IPOSManager posManager, IBankAccountManager bankAccountManager, IPaymentTypeManager paymentTypeManager, IEmailTemplateManager emailTemplateManager)
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
            _paymentTypeManager = paymentTypeManager;
            _emailTemplateManager = emailTemplateManager;
        }

        /// <summary>
        /// Index View 
        /// </summary>
        /// <returns></returns>
        public ActionResult DepositReport(long pos = 0, string meter = "", string transactionId = "", string from = null, string to = null)
        {
            ViewBag.Pritdatetime = BLL.Common.Utilities.GetLocalDateTime().ToString("dd/MM/yyyy hh:mm:ss tt");

            if (from == null)
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
                VendorId = LOGGEDIN_USER.UserID,
                IsInitialLoad = from == null ? true : false,
                PosId = pos,
                Meter = meter,
                TransactionId = transactionId,
                From = DateTime.Parse(from),
                To = DateTime.Parse(to)
            };



            var posList = _posManager.GetPOSWithNameSelectList(LOGGEDIN_USER.UserID, LOGGEDIN_USER.AgencyId);
            ViewBag.userPos = posList;
            var deposits = new PagingResult<DepositListingModel>();
            deposits = _depositManager.GetReportsPagedList(model, false, LOGGEDIN_USER.AgencyId);
            ViewBag.SelectedTab = SelectedAdminTab.Reports;
            ViewBag.DepositTypes = _paymentTypeManager.GetPaymentTypeSelectList();

            var assignedReportModule = _userManager.GetAssignedReportModules(LOGGEDIN_USER.UserID, LOGGEDIN_USER.UserType == UserRoles.Admin);
            ViewBag.AssignedReports = assignedReportModule;

            var bankAccounts = _bankAccountMananger.GetBankAccounts();
            ViewBag.Banks = bankAccounts.ToList().Select(p => new SelectListItem { Text = p.BankName, Value = p.BankAccountId.ToString() }).ToList();
            //deposits
            return View(deposits);

        }
       
        public ActionResult SalesReport(long pos = 0, string meter = "", string transactionId = "", string from = null, string to = null)
        {
            ViewBag.SelectedTab = SelectedAdminTab.Reports;
            ViewBag.Pritdatetime = BLL.Common.Utilities.GetLocalDateTime().ToString("dd/MM/yyyy hh:mm:ss tt");

            if (from == null)
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
                VendorId = LOGGEDIN_USER.UserID,
                IsInitialLoad = from == null?  true : false,
                AgencyId = LOGGEDIN_USER.AgencyId, 
                PosId = pos,
                Meter = meter == "undefined" ? "" : meter,
                TransactionId = transactionId == "undefined" ? "" : transactionId,
                From = DateTime.Parse(from),
                To = DateTime.Parse(to)
            };

            ViewBag.Products = _platformManager.GetActivePlatformsSelectList();

            var assignedReportModule = _userManager.GetAssignedReportModules(LOGGEDIN_USER.UserID, LOGGEDIN_USER.UserType == UserRoles.Admin);
            ViewBag.AssignedReports = assignedReportModule;
            var posList = _posManager.GetPOSWithNameSelectList(LOGGEDIN_USER.UserID, LOGGEDIN_USER.AgencyId);
            ViewBag.userPos = posList;
            var sales = new PagingResult<MeterRechargeApiListingModel>();
            sales = _meterManager.GetUserMeterRechargesReportAsync(model, false, LOGGEDIN_USER.AgencyId);
            ViewBag.SelectedTab = SelectedAdminTab.Reports; 
            return View(sales);

        }

        public ActionResult GSTSalesReport(long pos = 0, string meter = "", string transactionId = "", string from = null, string to = null)
        {
            ViewBag.Pritdatetime = BLL.Common.Utilities.GetLocalDateTime().ToString("dd/MM/yyyy hh:mm:ss tt");
            if (from == null)
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
                VendorId = LOGGEDIN_USER.UserID, 
                IsInitialLoad = false,
                AgencyId = LOGGEDIN_USER.AgencyId,
                PosId = pos,
                Meter = meter,
                TransactionId = transactionId,
                From = DateTime.Parse(from),
                To = DateTime.Parse(to)

            };
            var assignedReportModule = _userManager.GetAssignedReportModules(LOGGEDIN_USER.UserID, LOGGEDIN_USER.UserType == UserRoles.Admin);
            ViewBag.AssignedReports = assignedReportModule;
            var posList = _posManager.GetPOSSelectList(LOGGEDIN_USER.UserID, LOGGEDIN_USER.AgencyId);
            ViewBag.userPos = posList;
            var sales = new PagingResult<GSTRechargeApiListingModel>();
            sales = _meterManager.GetUserGSTRechargesReport(model, false, LOGGEDIN_USER.AgencyId);
            ViewBag.SelectedTab = SelectedAdminTab.Reports;
            return View(sales);

        }

        public ActionResult BalanceSheetReport(long pos = 0, string meter = "", string transactionId = "", string from = null, string to = null)
        {

            ViewBag.Pritdatetime = BLL.Common.Utilities.GetLocalDateTime().ToString("dd/MM/yyyy hh:mm:ss tt");
            if (from == null)
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
                VendorId = LOGGEDIN_USER.UserID,
                IsInitialLoad = false,
                AgencyId = LOGGEDIN_USER.AgencyId,
                PosId = pos,
                Meter = meter,
                TransactionId = transactionId,
                From = DateTime.Parse(from),
                To = DateTime.Parse(to)

            };
            model.RecordsPerPage = 1000000000;
            var assignedReportModule = _userManager.GetAssignedReportModules(LOGGEDIN_USER.UserID, LOGGEDIN_USER.UserType == UserRoles.Admin);
            ViewBag.AssignedReports = assignedReportModule;
            var posList = _posManager.GetPOSWithNameSelectList(LOGGEDIN_USER.UserID, LOGGEDIN_USER.AgencyId);


            var balanceSheet = new PagingResultWithDefaultAmount<BalanceSheetListingModel2>();
            ViewBag.userPos = posList;


            ViewBag.SelectedTab = SelectedAdminTab.Reports;
            return View(balanceSheet);

        }

        public ActionResult AgentRevenueReport(long pos = 0, string meter = "", string transactionId = "", string from = null, string to = null)
        {
            ViewBag.Pritdatetime = BLL.Common.Utilities.GetLocalDateTime().ToString("dd/MM/yyyy hh:mm:ss tt");
            if (from == null)
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
                VendorId = LOGGEDIN_USER.UserID,
                IsInitialLoad = true,
                AgencyId = LOGGEDIN_USER.AgencyId,
                PosId = pos,
                Meter = meter,
                TransactionId = transactionId,
                From = DateTime.Parse(from),
                To = DateTime.Parse(to)
            };
             
            var posList = _posManager.GetPOSSelectList(LOGGEDIN_USER.UserID, LOGGEDIN_USER.AgencyId);
            ViewBag.userPos = posList;
            var deposits = new PagingResult<AgentRevenueListingModel>();
            deposits = _depositManager.GetAgentRevenueReportsPagedList(model, false, LOGGEDIN_USER.AgencyId);
            ViewBag.SelectedTab = SelectedAdminTab.Reports;
            ViewBag.DepositTypes = _paymentTypeManager.GetPaymentTypeSelectList();

            var assignedReportModule = _userManager.GetAssignedReportModules(LOGGEDIN_USER.UserID, LOGGEDIN_USER.UserType == UserRoles.Admin);
            ViewBag.AssignedReports = assignedReportModule;

            //var bankAccounts = _bankAccountMananger.GetBankAccounts();
            //ViewBag.Banks = bankAccounts.ToList().Select(p => new SelectListItem { Text = p.BankName, Value = p.BankAccountId.ToString() }).ToList();
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

            modal = _depositManager.GetReportsPagedList(model, false, LOGGEDIN_USER.AgencyId);
            //List<string> resultString = new List<string>();
            //resultString.Add(RenderRazorViewToString("Partials/_depositListing", modal));
            //resultString.Add(modal.TotalCount.ToString());



            var resultString = new List<string> {
                RenderRazorViewToString("Partials/_depositListing", modal),
                modal.TotalCount.ToString()
            };

            return JsonResult(resultString);
        }
        [AjaxOnly, HttpPost]
        public JsonResult GetAgentRevenueReportPagingList(ReportSearchModel model)
        {
            ViewBag.SelectedTab = SelectedAdminTab.Deposits; 
            model.VendorId = LOGGEDIN_USER.UserID;
            model.RecordsPerPage = 10;
            model.AgencyId = LOGGEDIN_USER.AgencyId;
            var modal = new PagingResult<AgentRevenueListingModel>();

            modal = _depositManager.GetAgentRevenueReportsPagedList(model, false, LOGGEDIN_USER.AgencyId); 
            var resultString = new List<string> {
                RenderRazorViewToString("Partials/_agentRevenueListing", modal),
                modal.TotalCount.ToString()
            };

            return JsonResult(resultString);
        }

        [AjaxOnly, HttpPost]
        public JsonResult GetBalanceSheetReportsPagingList(ReportSearchModel model)
        {
            ViewBag.SelectedTab = SelectedAdminTab.Deposits;
            model.RecordsPerPage = 1000000000; 

            model.VendorId = LOGGEDIN_USER.UserID;
            var balanceSheet = new PagingResultWithDefaultAmount<BalanceSheetListingModel2>();
            var depositsBS = _depositManager.GetBalanceSheetReportsPagedList(model, false, LOGGEDIN_USER.AgencyId);
            var salesBS = _meterManager.GetBalanceSheetReportsPagedList(model, false, LOGGEDIN_USER.AgencyId);

            var result = depositsBS.Concat(salesBS).OrderBy(d => d.DateTime).ToList();

            balanceSheet = _posManager.CalculateBalancesheet(result);

            balanceSheet.Status = ActionStatus.Successfull;
            balanceSheet.Message = "Balance Sheet List";
            balanceSheet.TotalCount = depositsBS.Concat(salesBS).Count();

            var resultString = new List<string> { RenderRazorViewToString("Partials/_balanceSheetReportListing", balanceSheet), balanceSheet.TotalCount.ToString()
           };
            return JsonResult(resultString, balanceSheet.Amount);
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

            modal =  _meterManager.GetUserMeterRechargesReportAsync(model, false, LOGGEDIN_USER.AgencyId);

            //List<string> resultString = new List<string>();
            //resultString.Add(RenderRazorViewToString("Partials/_salesListing", modal));
            //resultString.Add(modal.TotalCount.ToString());


            var resultString = new List<string> {
              RenderRazorViewToString("Partials/_salesListing", modal),
              modal.TotalCount.ToString()
          };

            return JsonResult(resultString);
        }

        [AjaxOnly, HttpPost]
        public JsonResult GetGSTSalesReportPagingList(ReportSearchModel model)
        {
            ViewBag.SelectedTab = SelectedAdminTab.Reports;
            //model.SortBy = "CreatedAt";
            //model.SortOrder = "Desc";
            model.VendorId = LOGGEDIN_USER.UserID;
            model.RecordsPerPage = 100000000;
            var modal = new PagingResult<GSTRechargeApiListingModel>();

            modal = _meterManager.GetUserGSTRechargesReport(model, false, LOGGEDIN_USER.AgencyId);
             

            var resultString = new List<string> {
              RenderRazorViewToString("Partials/_gstSalesListing", modal),
              modal.TotalCount.ToString()
          };

            return JsonResult(resultString);
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

            model.VendorId = LOGGEDIN_USER.UserID;

            var balanceSheet = new PagingResultWithDefaultAmount<BalanceSheetListingModel2>();

            var depositsBS = _depositManager.GetBalanceSheetReportsPagedList(model, false, LOGGEDIN_USER.AgencyId);
            var salesBS = _meterManager.GetBalanceSheetReportsPagedList(model, false, LOGGEDIN_USER.AgencyId);


            KeyValuePair<string, string> GetVendorDetail = _posManager.GetVendorDetail(model.PosId ?? 0);

            var result = depositsBS.Concat(salesBS).OrderBy(d => d.DateTime).ToList();


            balanceSheet = _posManager.CalculateBalancesheet(result);
            balanceSheet.TotalCount = depositsBS.Concat(salesBS).Count();

           



            var list = balanceSheet.List.Select(a => new BalanceSheetReportExcelModel
            {
                BALANCE = a.Balance,
                DATE_TIME = a.DateTime,
                DEPOSITAMOUNT = a.DepositAmount,
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
                    ColumnSpan = 3,
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

                var openingBal ="OPENING BAL:  " + balanceSheet.Amount;
                var openingClosingHeader = new TableHeaderCell
                {
                    ColumnSpan = 2,
                    Text = "<br />" +
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

                decimal balance = 0;
                foreach (GridViewRow row in gv.Rows)
                {
                    decimal saleAmount = 0;
                    decimal depositAmount = 0;
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

                        saleAmount = Convert.ToDecimal(row.Cells[6].Text);
                        depositAmount = Convert.ToDecimal(row.Cells[5].Text);
                        balance = balance + depositAmount - saleAmount;

                        row.Cells[5].Text = BLL.Common.Utilities.FormatAmount(depositAmount);
                        row.Cells[6].Text = BLL.Common.Utilities.FormatAmount(saleAmount);
                        row.Cells[7].Text = BLL.Common.Utilities.FormatAmount(balance);
                        if (row.Cells[2].Text == "Deposit")
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
                        else //(row.Cells[2].Text == "ELECTRICITY (EDSA)")
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

            model.VendorId = LOGGEDIN_USER.UserID;
            var depositsBS = _depositManager.GetBalanceSheetReportsPagedList(model, false, LOGGEDIN_USER.AgencyId);
            var salesBS = _meterManager.GetBalanceSheetReportsPagedList(model, false, LOGGEDIN_USER.AgencyId);

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


            var list = _depositManager.GetReportExcelData(newfilters, LOGGEDIN_USER.AgencyId).List;
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
                    Text = "<img src='https://vendtechsl.com/Content/images/ventech.png' width='110' height='110' />",
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

        public void ExportAgentRevenueReportTo(ReportSearchModeluser model, string ExportType, string FromDate, string ToDate, string PrintedDateServer)
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

            var list = _depositManager.GetAgentRevenueReportsExcelDeposituser(newfilters, false, LOGGEDIN_USER.AgencyId).List;
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
                    BorderStyle = BorderStyle.None
                };
                forbr.BorderStyle = BorderStyle.None;
                forbr.Controls.Add(tecbr);
                gv.HeaderRow.Parent.Controls.AddAt(0, forbr);


                GridViewRow row3 = new GridViewRow(0, 0, DataControlRowType.Header, DataControlRowState.Normal);
                //TableHeaderCell tec3 = new TableHeaderCell();
                var tec3 = new TableHeaderCell
                {
                    ColumnSpan = 11,
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
                    ColumnSpan = 11,
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
                    ColumnSpan = 11,
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
                    ColumnSpan = 11,
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
                    ColumnSpan = 11,
                    Text = "VENDTECH AGENT REVENUE REPORTS",
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
                    ColumnSpan = 11,
                    Text = "<img src='https://vendtechsl.com/Content/images/ventech.png' width='110' height='110' />",
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
                gv.HeaderRow.Cells[6].Text = "BALANCE BEFORE"; //BALANCE BEFORE
                gv.HeaderRow.Cells[7].Text = "AMOUNT"; //AMOUNT
                gv.HeaderRow.Cells[8].Text = "%"; //%
                gv.HeaderRow.Cells[9].Text = "BALANCE"; //BALANCE


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
                        row.Cells[9].HorizontalAlign = HorizontalAlign.Right;
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
                AgencyId = LOGGEDIN_USER.AgencyId
            };


            var list = _meterManager.GetSalesExcelReportData(newfilters, false, LOGGEDIN_USER.AgencyId).List;


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
                    Text = "<img src='https://vendtechsl.com/Content/images/ventech.png' width='110' height='110' />",
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

        public void ExportGSTSalesReportTo(ReportSearchModeluser model, string ExportType, string FromDate, string ToDate, string PrintedDateServer)
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


            var list = _meterManager.GetUserGSTRechargesReport(newfilters, false).List;


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


                //DETAILS
                GridViewRow detailRow = new GridViewRow(0, 0, DataControlRowType.Header, DataControlRowState.Normal);
                gv.HeaderRow.Parent.Controls.AddAt(0, detailRow);
                var detail = new TableHeaderCell
                {
                    ColumnSpan = 7,
                    Text = "FROM DATE:  " + fromdate +
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


            var list = _depositManager.GetReportsExcelDeposituser(newfilters, false, LOGGEDIN_USER.AgencyId).List;
            return View(list);
        }
        [HttpGet]
        public ActionResult PrintAgencyRevenueReport(ReportSearchModeluser model, string FromDate, string ToDate, string PrintedDateServer)
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
                AgencyId = LOGGEDIN_USER.AgencyId,
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


            var list = _depositManager.GetAgentRevenueReportsExcelDeposituser(newfilters, false, LOGGEDIN_USER.AgencyId).List;
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

            var list = _meterManager.GetSalesExcelReportData(newfilters, false, LOGGEDIN_USER.AgencyId).List;
            return View(list);
        }

        [HttpGet]
        public ActionResult PrintGSTSalesReport(ReportSearchModeluser model, string FromDate, string ToDate, string PrintedDateServer)
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

            var list = _meterManager.GetUserGSTRechargesReport(newfilters, false, LOGGEDIN_USER.AgencyId).List;
            return View(list);
        }

        [AjaxOnly, HttpPost]
        public async Task<JsonResult> SendSms(ReChargeSMS request)
        {
            var td = _meterManager.GetSingleTransaction(string.Concat(request.TransactionId.Where(c => !Char.IsWhiteSpace(c))));
            if (td == null)
                return  Json( new { message = "Not found.", status = "failed" });

            if (request.PhoneNo == null)
                request.PhoneNo = "12345678";
            var requestmsg = new SendSMSRequest
            {
                Recipient = $"232{request.PhoneNo}",
                Payload = $"UID#:{td.SerialNumber}\n" +
                            $"{td.CreatedAt.ToString("dd/MM/yyyy")}\n" +
                            $"POSID:{td.POS.SerialNumber}\n" +
                            $"Meter:{td.MeterNumber1}\n" +
                            $"Amt:{BLL.Common.Utilities.FormatAmount(td.Amount)}\n" +
                            $"GST:{BLL.Common.Utilities.FormatAmount(Convert.ToDecimal(td.TaxCharge))}\n" +
                            $"Chg:{BLL.Common.Utilities.FormatAmount(Convert.ToDecimal(td.ServiceCharge))}\n" +
                            $"COU:{BLL.Common.Utilities.FormatAmount(Convert.ToDecimal(td.CostOfUnits))} \n" +
                            $"Units:{BLL.Common.Utilities.FormatAmount(Convert.ToDecimal(td.Units))}\n" +
                            $"PIN:{BLL.Common.Utilities.FormatThisToken(td.MeterToken1)}\n" +
                            "VENDTECH"
            };

            var json = JsonConvert.SerializeObject(requestmsg);

            HttpClient client = new HttpClient();
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            client.BaseAddress = new Uri("https://kwiktalk.io");
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            HttpRequestMessage httpRequest = new HttpRequestMessage(HttpMethod.Post, "/api/v2/submit");
            httpRequest.Content = new StringContent(json, Encoding.UTF8, "application/json");

            var res = await client.SendAsync(httpRequest);
            var stringResult = await res.Content.ReadAsStringAsync();

            if (res.StatusCode != (HttpStatusCode)200)
            {
                return Json(new { message = "Unable to send sms.", status = "failed" });
            }
            return Json(new { message = "Sms successfully sent.", status = "success" });
        }

        [AjaxOnly, HttpPost, Public]
        public JsonResult SendEmail(SendViaEmail request)
        {
            try
            {
                var td = _meterManager.GetSingleTransaction(string.Concat(request.TransactionId.Where(c => !Char.IsWhiteSpace(c))));
                if (td == null)
                    return Json(new { message = "Not found", status = "success" });

                var vendor = _userManager.GetUserDetailsByUserId(td.UserId);
                var emailTemplate = _emailTemplateManager.GetEmailTemplateByTemplateType(TemplateTypes.SendReceiptViaEmail);
                if (emailTemplate.TemplateStatus)
                {
                    string body = emailTemplate.TemplateContent;
                    body = body.Replace("%vendor%", vendor.Vendor);
                    body = body.Replace("%posid%", td.User.POS.FirstOrDefault().SerialNumber);
                    body = body.Replace("%customerName%", td.Customer);
                    body = body.Replace("%account%", td.AccountNumber);
                    body = body.Replace("%address%", td.CustomerAddress);
                    body = body.Replace("%meterNumber%", td.MeterNumber1);
                    body = body.Replace("%tarrif%", td.Tariff);
                    body = body.Replace("%amount%", BLL.Common.Utilities.FormatAmount(td.TenderedAmount));
                    body = body.Replace("%gst%", BLL.Common.Utilities.FormatAmount(Convert.ToDecimal(td.ServiceCharge)));
                    body = body.Replace("%serviceCharge%", BLL.Common.Utilities.FormatAmount(Convert.ToDecimal(td.TaxCharge)));
                    body = body.Replace("%debitRecovery%", td.DebitRecovery);
                    body = body.Replace("%costOfUnits%", td.CostOfUnits);
                    body = body.Replace("%units%", td.Units);
                    body = body.Replace("%pin%", BLL.Common.Utilities.FormatThisToken(td.MeterToken1));
                    body = body.Replace("%edsaSerial%", td.SerialNumber);
                    body = body.Replace("%vendtechSerial%", td.TransactionId);
                    body = body.Replace("%barcode%", td.MeterNumber1);
                    body = body.Replace("%date%", td.CreatedAt.ToString("dd/MM/yyyy"));
                    var file = BLL.Common.Utilities.CreatePdf(body, td.TransactionId + "_receipt.pdf");

                    var emailTemplate2 = _emailTemplateManager.GetEmailTemplateByTemplateType(TemplateTypes.SendReceiptViaEmailContent);
                    string body2 = emailTemplate2.TemplateContent;
                    body2 = body2.Replace("%customer%", "Customer");
                    body2 = body2.Replace("%invoiceNumber%", td.TransactionId);
                    body2 = body2.Replace("%meter%", td.MeterNumber1);
                    body2 = body2.Replace("%amount%", BLL.Common.Utilities.FormatAmount(td.TenderedAmount));
                    BLL.Common.Utilities.SendPDFEmail(request.Email, "Invoice - " + td.TransactionId + " from VENDTECHSL LTD", body2, file.FirstOrDefault().Value, td.TransactionId + "_receipt.pdf");
                }

                return Json(new { message = "Email successfully sent.", status = "success" });
            }
            catch (Exception)
            {
                return Json(new { message = "Email not sent.", status = "failed" });
            }
        }

        [Public]
        public ActionResult Downoload()
        {
            var pdfFilePath = "C:\\\\Users\\\\thispc\\\\Desktop\\\\FLAVETECH\\\\Vendtech\\\\vendtech-web\\\\VendTech\\/Receipts/30_receipt.pdf";

            var contentType = "application/pdf";
            return File(pdfFilePath, contentType, "reciept.pdf");
        }

    }
}