using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VendTech.BLL.Models
{
    public class RequestResponse
    {
        public string Request { get; set; }
        public string Response { get; set; }
        public ReceiptStatus ReceiptStatus { get; set; } = new ReceiptStatus();
    }
    public class ReceiptModel
    {
        public string ReceiptNo { get; set; }
        public string CustomerName { get; set; }
        public string AccountNo { get; set; }
        public string Address { get; set; }
        public string DeviceNumber { get; set; } 
        public string Amount { get; set; }
        public string Charges { get; set; }
        public string Discount { get; set; }
        public string Commission { get; set; }
        public string UnitCost { get; set; }
        public string Unit { get; set; }
        public string Tarrif { get; set; }
        public string TerminalID { get; set; }
        public string SerialNo { get; set; }
        public string POS { get; set; }
        public string VendorId { get; set; }
        public decimal DebitRecovery { get; set; }
        public string Pin1 { get; set; }
        public string Pin2 { get; set; }
        public string Pin3 { get; set; }
        public string Tax { get; set; }
        public string TransactionDate { get; set; }
        public string EDSASerial { get; set; }
        public string VTECHSerial { get; set; }
        public bool ShouldShowSmsButton { get; set; }
        public bool ShouldShowPrintButton { get; set; }
        public bool mobileShowSmsButton { get; set; }
        public bool mobileShowPrintButton { get; set; }
        public bool ShowEmailButtonOnWeb { get; set; } = false;
        public decimal CurrentBallance { get; set; }
        public ReceiptStatus ReceiptStatus { get; set; } = new ReceiptStatus();
        public int? PlatformId { get; set; }
        public string CurrencyCode { get; set; } = "";
    }

    public class ReceiptStatus
    {
        public string Status { get; set; }
        public string Message { get; set; }
    }

    public class AirtimeReceiptModel
    {
        public string ReceiptNo { get; set; }
        public string CustomerName { get; set; }
        public string Phone { get; set; }
        public string Amount { get; set; }
        public string Charges { get; set; }
        public string Discount { get; set; }
        public string Commission { get; set; }
        public string SerialNo { get; set; }
        public string POS { get; set; }
        public string VendorId { get; set; }
        public decimal DebitRecovery { get; set; }
        public string Token { get; set; }
        public string TransactionDate { get; set; }
        public string EDSASerial { get; set; }
        public string VTECHSerial { get; set; }
        public bool ShouldShowSmsButton { get; set; }
        public bool ShouldShowPrintButton { get; set; }
        public bool mobileShowSmsButton { get; set; }
        public bool mobileShowPrintButton { get; set; }
        public decimal CurrentBallance { get; set; }
        public string ReceiptTitle { get; set; }
        public bool IsNewRecharge { get; set; }
        public ReceiptStatus ReceiptStatus { get; set; } = new ReceiptStatus();
    }
}
