using VendTech.BLL.Interfaces;
using VendTech.BLL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Dynamic;
using VendTech.DAL;
using VendTech.BLL.Common;
using System.Web;
using System.IO;
using System.Web.Mvc;

namespace VendTech.BLL.Managers
{
    public class AgencyManager : BaseManager, IAgencyManager
    {

        PagingResult<AgencyListingModel> IAgencyManager.GetAgenciesPagedList(PagingModel model)
        {
            var result = new PagingResult<AgencyListingModel>();
            var query = Context.Agencies.Where(p => p.Status == (int)AgencyStatusEnum.Active).OrderBy(model.SortBy + " " + model.SortOrder);
            if (!string.IsNullOrEmpty(model.Search) && !string.IsNullOrEmpty(model.SearchField))
            {
                //query = query.Where(z => z.AgencyName.ToLower().Contains(model.Search.ToLower()) || z.Company.ToLower().Contains(model.Search.ToLower()) || ((AgentTypeEnum)z.AgentType).ToString().ToLower().Contains(model.Search.ToLower()) || z.Commission.Percentage.ToString().Contains(model.Search));
                if (model.SearchField.Equals("AGENCY"))
                    query = query.Where(z => z.AgencyName.ToLower().Contains(model.Search.ToLower()));
                //else if (model.SearchField.Equals("COMPANY"))
                //    query = query.Where(z => z.Company.ToLower().Contains(model.Search.ToLower()));
                //else if (model.SearchField.Equals("AGENT"))
                //    query = query.Where(z => ((AgentTypeEnum)z.AgentType).ToString().ToLower().Equals(model.Search.ToLower()));
                //else if (model.SearchField.Equals("%"))
                //    query = query.Where(z=>z.Commission.Percentage.ToString().Contains(model.Search));
            }
            var list = query
               .Skip(model.PageNo - 1).Take(model.RecordsPerPage)
               .ToList().Select(x => new AgencyListingModel(x)).ToList();
            result.List = list;
            result.Status = ActionStatus.Successfull;
            result.Message = "Agency List";
            result.TotalCount = query.Count();
            return result;
        }

        PagingResult<AgentListingModel> IAgencyManager.GetAgentsPagedList(PagingModel model, long agency)
        {
            var result = new PagingResult<AgentListingModel>();
            model.RecordsPerPage = 10000000;
            IQueryable<POS> query = null;

            query = Context.POS.Where(f => f.IsDeleted == false && f.User.AgentId == agency).OrderBy(model.SortBy + " " + model.SortOrder);
            //f.age == agent && 

            if (!string.IsNullOrEmpty(model.Search) && !string.IsNullOrEmpty(model.SearchField))
            {
                if (model.SearchField.Equals("AGENCY"))
                    query = query.Where(z => z.User.Agency.AgencyName.ToLower().Contains(model.Search.ToLower()));
                else if (model.SearchField.Equals("COMPANY"))
                    query = query.Where(z => z.User.CompanyName.ToLower().Contains(model.Search.ToLower()));
            }
            var list = query
               .Skip(model.PageNo - 1).Take(model.RecordsPerPage)
               .ToList().Select(x => new AgentListingModel(x)).ToList();
            result.List = list;
            result.Status = ActionStatus.Successfull;
            result.Message = "User List";
            result.TotalCount = query.Count();
            return result;
        }

        AddAgentModel IAgencyManager.GetAgentDetail(long agentId)
        {
            var agent = Context.Agencies.FirstOrDefault(p => p.AgencyId == agentId);
            if (agent == null)
                return null;
            return new AddAgentModel()
            {
                AgencyName = agent.AgencyName,
                AgencyId = agent.AgencyId,
                AgentType = agent.AgentType,
                Percentage = (int)agent.CommissionId, 
                Representative = agent.Representative
            };
        }
        ActionOutput IAgencyManager.AddAgent(SaveAgentModel model)
        {
            var agent = new Agency();
            if (model.AgencyId > 0)
            {
                agent = Context.Agencies.FirstOrDefault(p => p.AgencyId == model.AgencyId);
                if (agent == null)
                    return ReturnError("Agent not exist");
            }
            agent.AgentType = 10; 
            agent.AgencyName = model.AgencyName;
            agent.CommissionId = model.Percentage;
            agent.CreatedAt = DateTime.UtcNow;
            agent.Representative = model.Representative;
            if (model.AgencyId == 0)
            {
                agent.Status = (int)AgencyStatusEnum.Active;
                Context.Agencies.Add(agent);
            }
            Context.SaveChanges();
            return ReturnSuccess("Agent saved successfully.");
        }

        List<SelectListItem> IAgencyManager.GetAgentsSelectList()
        {
            return Context.Agencies.Where(p => p.Status == (int)AgencyStatusEnum.Active).ToList().Select(p => new SelectListItem
            {
                Text = p.AgencyName,
                Value = p.AgencyId.ToString()
            }).ToList();
        }
        decimal IAgencyManager.GetAgentPercentage(long vendorId)
        {
            var vendor = Context.Vendors.FirstOrDefault(p => p.VendorId == vendorId);
            if (vendor == null)
                return 0;
            return vendor.Agency.Commission.Percentage;
        }
        ActionOutput IAgencyManager.DeleteAgency(long agentId)
        {
            var agent = Context.Agencies.FirstOrDefault(p => p.AgencyId == agentId);
            if (agent == null)
                return ReturnError("Agency not exist.");
            agent.Status = (int)AgencyStatusEnum.Deleted;
            Context.SaveChanges();
            return ReturnSuccess("Agency deleted successfully.");
        }
    }


}
