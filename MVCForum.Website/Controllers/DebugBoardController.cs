using MVCForum.Domain.Interfaces;
using MVCForum.Domain.Interfaces.Services;
using MVCForum.Domain.Interfaces.UnitOfWork;
using MVCForum.Services.Data.Context;
using MVCForum.Services.Data.UnitOfWork;
using MVCForum.Website.Application.ActionFilterAttributes;
using MVCForum.Website.Areas.Admin.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace MVCForum.Website.Controllers
{
    public class DebugBoardController : BaseController
    {
        log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private readonly MVCForumContext _context;

        #region 建构式

        public DebugBoardController(IMVCForumContext context,
            ILoggingService loggingService,
            IUnitOfWorkManager unitOfWorkManager,
            IMembershipService membershipService,
            ILocalizationService localizationService,
            IRoleService roleService,
            ISettingsService settingsService)
            : base(loggingService, unitOfWorkManager, membershipService, localizationService, roleService, settingsService)
        {
            _context = context as MVCForumContext;


        }

        #endregion

        // GET: DebugBoard
        public ActionResult Index()
        {
            return View();
        }

        #region 生成测试账号
        [HttpPost]
        [BasicMultiButton("Btn_Generate5SupplierAccount")]
        public ActionResult GenerateTestAccount()
        {
            Stopwatch MyStopWatch = new Stopwatch(); //性能计时器
            MyStopWatch.Start(); //启动计时器

            using (var unitOfWork = UnitOfWorkManager.NewUnitOfWork())
            {
                try
                {
                    MembershipService.Create5SupplierAccount();
                    unitOfWork.Commit();
                }
                catch (Exception ex)
                {
                    unitOfWork.Rollback();
                    LoggingService.Error(ex);
                    FormsAuthentication.SignOut();
                    ModelState.AddModelError(string.Empty, LocalizationService.GetResourceString("Errors.GenericMessage"));
                }
            }

            ShowMessage(new GenericMessageViewModel
            {
                //Message = LocalizationService.GetResourceString("Member.ProfileUpdated"),
                Message = "生成5个供应商测试账号执行完毕",
                MessageType = GenericMessages.success
            });

            MyStopWatch.Stop();
            decimal t = MyStopWatch.ElapsedMilliseconds;
            logger.Debug(string.Format("生成5个供应商测试账号执行完毕.Timecost:{0} seconds.", t / 1000));

            return RedirectToAction("Register", "Members");
            //return Content("生成5个供应商测试账号执行完毕");
        }

        [HttpPost]
        [BasicMultiButton("Btn_Generate50TestAccount")]
        public ActionResult GenerateTestAccount2()
        {
            Stopwatch MyStopWatch = new Stopwatch(); //性能计时器
            MyStopWatch.Start(); //启动计时器

            using (var unitOfWork = UnitOfWorkManager.NewUnitOfWork())
            {
                try
                {
                    MembershipService.Create50TestAccount();
                    unitOfWork.Commit();
                }
                catch (Exception ex)
                {
                    unitOfWork.Rollback();
                    LoggingService.Error(ex);
                    FormsAuthentication.SignOut();
                    ModelState.AddModelError(string.Empty, LocalizationService.GetResourceString("Errors.GenericMessage"));
                }
            }
            ShowMessage(new GenericMessageViewModel
            {
                //Message = LocalizationService.GetResourceString("Member.ProfileUpdated"),
                Message = "生成50个测试账号执行完毕",
                MessageType = GenericMessages.success
            });

            MyStopWatch.Stop();
            decimal t = MyStopWatch.ElapsedMilliseconds;
            logger.Debug(string.Format("生成50个测试账号执行完毕.Timecost:{0} seconds.", t / 1000));
            return RedirectToAction("Register", "Members");
            //return Content("生成50个测试账号执行完毕");
        }






        [HttpPost]
        [BasicMultiButton("Btn_GenerateAiLvHuodong")]
        public ActionResult GenerateAiLvHuodong()
        {
            using (var unitOfWork = UnitOfWorkManager.NewUnitOfWork())
            {
                try
                {
                    MembershipService.Create50TestAccount();
                    unitOfWork.Commit();
                }
                catch (Exception ex)
                {
                    unitOfWork.Rollback();
                    LoggingService.Error(ex);
                    FormsAuthentication.SignOut();
                    ModelState.AddModelError(string.Empty, LocalizationService.GetResourceString("Errors.GenericMessage"));
                }
            }
            ShowMessage(new GenericMessageViewModel
            {
                //Message = LocalizationService.GetResourceString("Member.ProfileUpdated"),
                Message = "爱驴活动的测试数据已生成完毕。",
                MessageType = GenericMessages.success
            });
            return RedirectToAction("ZuiXinHuoDong", "AiLvHuoDong");
        }


        #endregion
    }
}