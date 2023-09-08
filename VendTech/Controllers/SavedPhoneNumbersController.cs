#region Default Namespaces
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
#endregion

#region Custom Namespaces
using VendTech.Attributes;
using VendTech.BLL.Interfaces;
using Ninject;
using VendTech.BLL.Models;
using System.Web.Script.Serialization;
using VendTech.BLL.Common;
using Newtonsoft.Json;
using VendTech.BLL.Managers;
#endregion

namespace VendTech.Controllers
{
    /// <summary>
    /// Home Controller 
    /// Created On: 10/04/2015
    /// </summary>
    public class SavedPhoneNumbersController : AppUserBaseController
    {
        #region Variable Declaration
        private readonly IUserManager _userManager;
        private readonly IAuthenticateManager _authenticateManager;
        private readonly ICMSManager _cmsManager;
        private readonly IMeterManager _meterManager;
        private readonly IPlatformManager _platformManager;
        private readonly IPOSManager _posManager;
        private readonly IPlatformTransactionManager _platformTransactionManager;


        #endregion

        public SavedPhoneNumbersController(IUserManager userManager, IPlatformManager platformManager, IErrorLogManager errorLogManager, IAuthenticateManager authenticateManager, ICMSManager cmsManager, IMeterManager meterManager, IPOSManager posManager, IPlatformTransactionManager platformTransactionManager)
            : base(errorLogManager)
        {
            _userManager = userManager;
            _authenticateManager = authenticateManager;
            _cmsManager = cmsManager;
            _meterManager = meterManager;
            _platformManager = platformManager;
            _posManager = posManager;
            _platformTransactionManager = platformTransactionManager;
        }

        /// <summary>
        /// Index View 
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            ViewBag.SelectedTab = SelectedAdminTab.Meters;
            var meters = _meterManager.GetPhoneNumbers(LOGGEDIN_USER.UserID, 1, 1000000000, true);
            return View(meters);
        }
        [AjaxOnly, HttpPost]
        public JsonResult GetSavedPhoneNumbersPagingList(PagingModel model)
        {
            ViewBag.SelectedTab = SelectedAdminTab.Users;
            var pageNo = (model.PageNo / model.RecordsPerPage) + (model.PageNo % model.RecordsPerPage > 0 ? 1 : 0);
            var modal = _meterManager.GetPhoneNumbers(LOGGEDIN_USER.UserID, pageNo, model.RecordsPerPage, model.IsActive);
            List<string> resultString = new List<string>();
            resultString.Add(RenderRazorViewToString("Partials/_numberListing", modal));
            resultString.Add(modal.TotalCount.ToString());
            return JsonResult(resultString);
        }
        [AjaxOnly, HttpPost]
        public JsonResult Delete(long Id)
        {
            return JsonResult(_meterManager.DeletePhoneNumber(Id, LOGGEDIN_USER.UserID));
        }

        /// <summary>
        /// Index View 
        /// </summary>
        /// <returns></returns>
        public ActionResult AddEditPhoneNumbers(string number = "")
        {
            ViewBag.SelectedTab = SelectedAdminTab.Meters;
            NumberModel model = new NumberModel();
            //if (meterId.HasValue && meterId > 0)
            //    model = _meterManager.GetMeterDetail(meterId.Value);
            var list = _platformManager.GetOperatorType(PlatformTypeEnum.AIRTIME);
            ViewBag.meterMakes = list;
            model.Number = number;
            return View(model);

        }
        public ActionResult EditPhoneNumbers(long id)
        {
            NumberModel model = new NumberModel();
            if (id > 0)
                model = _meterManager.GetPhoneNumberDetail(id);
            var list = _platformManager.GetOperatorType(PlatformTypeEnum.AIRTIME);

            ViewBag.meterMakes = list;
            return View("AddEditPhoneNumbers", model);

        }
        [AjaxOnly, HttpPost]

        public JsonResult AddEditPhoneNumbers(NumberModel model)
        {
            model.UserId = LOGGEDIN_USER.UserID;
            model.IsSaved = true;
            return JsonResult(_meterManager.SavePhoneNUmber(model));
        }


    }
}