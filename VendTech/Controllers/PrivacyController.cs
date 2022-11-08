using System.Web.Mvc;
using VendTech.BLL.Interfaces;
using VendTech.BLL.Models;

namespace VendTech.Controllers
{
    public class PrivacyController: Controller
    {

        private readonly ICMSManager _cmsManager;
        public PrivacyController(ICMSManager cmsManager)
        {
            _cmsManager = cmsManager;
        }
        public ActionResult Policy()
        {
            CMSPageViewModel model = _cmsManager.GetPageContentByPageIdforFront(2);
            return View(model);
        }
    }
}