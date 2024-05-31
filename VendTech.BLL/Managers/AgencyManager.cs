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
using System.Data.Entity;

namespace VendTech.BLL.Managers
{
    public class AgencyManager : BaseManager, IAgencyManager
    {

        PagingResult<AgencyListingModel> IAgencyManager.GetAgenciesPagedList(PagingModel model)
        {
            if(model.RecordsPerPage != 10)
            {
                model.RecordsPerPage = model.RecordsPerPage;
            }
            else
            {
                model.RecordsPerPage = 10000000;
            }
            var result = new PagingResult<AgencyListingModel>();
            var query = Context.Agencies.Where(p => p.Status == (int)AgencyStatusEnum.Active).OrderBy("User.Agency.AgencyName" + " " + model.SortOrder).Take(3);
            if (!string.IsNullOrEmpty(model.Search) && !string.IsNullOrEmpty(model.SearchField))
            {
                if (model.SearchField.Equals("AGENCY"))
                    query = query.Where(z => z.AgencyName.ToLower().Trim().Contains(model.Search.ToLower().Trim()));
                if (model.SearchField.Equals("ADMIN"))
                    query = query.Where(z => z.User.Name.ToLower().Trim().Contains(model.Search.ToLower().Trim()) || z.User.SurName.ToLower().Trim().Contains(model.Search.ToLower().Trim()));
                if (model.SearchField.Equals("COMMISSION"))
                    query = query.Where(z => z.Commission.Percentage.ToString().ToLower().Trim().Contains(model.Search.ToLower().Trim()));
                if (model.SearchField.Equals("POS"))
                    query = query.Where(z => z.User.POS.FirstOrDefault().SerialNumber.ToLower().Trim().Contains(model.Search.ToLower().Trim()));
                if (model.SearchField.Equals("BALANCE"))
                    query = query.Where(z => z.User.POS.FirstOrDefault().Balance.ToString().ToLower().Trim().Contains(model.Search.ToLower().Trim()));
            }
            var list = query
               .Skip(model.PageNo - 1).Take(model.RecordsPerPage)
               .ToList().Select(x => new AgencyListingModel(x)).OrderBy(x => x.SerialNumber).ToList();
            result.List = list;
            result.Status = ActionStatus.Successfull;
            result.Message = "Agency List";
            result.TotalCount = query.Count();
            return result;
        }

        PagingResult<AgentListingModel> IAgencyManager.GetAgentsPagedList(PagingModel model, long agency)
        {
            var result = new PagingResult<AgentListingModel>();
            if (model.RecordsPerPage != 10)
            {
                model.RecordsPerPage = model.RecordsPerPage;
            }
            else
            {
                if(model.RecordsPerPage != 10)
                {
                    model.RecordsPerPage = 10000000;
                }
            }
            IQueryable<POS> query = null;

            query = Context.POS.Where(f => f.IsDeleted == false && f.User.AgentId == agency && !f.IsAdmin && !f.SerialNumber.StartsWith("AGT") && f.User.Status != 3).OrderBy("User.Agency.AgencyName" + " " + model.SortOrder);

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
               .Skip(model.PageNo - 1).Take(model.RecordsPerPage).AsEnumerable()
               .Select(x => new AgentListingModel(x)).ToList();

            for(var i =  0; i < list.Count; i++)
            {
                var posId = list[i].POSID;
                var currentDate = DateTime.UtcNow.Date;

                var sale = Context.TransactionDetails
                    .Where(f => f.POSId == posId && DbFunctions.TruncateTime(f.CreatedAt) == DbFunctions.TruncateTime(currentDate) && f.Finalised == true)
                    .Select(d => d.Amount)
                    .DefaultIfEmpty(0)
                    .Sum();

                list[i].TodaySales = Utilities.FormatAmount(sale);
            }

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
                Representative = agent?.Representative,
                POSId = agent.User.POS.FirstOrDefault()?.POSId ?? 0,
                SerialNumber = agent.User.POS.FirstOrDefault()?.SerialNumber ?? ""
            };

            return ag;
        }
        ActionOutput IAgencyManager.AddAgent(SaveAgentModel model)
        {
            var isAgentCreated = false;
            var agent = new Agency();
            try
            {
                if (model.AgencyId > 0)
                {
                    agent = Context.Agencies.FirstOrDefault(p => p.AgencyId == model.AgencyId);
                    if (agent == null)
                        return ReturnError("Agent not exist");
                }

                if (model.Representative > 0)
                {
                    var repUserAccount = Context.Users.FirstOrDefault(we => we.UserId == model.Representative);
                    if (repUserAccount != null)
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
                isAgentCreated = true;
                RemoveORAddUserPermissions(model.Representative.Value, model);
                RemoveOrAddUserWidgets(model.Representative.Value, model);
                var agencyPosResult = SaveAgencyPos(model.SerialNumber, model.POSId, model.Representative, model.Percentage);
                if(agencyPosResult.Status != ActionStatus.Successfull)
                {
                    throw new ArgumentException(agencyPosResult.Message);
                }
                return ReturnSuccess("AGENT SAVED SUCCESSFULLY.");
            }
            catch (ArgumentException ex)
            {
                if (isAgentCreated && model.AgencyId == 0)
                {
                    var createdAgent = Context.Agencies.Find(agent.AgencyId);
                    if (createdAgent != null)
                        Context.Agencies.Remove(createdAgent);
                    Context.SaveChanges();
                }
                return ReturnError(ex.Message);
            }
        }

        private ActionOutput SaveAgencyPos(string SerialNumber, long POSId, long? userId, int commission)
        {
            var dbPos = new POS();
            if (POSId > 0)
            {
                dbPos = Context.POS.FirstOrDefault(p => p.POSId == POSId);
                if (dbPos == null)
                    return ReturnError("POS NOT FOUND");
            }
            else
            {
                if (Context.POS.Any(d => d.SerialNumber.Contains(SerialNumber)))
                {
                    return ReturnError("POS ALREADY EXIST");
                }
            }
            dbPos.SerialNumber = SerialNumber;
            dbPos.VendorId = userId;
            dbPos.VendorType = 0;
            dbPos.Phone = "N/A";
            dbPos.Enabled = true;
            dbPos.SMSNotificationDeposit = false;
            dbPos.SMSNotificationSales = false;
            dbPos.EmailNotificationSales = false;
            dbPos.EmailNotificationDeposit = false;
            //dbPos.CountryCode = "N/A";
            dbPos.CreatedAt = DateTime.UtcNow;
            dbPos.CommissionPercentage = commission;
            dbPos.IsDeleted = false;
            dbPos.WebSms = false;
            dbPos.PosSms = false;
            dbPos.PosPrint = false;
            dbPos.WebPrint = false;
            dbPos.WebBarcode = false;
            dbPos.PosBarcode = false;

            if (POSId == 0)
                Context.POS.Add(dbPos);
            Context.SaveChanges();


            return ReturnSuccess("POS SAVED SUCCESSFULLY.");
        }

        bool RemoveORAddUserPermissions(long userId, SaveAgentModel model)
        {
            var existingpermissons = Context.UserAssignedModules.Where(x => x.UserId == userId && x.IsAddedFromAgency == true).ToList();
            if (existingpermissons.Count() > 0)
            {
                Context.UserAssignedModules.RemoveRange(existingpermissons);
                Context.SaveChanges();
            }
            List<UserAssignedModule> newpermissos = new List<UserAssignedModule>();
            if (model.SelectedModules != null)
            {
                model.SelectedModules.ToList().ForEach(c =>
                 newpermissos.Add(new UserAssignedModule()
                 {
                     UserId = userId,
                     ModuleId = c,
                     CreatedAt = DateTime.UtcNow,
                     IsAddedFromAgency = true,
                 }));
                Context.UserAssignedModules.AddRange(newpermissos);
                Context.SaveChanges();
            }
            return true;
        }

        bool RemoveOrAddUserWidgets(long userId, SaveAgentModel model)
        {
            //Deleting Exisiting Widgets
            var existing_widgets = Context.UserAssignedWidgets.Where(x => x.UserId == userId && x.IsAddedFromAgency == true).ToList();
            if (existing_widgets.Count() > 0)
            {
                Context.UserAssignedWidgets.RemoveRange(existing_widgets);
                Context.SaveChanges();
            }

            List<UserAssignedWidget> newwidgets = new List<UserAssignedWidget>();
            if (model.SelectedWidgets != null)
            {
                model.SelectedWidgets.ToList().ForEach(c =>
                 newwidgets.Add(new UserAssignedWidget()
                 {
                     UserId = userId,
                     WidgetId = c,
                     CreatedAt = DateTime.UtcNow,
                     IsAddedFromAgency = true
                 }));
                Context.UserAssignedWidgets.AddRange(newwidgets);
                Context.SaveChanges();
            }
            return true;
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

        PagingResult<AgentListingModel> IAgencyManager.GetAgentsPagedList2(PagingModel model, long agency)
        {
            var result = new PagingResult<AgentListingModel>();
            model.RecordsPerPage = 10000000;
            IQueryable<POS> query = null;

            query = Context.POS.Where(f => f.IsDeleted == false && f.User.AgentId == agency).Take(model.RecordsPerPage).OrderBy("User.Agency.AgencyName" + " " + model.SortOrder);
             
            var list = query.ToList().Select(x => new AgentListingModel(x, 1)).ToList();

            result.List = list;
            result.Status = ActionStatus.Successfull;
            result.Message = "";
            result.TotalCount = query.Count();
            return result;
        }
        bool IAgencyManager.IsAdmin(long vendorId)
        {
            return Context.Agencies.Any(p => p.Representative == vendorId);
        }

    }


}
