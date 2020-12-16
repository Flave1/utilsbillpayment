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
    public class AgentController : AdminBaseController
    {
        #region Variable Declaration
        private readonly IAgencyManager _agencyManager;
        private readonly ICommissionManager _commissionManager;
        private readonly IEmailTemplateManager _templateManager;
        #endregion

        public AgentController(IAgencyManager agencyManager, IErrorLogManager errorLogManager, IEmailTemplateManager templateManager,ICommissionManager commissionManager)
            : base(errorLogManager)
        {
            _agencyManager = agencyManager;
            _templateManager = templateManager;
            _commissionManager = commissionManager;
        }

        #region User Management

        [HttpGet]
        public ActionResult ManageAgents()
        {
            ViewBag.SelectedTab = SelectedAdminTab.Agents;

            var users = _agencyManager.GetAgenciesPagedList(PagingModel.DefaultModel("CreatedAt","Desc"));
            return View(users);
        }

        [AjaxOnly, HttpPost]
        public JsonResult GetUsersPagingList(PagingModel model)
        {
            ViewBag.SelectedTab = SelectedAdminTab.Agents;
            var modal = _agencyManager.GetAgenciesPagedList(model);
            List<string> resultString = new List<string>();
            resultString.Add(RenderRazorViewToString("Partials/_agencyListing", modal));
            resultString.Add(modal.TotalCount.ToString());
            return JsonResult(resultString);
        }
        //[HttpGet]
        //public ActionResult AddAgent()
        //{
        //    var model = new SaveAgentModel();

        //    ViewBag.AgentTypes = Utilities.EnumToList(typeof(AgentTypeEnum));

        //    return View(model);
        //}

        [HttpPost]
        public ActionResult AddAgent(AddAgentModel model)
        {
            ViewBag.SelectedTab = SelectedAdminTab.Agents;
            return JsonResult(_agencyManager.AddAgent(model));
        }
        [HttpGet]
        public ActionResult AddAgent(long? id=null)
        {
            ViewBag.SelectedTab = SelectedAdminTab.Agents;
            var model = new AddAgentModel();
            var commissions = _commissionManager.GetCommissions();
            var drpCommissions = new List<SelectListItem>();
            foreach (var item in commissions)
            {
                drpCommissions.Add(new SelectListItem { Text = item.Value.ToString(), Value = item.CommissionId.ToString() });
            }
            ViewBag.AgentTypes = Utilities.EnumToList(typeof(AgentTypeEnum));
            ViewBag.commissions = drpCommissions;
            if (id.HasValue && id > 0)
            {
                model = _agencyManager.GetAgentDetail(id.Value);
            }
            return View(model);
        }

        //[HttpPost]
        //public ActionResult AddAgent(SaveAgentModel model)
        //{
        //    return JsonResult(_agencyManager.UpdateAgent(model));
        //}
        //Get Agent Percentage with vendorid
        public ActionResult GetAgentPercentage(long vendorId)
        {
            return Json(new { percentage = _agencyManager.GetAgentPercentage(vendorId) }, JsonRequestBehavior.AllowGet);
        }

        [AjaxOnly, HttpPost]
        public JsonResult DeleteAgent(long agentId)
        {
            ViewBag.SelectedTab = SelectedAdminTab.Agents;
            return JsonResult(_agencyManager.DeleteAgency(agentId));
        }
        #endregion
    }
}