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
            var query = Context.Agencies.Where(p=>p.Status==(int)AgencyStatusEnum.Active).OrderBy(model.SortBy + " " + model.SortOrder);
            if (!string.IsNullOrEmpty(model.Search) && !string.IsNullOrEmpty(model.SearchField))
            {
                    //query = query.Where(z => z.AgencyName.ToLower().Contains(model.Search.ToLower()) || z.Company.ToLower().Contains(model.Search.ToLower()) || ((AgentTypeEnum)z.AgentType).ToString().ToLower().Contains(model.Search.ToLower()) || z.Commission.Percentage.ToString().Contains(model.Search));
                    if (model.SearchField.Equals("AGENCY"))
                        query = query.Where(z => z.AgencyName.ToLower().Contains(model.Search.ToLower()));
                    else if (model.SearchField.Equals("COMPANY"))
                        query = query.Where(z => z.Company.ToLower().Contains(model.Search.ToLower()));
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

        AddAgentModel IAgencyManager.GetAgentDetail(long agentId)
        {
            var agent = Context.Agencies.FirstOrDefault(p => p.AgencyId == agentId);
            if (agent == null)
                return null;
            return new AddAgentModel()
            {
                AgencyName = agent.AgencyName,
                AgentId = agent.AgencyId,
                AgentType = agent.AgentType,
                FirstName = agent.REPName,
                LastName = agent.REPLastName,
                //Password = Utilities.DecryptPassword(agent.Password),
                //ConfirmPassword = Utilities.DecryptPassword(agent.Password),
                Percentage = agent.CommissionPercentage,
                Phone = agent.Phone,
                Company = agent.Company,
                Email = agent.REPEmail
            };
        }
        ActionOutput IAgencyManager.AddAgent(AddAgentModel model)
        {
            var agent = new Agency();
            if (model.AgentId > 0)
            {
                agent = Context.Agencies.FirstOrDefault(p => p.AgencyId == model.AgentId);
                if (agent == null)
                    return ReturnError("Agent not exist");
                //var existngUser = Context.Agencies.Where(z => z.REPEmail.Trim().ToLower() == model.Email.Trim().ToLower() && z.AgencyId != model.AgentId).FirstOrDefault();
                //if (existngUser != null)
                //{
                //    return new ActionOutput
                //    {
                //        Status = ActionStatus.Error,
                //        Message = "This email-id already exists for another agent."
                //    };
                //}
            }
            else
            {
                //var existngUser = Context.Agencies.Where(z => z.REPEmail.Trim().ToLower() == model.Email.Trim().ToLower()).FirstOrDefault();
                //if (existngUser != null)
                //{
                //    return new ActionOutput
                //    {
                //        Status = ActionStatus.Error,
                //        Message = "This email-id already exists for another agent."
                //    };
                //}
            }
            agent.REPEmail = model.Email;
            agent.REPName = model.FirstName;
            agent.REPLastName = model.LastName;
            agent.Phone = model.Phone;
            agent.CountryCode = model.CountryCode;
            //agent.Password = Utilities.EncryptPassword(model.Password);
            agent.AgentType = 10;
            agent.Company = model.Company;
            agent.AgencyName = model.AgencyName;
            agent.CommissionPercentage = 1;
            agent.CreatedAt = DateTime.UtcNow;
            if (model.AgentId == 0)
            {
                agent.Status = (int)AgencyStatusEnum.Active;
                Context.Agencies.Add(agent);
            }
            Context.SaveChanges();
            return ReturnSuccess("Agent saved successfully.");
        }

        List<SelectListItem> IAgencyManager.GetAgentsSelectList()
        {
            return Context.Agencies.Where(p=>p.Status==(int)AgencyStatusEnum.Active).ToList().Select(p => new SelectListItem
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
