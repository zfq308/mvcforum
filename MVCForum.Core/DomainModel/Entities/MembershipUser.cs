using System;
using System.Linq;
using System.Collections.Generic;
using MVCForum.Domain.DomainModel.Entities;
using MVCForum.Utilities;
using System.Data.SqlTypes;

namespace MVCForum.Domain.DomainModel
{
    #region 相关枚举定义

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
    /// 会员类型
    /// </summary>
    public enum Enum_UserType
    {
        /// <summary>
        /// A类
        /// </summary>
        A = 0,
        /// <summary>
        /// B类
        /// </summary>
        B = 1,
        /// <summary>
        /// C类
        /// </summary>
        C = 2,
        /// <summary>
        /// D类
        /// </summary>
        D = 3,
        /// <summary>
        /// E类
        /// </summary>
        E = 4,
    }

    /// <summary>
    /// 性别枚举
    /// </summary>
    public enum Enum_Gender
    {
        /// <summary>
        /// 男
        /// </summary>
        boy = 1,
        /// <summary>
        /// 女
        /// </summary>
        girl = 0,
    }

    /// <summary>
    /// 历法枚举
    /// </summary>
    public enum Enum_Calendar
    {
        /// <summary>
        /// 阴历，中国历
        /// </summary>
        LunarCalendar = 1,
        /// <summary>
        /// 公历，
        /// </summary>
        PublicCalendar = 2,
    }

    /// <summary>
    /// 婚姻状态
    /// </summary>
    public enum Enum_MarriedStatus
    {
        /// <summary>
        /// 已婚
        /// </summary>
        Married = 1,
        /// <summary>
        /// 未婚
        /// </summary>
        Single = 0,
    }

    /// <summary>
    /// 年龄段枚举
    /// </summary>
    public enum Enum_AgeRange
    {
        R_LowerThan20Year = 0,
        R_20YearsTo25Year = 1,
        R_25YearsTo30Year = 2,
        R_30YearsTo35Year = 3,
        R_35YearsTo40Year = 4,
        R_40YearsTo50Year = 5,
        R_GreatThan50Year = 6,
    }


    public enum Enum_IncomeRange
    {
        R_Lowthan1W = 1,
        R_1WTo5W = 2,
        R_5WMore = 3,
        R_NOClass = 4
    }
    #endregion

    /// <summary>
    /// 用户实体类
    /// </summary>
    public partial class MembershipUser : Entity
    {
        public MembershipUser()
        {
            #region 基本字段

            Id = GuidComb.GenerateComb();
            UserName = "";
            RealName = "";
            AliasName = "";
            Gender = Enum_Gender.boy;
            Birthday = new DateTime(2000, 1, 1);
            IsLunarCalendar = Enum_Calendar.PublicCalendar;
            IsMarried = Enum_MarriedStatus.Single;
            Height = 0;
            Weight = 0;
            Education = "";
            HomeTown = "";
            SchoolProvince = "110000";
            SchoolCity = "110100";
            SchoolName = "";
            LocationProvince = "110000";
            LocationCity = "110100";
            LocationCounty = "110108";
            Job = "";
            IncomeRange = Enum_IncomeRange.R_NOClass;
            Interest = "";
            MobilePhone = "";
            UserType = Enum_UserType.A;
            Comment = "";
            Avatar = "";

            #endregion

            #region 密码

            Password = "";
            LastPasswordChangedDate = (DateTime)SqlDateTime.MinValue;
            PasswordResetTokenCreatedAt = (DateTime)SqlDateTime.MinValue;

            #endregion

            IsApproved = false; // 设定每个用户注册都需要审核，系统不允许自动审核

            #region 其他

            Slug = "";
            Signature = "";
            Email = "";
            IsLockedOut = false;
            IsBanned = false;
            LastLockoutDate = (DateTime)SqlDateTime.MinValue;
            LastActivityDate = null;
            LoginIdExpires = null;
            HasAgreedToTermsAndConditions = true;
            DisableEmailNotifications = false;

            IsLockedOut = false;
            DisablePosting = false;
            DisablePrivateMessages = false;
            DisableFileUploads = false;
            LastLoginDate = (DateTime)SqlDateTime.MinValue;

            #endregion
        }

        #region 用户基本信息属性
        /// <summary>
        /// 账户Id（GUID）码
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
        public Enum_Gender Gender { get; set; }
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
        public Enum_Calendar IsLunarCalendar { get; set; }
        /// <summary>
        /// 婚否
        /// </summary>
        public Enum_MarriedStatus IsMarried { get; set; }
        /// <summary>
        /// 身高CM
        /// </summary>
        public int Height { get; set; }
        /// <summary>
        /// 体重Kg
        /// </summary>
        public int Weight { get; set; }
        /// <summary>
        /// 学历
        /// </summary>
        public string Education { get; set; }
        /// <summary>
        /// 家乡
        /// </summary>
        public string HomeTown { get; set; }
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
        /// 居住地，省
        /// </summary>
        public string LocationProvince { get; set; }
        /// <summary>
        /// 居住地，市
        /// </summary>
        public string LocationCity { get; set; }
        /// <summary>
        /// 居住地，县区
        /// </summary>
        public string LocationCounty { get; set; }
        /// <summary>
        /// 职业
        /// </summary>
        public string Job { set; get; }
        /// <summary>
        /// 月收入区间，（从枚举中取得）
        /// </summary>
        public Enum_IncomeRange IncomeRange { get; set; }
        /// <summary>
        /// 个人兴趣
        /// </summary>
        public string Interest { get; set; }
        /// <summary>
        /// 手机号码
        /// </summary>
        public string MobilePhone { get; set; }
        /// <summary>
        /// 用户类别
        /// </summary>
        public Enum_UserType UserType { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        public string Comment { get; set; }
        /// <summary>
        /// 头像地址
        /// </summary>
        public string Avatar { get; set; }

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
        /// 审核意见
        /// </summary>
        public string AuditComments { set; get; }
        /// <summary>
        /// 是否通过账户审核
        /// </summary>
        public bool IsApproved { get; set; }
        /// <summary>
        /// 是否锁定此账户
        /// </summary>
        public bool IsLockedOut { get; set; }
        /// <summary>
        /// 是否禁用此账户（隐藏用户）
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
        /// 最新活跃时间（用来判断用户在线状态）
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
        /// <summary>
        /// 禁用邮件通知功能
        /// </summary>
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

    /// <summary>
    /// 会员搜索模型类
    /// </summary>
    public partial class MembershipUserSearchModel
    {
        public string UserName { get; set; }
        public string AliasName { get; set; }
        public string RealName { get; set; }
        public Enum_Gender Gender { get; set; }
        public Enum_AgeRange AgeRange { get; set; }
        public Enum_MarriedStatus IsMarried { get; set; }
        public int Height { get; set; }
        public string Education { get; set; }
        public string SchoolName { get; set; }

        public string HomeTown { get; set; }

        public string LocationProvince { get; set; }
        public string LocationCity { get; set; }
        public string LocationCounty { get; set; }
        public string Job { get; set; }

        public Enum_IncomeRange IncomeRange { get; set; }

        /// <summary>
        /// 离最后登录间隔天数
        /// </summary>
        public int LastLoginDateInterval { get; set; }

        public Enum_UserType UserType { get; set; }
        public bool IsApproved { get; set; }
        public bool IsLockedOut { get; set; }


    }

    /// <summary>
    /// 用户属性变更记录项
    /// </summary>
    public partial class MembershipPropertyChange : Entity
    {
        #region 建构式

        public MembershipPropertyChange()
        {
            ChangeId = GuidComb.GenerateComb();
        }

        public MembershipPropertyChange(Guid userid, string propertyname, string propertytype, string sourcevalue, string targetvalue)
        {
            ChangeId = GuidComb.GenerateComb();
            UserId = userid;
            PropertyName = propertyname;
            PropertyTypeName = propertytype;
            SourceValue = sourcevalue;
            TargetValue = targetvalue;
        }

        #endregion

        #region 属性

        /// <summary>
        /// 用户属性变更Id
        /// </summary>
        public Guid ChangeId { get; set; }
        /// <summary>
        /// 用户属性变更申请记录的生成时间
        /// </summary>
        public DateTime CreateDate { get; set; }
        /// <summary>
        /// 要变更的属性名
        /// </summary>
        public string PropertyName { get; set; }
        /// <summary>
        /// 属性的数据类型，string, int or Datetime
        /// </summary>
        public string PropertyTypeName { get; set; }
        /// <summary>
        /// 变更前的值
        /// </summary>
        public string SourceValue { get; set; }
        /// <summary>
        /// 变更后的值
        /// </summary>
        public string TargetValue { get; set; }
        /// <summary>
        /// 审核标记
        /// </summary>
        public int ApprovedFlag { get; set; }
        /// <summary>
        /// 审核时间
        /// </summary>
        public DateTime ApprovedTime { get; set; }
        /// <summary>
        /// 审核人
        /// </summary>
        public string ApprovedMan { get; set; }
        /// <summary>
        /// 审核结论
        /// </summary>
        public string ApproveMessage { get; set; }

        /// <summary>
        /// 申请变更的用户Id
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// 申请变更的用户实例
        /// </summary>
        public virtual MembershipUser User { get; set; }

        #endregion

    }

    /// <summary>
    /// 今日之星特权状态枚举
    /// </summary>
    public enum MembershipTodayStarStatus
    {
        /// <summary>
        /// 今日之星特权生效中
        /// </summary>
        Valid = 1,
        /// <summary>
        /// 今日之星特权无效中
        /// </summary>
        Invalid = 0,
    }

    /// <summary>
    /// 用户今日之星特权的实体类
    /// </summary>
    public partial class MembershipTodayStar : Entity
    {
        /// <summary>
        /// 默认建构式
        /// </summary>
        public MembershipTodayStar()
        {
            Id = GuidComb.GenerateComb();
        }

        /// <summary>
        /// 用户今日之星特权的实体类的参数化建构式
        /// </summary>
        /// <param name="userid">今日之星特权的用户Id</param>
        /// <param name="starttime">今日之星特权的起始时间</param>
        /// <param name="stoptime">今日之星特权的结束时间</param>
        /// <param name="status">今日之星特权的状态</param>
        public MembershipTodayStar(Guid userid, DateTime starttime, DateTime stoptime, MembershipTodayStarStatus status)
        {
            Id = GuidComb.GenerateComb();
            UserId = userid;
            StartTime = starttime;
            StopTime = stoptime;
            Status = (int)status;
        }

        #region 相关属性

        /// <summary>
        /// 今日之星流水Id
        /// </summary>
        public Guid Id { get; set; }
        /// <summary>
        /// 申请今日之星的用户Id
        /// </summary>
        public Guid UserId { get; set; }
        /// <summary>
        /// 今日之星申请记录的生成时间
        /// </summary>
        public DateTime CreateDate { get; set; }
        /// <summary>
        /// 记录操作员信息
        /// </summary>
        public string Operator { get; set; }
        /// <summary>
        /// 今日之星记录的状态：1为有效，0为无效
        /// </summary>
        public int Status { get; set; }
        /// <summary>
        /// 今日之星操作票据流水号，默认为空，有值时为支付/购买交易的流水号
        /// </summary>
        public string JobId { get; set; }
        /// <summary>
        /// 今日之星特权的起始时间
        /// </summary>
        public DateTime StartTime { get; set; }
        /// <summary>
        /// 今日之星特权的结束时间
        /// </summary>
        public DateTime StopTime { get; set; }

        #endregion


    }


}
