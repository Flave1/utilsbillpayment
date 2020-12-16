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
    public class RoleController : AdminBaseController
    {
        #region Variable Declaration
        private readonly IRoleManager _roleManager;
        private readonly IEmailTemplateManager _templateManager;
        #endregion

        public RoleController(IRoleManager roleManager, IErrorLogManager errorLogManager, IEmailTemplateManager templateManager)
            : base(errorLogManager)
        {
            _roleManager = roleManager;
            _templateManager = templateManager;
        }

        public ActionResult ManageRoles()
        {

            ViewBag.SelectedTab = SelectedAdminTab.Platforms;

            return View(_roleManager.GetRoles());
        }
       
        [AjaxOnly, HttpPost]
        public JsonResult SaveRole(SaveRoleModel model)
        {
            return JsonResult(_roleManager.SaveRole(model));
        }
        [AjaxOnly, HttpPost]
        public JsonResult DeleteRole(int id)
        {
            return JsonResult(_roleManager.DeleteRole(id));
        }
    }
}