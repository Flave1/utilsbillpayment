using VendTech.BLL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace VendTech.BLL.Interfaces
{
    public interface IAgencyManager
    {
        PagingResult<AgencyListingModel> GetAgenciesPagedList(PagingModel model);
        PagingResult<AgentListingModel> GetAgentsPagedList(PagingModel model, long agency);
        ActionOutput AddAgent(SaveAgentModel model);
        AddAgentModel GetAgentDetail(long agentId);
        List<SelectListItem> GetAgentsSelectList();
        decimal GetAgentPercentage(long vendorId);
        ActionOutput DeleteAgency(long agentId);
    }
    
}
