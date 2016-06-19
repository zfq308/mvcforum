using System;
using System.Collections.Generic;
using MVCForum.Domain.DomainModel;
using MVCForum.Domain.Interfaces.UnitOfWork;

namespace MVCForum.Domain.Interfaces.Services
{
    public enum LoginAttemptStatus
    {
        /// <summary>
        /// 用户登录成功
        /// </summary>
        LoginSuccessful,
        /// <summary>
        /// 查无此人
        /// </summary>
        UserNotFound,
        /// <summary>
        /// 密码不匹配
        /// </summary>
        PasswordIncorrect,
        /// <summary>
        /// 尝试多次登录失败
        /// </summary>
        PasswordAttemptsExceeded,
        /// <summary>
        /// 账户被锁定
        /// </summary>
        UserLockedOut,
        /// <summary>
        /// 用户账户还没通过审核
        /// </summary>
        UserNotApproved,
        Banned
    }

    public partial interface IMembershipService
    {
        MembershipUser Add(MembershipUser newUser);
        MembershipUser SanitizeUser(MembershipUser membershipUser);
        bool ValidateUser(string userName, string password, int maxInvalidPasswordAttempts);
        LoginAttemptStatus LastLoginStatus { get; }
        string[] GetRolesForUser(string username);
        MembershipUser Get(Guid id);
        MembershipUser GetUser(string username, bool removeTracking = false);
        MembershipUser GetUserByEmail(string email, bool removeTracking = false);
        MembershipUser GetUserByMobilePhone(string MobilePhone, bool removeTracking = false);
        MembershipUser GetUserBySlug(string slug);
        IList<MembershipUser> GetUserBySlugLike(string slug);
        MembershipUser GetUserByFacebookId(long facebookId);
        MembershipUser GetUserByTwitterId(string twitterId);
        MembershipUser GetUserByGoogleId(string googleId);
        MembershipUser GetUserByOpenIdToken(string openId);
        IList<MembershipUser> GetUsersById(List<Guid> guids);
        IList<MembershipUser> GetUsersByDaysPostsPoints(int amoutOfDaysSinceRegistered, int amoutOfPosts);
        MembershipUser GetUser(Guid id);
        bool ChangePassword(MembershipUser user, string oldPassword, string newPassword);
        bool ResetPassword(MembershipUser user, string newPassword);
        void UnlockUser(string username, bool resetPasswordAttempts);
        MembershipCreateStatus CreateUser(MembershipUser newUser);
        string ErrorCodeToString(MembershipCreateStatus createStatus);
        MembershipUser CreateEmptyUser();
        IList<MembershipUser> GetAll();
        PagedList<MembershipUser> GetAll(int pageIndex, int pageSize);
        PagedList<MembershipUser> SearchMembers(string search, int pageIndex, int pageSize);
        IList<MembershipUser> SearchMembers(string username, int amount);
        IList<MembershipUser> GetActiveMembers();
        void ProfileUpdated(MembershipUser user);
        bool Delete(MembershipUser user, IUnitOfWork unitOfWork);
        IList<MembershipUser> GetLatestUsers(int amountToTake, bool isApproved = false);
        IList<MembershipUser> GetLowestPointUsers(int amountToTake);
        int MemberCount(bool isApproved = false);
        string ToCsv();
        CsvReport FromCsv(List<string> allLines);
        /// <summary>
        /// Completed scrubs a users account clean
        /// Clears everything - Posts, polls, votes, favourites, profile etc...
        /// </summary>
        /// <param name="user"></param>
        /// <param name="unitOfWork"></param>
        void ScrubUsers(MembershipUser user, IUnitOfWork unitOfWork);

        bool UpdatePasswordResetToken(MembershipUser user);
        bool ClearPasswordResetToken(MembershipUser user);
        bool IsPasswordResetTokenValid(MembershipUser user, string token);
    }


}
