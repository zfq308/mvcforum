using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web;
using System.Web.Mvc;
using MVCForum.Domain.Constants;
using MVCForum.Domain.DomainModel;
using MVCForum.Website.Application;
using System.ComponentModel.DataAnnotations.Schema;

namespace MVCForum.Website.ViewModels
{
    /// <summary>
    /// 爱驴服务TopicViewModel
    /// </summary>
    public class AiLvFuWu_ListViewModel
    {
        public List<TopicViewModel> Topics { get; set; }
        public int? PageIndex { get; set; }
        public int? TotalCount { get; set; }
        public int? TotalPages { get; set; }
    }

    /// <summary>
    /// 爱驴资讯ListViewModel
    /// </summary>
    public class AiLvZiXun_ListViewModel
    {
        public List<TopicViewModel> Topics { get; set; }
        public int? PageIndex { get; set; }
        public int? TotalCount { get; set; }
        public int? TotalPages { get; set; }
    }

    /// <summary>
    /// 爱驴记录ListViewModel
    /// </summary>
    public class AiLvJiLu_ListViewModel
    {
        public List<TopicViewModel> Topics { get; set; }
        public int? PageIndex { get; set; }
        public int? TotalCount { get; set; }
        public int? TotalPages { get; set; }
    }


    public class MeiRiXinQing_ListViewModel
    {
        public List<TopicViewModel> Topics { get; set; }
        public int? PageIndex { get; set; }
        public int? TotalCount { get; set; }
        public int? TotalPages { get; set; }
    }















    public class CreateTopicButtonViewModel
    {
        public MembershipUser LoggedOnUser { get; set; }
        public bool UserCanPostTopics { get; set; }
    }

    public class TopicViewModel
    {
        public Topic Topic { get; set; }
        public PermissionSet Permissions { get; set; }
        public bool MemberIsOnline { get; set; }

        // Poll
        public PollViewModel Poll { get; set; }

        // Post Stuff
        public PostViewModel StarterPost { get; set; }
        public List<PostViewModel> Posts { get; set; }
        public int? PageIndex { get; set; }
        public int? TotalCount { get; set; }
        public int? TotalPages { get; set; }
        public string LastPostPermaLink { get; set; }

        // Permissions
        public bool DisablePosting { get; set; }

        // Subscription
        public bool IsSubscribed { get; set; }

        // Votes
        public int VotesUp { get; set; }
        public int VotesDown { get; set; }

        // Quote/Reply
        public string QuotedPost { get; set; }
        public Guid? ReplyTo { get; set; }
        public string ReplyToUsername { get; set; }

        /// <summary>
        /// 帖子被跟帖次数
        /// </summary>
        public int Answers { get; set; }
        /// <summary>
        /// 帖子被浏览的次数
        /// </summary>
        public int Views { get; set; }

        // Misc
        public bool ShowUnSubscribedLink { get; set; }
    }

    #region 待检查的


    public class ActiveTopicsViewModel
    {
        public List<TopicViewModel> Topics { get; set; }
        public int? PageIndex { get; set; }
        public int? TotalCount { get; set; }
        public int? TotalPages { get; set; }
    }

    public class PostedInViewModel
    {
        public List<TopicViewModel> Topics { get; set; }
        public int? PageIndex { get; set; }
        public int? TotalCount { get; set; }
        public int? TotalPages { get; set; }
    }

    public class HotTopicsViewModel
    {
        public List<TopicViewModel> Topics { get; set; }
    }

    public class TagTopicsViewModel
    {
        public List<TopicViewModel> Topics { get; set; }
        public int? PageIndex { get; set; }
        public int? TotalCount { get; set; }
        public int? TotalPages { get; set; }
        public string Tag { get; set; }
        public Guid TagId { get; set; }
        public bool IsSubscribed { get; set; }
    }

    public class CheckCreateTopicPermissions
    {
        public bool CanUploadFiles { get; set; }
        public bool CanStickyTopic { get; set; }
        public bool CanLockTopic { get; set; }
        public bool CanCreatePolls { get; set; }
        public bool CanInsertImages { get; set; }
    }

    public class CreateEditTopicViewModel
    {
        [Required(ErrorMessage = "请填写必要的标题")]
        [StringLength(100, ErrorMessage = "标题最长为100个字符")]
        [ForumMvcResourceDisplayName("Topic.Label.TopicTitle")]
        public string Name { get; set; }

        //[Column(TypeName = "varchar(MAX)")]
        //[MaxLength]
        [UIHint(AppConstants.EditorType), AllowHtml]
        [StringLength(600000, ErrorMessage = "内容过多，请点击 插入图片 按钮的方式显式加入图片")]
        public string Content { get; set; }

        [ForumMvcResourceDisplayName("Post.Label.IsStickyTopic")]
        public bool IsSticky { get; set; }

        [ForumMvcResourceDisplayName("Post.Label.LockTopic")]
        public bool IsLocked { get; set; }

        [Required]
        [ForumMvcResourceDisplayName("Topic.Label.Category")]
        public Guid CategoryId { get; set; }

        public List<SelectListItem> Categories { get; set; }

        public string Tags { get; set; }

        [ForumMvcResourceDisplayName("Topic.Label.PollCloseAfterDays")]
        public int PollCloseAfterDays { get; set; }

        /// <summary>
        /// 调查问卷的选项实例清单
        /// </summary>
        public IList<PollAnswer> PollAnswers { get; set; }

        [ForumMvcResourceDisplayName("Topic.Label.SubscribeToTopic")]
        public bool SubscribeToTopic { get; set; }

        [ForumMvcResourceDisplayName("Topic.Label.UploadFiles")]
        public HttpPostedFileBase[] Files { get; set; }

        public CheckCreateTopicPermissions OptionalPermissions { get; set; }

        // Edit Properties
        [HiddenInput]
        public Guid Id { get; set; }

        [HiddenInput]
        public Guid TopicId { get; set; }

        public bool IsTopicStarter { get; set; }

        /// <summary>
        /// Topic所属类型枚举
        /// </summary>
        public Enum_TopicType TopicType { get; set; }

    }

    public class GetMorePostsViewModel
    {
        public Guid TopicId { get; set; }
        public int PageIndex { get; set; }
        public string Order { get; set; }
    }

    public class PollViewModel
    {
        public Poll Poll { get; set; }
        public bool UserHasAlreadyVoted { get; set; }
        public int TotalVotesInPoll { get; set; }
        public bool UserAllowedToVote { get; set; }
    }

    public class UpdatePollViewModel
    {
        public Guid PollId { get; set; }
        public Guid AnswerId { get; set; }
    }

    public class MoveTopicViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public Guid CategoryId { get; set; }
        public List<Category> Categories { get; set; }
    }

    public class NotifyNewTopicViewModel
    {
        public Guid CategoryId { get; set; }
    }

    #endregion

}