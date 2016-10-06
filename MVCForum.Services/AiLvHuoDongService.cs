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
        private readonly IActivityRegisterService _ActivityRegisterService;
        private readonly ITopicService _topicservice;
        private readonly ICategoryService _categoryService;
        private readonly IPostService _postservice;

        #endregion

        #region 建构式

        public AiLvHuoDongService(IMVCForumContext context, ISettingsService settingsService, ILoggingService loggingService,
            IUploadedFileService uploadedFileService, IMembershipService membershipService,
            IActivityRegisterService activityRegisterService,
            ICategoryService categoryservice,
            IPostService postservice,
            ITopicService topicservice)
        {
            _settingsService = settingsService;
            _loggingService = loggingService;
            _uploadedFileService = uploadedFileService;
            _membershipService = membershipService;
            _ActivityRegisterService = activityRegisterService;
            _topicservice = topicservice;
            _categoryService = categoryservice;
            _postservice = postservice;
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
                // 删除报名者
                var registerlist = _ActivityRegisterService.GetActivityRegisterListByHongDong(huodong);
                if (registerlist != null && registerlist.Count > 0)
                {
                    foreach (var item in registerlist)
                    {
                        _context.ActivityRegister.Remove(item);
                    }
                }
                _context.SaveChanges();

                // 删除活动记录
                var allowedAccessCategories = new List<Category>();
                allowedAccessCategories.Add(_categoryService.GetCategoryByEnumCategoryType(EnumCategoryType.AiLvJiLu));
                var topic = _topicservice.GetAll(allowedAccessCategories).Where(x => x.Name == "【" + huodong.MingCheng.Trim() + "】的活动记录").FirstOrDefault();

                if (topic != null)
                {
                    var postlist = _postservice.GetPostsByTopic(topic.Id);
                    if (postlist != null && postlist.Count > 0)
                    {
                        foreach (var post in postlist)
                        {
                            var uploadfiles = _uploadedFileService.GetAllByPost(post.Id);
                            foreach (var uploadfile in uploadfiles)
                            {
                                _context.UploadedFile.Remove(uploadfile);
                                _context.Entry<UploadedFile>(uploadfile).State = EntityState.Deleted;
                            }
                            //_context.Post.Attach(post);
                            _context.Post.Remove(post);
                            _context.Entry<Post>(post).State = EntityState.Deleted;
                        }
                        _context.SaveChanges();
                    }

                    _context.Topic.Attach(topic);
                    _context.Topic.Remove(topic);
                    _context.Entry<Topic>(topic).State = EntityState.Deleted;
                    _context.SaveChanges();
                }

                _context.AiLvHuoDong.Attach(huodong);
                _context.AiLvHuoDong.Remove(huodong);
                _context.Entry<AiLvHuoDong>(huodong).State = EntityState.Deleted;
                _context.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                _loggingService.Error(ex);
            }
            return false;
        }




        #endregion

        #region 查询/取得爱驴活动实例

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
        /// </summary>
        /// <returns></returns>
        public bool Update_ZhuangTai()
        {
            var list = _context.AiLvHuoDong.Where(x => x.ZhuangTai == Enum_HuoDongZhuangTai.Registing ||
                                                       x.ZhuangTai == Enum_HuoDongZhuangTai.StopRegister).AsNoTracking().ToList();
            foreach (AiLvHuoDong item in list)
            {
                if (item.StopTime < DateTime.Now)
                {
                    item.ZhuangTai = Enum_HuoDongZhuangTai.Finished;
                    continue;
                }
                if (item.ZhuangTai == Enum_HuoDongZhuangTai.Registing && item.BaoMingJieZhiTime < DateTime.Now)
                {
                    item.ZhuangTai = Enum_HuoDongZhuangTai.StopRegister;
                    continue;
                }
            }
            return true;
        }

        /// <summary>
        /// 审核活动
        /// </summary>
        /// <param name="ailvhuodongInstance"></param>
        /// <param name="auditresult"></param>
        /// <returns></returns>
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