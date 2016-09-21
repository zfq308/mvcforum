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

    #region Search相关ViewModel

    public class SearchMemberResultSingleMemberViewModel
    {
        [ForumMvcResourceDisplayName("Members.Label.Username")]
        public string UserName { get; set; }

        public string NiceUrl { get; set; }

        [ForumMvcResourceDisplayName("Members.Label.DateJoined")]
        public DateTime CreateDate { get; set; }

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

    #endregion


    public class MemberAddViewModel
    {
        [Required(ErrorMessage = "请输入您要注册的账号。")]
        [ForumMvcResourceDisplayName("Members.Label.Username")]
        [StringLength(150, MinimumLength = 4)]
        [Remote("CheckUserExistWhenRegister", "Members", HttpMethod = "POST", ErrorMessage = "账号已经存在,再重新输入一个账号吧。")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "行不更名坐不改姓，你是谁？")]
        [Display(Name = "真实姓名")]
        [StringLength(24, MinimumLength = 2, ErrorMessage = "姓名的长度最少为2位")]
        public string RealName { get; set; }

        [Required(ErrorMessage = "亲，你的昵称是什么？")]
        [Display(Name = "昵称")]
        [StringLength(24, MinimumLength = 2, ErrorMessage = "昵称的长度最少为2位")]
        public string AliasName { get; set; }

        [Required(ErrorMessage = "不填电话，你让我怎么联系你？！")]
        [ForumMvcResourceDisplayName("Members.Label.MobilePhone")]
        [StringLength(11, MinimumLength = 11, ErrorMessage = "你开玩笑吧，手机号码是11位哦！")]
        //[RegularExpression(@"((\d{11})|^((\d{7,8})|(\d{4}|\d{3})-(\d{7,8})|(\d{4}|\d{3})-(\d{7,8})-(\d{4}|\d{3}|\d{2}|\d{1})|(\d{7,8})-(\d{4}|\d{3}|\d{2}|\d{1}))$)", ErrorMessage = "格式不正确")]
        [Remote("CheckTelphoneExistWhenRegister", "Members", HttpMethod = "POST", ErrorMessage = "此手机号码已经注册。")]
        public string MobilePhone { get; set; }

        [Required(ErrorMessage = "快看手机，立即查看验证码。")]
        [ForumMvcResourceDisplayName("Members.Label.VerifyCode")]
        [StringLength(4, MinimumLength = 4)]
        public string VerifyCode { get; set; }

        //[Required]
        //[EmailAddress]
        //[ForumMvcResourceDisplayName("Members.Label.EmailAddress")]
        //public string Email { get; set; }

        [Required(ErrorMessage = "别忘记输入密码了。")]
        [StringLength(20, MinimumLength = 6)]
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
        [Display(Name = "已阅读并同意")]
        [MustBeTrue(ErrorMessage = "TermsAndConditions.Label.AgreeError")]
        public bool ReadPolicyFirst { get; set; }

        [ForumMvcResourceDisplayName("Members.Label.UserIsApproved")]
        public bool IsApproved { get; set; }

        [ForumMvcResourceDisplayName("Members.Label.Comment")]
        [DataType(DataType.MultilineText)] //当前字段是个多行文本  
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

        [Required(ErrorMessage = "账号信息不能缺失。")]
        [Display(Name = "账号")]
        [StringLength(50, MinimumLength = 4, ErrorMessage = "账号是一段长度介于4位到50位的文字")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "真实姓名信息不能缺失。")]
        [Display(Name = "真实姓名")]
        [StringLength(16, MinimumLength = 2, ErrorMessage = "您的姓名是一段长度介于2位到16位的文字")]
        public string RealName { get; set; }

        [Required(ErrorMessage = "昵称信息不能缺失。")]
        [Display(Name = "昵称")]
        [StringLength(24, MinimumLength = 2, ErrorMessage = "您的昵称是一段长度介于2位到16位的文字")]
        public string AliasName { get; set; }

        [Required]
        [Display(Name = "性别")]
        public Enum_Gender Gender { get; set; }

        [Required]
        [Display(Name = "出生日期")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [DataType(DataType.Date, ErrorMessage = "请按 2000-01-01 的日期格式输入您的出生日期信息")]
        public DateTime Birthday { get; set; }

        [Required]
        [Display(Name = "生日历法")]
        public Enum_Calendar IsLunarCalendar { get; set; }

        [Required]
        [Display(Name = "婚否")]
        public Enum_MarriedStatus IsMarried { get; set; }

        [Display(Name = "身高cm")]
        [Range(0, 250, ErrorMessage = "请输入有效的身高数据（0~250,单位cm,无小数）。")]
        [RegularExpression(@"^\d+$", ErrorMessage = "请输入有效的身高数据（0~250,单位cm,无小数）。")]
        public int Height { get; set; }

        [Display(Name = "体重kg")]
        [Range(0, 150, ErrorMessage = "请输入有效的体重数据（0~150,单位cm,无小数）。")]
        [RegularExpression(@"^\d+$", ErrorMessage = "请输入有效的体重数据（0~150,单位kg,无小数）。")]
        public int Weight { get; set; }

        [Required]
        [Display(Name = "学历")]
        public string Education { get; set; }

        [Required]
        [Display(Name = "学校所在省份")]
        public string SchoolProvince { get; set; }

        [Required]
        [Display(Name = "学校所在城市")]
        public string SchoolCity { get; set; }

        [Required(ErrorMessage = "学校名称是一段长度介于2位到20位的文字")]
        [Display(Name = "学校名称")]
        [StringLength(20, MinimumLength = 2)]
        public string SchoolName { get; set; }

        [Required]
        [Display(Name = "居住地所在省份")]
        public string LocationProvince { get; set; }

        [Required]
        [Display(Name = "居住地所在城市")]
        public string LocationCity { get; set; }

        [Required]
        [Display(Name = "居住地所在县区")]
        public string LocationCounty { get; set; }

        [Required(ErrorMessage = "家乡是一段长度介于2位到100位的文字")]
        [Display(Name = "家乡")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "家乡是一段长度介于2位到100位的文字")]
        public string HomeTown { get; set; }

        [Required]
        [Display(Name = "职业")]
        [StringLength(20, MinimumLength = 2, ErrorMessage = "职业是一段长度介于2位到20位的文字")]
        public string Job { get; set; }

        [Required]
        [Display(Name = "月收入区段")]
        public Enum_IncomeRange IncomeRange { get; set; }

        [Required(ErrorMessage = "兴趣爱好是一段长度介于2位到100位的文字")]
        [Display(Name = "兴趣爱好")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "兴趣爱好是一段长度介于2位到100位的文字")]
        public string Interest { get; set; }

        [Required]
        [Display(Name = "联系电话")]
        [StringLength(11, MinimumLength = 11)]
        public string MobilePhone { get; set; }

        [Display(Name = "个性签名")]
        [StringLength(1000)]
        [AllowHtml]
        public string Signature { get; set; }

        /// <summary>
        /// 头像
        /// </summary>
        public string Avatar { get; set; }

        /// <summary>
        /// 账号是否有通过审核
        /// </summary>
        public bool IsApproved { get; set; }

        public bool AuditResult { get; set; }

        [Display(Name = "审核意见")]
        public string AuditComment { get; set; }

        [ForumMvcResourceDisplayName("Members.Label.UploadNewAvatar")]
        public HttpPostedFileBase[] Files { get; set; }


        public IList<MembershipUserPicture> MembershipUserPictures { get; set; }

    }

    public class MemberSearchConfigViewMode
    {
        [Display(Name = "账号")]
        [StringLength(50, MinimumLength = 4, ErrorMessage = "账号是一段长度介于4位到50位的文字")]
        public string UserName { get; set; }
      
        [Display(Name = "真实姓名")]
        [StringLength(16, MinimumLength = 2, ErrorMessage = "您的姓名是一段长度介于2位到16位的文字")]
        public string RealName { get; set; }
      
        [Display(Name = "昵称")]
        [StringLength(24, MinimumLength = 2, ErrorMessage = "您的昵称是一段长度介于2位到16位的文字")]
        public string AliasName { get; set; }

        [Display(Name = "性别")]
        public Enum_Gender Gender { get; set; }
        
        [Display(Name = "婚否")]
        public Enum_MarriedStatus IsMarried { get; set; }

        [Display(Name = "年龄")]
        [Range(15, 70, ErrorMessage = "请输入有效的年龄数据（15~70）。")]
        [RegularExpression(@"^\d+$", ErrorMessage = "请输入有效的年龄数据（15~70）。")]
        public int Age { get; set; }

        [Display(Name = "学历")]
        public string Education { get; set; }

        [Display(Name = "毕业院校")]
        [StringLength(20, MinimumLength = 2)]
        public string SchoolName { get; set; }

        [Display(Name = "居住地所在省份")]
        public string LocationProvince { get; set; }

        [Display(Name = "居住地所在城市")]
        public string LocationCity { get; set; }

        [Display(Name = "居住地所在县区")]
        public string LocationCounty { get; set; }

        [Display(Name = "家乡")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "家乡是一段长度介于2位到100位的文字")]
        public string HomeTown { get; set; }

        [Display(Name = "职业")]
        [StringLength(20, MinimumLength = 2, ErrorMessage = "职业是一段长度介于2位到20位的文字")]
        public string Job { get; set; }

        [Display(Name = "月收入区段")]
        public Enum_IncomeRange IncomeRange { get; set; }

        [Display(Name = "最近未登录天数")]
        [RegularExpression(@"^\d+$", ErrorMessage = "请输入有效的年龄数据（15~70）。")]
        public int NoLoginDaySinceLastLoginDate { get; set; }
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
        public IList<MembershipUserPicture> MembershipUserPictures { get; set; }
        public MembershipTodayStar MeiRiZhiXing { get; set; }
        public string RoleId { get; set; }
        public int FollowStatus { get; set; }
        public int UserType { get; set; }

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


    public class ActiveMembersViewModel
    {
        public IList<MembershipUser> ActiveMembers { get; set; }
    }


    #region 密码操作相关ViewModel

    public class ForgotPasswordViewModel
    {
        [ForumMvcResourceDisplayName("Members.Label.Username")]
        [Required]
        public string UserName { get; set; }

        [Display(Name ="手机号码")]
        [Required]
        public string Telphone { get; set; }


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

    #endregion

}