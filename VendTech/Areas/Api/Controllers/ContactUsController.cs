using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
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
    public class ContactUsController : BaseAPIController
    {
        private readonly IUserManager _userManager;
        private readonly IAuthenticateManager _authenticateManager;
        private readonly IContactUsManager _contactUsManager;
        public ContactUsController(IUserManager userManager, IErrorLogManager errorLogManager, IContactUsManager contactUsManager, IAuthenticateManager authenticateManager)
            : base(errorLogManager)
        {
            _userManager = userManager;
            _authenticateManager = authenticateManager;
            _contactUsManager = contactUsManager;
        }
         [HttpPost]
         [ResponseType(typeof(ResponseBase))]
         public HttpResponseMessage SaveRequest(ContactUsModel model)
         {
             model.UserId = LOGGEDIN_USER.UserId;
             var result = _contactUsManager.SaveContactUsRequest(model);
             return new JsonContent(result.Message, result.Status == ActionStatus.Successfull ? Status.Success : Status.Failed).ConvertToHttpResponseOK();
         }
       
    }
}
