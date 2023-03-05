using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VendTech.BLL.Models
{


    public  class IcekloudQueryResponse
    { 
        public string Status { get; set; }
         
        public QueryContent Content { get; set; }
         
        public object[] ErrorLog { get; set; }
    }

    public  class QueryContent
    { 
        public long SerialNumber { get; set; }
         
        public long TransactionId { get; set; }
         
        public long StatusRequestCount { get; set; }

        public bool Finalised { get; set; } = false;
         
        public bool Sold { get; set; }
         
        public object DateAndTimeSold { get; set; }
         
        public DateTimeOffset DateAndTimeCreated { get; set; }
         
        public object DateAndTimeFinalised { get; set; }
         
        public object DateAndTimeLinked { get; set; }
         
        public DateTimeOffset DateAndTimeRequested { get; set; }
         
        public object Provider { get; set; }
         
        public object VoucherSerialNumber { get; set; }
         
        public object Denomination { get; set; }
         
        public string VoucherPin { get; set; }
         
        public object MeterNumber { get; set; }
         
        public object Units { get; set; }
         
        public object Tariff { get; set; }
         
        public string Customer { get; set; }
         
        public object CustomerAccNo { get; set; }
         
        public object ServiceCharge { get; set; }
         
        public object TaxCharge { get; set; }
         
        public string StatusDescription { get; set; }
         
        public string Status { get; set; }
    }
    public class IceCloudErorResponse
    { 
        public string Status { get; set; }
         
        public string Content { get; set; }
         
        public string SystemError { get; set; }
         
        public Stack[] Stack { get; set; }
    }

    public class Stack
    { 
        public long Code { get; set; }
         
        public string Message { get; set; }
         
        public string File { get; set; } 
        public string Detail { get; set; }
         
        public string Type { get; set; }
    }

    public class IcekloudRequestmodel
    { 
        public IcekloudAuth Auth { get; set; }
         
        public string Request { get; set; }
         
        public object[] Parameters { get; set; }
    }
    public partial class IcekloudAuth
    { 
        public string UserName { get; set; }
         
        public string Password { get; set; }
    }


    public partial class IceKloudResponse
    { 
        public string Status { get; set; }

        public Content Content { get; set; } = new Content();
         
        public object[] ErrorLog { get; set; }

        public IcekloudRequestmodel RequestModel { get; set; } = new IcekloudRequestmodel();
    }

    public partial class Content
    {
        public DataResponse Data { get; set; } = new DataResponse();
         
        public string ProcessOption { get; set; }
    }

    public partial class DataResponse
    {
        public Datum[] Data { get; set; } = new Datum[0];
         
        public string DataName { get; set; }
         
        public string Error { get; set; }
         
        public long ErrorCode { get; set; }
    }

    public partial class Datum
    { 
        public long Barcode { get; set; }
         
        public string DateAndTime { get; set; }
         
        public long DealerBalance { get; set; }
         
        public long Denomination { get; set; }
         
        public string ErrorMsg { get; set; }
         
        public long Id { get; set; }
         
        public string Instructions { get; set; }
         
        public string PinNumber { get; set; }
         
        public string PinNumber2 { get; set; }
         
        public string PinNumber3 { get; set; }

        public PowerHubVoucher PowerHubVoucher { get; set; } = new PowerHubVoucher();
         
        public string Provider { get; set; }
         
        public string SerialNumber { get; set; }

        public Tym2SellVoucher Tym2SellVoucher { get; set; } = new Tym2SellVoucher();
         
        public long VoucherProfit { get; set; }
         
        public object XmlResponse { get; set; }
    }

    public partial class PowerHubVoucher
    { 
        public long AccountCredit { get; set; }
         
        public string AccountNumber { get; set; }
         
        public string CostOfUnits { get; set; }
         
        public object CustAccNo { get; set; }
         
        public object CustAddress { get; set; }
         
        public object CustCanVend { get; set; }
         
        public object CustContactNo { get; set; }
         
        public object CustDaysLastPurchase { get; set; }
         
        public object CustLocalRef { get; set; }
         
        public object CustMsno { get; set; }
         
        public object CustMinVendAmt { get; set; }
         
        public object CustName { get; set; }
         
        public string Customer { get; set; }
         
        public long DebtRecoveryAmt { get; set; } 
        public long DebtRecoveryBalance { get; set; }
         
        public string MeterNumber { get; set; }
         
        public object PayAccDesc { get; set; }
         
        public object PayAccNo { get; set; }
         
        public object PayAmount { get; set; }
         
        public object PayBalance { get; set; }
         
        public object PayReceiptNo { get; set; }
         
        public string Pin1 { get; set; }
         
        public string Pin2 { get; set; }
         
        public string Pin3 { get; set; }
         
        public string RtsUniqueId { get; set; }
         
        public string ReceiptNumber { get; set; }
         
        public string Sgc { get; set; }
         
        public string ServiceCharge { get; set; }
         
        public string Tariff { get; set; }
         
        public string TaxCharge { get; set; }
         
        public string TenderedAmount { get; set; }
         
        public string TransactionAmount { get; set; }
         
        public string Units { get; set; }
         
        public long VatNumber { get; set; }
    }

    public partial class Tym2SellVoucher
    { 
        public object Account { get; set; }
         
        public object ClientId { get; set; }
         
        public object CostOfUnits { get; set; }
         
        public object Customer { get; set; }
         
        public long GovermentLevy { get; set; }
         
        public bool KeyChangeDetected { get; set; }
         
        public object KeyChangeToken1 { get; set; }
         
        public object KeyChangeToken2 { get; set; }
         
        public object ReceiptNumber { get; set; }
         
        public long StandingCharge { get; set; }
         
        public object StsMeter { get; set; }
         
        public object TenderedAmount { get; set; }
         
        public object Units { get; set; }
         
        public object Vat { get; set; }
         
        public object VatNo { get; set; }
         
        public bool VoucherTextDecodeFailed { get; set; }
    }
}
