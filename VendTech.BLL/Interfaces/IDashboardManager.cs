using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VendTech.BLL.Models;

namespace VendTech.BLL.Interfaces
{
    public interface IDashboardManager
    {
        decimal GetDepositTotal();
        decimal GetSalesTotal();
        decimal GetWalletBalance();
        int GetPOSCount();
        DashboardViewModel getDashboardData(long userId, long agentId  = 0);
        bool IsUserAnAgent(long userId = 0);
    }
}
