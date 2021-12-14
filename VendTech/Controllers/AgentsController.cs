using System.Collections.Generic;
using System.Web.Mvc;
using VendTech.Areas.Admin.Controllers;
using VendTech.Attributes;
using VendTech.BLL.Common;
using VendTech.BLL.Interfaces;
using VendTech.BLL.Models;

namespace VendTech.Controllers
{
    public class AgentsController : AppUserBaseController
    {
        #region Variable Declaration
        private readonly IAgencyManager _agencyManager;
        private readonly ICommissionManager _commissionManager;
        private readonly IEmailTemplateManager _templateManager;
        #endregion

        public AgentsController(IAgencyManager agencyManager, IErrorLogManager errorLogManager, IEmailTemplateManager templateManager, ICommissionManager commissionManager)
            : base(errorLogManager)
        {
            _agencyManager = agencyManager;
            _templateManager = templateManager;
            _commissionManager = commissionManager;
        }

        #region User Management
         
        public ActionResult ManageAgents()
        {
            ViewBag.SelectedTab = SelectedAdminTab.Agents;
            if(LOGGEDIN_USER.AgencyId > 0)
            {
                var users = _agencyManager.GetAgentsPagedList(PagingModel.DefaultModel("User.Agency.AgencyName", "Desc"), LOGGEDIN_USER.AgencyId);
                return View(users);
            }
            return View(new PagingResult<AgentListingModel>());
        }

        [AjaxOnly, HttpPost]
        public JsonResult GetAgentsPagedList(PagingModel model)
        {
            ViewBag.SelectedTab = SelectedAdminTab.Agents;
            var modal = _agencyManager.GetAgentsPagedList(model, LOGGEDIN_USER.AgencyId);
            List<string> resultString = new List<string>();
            resultString.Add(RenderRazorViewToString("Partials/_agencyListing", modal));
            resultString.Add(modal.TotalCount.ToString());
            return JsonResult(resultString);
        }

        [HttpPost]
        public ActionResult AddAgent(SaveAgentModel model)
        {
            ViewBag.SelectedTab = SelectedAdminTab.Agents;
            return JsonResult(_agencyManager.AddAgent(model));
        }
        [HttpGet]
        public ActionResult AddAgent(long? id = null)
        {
            ViewBag.SelectedTab = SelectedAdminTab.Agents;
            var model = new SaveAgentModel();
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