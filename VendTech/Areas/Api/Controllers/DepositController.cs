using iTextSharp.text;
using iTextSharp.text.pdf;
using System;
using System.IO;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Web.Http.Description;
using VendTech.BLL.Common;
using VendTech.BLL.Interfaces;
using VendTech.BLL.Models;
using VendTech.Framework.Api;


namespace VendTech.Areas.Api.Controllers
{
    public class DepositController : BaseAPIController
    {
        private readonly IUserManager _userManager;
        private readonly IAuthenticateManager _authenticateManager;
        private readonly IMeterManager _meterManager;
        private readonly IDepositManager _depositManager;
        private readonly IPOSManager _posManager;
        private readonly IEmailTemplateManager _templateManager;
        private readonly IPaymentTypeManager _paymentTypeManager;
        public DepositController(IUserManager userManager, IErrorLogManager errorLogManager, IMeterManager meterManager, IAuthenticateManager authenticateManager, IDepositManager depositManager, IPOSManager pOSManager, IEmailTemplateManager emailTemplateManager, IPaymentTypeManager paymentTypeManager)
            : base(errorLogManager)
        {
            _userManager = userManager;
            _authenticateManager = authenticateManager;
            _meterManager = meterManager;
            _depositManager = depositManager;
            _posManager = pOSManager;
            _templateManager = emailTemplateManager;
            _paymentTypeManager = paymentTypeManager;
        }

        [HttpPost]
        [ResponseType(typeof(ResponseBase))]
        public HttpResponseMessage SaveDepositRequest(DepositModel model)
        {
            model.UserId = LOGGEDIN_USER.UserId;
            //model.TotalAmountWithPercentage = model.Amount;
            model.BankAccountId = 1;
            //model.ValueDate = DateTime.Now.Date.ToString("dd/MM/yyyy");

            if (model.ContinueDepoit == 0)
            {
                var pendingDeposits = _depositManager.ReturnPendingDepositsTotalAmount(model);
                if (pendingDeposits > 0)
                {
                    return new JsonContent(BLL.Common.Utilities.FormatAmount(pendingDeposits), Status.Success).ConvertToHttpResponseOK();
                }
            }

            var result = _depositManager.SaveDepositRequest(model);


            var adminUsers = _userManager.GetAllAdminUsersByDepositRelease();

            var pos = _posManager.GetSinglePos(result.Object.POSId);
            if (pos != null)
            {
                foreach (var admin in adminUsers)
                {
                    var emailTemplate = _templateManager.GetEmailTemplateByTemplateType(TemplateTypes.DepositRequestNotification);
                    if (emailTemplate != null)
                    {
                        if (emailTemplate.TemplateStatus)
                        {
                            string body = emailTemplate.TemplateContent;
                            body = body.Replace("%AdminUserName%", admin.Name);
                            body = body.Replace("%VendorName%", pos.User.Vendor);
                            body = body.Replace("%POSID%", pos.SerialNumber);
                            body = body.Replace("%REF%", result.Object.CheckNumberOrSlipId);
                            body = body.Replace("%Amount%", BLL.Common.Utilities.FormatAmount(result.Object.Amount));
                            body = body.Replace("%CurrencyCode%", BLL.Common.Utilities.GetCountry().CurrencyCode);
                            VendTech.BLL.Common.Utilities.SendEmail(admin.Email, emailTemplate.EmailSubject, body);
                        }

                    }
                }
            }


            return new JsonContent(result.Message, result.Status == ActionStatus.Successfull ? Status.Success : Status.Failed).ConvertToHttpResponseOK();
        }


        [HttpPost]
        [ResponseType(typeof(ResponseBase))]
        public HttpResponseMessage SaveDepositRequestTemp(DepositModel model)
        {
            model.UserId = LOGGEDIN_USER.UserId;
            //model.TotalAmountWithPercentage = model.Amount;
            model.BankAccountId = 1;
            //model.ValueDate = DateTime.Now.Date.ToString("dd/MM/yyyy");

            if (model.ContinueDepoit == 0)
            {
                var pendingDeposits = _depositManager.ReturnPendingDepositsTotalAmount(model);
                if (pendingDeposits > 0)
                {
                    return new JsonContent(BLL.Common.Utilities.FormatAmount(pendingDeposits), Status.Success).ConvertToHttpResponseOK();
                }
            }

            var result = _depositManager.SaveDepositRequest(model);


            var adminUsers = _userManager.GetAllAdminUsersByDepositRelease();

            var pos = _posManager.GetSinglePos(result.Object.POSId);
            if (pos != null)
            {
                foreach (var admin in adminUsers)
                {
                    var emailTemplate = _templateManager.GetEmailTemplateByTemplateType(TemplateTypes.DepositRequestNotification);
                    if (emailTemplate != null)
                    {
                        if (emailTemplate.TemplateStatus)
                        {
                            string body = emailTemplate.TemplateContent;
                            body = body.Replace("%AdminUserName%", admin.Name);
                            body = body.Replace("%VendorName%", pos.User.Vendor);
                            body = body.Replace("%POSID%", pos.SerialNumber);
                            body = body.Replace("%REF%", result.Object.CheckNumberOrSlipId);
                            body = body.Replace("%Amount%", BLL.Common.Utilities.FormatAmount(result.Object.Amount));
                            VendTech.BLL.Common.Utilities.SendEmail(admin.Email, emailTemplate.EmailSubject, body);
                        }

                    }
                }
            }


            return new JsonContent(result.Message, result.Status == ActionStatus.Successfull ? Status.Success : Status.Failed).ConvertToHttpResponseOK();
        }




        [HttpGet]
         [ResponseType(typeof(ResponseBase))]
         public HttpResponseMessage GetDeposits(int pageNo,int pageSize)
         {            
             var result = _depositManager.GetUserDepositList(pageNo,pageSize,LOGGEDIN_USER.UserId);
             return new JsonContent(result.TotalCount,result.Message, result.Status == ActionStatus.Successfull ? Status.Success : Status.Failed,result.List).ConvertToHttpResponseOK();
         }

         [HttpGet]
         [ResponseType(typeof(ResponseBase))]
         public HttpResponseMessage GetDepositDetail(long depositId)
         {
             var result = _depositManager.GetDepositDetail(depositId);
             return new JsonContent(result.Message, result.Status == ActionStatus.Successfull ? Status.Success : Status.Failed, result.Object).ConvertToHttpResponseOK();
         }
         [HttpGet]
         [ResponseType(typeof(ResponseBase))]
         public HttpResponseMessage GetDepositPdf(long depositId)
         {
             var result = new ActionOutput<DepositListingModel>();
             var domain = VendTech.BLL.Common.Utilities.DomainUrl;
             result = _depositManager.GetDepositDetail(depositId);
             if(result.Object==null || result.Object.DepositId<=0)
                 return new JsonContent("Error occured in generating pdf link.", Status.Success, new { path = "" }).ConvertToHttpResponseOK();
             var folderName = "/Content/DepositPdf/" + result.Object.Status;
             var folderPath = HttpContext.Current.Server.MapPath("~"+folderName);
             if (!Directory.Exists(folderPath))
                 Directory.CreateDirectory(folderPath);
             var name = "Deposit_" + depositId + ".pdf";
             var fileName = Path.Combine(folderPath, name);
             if (File.Exists(fileName))
             {
                 domain = domain + folderName+"/" + name;
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
                         var example_html = System.IO.File.ReadAllText(HttpContext.Current.Server.MapPath("~/Templates/DepositPDF.html"));

                         /**************************************************
                          * Example #1                                     *
                          *                                                *
                          * Use the built-in HTMLWorker to parse the HTML. *
                          * Only inline CSS is supported.                  *
                          * ************************************************/

                         //Create a new HTMLWorker bound to our document
                         example_html = example_html.Replace("%UserName%", result.Object.UserName);
                         example_html = example_html.Replace("%ChkNoOrSlipId%", result.Object.ChkNoOrSlipId);
                         example_html = example_html.Replace("%Amount%", result.Object.Amount.ToString());
                         example_html = example_html.Replace("%CreatedAt%", result.Object.CreatedAt);
                         example_html = example_html.Replace("%TransactionId%", result.Object.TransactionId);
                         example_html = example_html.Replace("%Status%", result.Object.Status);
                         example_html = example_html.Replace("%Comments%", result.Object.Comments);
                         example_html = example_html.Replace("%Type%", result.Object.Type);
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
             domain = domain + folderName+"/" + name;
             return new JsonContent("Pdf link fetched successfully", Status.Success, new { path = domain }).ConvertToHttpResponseOK();
         }

        [HttpGet]
        [ResponseType(typeof(ResponseBase))]
        public HttpResponseMessage GetPaymentTypes()
        {
            var result = _paymentTypeManager.GetPaymentTypeSelectList();
            return new JsonContent("Payment types fetched successfully.", Status.Success, result).ConvertToHttpResponseOK();
        }
    }
}
