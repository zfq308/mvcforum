using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Data.Entity;
using System.Text;
using System.Linq;
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
using System.Diagnostics;
using DataGenerationFramework.Core;
using MVCForum.Domain.DomainModel.General;

namespace MVCForum.Services
{
    public partial class MembershipService : IMembershipService
    {
        log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #region 变量定义

        /// <summary>
        /// 密码重置操作最长有效小时数
        /// </summary>
        private const int MaxHoursToResetPassword = 48;
        private readonly MVCForumContext _context;
        private readonly IEmailService _emailService;
        private readonly IPostService _postService;
        private readonly IPollVoteService _pollVoteService;
        private readonly IPollAnswerService _pollAnswerService;
        private readonly IFavouriteService _favouriteService;
        private readonly ISettingsService _settingsService;
        private readonly IPollService _pollService;
        private readonly ITopicService _topicService;
        private readonly IActivityService _activityService;
        private readonly ILocalizationService _localizationService;
        private readonly IPrivateMessageService _privateMessageService;
        private readonly IMembershipUserPointsService _membershipUserPointsService;
        private readonly ITopicNotificationService _topicNotificationService;
        private readonly IUploadedFileService _uploadedFileService;
        private readonly IVoteService _voteService;
        private readonly IBadgeService _badgeService;
        private readonly ICategoryNotificationService _categoryNotificationService;
        private readonly ILoggingService _loggingService;
        private readonly ICategoryService _categoryService;
        private readonly IPostEditService _postEditService;

        #endregion

        #region 建构式

        /// <summary>
        /// 建构式
        /// </summary>
        /// <param name="context"></param>
        /// <param name="settingsService"> </param>
        /// <param name="emailService"> </param>
        /// <param name="localizationService"> </param>
        /// <param name="activityService"> </param>
        /// <param name="privateMessageService"> </param>
        /// <param name="membershipUserPointsService"> </param>
        /// <param name="topicNotificationService"> </param>
        /// <param name="voteService"> </param>
        /// <param name="badgeService"> </param>
        /// <param name="categoryNotificationService"> </param>
        /// <param name="loggingService"></param>
        /// <param name="uploadedFileService"></param>
        /// <param name="postService"></param>
        /// <param name="pollVoteService"></param>
        /// <param name="pollAnswerService"></param>
        /// <param name="pollService"></param>
        /// <param name="topicService"></param>
        /// <param name="favouriteService"></param>
        /// <param name="categoryService"></param>
        public MembershipService(IMVCForumContext context, ISettingsService settingsService,
            IEmailService emailService, ILocalizationService localizationService, IActivityService activityService,
            IPrivateMessageService privateMessageService, IMembershipUserPointsService membershipUserPointsService,
            ITopicNotificationService topicNotificationService, IVoteService voteService, IBadgeService badgeService,
            ICategoryNotificationService categoryNotificationService, ILoggingService loggingService, IUploadedFileService uploadedFileService,
            IPostService postService, IPollVoteService pollVoteService, IPollAnswerService pollAnswerService,
            IPollService pollService, ITopicService topicService, IFavouriteService favouriteService,
            ICategoryService categoryService, IPostEditService postEditService)
        {
            _settingsService = settingsService;
            _emailService = emailService;
            _localizationService = localizationService;
            _activityService = activityService;
            _privateMessageService = privateMessageService;
            _membershipUserPointsService = membershipUserPointsService;
            _topicNotificationService = topicNotificationService;
            _voteService = voteService;
            _badgeService = badgeService;
            _categoryNotificationService = categoryNotificationService;
            _loggingService = loggingService;
            _uploadedFileService = uploadedFileService;
            _postService = postService;
            _pollVoteService = pollVoteService;
            _pollAnswerService = pollAnswerService;
            _pollService = pollService;
            _topicService = topicService;
            _favouriteService = favouriteService;
            _categoryService = categoryService;
            _postEditService = postEditService;
            _context = context as MVCForumContext;
        }

        #endregion

        #region 创建用户实例

        /// <summary>
        /// 创建一个新的，用默认值填充的，没有保存的用户实例
        /// </summary>
        /// <returns></returns>
        public MembershipUser CreateEmptyUser()
        {
            var now = DateTime.Now;

            return new MembershipUser
            {
                UserName = string.Empty,
                Password = string.Empty,
                RealName = string.Empty,
                CreateDate = now,
            };
        }

        public MembershipUser Add(MembershipUser newUser)
        {
            return _context.MembershipUser.Add(newUser);
        }

        /// <summary>
        /// 净化用户实体类的实例的各个属性，使之无违例（屏蔽词）和注入的风险
        /// </summary>
        /// <param name="membershipUser"></param>
        /// <returns></returns>
        public MembershipUser SanitizeUser(MembershipUser membershipUser)
        {
            membershipUser.Comment = StringUtils.SafePlainText(membershipUser.Comment);
            membershipUser.Password = StringUtils.SafePlainText(membershipUser.Password);
            membershipUser.PasswordAnswer = StringUtils.SafePlainText(membershipUser.PasswordAnswer);
            membershipUser.PasswordQuestion = StringUtils.SafePlainText(membershipUser.PasswordQuestion);
            membershipUser.Signature = StringUtils.GetSafeHtml(membershipUser.Signature, true);
            membershipUser.UserName = StringUtils.SafePlainText(membershipUser.UserName);
            return membershipUser;
        }

        private MembershipUser setIsApprovedStatus(MembershipUser newUser)
        {
            if (newUser != null)
            {
                //var settings = _settingsService.GetSettings(false);
                //var manuallyAuthoriseMembers = settings.ManuallyAuthoriseNewMembers;
                //var memberEmailAuthorisationNeeded = settings.NewMemberEmailConfirmation == true;
                //if (manuallyAuthoriseMembers || memberEmailAuthorisationNeeded)
                //{
                //    newUser.IsApproved = false;
                //}
                //else
                //{
                //    newUser.IsApproved = true;
                //}

                newUser.IsApproved = false;
                return newUser;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// 创建用户实例
        /// </summary>
        /// <param name="newUser"></param>
        /// <returns></returns>
        public MembershipCreateStatus CreateUser(MembershipUser newUser)
        {
            var status = MembershipCreateStatus.Success;

            newUser = SanitizeUser(newUser);

            var e = new RegisterUserEventArgs { User = newUser };
            EventManager.Instance.FireBeforeRegisterUser(this, e);
            if (e.Cancel)
            {
                status = e.CreateStatus;
            }
            else
            {
                if (string.IsNullOrEmpty(newUser.UserName)) { status = MembershipCreateStatus.InvalidUserName; }
                if (GetUser(newUser.UserName, true) != null) { status = MembershipCreateStatus.DuplicateUserName; }
                if (GetUserByMobilePhone(newUser.MobilePhone, true) != null) { status = MembershipCreateStatus.DuplicateTelphone; }

                //if (GetUserByEmail(newUser.Email, true) != null) { status = MembershipCreateStatus.DuplicateEmail; }
                if (string.IsNullOrEmpty(newUser.Password)) { status = MembershipCreateStatus.InvalidPassword; }

                if (status == MembershipCreateStatus.Success)
                {
                    newUser.Roles = new List<MembershipRole> { _settingsService.GetSettings(false).NewMemberStartingRole };

                    #region 设定审核标志位

                    newUser = setIsApprovedStatus(newUser);

                    #endregion

                    #region 生成用户的密码的HASH值和SALT值

                    var salt = StringUtils.CreateSalt(AppConstants.SaltSize);
                    var hash = StringUtils.GenerateSaltedHash(newUser.Password, salt);
                    newUser.Password = hash;
                    newUser.PasswordSalt = salt;

                    #endregion

                    #region 其他属性赋值

                    newUser.CreateDate = newUser.LastPasswordChangedDate = DateTime.Now;
                    newUser.LastLockoutDate = (DateTime)SqlDateTime.MinValue;
                    newUser.LastLoginDate = (DateTime)SqlDateTime.MinValue;
                    newUser.IsLockedOut = false;
                    newUser.Slug = ServiceHelpers.GenerateSlug(newUser.UserName, GetUserBySlugLike(ServiceHelpers.CreateUrl(newUser.UserName)), null);

                    #endregion

                    try
                    {
                        Add(newUser);

                        #region 新用户加入时给管理员发邮件（此功能已禁用）
                        //if (settings.EmailAdminOnNewMemberSignUp)
                        //{
                        //    var sb = new StringBuilder();
                        //    sb.AppendFormat("<p>{0}</p>", string.Format(_localizationService.GetResourceString("Members.NewMemberRegistered"), settings.ForumName, settings.ForumUrl));
                        //    sb.AppendFormat("<p>{0} - {1}</p>", newUser.UserName, newUser.Email);
                        //    var email = new Email
                        //    {
                        //        EmailTo = settings.AdminEmailAddress,
                        //        NameTo = _localizationService.GetResourceString("Members.Admin"),
                        //        Subject = _localizationService.GetResourceString("Members.NewMemberSubject")
                        //    };
                        //    email.Body = _emailService.EmailTemplate(email.NameTo, sb.ToString());
                        //    _emailService.SendMail(email);
                        //}
                        #endregion

                        //生成用户注册活动记录
                        _activityService.MemberJoined(newUser);
                        EventManager.Instance.FireAfterRegisterUser(this, new RegisterUserEventArgs { User = newUser });
                    }
                    catch (Exception)
                    {
                        status = MembershipCreateStatus.UserRejected;
                    }
                }
            }

            return status;
        }

        /// <summary>
        /// 生成50个待审核通过的测试账号
        /// </summary>
        public void Create50TestAccount()
        {
            ReflectionDataGenerate.ForClass<MembershipUser>().Property(f => f.UserName).SetStringTypeEnum(EnumStringType.RandomString, 6, 6);
            ReflectionDataGenerate.ForClass<MembershipUser>().Property(f => f.RealName).SetStringTypeEnum(EnumStringType.HumanData_ChineseName);
            ReflectionDataGenerate.ForClass<MembershipUser>().Property(f => f.Email).SetStringTypeEnum(EnumStringType.HumanData_EmailAddress);
            ReflectionDataGenerate.ForClass<MembershipUser>().Property(f => f.Birthday).Range(new DateTime(1980, 1, 1), new DateTime(2003, 1, 1));
            ReflectionDataGenerate.ForClass<MembershipUser>().Property(f => f.Height).UseMin(150).UseMax(200);
            ReflectionDataGenerate.ForClass<MembershipUser>().Property(f => f.Weight).UseMin(45).UseMax(110);
            ReflectionDataGenerate.ForClass<MembershipUser>().Property(f => f.HomeTown).SetStringTypeEnum(EnumStringType.GEOData_ChineseHomeTown);
            ReflectionDataGenerate.ForClass<MembershipUser>().Property(f => f.SchoolName).SetStringTypeEnum(EnumStringType.HumanData_ChineseSchoolName);
            ReflectionDataGenerate.ForClass<MembershipUser>().Property(f => f.Interest).SetStringTypeEnum(EnumStringType.Language_ChineseFourWord);
            ReflectionDataGenerate.ForClass<MembershipUser>().Property(f => f.MobilePhone).SetStringTypeEnum(EnumStringType.HumanData_ChineseMobileNumber);
            ReflectionDataGenerate.ForClass<MembershipUser>().Property(f => f.IsApproved);
            ReflectionDataGenerate.ForClass<MembershipUser>().Property(f => f.Job).SetStringTypeEnum(EnumStringType.HumanData_ChineseJob);

            int count = 50;
            var items = ReflectionDataGenerate.Items<MembershipUser>(count).ToList();
            var EducationList = new List<string>();
            EducationList.AddRange(TEducation.LoadAllEducationList().Select(x =>
            {
                return x.EducationId;
            }));
            var CountryList = TCountry.LoadAllCountry();

            for (int i = 0; i < count; i++)
            {
                var item = items[i];
                item.AliasName = item.RealName + item.UserName;
                item.Gender = RandomHelper.GetEnum<Enum_Gender>();
                item.IsLunarCalendar = RandomHelper.GetEnum<Enum_Calendar>();

                item.IsMarried = RandomHelper.GetEnum<Enum_MarriedStatus>();
                item.Education = RandomHelper.GetListElement<string>(EducationList);

                var schoolInfo = RandomHelper.GetListElement<TCountry>(CountryList);
                item.SchoolProvince = schoolInfo.ProvinceId.ToString();
                item.SchoolCity = schoolInfo.CityId.ToString();

                var locationInfo = RandomHelper.GetListElement<TCountry>(CountryList);
                item.LocationProvince = locationInfo.ProvinceId.ToString();
                item.LocationCity = locationInfo.CityId.ToString();
                item.LocationCounty = locationInfo.CountryId.ToString();
                item.IncomeRange = RandomHelper.GetEnum<Enum_IncomeRange>();
                item.Avatar = "";
                item.UserType = RandomHelper.GetEnum<Enum_UserType>();
                item.Password = "password";
                item.CreateDate = DateTime.Now;
                item.LastLoginDate = DateTime.Now;
                item.LastUpdateTime = DateTime.Now;
                item.Slug = ServiceHelpers.CreateUrl("Test" + i.ToString());
                item.DisablePosting = false;
                item.DisablePrivateMessages = false;
                item.DisableFileUploads = false;
                item.Comment = "Test" + i.ToString();

                // Hash the password
                var salt = StringUtils.CreateSalt(AppConstants.SaltSize);
                var hash = StringUtils.GenerateSaltedHash(item.Password, salt);
                item.Password = hash;
                item.PasswordSalt = salt;

                var standardRole = _context.MembershipRole.FirstOrDefault(x => x.RoleName == SiteConstants.Instance.StandardMembers);
                item.Roles = new List<MembershipRole> { standardRole };

                _context.MembershipUser.Add(item);
            }
        }

        /// <summary>
        /// 生成5个供应商测试账号
        /// </summary>
        public void Create5SupplierAccount()
        {
            ReflectionDataGenerate.ForClass<MembershipUser>().Property(f => f.UserName).SetStringTypeEnum(EnumStringType.RandomString, 6, 6);
            ReflectionDataGenerate.ForClass<MembershipUser>().Property(f => f.RealName).SetStringTypeEnum(EnumStringType.HumanData_ChineseName);
            ReflectionDataGenerate.ForClass<MembershipUser>().Property(f => f.Email).SetStringTypeEnum(EnumStringType.HumanData_EmailAddress);
            ReflectionDataGenerate.ForClass<MembershipUser>().Property(f => f.Birthday).Range(new DateTime(1980, 1, 1), new DateTime(2003, 1, 1));
            ReflectionDataGenerate.ForClass<MembershipUser>().Property(f => f.Height).UseMin(150).UseMax(200);
            ReflectionDataGenerate.ForClass<MembershipUser>().Property(f => f.Weight).UseMin(45).UseMax(110);
            ReflectionDataGenerate.ForClass<MembershipUser>().Property(f => f.HomeTown).SetStringTypeEnum(EnumStringType.GEOData_ChineseHomeTown);
            ReflectionDataGenerate.ForClass<MembershipUser>().Property(f => f.SchoolName).SetStringTypeEnum(EnumStringType.HumanData_ChineseSchoolName);
            ReflectionDataGenerate.ForClass<MembershipUser>().Property(f => f.Interest).SetStringTypeEnum(EnumStringType.Language_ChineseFourWord);
            ReflectionDataGenerate.ForClass<MembershipUser>().Property(f => f.MobilePhone).SetStringTypeEnum(EnumStringType.HumanData_ChineseMobileNumber);
            ReflectionDataGenerate.ForClass<MembershipUser>().Property(f => f.IsApproved);
            ReflectionDataGenerate.ForClass<MembershipUser>().Property(f => f.Job).SetStringTypeEnum(EnumStringType.HumanData_ChineseJob);

            int count = 5;
            var items = ReflectionDataGenerate.Items<MembershipUser>(count).ToList();
            var EducationList = new List<string>();
            EducationList.AddRange(TEducation.LoadAllEducationList().Select(x =>
            {
                return x.EducationId;
            }));
            var CountryList = TCountry.LoadAllCountry();

            for (int i = 0; i < count; i++)
            {
                var item = items[i];
                item.RealName = "Supplier" + item.UserName;
                item.AliasName = item.RealName;
                item.Gender = RandomHelper.GetEnum<Enum_Gender>();
                item.IsLunarCalendar = RandomHelper.GetEnum<Enum_Calendar>();

                item.IsMarried = RandomHelper.GetEnum<Enum_MarriedStatus>();
                item.Education = RandomHelper.GetListElement<string>(EducationList);

                var schoolInfo = RandomHelper.GetListElement<TCountry>(CountryList);
                item.SchoolProvince = schoolInfo.ProvinceId.ToString();
                item.SchoolCity = schoolInfo.CityId.ToString();

                var locationInfo = RandomHelper.GetListElement<TCountry>(CountryList);
                item.LocationProvince = locationInfo.ProvinceId.ToString();
                item.LocationCity = locationInfo.CityId.ToString();
                item.LocationCounty = locationInfo.CountryId.ToString();
                item.IncomeRange = RandomHelper.GetEnum<Enum_IncomeRange>();
                item.Avatar = "";
                item.UserType = RandomHelper.GetEnum<Enum_UserType>();
                item.Password = "password";
                item.CreateDate = DateTime.Now;
                item.LastLoginDate = DateTime.Now;
                item.LastUpdateTime = DateTime.Now;
                item.Slug = ServiceHelpers.CreateUrl("Test_Supplier" + i.ToString());
                item.DisablePosting = false;
                item.DisablePrivateMessages = false;
                item.DisableFileUploads = false;
                item.Comment = "Test" + i.ToString();

                // Hash the password
                var salt = StringUtils.CreateSalt(AppConstants.SaltSize);
                var hash = StringUtils.GenerateSaltedHash(item.Password, salt);
                item.Password = hash;
                item.PasswordSalt = salt;

                var SupplierRoleName = _context.MembershipRole.FirstOrDefault(x => x.RoleName == AppConstants.SupplierRoleName);
                item.Roles = new List<MembershipRole> { SupplierRoleName };

                _context.MembershipUser.Add(item);
            }
        }

        #endregion

        #region 查询用户实例

        /// <summary>
        /// 按用户账号Id查找用户实例
        /// </summary>
        /// <param name="id">用户账号Id</param>
        /// <returns></returns>
        public MembershipUser GetUser(Guid id)
        {
            return _context.MembershipUser
                  .Include(x => x.Roles)
                  .FirstOrDefault(x => x.Id == id);
        }

        /// <summary>
        /// 按账号名查找用户实例
        /// </summary>
        /// <param name="username">账号名</param>
        /// <param name="removeTracking"></param>
        /// <returns></returns>
        public MembershipUser GetUser(string username, bool removeTracking = false)
        {
            MembershipUser member = null;
            if (!string.IsNullOrEmpty(username))
            {
                if (removeTracking)
                {
                    member = _context.MembershipUser
                        .Include(x => x.Roles)
                        .AsNoTracking()
                        .FirstOrDefault(name => name.UserName.Equals(username, StringComparison.CurrentCultureIgnoreCase));
                }
                else
                {
                    member = _context.MembershipUser
                        .Include(x => x.Roles)
                        .FirstOrDefault(name => name.UserName.Equals(username, StringComparison.CurrentCultureIgnoreCase));
                }

                if (member == null && HttpContext.Current.User.Identity.Name.ToLower() == username.ToLower())
                {
                    // Member is null so doesn't exist, yet they are logged in with that username - Log them out
                    FormsAuthentication.SignOut();
                }
            }
            return member;
        }

        /// <summary>
        /// 按手机号码查找用户实例
        /// </summary>
        /// <param name="MobilePhone">手机号码</param>
        /// <param name="removeTracking"></param>
        /// <returns></returns>
        public MembershipUser GetUserByMobilePhone(string MobilePhone, bool removeTracking = false)
        {
            MobilePhone = StringUtils.SafePlainText(MobilePhone);
            if (removeTracking)
            {
                return _context.MembershipUser.AsNoTracking()
                    .Include(x => x.Roles)
                    .FirstOrDefault(m => m.MobilePhone == MobilePhone);
            }
            return _context.MembershipUser
                .Include(x => x.Roles)
                .FirstOrDefault(m => m.MobilePhone == MobilePhone);
        }

        /// <summary>
        /// Get a user by slug
        /// </summary>
        /// <param name="slug"></param>
        /// <returns></returns>
        public MembershipUser GetUserBySlug(string slug)
        {
            slug = StringUtils.GetSafeHtml(slug);
            return _context.MembershipUser
                .Include(x => x.Badges)
                .Include(x => x.Roles)
                .FirstOrDefault(name => name.Slug == slug);
        }

        /// <summary>
        /// 对当前MembershipUser集合按输入条件进行模糊搜索
        /// </summary>
        /// <param name="slug">搜索条件</param>
        /// <returns></returns>
        public IList<MembershipUser> GetUserBySlugLike(string slug)
        {
            return _context.MembershipUser
                    .Include(x => x.Roles)
                    .AsNoTracking()
                    .Where(name => name.Slug.ToUpper().Contains(slug.ToUpper()))
                    .ToList();
        }

        /// <summary>
        /// Get users from a list of Id's
        /// </summary>
        /// <param name="guids"></param>
        /// <returns></returns>
        public IList<MembershipUser> GetUsersById(List<Guid> guids)
        {
            return _context.MembershipUser
              .Where(x => guids.Contains(x.Id))
              .AsNoTracking()
              .ToList();
        }

        /// <summary>
        /// 取得所有用户的实例集合
        /// </summary>
        /// <returns></returns>
        public IList<MembershipUser> GetAll(bool isApproved = false)
        {
            if (isApproved)
            {
                return _context.MembershipUser.Where(x => x.IsApproved == true).ToList();
            }
            else
            {
                return _context.MembershipUser.ToList();
            }
        }

        /// <summary>
        /// 分组取得用户实例集合
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="isApproved"></param>
        /// <returns></returns>
        public PagedList<MembershipUser> GetAll(int pageIndex, int pageSize, bool isApproved = false)
        {
            var totalCount = MemberCount(isApproved);
            var results = GetAll(isApproved).OrderByDescending(x => x.CreateDate) //按账号的注册时间降序排列
                          .Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
            return new PagedList<MembershipUser>(results, pageIndex, pageSize, totalCount);
        }

        /// <summary>
        /// 取得最新注册的amountToTake个用户实例的集合
        /// </summary>
        /// <param name="amountToTake"></param>
        /// <param name="isApproved">仅筛选已经审核过的用户实例</param>
        /// <returns></returns>
        public IList<MembershipUser> GetLatestUsers(int amountToTake, bool isApproved = false, bool RemoveMarriedFilter = false)
        {
            if (isApproved)
            {
                if (RemoveMarriedFilter)
                {
                    return _context.MembershipUser.Include(x => x.Roles).AsNoTracking()
                                               .Where(x => x.IsApproved == true && x.IsMarried == Enum_MarriedStatus.Single)
                                               .OrderByDescending(x => x.CreateDate)
                                               .Take(amountToTake)
                                               .ToList();
                }
                else
                {
                    return _context.MembershipUser.Include(x => x.Roles).AsNoTracking()
                           .Where(x => x.IsApproved == true)
                           .OrderByDescending(x => x.CreateDate)
                           .Take(amountToTake)
                           .ToList();
                }
            }
            else
            {
                if (RemoveMarriedFilter)
                {
                    return _context.MembershipUser.Include(x => x.Roles).AsNoTracking()
                                             .Where(x => x.IsMarried == Enum_MarriedStatus.Single)
                                             .OrderByDescending(x => x.CreateDate)
                                             .Take(amountToTake)
                                             .ToList();
                }
                else
                {
                    return _context.MembershipUser.Include(x => x.Roles).AsNoTracking()
                                          .OrderByDescending(x => x.CreateDate)
                                          .Take(amountToTake)
                                          .ToList();
                }
            }
        }

        /// <summary>
        /// 12分钟内的活动用户
        /// </summary>
        /// <returns></returns>
        public IList<MembershipUser> GetActiveMembers()
        {
            // Get members that last activity date is valid

            var date = DateTime.Now.AddMinutes(-AppConstants.TimeSpanInMinutesToShowMembers);
            return _context.MembershipUser
                .Where(x => x.LastActivityDate > date)
                .AsNoTracking()
                .ToList();
        }

        /// <summary>
        /// 当前用户的个数
        /// </summary>
        /// <param name="isApproved">仅筛选已经审核过的用户实例</param>
        /// <returns></returns>
        public int MemberCount(bool isApproved = false)
        {
            if (isApproved)
            {
                return _context.MembershipUser.AsNoTracking().Where(x => x.IsApproved == true).Count();
            }
            else
            {
                return _context.MembershipUser.AsNoTracking().Count();
            }
        }


        #region ==================在爱驴网项目中禁用的方法==========================

        [Obsolete("此方法在爱驴网项目中禁用")]
        public MembershipUser GetUserByEmail(string email, bool removeTracking = false)
        {
            email = StringUtils.SafePlainText(email);
            if (removeTracking)
            {
                return _context.MembershipUser.AsNoTracking()
                    .Include(x => x.Roles)
                    .FirstOrDefault(name => name.Email == email);
            }
            return _context.MembershipUser
                .Include(x => x.Roles)
                .FirstOrDefault(name => name.Email == email);
        }

        [Obsolete("此方法在爱驴网项目中禁用")]
        public IList<MembershipUser> GetLowestPointUsers(int amountToTake)
        {
            return _context.MembershipUser
                 .Join(_context.MembershipUserPoints.AsNoTracking(), // The sequence to join to the first sequence.
                        user => user.Id, // A function to extract the join key from each element of the first sequence.
                        userPoints => userPoints.User.Id, // A function to extract the join key from each element of the second sequence
                        (user, userPoints) => new { MembershipUser = user, UserPoints = userPoints } // A function to create a result element from two matching elements.
                    )
                 .AsNoTracking()
                .OrderBy(x => x.UserPoints)
                .Take(amountToTake)
                .Select(t => t.MembershipUser)
                .ToList();
        }

        [Obsolete("此方法在爱驴网项目中禁用")]
        public IList<MembershipUser> GetUsersByDaysPostsPoints(int amoutOfDaysSinceRegistered, int amoutOfPosts)
        {
            var registerEnd = DateTime.Now;
            var registerStart = registerEnd.AddDays(-amoutOfDaysSinceRegistered);
            return _context.MembershipUser
                .Where(x =>
                        x.Posts.Count <= amoutOfPosts &&
                        x.CreateDate > registerStart &&
                        x.CreateDate <= registerEnd)
                .ToList();
        }

        #region 通过第三方登录Id取得用户实例（此功能已禁用）

        [Obsolete("此方法在爱驴网项目中禁用")]
        public MembershipUser GetUserByFacebookId(long facebookId)
        {
            return _context.MembershipUser
                .Include(x => x.Roles)
                .FirstOrDefault(name => name.FacebookId == facebookId);
        }
        [Obsolete("此方法在爱驴网项目中禁用")]
        public MembershipUser GetUserByTwitterId(string twitterId)
        {
            return _context.MembershipUser
                .Include(x => x.Roles)
                .FirstOrDefault(name => name.TwitterAccessToken == twitterId);
        }
        [Obsolete("此方法在爱驴网项目中禁用")]
        public MembershipUser GetUserByGoogleId(string googleId)
        {
            return _context.MembershipUser
                .Include(x => x.Roles)
                .FirstOrDefault(name => name.GoogleAccessToken == googleId);
        }
        [Obsolete("此方法在爱驴网项目中禁用")]
        public MembershipUser GetUserByOpenIdToken(string openId)
        {
            openId = StringUtils.GetSafeHtml(openId);
            return _context.MembershipUser
                .Include(x => x.Roles)
                .FirstOrDefault(name => name.MiscAccessToken == openId);
        }

        #endregion

        #endregion


        #endregion

        #region 密码管理

        /// <summary>
        /// Change the user's password
        /// </summary>
        /// <param name="user"> </param>
        /// <param name="oldPassword"></param>
        /// <param name="newPassword"></param>
        /// <returns></returns>
        public bool ChangePassword(MembershipUser user, string oldPassword, string newPassword)
        {
            oldPassword = StringUtils.SafePlainText(oldPassword);
            newPassword = StringUtils.SafePlainText(newPassword);

            //n3oCacheHelper.Clear(user.UserName);
            var existingUser = GetUser(user.Id);
            var salt = existingUser.PasswordSalt;
            var oldHash = StringUtils.GenerateSaltedHash(oldPassword, salt);

            if (oldHash != existingUser.Password)
            {
                // Old password is wrong - do not allow update
                return false;
            }

            // Cleared to go ahead with new password
            salt = StringUtils.CreateSalt(AppConstants.SaltSize);
            var newHash = StringUtils.GenerateSaltedHash(newPassword, salt);

            existingUser.Password = newHash;
            existingUser.PasswordSalt = salt;
            existingUser.LastPasswordChangedDate = DateTime.Now;

            return true;
        }

        /// <summary>
        /// Reset a users password
        /// </summary>
        /// <param name="user"></param>
        /// <param name="newPassword"> </param>
        /// <returns></returns>
        public bool ResetPassword(MembershipUser user, string newPassword)
        {
            var existingUser = GetUser(user.Id);

            var salt = StringUtils.CreateSalt(AppConstants.SaltSize);
            var newHash = StringUtils.GenerateSaltedHash(newPassword, salt);

            existingUser.Password = newHash;
            existingUser.PasswordSalt = salt;
            existingUser.LastPasswordChangedDate = DateTime.Now;

            return true;
        }
        /// <summary>
        /// Update the user record with a newly generated password reset security token and timestamp
        /// </summary>
        public bool UpdatePasswordResetToken(MembershipUser user)
        {
            var existingUser = GetUser(user.Id);
            if (existingUser == null)
            {
                return false;
            }
            existingUser.PasswordResetToken = CreatePasswordResetToken();
            existingUser.PasswordResetTokenCreatedAt = DateTime.Now;
            return true;
        }

        /// <summary>
        /// Remove the password reset security token and timestamp from the user record
        /// </summary>
        public bool ClearPasswordResetToken(MembershipUser user)
        {
            var existingUser = GetUser(user.Id);
            if (existingUser == null)
            {
                return false;
            }
            existingUser.PasswordResetToken = null;
            existingUser.PasswordResetTokenCreatedAt = null;
            return true;
        }

        /// <summary>
        /// To be valid:
        /// - The user record must contain a password reset token
        /// - The given token must match the token in the user record
        /// - The token timestamp must be less than 24 hours ago
        /// </summary>
        public bool IsPasswordResetTokenValid(MembershipUser user, string token)
        {
            var existingUser = GetUser(user.Id);
            if (string.IsNullOrEmpty(existingUser?.PasswordResetToken))
            {
                return false;
            }
            // A security token must have an expiry date
            if (existingUser.PasswordResetTokenCreatedAt == null)
            {
                return false;
            }
            // The security token is only valid for 48 hours
            if ((DateTime.Now - existingUser.PasswordResetTokenCreatedAt.Value).TotalHours >= MaxHoursToResetPassword)
            {
                return false;
            }
            return existingUser.PasswordResetToken == token;
        }

        /// <summary>
        /// Generate a password reset token, a guid is sufficient
        /// </summary>
        private static string CreatePasswordResetToken()
        {
            return Guid.NewGuid().ToString().ToLower().Replace("-", "");
        }
        #endregion

        #region 用户管理
        /// <summary>
        /// Delete a member
        /// </summary>
        /// <param name="user"></param>
        /// <param name="unitOfWork"></param>
        public bool Delete(MembershipUser user, IUnitOfWork unitOfWork)
        {
            try
            {
                // Scrub all member data
                ScrubUsers(user, unitOfWork);

                // Just clear the roles, don't delete them
                user.Roles.Clear();

                // Now delete the member
                _context.MembershipUser.Remove(user);

                return true;
            }
            catch (Exception ex)
            {
                _loggingService.Error(ex);
            }
            return false;
        }

        /// <summary>
        /// Unlock a user
        /// </summary>
        /// <param name="username"></param>
        /// <param name="resetPasswordAttempts">If true, also reset password attempts to zero</param>
        public void UnlockUser(string username, bool resetPasswordAttempts)
        {
            {
                var user = GetUser(username);
                if (user == null)
                {
                    throw new ApplicationException(_localizationService.GetResourceString("Members.CantUnlock"));
                }

                var existingUser = GetUser(user.Id);

                user.IsLockedOut = false;
                user.Roles = existingUser.Roles;
                user.Votes = existingUser.Votes;
                user.Password = existingUser.Password;
                user.PasswordSalt = existingUser.PasswordSalt;

                if (resetPasswordAttempts)
                {
                    user.FailedPasswordAnswerAttempt = 0;
                }
            }
        }

        /// <summary>
        /// 更新用户的角色信息
        /// </summary>
        /// <param name="id">用户Id</param>
        /// <param name="role">要赋予的角色实例</param>
        public void UpdateUserRole(Guid id, MembershipRole role)
        {
            var user = GetUser(id);
            if (user != null && role != null)
            {
                user.Roles = new List<MembershipRole> { role };
            }
            else
            {
                throw new Exception("找不到对应的会员信息。");
            }
        }

        /// <summary>
        /// 删除所有用户数据,有空时需要重构这个方法
        /// </summary>
        /// <param name="user"></param>
        /// <param name="unitOfWork"></param>
        public void ScrubUsers(MembershipUser user, IUnitOfWork unitOfWork)
        {
            #region 基本信息的擦除

            user.IsApproved = false;
            user.Avatar = string.Empty;

            user.Website = string.Empty;
            user.Twitter = string.Empty;
            user.Facebook = string.Empty;
            user.Signature = string.Empty;

            #endregion

            #region User Votes
            if (user.Votes != null)
            {
                var votesToDelete = new List<Vote>();
                votesToDelete.AddRange(user.Votes);
                foreach (var d in votesToDelete)
                {
                    _voteService.Delete(d);
                }
                user.Votes.Clear();
            }
            #endregion

            #region User Badges
            if (user.Badges != null)
            {
                var toDelete = new List<Badge>();
                toDelete.AddRange(user.Badges);
                foreach (var obj in toDelete)
                {
                    _badgeService.Delete(obj);
                }
                user.Badges.Clear();
            }

            // User badge time checks
            if (user.BadgeTypesTimeLastChecked != null)
            {
                var toDelete = new List<BadgeTypeTimeLastChecked>();
                toDelete.AddRange(user.BadgeTypesTimeLastChecked);
                foreach (var obj in toDelete)
                {
                    _badgeService.DeleteTimeLastChecked(obj);
                }
                user.BadgeTypesTimeLastChecked.Clear();
            }

            #endregion

            #region User category notifications
            if (user.CategoryNotifications != null)
            {
                var toDelete = new List<CategoryNotification>();
                toDelete.AddRange(user.CategoryNotifications);
                foreach (var obj in toDelete)
                {
                    _categoryNotificationService.Delete(obj);
                }
                user.CategoryNotifications.Clear();
            }
            #endregion

            #region User PrivateMessage Received

            var pmUpdate = false;
            if (user.PrivateMessagesReceived != null)
            {
                pmUpdate = true;
                var toDelete = new List<PrivateMessage>();
                toDelete.AddRange(user.PrivateMessagesReceived);
                foreach (var obj in toDelete)
                {
                    _privateMessageService.DeleteMessage(obj);
                }
                user.PrivateMessagesReceived.Clear();
            }
            #endregion

            #region User PrivateMessage Sent

            if (user.PrivateMessagesSent != null)
            {
                pmUpdate = true;
                var toDelete = new List<PrivateMessage>();
                toDelete.AddRange(user.PrivateMessagesSent);
                foreach (var obj in toDelete)
                {
                    _privateMessageService.DeleteMessage(obj);
                }
                user.PrivateMessagesSent.Clear();
            }

            if (pmUpdate)
            {
                unitOfWork.SaveChanges();
            }

            #endregion

            #region User Favourites

            if (user.Favourites != null)
            {
                var toDelete = new List<Favourite>();
                toDelete.AddRange(user.Favourites);
                foreach (var obj in toDelete)
                {
                    _favouriteService.Delete(obj);
                }
                user.Favourites.Clear();
            }

            #endregion

            #region User TopicNotifications

            if (user.TopicNotifications != null)
            {
                var notificationsToDelete = new List<TopicNotification>();
                notificationsToDelete.AddRange(user.TopicNotifications);
                foreach (var topicNotification in notificationsToDelete)
                {
                    _topicNotificationService.Delete(topicNotification);
                }
                user.TopicNotifications.Clear();
            }

            #endregion

            // Also clear their points
            var userPoints = user.Points;
            if (userPoints.Any())
            {
                var pointsList = new List<MembershipUserPoints>();
                pointsList.AddRange(userPoints);
                foreach (var point in pointsList)
                {
                    point.User = null;
                    _membershipUserPointsService.Delete(point);
                }
                user.Points.Clear();
            }

            // Now clear all activities for this user
            var usersActivities = _activityService.GetDataFieldByGuid(user.Id);
            _activityService.Delete(usersActivities.ToList());

            // Also clear their poll votes
            var userPollVotes = user.PollVotes;
            if (userPollVotes.Any())
            {
                var pollList = new List<PollVote>();
                pollList.AddRange(userPollVotes);
                foreach (var vote in pollList)
                {
                    vote.User = null;
                    _pollVoteService.Delete(vote);
                }
                user.PollVotes.Clear();
            }

            unitOfWork.SaveChanges();


            // Also clear their polls
            var userPolls = user.Polls;
            if (userPolls.Any())
            {
                var polls = new List<Poll>();
                polls.AddRange(userPolls);
                foreach (var poll in polls)
                {
                    //Delete the poll answers
                    var pollAnswers = poll.PollAnswers;
                    if (pollAnswers.Any())
                    {
                        var pollAnswersList = new List<PollAnswer>();
                        pollAnswersList.AddRange(pollAnswers);
                        foreach (var answer in pollAnswersList)
                        {
                            answer.Poll = null;
                            _pollAnswerService.Delete(answer);
                        }
                    }

                    poll.PollAnswers.Clear();
                    poll.User = null;
                    _pollService.Delete(poll);
                }
                user.Polls.Clear();
            }

            unitOfWork.SaveChanges();

            // ######### POSTS TOPICS ########

            // Delete all topics first
            var topics = user.Topics;
            if (topics != null && topics.Any())
            {
                var topicList = new List<Topic>();
                topicList.AddRange(topics);
                foreach (var topic in topicList)
                {
                    topic.LastPost = null;
                    topic.Posts.Clear();
                    topic.Tags.Clear();
                    _topicService.Delete(topic, unitOfWork);
                }
                user.Topics.Clear();
                unitOfWork.SaveChanges();
            }

            // Now sorts Last Posts on topics and delete all the users posts
            var posts = user.Posts;
            if (posts != null && posts.Any())
            {
                var postIds = posts.Select(x => x.Id).ToList();

                // Get all categories
                var allCategories = _categoryService.GetAllUserLevelCategory();

                // Need to see if any of these are last posts on Topics
                // If so, need to swap out last post
                var lastPostTopics = _topicService.GetTopicsByLastPost(postIds, allCategories.ToList());
                foreach (var topic in lastPostTopics.Where(x => x.User.Id != user.Id))
                {
                    var lastPost = topic.Posts.Where(x => !postIds.Contains(x.Id)).OrderByDescending(x => x.DateCreated).FirstOrDefault();
                    topic.LastPost = lastPost;
                }

                unitOfWork.SaveChanges();

                user.UploadedFiles.Clear();

                // Delete all posts

                var postList = new List<Post>();
                postList.AddRange(posts);
                foreach (var post in postList)
                {
                    if (post.Files != null)
                    {
                        var files = post.Files;
                        var filesList = new List<UploadedFile>();
                        filesList.AddRange(files);
                        foreach (var file in filesList)
                        {
                            // store the file path as we'll need it to delete on the file system
                            var filePath = file.FilePath;

                            // Now delete it
                            _uploadedFileService.Delete(file);

                            // And finally delete from the file system
                            System.IO.File.Delete(HostingEnvironment.MapPath(filePath));
                        }
                        post.Files.Clear();
                    }
                    var postEdits = new List<PostEdit>();
                    postEdits.AddRange(post.PostEdits);
                    _postEditService.Delete(postEdits);
                    post.PostEdits.Clear();
                    _postService.Delete(post, unitOfWork, true);
                }
                user.Posts.Clear();

                unitOfWork.SaveChanges();
            }
        }

        /// <summary>
        /// Save user (does NOT update password data)
        /// </summary>
        /// <param name="user"></param>
        public void ProfileUpdated(MembershipUser user)
        {
            var e = new UpdateProfileEventArgs { User = user };
            EventManager.Instance.FireBeforeProfileUpdated(this, e);

            if (!e.Cancel)
            {
                EventManager.Instance.FireAfterProfileUpdated(this, new UpdateProfileEventArgs { User = user });
                _activityService.ProfileUpdated(user);
            }
        }

        #endregion

        #region 用户搜索
        private List<MembershipUser> SearchbyCondition(MembershipUserSearchModel searchusermodel, bool AdministratorMode = false)
        {
            var total = _context.MembershipUser.ToList();

            if (searchusermodel != null)
            {
                #region 过滤搜索条件
                //账号
                if (!String.IsNullOrEmpty(searchusermodel.UserName))
                {
                    total = total.Where(p => p.UserName.ToLower() == searchusermodel.UserName.ToLower()).ToList();
                }
                //真实姓名
                if (!String.IsNullOrEmpty(searchusermodel.RealName))
                {
                    total = total.Where(p => p.RealName.ToLower() == searchusermodel.RealName.ToLower()).ToList();
                }
                //昵称
                if (!String.IsNullOrEmpty(searchusermodel.AliasName))
                {
                    total = total.Where(p => p.AliasName.ToLower() == searchusermodel.AliasName.ToLower()).ToList();
                }
                //婚否
                if (!string.IsNullOrEmpty(searchusermodel.IsMarried))
                {
                    if (searchusermodel.IsMarried == "1") //isMarried
                    {
                        total = total.Where(p => p.IsMarried == Enum_MarriedStatus.Married).ToList();
                    }
                    else
                    {
                        total = total.Where(p => p.IsMarried == Enum_MarriedStatus.Single).ToList();
                    }
                }
                //性别
                if (!string.IsNullOrEmpty(searchusermodel.Gender))
                {
                    if (searchusermodel.Gender == "1") // the people is man
                    {
                        total = total.Where(p => p.Gender == Enum_Gender.boy).ToList();
                    }
                    else //the people is female.
                    {
                        total = total.Where(p => p.Gender == Enum_Gender.girl).ToList();
                    }
                }

                #region 年龄段
                if (!string.IsNullOrEmpty(searchusermodel.AgeRange))
                {
                    switch (searchusermodel.AgeRange)
                    {
                        case "1":
                            total = total.Where(p => p.Age < 20).ToList();
                            break;
                        case "2":
                            total = total.Where(p => p.Age >= 20 && p.Age < 25).ToList();
                            break;
                        case "3":
                            total = total.Where(p => p.Age >= 25 && p.Age < 30).ToList();
                            break;
                        case "4":
                            total = total.Where(p => p.Age >= 30 && p.Age < 35).ToList();
                            break;
                        case "5":
                            total = total.Where(p => p.Age >= 35 && p.Age < 40).ToList();
                            break;
                        case "6":
                            total = total.Where(p => p.Age >= 40 && p.Age < 50).ToList();
                            break;
                        case "7":
                            total = total.Where(p => p.Age >= 50).ToList();
                            break;
                    }

                }

                #endregion

                #region 学历
                if (!String.IsNullOrEmpty(searchusermodel.Education))
                {
                    switch (searchusermodel.Education)
                    {
                        case "1":   //高中及以上
                            total = total.Where(p => p.Education == "2" || p.Education == "3" || p.Education == "4" || p.Education == "5" || p.Education == "6").ToList();
                            break;
                        case "2":   //大专及以上
                            total = total.Where(p => p.Education == "3" || p.Education == "4" || p.Education == "5" || p.Education == "6").ToList();
                            break;
                        case "3":   //本科及以上
                            total = total.Where(p => p.Education == "4" || p.Education == "5" || p.Education == "6").ToList();
                            break;
                        case "4":   //硕士及以上
                            total = total.Where(p => p.Education == "5" || p.Education == "6").ToList();
                            break;
                        case "5":   //博士
                            total = total.Where(p => p.Education == "6").ToList();
                            break;
                        default:
                            break;
                    }
                }
                #endregion

                #region 居住地


                if (!String.IsNullOrEmpty(searchusermodel.LocationProvince) &&
                   !String.IsNullOrEmpty(searchusermodel.LocationCity) &&
                   !String.IsNullOrEmpty(searchusermodel.LocationCounty) &&
                   searchusermodel.LocationProvince != "0" &&
                   searchusermodel.LocationCity == "0" &&
                   searchusermodel.LocationCounty == "0"
                   )
                {
                    total = total.Where(p => p.LocationProvince == searchusermodel.LocationProvince
                    ).ToList();
                }

                if (!String.IsNullOrEmpty(searchusermodel.LocationProvince) &&
                 !String.IsNullOrEmpty(searchusermodel.LocationCity) &&
                 !String.IsNullOrEmpty(searchusermodel.LocationCounty) &&
                 searchusermodel.LocationProvince != "0" &&
                 searchusermodel.LocationCity != "0" &&
                 searchusermodel.LocationCounty == "0"
                 )
                {
                    total = total.Where(p => p.LocationProvince == searchusermodel.LocationProvince &&
                                             p.LocationCity == searchusermodel.LocationCity
                    ).ToList();
                }

                if (!String.IsNullOrEmpty(searchusermodel.LocationProvince) &&
                    !String.IsNullOrEmpty(searchusermodel.LocationCity) &&
                    !String.IsNullOrEmpty(searchusermodel.LocationCounty) &&
                    searchusermodel.LocationProvince != "0" &&
                    searchusermodel.LocationCity != "0" &&
                    searchusermodel.LocationCounty != "0"
                    )
                {
                    total = total.Where(p => p.LocationProvince == searchusermodel.LocationProvince &&
                                             p.LocationCity == searchusermodel.LocationCity &&
                                             p.LocationCounty == searchusermodel.LocationCounty
                    ).ToList();
                }

                #endregion

                #region 月收入
                if (!String.IsNullOrEmpty(searchusermodel.IncomeRange))
                {
                    switch (searchusermodel.IncomeRange)
                    {
                        case "1":
                            total = total.Where(p => p.IncomeRange == Enum_IncomeRange.R_Lowthan1W).ToList();
                            break;
                        case "2":
                            total = total.Where(p => p.IncomeRange == Enum_IncomeRange.R_1WTo5W || p.IncomeRange == Enum_IncomeRange.R_5WMore).ToList();
                            break;
                        case "3":
                            total = total.Where(p => p.IncomeRange == Enum_IncomeRange.R_5WMore).ToList();
                            break;
                        default:
                            break;
                    }
                }
                #endregion

                //职业
                if (!String.IsNullOrEmpty(searchusermodel.Job))
                {
                    total = total.Where(p => p.Job.Contains(searchusermodel.Job)).ToList();
                }

                //毕业院校
                if (!String.IsNullOrEmpty(searchusermodel.SchoolName))
                {
                    total = total.Where(p => p.SchoolName.Contains(searchusermodel.SchoolName)).ToList();
                }

                //最近未登录天数
                switch (searchusermodel.NoLoginDays)
                {
                    case "1":
                        var d1 = DateTime.Now - TimeSpan.FromDays(7);
                        total = total.Where(p => p.LastLoginDate <= d1).ToList();
                        break;
                    case "2":
                        var d2 = DateTime.Now - TimeSpan.FromDays(30);
                        total = total.Where(p => p.LastLoginDate <= d2).ToList();
                        break;
                    case "3":
                        var d3 = DateTime.Now - TimeSpan.FromDays(60);
                        total = total.Where(p => p.LastLoginDate <= d3).ToList();
                        break;
                    default:
                        break;
                }

                //会员类型
                if (!string.IsNullOrEmpty(searchusermodel.UserType))
                {
                    switch (searchusermodel.UserType)
                    {
                        case "1":
                            total = total.Where(p => p.UserType == Enum_UserType.A).ToList();
                            break;
                        case "2":
                            total = total.Where(p => p.UserType == Enum_UserType.B).ToList();
                            break;
                        case "3":
                            total = total.Where(p => p.UserType == Enum_UserType.C).ToList();
                            break;
                        case "4":
                            total = total.Where(p => p.UserType == Enum_UserType.D).ToList();
                            break;
                        case "5":
                            total = total.Where(p => p.UserType == Enum_UserType.E).ToList();
                            break;
                        default:
                            break;
                    }
                }

                if (!string.IsNullOrEmpty(searchusermodel.UserStatus))
                {
                    switch (searchusermodel.UserStatus)
                    {
                        case "1":
                            total = total.Where(p => p.IsApproved == true).ToList();
                            break;
                        case "2":
                            total = total.Where(p => p.IsApproved == false).ToList();
                            break;
                        case "3":
                            total = total.Where(p => p.IsLockedOut == true).ToList();
                            break;
                        case "4":
                            total = total.Where(p => p.IsBanned == true).ToList();
                            break;
                        default:
                            break;
                    }
                }

                if (!AdministratorMode)
                {
                    total = total.Where(p => p.IsApproved == true).ToList();
                }

                #endregion
            }
            return total;
        }

        public IList<MembershipUser> SearchMembers(MembershipUserSearchModel searchusermodel, int amount)
        {
            return SearchbyCondition(searchusermodel).OrderByDescending(x => x.LastLoginDate)
              .Take(amount)
              .ToList();
        }

        public IList<MembershipUser> SearchMembers(MembershipUserSearchModel searchusermodel, int amount, bool AdministratorMode)
        {
            return SearchbyCondition(searchusermodel, AdministratorMode).OrderByDescending(x => x.LastLoginDate)
              .Take(amount)
              .ToList();
        }

        public PagedList<MembershipUser> SearchMembers(MembershipUserSearchModel searchusermodel, int pageIndex, int pageSize)
        {
            var total = SearchbyCondition(searchusermodel);
            var results = total
           .OrderByDescending(x => x.LastLoginDate)
           .Skip((pageIndex - 1) * pageSize)
           .Take(pageSize)
           .ToList();

            return new PagedList<MembershipUser>(results, pageIndex, pageSize, total.Count());
        }


        [Obsolete("此方法无法通用，应该取消。")]
        public PagedList<MembershipUser> SearchMembers(string search, int pageIndex, int pageSize)
        {
            search = StringUtils.SafePlainText(search);
            var query = _context.MembershipUser
                                .Where(x => x.UserName.ToUpper().Contains(search.ToUpper()) || x.AliasName.ToUpper().Contains(search.ToUpper()));

            var results = query
                .OrderBy(x => x.UserName)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return new PagedList<MembershipUser>(results, pageIndex, pageSize, query.Count());
        }

        [Obsolete("此方法无法通用，应该取消。")]
        public IList<MembershipUser> SearchMembers(string username, int amount)
        {
            username = StringUtils.SafePlainText(username);
            return _context.MembershipUser
                .Where(x => x.UserName.ToUpper().Contains(username.ToUpper()))
                .OrderBy(x => x.UserName)
                .Take(amount)
                .ToList();
        }

        #endregion

        #region Membership账号的导入和导出


        public string RemoveHTMLToCSV(string source)
        {
            return source.Replace("<table border='1' bordercolor='#a0c6e5' style='border-collapse:collapse;'>", "").Replace("</table>", "").Replace("<tr>", "").Replace("</tr>", "").Replace("<td>", "").Replace("</td>", ",");
        }

        public string ToCsv(List<MembershipUser> userlist, bool isAdmin)
        {

            var csv = new StringBuilder("");
            if (userlist != null && userlist.Count > 0)
            {
                csv.Append("<table border='1' bordercolor='#a0c6e5' style='border-collapse:collapse;'>");
                if (isAdmin)
                {
                    csv.Append("<tr>");
                    csv.AppendLine("<td>账号</td><td>昵称</td><td>真实姓名</td><td>联系方式</td><td>性别</td><td>年龄</td><td>婚否</td><td>身高</td><td>体重</td><td>现居地</td><td>最后登录时间</td><td>审核标志位</td><td>会员状态</td><td>会员类别</td>");
                    csv.Append("</tr>");
                    foreach (var user in userlist)
                    {
                        if (user == null) continue;

                        var UserName = string.Empty;
                        var AliasName = string.Empty;
                        var RealName = string.Empty;
                        var MobilePhone = string.Empty;
                        var Gender = string.Empty;
                        var Age = string.Empty;
                        var IsMarried = string.Empty;
                        var Height = string.Empty;
                        var Weight = string.Empty;
                        var Location = string.Empty;
                        var IsApproved = string.Empty;
                        var IsBanned = string.Empty;
                        var UserType = string.Empty;
                        var LastLoginDate = "";

                        try
                        {
                            UserName = user.UserName;
                            AliasName = user.AliasName;
                            RealName = user.RealName;
                            MobilePhone = user.MobilePhone;
                            Gender = user.Gender == Enum_Gender.boy ? "男" : "女";
                            Age = user.Age.ToString();
                            IsMarried = user.IsMarried == Enum_MarriedStatus.Married ? "已婚" : "单身";
                            Height = user.Height.ToString();
                            Weight = user.Weight.ToString();
                            Location = string.Concat(TProvince.LoadProvinceByProvincedId(Convert.ToInt32(user.LocationProvince)).ProvinceName,
                                          TCity.LoadCityByCityId(Convert.ToInt32(user.LocationCity)).CityName,
                                          TCountry.LoadCountryByCountryId(Convert.ToInt32(user.LocationCounty)).CountryName);
                            LastLoginDate = user.LastLoginDate.ToString("yyyy-MM-dd HH:mm:ss");
                            IsApproved = user.IsApproved ? "已审核" : "待审核";
                            IsBanned = user.IsBanned ? "用户已隐藏" : "正常状态";
                            UserType = user.UserType.ToString();
                            csv.Append("<tr>");
                            csv.AppendFormat("<td>{0}</td><td>{1}</td><td>{2}</td><td>{3}</td><td>{4}</td><td>{5}</td><td>{6}</td><td>{7}</td><td>{8}</td><td>{9}</td><td>{10}</td><td>{11}</td><td>{12}</td><td>{13}</td>",
                                UserName,
                                AliasName,
                                RealName,
                                MobilePhone,
                                Gender,
                                Age,
                                IsMarried,
                                Height,
                                Weight,
                                Location,
                                LastLoginDate,
                                IsApproved,
                                IsBanned,
                                UserType
                                );
                            csv.Append("</tr>");
                            csv.AppendLine();
                        }
                        catch (Exception ex)
                        {
                            _loggingService.Error(ex.Message);
                        }
                    }
                }
                else
                {
                    csv.Append("<tr>");
                    csv.AppendLine("<td>账号</td><td>昵称</td><td>性别</td><td>年龄</td><td>婚否</td><td>身高</td><td>体重</td><td>现居地</td><td>最后登录时间</td><td>审核标志位</td><td>会员状态</td><td>会员类别</td>");
                    csv.Append("</tr>");
                    foreach (var user in userlist)
                    {
                        if (user == null) continue;
                        csv.Append("<tr>");
                        csv.AppendFormat("<td>{0}</td><td>{1}</td><td>{2}</td><td>{3}</td><td>{4}</td><td>{5}</td><td>{6}</td><td>{7}</td><td>{8}</td><td>{9}</td><td>{10}</td><td>{11}</td>",
                            user.UserName,
                            user.AliasName,
                            user.RealName,
                            user.MobilePhone,
                            user.Gender == Enum_Gender.boy ? "男" : "女",
                            user.Age,
                            user.IsMarried == Enum_MarriedStatus.Married ? "已婚" : "单身",
                            user.Height,
                            user.Weight,
                            string.Concat(TProvince.LoadProvinceByProvincedId(Convert.ToInt32(user.LocationProvince)).ProvinceName,
                                          TCity.LoadCityByCityId(Convert.ToInt32(user.LocationCity)).CityName,
                                          TCountry.LoadCountryByCountryId(Convert.ToInt32(user.LocationCounty)).CountryName),
                            user.LastLoginDate.ToString("yyyy-MM-dd HH:mm:ss"),
                            user.IsApproved ? "已审核" : "待审核",
                            user.IsBanned ? "用户已隐藏" : "正常状态",
                            user.UserType.ToString()
                            );
                        csv.Append("</tr>");
                        csv.AppendLine();
                    }
                }
                csv.Append("</table>");
            }
            return csv.ToString();
        }

        /// <summary>
        /// 导出所有会员信息到CSV档输出string中
        /// </summary>
        /// <returns></returns>
        public string ToCsv()
        {
            var userlist = GetAll().ToList();
            return ToCsv(userlist, false);
        }

        /// <summary>
        /// 从特定的CSV文件中导入会员数据
        /// </summary>
        /// <returns></returns>
        [Obsolete("因爱驴网项目无此需求，故此方法代码还没有实现。")]
        public CsvReport FromCsv(List<string> allLines)
        {
            var usersProcessed = new List<string>();
            var commaSeparator = new[] { ',' };
            var report = new CsvReport();

            //if (allLines == null || allLines.Count == 0)
            //{
            //    report.Errors.Add(new CsvErrorWarning
            //    {
            //        ErrorWarningType = CsvErrorWarningType.BadDataFormat,
            //        Message = "No users found."
            //    });
            //    return report;
            //}
            //var settings = _settingsService.GetSettings(true);
            //var lineCounter = 0;
            //foreach (var line in allLines)
            //{
            //    try
            //    {
            //        lineCounter++;

            //        // Each line is made up of n items in a predefined order

            //        var values = line.Split(commaSeparator);

            //        if (values.Length < 2)
            //        {
            //            report.Errors.Add(new CsvErrorWarning
            //            {
            //                ErrorWarningType = CsvErrorWarningType.MissingKeyOrValue,
            //                Message = $"Line {lineCounter}: insufficient values supplied."
            //            });

            //            continue;
            //        }

            //        var userName = values[0];

            //        if (userName.IsNullEmpty())
            //        {
            //            report.Errors.Add(new CsvErrorWarning
            //            {
            //                ErrorWarningType = CsvErrorWarningType.MissingKeyOrValue,
            //                Message = $"Line {lineCounter}: no username supplied."
            //            });

            //            continue;
            //        }

            //        var email = values[1];
            //        if (email.IsNullEmpty())
            //        {
            //            report.Errors.Add(new CsvErrorWarning
            //            {
            //                ErrorWarningType = CsvErrorWarningType.MissingKeyOrValue,
            //                Message = $"Line {lineCounter}: no email supplied."
            //            });

            //            continue;
            //        }

            //        // get the user
            //        var userToImport = GetUser(userName);

            //        if (userToImport != null)
            //        {
            //            report.Errors.Add(new CsvErrorWarning
            //            {
            //                ErrorWarningType = CsvErrorWarningType.AlreadyExists,
            //                Message = $"Line {lineCounter}: user already exists in forum."
            //            });

            //            continue;
            //        }

            //        if (usersProcessed.Contains(userName))
            //        {
            //            report.Errors.Add(new CsvErrorWarning
            //            {
            //                ErrorWarningType = CsvErrorWarningType.AlreadyExists,
            //                Message = $"Line {lineCounter}: user already exists in import file."
            //            });

            //            continue;
            //        }

            //        usersProcessed.Add(userName);

            //        userToImport = CreateEmptyUser();
            //        userToImport.UserName = userName;
            //        userToImport.Slug = ServiceHelpers.GenerateSlug(userToImport.UserName, GetUserBySlugLike(ServiceHelpers.CreateUrl(userToImport.UserName)), userToImport.Slug);
            //        userToImport.Email = email;
            //        userToImport.IsApproved = true;
            //        userToImport.PasswordSalt = StringUtils.CreateSalt(AppConstants.SaltSize);

            //        string createDateStr = null;
            //        if (values.Length >= 3)
            //        {
            //            createDateStr = values[2];
            //        }
            //        userToImport.CreateDate = createDateStr.IsNullEmpty() ? DateTime.Now : DateTime.Parse(createDateStr);

            //        if (values.Length >= 4)
            //        {
            //            // userToImport.Age = Int32.Parse(values[3]);
            //        }
            //        if (values.Length >= 5)
            //        {
            //            userToImport.HomeTown = values[4];
            //        }
            //        if (values.Length >= 6)
            //        {
            //            userToImport.Website = values[5];
            //        }
            //        if (values.Length >= 7)
            //        {
            //            userToImport.Facebook = values[6];
            //        }
            //        if (values.Length >= 8)
            //        {
            //            userToImport.Signature = values[7];
            //        }
            //        userToImport.Roles = new List<MembershipRole> { settings.NewMemberStartingRole };
            //        Add(userToImport);
            //    }
            //    catch (Exception ex)
            //    {
            //        report.Errors.Add(new CsvErrorWarning { ErrorWarningType = CsvErrorWarningType.GeneralError, Message = ex.Message });
            //    }
            //}

            return report;
        }

        #endregion

        #region 其他辅助功能

        /// <summary>
        /// 返回最后的登录状态
        /// </summary>
        public LoginAttemptStatus LastLoginStatus { get; private set; } = LoginAttemptStatus.LoginSuccessful;

        /// <summary>
        /// 通过账号密码校验用户实例，并返回校验状态
        /// </summary>
        /// <param name="userName">账号</param>
        /// <param name="password">密码</param>
        /// <param name="maxInvalidPasswordAttempts">最大无效密码尝试的次数</param>
        /// <returns></returns>
        public bool ValidateUser(string userName, string password, int maxInvalidPasswordAttempts)
        {
            userName = StringUtils.SafePlainText(userName);
            password = StringUtils.SafePlainText(password);

            LastLoginStatus = LoginAttemptStatus.LoginSuccessful;

            var user = GetUser(userName);

            if (user == null)
            {
                LastLoginStatus = LoginAttemptStatus.UserNotFound;
                return false;
            }

            if (user.IsBanned)
            {
                LastLoginStatus = LoginAttemptStatus.Banned;
                return false;
            }

            if (user.IsLockedOut)
            {
                LastLoginStatus = LoginAttemptStatus.UserLockedOut;
                return false;
            }

            //if (!user.IsApproved)
            //{
            //    LastLoginStatus = LoginAttemptStatus.UserNotApproved;
            //    return false;
            //}

            var allowedPasswordAttempts = maxInvalidPasswordAttempts;
            if (user.FailedPasswordAttemptCount >= allowedPasswordAttempts)
            {
                LastLoginStatus = LoginAttemptStatus.PasswordAttemptsExceeded;
                return false;
            }

            var salt = user.PasswordSalt;
            var hash = StringUtils.GenerateSaltedHash(password, salt);
            var passwordMatches = hash == user.Password;

            user.FailedPasswordAttemptCount = passwordMatches ? 0 : user.FailedPasswordAttemptCount + 1;

            if (user.FailedPasswordAttemptCount >= allowedPasswordAttempts)
            {
                user.IsLockedOut = true;
                user.LastLockoutDate = DateTime.Now;
            }

            if (!passwordMatches)
            {
                LastLoginStatus = LoginAttemptStatus.PasswordIncorrect;
                return false;
            }

            return LastLoginStatus == LoginAttemptStatus.LoginSuccessful;
        }

        /// <summary>
        /// 取得特定用户所拥有的角色清单
        /// </summary>
        /// <param name="username">用户账号</param>
        /// <returns></returns>
        public string[] GetRolesForUser(string username)
        {
            username = StringUtils.SafePlainText(username);
            var roles = new List<string>();
            var user = GetUser(username, true);

            if (user != null)
            {
                roles.AddRange(user.Roles.Select(role => role.RoleName));
            }

            return roles.ToArray();
        }

        public string ErrorCodeToString(MembershipCreateStatus createStatus)
        {
            // See http://go.microsoft.com/fwlink/?LinkID=177550 for a full list of status codes.
            // Benjamin: 具体的状态描述请参见：  https://msdn.microsoft.com/zh-cn/library/system.web.security.membershipcreatestatus.aspx
            switch (createStatus)
            {
                case MembershipCreateStatus.DuplicateUserName:  //用户名已存在于应用程序的数据库中。
                    return _localizationService.GetResourceString("Members.Errors.DuplicateUserName");

                case MembershipCreateStatus.DuplicateEmail:  //电子邮件地址已存在于应用程序的数据库中。
                    return _localizationService.GetResourceString("Members.Errors.DuplicateEmail");

                case MembershipCreateStatus.DuplicateTelphone:
                    return "您填写的手机号码已经注册。";

                case MembershipCreateStatus.InvalidPassword:   //密码的格式设置不正确。
                    return _localizationService.GetResourceString("Members.Errors.InvalidPassword");

                case MembershipCreateStatus.InvalidEmail:
                    return _localizationService.GetResourceString("Members.Errors.InvalidEmail");

                case MembershipCreateStatus.InvalidAnswer:
                    return _localizationService.GetResourceString("Members.Errors.InvalidAnswer");

                case MembershipCreateStatus.InvalidQuestion:
                    return _localizationService.GetResourceString("Members.Errors.InvalidQuestion");

                case MembershipCreateStatus.InvalidUserName:
                    return _localizationService.GetResourceString("Members.Errors.InvalidUserName");

                case MembershipCreateStatus.ProviderError:
                    return _localizationService.GetResourceString("Members.Errors.ProviderError");

                case MembershipCreateStatus.UserRejected:
                    return _localizationService.GetResourceString("Members.Errors.UserRejected");

                default:
                    return _localizationService.GetResourceString("Members.Errors.Unknown");
            }
        }

        #endregion

        public void UpdateUserType(Guid id, int UserType)
        {
            var existingUser = GetUser(id);
            existingUser.UserType = (Enum_UserType)UserType;
            _context.Entry<MembershipUser>(existingUser).State = EntityState.Modified;
            _context.SaveChanges();
        }


    }
}
