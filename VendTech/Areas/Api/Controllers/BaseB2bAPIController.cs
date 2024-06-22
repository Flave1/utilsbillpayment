using System.Web.Http;
using VendTech.Attributes;
using VendTech.BLL.Interfaces;
using VendTech.BLL.Models;

namespace VendTech.Areas.Api.Controllers
{
    [HandelException, CheckAuthorization, ValidateModel]
    public class BaseB2bAPIController : ApiController
    {

        #region Variable Declaration
        protected readonly IErrorLogManager _errorLogManager;


        /// <summary>
        /// Contains Information for Logged In User
        /// </summary>
        public B2bUserAccessDTO LOGGEDIN_USER { get; set; }
        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="errorLogManager"></param>
        public BaseB2bAPIController(IErrorLogManager errorLogManager)
        {
            _errorLogManager = errorLogManager;

        }
    }
}
