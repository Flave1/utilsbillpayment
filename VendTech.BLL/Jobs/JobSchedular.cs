using Quartz;
using Quartz.Impl;
using VendTech.BLL.Jobs;

public class JobScheduler
{
    public static void Start()
    {
        ISchedulerFactory schedulerFactory = new StdSchedulerFactory();
        IScheduler scheduler = schedulerFactory.GetScheduler();

        scheduler.Start();

        IJobDetail jobDetail = JobBuilder.Create<PendingTransactionCheckJob>()
            .WithIdentity("PendingTransactionCheck")
            .Build();

        ITrigger trigger = TriggerBuilder.Create()
            .WithIdentity("PendingTransactionCheckTrigger")
            .WithCronSchedule("0 0/1 * 1/1 * ? *") // Cron expression for every 1 minutes
            .Build();

        scheduler.ScheduleJob(jobDetail, trigger);
    }
}
