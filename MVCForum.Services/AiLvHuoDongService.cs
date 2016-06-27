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
        private readonly MVCForumContext _context;
        private readonly ISettingsService _settingsService;
        private readonly IUploadedFileService _uploadedFileService;
        private readonly ILoggingService _loggingService;
        private readonly IMembershipService _membershipService;

        public AiLvHuoDongService(IMVCForumContext context, ISettingsService settingsService, ILoggingService loggingService, IUploadedFileService uploadedFileService, IMembershipService membershipService)
        {
            _settingsService = settingsService;
            _loggingService = loggingService;
            _uploadedFileService = uploadedFileService;
            _membershipService = membershipService;
            _context = context as MVCForumContext;

        }

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
        /// 检查活动截止时间
        /// </summary>
        /// <param name="huodong"></param>
        /// <returns></returns>
        public bool CheckHuoDongJieZhiShijian(AiLvHuoDong huodong)
        {
            if (huodong != null)
            {
                return DateTime.Now < huodong.BaoMingJieZhiTime;
            }
            return false;
        }

        #region 检查男女性别比例

        /// <summary>
        /// 检查特定用户user是否符合爱驴活动实例huodong的男女比例
        /// </summary>
        /// <param name="huodong"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        public bool CheckMemberCondition(AiLvHuoDong huodong, MembershipUser user)
        {
            if (huodong != null && user != null)
            {
                int YuGuRenShu = huodong.YuGuRenShu;
                string XingBieBiLi = huodong.XingBieBiLi;

                if (YuGuRenShu <= 0) return false;

                if (CheckMemberCondition(XingBieBiLi))
                {
                    string[] pair = XingBieBiLi.Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                    if (pair.Length != 2) return false;

                    int BoyRate = 0;
                    int.TryParse(pair[0], out BoyRate);

                    int GirlRate = 0;
                    int.TryParse(pair[1], out GirlRate);

                    if (BoyRate + GirlRate == 0) return false;

                    if (user.Gender == 1) //Boy=1 , girl=0
                    {
                        int BoyNumber = (int)((YuGuRenShu * BoyRate) / (BoyRate + GirlRate));
                        return _context.AiLvHuoDongDetail.Where(x => x.Id == huodong.Id && x.UserGender == 1).Count() + 1 <= BoyNumber;
                    }
                    else
                    {
                        int GirlNumber = (int)((YuGuRenShu * GirlRate) / (BoyRate + GirlRate));
                        return _context.AiLvHuoDongDetail.Where(x => x.Id == huodong.Id && x.UserGender == 0).Count() + 1 <= GirlNumber;
                    }
                }

                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 检查男女比例
        /// </summary>
        /// <param name="XingBieBiLi"></param>
        /// <returns></returns>
        private bool CheckMemberCondition(string XingBieBiLi)
        {
            string[] pair = XingBieBiLi.Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
            if (pair.Length != 2)
            {
                return false;
            }

            int BoyRate = 0;
            int.TryParse(pair[0], out BoyRate);

            int GirlRate = 0;
            int.TryParse(pair[1], out GirlRate);

            if (BoyRate + GirlRate == 0) return false;

            return true;
        }

        #endregion

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

        public AiLvHuoDong Get(Guid id)
        {
            return _context.AiLvHuoDong.AsNoTracking().FirstOrDefault(x => x.Id == id);
        }

        public IList<AiLvHuoDong> GetAiLvHongDongListByName(string SearchCondition)
        {
            SearchCondition = StringUtils.SafePlainText(SearchCondition);
            return _context.AiLvHuoDong.AsNoTracking()
                .Where(x => x.MingCheng.ToUpper().Contains(SearchCondition.ToUpper()))
                .OrderByDescending(x => x.MingCheng)
                .ToList();
        }

        public IList<AiLvHuoDong> GetAll()
        {
            return _context.AiLvHuoDong.AsNoTracking().ToList();
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

        /// <summary>
        /// 自动更新活动状态
        /// TODO: Benjamin 此方法的实现需要客户确认
        /// </summary>
        /// <returns></returns>
        public bool Update_ZhuangTai()
        {

            var list = _context.AiLvHuoDong.Where(x => x.ZhuangTai == (int)Enum_HuoDongZhuangTai.Registing ||
                                                       x.ZhuangTai == (int)Enum_HuoDongZhuangTai.StopRegister).AsNoTracking().ToList();
            foreach (AiLvHuoDong item in list)
            {
                //Do nothing
            }
            return true;
        }

        public IList<AiLvHuoDong> GetAllAiLvHuodongByStatus(Enum_HuoDongZhuangTai status)
        {
            switch (status)
            {
                case Enum_HuoDongZhuangTai.Registing:
                    return _context.AiLvHuoDong.Where(x => x.ZhuangTai == (int)Enum_HuoDongZhuangTai.Registing).AsNoTracking().ToList();
                case Enum_HuoDongZhuangTai.StopRegister:
                    return _context.AiLvHuoDong.Where(x => x.ZhuangTai == (int)Enum_HuoDongZhuangTai.StopRegister).AsNoTracking().ToList();
                case Enum_HuoDongZhuangTai.Finished:
                    return _context.AiLvHuoDong.Where(x => x.ZhuangTai == (int)Enum_HuoDongZhuangTai.Finished).AsNoTracking().ToList();
                default:
                    return _context.AiLvHuoDong.AsNoTracking().ToList();
            }
        }

        public bool AuditAiLvHuodong(AiLvHuoDong ailvhuodongInstance, bool auditresult)
        {
            if (ailvhuodongInstance != null && !string.IsNullOrEmpty(ailvhuodongInstance.GongYingShangUserId))
            {
                var user = _membershipService.GetUser(Guid.Parse(ailvhuodongInstance.GongYingShangUserId));
                if (user != null && user.Roles.Any(x => x.RoleName == AppConstants.SupplierRoleName))
                {
                    ailvhuodongInstance.ShenHeBiaoZhi = auditresult ? 1 : 0;
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