using System;
using System.Collections.Generic;
using System.Linq;
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
        PagingResult<DepositListingModel> GetLastTenDepositPagedList(PagingModel model, long posId = 0);
        PagingResult<DepositListingModel> GetAllPendingDepositPagedList(PagingModel model, bool getForRelease = false, long vendorId = 0, string status = "");
        PagingResult<DepositLogListingModel> GetDepositLogsPagedList(PagingModel model);
        decimal GetPendingDepositTotal();
        ActionOutput ChangeDepositStatus(long depositId, DepositPaymentStatusEnum status, long currentUserId); 
        ActionOutput<string> SendOTP();
        ActionOutput<PendingDeposit> SaveDepositRequest(DepositModel model);
        ActionOutput<List<long>> ChangeMultipleDepositStatus(ReleaseDepositModel model, long userId);
        PagingResult<DepositListingModel> GetUserDepositList(int pageNo, int pageSize, long userId);
        ActionOutput<DepositListingModel> GetDepositDetail(long depositId);
        PagingResult<DepositListingModel> GetReportsPagedList(ReportSearchModel model, bool callFromAdmin = false, long agentId = 0);
        PagingResult<DepositAuditModel> GetAuditReportsPagedList(ReportSearchModel model, bool callFromAdmin = false);
        PagingResult<DepositAuditModel> GetDepositAuditReports(ReportSearchModel model, bool callFromAdmin = false);
        DepositAuditModel SaveDepositAuditRequest(DepositAuditModel depositAuditModel);
        PagingResult<DepositListingModel> GetReportsPagedHistoryList(ReportSearchModel model, bool callFromAdmin = false, long agentId = 0);
        PagingResult<DepositExcelReportModel> GetReportsExcelDeposituser(ReportSearchModel model, bool callFromAdmin = false, long agentId = 0);
        PagingResult<DepositExcelReportModel> GetReportExcelData(ReportSearchModel model, long agentId = 0);
        PagingResult<DepositAuditExcelReportModel> GetAuditReportExcelData(ReportSearchModel model);
        PagingResult<DepositListingModel> GetReleasedDepositPagedList(PagingModel model, bool getForRelease, long vendorId = 0);
        ActionOutput ChangeMultipleDepositStatusOnReverse(ReverseDepositModel model, long userId);
        ActionOutput ReverseDepositStatus(long depositId, DepositPaymentStatusEnum status, long currentUserId);
        List<Deposit> GetUnclearedDeposits();
        void UpdateNextReminderDate(Deposit deposit);
        DepositAuditModel UpdateDepositAuditRequest(DepositAuditModel depositAuditModel);
        List<PendingDeposit> GetListOfDeposits(List<long> depositIds);
        decimal ReturnPendingDepositsTotalAmount(DepositModel model);
        decimal TakeCommisionsAndReturnAgentsCommision(long posId, decimal amt);
        Deposit SaveApprovedDeposit(PendingDeposit model);
        IQueryable<BalanceSheetListingModel> GetBalanceSheetReportsPagedList(ReportSearchModel model, bool callFromAdmin, long agentId);
        IQueryable<DashboardBalanceSheetModel> GetDashboardBalanceSheetReports(DateTime date);
        PagingResult<AgentRevenueListingModel> GetAgentRevenueReportsPagedList(ReportSearchModel model, bool callFromAdmin= false, long agentId = 0);
        PagingResult<AgencyRevenueExcelReportModel> GetAgentRevenueReportsExcelDeposituser(ReportSearchModel model, bool callFromAdmin = false, long agentId = 0);
        void DeletePendingDeposits(List<PendingDeposit> deposits);
        ActionOutput CreateDepositCreditTransfer(Deposit dbDeposit, long currentUserId, long fromPos, string otp);
        ActionOutput CreateDepositDebitTransfer(Deposit dbDeposit, long currentUserId, string otp);
        ActionOutput DepositToAgencyAdminAccount(Deposit dbDeposit, long currentUserId, string OTP);
        ActionOutput<string> CancelDeposit(CancelDepositModel model);
    }

}
