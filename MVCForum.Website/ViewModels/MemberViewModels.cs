using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web;
using System.Web.Mvc;
using MVCForum.Domain.DomainModel;
using MVCForum.Domain.DomainModel.Enums;
using MVCForum.Website.Application;
using MVCForum.Website.Application.ActionFilterAttributes;

namespace MVCForum.Website.ViewModels
{
    public class ActiveMembersViewModel
    {
        public IList<MembershipUser> ActiveMembers { get; set; }
    }

    public class MemberAddViewModel
    {
        [Required(ErrorMessage ="请输入您要注册的账号。")]
        [ForumMvcResourceDisplayName("Members.Label.Username")]
        [StringLength(150, MinimumLength = 4)]
        public string UserName { get; set; }

        [Required(ErrorMessage = "行不更名坐不改姓，你是谁？")]
        [ForumMvcResourceDisplayName("Members.Label.RealName")]
        [StringLength(24, MinimumLength = 2, ErrorMessage = "姓名的长度最少为2位")]
        public string RealName { get; set; }

        [Required(ErrorMessage = "亲，你的昵称是什么？")]
        [Display(Name = "昵称")]
        [StringLength(24, MinimumLength = 2,ErrorMessage ="昵称的长度最少为2位")]
        public string AliasName { get; set; }

        [Required(ErrorMessage = "不填电话，你让我怎么联系你？！")]
        [ForumMvcResourceDisplayName("Members.Label.MobilePhone")]
        [StringLength(11, MinimumLength = 11,ErrorMessage ="你开玩笑吧，手机号码是11位哦！")]
        public string MobilePhone { get; set; }

        [Required(ErrorMessage = "快看手机，立即查看验证码。")]
        [ForumMvcResourceDisplayName("Members.Label.VerifyCode")]
        [StringLength(6, MinimumLength = 6)]
        public string VerifyCode { get; set; }

        //[Required]
        //[EmailAddress]
        //[ForumMvcResourceDisplayName("Members.Label.EmailAddress")]
        //public string Email { get; set; }

        [Required(ErrorMessage ="别忘记输入密码了。")]
        [StringLength(100, MinimumLength = 6)]
        [DataType(DataType.Password)]
        [ForumMvcResourceDisplayName("Members.Label.Password")]
        public string Password { get; set; }


        [Required(ErrorMessage = "别忘记输入密码了。")]
        [DataType(DataType.Password)]
        [System.ComponentModel.DataAnnotations.Compare("Password")]
        [ForumMvcResourceDisplayName("Members.Label.ConfirmPassword")]
        public string ConfirmPassword { get; set; }

        [Required]
        public int MinPasswordLength { get; set; }

        [Required]
        [Display(Name ="已阅读并同意")]
        [MustBeTrue(ErrorMessage = "TermsAndConditions.Label.AgreeError")]
        public bool ReadPolicyFirst { get; set; }

        [ForumMvcResourceDisplayName("Members.Label.UserIsApproved")]
        public bool IsApproved { get; set; }

        [ForumMvcResourceDisplayName("Members.Label.Comment")]
        public string Comment { get; set; }

        [ForumMvcResourceDisplayName("Members.Label.Roles")]
        public string[] Roles { get; set; }

        public IList<MembershipRole> AllRoles { get; set; }
        public string SpamAnswer { get; set; }
        public string ReturnUrl { get; set; }
        public string SocialProfileImageUrl { get; set; }
        public string UserAccessToken { get; set; }
        public LoginType LoginType { get; set; }
    }

    public class MemberFrontEndEditViewModel
    {
        [Required]
        public Guid Id { get; set; }

        [Required]
        [ForumMvcResourceDisplayName("Members.Label.Username")]
        [StringLength(150, MinimumLength = 4)]
        public string UserName { get; set; }

        [Required]
        [ForumMvcResourceDisplayName("Members.Label.RealName")]
        [StringLength(16, MinimumLength = 4)]
        public string RealName { get; set; }

        [Required]
        [Display(Name = "昵称")]
        [StringLength(24, MinimumLength = 2)]
        public string AliasName { get; set; }

        [Required]
        [ForumMvcResourceDisplayName("Members.Label.Gender")]
        public int Gender { get; set; }

        [Required]
        [ForumMvcResourceDisplayName("Members.Label.Birthday")]
        public DateTime Birthday { get; set; }

        [Required]
        [ForumMvcResourceDisplayName("Members.Label.IsLunarCalendar")]
        public bool IsLunarCalendar { get; set; }

        [Required]
        [ForumMvcResourceDisplayName("Members.Label.IsMarried")]
        public bool IsMarried { get; set; }

        [ForumMvcResourceDisplayName("Members.Label.Height")]
        [Range(0, 250)]
        public int Height { get; set; }

        [ForumMvcResourceDisplayName("Members.Label.Weight")]
        [Range(0, 250)]
        public int Weight { get; set; }

        [Required]
        [ForumMvcResourceDisplayName("Members.Label.Education")]
        public string Education { get; set; }

        [Required]
        [ForumMvcResourceDisplayName("Members.Label.Location")]
        [StringLength(100, MinimumLength = 2)]
        public string Location { get; set; }

        [Required]
        [ForumMvcResourceDisplayName("Members.Label.SchoolProvince")]
        public string SchoolProvince { get; set; }

        [Required]
        [ForumMvcResourceDisplayName("Members.Label.SchoolCity")]
        public string SchoolCity { get; set; }

        [Required]
        [ForumMvcResourceDisplayName("Members.Label.SchoolName")]
        [StringLength(20, MinimumLength = 2)]
        public string SchoolName { get; set; }

        [Required]
        [ForumMvcResourceDisplayName("Members.Label.HomeTownProvince")]
        public string HomeTownProvince { get; set; }

        [Required]
        [ForumMvcResourceDisplayName("Members.Label.HomeTownCity")]
        public string HomeTownCity { get; set; }

        [Required]
        [ForumMvcResourceDisplayName("Members.Label.HomeTownCounty")]
        public string HomeTownCounty { get; set; }

        [Required]
        [ForumMvcResourceDisplayName("Members.Label.Job")]
        [StringLength(20, MinimumLength = 2)]
        public string Job { get; set; }

        [Required]
        [ForumMvcResourceDisplayName("Members.Label.IncomeRange")]
        public int IncomeRange { get; set; }

        [Required]
        [ForumMvcResourceDisplayName("Members.Label.Interest")]
        [StringLength(100, MinimumLength = 2)]
        public string Interest { get; set; }

        [Required]
        [ForumMvcResourceDisplayName("Members.Label.MobilePhone")]
        [StringLength(11, MinimumLength = 11)]
        public string MobilePhone { get; set; }

        [ForumMvcResourceDisplayName("Members.Label.Signature")]
        [StringLength(1000)]
        [AllowHtml]
        public string Signature { get; set; }

        #region Unuse code

        //[ForumMvcResourceDisplayName("Members.Label.EmailAddress")]
        //[EmailAddress]
        //[Required]
        //public string Email { get; set; }

        //[ForumMvcResourceDisplayName("Members.Label.UploadNewAvatar")]
        //public HttpPostedFileBase[] Files { get; set; }

        //[ForumMvcResourceDisplayName("Members.Label.Facebook")]
        //[Url]
        //[StringLength(60)]
        //public string Facebook { get; set; }

        //public string Avatar { get; set; }

        //public bool DisableFileUploads { get; set; }

        //[ForumMvcResourceDisplayName("Members.Label.DisableEmailNotifications")]
        //public bool DisableEmailNotifications { get; set; }

        //public int AmountOfPoints { get; set; }

        //[ForumMvcResourceDisplayName("Members.Label.Age")]
        //[Range(0, int.MaxValue)]
        //public int? Age { get; set; }

        //[ForumMvcResourceDisplayName("Members.Label.Website")]
        //[Url]
        //[StringLength(100)]
        //public string Website { get; set; }

        //[ForumMvcResourceDisplayName("Members.Label.Twitter")]
        //[Url]
        //[StringLength(60)]
        //public string Twitter { get; set; }

        //[Required]
        //[StringLength(12, MinimumLength = 4)]
        //public string QQ { get; set; }

        #endregion
    }

    public class LogOnViewModel
    {
        public string ReturnUrl { get; set; }

        /// <summary>
        /// 账号
        /// </summary>
        [Required]
        [ForumMvcResourceDisplayName("Members.Label.Username")]
        public string UserName { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [ForumMvcResourceDisplayName("Members.Label.Password")]
        public string Password { get; set; }

        [ForumMvcResourceDisplayName("Members.Label.RememberMe")]
        public bool RememberMe { get; set; }

        [Required]
        [ForumMvcResourceDisplayName("Members.Label.VerifyCode")]
        public string VerifyCode { get; set; }
    }

    public class ViewMemberViewModel
    {
        public MembershipUser User { get; set; }
        public Guid LoggedOnUserId { get; set; }
        public PermissionSet Permissions { get; set; }
    }

    public class ViewMemberDiscussionsViewModel
    {
        public IList<TopicViewModel> Topics { get; set; }
    }

    public class ViewAdminSidePanelViewModel
    {
        public MembershipUser CurrentUser { get; set; }
        public int NewPrivateMessageCount { get; set; }
        public bool CanViewPrivateMessages { get; set; }
        public bool IsDropDown { get; set; }
        public int ModerateCount { get; set; }
    }

    public class AdminMemberProfileToolsViewModel
    {
        public MembershipUser CurrentUser { get; set; }
    }

    public class AutocompleteViewModel
    {
        public string label { get; set; }
        public string value { get; set; }
        public string id { get; set; }
    }

    public class ReportMemberViewModel
    {
        public Guid Id { get; set; }
        public string Username { get; set; }
        public string Reason { get; set; }
    }

    public class ListLatestMembersViewModel
    {
        public Dictionary<string, string> Users { get; set; }
    }

    public class PublicMemberListViewModel
    {
        [ForumMvcResourceDisplayName("Members.Label.Users")]
        public IList<PublicSingleMemberListViewModel> Users { get; set; }
        public int? PageIndex { get; set; }
        public int? TotalCount { get; set; }
        public string Search { get; set; }
    }

    public class PublicSingleMemberListViewModel
    {
        [ForumMvcResourceDisplayName("Members.Label.Username")]
        public string UserName { get; set; }

        public string NiceUrl { get; set; }

        [ForumMvcResourceDisplayName("Members.Label.DateJoined")]
        public DateTime CreateDate { get; set; }

        public int TotalPoints { get; set; }
    }

    public class ForgotPasswordViewModel
    {
        [ForumMvcResourceDisplayName("Members.Label.Username")]
        [Required]
        public string UserName { get; set; }


        [ForumMvcResourceDisplayName("Members.Label.VerifyCode")]
        [Required]
        public string VerifyCode { get; set; }

    }

    public class ChangePasswordViewModel
    {
        [Required]
        [DataType(DataType.Password)]
        [ForumMvcResourceDisplayName("Members.Label.CurrentPassword")]
        public string OldPassword { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 6)]
        [DataType(DataType.Password)]
        [ForumMvcResourceDisplayName("Members.Label.NewPassword")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [ForumMvcResourceDisplayName("Members.Label.ConfirmNewPassword")]
        [System.ComponentModel.DataAnnotations.Compare("NewPassword")]
        public string ConfirmPassword { get; set; }
    }

    public class ResetPasswordViewModel
    {
        [Required]
        public Guid? Id { get; set; }

        [Required]
        public string Token { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 6)]
        [DataType(DataType.Password)]
        [ForumMvcResourceDisplayName("Members.Label.NewPassword")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [ForumMvcResourceDisplayName("Members.Label.ConfirmNewPassword")]
        [System.ComponentModel.DataAnnotations.Compare("NewPassword")]
        public string ConfirmPassword { get; set; }

    }



}