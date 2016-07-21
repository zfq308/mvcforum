using System;
using System.Linq;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Data.Entity;
using System.Text;
using System.Web;
using System.Web.Hosting;
using System.Web.Security;
using MVCForum.Domain.Constants;
using MVCForum.Domain.DomainModel;
using MVCForum.Domain.DomainModel.Entities;
using MVCForum.Domain.Events;
using MVCForum.Domain.Interfaces;
using MVCForum.Domain.Interfaces.Services;
using MVCForum.Domain.Interfaces.UnitOfWork;
using MVCForum.Services.Data.Context;
using MVCForum.Utilities;

namespace MVCForum.Services
{

    public partial class AiLvHuoDongService : IAiLvHuoDongService
    {
        #region 定义只读变量

        private readonly MVCForumContext _context;
        private readonly ISettingsService _settingsService;
        private readonly IUploadedFileService _uploadedFileService;
        private readonly ILoggingService _loggingService;
        private readonly IMembershipService _membershipService;

        #endregion

        #region 建构式

        public AiLvHuoDongService(IMVCForumContext context, ISettingsService settingsService, ILoggingService loggingService, IUploadedFileService uploadedFileService, IMembershipService membershipService)
        {
            _settingsService = settingsService;
            _loggingService = loggingService;
            _uploadedFileService = uploadedFileService;
            _membershipService = membershipService;
            _context = context as MVCForumContext;

        }

        #endregion

        #region 增删爱驴活动实例

        /// <summary>
        /// 新增爱驴活动实例
        /// </summary>
        /// <param name="newHuoDong"></param>
        /// <returns></returns>
        public AiLvHuoDong Add(AiLvHuoDong newHuoDong)
        {
            return _context.AiLvHuoDong.Add(newHuoDong);
        }

        /// <summary>
        /// 删除爱驴活动实例
        /// </summary>
        /// <param name="huodong"></param>
        /// <param name="unitOfWork"></param>
        /// <returns></returns>
        public bool Delete(AiLvHuoDong huodong)
        {
            try
            {
                _context.AiLvHuoDong.Remove(huodong);
                return true;
            }
            catch (Exception ex)
            {
                _loggingService.Error(ex);
            }
            return false;
        }

        #endregion

        #region 查询/取得爱驴活动记录

        public AiLvHuoDong Get(Guid id)
        {
            return _context.AiLvHuoDong.AsNoTracking().FirstOrDefault(x => x.Id == id);
        }

        public IList<AiLvHuoDong> GetAll()
        {
            return _context.AiLvHuoDong.AsNoTracking().ToList();
        }

      
        public IList<AiLvHuoDong> GetRecentAiLvHuodong(int amountToTake)
        {
            var results = _context.AiLvHuoDong.AsNoTracking().Where(x => x.ShenHeBiaoZhi == Enum_ShenHeBiaoZhi.AuditSuccess)
                              .OrderByDescending(x => x.CreatedTime)
                              .Take(amountToTake)
                              .ToList();
            return results;
        }

        public PagedList<AiLvHuoDong> GetAll(int pageIndex, int pageSize)
        {
            var totalCount = _context.AiLvHuoDong.Count();
            var results = _context.AiLvHuoDong.AsNoTracking()
                                .OrderByDescending(x => x.CreatedTime)
                                .Skip((pageIndex - 1) * pageSize)
                                .Take(pageSize)
                                .ToList();

            return new PagedList<AiLvHuoDong>(results, pageIndex, pageSize, totalCount);
        }

        public IList<AiLvHuoDong> GetAllAiLvHuodongByStatus(Enum_HuoDongZhuangTai status)
        {
            switch (status)
            {
                case Enum_HuoDongZhuangTai.Registing:
                    return _context.AiLvHuoDong.Where(x => x.ZhuangTai == Enum_HuoDongZhuangTai.Registing).AsNoTracking().ToList();
                case Enum_HuoDongZhuangTai.StopRegister:
                    return _context.AiLvHuoDong.Where(x => x.ZhuangTai == Enum_HuoDongZhuangTai.StopRegister).AsNoTracking().ToList();
                case Enum_HuoDongZhuangTai.Finished:
                    return _context.AiLvHuoDong.Where(x => x.ZhuangTai == Enum_HuoDongZhuangTai.Finished).AsNoTracking().ToList();
                default:
                    return _context.AiLvHuoDong.AsNoTracking().ToList();
            }
        }

        public IList<AiLvHuoDong> GetAiLvHongDongListByName(string SearchCondition)
        {
            SearchCondition = StringUtils.SafePlainText(SearchCondition);
            return _context.AiLvHuoDong.AsNoTracking()
                .Where(x => x.MingCheng.ToUpper().Contains(SearchCondition.ToUpper()))
                .OrderByDescending(x => x.MingCheng)
                .ToList();
        }

        #endregion

        /// <summary>
        /// 自动更新活动状态
        /// TODO: Benjamin 此方法的实现需要客户确认
        /// </summary>
        /// <returns></returns>
        public bool Update_ZhuangTai()
        {

            var list = _context.AiLvHuoDong.Where(x => x.ZhuangTai == Enum_HuoDongZhuangTai.Registing ||
                                                       x.ZhuangTai == Enum_HuoDongZhuangTai.StopRegister).AsNoTracking().ToList();
            foreach (AiLvHuoDong item in list)
            {
                //Do nothing
            }
            return true;
        }

        public bool AuditAiLvHuodong(AiLvHuoDong ailvhuodongInstance, bool auditresult)
        {
            if (ailvhuodongInstance != null && !string.IsNullOrEmpty(ailvhuodongInstance.GongYingShangUserId))
            {
                var user = _membershipService.GetUser(Guid.Parse(ailvhuodongInstance.GongYingShangUserId));
                if (user != null && user.Roles.Any(x => x.RoleName == AppConstants.SupplierRoleName))
                {
                    ailvhuodongInstance.ShenHeBiaoZhi = auditresult ? Enum_ShenHeBiaoZhi.AuditSuccess : Enum_ShenHeBiaoZhi.AuditReject;
                    return true;
                }
                return false;
            }
            else
            {
                return false;
            }
        }


    }
}