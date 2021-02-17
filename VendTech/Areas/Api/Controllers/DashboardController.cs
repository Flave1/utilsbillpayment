using System.Net.Http;
using System.Web.Http.Description;
using System.Web.Mvc;
using VendTech.Attributes;
using VendTech.BLL.Interfaces;
using VendTech.Framework.Api;

namespace VendTech.Areas.Api.Controllers
{
    public class DashboardController : BaseAPIController
    {
        private readonly IUserManager _userManager;
        public DashboardController(IUserManager userManager,
            IErrorLogManager errorLogManager)
            : base(errorLogManager)
        {
            _userManager = userManager;
        }

        [HttpGet, CheckAuthorizationAttribute.SkipAuthentication, CheckAuthorizationAttribute.SkipAuthorization]
        [ResponseType(typeof(ResponseBase))]
        public HttpResponseMessage GetNavigations(long userId)
        {
            var getNavigations = _userManager.GetNavigations(userId);
            return new JsonContent("Get Navigations", Status.Success, getNavigations).ConvertToHttpResponseOK();
        }

    }
}