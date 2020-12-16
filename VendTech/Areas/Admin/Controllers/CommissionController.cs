using VendTech.Attributes;
using VendTech.BLL.Interfaces;
using VendTech.BLL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using VendTech.BLL.Common;

namespace VendTech.Areas.Admin.Controllers
{
    public class CommissionController : AdminBaseController
    {
        #region Variable Declaration
        private readonly ICommissionManager _commissionManager;
        private readonly IEmailTemplateManager _templateManager;
        #endregion

        public CommissionController(ICommissionManager commissionManager, IErrorLogManager errorLogManager, IEmailTemplateManager templateManager)
            : base(errorLogManager)
        {
            _commissionManager = commissionManager;
            _templateManager = templateManager;
        }

        public ActionResult ManageCommissions()
        {
            ViewBag.SelectedTab = SelectedAdminTab.Platforms;
            return View(_commissionManager.GetCommissions());
        }
        [AjaxOnly, HttpPost]
        public JsonResult DeleteCommission(int id)
        {
            return JsonResult(_commissionManager.DeleteCommission(id));
        }
        [AjaxOnly, HttpPost]
        public JsonResult SaveCommission(SaveCommissionModel model)
        {
            return JsonResult(_commissionManager.SaveCommission(model));
        }
    }
}