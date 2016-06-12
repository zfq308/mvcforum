using System;
using MVCForum.Utilities;

namespace MVCForum.Domain.DomainModel
{
    /// <summary>
    /// 系统设定配置类
    /// </summary>
    public partial class Settings : Entity
    {
        public Settings()
        {
            Id = GuidComb.GenerateComb();
        }
        public Guid Id { get; set; }

        /// <summary>
        /// 系统名称
        /// </summary>
        public string ForumName { get; set; }
        /// <summary>
        /// 系统发布后对应的URL
        /// </summary>
        public string ForumUrl { get; set; }
        /// <summary>
        /// 页面标题
        /// </summary>
        public string PageTitle { get; set; }
        /// <summary>
        /// 页面元数据描述
        /// </summary>
        public string MetaDesc { get; set; }
        /// <summary>
        /// 系统是否关闭中
        /// </summary>
        public bool IsClosed { get; set; }
        /// <summary>
        /// 支持RSS Feed
        /// </summary>
        public bool EnableRSSFeeds { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool DisplayEditedBy { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool EnablePostFileAttachments { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool EnableMarkAsSolution { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int? MarkAsSolutionReminderTimeFrame { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool EnableSpamReporting { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool EnableMemberReporting { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool EnableEmailSubscriptions { get; set; }
        /// <summary>
        /// 是否采用手动审核新用户的方式
        /// </summary>
        public bool ManuallyAuthoriseNewMembers { get; set; }
        /// <summary>
        /// 新用户是否需要采用注册邮件的验证
        /// </summary>
        public bool? NewMemberEmailConfirmation { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool EmailAdminOnNewMemberSignUp { get; set; }
        /// <summary>
        /// 每页主题数
        /// </summary>
        public int TopicsPerPage { get; set; }
        /// <summary>
        /// 每页帖子数
        /// </summary>
        public int PostsPerPage { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int ActivitiesPerPage { get; set; }
        /// <summary>
        /// 系统是否支持私信功能
        /// </summary>
        public bool EnablePrivateMessages { get; set; }
        /// <summary>
        /// 每个用户所支持的最大私信数量
        /// </summary>
        public int MaxPrivateMessagesPerMember { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int PrivateMessageFloodControl { get; set; }
        /// <summary>
        /// 是否支持用户签名
        /// </summary>
        public bool EnableSignatures { get; set; }
        /// <summary>
        /// 系统是否支持用户激励点数功能
        /// </summary>
        public bool EnablePoints { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int? PointsAllowedForExtendedProfile { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int PointsAllowedToVoteAmount { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int PointsAddedPerPost { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int PointsAddedPostiveVote { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int PointsDeductedNagativeVote { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int PointsAddedForSolution { get; set; }
        /// <summary>
        /// Admin的邮件地址
        /// </summary>
        public string AdminEmailAddress { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string NotificationReplyEmail { get; set; }

        #region SMTP 相关设定

        /// <summary>
        /// 外发邮件使用的SMTP Server名称
        /// </summary>
        public string SMTP { get; set; }
        /// <summary>
        /// 外发邮件使用的SMTP服务用的用户名
        /// </summary>
        public string SMTPUsername { get; set; }
        /// <summary>
        /// 外发邮件使用的SMTP服务用的密码
        /// </summary>
        public string SMTPPassword { get; set; }
        /// <summary>
        /// 外发邮件使用的SMTP服务用的端口
        /// </summary>
        public string SMTPPort { get; set; }
        /// <summary>
        /// 外发邮件使用的SMTP服务使用SSL协议
        /// </summary>
        public bool? SMTPEnableSSL { get; set; }

        #endregion

        /// <summary>
        /// 系统设定的“主题”名称
        /// </summary>
        public string Theme { get; set; }
        public bool? EnableSocialLogins { get; set; }
        public string SpamQuestion { get; set; }
        public string SpamAnswer { get; set; }
        public bool? EnableAkisment { get; set; }
        public string AkismentKey { get; set; }
        public string CurrentDatabaseVersion { get; set; }
        public bool? EnablePolls { get; set; }
        public bool? SuspendRegistration { get; set; }
        public string CustomHeaderCode { get; set; }
        public string CustomFooterCode { get; set; }






        #region 云之讯接入配置项
       
        public string UCPaasConfig_Account { get; set; }
        public string UCPaasConfig_Token { get; set; }
        public string UCPaasConfig_AppId { get; set; }
        public string UCPaasConfig_TemplatedId { get; set; }

        #endregion

        /// <summary>
        /// 是否支持表情图标
        /// </summary>
        public bool? EnableEmoticons { get; set; }
        /// <summary>
        /// 禁用“踩”按钮
        /// </summary>
        public bool DisableDislikeButton { get; set; }
        /// <summary>
        /// 是否支持网站规定和相关免责协议
        /// </summary>
        public bool? AgreeToTermsAndConditions { get; set; }
        /// <summary>
        /// 网站规定和相关免责协议文本
        /// </summary>
        public string TermsAndConditions { get; set; }
        /// <summary>
        /// 是否禁用标准注册模式
        /// </summary>
        public bool? DisableStandardRegistration { get; set; }
        /// <summary>
        /// 新用户开始时所拥有的角色
        /// </summary>
        public virtual MembershipRole NewMemberStartingRole { get; set; }
        /// <summary>
        /// 系统采用的默认语言
        /// </summary>
        public virtual Language DefaultLanguage { get; set; }
    }
}
