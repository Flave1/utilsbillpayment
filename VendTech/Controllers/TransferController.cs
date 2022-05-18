using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Web.Http.Description;
using System.Web.Mvc;
using VendTech.Areas.Admin.Controllers;
using VendTech.Attributes;
using VendTech.BLL.Common;
using VendTech.BLL.Interfaces;
using VendTech.BLL.Managers;
using VendTech.BLL.Models;
using VendTech.DAL;
using VendTech.Framework.Api;

namespace VendTech.Controllers
{
    public class TransferController : AppUserBaseController
    {
        #region Variable Declaration
        private readonly ITransferManager _transferManager;
        #endregion

        public TransferController(IErrorLogManager errorLogManager, ITransferManager transferManager)
            : base(errorLogManager)
        {
            _transferManager = transferManager;
        }

        #region User Management

        public ActionResult Index()
        {
            ViewBag.SelectedTab = SelectedAdminTab.Transfer;

            var vendorList = _transferManager.GetAllAgencyAdminVendor(PagingModel.DefaultModel("User.Agency.AgencyName", "Desc"), LOGGEDIN_USER.AgencyId);
            return View(new PagingResult<AgentListingModel>());

        }

        [AjaxOnly, HttpPost, Public]
        public JsonResult GetAllAgencyAdminVendor(FetchItemsModel request)
        {
            var vendorList = _transferManager.GetAllAgencyAdminVendor(PagingModel.DefaultModel("User.Agency.AgencyName", "Desc"), LOGGEDIN_USER.AgencyId);
            return Json(new { result = JsonConvert.SerializeObject(vendorList.List) });
        }
    #endregion

    }


}
