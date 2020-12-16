using iTextSharp.text;
using iTextSharp.text.pdf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Configuration;
using System.Web.Http;
using System.Web.Http.Description;
using VendTech.Attributes;
using VendTech.BLL.Common;
using VendTech.BLL.Interfaces;
using VendTech.BLL.Models;
using VendTech.Framework.Api;

namespace VendTech.Areas.Api.Controllers
{
    public class MeterController : BaseAPIController
    {
        private readonly IUserManager _userManager;
        private readonly IAuthenticateManager _authenticateManager;
        private readonly IMeterManager _meterManager;
        public MeterController(IUserManager userManager, IErrorLogManager errorLogManager, IMeterManager meterManager, IAuthenticateManager authenticateManager)
            : base(errorLogManager)
        {
            _userManager = userManager;
            _authenticateManager = authenticateManager;
            _meterManager = meterManager;
        }
        [HttpPost]
        [ResponseType(typeof(ResponseBase))]
        public HttpResponseMessage SaveMeter(MeterModel model)
        {
            model.UserId = LOGGEDIN_USER.UserId;
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
            var result = _meterManager.GetMeters(LOGGEDIN_USER.UserId, pageNo, pageSize);
            return new JsonContent(result.TotalCount, result.Message, result.Status == ActionStatus.Successfull ? Status.Success : Status.Failed, result.List).ConvertToHttpResponseOK();
        }
        [HttpPost]
        [ResponseType(typeof(ResponseBase))]
        public HttpResponseMessage RechargeMeter(RechargeMeterModel model)
        {
            model.UserId = LOGGEDIN_USER.UserId;
            var result = _meterManager.RechargeMeter(model);
            return new JsonContent(result.Message, result.Status == ActionStatus.Successfull ? Status.Success : Status.Failed).ConvertToHttpResponseOK();
        }

        [HttpGet]
        [ResponseType(typeof(ResponseBase))]
        public HttpResponseMessage GetUserMeterRecharges(int pageNo, int pageSize)
        {
            var result = _meterManager.GetUserMeterRecharges(LOGGEDIN_USER.UserId, pageNo, pageSize);
            return new JsonContent(result.TotalCount, result.Message, result.Status == ActionStatus.Successfull ? Status.Success : Status.Failed, result.List).ConvertToHttpResponseOK();
        }
        [HttpGet]
        [ResponseType(typeof(ResponseBase))]
        public HttpResponseMessage GetRechargeDetail(long rechargeId)
        {
            var result = _meterManager.GetRechargeDetail(rechargeId);
            return new JsonContent( result.Message, result.Status == ActionStatus.Successfull ? Status.Success : Status.Failed, result.Object).ConvertToHttpResponseOK();
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
    }
}
