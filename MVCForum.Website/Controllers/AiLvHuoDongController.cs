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

namespace MVCForum.Website.Controllers
{
    public class AiLvHuoDongController : BaseController
    {
        private readonly IAiLvHuoDongService _aiLvHuoDongService;
        private readonly ITopicService _topicService;
        private readonly MVCForumContext _context;

        public AiLvHuoDongController(IMVCForumContext context, ITopicService TopicService, IAiLvHuoDongService aiLvHuoDongService, ILoggingService loggingService, IUnitOfWorkManager unitOfWorkManager, IMembershipService membershipService, ILocalizationService localizationService, IRoleService roleService,
            ISettingsService settingsService)
             : base(loggingService, unitOfWorkManager, membershipService, localizationService, roleService, settingsService)
        {
            _aiLvHuoDongService = aiLvHuoDongService;
            _topicService = TopicService;
            _context = context as MVCForumContext;
        }


        #region 爱驴活动模块

        /// <summary>
        /// 最新活动
        /// </summary>
        /// <returns></returns>
        public ActionResult ZuiXinHuoDong()
        {
            var HuoDongList = new AiLvHuoDong_TopicsViewModel
            {
                AiLvHuoDongList = _aiLvHuoDongService.GetAll()
            };
            return View(HuoDongList);
        }

        [Authorize]
        public ActionResult CreateAiLvHuoDong()
        {
            return View(new AiLvHuoDong_CreateEdit_ViewModel());
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult CreateAiLvHuoDong(AiLvHuoDong_CreateEdit_ViewModel ViewModel)
        {
            if (ModelState.IsValid)
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
                item.YaoQingMa = ViewModel.YaoQingMa;
                item.ZhuangTai = ViewModel.ZhuangTai;
                item.GongYingShangUserId = LoggedOnReadOnlyUser.Id.ToString();
                item.ShenHeBiaoZhi = 0;
                item.CreatedTime = DateTime.Now;

                // Save the changes
                _context.AiLvHuoDong.Add(item);
                _context.SaveChanges();

                TempData[AppConstants.MessageViewBagName] = new GenericMessageViewModel
                {
                    Message = "活动已创建。",
                    MessageType = GenericMessages.info
                };
                return RedirectToAction("ZuiXinHuoDong", "AiLvHuoDong");

            }

            return View(ViewModel);
        }

        [Authorize(Roles = "Admin,Supplier")]
        public ActionResult EditAiLvHuoDong(Guid Id)
        {
            var item = _aiLvHuoDongService.Get(Id);
            var EditModel = new AiLvHuoDong_CreateEdit_ViewModel
            {
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
            return View(EditModel);
        }

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

        [Authorize(Roles = "Admin")]
        [MultiButton("btn_AuditSuccess")]
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
        [MultiButton("btn_AuditFail")]
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


        #region 爱驴资讯模块
        /// <summary>
        /// 爱驴资讯
        /// </summary>
        /// <returns></returns>
        public ActionResult ZuiXinZiXun()
        {
            var ZiXunList = new AiLvZiXunTopicsViewModel();
            var topics= _topicService.GetAllTopicsByCategory(EnumCategoryType.AiLvZiXun);
            if (topics != null && topics.Count > 0)
            {
                ZiXunList.Topics = new List<TopicViewModel>();
                foreach (var topic in topics)
                {
                    var newitem = new TopicViewModel
                    {
                        //TODO: Ben 需要检查TopicViewModel的赋值是否完备
                        Topic = topic,
                        DisablePosting = true,
                        IsSubscribed = false
                    };
                    ZiXunList.Topics.Add(newitem);
                }
            }
            return View(ZiXunList);
    }

    #endregion

    #region MyRegion

    /// <summary>
    /// 最新记录
    /// </summary>
    /// <returns></returns>
    public ActionResult ZuiXinJilu()
    {
        return View();
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
    /// 最新服务
    /// </summary>
    /// <returns></returns>
    public ActionResult ZuiXinFuWu()
    {
        return View();
    }

    /// <summary>
    /// 最新资讯
    /// </summary>
    /// <returns></returns>
    public ActionResult ZuiXinZixun()
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