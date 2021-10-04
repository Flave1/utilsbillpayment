using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VendTech.BLL.Models;

namespace VendTech.BLL.Interfaces
{
    public interface ISMSManager
    {
        Task<bool> SendSmsAsync(SendSMSRequest request);
    }
}
