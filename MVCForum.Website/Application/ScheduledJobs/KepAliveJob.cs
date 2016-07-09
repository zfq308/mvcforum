using System;
using MVCForum.Domain.Interfaces.Services;
using MVCForum.Domain.Interfaces.UnitOfWork;
using Quartz;

namespace MVCForum.Website.Application.ScheduledJobs
{
    [DisallowConcurrentExecution]
    public class KeepAliveJob : IJob
    {
        private readonly ILoggingService _loggingService;
        private readonly IUnitOfWorkManager _unitOfWorkManager;
        private readonly ISettingsService _settingsService;

        public KeepAliveJob(ILoggingService loggingService, 
                            IUnitOfWorkManager unitOfWorkManager, 
                            ISettingsService settingsService)
        {
            _loggingService = loggingService;
            _unitOfWorkManager = unitOfWorkManager;
            _settingsService = settingsService;
        }

        public void Execute(IJobExecutionContext context)
        {
            using (_unitOfWorkManager.NewUnitOfWork())
            {
                try
                {
                    //TODO: 鉴于系统的首页有较大的计算和流量，可以考虑将KeepAlive访问其他页面
                    var url = _settingsService.GetSettings(false).ForumKeepAliveURL;
                    AppHelpers.Ping(url);
                }
                catch (Exception ex)
                {
                    _loggingService.Error(string.Concat("Error in KeepAlive job > ", ex.Message));
                }
            }
            
        }
    }
}