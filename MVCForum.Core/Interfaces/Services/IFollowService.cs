using System;
using System.Linq;
using System.Collections.Generic;
using MVCForum.Domain.DomainModel;
using MVCForum.Domain.Interfaces.UnitOfWork;

namespace MVCForum.Domain.Interfaces.Services
{

    public partial interface IFollowService
    {
        /// <summary>
        /// 取得关注记录的实例
        /// </summary>
        /// <param name="FollowId"></param>
        /// <returns></returns>
        Follow Get(Guid FollowId);
        /// <summary>
        /// 删除关注记录的实例
        /// </summary>
        /// <param name="followInfo"></param>
        /// <returns></returns>
        bool Delete(Follow followInfo);
        /// <summary>
        /// 添加关注记录的实例
        /// </summary>
        /// <param name="followInfo"></param>
        /// <returns></returns>
        Follow Add(Follow followInfo);
        /// <summary>
        /// 取得特定用户的关注列表
        /// </summary>
        /// <param name="UserId"></param>
        /// <returns></returns>
        IList<Follow> Get_SpecificUser_Followed_Poeple_List(Guid UserId);
        /// <summary>
        /// 取得关注特定用户的用户列表
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        IList<Follow> Get_People_Followed_SpecificUser_List(Guid userId);
        /// <summary>
        /// 添加黑名单记录
        /// </summary>
        /// <param name="UserId"></param>
        /// <param name="BlacklistUserId"></param>
        /// <returns></returns>
        Follow AddIntoBlackList(Guid UserId, Guid BlacklistUserId);
        /// <summary>
        /// 取消黑名单记录
        /// </summary>
        /// <param name="UserId"></param>
        /// <param name="BlacklistUserId"></param>
        /// <returns></returns>
        Follow CancelBlackList(Guid UserId, Guid BlacklistUserId);

        /// <summary>
        /// 取得特定用户的好友列表（彼此关注）
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        IList<Follow> GetFriendList(Guid userId);

        /// <summary>
        /// 取得特定用户的黑名单
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        IList<Follow> GetBlackList(Guid userId);
    }

}