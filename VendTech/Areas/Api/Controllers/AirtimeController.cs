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
using VendTech.BLL.Models;
using VendTech.Framework.Api;

namespace VendTech.Areas.Api.Controllers
{
    public class AirtimeController : BaseAPIController
    {
        private readonly IUserManager _userManager;
        private readonly IAuthenticateManager _authenticateManager;
        private IPlatformTransactionManager _platformTransactionManager;
        private readonly IEmailTemplateManager _emailTemplateManager;
        private readonly IPlatformManager _platformManager;
        public AirtimeController(IUserManager userManager, IErrorLogManager errorLogManager, IAuthenticateManager authenticateManager, IEmailTemplateManager emailTemplateManager, IPlatformManager platformManager, IPlatformTransactionManager platformTransactionManager)
            : base(errorLogManager)
        {
            _userManager = userManager;
            _authenticateManager = authenticateManager;
            _emailTemplateManager = emailTemplateManager;
            _platformManager = platformManager;
            _platformTransactionManager = platformTransactionManager;
        }



        [HttpPost]
        [ResponseType(typeof(ResponseBase))]
        //[HttpPost, CheckAuthorizationAttribute.SkipAuthentication, CheckAuthorizationAttribute.SkipAuthorization]
        public HttpResponseMessage RechargePhone(AirtimePurchaseModel request)
        {
            var platf = _platformManager.GetSinglePlatform(request.PlatformId);
            if (platf.DisablePlatform)
            {
                return new JsonContent("SELECTED SERVICE IS DISABLED", Status.Failed).ConvertToHttpResponseOK();
            }

            if (platf.MinimumAmount > request.Amount)
            {
                return new JsonContent($"PLEASE TENDER NLe: {platf.MinimumAmount} & ABOVE", Status.Failed).ConvertToHttpResponseOK();
            }
            request.UserId = LOGGEDIN_USER.UserId;


            if (!request.Phone.StartsWith("234") && !request.Phone.StartsWith("+234"))
            {
                request.Phone = "234" + request.Phone;
            }
            request.Currency = "SLE";

            var model = new PlatformTransactionModel { PlatformId = request.PlatformId, Amount = request.Amount, Currency = request.Currency, UserId = request.UserId, Beneficiary = request.Phone };
            var result = _platformTransactionManager.RechargeAirtime(model);
            if (result.ReceiptStatus.Status == "unsuccessful")
            {
                return new JsonContent(result.ReceiptStatus.Message, result.ReceiptStatus.Status == "unsuccessfull" ? Status.Failed : Status.Success, result).ConvertToHttpResponseOK();
            }
            if (result.ReceiptStatus.Status == "pending")
            {
                return new JsonContent(result.ReceiptStatus.Message, Status.Success, result).ConvertToHttpResponseOK();
            }
            if (result != null)
                return new JsonContent(result.ReceiptStatus.Message, Status.Success, result).ConvertToHttpResponseOK();
            return new JsonContent(result.ReceiptStatus.Message, result.ReceiptStatus.Status == "unsuccessfull" ? Status.Failed : Status.Success, result).ConvertToHttpResponseOK();


        }


    }
}
