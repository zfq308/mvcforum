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

namespace MVCForum.Services
{
    public partial class MembershipService : IMembershipService
    {
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

        #region 用户实例操作


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
                Email = string.Empty,
                RealName = string.Empty,
                PasswordQuestion = string.Empty,
                PasswordAnswer = string.Empty,
                CreateDate = now,
                FailedPasswordAnswerAttempt = 0,
                FailedPasswordAttemptCount = 0,
                LastLockoutDate = (DateTime)SqlDateTime.MinValue,
                LastPasswordChangedDate = now,
                IsApproved = false,
                IsLockedOut = false,
                LastLoginDate = (DateTime)SqlDateTime.MinValue,
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
            //TODO:Benjamin, 此方法需要对关键属性进行操作。下列代码应为关键属性的操作。
            membershipUser.Comment = StringUtils.SafePlainText(membershipUser.Comment);
            membershipUser.Password = StringUtils.SafePlainText(membershipUser.Password);
            membershipUser.PasswordAnswer = StringUtils.SafePlainText(membershipUser.PasswordAnswer);
            membershipUser.PasswordQuestion = StringUtils.SafePlainText(membershipUser.PasswordQuestion);
            membershipUser.Signature = StringUtils.GetSafeHtml(membershipUser.Signature, true);
            membershipUser.UserName = StringUtils.SafePlainText(membershipUser.UserName);

            //membershipUser.Email = StringUtils.SafePlainText(membershipUser.Email);
            //membershipUser.Twitter = StringUtils.SafePlainText(membershipUser.Twitter);
            //membershipUser.Website = StringUtils.SafePlainText(membershipUser.Website);
            //membershipUser.Avatar = StringUtils.SafePlainText(membershipUser.Avatar);
            return membershipUser;
        }

        /// <summary>
        /// 创建用户实例
        /// </summary>
        /// <param name="newUser"></param>
        /// <returns></returns>
        public MembershipCreateStatus CreateUser(MembershipUser newUser)
        {
            newUser = SanitizeUser(newUser);
            var settings = _settingsService.GetSettings(false);


            var status = MembershipCreateStatus.Success;

            var e = new RegisterUserEventArgs { User = newUser };
            EventManager.Instance.FireBeforeRegisterUser(this, e);

            if (e.Cancel)
            {
                status = e.CreateStatus;
            }
            else
            {
                if (string.IsNullOrEmpty(newUser.UserName))
                {
                    status = MembershipCreateStatus.InvalidUserName;
                }

                // get by username
                if (GetUser(newUser.UserName, true) != null)
                {
                    status = MembershipCreateStatus.DuplicateUserName;
                }

                //// Add get by email address
                //if (GetUserByEmail(newUser.Email, true) != null)
                //{
                //    status = MembershipCreateStatus.DuplicateEmail;
                //}

                if (string.IsNullOrEmpty(newUser.Password))
                {
                    status = MembershipCreateStatus.InvalidPassword;
                }

                if (status == MembershipCreateStatus.Success)
                {
                    // Hash the password
                    var salt = StringUtils.CreateSalt(AppConstants.SaltSize);
                    var hash = StringUtils.GenerateSaltedHash(newUser.Password, salt);
                    newUser.Password = hash;
                    newUser.PasswordSalt = salt;

                    newUser.Roles = new List<MembershipRole> { settings.NewMemberStartingRole };

                    // Set dates
                    newUser.CreateDate = newUser.LastPasswordChangedDate = DateTime.Now;
                    newUser.LastLockoutDate = (DateTime)SqlDateTime.MinValue;
                    newUser.LastLoginDate = DateTime.Now;
                    newUser.IsLockedOut = false;

                    var manuallyAuthoriseMembers = settings.ManuallyAuthoriseNewMembers;
                    var memberEmailAuthorisationNeeded = settings.NewMemberEmailConfirmation == true;
                    if (manuallyAuthoriseMembers || memberEmailAuthorisationNeeded)
                    {
                        newUser.IsApproved = false;
                    }
                    else
                    {
                        newUser.IsApproved = true;
                    }

                    // url generator
                    newUser.Slug = ServiceHelpers.GenerateSlug(newUser.UserName, GetUserBySlugLike(ServiceHelpers.CreateUrl(newUser.UserName)), null);

                    try
                    {
                        Add(newUser);

                        if (settings.EmailAdminOnNewMemberSignUp)
                        {
                            var sb = new StringBuilder();
                            sb.AppendFormat("<p>{0}</p>", string.Format(_localizationService.GetResourceString("Members.NewMemberRegistered"), settings.ForumName, settings.ForumUrl));
                            sb.AppendFormat("<p>{0} - {1}</p>", newUser.UserName, newUser.Email);
                            var email = new Email
                            {
                                EmailTo = settings.AdminEmailAddress,
                                NameTo = _localizationService.GetResourceString("Members.Admin"),
                                Subject = _localizationService.GetResourceString("Members.NewMemberSubject")
                            };
                            email.Body = _emailService.EmailTemplate(email.NameTo, sb.ToString());
                            _emailService.SendMail(email);
                        }

                        _activityService.MemberJoined(newUser);
                        EventManager.Instance.FireAfterRegisterUser(this,
                                                                    new RegisterUserEventArgs { User = newUser });
                    }
                    catch (Exception)
                    {
                        status = MembershipCreateStatus.UserRejected;
                    }
                }
            }

            return status;
        }


        #endregion

        #region 查询用户实例

        /// <summary>
        /// 通过用户Id取得用户实例
        /// </summary>
        /// <param name="id">用户Id</param>
        /// <returns></returns>
        public MembershipUser Get(Guid id)
        {
            return _context.MembershipUser
                .Include(x => x.Roles)
                .FirstOrDefault(x => x.Id == id);
        }

        /// <summary>
        /// 按用户账号Id查找用户实例
        /// </summary>
        /// <param name="id">用户账号Id</param>
        /// <returns></returns>
        public MembershipUser GetUser(Guid id)
        {
            return Get(id);
        }

        /// <summary>
        /// Get a user by username
        /// </summary>
        /// <param name="username"></param>
        /// <param name="removeTracking"></param>
        /// <returns></returns>
        public MembershipUser GetUser(string username, bool removeTracking = false)
        {
            MembershipUser member;

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

            // TODO: Benjamin, 需要Review这块的必要性！
            // Do a check to log out the user if they are logged in and have been deleted
            if (member == null && HttpContext.Current.User.Identity.Name == username)
            {
                // Member is null so doesn't exist, yet they are logged in with that username - Log them out
                FormsAuthentication.SignOut();
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
                    .FirstOrDefault(name => name.MobilePhone == MobilePhone);
            }
            return _context.MembershipUser
                .Include(x => x.Roles)
                .FirstOrDefault(name => name.MobilePhone == MobilePhone);
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
        public IList<MembershipUser> GetAll()
        {
            return _context.MembershipUser.ToList();
        }


        #endregion

        /// <summary>
        /// 取得最新注册的amountToTake个用户实例的集合
        /// </summary>
        /// <param name="amountToTake"></param>
        /// <param name="isApproved">仅筛选已经审核过的用户实例</param>
        /// <returns></returns>
        public IList<MembershipUser> GetLatestUsers(int amountToTake, bool isApproved = false)
        {
            if (isApproved)
            {
                return _context.MembershipUser.Include(x => x.Roles).AsNoTracking()
                            .Where(x => x.IsApproved == true)
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

            if (!user.IsApproved)
            {
                LastLoginStatus = LoginAttemptStatus.UserNotApproved;
                //return false;
            }

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
        /// <param name="username"></param>
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






        //  =================以下代码还需要Review=================



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

        public PagedList<MembershipUser> GetAll(int pageIndex, int pageSize)
        {
            var totalCount = _context.MembershipUser.Count();
            var results = _context.MembershipUser
                                .OrderBy(x => x.UserName)
                                .Skip((pageIndex - 1) * pageSize)
                                .Take(pageSize)
                                .ToList();

            return new PagedList<MembershipUser>(results, pageIndex, pageSize, totalCount);
        }

        public PagedList<MembershipUser> SearchMembers(string search, int pageIndex, int pageSize)
        {
            search = StringUtils.SafePlainText(search);
            var query = _context.MembershipUser
                                .Where(x => x.UserName.ToUpper().Contains(search.ToUpper()) || x.Email.ToUpper().Contains(search.ToUpper()));

            var results = query
                .OrderBy(x => x.UserName)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return new PagedList<MembershipUser>(results, pageIndex, pageSize, query.Count());
        }

        public IList<MembershipUser> SearchMembers(string username, int amount)
        {
            username = StringUtils.SafePlainText(username);
            return _context.MembershipUser
                .Where(x => x.UserName.ToUpper().Contains(username.ToUpper()))
                .OrderBy(x => x.UserName)
                .Take(amount)
                .ToList();
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

                var existingUser = Get(user.Id);

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
        /// 删除用户下的所有Post和Topic集合
        /// </summary>
        /// <param name="user"></param>
        /// <param name="unitOfWork"></param>
        public void ScrubUsers(MembershipUser user, IUnitOfWork unitOfWork)
        {
            // TODO - This REALLY needs to be refactored

            // PROFILE
            user.Website = string.Empty;
            user.Twitter = string.Empty;
            user.Facebook = string.Empty;
            user.Avatar = string.Empty;
            user.Signature = string.Empty;

            // User Votes
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

            // User Badges
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

            // User category notifications
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

            // User PM Received
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

            // User PM Sent
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

            // User Favourites
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

        #region Membership账号的导入和导出

        /// <summary>
        /// Convert all users into CSV format (e.g. for export)
        /// </summary>
        /// <returns></returns>
        public string ToCsv()
        {
            var csv = new StringBuilder();
            var userlist = GetAll();
            foreach (var user in userlist)
            {
                //csv.AppendFormat("{0},{1},{2},{3},{4},{5},{6},{7}", user.UserName, user.Email, user.CreateDate, user.Age,
                //    user.Location, user.Website, user.Facebook, user.Signature);

                csv.AppendFormat("{0},{1},{2}", user.UserName, user.Email, user.CreateDate);

                csv.AppendLine();
            }

            return csv.ToString();
        }

        /// <summary>
        /// Extract users from CSV format and import them
        /// </summary>
        /// <returns></returns>
        public CsvReport FromCsv(List<string> allLines)
        {
            var usersProcessed = new List<string>();
            var commaSeparator = new[] { ',' };
            var report = new CsvReport();

            if (allLines == null || allLines.Count == 0)
            {
                report.Errors.Add(new CsvErrorWarning
                {
                    ErrorWarningType = CsvErrorWarningType.BadDataFormat,
                    Message = "No users found."
                });
                return report;
            }
            var settings = _settingsService.GetSettings(true);
            var lineCounter = 0;
            foreach (var line in allLines)
            {
                try
                {
                    lineCounter++;

                    // Each line is made up of n items in a predefined order

                    var values = line.Split(commaSeparator);

                    if (values.Length < 2)
                    {
                        report.Errors.Add(new CsvErrorWarning
                        {
                            ErrorWarningType = CsvErrorWarningType.MissingKeyOrValue,
                            Message = $"Line {lineCounter}: insufficient values supplied."
                        });

                        continue;
                    }

                    var userName = values[0];

                    if (userName.IsNullEmpty())
                    {
                        report.Errors.Add(new CsvErrorWarning
                        {
                            ErrorWarningType = CsvErrorWarningType.MissingKeyOrValue,
                            Message = $"Line {lineCounter}: no username supplied."
                        });

                        continue;
                    }

                    var email = values[1];
                    if (email.IsNullEmpty())
                    {
                        report.Errors.Add(new CsvErrorWarning
                        {
                            ErrorWarningType = CsvErrorWarningType.MissingKeyOrValue,
                            Message = $"Line {lineCounter}: no email supplied."
                        });

                        continue;
                    }

                    // get the user
                    var userToImport = GetUser(userName);

                    if (userToImport != null)
                    {
                        report.Errors.Add(new CsvErrorWarning
                        {
                            ErrorWarningType = CsvErrorWarningType.AlreadyExists,
                            Message = $"Line {lineCounter}: user already exists in forum."
                        });

                        continue;
                    }

                    if (usersProcessed.Contains(userName))
                    {
                        report.Errors.Add(new CsvErrorWarning
                        {
                            ErrorWarningType = CsvErrorWarningType.AlreadyExists,
                            Message = $"Line {lineCounter}: user already exists in import file."
                        });

                        continue;
                    }

                    usersProcessed.Add(userName);

                    userToImport = CreateEmptyUser();
                    userToImport.UserName = userName;
                    userToImport.Slug = ServiceHelpers.GenerateSlug(userToImport.UserName, GetUserBySlugLike(ServiceHelpers.CreateUrl(userToImport.UserName)), userToImport.Slug);
                    userToImport.Email = email;
                    userToImport.IsApproved = true;
                    userToImport.PasswordSalt = StringUtils.CreateSalt(AppConstants.SaltSize);

                    string createDateStr = null;
                    if (values.Length >= 3)
                    {
                        createDateStr = values[2];
                    }
                    userToImport.CreateDate = createDateStr.IsNullEmpty() ? DateTime.Now : DateTime.Parse(createDateStr);

                    if (values.Length >= 4)
                    {
                        //TODO: Benjamin Check again and fix the issue.
                        // userToImport.Age = Int32.Parse(values[3]);
                    }
                    if (values.Length >= 5)
                    {
                        userToImport.Location = values[4];
                    }
                    if (values.Length >= 6)
                    {
                        userToImport.Website = values[5];
                    }
                    if (values.Length >= 7)
                    {
                        userToImport.Facebook = values[6];
                    }
                    if (values.Length >= 8)
                    {
                        userToImport.Signature = values[7];
                    }
                    userToImport.Roles = new List<MembershipRole> { settings.NewMemberStartingRole };
                    Add(userToImport);
                }
                catch (Exception ex)
                {
                    report.Errors.Add(new CsvErrorWarning { ErrorWarningType = CsvErrorWarningType.GeneralError, Message = ex.Message });
                }
            }

            return report;
        }

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
            var existingUser = Get(user.Id);
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
            var existingUser = Get(user.Id);

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

        #region 未来拓展方法

        /// <summary>
        /// Get a user by email address
        /// </summary>
        /// <param name="email"></param>
        /// <param name="removeTracking"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Get by posts and date
        /// </summary>
        /// <param name="amoutOfDaysSinceRegistered"></param>
        /// <param name="amoutOfPosts"></param>
        /// <returns></returns>
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



        /// <summary>
        /// Gets a user by their facebook id
        /// </summary>
        /// <param name="facebookId"></param>
        /// <returns></returns>
        public MembershipUser GetUserByFacebookId(long facebookId)
        {
            return _context.MembershipUser
                .Include(x => x.Roles)
                .FirstOrDefault(name => name.FacebookId == facebookId);
        }

        public MembershipUser GetUserByTwitterId(string twitterId)
        {
            return _context.MembershipUser
                .Include(x => x.Roles)
                .FirstOrDefault(name => name.TwitterAccessToken == twitterId);
        }

        public MembershipUser GetUserByGoogleId(string googleId)
        {
            return _context.MembershipUser
                .Include(x => x.Roles)
                .FirstOrDefault(name => name.GoogleAccessToken == googleId);
        }

        /// <summary>
        /// Get users by openid token
        /// </summary>
        /// <param name="openId"></param>
        /// <returns></returns>
        public MembershipUser GetUserByOpenIdToken(string openId)
        {
            openId = StringUtils.GetSafeHtml(openId);
            return _context.MembershipUser
                .Include(x => x.Roles)
                .FirstOrDefault(name => name.MiscAccessToken == openId);
        }


        #endregion

    }

}
