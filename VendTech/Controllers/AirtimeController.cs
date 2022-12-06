using System.Web.Mvc;
using VendTech.Areas.Admin.Controllers;
using VendTech.BLL.Interfaces;

namespace VendTech.Controllers
{
    public class AirtimeController: AppUserBaseController
    {
        public AirtimeController( IErrorLogManager errorLogManager): base(errorLogManager)
        {
        }

        public ActionResult Recharge()
        {
            return View();
        }
    }
}