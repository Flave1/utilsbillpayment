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
    public interface ICommissionManager
    {
        List<CommissionModel> GetCommissions();
        ActionOutput SaveCommission(SaveCommissionModel model);
        ActionOutput DeleteCommission(int commissionId);
        List<SelectListItem> GetCommissionSelectList();
    }
    
}
