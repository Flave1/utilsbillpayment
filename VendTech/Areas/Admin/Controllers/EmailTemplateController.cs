using System.Collections.Generic;
using System.Web.Mvc;
using VendTech.Attributes;
using VendTech.BLL.Interfaces;
using VendTech.BLL.Models;

namespace VendTech.Areas.Admin.Controllers
{
    public class EmailTemplateController : AdminBaseController
    {
        #region Variable Declaration
        private readonly IUserManager _userManager;
        private readonly IEmailTemplateManager _templateManager;
        #endregion

        public EmailTemplateController(IUserManager userManager, IErrorLogManager errorLogManager, IEmailTemplateManager templateManager)
            : base(errorLogManager)
        {
            _userManager = userManager;
            _templateManager = templateManager;
        }

        #region Email Template Management

        /// <summary>
        /// This Will be used to get Template List
        /// </summary>
        /// <returns></returns>
        public ActionResult ManageTemplates()
        {
            ViewBag.SelectedTab = SelectedAdminTab.Platforms;
            var templates = _templateManager.GetEmailTemplateList(new PagingModel { SortBy = "sortOrder", SortOrder = "Asc" });
            return View(templates);
        }

        [AjaxOnly, HttpPost]
        public JsonResult GetTemplatesPagingList(PagingModel model)
        {
            ViewBag.SelectedTab = SelectedAdminTab.Platforms;
            PagingResult<TemplateViewModel> modal = _templateManager.GetEmailTemplateList(model);
            List<string> resultString = new List<string>();
            resultString.Add(RenderRazorViewToString("Partials/_templateListing", modal));
            resultString.Add(modal.TotalCount.ToString());
            return JsonResult(resultString);

        }

        [HttpGet]
        public ActionResult AddTemplate()
        {
            ViewBag.SelectedTab = SelectedAdminTab.Platforms;
            AddEditEmailTemplateModel model = new AddEditEmailTemplateModel();
            return View(model);
        }

        [HttpGet]
        public ActionResult EditTemplate(int templateId)
        {
            ViewBag.SelectedTab = SelectedAdminTab.Platforms;
            AddEditEmailTemplateModel model = _templateManager.GetEmailTemplateByTemplateId(templateId);
            if (model == null)
            {
                model = new AddEditEmailTemplateModel();
            }
            return View("AddTemplate", model);
        }
        [HttpPost, AjaxOnly]
        [ValidateInput(false)]
        public JsonResult AddUpdateTemplate(AddEditEmailTemplateModel model)
        {
            var result = _templateManager.AddUpdateEmailTemplate(model);
            return JsonResult(result);
        }
        #endregion
    }
}