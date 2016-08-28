﻿using MVCForum.Domain.Constants;
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
using MVCForum.Website.Application;

namespace MVCForum.Website.Controllers
{
    public class AiLvHuoDongController : BaseController
    {
        #region 成员变量
        log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private readonly IAiLvHuoDongService _aiLvHuoDongService;
        private readonly ITopicService _topicService;
        private readonly ICategoryService _categoryservice;
        private readonly MVCForumContext _context;
        private readonly IActivityRegisterService _ActivityRegisterService;
        private readonly IMembershipService _MembershipService;
        private readonly IMembershipTodayStarService _membershipTodayStarService;

        #endregion

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
            IMembershipTodayStarService membershipTodayStarService,
            ISettingsService settingsService)
             : base(loggingService, unitOfWorkManager, membershipService, localizationService, roleService, settingsService)
        {
            _aiLvHuoDongService = aiLvHuoDongService;
            _ActivityRegisterService = ActivityRegisterService;
            _topicService = TopicService;
            _categoryservice = Categoryservice;
            _MembershipService = membershipService;
            _membershipTodayStarService = membershipTodayStarService;
            _context = context as MVCForumContext;
        }

        #endregion


        public ActionResult Index()
        {
            return View();
        }

        #region 爱驴活动模块

        #region 创建活动

        [Authorize]
        public ActionResult CreateAiLvHuoDong()
        {
            using (UnitOfWorkManager.NewUnitOfWork())
            {
                var loggedOnUserId = LoggedOnReadOnlyUser?.Id ?? Guid.Empty;
                var permissions = RoleService.GetPermissions(null, UsersRole);

                if (UserIsAdmin || User.IsInRole(AppConstants.SupplierRoleName))
                {
                    //绑定页面控件
                    BindControlData();
                    //给出部分默认值
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

                    #region 属性赋值
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
                    return View(ViewModel);
                }
            }
            else
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors);
                return View(ViewModel);
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

        #region 查看/载入活动
        /// <summary>
        /// 活动List清单
        /// </summary>
        /// <returns></returns>
        public ActionResult ZuiXinHuoDong()
        {
            var HuoDongList = new AiLvHuoDong_ListViewModel
            {
                AiLvHuoDongList = _aiLvHuoDongService.GetAll().OrderByDescending(x => x.CreatedTime).ToList()
            };
            return View(HuoDongList);
        }

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
                YaoQingMa = "", //item.YaoQingMa,
                ZhuangTai = item.ZhuangTai,
                ShenHeBiaoZhi = item.ShenHeBiaoZhi,
                GongYingShangUserId = item.GongYingShangUserId,
            };
            var userlist = _ActivityRegisterService.GetActivityRegisterListByHongDong(item);
            if (userlist != null && userlist.Count > 0)
            {
                List<Guid> BoyUserId = new List<Guid>();
                List<Guid> GirlUserId = new List<Guid>();
                EditModel.BoyJoinner = new List<MembershipUser>();
                EditModel.GirlJoiner = new List<MembershipUser>();
                foreach (ActivityRegister ar in userlist)
                {
                    if (ar.UserGender == Enum_Gender.boy)
                    {
                        if (!BoyUserId.Contains(ar.UserId))
                        {
                            BoyUserId.Add(ar.UserId);
                            MembershipUser user1 = _MembershipService.GetUser(ar.UserId);
                            if (user1 != null)
                            {
                                EditModel.BoyJoinner.Add(user1);
                            }
                        }
                    }
                    else
                    {
                        if (!GirlUserId.Contains(ar.UserId))
                        {
                            GirlUserId.Add(ar.UserId);
                            MembershipUser user1 = _MembershipService.GetUser(ar.UserId);
                            if (user1 != null)
                            {
                                EditModel.GirlJoiner.Add(user1);
                            }
                        }

                    }
                }
            }

            return View(EditModel);
        }

        #endregion

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

                if (huodong==null)
                {
                    TempData[AppConstants.MessageViewBagName] = new GenericMessageViewModel
                    {
                        Message = "参数错误，请联系管理员。",
                        MessageType = GenericMessages.danger
                    };
                    return RedirectToAction("ZuiXinHuoDong", "AiLvHuoDong");
                }


                if (huodong.LeiBie== Enum_HuoDongLeiBie.SpecicalRegister)
                {
                   if( huodong.YaoQingMa!= YaoQingMa)
                    {
                        TempData[AppConstants.MessageViewBagName] = new GenericMessageViewModel
                        {
                            Message = "邀请码不正确。",
                            MessageType = GenericMessages.danger
                        };
                        return RedirectToAction("ZuiXinHuoDong", "AiLvHuoDong");
                    }
                }
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

        [Authorize]
        [HttpPost]
        public ActionResult ExportHuoDongUsers(Guid id)
        {
            using (UnitOfWorkManager.NewUnitOfWork())
            {
                List<MembershipUser> customers = null;
                var collection = _ActivityRegisterService.GetActivityRegisterListByHongDongId(id).ToList();
                if (collection != null && collection.Count > 0)
                {
                    customers = new List<MembershipUser>();
                    foreach (var item in collection)
                    {
                        MembershipUser user = _MembershipService.GetUser(item.UserId);
                        if (!customers.Contains(user))
                        {
                            customers.Add(user);
                        }
                    }
                }
                return new CsvFileResult { FileDownloadName = "MVCForumUsers.csv", Body = MembershipService.ToCsv(customers, UserIsAdmin) };
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

        #region 每日心情

        [Authorize]
        public ActionResult DailyRecord()
        {
            var JiluList = new MeiRiXinQing_ListViewModel { Topics = new List<TopicViewModel>() };
            var topics = _topicService.GetAllTopicsByCondition(EnumCategoryType.MeiRiXinqing, LoggedOnReadOnlyUser);
            if (topics != null && topics.Count > 0)
            {
                var settings = SettingsService.GetSettings();
                var allowedCategories = new List<Category>();
                allowedCategories.Add(_categoryservice.GetCategoryByEnumCategoryType(EnumCategoryType.MeiRiXinqing));
                var topicViewModels = ViewModelMapping.CreateTopicViewModels(topics.ToList(), RoleService, UsersRole, LoggedOnReadOnlyUser, allowedCategories, settings);
                JiluList.Topics.AddRange(topicViewModels);
            }
            return View(JiluList);
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

        /// <summary>
        /// 每日之星
        /// </summary>
        /// <returns></returns>
        public ActionResult MeiRiZhiXing()
        {
            MeiRiZhiXing_ListViewModel model = new MeiRiZhiXing_ListViewModel();
            model.MeiRiZhiXingUserList=_membershipTodayStarService.LoadAllAvailidUsers();
            return View(model);
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
        /// 我的
        /// </summary>
        /// <returns></returns>
        [Authorize]
        public ActionResult AiLvZhangHu()
        {
            AiLvZhangHu_ViewModel model = new AiLvZhangHu_ViewModel();
            model.User = LoggedOnReadOnlyUser;

            return View(model);
        }


    }
}