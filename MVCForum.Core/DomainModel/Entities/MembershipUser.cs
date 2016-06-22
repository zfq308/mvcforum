using System;
using System.Linq;
using System.Collections.Generic;
using MVCForum.Domain.DomainModel.Entities;
using MVCForum.Utilities;
using System.Data.SqlTypes;

namespace MVCForum.Domain.DomainModel
{
    /// <summary>
    /// 创建用户的状态信息枚举
    /// </summary>
    public enum MembershipCreateStatus
    {
        Success,
        DuplicateUserName,
        DuplicateEmail,
        InvalidPassword,
        InvalidEmail,
        InvalidAnswer,
        InvalidQuestion,
        InvalidUserName,
        ProviderError,
        UserRejected
    }

    /// <summary>
    /// 用户实体类
    /// </summary>
    public partial class MembershipUser : Entity
    {
        public MembershipUser()
        {
            Id = GuidComb.GenerateComb();
            PasswordQuestion = "";
            PasswordAnswer = "";
            LastPasswordChangedDate = (DateTime)SqlDateTime.MinValue;
            FailedPasswordAnswerAttempt = 0;
            PasswordResetToken = "";
            PasswordResetTokenCreatedAt = (DateTime)SqlDateTime.MinValue;
            IsLockedOut = false;
            IsBanned = false;
            LastLockoutDate = (DateTime)SqlDateTime.MinValue;
            LastActivityDate = null;
            LoginIdExpires = null;
            HasAgreedToTermsAndConditions = true;
            DisableEmailNotifications = false;
        }

        #region 用户基本信息属性
        /// <summary>
        /// 账户Id
        /// </summary>
        public Guid Id { get; set; }
        /// <summary>
        /// 账号
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// 用户真实姓名
        /// </summary>
        public string RealName { get; set; }

        /// <summary>
        /// 用户呢称
        /// </summary>
        public string AliasName { get; set; }

        /// <summary>
        /// 性别
        /// </summary>
        public int Gender { get; set; }
        /// <summary>
        /// 用户生日
        /// </summary>
        public DateTime Birthday { get; set; }
        /// <summary>
        /// 年龄
        /// </summary>
        public int Age { get { return (int)(DateTime.Now.Subtract(Birthday).TotalDays / 365); } }
        /// <summary>
        /// 生日所属历法, false 为公历， true 为农历
        /// </summary>
        public bool IsLunarCalendar { get; set; }
        /// <summary>
        /// 婚否
        /// </summary>
        public bool IsMarried { get; set; }
        /// <summary>
        /// 身高
        /// </summary>
        public int Height { get; set; }
        /// <summary>
        /// 体重
        /// </summary>
        public int Weight { get; set; }
        /// <summary>
        /// 学历
        /// </summary>
        public string Education { get; set; }
        /// <summary>
        /// 用户现居住地
        /// </summary>
        public string Location { get; set; }
        /// <summary>
        /// 学校，省
        /// </summary>
        public string SchoolProvince { get; set; }
        /// <summary>
        /// 学校，市
        /// </summary>
        public string SchoolCity { get; set; }
        /// <summary>
        /// 学校名称
        /// </summary>
        public string SchoolName { get; set; }

        /// <summary>
        /// 家乡，省
        /// </summary>
        public string HomeTownProvince { get; set; }
        /// <summary>
        /// 家乡，市
        /// </summary>
        public string HomeTownCity { get; set; }
        /// <summary>
        /// 家乡，县区
        /// </summary>
        public string HomeTownCounty { get; set; }
        /// <summary>
        /// 职业
        /// </summary>
        public string Job { set; get; }
        /// <summary>
        /// 月收入区间，（从枚举中取得）
        /// </summary>
        public int IncomeRange { get; set; }
        /// <summary>
        /// 个人兴趣
        /// </summary>
        public string Interest { get; set; }
        /// <summary>
        /// 手机号码
        /// </summary>
        public string MobilePhone { get; set; }
        #endregion

        #region 密码管理相关属性
        /// <summary>
        /// 密码
        /// </summary>
        public string Password { get; set; }
        /// <summary>
        /// 密码Salt
        /// </summary>
        public string PasswordSalt { get; set; }
        /// <summary>
        /// 密码提示问题
        /// </summary>
        public string PasswordQuestion { get; set; }
        /// <summary>
        /// 密码提示答案
        /// </summary>
        public string PasswordAnswer { get; set; }
        /// <summary>
        /// 最后的密码修改时间
        /// </summary>
        public DateTime LastPasswordChangedDate { get; set; }
        /// <summary>
        /// 密码提示回答错误次数
        /// </summary>
        public int FailedPasswordAnswerAttempt { get; set; }
        /// <summary>
        /// 密码重置Token
        /// </summary>
        public string PasswordResetToken { get; set; }
        /// <summary>
        /// 密码重置Token生成时间
        /// </summary>
        public DateTime? PasswordResetTokenCreatedAt { get; set; }
        #endregion

        #region 用户状态相关属性

        /// <summary>
        /// 是否通过账户审核
        /// </summary>
        public bool IsApproved { get; set; }
        /// <summary>
        /// 是否锁定此账户
        /// </summary>
        public bool IsLockedOut { get; set; }
        /// <summary>
        /// 是否禁用此账户
        /// </summary>
        public bool IsBanned { get; set; }
        /// <summary>
        /// 账户注册时间
        /// </summary>
        public DateTime CreateDate { get; set; }
        /// <summary>
        /// 最后登录时间
        /// </summary>
        public DateTime LastLoginDate { get; set; }
        /// <summary>
        /// 记录审核通过后的更新时间
        /// </summary>
        public DateTime LastUpdateTime { get; set; }
        /// <summary>
        /// 最后的锁定时间
        /// </summary>
        public DateTime LastLockoutDate { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public DateTime? LastActivityDate { get; set; }
        /// <summary>
        /// 登录失败次数
        /// </summary>
        public int FailedPasswordAttemptCount { get; set; }
        /// <summary>
        /// 账号登录的过期时间
        /// </summary>
        public DateTime? LoginIdExpires { get; set; }
        /// <summary>
        /// 用户是否已经同意相关系统要求的协议
        /// </summary>
        public bool? HasAgreedToTermsAndConditions { get; set; }

        #endregion

        #region 用户功能定义属性

        public bool? DisableEmailNotifications { get; set; }
        /// <summary>
        /// 是否禁用用户的回帖功能
        /// </summary>
        public bool? DisablePosting { get; set; }
        /// <summary>
        /// 是否禁用用户的私信功能
        /// </summary>
        public bool? DisablePrivateMessages { get; set; }
        /// <summary>
        /// 是否禁止用户使用文件上传功能
        /// </summary>
        public bool? DisableFileUploads { get; set; }

        #endregion

        #region 相关管理集合属性定义
        /// <summary>
        /// 用户拥有的角色清单
        /// </summary>
        public virtual IList<MembershipRole> Roles { get; set; }
        public virtual IList<Post> Posts { get; set; }
        public virtual IList<Topic> Topics { get; set; }
        public virtual IList<Vote> Votes { get; set; }
        public virtual IList<Vote> VotesGiven { get; set; }
        public virtual IList<Badge> Badges { get; set; }
        public virtual IList<BadgeTypeTimeLastChecked> BadgeTypesTimeLastChecked { get; set; }
        public virtual IList<CategoryNotification> CategoryNotifications { get; set; }
        public virtual IList<TopicNotification> TopicNotifications { get; set; }
        public virtual IList<TagNotification> TagNotifications { get; set; }
        public virtual IList<MembershipUserPoints> Points { get; set; }
        public virtual IList<PrivateMessage> PrivateMessagesReceived { get; set; }
        public virtual IList<PrivateMessage> PrivateMessagesSent { get; set; }
        public virtual IList<Poll> Polls { get; set; }
        public virtual IList<PollVote> PollVotes { get; set; }
        public virtual IList<Favourite> Favourites { get; set; }
        public virtual IList<UploadedFile> UploadedFiles { get; set; }
        public virtual IList<Block> BlockedUsers { get; set; }
        public virtual IList<Block> BlockedByOtherUsers { get; set; }
        public virtual IList<PostEdit> PostEdits { get; set; }

        #endregion

        #region 其他基本属性

       
        /// <summary>
        /// 用户类别，用于未来机构用户拓展
        /// </summary>
        public int UserType { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        public string Comment { get; set; }
        /// <summary>
        /// 搜索聚合字段
        /// </summary>
        public string Slug { get; set; }
        /// <summary>
        /// 用户签名
        /// </summary>
        public string Signature { get; set; }
      
        public string NiceUrl => UrlTypes.GenerateUrl(UrlType.Member, Slug);

        #endregion

        #region 拓展属性
        /// <summary>
        /// 电子邮件地址
        /// </summary>
        public string Email { get; set; }
        /// <summary>
        /// 用户WebSite
        /// </summary>
        public string Website { get; set; }
        /// <summary>
        /// 用户Twitter
        /// </summary>
        public string Twitter { get; set; }
        /// <summary>
        /// 用户Facebook
        /// </summary>
        public string Facebook { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Avatar { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string FacebookAccessToken { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public long? FacebookId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string TwitterAccessToken { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string TwitterId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string GoogleAccessToken { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string GoogleId { get; set; }
        /// <summary>
        /// MicrosoftAccessToken
        /// </summary>
        public string MicrosoftAccessToken { get; set; }
        /// <summary>
        /// MicrosoftId
        /// </summary>
        public string MicrosoftId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool? TwitterShowFeed { get; set; }
        /// <summary>
        /// QQ
        /// </summary>
        public string QQ { get; set; }
        /// <summary>
        /// 纬度信息
        /// </summary>
        public string Latitude { get; set; }
        /// <summary>
        /// 经度信息
        /// </summary>
        public string Longitude { get; set; }
        /// <summary>
        /// 是否为外部账号
        /// </summary>
        public bool? IsExternalAccount { get; set; }

        public string MiscAccessToken { get; set; }

        /// <summary>
        /// 勋章点数
        /// </summary>
        public int TotalPoints
        {
            get
            {
                return Points?.Select(x => x.Points).Sum() ?? 0;
            }
        }

        #endregion



    }

}
