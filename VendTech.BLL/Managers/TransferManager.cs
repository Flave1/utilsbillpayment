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
    public class TransferManager : BaseManager, ITransferManager
    {

        PagingResult<AgentListingModel> ITransferManager.GetAllAgencyAdminVendors(PagingModel model, long agency)
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

        PagingResult<AgentListingModel> ITransferManager.GetOtherVendors(PagingModel model, long agency)
        {
            var result = new PagingResult<AgentListingModel>();
            model.RecordsPerPage = 10000000;
            IQueryable<POS> query = null;

            query = Context.POS.Where(f => f.IsDeleted == false && f.User.AgentId != agency).Take(5).OrderBy("User.Agency.AgencyName" + " " + model.SortOrder);

            var list = query.ToList().Select(x => new AgentListingModel(x, 1)).ToList();

            result.List = list;
            result.Status = ActionStatus.Successfull;
            result.Message = "";
            result.TotalCount = query.Count();
            return result;
        }

    }


}
