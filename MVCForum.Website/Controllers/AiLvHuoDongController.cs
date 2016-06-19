using MVCForum.Domain.Constants;
using MVCForum.Domain.DomainModel;
using MVCForum.Domain.Interfaces.Services;
using MVCForum.Domain.Interfaces.UnitOfWork;
using MVCForum.Services;
using MVCForum.Website.Areas.Admin.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MVCForum.Website.Controllers
{
    public class AiLvHuoDongController  : BaseController
    {
        public AiLvHuoDongController(ILoggingService loggingService, IUnitOfWorkManager unitOfWorkManager, IMembershipService membershipService, ILocalizationService localizationService, IRoleService roleService,
            ISettingsService settingsService)
             : base(loggingService, unitOfWorkManager, membershipService, localizationService, roleService, settingsService)
        {
          
        }


        /// <summary>
        /// 最新活动
        /// </summary>
        /// <returns></returns>
        public ActionResult ZuiXinHuoDong()
        {
          

            return View();
        }

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

    }
}