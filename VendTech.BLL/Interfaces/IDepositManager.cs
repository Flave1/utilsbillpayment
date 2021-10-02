using System.Collections.Generic;
using VendTech.BLL.Models;
using VendTech.DAL;

namespace VendTech.BLL.Interfaces
{
    public interface IDepositManager
    {
        /// <summary>
        /// Dummy Method for testing purpose:  Get Welcome Message
        /// </summary>
        /// <returns></returns>
        string GetWelcomeMessage();

        /// <summary>
        /// This will be used to get user listing model
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        PagingResult<DepositListingModel> GetDepositPagedList(PagingModel model, bool getForRelease = false, long vendorId = 0,string status="");
        PagingResult<DepositLogListingModel> GetDepositLogsPagedList(PagingModel model);
        decimal GetPendingDepositTotal();
        ActionOutput ChangeDepositStatus(long depositId, DepositPaymentStatusEnum status, long currentUserId); 
        ActionOutput<string> SendOTP();
        ActionOutput<Deposit> SaveDepositRequest(DepositModel model);
        ActionOutput<List<long>> ChangeMultipleDepositStatus(ReleaseDepositModel model, long userId);
        PagingResult<DepositListingModel> GetUserDepositList(int pageNo, int pageSize, long userId);
        ActionOutput<DepositListingModel> GetDepositDetail(long depositId);
        PagingResult<DepositListingModel> GetReportsPagedList(ReportSearchModel model, bool callFromAdmin = false);
        PagingResult<DepositAuditModel> GetAuditReportsPagedList(ReportSearchModel model, bool callFromAdmin = false);
        PagingResult<DepositAuditModel> GetDepositAuditReports(ReportSearchModel model, bool callFromAdmin = false);
        DepositAuditModel SaveDepositAuditRequest(DepositAuditModel depositAuditModel);
        PagingResult<DepositListingModel> GetReportsPagedHistoryList(ReportSearchModel model, bool callFromAdmin = false);
        PagingResult<DepositExcelReportModel> GetReportsExcelDeposituser(ReportSearchModel model, bool callFromAdmin = false);
        PagingResult<DepositExcelReportModel> GetReportExcelData(ReportSearchModel model);
        PagingResult<DepositAuditExcelReportModel> GetAuditReportExcelData(ReportSearchModel model);
        PagingResult<DepositListingModel> GetReleasedDepositPagedList(PagingModel model, bool getForRelease, long vendorId = 0);
        ActionOutput ChangeMultipleDepositStatusOnReverse(ReverseDepositModel model, long userId);
        ActionOutput ReverseDepositStatus(long depositId, DepositPaymentStatusEnum status, long currentUserId);
        List<Deposit> GetUnclearedDeposits();
        void UpdateNextReminderDate(Deposit deposit);
        DepositAuditModel UpdateDepositAuditRequest(DepositAuditModel depositAuditModel);
    }

}
