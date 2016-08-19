using MVCForum.Domain.Constants;
using MVCForum.Domain.DomainModel;
using MVCForum.Domain.Interfaces;
using MVCForum.Domain.Interfaces.Services;
using MVCForum.Domain.Interfaces.UnitOfWork;
using MVCForum.Services.Data.Context;
using MVCForum.Services.Data.UnitOfWork;
using MVCForum.Website.Application.ActionFilterAttributes;
using MVCForum.Website.Areas.Admin.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Entity;
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
        private readonly IAiLvHuoDongService _aiLvHuoDongService;

        #region 建构式

        public DebugBoardController(IMVCForumContext context,
            ILoggingService loggingService,
            IUnitOfWorkManager unitOfWorkManager,
            IMembershipService membershipService,
            ILocalizationService localizationService,
            IRoleService roleService,
            IAiLvHuoDongService aiLvHuoDongService,
            ISettingsService settingsService)
            : base(loggingService, unitOfWorkManager, membershipService, localizationService, roleService, settingsService)
        {
            _context = context as MVCForumContext;
            _aiLvHuoDongService = aiLvHuoDongService;

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
        [Authorize(Roles = "Admin")]
        //[ValidateAntiForgeryToken]
        public ActionResult ClearAiLvHuodong()
        {
            using (var unitOfWork = UnitOfWorkManager.NewUnitOfWork())
            {
                try
                {
                    var allHuodong = _aiLvHuoDongService.GetAll();
                    if (allHuodong != null && allHuodong.Count > 0)
                    {
                        foreach (var item in allHuodong)
                        {
                            _aiLvHuoDongService.Delete(item);
                            _context.Entry<AiLvHuoDong>(item).State = EntityState.Deleted;
                        }
                    }
                    _context.SaveChanges();
                    unitOfWork.Commit();
                    TempData[AppConstants.MessageViewBagName] = new GenericMessageViewModel
                    {
                        Message = "历史活动及其记录已清除。",
                        MessageType = GenericMessages.info
                    };
                    return RedirectToAction("ZuiXinHuoDong", "AiLvHuoDong");
                }
                catch (Exception ex)
                {
                    unitOfWork.Rollback();
                    LoggingService.Error(ex);
                    ModelState.AddModelError(string.Empty, LocalizationService.GetResourceString("Errors.GenericMessage"));
                }
                return View();

            }
            return RedirectToAction("ZuiXinHuoDong", "AiLvHuoDong");
        }


        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult CreateTestAiLvHuoDongData()
        {
            using (var unitOfWork = UnitOfWorkManager.NewUnitOfWork())
            {

                #region 第一个case：管理员创建的公开参加的3人无性别限定3日游。不要邀请码，超过3人（要求已审核的会员）后不能报名。

                var item = new AiLvHuoDong();
                item.MingCheng = "管理员创建的公开参加的3人无性别限定3日游";
                item.LeiBie = Enum_HuoDongLeiBie.FreeRegister;
                item.YaoQiu = Enum_HuoDongYaoQiu.None;
                item.StartTime = DateTime.Today.AddDays(7).AddHours(8);
                item.StopTime = DateTime.Today.AddDays(10).AddHours(8);
                item.BaoMingJieZhiTime = DateTime.Today.AddDays(1).AddHours(8);
                item.DiDian = "火星基地";
                item.LiuCheng = "先吃后睡，再吃再睡";
                item.Feiyong = 100;
                item.FeiyongShuoMing = "这是活动的费用说明。";
                item.ZhuYiShiXiang = "这是活动的注意事项说明。";
                item.YuGuRenShu = 3;
                item.XingBieBiLi = "/"; //无性别限定
                item.YaoQingMa = "";
                item.ZhuangTai = Enum_HuoDongZhuangTai.Registing;
                item.GongYingShangUserId = LoggedOnReadOnlyUser.Id.ToString();
                if (UserIsAdmin)
                {
                    item.ShenHeBiaoZhi = Enum_ShenHeBiaoZhi.AuditSuccess;
                    item.AuditComments = "";
                }
                else
                {
                    item.ShenHeBiaoZhi = Enum_ShenHeBiaoZhi.WaitingAudit;
                    item.AuditComments = "";
                }
                item.CreatedTime = DateTime.Now;

                #endregion

                #region 第二个Case：管理员创建的公开参加的3人有性别限定3日游。不要邀请码，限1男2女（要求已审核的会员）参加，超过人数后不能报名。
                var item2 = new AiLvHuoDong();
                item2.MingCheng = "管理员创建的公开参加的3人有性别限定3日游";
                item2.LeiBie = Enum_HuoDongLeiBie.FreeRegister;
                item2.YaoQiu = Enum_HuoDongYaoQiu.None;
                item2.StartTime = DateTime.Today.AddDays(7).AddHours(8);
                item2.StopTime = DateTime.Today.AddDays(10).AddHours(8);
                item2.BaoMingJieZhiTime = DateTime.Today.AddDays(1).AddHours(8);
                item2.DiDian = "火星基地";
                item2.LiuCheng = "先吃后睡，再吃再睡";
                item2.Feiyong = 100;
                item2.FeiyongShuoMing = "这是活动的费用说明。";
                item2.ZhuYiShiXiang = "这是活动的注意事项说明。";
                item2.YuGuRenShu = 3;
                item2.XingBieBiLi = "1:2"; //有性别限定
                item2.YaoQingMa = "";
                item2.ZhuangTai = Enum_HuoDongZhuangTai.Registing;
                item2.GongYingShangUserId = LoggedOnReadOnlyUser.Id.ToString();
                if (UserIsAdmin)
                {
                    item2.ShenHeBiaoZhi = Enum_ShenHeBiaoZhi.AuditSuccess;
                    item2.AuditComments = "";
                }
                else
                {
                    item2.ShenHeBiaoZhi = Enum_ShenHeBiaoZhi.WaitingAudit;
                    item2.AuditComments = "";
                }
                item2.CreatedTime = DateTime.Now;
                #endregion

                #region 第三个Case：管理员创建的邀请参加的3人有性别限定3日游。必须填入邀请码ABCDE,限1男2女（要求已审核的会员）参加，超过人数后不能报名。
                var item3 = new AiLvHuoDong();
                item3.MingCheng = "管理员创建的邀请参加的3人有性别限定3日游";
                item3.LeiBie = Enum_HuoDongLeiBie.SpecicalRegister;
                item3.YaoQiu = Enum_HuoDongYaoQiu.None;
                item3.StartTime = DateTime.Today.AddDays(7).AddHours(8);
                item3.StopTime = DateTime.Today.AddDays(10).AddHours(8);
                item3.BaoMingJieZhiTime = DateTime.Today.AddDays(1).AddHours(8);
                item3.DiDian = "火星基地";
                item3.LiuCheng = "先吃后睡，再吃再睡";
                item3.Feiyong = 100;
                item3.FeiyongShuoMing = "这是活动的费用说明。";
                item3.ZhuYiShiXiang = "这是活动的注意事项说明。";
                item3.YuGuRenShu = 3;
                item3.XingBieBiLi = "1:2"; //有性别限定
                item3.YaoQingMa = "ABCDE";
                item3.ZhuangTai = Enum_HuoDongZhuangTai.Registing;
                item3.GongYingShangUserId = LoggedOnReadOnlyUser.Id.ToString();
                if (UserIsAdmin)
                {
                    item3.ShenHeBiaoZhi = Enum_ShenHeBiaoZhi.AuditSuccess;
                    item3.AuditComments = "";
                }
                else
                {
                    item3.ShenHeBiaoZhi = Enum_ShenHeBiaoZhi.WaitingAudit;
                    item3.AuditComments = "";
                }
                item3.CreatedTime = DateTime.Now;
                #endregion

                #region 第四个Case：管理员创建的邀请参加的3人无性别限定3日游。必须填入邀请码ABCDE, 超过3人（要求已审核的会员）后不能报名。
                var item4 = new AiLvHuoDong();
                item4.MingCheng = "管理员创建的邀请参加的3人无性别限定3日游";
                item4.LeiBie = Enum_HuoDongLeiBie.SpecicalRegister;
                item4.YaoQiu = Enum_HuoDongYaoQiu.None;
                item4.StartTime = DateTime.Today.AddDays(7).AddHours(8);
                item4.StopTime = DateTime.Today.AddDays(10).AddHours(8);
                item4.BaoMingJieZhiTime = DateTime.Today.AddDays(1).AddHours(8);
                item4.DiDian = "火星基地";
                item4.LiuCheng = "先吃后睡，再吃再睡";
                item4.Feiyong = 100;
                item4.FeiyongShuoMing = "这是活动的费用说明。";
                item4.ZhuYiShiXiang = "这是活动的注意事项说明。";
                item4.YuGuRenShu = 3;
                item4.XingBieBiLi = "/"; //无性别限定
                item4.YaoQingMa = "ABCDE"; //邀请码
                item4.ZhuangTai = Enum_HuoDongZhuangTai.Registing;
                item4.GongYingShangUserId = LoggedOnReadOnlyUser.Id.ToString();
                if (UserIsAdmin)
                {
                    item4.ShenHeBiaoZhi = Enum_ShenHeBiaoZhi.AuditSuccess;
                    item4.AuditComments = "";
                }
                else
                {
                    item4.ShenHeBiaoZhi = Enum_ShenHeBiaoZhi.WaitingAudit;
                    item4.AuditComments = "";
                }
                item4.CreatedTime = DateTime.Now;
                #endregion

                #region 第五个Case：管理员创建的邀请参加的单身3人无性别限定3日游。必须填入邀请码ABCDE, 限3名单身会员（要求已审核的会员）报名，超过3人（要求已审核的会员）后，都不能报名。
                var item5 = new AiLvHuoDong();
                item5.MingCheng = "管理员创建的邀请参加的单身3人无性别限定3日游";
                item5.LeiBie = Enum_HuoDongLeiBie.SpecicalRegister;
                item5.YaoQiu = Enum_HuoDongYaoQiu.Single;
                item5.StartTime = DateTime.Today.AddDays(7).AddHours(8);
                item5.StopTime = DateTime.Today.AddDays(10).AddHours(8);
                item5.BaoMingJieZhiTime = DateTime.Today.AddDays(1).AddHours(8);
                item5.DiDian = "火星基地";
                item5.LiuCheng = "先吃后睡，再吃再睡";
                item5.Feiyong = 100;
                item5.FeiyongShuoMing = "这是活动的费用说明。";
                item5.ZhuYiShiXiang = "这是活动的注意事项说明。";
                item5.YuGuRenShu = 3;
                item5.XingBieBiLi = "/"; //无性别限定
                item5.YaoQingMa = "ABCDE"; //邀请码
                item5.ZhuangTai = Enum_HuoDongZhuangTai.Registing;
                item5.GongYingShangUserId = LoggedOnReadOnlyUser.Id.ToString();
                if (UserIsAdmin)
                {
                    item5.ShenHeBiaoZhi = Enum_ShenHeBiaoZhi.AuditSuccess;
                    item5.AuditComments = "";
                }
                else
                {
                    item5.ShenHeBiaoZhi = Enum_ShenHeBiaoZhi.WaitingAudit;
                    item5.AuditComments = "";
                }
                item5.CreatedTime = DateTime.Now;
                #endregion

                #region 第六个Case：管理员创建的邀请参加的特别邀请3人有性别限定3日游。必须填入邀请码ABCDE,限1男2女（要求已审核的会员）的单身会员后参加，超过人数后不能报名。
                var item6 = new AiLvHuoDong();
                item6.MingCheng = "管理员创建的邀请参加的特别邀请3人有性别限定单身会员3日游";
                item6.LeiBie = Enum_HuoDongLeiBie.SpecicalRegister;
                item6.YaoQiu = Enum_HuoDongYaoQiu.Single;
                item6.StartTime = DateTime.Today.AddDays(7).AddHours(8);
                item6.StopTime = DateTime.Today.AddDays(10).AddHours(8);
                item6.BaoMingJieZhiTime = DateTime.Today.AddDays(1).AddHours(8);
                item6.DiDian = "火星基地";
                item6.LiuCheng = "先吃后睡，再吃再睡";
                item6.Feiyong = 100;
                item6.FeiyongShuoMing = "这是活动的费用说明。";
                item6.ZhuYiShiXiang = "这是活动的注意事项说明。";
                item6.YuGuRenShu = 3;
                item6.XingBieBiLi = "1:2"; //有性别限定
                item6.YaoQingMa = "ABCDE"; //邀请码
                item6.ZhuangTai = Enum_HuoDongZhuangTai.Registing;
                item6.GongYingShangUserId = LoggedOnReadOnlyUser.Id.ToString();
                if (UserIsAdmin)
                {
                    item6.ShenHeBiaoZhi = Enum_ShenHeBiaoZhi.AuditSuccess;
                    item6.AuditComments = "";
                }
                else
                {
                    item6.ShenHeBiaoZhi = Enum_ShenHeBiaoZhi.WaitingAudit;
                    item6.AuditComments = "";
                }
                item6.CreatedTime = DateTime.Now;
                #endregion

                #region 第七个case：供应商创建的公开参加的3人无性别限定3日游。待审核。不要邀请码，超过3人（要求已审核的会员）后不能报名。

                var item7 = new AiLvHuoDong();
                item7.MingCheng = "供应商创建的公开参加的3人无性别限定3日游";
                item7.LeiBie = Enum_HuoDongLeiBie.FreeRegister;
                item7.YaoQiu = Enum_HuoDongYaoQiu.None;
                item7.StartTime = DateTime.Today.AddDays(7).AddHours(8);
                item7.StopTime = DateTime.Today.AddDays(10).AddHours(8);
                item7.BaoMingJieZhiTime = DateTime.Today.AddDays(1).AddHours(8);
                item7.DiDian = "火星基地";
                item7.LiuCheng = "先吃后睡，再吃再睡";
                item7.Feiyong = 100;
                item7.FeiyongShuoMing = "这是活动的费用说明。";
                item7.ZhuYiShiXiang = "这是活动的注意事项说明。";
                item7.YuGuRenShu = 3;
                item7.XingBieBiLi = "/"; //无性别限定
                item7.YaoQingMa = "";
                item7.ZhuangTai = Enum_HuoDongZhuangTai.Registing;
                item7.GongYingShangUserId = LoggedOnReadOnlyUser.Id.ToString();
                item7.ShenHeBiaoZhi = Enum_ShenHeBiaoZhi.WaitingAudit;  //待审核
                item7.AuditComments = "";
                item7.CreatedTime = DateTime.Now;

                #endregion

                #region 第八个Case：供应商创建的公开参加的3人有性别限定3日游。待审核。不要邀请码，限1男2女（要求已审核的会员）参加，超过人数后不能报名。
                var item8 = new AiLvHuoDong();
                item8.MingCheng = "供应商创建的公开参加的3人有性别限定3日游";
                item8.LeiBie = Enum_HuoDongLeiBie.FreeRegister;
                item8.YaoQiu = Enum_HuoDongYaoQiu.None;
                item8.StartTime = DateTime.Today.AddDays(7).AddHours(8);
                item8.StopTime = DateTime.Today.AddDays(10).AddHours(8);
                item8.BaoMingJieZhiTime = DateTime.Today.AddDays(1).AddHours(8);
                item8.DiDian = "火星基地";
                item8.LiuCheng = "先吃后睡，再吃再睡";
                item8.Feiyong = 100;
                item8.FeiyongShuoMing = "这是活动的费用说明。";
                item8.ZhuYiShiXiang = "这是活动的注意事项说明。";
                item8.YuGuRenShu = 3;
                item8.XingBieBiLi = "1:2"; //有性别限定
                item8.YaoQingMa = "";
                item8.ZhuangTai = Enum_HuoDongZhuangTai.Registing;
                item8.GongYingShangUserId = LoggedOnReadOnlyUser.Id.ToString();
                item8.ShenHeBiaoZhi = Enum_ShenHeBiaoZhi.WaitingAudit; //待审核
                item8.AuditComments = "";
                item8.CreatedTime = DateTime.Now;
                #endregion

                #region 第九个Case：供应商创建的邀请参加的3人有性别限定3日游。必须填入邀请码ABCDE,限1男2女（要求已审核的会员）参加，超过人数后不能报名。
                var item9 = new AiLvHuoDong();
                item9.MingCheng = "供应商创建的邀请参加的3人有性别限定3日游";
                item9.LeiBie = Enum_HuoDongLeiBie.SpecicalRegister;
                item9.YaoQiu = Enum_HuoDongYaoQiu.None;
                item9.StartTime = DateTime.Today.AddDays(7).AddHours(8);
                item9.StopTime = DateTime.Today.AddDays(10).AddHours(8);
                item9.BaoMingJieZhiTime = DateTime.Today.AddDays(1).AddHours(8);
                item9.DiDian = "火星基地";
                item9.LiuCheng = "先吃后睡，再吃再睡";
                item9.Feiyong = 100;
                item9.FeiyongShuoMing = "这是活动的费用说明。";
                item9.ZhuYiShiXiang = "这是活动的注意事项说明。";
                item9.YuGuRenShu = 3;
                item9.XingBieBiLi = "1:2"; //有性别限定
                item9.YaoQingMa = "ABCDE";
                item9.ZhuangTai = Enum_HuoDongZhuangTai.Registing;
                item9.GongYingShangUserId = LoggedOnReadOnlyUser.Id.ToString();
                item9.ShenHeBiaoZhi = Enum_ShenHeBiaoZhi.WaitingAudit;  //待审核
                item9.AuditComments = "";
                item9.CreatedTime = DateTime.Now;
                #endregion

                #region 第十个Case：供应商创建的邀请参加的3人无性别限定3日游。必须填入邀请码ABCDE, 超过3人（要求已审核的会员）后不能报名。
                var item10 = new AiLvHuoDong();
                item10.MingCheng = "供应商创建的邀请参加的3人无性别限定3日游";
                item10.LeiBie = Enum_HuoDongLeiBie.SpecicalRegister;
                item10.YaoQiu = Enum_HuoDongYaoQiu.None;
                item10.StartTime = DateTime.Today.AddDays(7).AddHours(8);
                item10.StopTime = DateTime.Today.AddDays(10).AddHours(8);
                item10.BaoMingJieZhiTime = DateTime.Today.AddDays(1).AddHours(8);
                item10.DiDian = "火星基地";
                item10.LiuCheng = "先吃后睡，再吃再睡";
                item10.Feiyong = 100;
                item10.FeiyongShuoMing = "这是活动的费用说明。";
                item10.ZhuYiShiXiang = "这是活动的注意事项说明。";
                item10.YuGuRenShu = 3;
                item10.XingBieBiLi = "/"; //无性别限定
                item10.YaoQingMa = "ABCDE"; //邀请码
                item10.ZhuangTai = Enum_HuoDongZhuangTai.Registing;
                item10.GongYingShangUserId = LoggedOnReadOnlyUser.Id.ToString();
                if (UserIsAdmin)
                {
                    item10.ShenHeBiaoZhi = Enum_ShenHeBiaoZhi.AuditSuccess;
                    item10.AuditComments = "";
                }
                else
                {
                    item10.ShenHeBiaoZhi = Enum_ShenHeBiaoZhi.WaitingAudit;
                    item10.AuditComments = "";
                }
                item10.CreatedTime = DateTime.Now;
                #endregion

                #region 第11个Case：供应商创建的邀请参加的单身3人无性别限定3日游。必须填入邀请码ABCDE, 限3名单身会员（要求已审核的会员）报名，超过3人（要求已审核的会员）后，都不能报名。
                var item11 = new AiLvHuoDong();
                item11.MingCheng = "供应商创建的邀请参加的单身3人无性别限定3日游";
                item11.LeiBie = Enum_HuoDongLeiBie.SpecicalRegister;
                item11.YaoQiu = Enum_HuoDongYaoQiu.Single;
                item11.StartTime = DateTime.Today.AddDays(7).AddHours(8);
                item11.StopTime = DateTime.Today.AddDays(10).AddHours(8);
                item11.BaoMingJieZhiTime = DateTime.Today.AddDays(1).AddHours(8);
                item11.DiDian = "火星基地";
                item11.LiuCheng = "先吃后睡，再吃再睡";
                item11.Feiyong = 100;
                item11.FeiyongShuoMing = "这是活动的费用说明。";
                item11.ZhuYiShiXiang = "这是活动的注意事项说明。";
                item11.YuGuRenShu = 3;
                item11.XingBieBiLi = "/"; //无性别限定
                item11.YaoQingMa = "ABCDE"; //邀请码
                item11.ZhuangTai = Enum_HuoDongZhuangTai.Registing;
                item11.GongYingShangUserId = LoggedOnReadOnlyUser.Id.ToString();
                if (UserIsAdmin)
                {
                    item11.ShenHeBiaoZhi = Enum_ShenHeBiaoZhi.AuditSuccess;
                    item11.AuditComments = "";
                }
                else
                {
                    item11.ShenHeBiaoZhi = Enum_ShenHeBiaoZhi.WaitingAudit;
                    item11.AuditComments = "";
                }
                item11.CreatedTime = DateTime.Now;
                #endregion

                #region 第12个Case：供应商创建的邀请参加的特别邀请3人有性别限定3日游。必须填入邀请码ABCDE,限1男2女（要求已审核的会员）的单身会员后参加，超过人数后不能报名。
                var item12 = new AiLvHuoDong();
                item12.MingCheng = "供应商创建的邀请参加的特别邀请3人有性别限定单身会员3日游";
                item12.LeiBie = Enum_HuoDongLeiBie.SpecicalRegister;
                item12.YaoQiu = Enum_HuoDongYaoQiu.Single;
                item12.StartTime = DateTime.Today.AddDays(7).AddHours(8);
                item12.StopTime = DateTime.Today.AddDays(10).AddHours(8);
                item12.BaoMingJieZhiTime = DateTime.Today.AddDays(1).AddHours(8);
                item12.DiDian = "火星基地";
                item12.LiuCheng = "先吃后睡，再吃再睡";
                item12.Feiyong = 100;
                item12.FeiyongShuoMing = "这是活动的费用说明。";
                item12.ZhuYiShiXiang = "这是活动的注意事项说明。";
                item12.YuGuRenShu = 3;
                item12.XingBieBiLi = "1:2"; //有性别限定
                item12.YaoQingMa = "ABCDE"; //邀请码
                item12.ZhuangTai = Enum_HuoDongZhuangTai.Registing;
                item12.GongYingShangUserId = LoggedOnReadOnlyUser.Id.ToString();
                if (UserIsAdmin)
                {
                    item12.ShenHeBiaoZhi = Enum_ShenHeBiaoZhi.AuditSuccess;
                    item12.AuditComments = "";
                }
                else
                {
                    item12.ShenHeBiaoZhi = Enum_ShenHeBiaoZhi.WaitingAudit;
                    item12.AuditComments = "";
                }
                item12.CreatedTime = DateTime.Now;
                #endregion


                try
                {
                    EntityOperationUtils.InsertObject(item);
                    EntityOperationUtils.InsertObject(item2);
                    EntityOperationUtils.InsertObject(item3);
                    EntityOperationUtils.InsertObject(item4);
                    EntityOperationUtils.InsertObject(item5);
                    EntityOperationUtils.InsertObject(item6);
                    EntityOperationUtils.InsertObject(item7);
                    EntityOperationUtils.InsertObject(item8);
                    EntityOperationUtils.InsertObject(item9);
                    EntityOperationUtils.InsertObject(item10);
                    EntityOperationUtils.InsertObject(item11);
                    EntityOperationUtils.InsertObject(item12);
                    unitOfWork.Commit();
                    TempData[AppConstants.MessageViewBagName] = new GenericMessageViewModel
                    {
                        Message = "活动已创建。",
                        MessageType = GenericMessages.info
                    };
                    return RedirectToAction("ZuiXinHuoDong", "AiLvHuoDong");
                }
                catch (Exception ex)
                {
                    unitOfWork.Rollback();
                    LoggingService.Error(ex);
                    ModelState.AddModelError(string.Empty, LocalizationService.GetResourceString("Errors.GenericMessage"));
                }
                return View();
            }


        }
        #endregion
    }
}