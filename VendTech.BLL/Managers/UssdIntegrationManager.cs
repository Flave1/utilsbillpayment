using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VendTech.BLL.Interfaces;

namespace VendTech.BLL.Managers
{
    public class UssdIntegrationManager : IUssdIntegrationManager
    {
        private readonly IPOSManager _posManager;
        public UssdIntegrationManager(IPOSManager posManager)
        {
            _posManager = posManager;
            //_posManager.DeductFromVendorPOSBalance;
            //_posManager.keepTransanctiondetails
        }

    }
}
