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

    public partial class FollowService : IFollowService
    {
        private readonly MVCForumContext _context;
        private readonly ILoggingService _loggingService;
        private readonly IMembershipService _membershipService;


        public FollowService(IMVCForumContext context, ILoggingService loggingService, IMembershipService membershipService)
        {
            _loggingService = loggingService;
            _membershipService = membershipService;
            _context = context as MVCForumContext;

        }


        #region 增删用户关注实例

        /// <summary>
        /// 添加关注记录的实例
        /// </summary>
        /// <param name="followInfo"></param>
        /// <returns></returns>
        public Follow Add(Follow followInfo)
        {
            return _context.Follow.Add(followInfo);
        }

        /// <summary>
        /// 删除关注记录的实例
        /// </summary>
        /// <param name="followInfo"></param>
        /// <returns></returns>
        public bool Delete(Follow followInfo)
        {
            try
            {
                _context.Follow.Remove(followInfo);
                return true;
            }
            catch (Exception ex)
            {
                _loggingService.Error(ex);
            }
            return false;
        }
        #endregion

        #region 关注黑名单操作


        /// <summary>
        /// 添加黑名单记录
        /// </summary>
        /// <param name="UserId"></param>
        /// <param name="BlacklistUserId"></param>
        /// <returns></returns>
        public Follow AddIntoBlackList(Guid UserId, Guid BlacklistUserId)
        {
            var query = _context.Follow.AsNoTracking().Where(x => x.UserId == UserId && x.FriendUserId == BlacklistUserId);
            if (query != null && query.Count() >= 1)
            {
                Follow item = query.FirstOrDefault();
                item.OpsFlag = "Black";
                return item;
            }
            else
            {
                var newitem = new Follow() { UserId = UserId, FriendUserId = BlacklistUserId, CreateTime = DateTime.Now, UpdateTime = DateTime.Now, OpsFlag = "Black" };
                return Add(newitem);
            }

        }

        /// <summary>
        /// 取消黑名单记录
        /// </summary>
        /// <param name="UserId"></param>
        /// <param name="BlacklistUserId"></param>
        /// <returns></returns>
        public Follow CancelBlackList(Guid UserId, Guid BlacklistUserId)
        {
            var query = _context.Follow.AsNoTracking().Where(x => x.UserId == UserId && x.FriendUserId == BlacklistUserId && x.OpsFlag == "Black");
            if (query != null && query.Count() >= 1)
            {
                Follow item = query.FirstOrDefault();
                item.OpsFlag = "";
                return item;
            }
            return null;
        }

        #endregion

        /// <summary>
        /// 取得关注记录的实例
        /// </summary>
        /// <param name="FollowId"></param>
        /// <returns></returns>
        public Follow Get(Guid FollowId)
        {
            return _context.Follow.AsNoTracking().FirstOrDefault(x => x.Id == FollowId);
        }

        /// <summary>
        /// 取得关注记录的实例
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="FriendId"></param>
        /// <returns></returns>
        public Follow Get(Guid userId, Guid FriendId)
        {
            return _context.Follow.AsNoTracking().FirstOrDefault(x => x.UserId == userId && x.FriendUserId == FriendId);
        }


        /// <summary>
        /// 取得关注特定用户的用户列表
        /// </summary>
        /// <param name="UserId"></param>
        /// <returns></returns>
        public IList<Follow> Get_People_Followed_SpecificUser_List(Guid UserId)
        {
            var query =
              from a in _context.Follow
              where a.FriendUserId == UserId &&
                    !_context.Follow.Any(e => (e.UserId == a.FriendUserId) && (e.FriendUserId == a.UserId)) &&
                    a.OpsFlag==""
              select a;
            return query.Distinct().ToList();
        }

        /// <summary>
        /// 取得特定用户的关注列表
        /// </summary>
        /// <param name="UserId"></param>
        /// <returns></returns>
        public IList<Follow> Get_SpecificUser_Followed_Poeple_List(Guid UserId)
        {
            var query =
                from a in _context.Follow
                where a.UserId == UserId && 
                      !_context.Follow.Any(e => (e.UserId == a.FriendUserId) && (e.FriendUserId == a.UserId)) &&
                      a.OpsFlag==""
                select a;
            return query.Distinct().ToList();
        }

        public IList<Follow> GetFriendList(Guid userId)
        {
            var query =
               (from p in _context.Follow
                join q in _context.Follow
                on p.UserId equals q.FriendUserId
                where p.UserId == userId && p.OpsFlag == "" && q.OpsFlag == ""
                select new {
                    Id = p.Id,
                    UserId = p.UserId,
                    FriendUserId = p.FriendUserId,
                    CreateTime = p.CreateTime,
                    UpdateTime = p.UpdateTime,
                    OpsFlag = p.OpsFlag }
               ).Distinct().AsEnumerable().Select(x => new Follow
               {
                   Id = x.Id,
                   UserId = x.UserId,
                   FriendUserId = x.FriendUserId,
                   CreateTime = x.CreateTime,
                   UpdateTime = x.UpdateTime,
                   OpsFlag = x.OpsFlag
               });
            return query.ToList();
        }

        public IList<Follow> GetBlackList(Guid userId)
        {
            return _context.Follow.AsNoTracking().Where(x => x.UserId == userId && x.OpsFlag == "Black").ToList();
        }


    }

}