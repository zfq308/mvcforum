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
        #region 增删改用户
        MembershipUser Add(MembershipUser newUser);
        void UnlockUser(string username, bool resetPasswordAttempts);
        MembershipCreateStatus CreateUser(MembershipUser newUser);
        MembershipUser CreateEmptyUser();
        void ProfileUpdated(MembershipUser user);
        bool Delete(MembershipUser user, IUnitOfWork unitOfWork);
        void Create50TestAccount();
        void Create5SupplierAccount();
        /// <summary>
        /// Completed scrubs a users account clean
        /// Clears everything - Posts, polls, votes, favourites, profile etc...
        /// </summary>
        /// <param name="user"></param>
        /// <param name="unitOfWork"></param>
        void ScrubUsers(MembershipUser user, IUnitOfWork unitOfWork);
        #endregion

        #region 搜索

        PagedList<MembershipUser> SearchMembers(string search, int pageIndex, int pageSize);

        IList<MembershipUser> SearchMembers(string username, int amount);
        IList<MembershipUser> SearchMembers(MembershipUserSearchModel searchusermodel, int amount);
        #endregion

        #region 报表导入导出
        string RemoveHTMLToCSV(string source);
        string ToCsv();
        string ToCsv(List<MembershipUser> userlist, bool isAdmin);
        CsvReport FromCsv(List<string> allLines);

        #endregion

        #region 查找用户
        IList<MembershipUser> GetActiveMembers();
        IList<MembershipUser> GetAll(bool isApproved = false);
        PagedList<MembershipUser> GetAll(int pageIndex, int pageSize, bool isApproved = false);
        MembershipUser GetUser(Guid id);
        MembershipUser GetUser(string username, bool removeTracking = false);
        MembershipUser GetUserByMobilePhone(string MobilePhone, bool removeTracking = false);
        MembershipUser GetUserBySlug(string slug);
        IList<MembershipUser> GetUserBySlugLike(string slug);
        IList<MembershipUser> GetUsersById(List<Guid> guids);
        IList<MembershipUser> GetLatestUsers(int amountToTake, bool isApproved = false,bool RemoveMarriedFilter=false);

        int MemberCount(bool isApproved = false);


        #region 暂不适用代码

        MembershipUser GetUserByEmail(string email, bool removeTracking = false);
        MembershipUser GetUserByFacebookId(long facebookId);
        MembershipUser GetUserByTwitterId(string twitterId);
        MembershipUser GetUserByGoogleId(string googleId);
        MembershipUser GetUserByOpenIdToken(string openId);
        IList<MembershipUser> GetLowestPointUsers(int amountToTake);
        IList<MembershipUser> GetUsersByDaysPostsPoints(int amoutOfDaysSinceRegistered, int amoutOfPosts);

        #endregion

        #endregion

        #region 密码相关功能
        bool ChangePassword(MembershipUser user, string oldPassword, string newPassword);
        bool ResetPassword(MembershipUser user, string newPassword);
        void UpdateUserRole(Guid id, MembershipRole role);
        bool UpdatePasswordResetToken(MembershipUser user);
        bool ClearPasswordResetToken(MembershipUser user);
        bool IsPasswordResetTokenValid(MembershipUser user, string token);

        #endregion

        #region 其他辅助功能
        MembershipUser SanitizeUser(MembershipUser membershipUser);
        bool ValidateUser(string userName, string password, int maxInvalidPasswordAttempts);
        LoginAttemptStatus LastLoginStatus { get; }
        string[] GetRolesForUser(string username);
        string ErrorCodeToString(MembershipCreateStatus createStatus);
        #endregion
    }
}
