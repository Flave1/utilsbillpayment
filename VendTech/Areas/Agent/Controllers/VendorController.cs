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

namespace VendTech.Areas.Agent.Controllers
{
    /// <summary>
    /// Home Controller 
    /// Created On: 10/04/2015
    /// </summary>
    public class VendorController : BaseAgentController
    {
        #region Variable Declaration
        private readonly IVendorManager _vendorManager;
    
        #endregion

        public VendorController(IErrorLogManager errorLogManager,IVendorManager vendorManager)
            : base(errorLogManager)
        {
            _vendorManager = vendorManager;
          
        }

        /// <summary>
        /// Index View 
        /// </summary>
        /// <returns></returns>
        public ActionResult ManageAgentVendors()
        {
            // by using this way we can call required methods.
            var vendors = _vendorManager.GetVendorsPagedList(PagingModel.DefaultModel("CreatedAt", "Desc"),LOGGEDIN_USER.UserID);
            return View(vendors);
        }
        [AjaxOnly, HttpPost]
        public JsonResult GetVendorsPagingList(PagingModel model)
        {
            var modal = _vendorManager.GetVendorsPagedList(model,LOGGEDIN_USER.UserID);
            List<string> resultString = new List<string>();
            resultString.Add(RenderRazorViewToString("Partials/_vendorListing", modal));
            resultString.Add(modal.TotalCount.ToString());
            return JsonResult(resultString);
        }
        /// <summary>
        /// About Us Page
        /// </summary>
        /// <returns></returns>
        [Public]
        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        /// <summary>
        /// Contact Us Page
        /// </summary>
        /// <returns></returns>
        [Public]
        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        [HttpPost]
        public ActionResult AddEditVendor(SaveVendorModel model)
        {
            ViewBag.SelectedTab = SelectedAdminTab.Vendors;
            return JsonResult(_vendorManager.SaveVendor(model));
        }
        [HttpGet]
        public ActionResult AddEditVendor(long? id = null)
        {
            ViewBag.SelectedTab = SelectedAdminTab.Vendors;
            var model = new SaveVendorModel();
            model.AgencyId = LOGGEDIN_USER.UserID;
            if (id.HasValue && id > 0)
            {
                model = _vendorManager.GetVendorDetail(id.Value);
            }
            return View(model);
        }
    }
}