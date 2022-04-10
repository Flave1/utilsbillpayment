using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VendTech.Framework.Api
{
    public class ApiLogModel
    {
        public string Url { get; set; }
        public string Data { get; set; }
        public string Headers { get; set; }
        public string IPAddress { get; set; }
        public DateTime CreatedOn { get; set; }
        public string HttpMethod { get; set; }
    }

    public class ApiLogViewModel
    {
        public int SerialNo { get; set; }
        public string Url { get; set; }
        public string Data { get; set; }
        public string Token { get; set; }
        public string IPAddress { get; set; }
        public DateTime CreatedOn { get; set; }
        public string HttpMethod { get; set; }
    }
}
