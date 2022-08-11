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
        public List<PlatformModel> platFormModels { get; set; } = new List<PlatformModel>();
        public List<TransactionChartData> transactionChartData { get; set; } = new List<TransactionChartData>();
        public UserModel currentUser { get; set; } = new UserModel();

        public PagingResult<VendorStatus> Bs { get; set; } = new PagingResult<VendorStatus>();

        public PagingResult<MeterRechargeApiListingModel> s { get; set; } = new PagingResult<MeterRechargeApiListingModel>();
        public PagingResult<AgentListingModel> TransferFromVendors { get; set; } = new PagingResult<AgentListingModel>();

    }
}
