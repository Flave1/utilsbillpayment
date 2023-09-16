using Newtonsoft.Json;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using VendTech.Attributes;
using VendTech.BLL.Interfaces;
using VendTech.BLL.Models;
using VendTech.Framework.Api;

namespace VendTech.Areas.Api.Controllers
{
    public class NumberController : BaseAPIController
    {
        private readonly IMeterManager _meterManager;
        public NumberController(IErrorLogManager errorLogManager, IMeterManager meterManager)
            : base(errorLogManager)
        {
            _meterManager = meterManager;
        }
        [HttpPost]
        [ResponseType(typeof(ResponseBase))]
        public HttpResponseMessage SaveNumber(NumberModel model)
        {
            model.UserId = LOGGEDIN_USER.UserId;
            model.IsSaved = true;
            var result = _meterManager.SavePhoneNUmber(model);
            return new JsonContent(result.Message, result.Status == ActionStatus.Successfull ? Status.Success : Status.Failed).ConvertToHttpResponseOK();
        }
        [HttpPost]
        [ResponseType(typeof(ResponseBase))]
        public HttpResponseMessage DeletePhone(long meterId)
        {
            var result = _meterManager.DeletePhoneNumber(meterId, LOGGEDIN_USER.UserId);
            return new JsonContent(result.Message, result.Status == ActionStatus.Successfull ? Status.Success : Status.Failed).ConvertToHttpResponseOK();
        }
        [HttpGet]
        //[HttpGet, CheckAuthorizationAttribute.SkipAuthentication, CheckAuthorizationAttribute.SkipAuthorization]
        [ResponseType(typeof(ResponseBase))]
        public HttpResponseMessage GetNumbers(int pageNo, int pageSize)
        {
            var result = _meterManager.GetPhoneNumbers(LOGGEDIN_USER.UserId, pageNo, pageSize, true);
            return new JsonContent(result.TotalCount, result.Message, result.Status == ActionStatus.Successfull ? Status.Success : Status.Failed, result.List).ConvertToHttpResponseOK();
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

    }
}
