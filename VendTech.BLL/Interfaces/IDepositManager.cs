using VendTech.BLL.Models;

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
        PagingResult<DepositListingModel> GetDepositPagedList(PagingModel model, bool getForRelease = false, long vendorId = 0);
        PagingResult<DepositLogListingModel> GetDepositLogsPagedList(PagingModel model);
        decimal GetPendingDepositTotal();
        ActionOutput ChangeDepositStatus(long depositId, DepositPaymentStatusEnum status, long currentUserId);
        ActionOutput<string> SendOTP();
        ActionOutput SaveDepositRequest(DepositModel model);
        ActionOutput ChangeMultipleDepositStatus(ReleaseDepositModel model, long userId);
        PagingResult<DepositListingModel> GetUserDepositList(int pageNo, int pageSize, long userId);
        ActionOutput<DepositListingModel> GetDepositDetail(long depositId);
        PagingResult<DepositListingModel> GetReportsPagedList(ReportSearchModel model, bool callFromAdmin = false);
        PagingResult<DepositAuditModel> GetAuditReportsPagedList(ReportSearchModel model, bool callFromAdmin = false);
        PagingResult<DepositAuditModel> GetDepositAuditReports(ReportSearchModel model, bool callFromAdmin = false);
        DepositAuditModel SaveDepositAuditRequest(DepositAuditModel depositAuditModel);
        PagingResult<DepositExcelReportModel> GetReportsExcelDeposituser(ReportSearchModel model, bool callFromAdmin = false);
        PagingResult<DepositExcelReportModel> GetReportExcelData(ReportSearchModel model);
        PagingResult<DepositAuditExcelReportModel> GetAuditReportExcelData(ReportSearchModel model);
    }

}
