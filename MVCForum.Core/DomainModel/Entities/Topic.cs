using System;
using System.Collections.Generic;
using System.Linq;
using MVCForum.Utilities;

namespace MVCForum.Domain.DomainModel
{
    /// <summary>
    /// 话题实体定义类
    /// </summary>
    public partial class Topic : Entity
    {
        /// <summary>
        /// 话题实体定义建构式
        /// </summary>
        public Topic()
        {
            Id = GuidComb.GenerateComb();
        }
        /// <summary>
        /// 话题实体定义的Id编号
        /// </summary>
        public Guid Id { get; set; }
        /// <summary>
        /// 话题所属类别
        /// </summary>
        public string TopicType { get; set; }
        /// <summary>
        /// 话题名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 记录生成时间
        /// </summary>
        public DateTime CreateDate { get; set; }
        /// <summary>
        /// 话题是否已结束
        /// </summary>
        public bool Solved { get; set; }
        public bool? SolvedReminderSent { get; set; }
        public string Slug { get; set; }
        /// <summary>
        /// 被浏览次数
        /// </summary>
        public int Views { get; set; }
        /// <summary>
        /// 是否置顶
        /// </summary>
        public bool IsSticky { get; set; }
        /// <summary>
        /// 是否锁定
        /// </summary>
        public bool IsLocked { get; set; }

        /// <summary>
        /// 此话题下的最后提交的帖子
        /// </summary>
        public virtual Post LastPost { get; set; }
        /// <summary>
        /// 此话题所属的分类定义
        /// </summary>
        public virtual Category Category { get; set; }
        /// <summary>
        /// 此话题下的全部帖子
        /// </summary>
        public virtual IList<Post> Posts { get; set; }
        /// <summary>
        /// 此话题下的包含的话题标签
        /// </summary>
        public virtual IList<TopicTag> Tags { get; set; }
        /// <summary>
        /// 此话题的建立者
        /// </summary>
        public virtual MembershipUser User { get; set; }
        public virtual IList<TopicNotification> TopicNotifications { get; set; }
        public virtual IList<Favourite> Favourites { get; set; }
        public virtual Poll Poll { get; set; }
        public bool? Pending { get; set; }
        public string NiceUrl
        {
            get { return UrlTypes.GenerateUrl(UrlType.Topic, Slug); }
            //get { return Slug; }
        }
        public int VoteCount
        {
            get { return Posts.Select(x => x.VoteCount).Sum(); }
        }
    }

    public enum EnumTopicType
    {

    }

}
