using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using VendTech.Attributes;
using VendTech.BLL.Common;
using VendTech.BLL.Interfaces;
using VendTech.BLL.Managers;
using VendTech.BLL.Models;
using VendTech.Framework.Api;

namespace VendTech.Areas.Api.Controllers
{
    public class ReportController : BaseAPIController
    {
        private readonly IDepositManager _depositManager;
        private readonly IMeterManager _meterManager;
        private readonly IUserManager _userManager;
        private readonly IEmailTemplateManager _emailTemplateManager;
        public ReportController(IErrorLogManager errorLogManager, IDepositManager depositManager, IMeterManager meterManager, IUserManager userManager, IEmailTemplateManager emailTemplateManager)
            : base(errorLogManager)
        {
            _depositManager = depositManager;
            _meterManager = meterManager;
            _userManager = userManager;
            _emailTemplateManager = emailTemplateManager;
        }
        [HttpPost]
        [ResponseType(typeof(ResponseBase))]
        public HttpResponseMessage GetSalesReport(ReportSearchModel model)
        {
            model.SortBy = "CreatedAt";
            model.SortOrder = "Desc";
            model.VendorId = LOGGEDIN_USER.UserId;
            model.PageNo = ((model.PageNo - 1) * model.RecordsPerPage) + 1;

            //CultureInfo provider = CultureInfo.InvariantCulture;

            //if (model.From != null)
            //{
            //    model.From = DateTime.ParseExact(model.From.Value.Date.ToString(), "dd/MM/yyyy", provider);
            //}

            //if (model.To != null)
            //{
            //    model.To = DateTime.ParseExact(model.To.Value.ToString(), "dd/MM/yyyy", provider);
            //}

            var deposits = new PagingResult<MeterRechargeApiListingModelMobile>();
            deposits = _meterManager.GetUserMeterRechargesReportMobileAsync(model);
            return new JsonContent(deposits.TotalCount, deposits.Message, Status.Success, deposits.List).ConvertToHttpResponseOK();
        }
        [HttpPost]
        [ResponseType(typeof(ResponseBase))]
        public HttpResponseMessage GetDepositReport(ReportSearchModel model)
        {
            model.SortBy = "CreatedAt";
            model.SortOrder = "Desc";
            model.VendorId = LOGGEDIN_USER.UserId;
            model.PageNo = ((model.PageNo - 1) * model.RecordsPerPage) + 1;

            //CultureInfo provider = CultureInfo.InvariantCulture;
            //if (model.From != null)
            //{
            //    model.From = DateTime.ParseExact(model.From.Value.Date.ToString(), "dd/MM/yyyy", provider);
            //}

            //if (model.To != null)
            //{
            //    model.To = DateTime.ParseExact(model.To.Value.ToString(), "dd/MM/yyyy", provider);
            //}

            var deposits = new PagingResult<DepositListingModelMobile>();
            deposits = _depositManager.GetReportsMobilePagedList(model);
            return new JsonContent(deposits.TotalCount, deposits.Message, Status.Success, deposits.List).ConvertToHttpResponseOK();
        }


        [HttpPost, CheckAuthorizationAttribute.SkipAuthentication, CheckAuthorizationAttribute.SkipAuthorization]
        [ResponseType(typeof(ResponseBase))]
        public HttpResponseMessage CreateEdsaAsPDF(RechargeSimpleRequest request)
        {
            try
            {
                var td = _meterManager.GetSingleTransaction(string.Concat(request.Target.Where(c => !Char.IsWhiteSpace(c))));
                if (td == null)
                    return new JsonContent("Not found.", Status.Failed, request).ConvertToHttpResponseOK();

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
                    body = body.Replace("%amount%", Utilities.FormatAmount(td.TenderedAmount));
                    body = body.Replace("%gst%", Utilities.FormatAmount(Convert.ToDecimal(td.ServiceCharge)));
                    body = body.Replace("%serviceCharge%", Utilities.FormatAmount(Convert.ToDecimal(td.TaxCharge)));
                    body = body.Replace("%debitRecovery%", td.DebitRecovery);
                    body = body.Replace("%costOfUnits%", td.CostOfUnits);
                    body = body.Replace("%units%", td.Units);
                    body = body.Replace("%pin%", BLL.Common.Utilities.FormatThisToken(td.MeterToken1));
                    body = body.Replace("%edsaSerial%", td.SerialNumber);
                    body = body.Replace("%vendtechSerial%", td.TransactionId);
                    body = body.Replace("%barcode%", td.MeterNumber1);
                    body = body.Replace("%date%", td.CreatedAt.ToString("dd/MM/yyyy"));
                    var file = Utilities.CreatePdf(body, td.TransactionId);

                    //var response = new HttpResponseMessage(System.Net.HttpStatusCode.OK);

                    //byte[] fileBytes = File.ReadAllBytes(file);

                    //response.Content = new ByteArrayContent(fileBytes);
                    //response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment");
                    //response.Content.Headers.ContentDisposition.FileName = "receipt.pdf";
                    //response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
                    //return response;


                    return new JsonContent("PDF Created successfully.", Status.Success, file.FirstOrDefault().Key).ConvertToHttpResponseOK();
                }
                return new JsonContent("PDF Not Created.", Status.Failed, "").ConvertToHttpResponseOK();

            }
            catch (Exception)
            {
                return new JsonContent("Sms Not sent.", Status.Failed, "").ConvertToHttpResponseOK();
            }
        }

        [HttpPost, CheckAuthorizationAttribute.SkipAuthentication, CheckAuthorizationAttribute.SkipAuthorization]
        [ResponseType(typeof(ResponseBase))]
        public HttpResponseMessage DeleteFileFromDirectory(RechargeSimpleRequest request)
        {
            try
            {
                Utilities.DeleteFileFromDirectory(request.Target);
                return new JsonContent("File removed successfully.", Status.Success, "").ConvertToHttpResponseOK();
            }
            catch (Exception ex)
            {
                return new JsonContent("Invalid request.", Status.Failed, "").ConvertToHttpResponseOK();
            }
        }

        [HttpPost, CheckAuthorizationAttribute.SkipAuthentication, CheckAuthorizationAttribute.SkipAuthorization]
        [ResponseType(typeof(ResponseBase))]
        public HttpResponseMessage Downoload()
        {
            var pdfFilePath = "C:\\\\Users\\\\thispc\\\\Desktop\\\\FLAVETECH\\\\Vendtech\\\\vendtech-web\\\\VendTech\\/Receipts/30_receipt.pdf";
            if (!File.Exists(pdfFilePath))
            {
                return Request.CreateErrorResponse(System.Net.HttpStatusCode.NotFound, "File not found");
            }

            var response = new HttpResponseMessage(System.Net.HttpStatusCode.OK);

            byte[] fileBytes = File.ReadAllBytes(pdfFilePath);

            response.Content = new ByteArrayContent(fileBytes);
            response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment");
            response.Content.Headers.ContentDisposition.FileName = "receipt.pdf";
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");

            return response;
        }


    }
}
