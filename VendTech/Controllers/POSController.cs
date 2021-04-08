#region Default Namespaces
using System.Collections.Generic;
using System.Web.Mvc;
using VendTech.Areas.Admin.Controllers;
#endregion

#region Custom Namespaces
using VendTech.Attributes;
using VendTech.BLL.Interfaces;
using VendTech.BLL.Models;
#endregion

namespace VendTech.Controllers
{
    /// <summary>
    /// Home Controller 
    /// Created On: 10/04/2015
    /// </summary>
    public class POSController : AppUserBaseController
    {
        #region Variable Declaration
        private readonly IUserManager _userManager;
        private readonly IAuthenticateManager _authenticateManager;
        private readonly ICMSManager _cmsManager;
        private readonly IMeterManager _meterManager;
        private readonly IPlatformManager _platformManager;
        private readonly IVendorManager _vendorManager;
        private readonly IPOSManager _posManager;


        #endregion

        public POSController(IUserManager userManager, IPlatformManager platformManager, IErrorLogManager errorLogManager, IAuthenticateManager authenticateManager, ICMSManager cmsManager, IMeterManager meterManager, IVendorManager vendorManager, IPOSManager posManager)
            : base(errorLogManager)
        {
            _userManager = userManager;
            _authenticateManager = authenticateManager;
            _cmsManager = cmsManager;
            _meterManager = meterManager;
            _platformManager = platformManager;
            _vendorManager = vendorManager;
            _posManager = posManager;
        }

        /// <summary>
        /// Index View 
        /// </summary>
        /// <returns></returns>
        public ActionResult ManagePos()
        {
            var vendorId = _vendorManager.GetVendorIdByAppUserId(LOGGEDIN_USER.UserID);
            var vendor = _vendorManager.GetVendorDetail(vendorId);
            var users = new PagingResult<POSListingModel>();
            //if (vendorId > 0)
            //{
            users = _posManager.GetPOSPagedList(PagingModel.DefaultModel("CreatedAt", "Desc"), 0, LOGGEDIN_USER.UserID);
            ViewBag.VendorName = vendor != null ? vendor.Vendor : "";

            //}
            return View(users);

        }
        [AjaxOnly, HttpPost]
        public JsonResult GetPosPagingList(PagingModel model)
        {
            ViewBag.SelectedTab = SelectedAdminTab.POS;
            //var vendorId = _vendorManager.GetVendorIdByAppUserId(LOGGEDIN_USER.UserID);
            var modal = new PagingResult<POSListingModel>();
            //if(vendorId>0)
            modal = _posManager.GetPOSPagedList(model, 0, LOGGEDIN_USER.UserID);
            List<string> resultString = new List<string>();
            resultString.Add(RenderRazorViewToString("Partials/_posListing", modal));
            resultString.Add(modal.TotalCount.ToString());
            return JsonResult(resultString);
        }

        public ActionResult GetPosBalance(long posId)
        {
            var balance = _posManager.GetPosBalance(posId);
            return Json(new { balance }, JsonRequestBehavior.AllowGet);

        }

    }
}