using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VendTech.DAL;

namespace VendTech.BLL.Models
{
    public class DepositListingModel
    {
        public string UserName { get; set; }
        public string VendorName { get; set; }
        public string ChkNoOrSlipId { get; set; }
        public string Type { get; set; }
        public string Comments { get; set; }
        public string Bank { get; set; }
        public string Status { get; set; }
        public string PosNumber { get; set; }
        public string CreatedAt { get; set; }
        public string TransactionId { get; set; }
        public decimal Amount { get; set; }
        public decimal Balance { get; set; }
        public decimal NewBalance { get; set; }
        public decimal? PercentageAmount { get; set; }
        public long DepositId { get; set; }
        public DepositListingModel(Deposit obj, bool changeStatusForApi = false)
        {
            UserName = obj.User.Name + " " + obj.User.SurName;
            PosNumber = obj.POS != null ? obj.POS.SerialNumber : "";
            VendorName = !string.IsNullOrEmpty(obj.User.Vendor) ? obj.User.Vendor : obj.User.Name + " " + obj.User.SurName;
            ChkNoOrSlipId = obj.CheckNumberOrSlipId;
            Type = ((DepositPaymentTypeEnum)obj.PaymentType).ToString();
            Comments = obj.Comments;
            Bank = obj.BankAccount == null ? "GTBANK" : obj.BankAccount.BankName;
            if (!changeStatusForApi)
                Status = ((DepositPaymentStatusEnum)obj.Status).ToString();
            else
            {
                if (obj.Status == (int)DepositPaymentStatusEnum.Pending)
                    Status = "Pending";
                else if (obj.Status == (int)DepositPaymentStatusEnum.RejectedByAccountant || obj.Status == (int)DepositPaymentStatusEnum.Rejected)
                    Status = "Rejected";
                else if (obj.Status == (int)DepositPaymentStatusEnum.ApprovedByAccountant)
                    Status = "Processing";
                else if (obj.Status == (int)DepositPaymentStatusEnum.Released)
                    Status = "Approved";
            }
            Amount = obj.Amount;
            NewBalance = obj.NewBalance == null ? obj.Amount : obj.NewBalance.Value;
            PercentageAmount = obj.PercentageAmount;
            CreatedAt = obj.CreatedAt.ToString("dd/MM/yyyy hh:mm");//ToString("dd/MM/yyyy HH:mm");
            TransactionId = obj.TransactionId;
            DepositId = obj.DepositId;
            //Balance = obj.User.Balance == null ? 0 : obj.User.Balance.Value;
        }
    }
    public class DepositExcelReportModel
    {
        [DisplayName("Date/Time")]
        public string DATE_TIME { get; set; }
        public string POSID { get; set; }
        public string USERNAME { get; set; }
        public string AMOUNT { get; set; }
        [DisplayName("%")]
        public string PERCENT { get; set; }
        public string DEPOSIT_TYPE { get; set; }
        public string BANK { get; set; }
        public string DEPOSIT_REF_NO { get; set; }
        public string NEW_BALANCE { get; set; }
        public DepositExcelReportModel(Deposit obj, bool changeStatusForApi = false)
        {
            DATE_TIME = obj.CreatedAt.ToString("dd/MM/yyyy hh:mm");      //ToString("dd/MM/yyyy HH:mm");
            USERNAME = obj.User.Name + " " + obj.User.SurName;
            POSID = obj.POS != null ? obj.POS.SerialNumber : "";
            DEPOSIT_REF_NO = obj.CheckNumberOrSlipId;
            DEPOSIT_TYPE = ((DepositPaymentTypeEnum)obj.PaymentType).ToString();
            BANK = obj.BankAccount == null ? "GTBANK" : obj.BankAccount.BankName;
            AMOUNT = string.Format("{0:N0}", obj.Amount);
            NEW_BALANCE = obj.NewBalance == null ? string.Format("{0:N0}", obj.Amount) : string.Format("{0:N0}", obj.NewBalance.Value);
            PERCENT = string.Format("{0:N0}", obj.PercentageAmount);
            //Balance = obj.User.Balance == null ? 0 : obj.User.Balance.Value;
        }

        public DepositExcelReportModel()
        {
        }
    }
    public class DepositLogListingModel
    {
        public string UserName { get; set; }
        public string DepositerName { get; set; }
        public string PreviousStatus { get; set; }
        public string NewStatus { get; set; }
        public string CreatedAt { get; set; }
        public string VendorName { get; set; }
        public decimal Amount { get; set; }
        public decimal Balance { get; set; }
        public decimal? PercentageAmount { get; set; }
        public long DepositId { get; set; }
        public string PosNumber { get; set; }

        public string TransactionId { get; set; }
        public long DepositLogId { get; set; }
        public DepositLogListingModel(DepositLog obj)
        {
            UserName = obj.User.Name + " " + obj.User.SurName;
            PreviousStatus = ((DepositPaymentStatusEnum)obj.PreviousStatus).ToString();
            NewStatus = ((DepositPaymentStatusEnum)obj.NewStatus).ToString();
            VendorName = obj.Deposit.User.User1 == null ? "" : obj.Deposit.User.User1.Name + " " + obj.Deposit.User.User1.SurName;
            Amount = obj.Deposit.Amount;
            PercentageAmount = obj.Deposit.PercentageAmount;
            TransactionId = obj.Deposit.TransactionId;
            CreatedAt = obj.CreatedAt.ToString("dd/MM/yyyy hh:mm");//ToString("dd/MM/yyyy hh:mm:ss tt"); ;
            DepositId = obj.DepositId;
            DepositLogId = obj.DepositLogId;
            DepositerName = obj.Deposit.User.Name + " " + obj.Deposit.User.SurName;
            //Balance = obj.Deposit.User.Balance == null ? 0 : obj.Deposit.User.Balance.Value;
        }
    }

    public class DepositModel
    {

        public long UserId { get; set; }
        public long VendorId { get; set; }
        [Required(ErrorMessage = "Please select POS ID")]
        public long PosId { get; set; }
        public int BankAccountId { get; set; }
        public DepositPaymentTypeEnum DepositType { get; set; }
        [Required(ErrorMessage = "Cheque Or Slip No is required")]
        public string ChkOrSlipNo { get; set; }
        public string ChkBankName { get; set; }
        public string NameOnCheque { get; set; }
        [Required]
        public decimal Amount { get; set; }

        public int Percentage { get; set; }
        public decimal TotalAmountWithPercentage { get; set; }
        public string Comments { get; set; }
    }
    public class ReleaseDepositModel
    {
        public List<long> ReleaseDepositIds { get; set; }
        public List<long> CancelDepositIds { get; set; }
        public string OTP { get; set; }
    }

    public class MeterAndDepositListingModel
    {
        public List<DepositListingModel> Deposits { get; set; }
        public List<MeterRechargeApiListingModel> Recharges { get; set; }
    }

    public class ReportSearchModel
    {
        public ReportSearchModel()
        {
            if (PageNo <= 1)
            {
                PageNo = 1;
            }
            if (RecordsPerPage <= 0)
            {
                RecordsPerPage = AppDefaults.PageSize;
            }
        }
        public long? VendorId { get; set; }
        public string ReportType { get; set; }
        public string ProductShortName { get; set; }
        public long? PosId { get; set; }
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
        public string Meter { get; set; }
        public string RefNumber { get; set; }
        public string TransactionId { get; set; }
        public string RechargeToken { get; set; }
        public int? Bank { get; set; }
        public int? DepositType { get; set; }
        public int PageNo { get; set; }
        public int RecordsPerPage { get; set; }
        public string SortBy { get; set; }
        public string SortOrder { get; set; }
    }

    
}
