using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VendTech.BLL.Models
{
    public class ReceiptModel
    {
        public string ReceiptNo { get; set; }
        public string CustomerName { get; set; }
        public string AccountNo { get; set; }
        public string Address { get; set; }
        public string DeviceNumber { get; set; }
        public string RechargeToken { get; set; }
        public double Amount { get; set; }
        public double Charges { get; set; }
        public double Discount { get; set; }
        public double Commission { get; set; }
        public double UnitCost { get; set; }
        public double Unit { get; set; }
        public double Tarrif { get; set; }
        public string TerminalID { get; set; }
        public string SerialNo { get; set; }
    }
}
