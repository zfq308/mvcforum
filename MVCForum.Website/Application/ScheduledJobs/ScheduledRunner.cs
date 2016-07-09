using Microsoft.Practices.Unity;
using MVCForum.Domain.Constants;
using Quartz;

namespace MVCForum.Website.Application.ScheduledJobs
{
    public static class ScheduledRunner
    {
        public static void Run(IUnityContainer container)
        {
            // Resolving IScheduler instance
            var scheduler = container.Resolve<IScheduler>();

            #region Triggers

            // 定义5分钟长期定时器
            var fiveMinuteTriggerForever = (ISimpleTrigger)TriggerBuilder.Create()
                 .WithIdentity("FiveMinuteTriggerForever", AppConstants.DefaultTaskGroup)
                 .StartNow()
                 .WithSimpleSchedule(x => x.WithIntervalInMinutes(5).RepeatForever())
                 .Build();

            // 定义1小时长期定时器
            var OneHourTriggerForever = (ISimpleTrigger)TriggerBuilder.Create()
                    .WithIdentity("OneHourTriggerForever", AppConstants.DefaultTaskGroup)
                    .StartNow()
                    .WithSimpleSchedule(x => x.WithIntervalInHours(1).RepeatForever())
                    .Build();

            #region 无效的定时器定义
            //// 定义2分钟长期定时器
            //var twoMinuteTriggerForever = (ISimpleTrigger)TriggerBuilder.Create()
            // .WithIdentity("TwoMinuteTriggerForever", AppConstants.DefaultTaskGroup)
            // .StartNow()
            // .WithSimpleSchedule(x => x
            //     .WithIntervalInMinutes(2)
            //     .RepeatForever())
            // .Build();

            //// 定义15秒长期定时器
            //var fifteenSecondsTriggerForever = (ISimpleTrigger)TriggerBuilder.Create()
            //        .WithIdentity("FifteenSecondsTriggerForever", AppConstants.DefaultTaskGroup)
            //        .StartNow()
            //        .WithSimpleSchedule(x => x
            //            .WithIntervalInSeconds(15)
            //            .RepeatForever())
            //        .Build();

            //// 定义6小时长期定时器
            //var sixHourTriggerForever = (ISimpleTrigger)TriggerBuilder.Create()
            //        .WithIdentity("SixHourTriggerForever", AppConstants.DefaultTaskGroup)
            //        .StartNow()
            //        .WithSimpleSchedule(x => x
            //            .WithIntervalInHours(6)
            //            .RepeatForever())
            //        .Build();
            #endregion

            #endregion

            #region CleanInvalidVerifyCode every 1 hours
            var CleanInvalidVerifyCodeJob = JobBuilder.Create<CleanVerifyCodeJob>()
                                                      .WithIdentity("CleanVerifyCodeJob", AppConstants.DefaultTaskGroup)
                                                      .Build();
            scheduler.ScheduleJob(CleanInvalidVerifyCodeJob, OneHourTriggerForever);

            #endregion

            #region KeepAliveJob

            // Ping the site every 5 minutes to keep it alive
            var keepAliveJob = JobBuilder.Create<KeepAliveJob>()
                        .WithIdentity("KeepAliveJob", AppConstants.DefaultTaskGroup)
                        .Build();

            scheduler.ScheduleJob(keepAliveJob, fiveMinuteTriggerForever);

            #endregion

            #region 无效定时Job 

            #region Mark As Solution Job

            // Send mark as solution reminder emails
            //var markAsSolutionReminderJob = JobBuilder.Create<MarkAsSolutionReminderJob>()
            //                                          .WithIdentity("MarkAsSolutionReminderJob", AppConstants.DefaultTaskGroup)
            //                                          .Build();
            //scheduler.ScheduleJob(markAsSolutionReminderJob, sixHourTriggerForever);

            #endregion

            #region Send Email Job   // Send emails every 15 seconds
            //var emailJob = JobBuilder.Create<EmailJob>()
            //                         .WithIdentity("EmailJob", AppConstants.DefaultTaskGroup)
            //                         .Build();
            //scheduler.ScheduleJob(emailJob, fifteenSecondsTriggerForever);
            #endregion

            #endregion

            // Starting scheduler
            scheduler.Start();
        }
    }
}