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
using VendTech.BLL.Common;
#endregion

namespace VendTech.Areas.Agent.Controllers
{
    /// <summary>
    /// Home Controller 
    /// Created On: 10/04/2015
    /// </summary>
    public class POSController : BaseAgentController
    {
        #region Variable Declaration
        private readonly IPOSManager _posManager;
        private readonly IEmailTemplateManager _templateManager;
        private readonly IVendorManager _vendorManager;

        #endregion

        public POSController(IPOSManager posManager, IErrorLogManager errorLogManager, IEmailTemplateManager templateManager, IVendorManager vendorManager)
            : base(errorLogManager)
        {
            _posManager = posManager;
            _templateManager = templateManager;
            _vendorManager = vendorManager;

        }

        #region User Management

        [HttpGet]
        public ActionResult ManagePOS()
        {
            var users = _posManager.GetPOSPagedList(PagingModel.DefaultModel("CreatedAt", "Desc"),LOGGEDIN_USER.UserID);
            return View(users);
        }

        [AjaxOnly, HttpPost]
        public JsonResult GetUsersPagingList(PagingModel model)
        {
            var modal = _posManager.GetPOSPagedList(model,LOGGEDIN_USER.UserID);
            List<string> resultString = new List<string>();
            resultString.Add(RenderRazorViewToString("Partials/_posListing", modal));
            resultString.Add(modal.TotalCount.ToString());
            return JsonResult(resultString);
        }

        [HttpPost]
        public ActionResult AddEditPos(SavePosModel model)
        {
            ViewBag.SelectedTab = SelectedAdminTab.POS;
            return JsonResult(_posManager.SavePos(model));
        }
        [HttpGet]
        public ActionResult AddEditPos(long? id = null)
        {
            ViewBag.SelectedTab = SelectedAdminTab.POS;
            var model = new SavePosModel();

            ViewBag.PosTypes = Utilities.EnumToList(typeof(PosTypeEnum));
            ViewBag.Vendors = _vendorManager.GetVendorsSelectList(LOGGEDIN_USER.UserID);
            if (id.HasValue && id > 0)
            {
                model = _posManager.GetPosDetail(id.Value);
            }
            return View(model);
        }
        #endregion
     
    }
}