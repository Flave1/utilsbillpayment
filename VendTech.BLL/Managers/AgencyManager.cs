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
                if (model.SearchField.Equals("AGENCY"))
                    query = query.Where(z => z.AgencyName.ToLower().Contains(model.Search.ToLower()));
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

            query = Context.POS.Where(f => f.IsDeleted == false && f.User.AgentId == agency && !f.IsAdmin && !f.SerialNumber.StartsWith("AGT")).OrderBy("User.Agency.AgencyName" + " " + model.SortOrder);

            if (model.SortBy.Equals("AGENCY"))
            {
                query = query.Where(f => f.IsDeleted == false && f.User.AgentId == agency).OrderBy("User.Agency.AgencyName" + " " + model.SortOrder);
            }
            if (model.SortBy.Equals("VENDOR"))
            {
                query = query.Where(f => f.IsDeleted == false && f.User.AgentId == agency).OrderBy("User.Name" + " " + model.SortOrder);
            }
            if (model.SortBy.Equals("POSID"))
            {
                query = query.Where(f => f.IsDeleted == false && f.User.AgentId == agency).OrderBy("SerialNumber" + " " + model.SortOrder);
            }
            if (model.SortBy.Equals("PHONE"))
            {
                query = query.Where(f => f.IsDeleted == false && f.User.AgentId == agency).OrderBy("User.Phone" + " " + model.SortOrder);
            }
            if (model.SortBy.Equals("AGENT"))
            {
                query = query.Where(f => f.IsDeleted == false && f.User.AgentId == agency).OrderBy("User.Name" + " " + model.SortOrder);
            }
            if (model.SortBy.Equals("ENABLED"))
            {
                query = query.Where(f => f.IsDeleted == false && f.User.AgentId == agency).OrderBy("Enabled" + " " + model.SortOrder);
            }
            if (model.SortBy.Equals("BALANCE"))
            {
                query = query.Where(f => f.IsDeleted == false && f.User.AgentId == agency).OrderBy("Balance" + " " + model.SortOrder);
            }
            //f.age == agent && 

            if (!string.IsNullOrEmpty(model.Search) && !string.IsNullOrEmpty(model.SearchField))
            {
               
                 if (model.SearchField.Equals("AGENT"))
                    query = query.Where(z => z.User.Name.ToLower().Contains(model.Search.ToLower()));
                else if (model.SearchField.Equals("POSID"))
                    query = query.Where(z => z.SerialNumber.ToLower().Contains(model.Search.ToLower()));
                else if (model.SearchField.Equals("CELL"))
                    query = query.Where(z => z.User.Phone.ToLower().Contains(model.Search.ToLower())); 
                else if (model.SearchField.Equals("ENABLED"))
                {
                    var ena = Convert.ToBoolean(model.Search.ToLower());
                    query = query.Where(z => z.Enabled == ena);

                }
                else if (model.SearchField.Equals("BALANCE"))
                    query = query.Where(z => z.Balance.ToString().ToLower().Contains(model.Search.Replace(",", "").ToLower()));
            }
            var list = query
               .Skip(model.PageNo - 1).Take(model.RecordsPerPage)
               .ToList().Select(x => new AgentListingModel(x)).ToList();

            if (!string.IsNullOrEmpty(model.Search) && !string.IsNullOrEmpty(model.SearchField))
            {
                if (model.SearchField.Equals("TODAY'S"))
                    list = list.Where(z => z.TodaySales.Contains(model.Search.ToLower())).ToList();
                else if (model.SearchField.Equals("VENDOR"))
                    list = list.Where(z => z.Vendor.ToLower().Contains(model.Search.ToLower())).ToList();
            }
          
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
            var ag = new AddAgentModel()
            {
                AgencyName = agent.AgencyName,
                AgencyId = agent.AgencyId,
                AgentType = agent.AgentType,
                Percentage = agent?.CommissionId ?? 0,
                Representative = agent?.Representative
            };

            return ag;
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

            if(model.Representative > 0)
            {
                var repUserAccount = Context.Users.FirstOrDefault(we => we.UserId == model.Representative);
                if(repUserAccount != null)
                {
                    repUserAccount.UserType = 9; // AGENCY ADMIN
                }
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
            return Context.Agencies.Where(p => p.Status == (int)AgencyStatusEnum.Active && p.AgencyId != 20).ToList().Select(p => new SelectListItem
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
