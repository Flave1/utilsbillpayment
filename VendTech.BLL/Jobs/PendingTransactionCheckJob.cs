using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VendTech.BLL.Interfaces;

namespace VendTech.BLL.Jobs
{
    public class PendingTransactionCheckJob : IJob
    {
        private IPlatformTransactionManager _platformTransactionManager;

        public PendingTransactionCheckJob(IPlatformTransactionManager platformTransactionManager) 
        {
            _platformTransactionManager = platformTransactionManager;
        }
        public void Execute(IJobExecutionContext context)
        {
            _platformTransactionManager.CheckPendingTransaction();
        }
    }
}
