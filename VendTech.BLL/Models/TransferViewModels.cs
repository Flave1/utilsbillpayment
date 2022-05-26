using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VendTech.BLL.Models
{
    public class TransferViewModel
    {
        public bool CanTranferToOtherVendors { get; set; } = false;
        public bool CanTranferToOwnVendors { get; set; } = false;
        public string Vendor { get; set; }
        public string AdminName { get; set; }
        public string AdminPos { get; set; }
        public string AdminBalance { get; set; }
    }
}
