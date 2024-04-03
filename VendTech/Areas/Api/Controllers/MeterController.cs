using iTextSharp.text;
using iTextSharp.text.pdf;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Description;
using VendTech.Attributes;
using VendTech.BLL.Interfaces;
using VendTech.BLL.Managers;
using VendTech.BLL.Models;
using VendTech.Framework.Api;

namespace VendTech.Areas.Api.Controllers
{
    public class MeterController : BaseAPIController
    {
        private readonly IUserManager _userManager;
        private readonly IAuthenticateManager _authenticateManager;
        private readonly IMeterManager _meterManager;
        private readonly IEmailTemplateManager _emailTemplateManager;
        private readonly IPlatformManager _platformManager;
        public MeterController(IUserManager userManager, IErrorLogManager errorLogManager, IMeterManager meterManager, IAuthenticateManager authenticateManager, IEmailTemplateManager emailTemplateManager, IPlatformManager platformManager)
            : base(errorLogManager)
        {
            _userManager = userManager;
            _authenticateManager = authenticateManager;
            _meterManager = meterManager;
            _emailTemplateManager = emailTemplateManager;
            _platformManager = platformManager;
        }
        [HttpPost]
        [ResponseType(typeof(ResponseBase))]
        public HttpResponseMessage SaveMeter(MeterModel model)
        {
            model.UserId = LOGGEDIN_USER.UserId;
            model.IsSaved = true;
            var result = _meterManager.SaveMeter(model);
            return new JsonContent(result.Message, result.Status == ActionStatus.Successfull ? Status.Success : Status.Failed).ConvertToHttpResponseOK();
        }
        [HttpPost]
        [ResponseType(typeof(ResponseBase))]
        public HttpResponseMessage DeleteMeter(long meterId)
        {
            var result = _meterManager.DeleteMeter(meterId, LOGGEDIN_USER.UserId);
            return new JsonContent(result.Message, result.Status == ActionStatus.Successfull ? Status.Success : Status.Failed).ConvertToHttpResponseOK();
        }
        [HttpGet]
        [ResponseType(typeof(ResponseBase))]
        public HttpResponseMessage GetMeters(int pageNo, int pageSize)
        {
            var result = _meterManager.GetMeters(LOGGEDIN_USER.UserId, pageNo, pageSize, true);
            return new JsonContent(result.TotalCount, result.Message, result.Status == ActionStatus.Successfull ? Status.Success : Status.Failed, result.List).ConvertToHttpResponseOK();
        }


        //[HttpPost]
        //[ResponseType(typeof(ResponseBase))]
        //public async Task<HttpResponseMessage> RechargeMeter(RechargeMeterModel model)
        //{
        //    model.UserId = LOGGEDIN_USER.UserId;

        //    var minVend = _meterManager.ReturnElectricityMinVend();
        //    if (model.Amount < minVend)
        //    {
        //        return new JsonContent($"PLEASE TENDER NLe: {minVend} & ABOVE", Status.Failed).ConvertToHttpResponseOK();
        //    }
        //    var result = await _meterManager.RechargeMeterReturnIMPROVED(model);
        //    return new JsonContent(result.Message, result.Status == ActionStatus.Successfull ? Status.Success : Status.Failed).ConvertToHttpResponseOK();
        //}

        [HttpGet]
        [ResponseType(typeof(ResponseBase))]
        public HttpResponseMessage GetUserMeterRecharges(int pageNo, int pageSize)
        {
            var result = _meterManager.GetUserMeterRecharges(LOGGEDIN_USER.UserId, pageNo, pageSize);
            return new JsonContent(result.TotalCount, result.Message, result.Status == ActionStatus.Successfull ? Status.Success : Status.Failed, result.List).ConvertToHttpResponseOK();
        }
        [HttpGet]
        //[HttpGet, CheckAuthorizationAttribute.SkipAuthentication, CheckAuthorizationAttribute.SkipAuthorization]
        [ResponseType(typeof(ResponseBase))]
        public HttpResponseMessage GetRechargeDetail(long rechargeId)
        {
            var result = _meterManager.GetMobileRechargeDetail(rechargeId);
            return new JsonContent(result.Message, result.Status == ActionStatus.Successfull ? Status.Success : Status.Failed, result.Object).ConvertToHttpResponseOK();
        }

        [HttpGet]
        [ResponseType(typeof(ResponseBase))]
        public HttpResponseMessage GetMeterRechargePdf(long rechargeId)
        {
            var result = new RechargeDetailPDFData();
            var domain = VendTech.BLL.Common.Utilities.DomainUrl;
            var folderPath = HttpContext.Current.Server.MapPath("~/Content/RechargePdf");
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);
            var name = "MeterRecharge_" + rechargeId + ".pdf";
            var fileName = Path.Combine(folderPath, name);

            if (!File.Exists(fileName))

                result = _meterManager.GetRechargePDFData(rechargeId);
            else
            {
                domain = domain + "/Content/RechargePdf/" + name;
                return new JsonContent("Pdf link fetched successfully", Status.Success, new { path = domain }).ConvertToHttpResponseOK();
            }
            //Create a byte array that will eventually hold our final PDF

            Byte[] bytes;

            //Boilerplate iTextSharp setup here
            //Create a stream that we can write to, in this case a MemoryStream
            using (var ms = new MemoryStream())
            {

                //Create an iTextSharp Document which is an abstraction of a PDF but **NOT** a PDF
                using (var doc = new Document())
                {

                    //Create a writer that's bound to our PDF abstraction and our stream
                    using (var writer = PdfWriter.GetInstance(doc, ms))
                    {

                        //Open the document for writing
                        doc.Open();

                        //Our sample HTML and CSS
                        var example_html = System.IO.File.ReadAllText(HttpContext.Current.Server.MapPath("~/Templates/RechargeMeter.html"));

                        /**************************************************
                         * Example #1                                     *
                         *                                                *
                         * Use the built-in HTMLWorker to parse the HTML. *
                         * Only inline CSS is supported.                  *
                         * ************************************************/

                        //Create a new HTMLWorker bound to our document
                        example_html = example_html.Replace("%UserName%", result.UserName);
                        example_html = example_html.Replace("%MeterNumber%", result.MeterNumber);
                        example_html = example_html.Replace("%Amount%", result.Amount.ToString());
                        example_html = example_html.Replace("%CreatedAt%", result.CreatedAt);
                        example_html = example_html.Replace("%TransactionId%", result.TransactionId);
                        example_html = example_html.Replace("%Status%", result.Status);
                        using (var htmlWorker = new iTextSharp.text.html.simpleparser.HTMLWorker(doc))
                        {

                            //HTMLWorker doesn't read a string directly but instead needs a TextReader (which StringReader subclasses)
                            using (var sr = new StringReader(example_html))
                            {

                                //Parse the HTML
                                htmlWorker.Parse(sr);
                            }
                        }




                        doc.Close();
                    }
                }
                bytes = ms.ToArray();
            }
            System.IO.File.WriteAllBytes(fileName, bytes);
            domain = domain + "/Content/RechargePdf/" + name;
            return new JsonContent("Pdf link fetched successfully", Status.Success, new { path = domain }).ConvertToHttpResponseOK();
        }

        [HttpPost]
        [ResponseType(typeof(ResponseBase))]
        //[HttpPost, CheckAuthorizationAttribute.SkipAuthentication, CheckAuthorizationAttribute.SkipAuthorization]
        public async Task<HttpResponseMessage> RechargeMeterReceipt(RechargeMeterModel model)
        {
            var platf = _platformManager.GetSinglePlatform(1);
            if (platf.DisablePlatform)
            {
                return new JsonContent("VENDING SERVICE IS DISABLED", Status.Failed).ConvertToHttpResponseOK();
            }

            if (platf.MinimumAmount > model.Amount)
            {
                return new JsonContent($"PLEASE TENDER NLe: {platf.MinimumAmount} & ABOVE", Status.Failed).ConvertToHttpResponseOK();
            }
            model.UserId = LOGGEDIN_USER.UserId;
            var result = await _meterManager.RechargeMeterReturnIMPROVED(model);
            return new JsonContent(result.ReceiptStatus.Message, result.ReceiptStatus.Status == "unsuccessfull" ? Status.Failed : Status.Success, result).ConvertToHttpResponseOK();
        }

        [HttpPost, CheckAuthorizationAttribute.SkipAuthentication, CheckAuthorizationAttribute.SkipAuthorization]
        [ResponseType(typeof(ResponseBase))]
        public HttpResponseMessage TransactionDetail(Tokenobject tokenobject)
        {
            var result = _meterManager.ReturnVoucherReceipt(tokenobject.Token.Trim());
            return new JsonContent(result.ReceiptStatus.Message, result.ReceiptStatus.Status == "unsuccessfull" ? Status.Failed : Status.Success, result).ConvertToHttpResponseOK();
        }


        [HttpPost, CheckAuthorizationAttribute.SkipAuthentication, CheckAuthorizationAttribute.SkipAuthorization]
        [ResponseType(typeof(ResponseBase))]
        public  async Task<HttpResponseMessage> SendSmsOnRecharge(ReChargeSMS request)
        {
            var td = _meterManager.GetSingleTransaction(string.Concat(request.TransactionId.Where(c => !Char.IsWhiteSpace(c))));
            if (td == null)
                return new JsonContent("Not found.", Status.Failed, request).ConvertToHttpResponseOK();

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


            _meterManager.LogSms(td, request.PhoneNo);
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
                return new JsonContent("Unable to send sms.", Status.Failed, stringResult).ConvertToHttpResponseOK();
            }
            return new JsonContent("Sms successfully sent.", Status.Success, stringResult).ConvertToHttpResponseOK();
        }

        [HttpPost, CheckAuthorizationAttribute.SkipAuthentication, CheckAuthorizationAttribute.SkipAuthorization]
        [ResponseType(typeof(ResponseBase))]
        public async Task<HttpResponseMessage> SendViaEmail(SendViaEmail request)
        {
            try
            {
                var td = _meterManager.GetSingleTransaction(string.Concat(request.TransactionId.Where(c => !Char.IsWhiteSpace(c))));
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
                    var file = BLL.Common.Utilities.CreatePdf(body, td.TransactionId);

                    var emailTemplate2 = _emailTemplateManager.GetEmailTemplateByTemplateType(TemplateTypes.SendReceiptViaEmailContent);
                    string body2 = emailTemplate2.TemplateContent;
                    body2 = body2.Replace("%customer%", "Customer");
                    body2 = body2.Replace("%invoiceNumber%", td.TransactionId);
                    body2 = body2.Replace("%meter%", td.MeterNumber1);
                    body2 = body2.Replace("%amount%", BLL.Common.Utilities.FormatAmount(td.TenderedAmount));
                    BLL.Common.Utilities.SendPDFEmail(request.Email, "Invoice - " + td.TransactionId + " from VENDTECHSL LTD", body2, file.FirstOrDefault().Value, td.TransactionId + "_receipt.pdf");
                }

                return new JsonContent("Sms successfully sent.", Status.Success, "").ConvertToHttpResponseOK();
            }
            catch (Exception)
            {
                return new JsonContent("Sms Not sent.", Status.Failed, "").ConvertToHttpResponseOK();
            }
        }

        [HttpPost, CheckAuthorizationAttribute.SkipAuthentication, CheckAuthorizationAttribute.SkipAuthorization]
        [ResponseType(typeof(ResponseBase))]
        public HttpResponseMessage Redenominate()
        {
            try
            {
                 _meterManager.RedenominateBalnces();
            }
            catch (Exception)
            {
                throw;
            }
            return new JsonContent("Sms successfully sent.", Status.Success, null).ConvertToHttpResponseOK();
        }

    }
    public class Tokenobject
    {
        public string Token { get; set; }
    }
}
