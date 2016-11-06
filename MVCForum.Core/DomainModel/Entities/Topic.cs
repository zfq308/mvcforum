using System;
using System.Collections.Generic;
using System.Linq;
using MVCForum.Utilities;

namespace MVCForum.Domain.DomainModel
{
    /// <summary>
    /// 话题类型
    /// </summary>
    public enum Enum_TopicType
    {
        /// <summary>
        /// 系统公告类型
        /// </summary>
        Announcement = 1,
        /// <summary>
        /// 个人话题类型
        /// </summary>
        PersonalTopic = 2,
        /// <summary>
        /// 其他类型，暂定
        /// </summary>
        Other = 3,
    }


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

        #region 属性

        /// <summary>
        /// 话题实体定义的Id编号
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// 话题所属类别
        /// </summary>
        public Enum_TopicType TopicType { get; set; }

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
        /// 此话题下的最后提交的跟帖（回复）
        /// </summary>
        public virtual Post LastPost { get; set; }

        /// <summary>
        /// 此话题下的全部跟帖（回复）
        /// </summary>
        public virtual IList<Post> Posts { get; set; }

        /// <summary>
        /// 此话题下的包含的话题标签
        /// </summary>
        public virtual IList<TopicTag> Tags { get; set; }

        /// <summary>
        /// 此话题所属的分类定义
        /// </summary>
        public virtual Category Category { get; set; }

        /// <summary>
        /// 此话题的建立者
        /// </summary>
        public virtual MembershipUser User { get; set; }

        public virtual IList<TopicNotification> TopicNotifications { get; set; }

        public virtual IList<Favourite> Favourites { get; set; }

        public virtual Poll Poll { get; set; }

        /// <summary>
        /// 当前话题是否被设定为“待定”
        /// </summary>
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


        #endregion
    }

    public partial class TopicNotification : Entity
    {
        public TopicNotification()
        {
            Id = GuidComb.GenerateComb();
        }
        public Guid Id { get; set; }
        public virtual Topic Topic { get; set; }
        public virtual MembershipUser User { get; set; }
    }

    public partial class TopicTag : Entity
    {
        public TopicTag()
        {
            Id = GuidComb.GenerateComb();
        }
        public Guid Id { get; set; }
        public string Tag { get; set; }
        public string Slug { get; set; }
        public string Description { get; set; }

        public string NiceUrl
        {
            get
            {
                var url = UrlTypes.GenerateUrl(UrlType.Tag, StringUtils.RemoveAccents(Slug));
                return url;
            }
        }

        public virtual IList<Topic> Topics { get; set; }
        public virtual IList<TagNotification> Notifications { get; set; }
    }

    public partial class TagNotification : Entity
    {
        public TagNotification()
        {
            Id = GuidComb.GenerateComb();
        }
        public Guid Id { get; set; }
        public virtual TopicTag Tag { get; set; }
        public virtual MembershipUser User { get; set; }
    }

}
