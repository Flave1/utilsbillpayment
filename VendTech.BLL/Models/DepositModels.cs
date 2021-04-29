using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
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
        public string Payer { get; set; }
        public string IssuingBank { get; set; }
        public string ValueDate { get; set; }
        public DepositListingModel(Deposit obj, bool changeStatusForApi = false)
        {
            VendorName = !string.IsNullOrEmpty(obj.User.Vendor) ? obj.User.Vendor : obj.User.Name + " " + obj.User.SurName;
            Type = ((DepositPaymentTypeEnum)obj.PaymentType).ToString();
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
            Payer = obj.NameOnCheque == null ? "" : obj.NameOnCheque;
            IssuingBank = obj.ChequeBankName != null ? obj.ChequeBankName + '-' + obj.BankAccount.AccountNumber.Replace("/", string.Empty).Substring(obj.BankAccount.AccountNumber.Replace("/", string.Empty).Length - 3) : "";
            ValueDate = obj.ValueDate == null ? obj.CreatedAt.ToString("dd/MM/yyyy hh:mm") : obj.ValueDate;
        }
         
    }
    public class DepositExcelReportModel
    {
        [DisplayName("Date/Time")]
        public string DATE_TIME { get; set; }
        public string VALUEDATE { get; set; }
        public string POSID { get; set; }
        public string USERNAME { get; set; }
        public string DEPOSIT_TYPE { get; set; }
        public string BANK { get; set; }
        [DisplayName("TRANSACTION ID")]
        public string TRANSACTION_ID { get; set; }
        public string DEPOSIT_REF_NO { get; set; }
        public string AMOUNT { get; set; }
        [DisplayName("%")]
        public string PERCENT { get; set; }
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
            TRANSACTION_ID = obj?.TransactionId;
            VALUEDATE = obj.CreatedAt.ToString("dd/MM/yyyy hh:mm");
            //Balance = obj.User.Balance == null ? 0 : obj.User.Balance.Value;
        }

        public DepositExcelReportModel()
        {
        }
    }

    public class DepositAuditExcelReportModel
    {
        [DisplayName("Date/Time")]
        public string DATE_TIME { get; set; }
        //public string VALUEDATE { get; set; }
        public string POSID { get; set; }
        public string GTBANK { get; set; }
        [DisplayName("DepositBy")]
        public string DEPOSIT_BY { get; set; }
        [DisplayName("DepositType")]
        public string DEPOSIT_TYPE { get; set; }
        [DisplayName("PayerBank")]
        public string ISSUINGBANK { get; set; }
        public string PAYER { get; set; }
        [DisplayName("DepositRef#")]
        public string DEPOSIT_REF_NO { get; set; }
        public string AMOUNT { get; set; }
        public string STATUS { get; set; }

        public DepositAuditExcelReportModel(Deposit obj, bool changeStatusForApi = false)
        {
            DATE_TIME = obj.CreatedAt.ToString("dd/MM/yyyy hh:mm");      //ToString("dd/MM/yyyy HH:mm");
            DEPOSIT_BY = obj.User.Name + " " + obj.User.SurName;
            POSID = obj.POS != null ? obj.POS.SerialNumber : "";
            DEPOSIT_REF_NO = obj.CheckNumberOrSlipId;
            DEPOSIT_TYPE = ((DepositPaymentTypeEnum)obj.PaymentType).ToString();
            AMOUNT = string.Format("{0:N0}", obj.Amount);
            PAYER = obj.NameOnCheque == null ? "" : obj.NameOnCheque;
            ISSUINGBANK = !string.IsNullOrEmpty(obj.ChequeBankName) ? obj.ChequeBankName.IndexOf('-') == -1 ? obj.ChequeBankName : obj.ChequeBankName.Substring(0, obj.ChequeBankName.IndexOf("-")) : "";
            STATUS = Convert.ToBoolean(obj.isAudit) ? "Cleared" : "Open";
            GTBANK = obj.BankAccount == null ? "GTBANK" + '-' + obj.BankAccount.AccountNumber.Replace("/", string.Empty).Substring(obj.BankAccount.AccountNumber.Replace("/", string.Empty).Length - 3) : obj.BankAccount.BankName + '-' + obj.BankAccount.AccountNumber.Replace("/", string.Empty).Substring(obj.BankAccount.AccountNumber.Replace("/", string.Empty).Length - 3);
            //VALUEDATE = obj.CreatedAt.ToString("dd/MM/yyyy hh:mm");
        }

        public DepositAuditExcelReportModel()
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
        public DateTime ValueDate { get; set; } = DateTime.UtcNow;

        public List<DepositListingModel> History { get; set; } = new List<DepositListingModel>();
    }
    public class ReleaseDepositModel
    {
        public List<long> ReleaseDepositIds { get; set; }
        public List<long> CancelDepositIds { get; set; }
        public string OTP { get; set; }
    }

    public class ReverseDepositModel
    {
        public List<long> ReverseDepositIds { get; set; }
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
        public string Payer { get; set; }
        public string IssuingBank { get; set; }
        public string Amount { get; set; }
        public bool IsAudit { get; set; }
        public string Status { get; set; }
        public bool IsInitialLoad { get; set; } = false;
    }

    public class DepositAuditModel
    {
        public long DepositId { get; set; }
        public string DateTime { get; set; }
        public long? PosId { get; set; }
        public string DepositBy { get; set; }
        public string Payer { get; set; }
        public string IssuingBank { get; set; }
        public string DepositRef { get; set; }
        public string GTBank { get; set; }
        public decimal Amount { get; set; }
        public string VendorName { get; set; }
        public string Type { get; set; }
        public string Status { get; set; }
        public string CreatedAt { get; set; }
        public string TransactionId { get; set; }
        public long Id { get; set; }
        public bool isAudit { get; set; }
        public long UserId { get; set; }
        public string Price { get; set; }
        public DateTime ValueDate { get; set; }
        public string ValueDateModel { get; set; }
        public DepositAuditModel() { }
        public DepositAuditModel(Deposit obj, bool changeStatusForApi = false)
        {
            Id = obj.DepositId;
            isAudit = Convert.ToBoolean(obj.isAudit);
            UserId = obj.UserId;
            DepositBy =  obj.User.Name.Trim() + " " + obj.User.SurName.Trim();
            PosId = obj.POS != null ? Convert.ToInt64(obj.POS.SerialNumber) : 0;
            VendorName = !string.IsNullOrEmpty(obj.User.Vendor) ? obj.User.Vendor : obj.User.Name + " " + obj.User.SurName;
            DepositRef = obj.CheckNumberOrSlipId;
            Type = ((DepositPaymentTypeEnum)obj.PaymentType).ToString(); GTBank = obj.BankAccount == null ? "GTBANK" + '-' + obj.BankAccount.AccountNumber.Replace("/", string.Empty).Substring(obj.BankAccount.AccountNumber.Replace("/", string.Empty).Length - 3) : obj.BankAccount.BankName + '-' + obj.BankAccount.AccountNumber.Replace("/", string.Empty).Substring(obj.BankAccount.AccountNumber.Replace("/", string.Empty).Length - 3);
            Payer = !string.IsNullOrEmpty(obj.NameOnCheque) ? obj.NameOnCheque : "";
            IssuingBank = obj.ChequeBankName != null ? obj.ChequeBankName + '-' + obj.BankAccount.AccountNumber.Replace("/", string.Empty).Substring(obj.BankAccount.AccountNumber.Replace("/", string.Empty).Length - 3) : "";
            Amount = obj.Amount;
            CreatedAt = obj.CreatedAt.ToString("dd/MM/yyyy hh:mm");//ToString("dd/MM/yyyy HH:mm");
            TransactionId = obj.TransactionId;
            ValueDateModel = obj.ValueDate == null ? new DateTime().ToString("dd/MM/yyyy hh:mm") : System.DateTime.ParseExact(obj.ValueDate,"dd/MM/yyyy hh:mm",CultureInfo.InvariantCulture).ToString("dd/MM/yyyy hh:mm");
        }
    }
}
