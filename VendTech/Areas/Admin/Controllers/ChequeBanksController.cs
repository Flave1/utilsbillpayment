using System.Web.Mvc;
using VendTech.Attributes;
using VendTech.BLL.Interfaces;
using VendTech.BLL.Models;

namespace VendTech.Areas.Admin.Controllers
{
    public class ChequeBanksController : AdminBaseController
    {
        #region Variable Declaration
        private readonly IBankAccountManager _ChequeBanksManager;
        #endregion

        public ChequeBanksController(IBankAccountManager ChequeBanksManager, IErrorLogManager errorLogManager, IEmailTemplateManager templateManager, IVendorManager vendorManager)
            : base(errorLogManager)
        {
            _ChequeBanksManager = ChequeBanksManager;
        }


        [HttpGet]
        public ActionResult ManageChequeBanks()
        {
            ViewBag.SelectedTab = SelectedAdminTab.Platforms;
            var ChequeBankss = _ChequeBanksManager.GetChequeBanks();
            return View(ChequeBankss);
        }

        [HttpGet]
        public ActionResult EditChequeBanks(long id)
        {
            ViewBag.SelectedTab = SelectedAdminTab.Platforms;
            var ChequeBanks = _ChequeBanksManager.GetChequeBankDetail(id);
            return View(ChequeBanks);
        }
        public ActionResult AddChequeBanks()
        {
            ViewBag.SelectedTab = SelectedAdminTab.Platforms;
            return View(new ChequeBankModel());
        }

        [AjaxOnly, HttpPost]
        public JsonResult UpdateChequeBanksDetails(ChequeBankModel model)
        {
            return JsonResult(_ChequeBanksManager.SaveChequeBank(model));
        }
        [AjaxOnly, HttpPost]
        public JsonResult AddChequeBanksDetails(ChequeBankModel model)
        {
            return JsonResult(_ChequeBanksManager.SaveChequeBank(model));
        }
        [AjaxOnly, HttpPost]
        public JsonResult Delete(int id)
        {
            return JsonResult(_ChequeBanksManager.DeleteChequeBank(id));
        }
    }
}