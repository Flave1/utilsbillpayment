using VendTech.BLL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using VendTech.DAL;

namespace VendTech.BLL.Interfaces
{
    public interface IMeterManager
    {
        ActionOutput SaveMeter(MeterModel model);
        ActionOutput DeleteMeter(long meterId,long userId);
        PagingResult<MeterAPIListingModel> GetMeters(long userID, int pageNo, int pageSize);
        ActionOutput RechargeMeter(RechargeMeterModel model);
        PagingResult<MeterRechargeApiListingModel> GetUserMeterRecharges(long userID, int pageNo, int pageSize);
        RechargeDetailPDFData GetRechargePDFData(long rechargeId);
        ActionOutput<MeterRechargeApiListingModel> GetRechargeDetail(long rechargeId);
        MeterModel GetMeterDetail(long meterId);
        Task<PagingResult<MeterRechargeApiListingModel>> GetUserMeterRechargesReportAsync(ReportSearchModel model,bool callFromAdmin=false, long agentId = 0);
        PagingResult<MeterRechargeApiListingModel> GetUserMeterRechargesHistory(ReportSearchModel model, bool callFromAdmin = false);
        List<SelectListItem> GetMetersDropDown(long userID);
        PagingResult<SalesReportExcelModel> GetSalesExcelReportData(ReportSearchModel model, bool callFromAdmin, long agentId = 0);
        Task<ReceiptModel> RechargeMeterReturn(RechargeMeterModel model);
        ReceiptModel ReturnVoucherReceipt(string token);
        RequestResponse ReturnRequestANDResponseJSON(string token);
        TransactionDetail GetLastTransaction();
        TransactionDetail GetSingleTransaction(string transactionId);
        IQueryable<BalanceSheetListingModel> GetBalanceSheetReportsPagedList(ReportSearchModel model, bool callFromAdmin, long agentId);
        PagingResult<GSTRechargeApiListingModel> GetUserGSTRechargesReport(ReportSearchModel model, bool callFromAdmin, long agentId = 0);
        IQueryable<DashboardBalanceSheetModel> GetDashboardBalanceSheetReports();
    }
    
}
