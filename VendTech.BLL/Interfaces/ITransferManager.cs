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
    public interface ITransferManager
    {
        PagingResult<AgentListingModel> GetAllAgencyAdminVendor(PagingModel model, long agency);
    }

}
