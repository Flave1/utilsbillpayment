using System.Collections.Generic;
using System.Web.Mvc;
using VendTech.Attributes;
using VendTech.BLL.Interfaces;
using VendTech.BLL.Models;

namespace VendTech.Areas.Admin.Controllers
{
    public class PaymentTypeController : AdminBaseController
    {
        #region Variable Declaration
        private readonly IPaymentTypeManager _PaymentTypeManager;
        private readonly IEmailTemplateManager _templateManager;
        #endregion

        public PaymentTypeController(IPaymentTypeManager PaymentTypeManager, IErrorLogManager errorLogManager, IEmailTemplateManager templateManager)
            : base(errorLogManager)
        {
            _PaymentTypeManager = PaymentTypeManager;
            _templateManager = templateManager;
        }

        public ActionResult Index()
        {

            ViewBag.SelectedTab = SelectedAdminTab.Platforms; 
            return View(_PaymentTypeManager.GetPaymentTypes());
        }

        [AjaxOnly, HttpPost]
        public JsonResult GetPaymentTypes(PagingModel model)
        {
            ViewBag.SelectedTab = SelectedAdminTab.Agents;
            var modal = _PaymentTypeManager.GetPagedList(model);
            List<string> resultString = new List<string>();
            resultString.Add(RenderRazorViewToString("Partials/_paymentTypeListing", modal));
            resultString.Add(modal.TotalCount.ToString());
            return JsonResult(resultString);
        }

        [AjaxOnly, HttpPost]
        public JsonResult SavePaymentType(PaymentTypeModel model)
        {
            return JsonResult(_PaymentTypeManager.SavePaymentType(model));
        }
        [AjaxOnly, HttpPost]
        public JsonResult DeletePaymentType(int id)
        {
            return JsonResult(_PaymentTypeManager.Delete(id));
        }

        [AjaxOnly, HttpPost]
        public JsonResult DeactivatePaymentType(int id)
        {
            return JsonResult(_PaymentTypeManager.Deactivate(id));
        }

        [AjaxOnly, HttpPost]
        public JsonResult ActivatePaymentType(int id)
        {
            return JsonResult(_PaymentTypeManager.Activate(id));
        }

      

        public ActionResult AddEditPaymentType(int id = 0)
        {
            ViewBag.SelectedTab = SelectedAdminTab.Platforms;
            if(id > 0)
            {
                ViewBag.Title = "Edit Payment Type";
                var pt = _PaymentTypeManager.GetPaymentTypeDetail(id);
                return View(pt);
            }
            ViewBag.Title = "Add Payment Type";
            return View(new PaymentTypeModel());
        }
    }
}