using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VendTech.BLL.Models
{
    public class SendSMSRequest
    {
        public SendSMSRequest()
        {
            Authorization = "dnRlY2g6cFhQcnkkR3BuXzVVdndfIQ==";
            Sender = "VENDTECH";
        }
        public string Authorization { get; set; }
        public string Sender { get; set; }
        public string Recipient { get; set; }
        public string Payload { get; set; }
    }

    public class ReChargeSMS
    {
        public string TransactionId { get; set; }
        public string PhoneNo { get; set; }
    }
    public class SendViaEmail
    {
        public string TransactionId { get; set; }
        public string Email { get; set; }
    }

    public class RechargeSimpleRequest
    {
        public string Target { get; set; }
    }
}
