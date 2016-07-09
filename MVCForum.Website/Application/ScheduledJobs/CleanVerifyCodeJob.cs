using System;
using System.Linq;
using System.Collections.Generic;
using MVCForum.Domain.Interfaces.Services;
using MVCForum.Domain.Interfaces.UnitOfWork;
using Quartz;

namespace MVCForum.Website.Application.ScheduledJobs
{
    // 定期清理无效的验证码
    public class CleanVerifyCodeJob : IJob
    {
        log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IUnitOfWorkManager _unitOfWorkManager;
        private readonly ILoggingService _loggingService;
        private readonly IVerifyCodeService _verifyCodeService;

        public CleanVerifyCodeJob(IUnitOfWorkManager UnitOfWorkManager, ILoggingService LoggingService, IVerifyCodeService VerifyCodeService)
        {
            _unitOfWorkManager = UnitOfWorkManager;
            _loggingService = LoggingService;
            _verifyCodeService = VerifyCodeService;
        }

        public void Execute(IJobExecutionContext context)
        {
            using (var unitOfWork = _unitOfWorkManager.NewUnitOfWork())
            {
                try
                {
                    _verifyCodeService.CheckInvalidVerifyCode();

                    logger.Info("CleanVerifyCodeJob executed.");
                   
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