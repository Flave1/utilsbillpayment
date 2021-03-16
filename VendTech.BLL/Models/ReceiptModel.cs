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
        public double Charges { get; set; }
        public double Discount { get; set; }
        public double Commission { get; set; }
        public string UnitCost { get; set; }
        public double Unit { get; set; }
        public double Tarrif { get; set; }
        public string TerminalID { get; set; }
        public string SerialNo { get; set; }
        public string POS { get; set; }
        public string VendorId { get; set; }
        public double DebitRecovery { get; set; }
        public string Pin1 { get; set; }
        public string Pin2 { get; set; }
        public string Pin3 { get; set; }
        public decimal Tax { get; set; }
        public string TransactionDate { get; set; }
        public string EDSASerial { get; set; }
        public string VTECHSerial { get; set; }
        public bool ShouldShowSmsButton { get; set; }
        public ReceiptStatus ReceiptStatus { get; set; } = new ReceiptStatus();
    }

    public class ReceiptStatus
    {
        public string Status { get; set; }
        public string Message { get; set; }
    }
}
