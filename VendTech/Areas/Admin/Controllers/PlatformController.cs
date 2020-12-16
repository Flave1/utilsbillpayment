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
    public class PlatformController : AdminBaseController
    {
        #region Variable Declaration
        private readonly IPlatformManager _platformManager;
        private readonly IEmailTemplateManager _templateManager;
        #endregion

        public PlatformController(IPlatformManager platformManager, IErrorLogManager errorLogManager, IEmailTemplateManager templateManager)
            : base(errorLogManager)
        {
            _platformManager = platformManager;
            _templateManager = templateManager;
        }

        public ActionResult ManagePlatforms()
        {
            ViewBag.SelectedTab = SelectedAdminTab.Platforms;
            return View(_platformManager.GetPlatforms());
        }
        [AjaxOnly, HttpPost]
        public JsonResult DeletePlatform(int id)
        {
            return JsonResult(_platformManager.DeletePlatform(id));
        }
        [AjaxOnly, HttpPost]
        public JsonResult EnablePlatform(int id)
        {
            return JsonResult(_platformManager.ChangePlatformStatus(id,true));
        }
        [AjaxOnly, HttpPost]
        public JsonResult DisablePlatform(int id)
        {
            return JsonResult(_platformManager.ChangePlatformStatus(id,false));
        }
        [AjaxOnly, HttpPost]
        public JsonResult SavePlatform(SavePlatformModel model)
        {
            return JsonResult(_platformManager.SavePlateform(model));
        }
    }
}