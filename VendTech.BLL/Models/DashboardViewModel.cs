using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace VendTech.BLL.Models
{
    public class DashboardViewModel
    {
        public decimal totalDeposit { get; set; }
        public decimal totalSales { get; set; }
        public int posCount { get; set; }
        public int userCount { get; set; }
        public decimal walletBalance { get; set; }
        public decimal revenue { get; set; }
        public List<PlatformModel> platFormModels { get; set; }
        public List<TransactionChartData> transactionChartData { get; set; }
        public UserModel currentUser { get; set; }

        public PagingResult<DashboardBalanceSheetModel> Bs { get; set; } = new PagingResult<DashboardBalanceSheetModel>();

    }
}
