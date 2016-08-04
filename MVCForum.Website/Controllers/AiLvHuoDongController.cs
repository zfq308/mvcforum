using MVCForum.Domain.Constants;
using MVCForum.Domain.DomainModel;
using MVCForum.Domain.Interfaces;
using MVCForum.Domain.Interfaces.Services;
using MVCForum.Domain.Interfaces.UnitOfWork;
using MVCForum.Services;
using MVCForum.Services.Data.Context;
using MVCForum.Website.Application.ActionFilterAttributes;
using MVCForum.Website.Areas.Admin.ViewModels;
using MVCForum.Website.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MVCForum.Website.ViewModels.Mapping;

namespace MVCForum.Website.Controllers
{
    public class AiLvHuoDongController : BaseController
    {



        private readonly IAiLvHuoDongService _aiLvHuoDongService;
        private readonly ITopicService _topicService;
        private readonly ICategoryService _categoryservice;
        private readonly MVCForumContext _context;
        private readonly IActivityRegisterService _ActivityRegisterService;

        #region 建构式  
        public AiLvHuoDongController(
            IMVCForumContext context,
            ICategoryService Categoryservice,
            ITopicService TopicService,
            IAiLvHuoDongService aiLvHuoDongService,
            IActivityRegisterService ActivityRegisterService,

            ILoggingService loggingService,
            IUnitOfWorkManager unitOfWorkManager,
            IMembershipService membershipService,
            ILocalizationService localizationService,
            IRoleService roleService,
            ISettingsService settingsService)
             : base(loggingService, unitOfWorkManager, membershipService, localizationService, roleService, settingsService)
        {
            _aiLvHuoDongService = aiLvHuoDongService;
            _ActivityRegisterService = ActivityRegisterService;
            _topicService = TopicService;
            _categoryservice = Categoryservice;
            _context = context as MVCForumContext;
        }

        #endregion

        #region 爱驴活动模块

        /// <summary>
        /// 活动List清单
        /// </summary>
        /// <returns></returns>
        public ActionResult ZuiXinHuoDong()
        {
            var HuoDongList = new AiLvHuoDong_ListViewModel
            {
                AiLvHuoDongList = _aiLvHuoDongService.GetAll()
            };
            return View(HuoDongList);
        }

        private void BindControlData()
        {
            var Items_LeiBieList = new List<SelectListItem>();
            Items_LeiBieList.Add(new SelectListItem { Text = "自由报名", Value = "1" });
            Items_LeiBieList.Add(new SelectListItem { Text = "特殊邀请", Value = "2" });
            ViewData["LeiBieList"] = Items_LeiBieList;


            var Items_YaoQiuList = new List<SelectListItem>();
            Items_YaoQiuList.Add(new SelectListItem { Text = "单身人士", Value = "1" });
            Items_YaoQiuList.Add(new SelectListItem { Text = "邀请人员", Value = "2" });
            Items_YaoQiuList.Add(new SelectListItem { Text = "无要求", Value = "3" });
            ViewData["YaoQiuList"] = Items_YaoQiuList;
        }

        #region 创建活动

        [Authorize]
        public ActionResult CreateAiLvHuoDong()
        {
            using (UnitOfWorkManager.NewUnitOfWork())
            {
                var loggedOnUserId = LoggedOnReadOnlyUser?.Id ?? Guid.Empty;
                var permissions = RoleService.GetPermissions(null, UsersRole);
                // Check is has permissions
                if (UserIsAdmin)
                {
                    var user = MembershipService.GetUser(loggedOnUserId);
                    BindControlData();

                    var model = new AiLvHuoDong_CreateEdit_ViewModel();
                    model.StartTime = DateTime.Today.AddDays(14).AddHours(8);
                    model.StopTime = DateTime.Today.AddDays(15);
                    model.BaoMingJieZhiTime = DateTime.Today.AddDays(10).AddHours(20);
                    model.LeiBie = Enum_HuoDongLeiBie.FreeRegister;

                    return View(model);
                }

                return ErrorToHomePage(LocalizationService.GetResourceString("Errors.NoPermission"));
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Supplier")]
        [ValidateAntiForgeryToken]
        public ActionResult CreateAiLvHuoDong(AiLvHuoDong_CreateEdit_ViewModel ViewModel)
        {
            if (ModelState.IsValid)
            {
                using (var unitOfWork = UnitOfWorkManager.NewUnitOfWork())
                {
                    var item = new AiLvHuoDong();
                    item.MingCheng = ViewModel.MingCheng;
                    item.LeiBie = ViewModel.LeiBie;
                    item.YaoQiu = ViewModel.YaoQiu;
                    item.StartTime = ViewModel.StartTime;
                    item.StopTime = ViewModel.StopTime;
                    item.BaoMingJieZhiTime = ViewModel.BaoMingJieZhiTime;
                    item.DiDian = ViewModel.DiDian;
                    item.LiuCheng = ViewModel.LiuCheng;
                    item.Feiyong = ViewModel.Feiyong;
                    item.FeiyongShuoMing = ViewModel.FeiyongShuoMing;
                    item.ZhuYiShiXiang = ViewModel.ZhuYiShiXiang;
                    item.YuGuRenShu = ViewModel.YuGuRenShu;
                    item.XingBieBiLi = ViewModel.XingBieBiLi;
                    item.YaoQingMa = string.IsNullOrEmpty(ViewModel.YaoQingMa) ? "" : ViewModel.YaoQingMa;
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

                    try
                    {
                        EntityOperationUtils.InsertObject(item);
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
                    return View(ViewModel);
                }
            }
            else
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors);
                return View(ViewModel);
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public ActionResult ClearAiLvHuodong()
        {
            //TODO: 待完成
            return RedirectToAction("ZuiXinHuoDong", "AiLvHuoDong");
        }


        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public ActionResult CreateTestAiLvHuoDongData()
        {
            using (var unitOfWork = UnitOfWorkManager.NewUnitOfWork())
            {

                #region 第一个case：管理员创建的公开参加的3人无性别限定3日游。不要邀请码，超过3人（要求已审核的会员）后不能报名。

                var item = new AiLvHuoDong();
                item.MingCheng = "管理员创建的公开参加的3人无性别限定3日游";
                item.LeiBie =  Enum_HuoDongLeiBie.FreeRegister;
                item.YaoQiu = Enum_HuoDongYaoQiu.None;
                item.StartTime = DateTime.Today.AddDays(7).AddHours(8);
                item.StopTime = DateTime.Today.AddDays(10).AddHours(8);
                item.BaoMingJieZhiTime = DateTime.Today.AddDays(1).AddHours(8);
                item.DiDian ="火星基地";
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
                item7.MingCheng = "管理员创建的公开参加的3人无性别限定3日游";
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
                if (UserIsAdmin)
                {
                    item7.ShenHeBiaoZhi = Enum_ShenHeBiaoZhi.AuditSuccess;
                    item7.AuditComments = "";
                }
                else
                {
                    item7.ShenHeBiaoZhi = Enum_ShenHeBiaoZhi.WaitingAudit;
                    item7.AuditComments = "";
                }
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
                if (UserIsAdmin)
                {
                    item8.ShenHeBiaoZhi = Enum_ShenHeBiaoZhi.AuditSuccess;
                    item8.AuditComments = "";
                }
                else
                {
                    item8.ShenHeBiaoZhi = Enum_ShenHeBiaoZhi.WaitingAudit;
                    item8.AuditComments = "";
                }
                item8.CreatedTime = DateTime.Now;
                #endregion

                #region 第九个Case：管理员创建的邀请参加的3人有性别限定3日游。必须填入邀请码ABCDE,限1男2女（要求已审核的会员）参加，超过人数后不能报名。
                var item9 = new AiLvHuoDong();
                item9.MingCheng = "管理员创建的邀请参加的3人有性别限定3日游";
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
                if (UserIsAdmin)
                {
                    item9.ShenHeBiaoZhi = Enum_ShenHeBiaoZhi.AuditSuccess;
                    item9.AuditComments = "";
                }
                else
                {
                    item9.ShenHeBiaoZhi = Enum_ShenHeBiaoZhi.WaitingAudit;
                    item9.AuditComments = "";
                }
                item9.CreatedTime = DateTime.Now;
                #endregion

                #region 第十个Case：管理员创建的邀请参加的3人无性别限定3日游。必须填入邀请码ABCDE, 超过3人（要求已审核的会员）后不能报名。
                var item10 = new AiLvHuoDong();
                item10.MingCheng = "管理员创建的邀请参加的3人无性别限定3日游";
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

                #region 第11个Case：管理员创建的邀请参加的单身3人无性别限定3日游。必须填入邀请码ABCDE, 限3名单身会员（要求已审核的会员）报名，超过3人（要求已审核的会员）后，都不能报名。
                var item11 = new AiLvHuoDong();
                item11.MingCheng = "管理员创建的邀请参加的单身3人无性别限定3日游";
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

                #region 第12个Case：管理员创建的邀请参加的特别邀请3人有性别限定3日游。必须填入邀请码ABCDE,限1男2女（要求已审核的会员）的单身会员后参加，超过人数后不能报名。
                var item12 = new AiLvHuoDong();
                item12.MingCheng = "管理员创建的邀请参加的特别邀请3人有性别限定单身会员3日游";
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

        #region 编辑活动

        [HttpPost]
        [Authorize(Roles = "Admin,Supplier")]
        [ValidateAntiForgeryToken]
        public ActionResult EditAiLvHuoDong(AiLvHuoDong_CreateEdit_ViewModel ViewModel)
        {
            if (ModelState.IsValid)
            {

                using (var unitOfWork = UnitOfWorkManager.NewUnitOfWork())
                {
                    var item = _aiLvHuoDongService.Get(ViewModel.Id);
                    if (item != null)
                    {
                        item.MingCheng = ViewModel.MingCheng;
                        item.LeiBie = ViewModel.LeiBie;
                        item.YaoQiu = ViewModel.YaoQiu;
                        item.StartTime = ViewModel.StartTime;
                        item.StopTime = ViewModel.StopTime;
                        item.BaoMingJieZhiTime = ViewModel.BaoMingJieZhiTime;
                        item.DiDian = ViewModel.DiDian;
                        item.LiuCheng = ViewModel.LiuCheng;
                        item.Feiyong = ViewModel.Feiyong;
                        item.FeiyongShuoMing = ViewModel.FeiyongShuoMing;
                        item.ZhuYiShiXiang = ViewModel.ZhuYiShiXiang;
                        item.YuGuRenShu = ViewModel.YuGuRenShu;
                        item.XingBieBiLi = ViewModel.XingBieBiLi;
                        item.YaoQingMa = ViewModel.YaoQingMa;
                        item.ZhuangTai = ViewModel.ZhuangTai;
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
                        EntityOperationUtils.ModifyObject(item);
                    }

                    try
                    {

                        unitOfWork.Commit();
                        TempData[AppConstants.MessageViewBagName] = new GenericMessageViewModel
                        {
                            Message = "活动已更新。",
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
                    return View(ViewModel);

                }
            }
            else
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors);
                return View(ViewModel);
            }
        }

        //[Authorize(Roles = "Admin,Supplier")]
        public ActionResult EditAiLvHuoDong(Guid Id)
        {
            var item = _aiLvHuoDongService.Get(Id);
            var EditModel = new AiLvHuoDong_CreateEdit_ViewModel
            {
                Id = item.Id,
                MingCheng = item.MingCheng,
                LeiBie = item.LeiBie,
                YaoQiu = item.YaoQiu,
                StartTime = item.StartTime,
                StopTime = item.StopTime,
                BaoMingJieZhiTime = item.BaoMingJieZhiTime,
                DiDian = item.DiDian,
                LiuCheng = item.LiuCheng,
                Feiyong = item.Feiyong,
                FeiyongShuoMing = item.FeiyongShuoMing,
                ZhuYiShiXiang = item.ZhuYiShiXiang,
                YuGuRenShu = item.YuGuRenShu,
                XingBieBiLi = item.XingBieBiLi,
                YaoQingMa = item.YaoQingMa,
                ZhuangTai = item.ZhuangTai,
                ShenHeBiaoZhi = item.ShenHeBiaoZhi,
            };
            BindControlData();
            return View(EditModel);
        }

        #endregion

        #region 删除活动

        [Authorize(Roles = "Admin,Supplier")]
        public ActionResult DeleteAiLvHuoDong(Guid Id)
        {
            var item = _aiLvHuoDongService.Get(Id);
            _aiLvHuoDongService.Delete(item);
            _context.Entry<AiLvHuoDong>(item).State = EntityState.Deleted;
            try
            {
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                LoggingService.Error(ex);
            }
            return RedirectToAction("ZuiXinHuoDong");

        }

        #endregion

        #region 活动审核

        [Authorize(Roles = "Admin")]
        [BasicMultiButton("btn_AuditSuccess")]
        public ActionResult AuditAiLvHuodong_Success(Guid Id)
        {
            var item = _aiLvHuoDongService.Get(Id);
            _aiLvHuoDongService.AuditAiLvHuodong(item, true);
            _context.Entry<AiLvHuoDong>(item).State = EntityState.Modified;
            try
            {
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                LoggingService.Error(ex);
            }
            return RedirectToAction("ZuiXinHuoDong");

        }

        [Authorize(Roles = "Admin")]
        [BasicMultiButton("btn_AuditFail")]
        public ActionResult AuditAiLvHuodong_Fail(Guid Id)
        {
            var item = _aiLvHuoDongService.Get(Id);
            _aiLvHuoDongService.AuditAiLvHuodong(item, false);
            _context.Entry<AiLvHuoDong>(item).State = EntityState.Modified;
            try
            {
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                LoggingService.Error(ex);
            }
            return RedirectToAction("ZuiXinHuoDong");

        }
        #endregion

        #endregion


        #region 活动报名
        /// <summary>
        /// 参与活动报名
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [Authorize()]
        [HttpPost]
        public ActionResult CreateActivityRegister(AiLvHuoDong_CreateEdit_ViewModel model)
        {
            string YaoQingMa = Request.Form["YaoQingMa"];
            string Idstr = Request.Form["Id"];

            using (var unitOfWork = UnitOfWorkManager.NewUnitOfWork())
            {
                Guid Id = Guid.Parse(Idstr);
                var huodong = _aiLvHuoDongService.Get(Id);
                var ar = new ActivityRegister(Id, LoggedOnReadOnlyUser);

                ar.UserTelphone = LoggedOnReadOnlyUser.MobilePhone;
                ar.FeeSource = "WeChat";
                ar.FeeId = "000000";
                ar.FeeNumber = huodong.Feiyong;


                switch (_ActivityRegisterService.CheckRegisterStatus(huodong, LoggedOnReadOnlyUser))
                {
                    case Enum_VerifyActivityRegisterStatus.Success:
                        //_ActivityRegisterService.Add(ar);
                        EntityOperationUtils.InsertObject(ar);
                        break;
                    case Enum_VerifyActivityRegisterStatus.Fail_BeyondDeadlineTime:
                        TempData[AppConstants.MessageViewBagName] = new GenericMessageViewModel
                        {
                            Message = "已超过报名时限。",
                            MessageType = GenericMessages.danger
                        };
                        return RedirectToAction("ZuiXinHuoDong", "AiLvHuoDong");
                    case Enum_VerifyActivityRegisterStatus.Fail_VerifyMarriedStatus:
                        TempData[AppConstants.MessageViewBagName] = new GenericMessageViewModel
                        {
                            Message = "此活动限未婚人士参加。",
                            MessageType = GenericMessages.danger
                        };
                        return RedirectToAction("ZuiXinHuoDong", "AiLvHuoDong");
                    case Enum_VerifyActivityRegisterStatus.Fail_VerifyUserGender:
                        TempData[AppConstants.MessageViewBagName] = new GenericMessageViewModel
                        {
                            Message = "此活动同性别名额已满员。",
                            MessageType = GenericMessages.danger
                        };
                        return RedirectToAction("ZuiXinHuoDong", "AiLvHuoDong");

                    case Enum_VerifyActivityRegisterStatus.Fail_VerifyUserApproveStatus:
                        TempData[AppConstants.MessageViewBagName] = new GenericMessageViewModel
                        {
                            Message = "您的账号还未通过管理员审核，请等待管理员审核后再注册精彩的爱驴活动。",
                            MessageType = GenericMessages.danger
                        };
                        return RedirectToAction("ZuiXinHuoDong", "AiLvHuoDong");
                    default:
                        break;
                }


                try
                {
                    unitOfWork.Commit();
                    TempData[AppConstants.MessageViewBagName] = new GenericMessageViewModel
                    {
                        Message = "活动已报名。",
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

        #region 爱驴记录模块

        /// <summary>
        /// 最新记录
        /// </summary>
        /// <returns></returns>
        public ActionResult ZuiXinJilu()
        {
            var JiluList = new AiLvJiLu_ListViewModel { Topics = new List<TopicViewModel>() };
            var topics = _topicService.GetAllTopicsByCategory(EnumCategoryType.AiLvJiLu);
            if (topics != null && topics.Count > 0)
            {
                var settings = SettingsService.GetSettings();
                var allowedCategories = new List<Category>();
                allowedCategories.Add(_categoryservice.GetCategoryByEnumCategoryType(EnumCategoryType.AiLvJiLu));
                var topicViewModels = ViewModelMapping.CreateTopicViewModels(topics.ToList(), RoleService, UsersRole, LoggedOnReadOnlyUser, allowedCategories, settings);
                JiluList.Topics.AddRange(topicViewModels);
            }
            return View(JiluList);
        }


        [Authorize(Roles = "Admin")]
        [HttpPost]
        public ActionResult CreateAiLvHuodongJilu(Guid Id)
        {
            var item = _aiLvHuoDongService.Get(Id);

            var model = new CreateEditTopicViewModel();

            var category = _categoryservice.GetCategoryByEnumCategoryType(EnumCategoryType.AiLvJiLu);
            model.CategoryId = category.Id;
            var allowedCategories = new List<Category>();
            allowedCategories.Add(category);
            model.Categories = _categoryservice.GetBaseSelectListCategories(allowedCategories);

            var permissions = RoleService.GetPermissions(null, UsersRole);
            var canInsertImages = UserIsAdmin;
            if (!canInsertImages)
            {
                canInsertImages = permissions[SiteConstants.Instance.PermissionInsertEditorImages].IsTicked;
            }

            model.OptionalPermissions = new CheckCreateTopicPermissions
            {
                CanLockTopic = UserIsAdmin,
                CanStickyTopic = UserIsAdmin,
                CanUploadFiles = UserIsAdmin,
                CanCreatePolls = UserIsAdmin,
                CanInsertImages = canInsertImages
            };
            model.PollAnswers = new List<PollAnswer>();
            model.IsTopicStarter = true;
            model.PollCloseAfterDays = 0;

            model.Name = "【" + item.MingCheng.Trim() + "】的活动记录";
            model.Content = "请更新" + model.Name;

            var checkexistHuodongJilu = _topicService.GetAllTopicsByCategory(category.Id).Where(x => x.Name == model.Name).Count();
            if (checkexistHuodongJilu == 0)
            {
                //Benjamin, 以POST方式主动提交Action
                TempData["model"] = model;
                return RedirectToAction("CreateNewTopicRecord", "Topic", new { model });
            }
            else
            {
                TempData[AppConstants.MessageViewBagName] = new GenericMessageViewModel
                {
                    Message = "活动记录已存在。",
                    MessageType = GenericMessages.danger
                };
                return RedirectToAction("ZuiXinHuoDong", "AiLvHuoDong");
            }
        }

        #endregion

        #region 爱驴资讯模块

        public ActionResult ZuiXinZiXun()
        {
            var ziXunList = new AiLvZiXun_ListViewModel { Topics = new List<TopicViewModel>() };
            var topics = _topicService.GetAllTopicsByCategory(EnumCategoryType.AiLvZiXun);
            if (topics != null && topics.Count > 0)
            {
                var settings = SettingsService.GetSettings();
                var allowedCategories = new List<Category>();
                allowedCategories.Add(_categoryservice.GetCategoryByEnumCategoryType(EnumCategoryType.AiLvZiXun));
                var topicViewModels = ViewModelMapping.CreateTopicViewModels(topics.ToList(), RoleService, UsersRole, LoggedOnReadOnlyUser, allowedCategories, settings);
                ziXunList.Topics.AddRange(topicViewModels);
            }
            return View(ziXunList);
        }

        #endregion

        #region 爱驴服务模块

        public ActionResult ZuiXinFuWu()
        {
            var fuwuList = new AiLvFuWu_ListViewModel { Topics = new List<TopicViewModel>() };
            var topics = _topicService.GetAllTopicsByCategory(EnumCategoryType.AiLvFuWu);
            if (topics != null && topics.Count > 0)
            {
                var settings = SettingsService.GetSettings();
                var allowedCategories = new List<Category>();
                allowedCategories.Add(_categoryservice.GetCategoryByEnumCategoryType(EnumCategoryType.AiLvFuWu));
                var topicViewModels = ViewModelMapping.CreateTopicViewModels(topics.ToList(), RoleService, UsersRole, LoggedOnReadOnlyUser, allowedCategories, settings);
                fuwuList.Topics.AddRange(topicViewModels);
            }
            return View(fuwuList);
        }

        #endregion

        #region 其他View视图

        public ActionResult ViewActivity(Guid Id)
        {
            var item = _aiLvHuoDongService.Get(Id);
            var EditModel = new AiLvHuoDong_CreateEdit_ViewModel
            {
                Id = item.Id,
                MingCheng = item.MingCheng,
                LeiBie = item.LeiBie,
                YaoQiu = item.YaoQiu,
                StartTime = item.StartTime,
                StopTime = item.StopTime,
                BaoMingJieZhiTime = item.BaoMingJieZhiTime,
                DiDian = item.DiDian,
                LiuCheng = item.LiuCheng,
                Feiyong = item.Feiyong,
                FeiyongShuoMing = item.FeiyongShuoMing,
                ZhuYiShiXiang = item.ZhuYiShiXiang,
                YuGuRenShu = item.YuGuRenShu,
                XingBieBiLi = item.XingBieBiLi,
                YaoQingMa = item.YaoQingMa,
                ZhuangTai = item.ZhuangTai,
                ShenHeBiaoZhi = item.ShenHeBiaoZhi,
                GongYingShangUserId = item.GongYingShangUserId,
            };
            return View(EditModel);
        }

        /// <summary>
        /// 每日之星
        /// </summary>
        /// <returns></returns>
        public ActionResult MeiRiZhiXing()
        {
            return View();
        }

        /// <summary>
        /// 最新会员
        /// </summary>
        /// <returns></returns>
        public ActionResult ZuiXinHuiYuan()
        {
            return View();
        }

        /// <summary>
        /// 爱驴账户
        /// </summary>
        /// <returns></returns>
        public ActionResult AiLvZhangHu()
        {
            return View();
        }

        #endregion
    }
}