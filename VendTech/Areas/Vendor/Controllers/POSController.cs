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
#endregion

namespace VendTech.Areas.Vendor.Controllers
{
    /// <summary>
    /// Home Controller 
    /// Created On: 10/04/2015
    /// </summary>
    public class POSController : BaseVendorController
    {
        #region Variable Declaration
        private readonly IPOSManager _posManager;
        private readonly IEmailTemplateManager _templateManager;
        #endregion

        public POSController(IPOSManager posManager, IErrorLogManager errorLogManager, IEmailTemplateManager templateManager)
            : base(errorLogManager)
        {
            _posManager = posManager;
            _templateManager = templateManager;
        }

        #region User Management

        [HttpGet]
        public ActionResult ManageVendorPOS()
        {
            var users = _posManager.GetPOSPagedList(PagingModel.DefaultModel("CreatedAt", "Desc"),vendorId:LOGGEDIN_USER.UserID);
            return View(users);
        }

        [AjaxOnly, HttpPost]
        public JsonResult GetUsersPagingList(PagingModel model)
        {
            var modal = _posManager.GetPOSPagedList(model, vendorId: LOGGEDIN_USER.UserID);
            List<string> resultString = new List<string>();
            resultString.Add(RenderRazorViewToString("Partials/_posListing", modal));
            resultString.Add(modal.TotalCount.ToString());
            return JsonResult(resultString);
        }
        #endregion
     
    }
}