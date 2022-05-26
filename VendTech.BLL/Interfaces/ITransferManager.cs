using VendTech.BLL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using VendTech.DAL;

namespace VendTech.BLL.Interfaces
{
    public interface ITransferManager
    {
        PagingResult<AgentListingModel> GetAllAgencyAdminVendors(PagingModel model, long agency);
        PagingResult<AgentListingModel> GetOtherVendors(PagingModel model, long agency);
    }

}
