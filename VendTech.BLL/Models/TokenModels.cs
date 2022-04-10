using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VendTech.BLL.Models
{
    public class TokenModel
    {
        public long TokenId { get; set; }
        public long UserId { get; set; }
        public string TokenKey { get; set; }
        public string DeviceToken { get; set; }
        public string AppType { get; set; }
        public DateTime ExpiresOn { get; set; }
        public DateTime CreatedOn { get; set; }
        public string PosNumber { get; set; }
    }
}
