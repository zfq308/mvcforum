using System;
using System.Collections.Generic;
using MVCForum.Domain.DomainModel.Entities;
using MVCForum.Utilities;

namespace MVCForum.Domain.DomainModel
{
    /// <summary>
    /// 跟帖排序依据
    /// </summary>
    public enum PostOrderBy
    {
        Standard,
        Newest,
        Votes
    }

    /// <summary>
    /// 跟帖回复类
    /// </summary>
    public partial class Post : Entity
    {
        public Post()
        {
            Id = GuidComb.GenerateComb();
        }
        public Guid Id { get; set; }
        public string PostContent { get; set; }
        public DateTime DateCreated { get; set; }
        public int VoteCount { get; set; }
        public DateTime DateEdited { get; set; }
        public bool IsSolution { get; set; }
        public bool IsTopicStarter { get; set; }
        public bool? FlaggedAsSpam { get; set; }
        public string IpAddress { get; set; }
        /// <summary>
        /// 当前帖子是否被设定为“待定”
        /// </summary>
        public bool? Pending { get; set; }
        public string SearchField { get; set; }
        public Guid? InReplyTo { get; set; }
        public virtual Topic Topic { get; set; }
        public virtual MembershipUser User { get; set; }
        public virtual IList<Vote> Votes { get; set; }
        public virtual IList<UploadedFile> Files { get; set; }
        public virtual IList<Favourite> Favourites { get; set; }
        public virtual IList<PostEdit> PostEdits { get; set; }
    }

    /// <summary>
    /// 跟帖回复修改记录
    /// </summary>
    public partial class PostEdit : Entity
    {
        public PostEdit()
        {
            Id = GuidComb.GenerateComb();
        }

        #region 属性
        /// <summary>
        /// 跟帖回复修改的流水号
        /// </summary>
        public Guid Id { get; set; }
        /// <summary>
        /// 修改时间
        /// </summary>
        public DateTime DateEdited { get; set; }
        /// <summary>
        /// 原跟帖的内容
        /// </summary>
        public string OriginalPostContent { get; set; }
        /// <summary>
        /// 修改后的跟帖内容
        /// </summary>
        public string EditedPostContent { get; set; }
        /// <summary>
        /// 原跟帖标题
        /// </summary>
        public string OriginalPostTitle { get; set; }
        /// <summary>
        /// 修改后的跟帖标题
        /// </summary>
        public string EditedPostTitle { get; set; }
        /// <summary>
        /// 修改人
        /// </summary>
        public virtual MembershipUser EditedBy { get; set; }
        /// <summary>
        /// 跟帖的实例
        /// </summary>
        public virtual Post Post { get; set; }

        #endregion
    }
}
