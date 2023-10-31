using Quartz;
using System.Web.Mvc;
using VendTech.BLL.Interfaces;

namespace VendTech.BLL.Jobs
{
    public class PendingTransactionCheckJob : IJob
    {
        public void Execute(IJobExecutionContext context)
        {
            var _platformTransactionManager = DependencyResolver.Current.GetService<IPlatformTransactionManager>();
            _platformTransactionManager.CheckPendingTransaction();
        }
    }
}
