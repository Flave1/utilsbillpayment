using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
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




        /// <summary>
        /// 
        /// </summary>
        /// <param name="errorLogManager"></param>
        public BaseAPIController(IErrorLogManager errorLogManager)
        {
            _errorLogManager = errorLogManager;

        }
    }
}
