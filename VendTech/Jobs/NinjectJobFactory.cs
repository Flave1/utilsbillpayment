using Ninject;
using Quartz;
using Quartz.Simpl;
using Quartz.Spi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using VendTech.BLL.Jobs;

namespace VendTech.Jobs
{
    public class NinjectJobFactory : SimpleJobFactory
    {
        readonly IKernel _kernel;

        public NinjectJobFactory(IKernel kernel)
        {
            this._kernel = kernel;
        }

        public override IJob NewJob(TriggerFiredBundle bundle, IScheduler scheduler)
        {
            try
            {
                // this will inject dependencies that the job requires
                return (IJob)this._kernel.Get(bundle.JobDetail.JobType);
            }
            catch (Exception e)
            {
                throw new SchedulerException(string.Format("Problem while instantiating job '{0}' from the NinjectJobFactory.", bundle.JobDetail.Key), e);
            }
        }

        public static void RegisterJobs(IKernel kernel)
        {
            //Get the scheduler that has been registered with the Kernel
            var scheduler = kernel.Get<IScheduler>();

            // Below this line should be whatever code you are using today to schedule jobs, triggers, etc. and start the scheduler. This is just here for context

            // add jobs and start scheduler

            //Transaction Job Checker
            IJobDetail pendingTranxCheckerJob = JobBuilder
                .Create<PendingTransactionCheckJob>()
                .WithIdentity("PendingTransactionCheckJob", "PendingTranxCheckerGroup")
                .Build();

            //Trigger for the pending transaction checker job
            ITrigger pendingTranxCheckerJobTrigger = TriggerBuilder
                .Create()
                .WithSimpleSchedule(s => s.WithIntervalInSeconds(5).RepeatForever())
                .Build();

            scheduler.ScheduleJob(pendingTranxCheckerJob, pendingTranxCheckerJobTrigger);

            if (! scheduler.IsStarted)
            {
                scheduler.Start();
            }
        }
    }
}