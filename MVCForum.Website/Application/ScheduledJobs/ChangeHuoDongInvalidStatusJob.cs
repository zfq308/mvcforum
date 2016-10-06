using System;
using System.Linq;
using System.Collections.Generic;
using MVCForum.Domain.Interfaces.Services;
using MVCForum.Domain.Interfaces.UnitOfWork;
using Quartz;

namespace MVCForum.Website.Application.ScheduledJobs
{

    /// <summary>
    /// 定期变更活动的状态标志
    /// </summary>
    public class ChangeHuoDongInvalidStatusJob : IJob
    {
        //log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        log4net.ILog logger = log4net.LogManager.GetLogger("SchedulerLogs");

        private readonly IUnitOfWorkManager _unitOfWorkManager;
        private readonly ILoggingService _loggingService;
        private readonly IAiLvHuoDongService _aiLvHuoDongService;


        public ChangeHuoDongInvalidStatusJob(IUnitOfWorkManager UnitOfWorkManager, ILoggingService LoggingService, IAiLvHuoDongService AiLvHuoDongService)
        {
            _unitOfWorkManager = UnitOfWorkManager;
            _loggingService = LoggingService;
            _aiLvHuoDongService = AiLvHuoDongService;

        }
        public void Execute(IJobExecutionContext context)
        {
            using (var unitOfWork = _unitOfWorkManager.NewUnitOfWork())
            {
                try
                {
                    _aiLvHuoDongService.Update_ZhuangTai();

                    logger.Info("ChangeHuoDongInvalidStatusJob executed.");

                    unitOfWork.Commit();
                }
                catch (Exception ex)
                {
                    unitOfWork.Rollback();
                    _loggingService.Error(ex);
                }
            }
        }
    }

}