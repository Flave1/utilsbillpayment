using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using VendTech.Attributes;
using VendTech.BLL.Interfaces;
using VendTech.BLL.Models;

namespace VendTech.Areas.Api.Controllers
{
    [HandelException, CheckAuthorization, ValidateModel]
    public class BaseAPIController : ApiController
    {

        #region Variable Declaration
        protected readonly IErrorLogManager _errorLogManager;


        /// <summary>
        /// Contains Information for Logged In User
        /// </summary>
        public UserModel LOGGEDIN_USER { get; set; }
        #endregion


        public static readonly string CurrentAppVersion = "2.4";

        /// <summary>
        /// 
        /// </summary>
        /// <param name="errorLogManager"></param>
        public BaseAPIController(IErrorLogManager errorLogManager)
        {
            _errorLogManager = errorLogManager;

        }

        public async Task<bool> SendSmsAsync(SendSMSRequest model)
        {
            try
            {
               
                return true;
            }
            catch (HttpRequestException e)
            {
                throw e;
            }
            catch (Exception x)
            {
                throw x;
            }
        }
    }
}
