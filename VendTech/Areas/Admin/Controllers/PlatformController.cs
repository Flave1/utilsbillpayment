using DocumentFormat.OpenXml.Office2010.Excel;
using System.Web.Mvc;
using VendTech.Attributes;
using VendTech.BLL.Interfaces;
using VendTech.BLL.Models;

namespace VendTech.Areas.Admin.Controllers
{
    public class PlatformController : AdminBaseV2Controller
    {
        #region Variable Declaration
        private readonly IPlatformManager _platformManager;
        private readonly IEmailTemplateManager _templateManager;
        private readonly IPlatformApiManager _platformApiManager;
        #endregion

        public PlatformController(
            IPlatformManager platformManager, 
            IErrorLogManager errorLogManager, 
            IEmailTemplateManager templateManager,
            IPlatformApiManager platformApiManager)
            : base(errorLogManager)
        {
            _platformManager = platformManager;
            _templateManager = templateManager;
            _platformApiManager = platformApiManager;
        }

        public ActionResult ManagePlatforms()
        {
            ViewBag.SelectedTab = SelectedAdminTab.Platforms;
            ViewBag.PlatformTypes = PlatformModel.GetPlatformTypes();
            return View("ManagePlatformsV2", _platformManager.GetPlatforms());
        }

        [AjaxOnly, HttpGet]
        public JsonResult GetApiConnectionsForPlatform(int platformId)
        {
            return Json(_platformApiManager.GetPlatformApiConnectionsForPlatform(platformId), JsonRequestBehavior.AllowGet);
        }

        [AjaxOnly, HttpPost]
        public JsonResult DeletePlatform(int id)
        {
            return JsonResult(_platformManager.DeletePlatform(id));
        }
        [AjaxOnly, HttpPost]
        public JsonResult EnablePlatform(int id)
        {
            return JsonResult(_platformManager.ChangePlatformStatus(id, true));
        }
        [AjaxOnly, HttpPost]
        public JsonResult DisablePlatform(int id)
        {
            return JsonResult(_platformManager.ChangePlatformStatus(id, false));
        }
        [AjaxOnly, HttpPost]
        public JsonResult SavePlatform(SavePlatformModel model)
        {
            return JsonResult(_platformManager.SavePlateform(model));
        }

        [AjaxOnly, HttpPost]
        public JsonResult EnableThisPlatform(EnableThisPlatform model)
        {
            return JsonResult(_platformManager.EnableThisPlateform(model));
        }
    }
}