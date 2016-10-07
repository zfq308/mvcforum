using MVCForum.Domain.Constants;
using MVCForum.Domain.DomainModel;
using MVCForum.Domain.DomainModel.Enums;
using MVCForum.Domain.DomainModel.General;
using MVCForum.Domain.Events;
using MVCForum.Domain.Interfaces;
using MVCForum.Domain.Interfaces.Services;
using MVCForum.Domain.Interfaces.UnitOfWork;
using MVCForum.Services;
using MVCForum.Services.Data.Context;
using MVCForum.Utilities;
using MVCForum.Website.Application;
using MVCForum.Website.Application.ActionFilterAttributes;
using MVCForum.Website.Areas.Admin.ViewModels;
using MVCForum.Website.ViewModels;
using MVCForum.Website.ViewModels.Mapping;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;
using System.Web.Security;
using MembershipCreateStatus = MVCForum.Domain.DomainModel.MembershipCreateStatus;
using MembershipUser = MVCForum.Domain.DomainModel.MembershipUser;

namespace MVCForum.Website.Controllers
{
    public partial class MembersController : BaseController
    {

        #region 成员变量

        //log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        log4net.ILog loggerForCoreAction = log4net.LogManager.GetLogger("CoreActionLog");
        log4net.ILog loggerForPerformance = log4net.LogManager.GetLogger("PerformanceLog");

        private readonly IPostService _postService;
        private readonly ITopicService _topicService;
        private readonly IReportService _reportService;
        private readonly IEmailService _emailService;
        private readonly IPrivateMessageService _privateMessageService;
        private readonly IBannedWordService _bannedWordService;
        private readonly ICategoryService _categoryService;
        private readonly IVerifyCodeService _verifyCodeService;
        private readonly MVCForumContext _context;
        private readonly IMembershipUserPictureService _MembershipUserPictureService;
        private readonly IMembershipTodayStarService _MembershipTodayStarService;
        private readonly IFollowService _FollowService;
        private readonly ILoggingService _loggingService;


        #endregion

        #region 建构式
        public MembersController(IMVCForumContext context,
            //基类Service
            ILoggingService loggingService,
            IUnitOfWorkManager unitOfWorkManager,
            IMembershipService membershipService,
            ILocalizationService localizationService,
            IRoleService roleService,
            ISettingsService settingsService,

            //其他Service
            IVerifyCodeService verifyCodeService,
            IPostService postService,
            IReportService reportService,
            IEmailService emailService,
            IPrivateMessageService privateMessageService,
            IBannedWordService bannedWordService,
            ITopicNotificationService topicNotificationService,
            IPollAnswerService pollAnswerService,
            IVoteService voteService,
            ICategoryService categoryService,
            ITopicService topicService,
            IFollowService followService,
            IMembershipUserPictureService MembershipUserPictureService,
            IMembershipTodayStarService MembershipTodayStarService
            )
            : base(loggingService, unitOfWorkManager, membershipService, localizationService, roleService, settingsService)
        {
            _loggingService = loggingService;
            _context = context as MVCForumContext;
            _postService = postService;
            _reportService = reportService;
            _emailService = emailService;
            _privateMessageService = privateMessageService;
            _bannedWordService = bannedWordService;
            _categoryService = categoryService;
            _topicService = topicService;
            _verifyCodeService = verifyCodeService;
            _FollowService = followService;
            _MembershipUserPictureService = MembershipUserPictureService;
            _MembershipTodayStarService = MembershipTodayStarService;

        }

        #endregion

        public ActionResult Index()
        {
            return View();
        }

        #region 用户设置操作
        [Authorize(Roles = AppConstants.AdminRoleName)]
        public ActionResult SrubAndBanUser(Guid id)
        {
            var user = base.MembershipService.GetUser(id);

            using (var unitOfWork = UnitOfWorkManager.NewUnitOfWork())
            {
                if (!user.Roles.Any(x => x.RoleName.Contains(AppConstants.AdminRoleName)))
                {
                    MembershipService.ScrubUsers(user, unitOfWork);

                    try
                    {
                        unitOfWork.Commit();
                        TempData[AppConstants.MessageViewBagName] = new GenericMessageViewModel
                        {
                            Message = LocalizationService.GetResourceString("Members.SuccessfulSrub"),
                            MessageType = GenericMessages.success
                        };
                    }
                    catch (Exception ex)
                    {
                        unitOfWork.Rollback();
                        LoggingService.Error(ex);
                        TempData[AppConstants.MessageViewBagName] = new GenericMessageViewModel
                        {
                            Message = LocalizationService.GetResourceString("Members.UnSuccessfulSrub"),
                            MessageType = GenericMessages.danger
                        };
                    }
                }
            }

            using (UnitOfWorkManager.NewUnitOfWork())
            {
                var viewModel = ViewModelMapping.UserToMemberEditViewModel(user);
                viewModel.AllRoles = RoleService.AllRoles();
                return Redirect(user.NiceUrl);
            }

        }

        [Authorize]
        public ActionResult BanMember(Guid id)
        {
            using (var unitOfWork = UnitOfWorkManager.NewUnitOfWork())
            {
                var user = MembershipService.GetUser(id);
                var permissions = RoleService.GetPermissions(null, UsersRole);

                if (permissions[SiteConstants.Instance.PermissionEditMembers].IsTicked)
                {

                    if (!user.Roles.Any(x => x.RoleName.Contains(AppConstants.AdminRoleName)))
                    {
                        user.IsBanned = true;

                        try
                        {
                            unitOfWork.Commit();
                            TempData[AppConstants.MessageViewBagName] = new GenericMessageViewModel
                            {
                                Message = LocalizationService.GetResourceString("Members.NowBanned"),
                                MessageType = GenericMessages.success
                            };
                        }
                        catch (Exception ex)
                        {
                            unitOfWork.Rollback();
                            LoggingService.Error(ex);
                            TempData[AppConstants.MessageViewBagName] = new GenericMessageViewModel
                            {
                                Message = LocalizationService.GetResourceString("Error.UnableToBanMember"),
                                MessageType = GenericMessages.danger
                            };
                        }
                    }
                }

                return Redirect(user.NiceUrl);
            }
        }

        [Authorize]
        public ActionResult UnBanMember(Guid id)
        {
            using (var unitOfWork = UnitOfWorkManager.NewUnitOfWork())
            {
                var user = MembershipService.GetUser(id);
                var permissions = RoleService.GetPermissions(null, UsersRole);

                if (permissions[SiteConstants.Instance.PermissionEditMembers].IsTicked)
                {
                    if (!user.Roles.Any(x => x.RoleName.Contains(AppConstants.AdminRoleName)))
                    {
                        user.IsBanned = false;

                        try
                        {
                            unitOfWork.Commit();
                            TempData[AppConstants.MessageViewBagName] = new GenericMessageViewModel
                            {
                                Message = LocalizationService.GetResourceString("Members.NowUnBanned"),
                                MessageType = GenericMessages.success
                            };
                        }
                        catch (Exception ex)
                        {
                            unitOfWork.Rollback();
                            LoggingService.Error(ex);
                            TempData[AppConstants.MessageViewBagName] = new GenericMessageViewModel
                            {
                                Message = LocalizationService.GetResourceString("Error.UnableToUnBanMember"),
                                MessageType = GenericMessages.danger
                            };
                        }
                    }
                }

                return Redirect(user.NiceUrl);
            }
        }
        #endregion

        #region 短信注册码
        /// <summary>
        /// 检查验证码是否正确
        /// </summary>
        /// <returns></returns>
        public ActionResult CheckCode()
        {
            bool result = false;
            //用户输入的验证码
            string checkCode = Request["CheckCode"].Trim();
            //取出存在session中的验证码
            string code = Session["code"].ToString();
            try
            {
                //验证是否一致
                if (checkCode != code)
                {
                    if (code == "!@#$")  //万能验证码
                    {
                        result = true;
                        Session["code"] = null;
                    }
                    else
                    {
                        result = false;
                    }
                }
                else
                {
                    result = true;
                    Session["code"] = null;
                }

                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                throw new Exception("短信验证失败", e);
            }
        }

        /// <summary>
        /// 检查用户填写的账号是否已经存在
        /// </summary>
        /// <returns></returns>
        public ActionResult CheckUserExistWhenRegister()
        {
            return CheckUserExistWhenRegister(Request["UserName"]);
        }

        [HttpPost]
        public ActionResult CheckUserExistWhenRegister(string username)
        {
            try
            {
                bool result;
                var user = MembershipService.GetUser(username);
                result = user == null ? true : false;
                return Json(result, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                LoggingService.Error(ex);
                throw;
            }
        }

        [HttpPost]
        public ActionResult CheckTelphoneExistWhenRegister(string MobilePhone)
        {
            try
            {
                bool result;
                var user = MembershipService.GetUserByMobilePhone(MobilePhone);
                result = user == null ? true : false;
                return Json(result, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                LoggingService.Error(ex);
                throw;
            }
        }

        /// <summary>
        /// 返回json到界面
        /// </summary>
        /// <returns>string</returns>
        public ActionResult GetVerifyCode()
        {
            try
            {
                bool result;
                //接收前台传过来的参数。短信验证码和手机号码
                string code = Request["Code"];
                string MobilePhone = Request["MobilePhone"];

                var user = MembershipService.GetUserByMobilePhone(MobilePhone);
                if (user == null)
                {
                    // 短信验证码存入session(session的默认失效时间30分钟) 
                    //也可存入Memcached缓存
                    Session.Add("code", code);
                    try
                    {
                        _verifyCodeService.SendVerifyCode(new VerifyCode(MobilePhone, VerifyCodeStatus.Waiting, code));
                        result = true;// 成功    
                        loggerForCoreAction.Info("发送验证码GetVerifyCode成功，目标手机为：" + MobilePhone);
                    }
                    catch (Exception ex)
                    {
                        result = false;// 失败    
                        loggerForCoreAction.Error("发送验证码GetVerifyCode失败，详细错误信息为：" + ex.Message);
                    }
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(false, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                loggerForCoreAction.Error("MembersController.GetVerifyCode方法执行出现异常，详细错误信息为：" + ex.Message);
                throw ex;
            }
        }

        public ActionResult GetVerifyCodeByAccount()
        {
            try
            {
                bool result;
                //接收前台传过来的参数。
                string code = Request["Code"];
                string UserName = Request["UserName"];

                var user = MembershipService.GetUser(UserName);
                if (user != null && !string.IsNullOrEmpty(user.MobilePhone))
                {
                    // 短信验证码存入session
                    Session.Add("code", code);
                    try
                    {
                        _verifyCodeService.SendVerifyCode(new VerifyCode(user.MobilePhone, VerifyCodeStatus.Waiting, code));
                        result = true;// 成功  
                        loggerForCoreAction.Info("发送验证码GetVerifyCodeByAccount成功，目标手机为：" + user.MobilePhone);
                    }
                    catch (Exception ex)
                    {
                        result = false;// 失败    
                        LoggingService.Error(ex.Message);
                        loggerForCoreAction.Error("发送验证码GetVerifyCodeByAccount失败，详细错误信息为：" + ex.Message);
                    }
                }
                else
                {
                    result = false;// 失败  
                    LoggingService.Error(string.Format("对应的用户信息：{0}不存在。", UserName));
                    ModelState.AddModelError("NoExistUser", "用户信息不存在。");
                }
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                loggerForCoreAction.Error("GetVerifyCodeByAccount()方法发生错误，详细信息为：" + ex.Message);
                ModelState.AddModelError("", LocalizationService.GetResourceString("Members.ResetPassword.Error"));
                return View();
            }
        }

        #endregion

        #region 用户注册
        /// <summary>
        /// 用户注册
        /// </summary>
        /// <returns></returns>
        public ActionResult Register()
        {
            if (SettingsService.GetSettings().SuspendRegistration != true)
            {
                //系统当前允许注册
                using (UnitOfWorkManager.NewUnitOfWork())
                {
                    var user = MembershipService.CreateEmptyUser();
                    var viewModel = new MemberAddViewModel
                    {
                        AliasName = user.AliasName,
                        UserName = user.UserName,
                        MobilePhone = user.MobilePhone,
                        Password = user.Password,
                        IsApproved = user.IsApproved,
                        Comment = user.Comment,
                        RealName = user.RealName,
                        ReadPolicyFirst = true,
                        AllRoles = RoleService.AllRoles()
                    };
                    var returnUrl = Request["ReturnUrl"];
                    if (!string.IsNullOrEmpty(returnUrl))
                    {
                        viewModel.ReturnUrl = returnUrl;
                    }
                    return View(viewModel);
                }
            }
            else
            {
                return RedirectToAction("Index", "Home");   // 调到首页
            }
        }

        /// <summary>
        /// 用户注册
        /// </summary>
        /// <param name="userModel"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(MemberAddViewModel userModel)
        {
            if (ModelState.IsValid)
            {
                Settings settings = SettingsService.GetSettings();
                if (settings.SuspendRegistration != true && settings.DisableStandardRegistration != true)
                {
                    #region 防注册机进行垃圾注册
                    using (UnitOfWorkManager.NewUnitOfWork())
                    {
                        if (!string.IsNullOrEmpty(settings.SpamQuestion))
                        {
                            // There is a spam question, if answer is wrong return with error
                            if (userModel.SpamAnswer == null || userModel.SpamAnswer.Trim() != settings.SpamAnswer)
                            {
                                ModelState.AddModelError(string.Empty, LocalizationService.GetResourceString("Error.WrongAnswerRegistration"));
                                return View();
                            }
                        }
                    }
                    #endregion

                    userModel.LoginType = LoginType.Standard;  // 爱驴网本地登录模式（非社会化工具登录）

                    return MemberRegisterLogic(userModel);
                }
                return RedirectToAction("Index", "Home");
            }
            return View(); //ModelState无效
        }

        /// <summary>
        /// 用户注册的执行逻辑
        /// </summary>
        /// <param name="userModel"></param>
        /// <returns></returns>
        public ActionResult MemberRegisterLogic(MemberAddViewModel userModel)
        {
            using (var unitOfWork = UnitOfWorkManager.NewUnitOfWork())
            {
                var settings = SettingsService.GetSettings();
                var manuallyAuthoriseMembers = settings.ManuallyAuthoriseNewMembers;
                var memberEmailAuthorisationNeeded = settings.NewMemberEmailConfirmation == true;
                var homeRedirect = false;

                var userToSave = new MembershipUser();
                userToSave.UserName = _bannedWordService.SanitiseBannedWords(userModel.UserName);
                userToSave.AliasName = userModel.AliasName.Trim();
                userToSave.RealName = userModel.RealName.Trim();
                userToSave.MobilePhone = userModel.MobilePhone.Trim();
                userToSave.Password = userModel.Password.Trim();
                userToSave.CreateDate = DateTime.Now;
                userToSave.LastLoginDate = DateTime.Now;
                userToSave.LastUpdateTime = DateTime.Now;
                userToSave.UserType = Enum_UserType.A;
                userToSave.Comment = userModel.Comment;

                var createStatus = MembershipService.CreateUser(userToSave);
                if (createStatus != MembershipCreateStatus.Success)
                {
                    ModelState.AddModelError(string.Empty, MembershipService.ErrorCodeToString(createStatus));
                }
                else //用户注册成功
                {
                    loggerForCoreAction.Info(string.Format("新注册用户{0}成功,真实姓名{1},电话{2}.", userToSave.UserName, userToSave.RealName, userToSave.MobilePhone));

                    //将等待验证（Waiting）的验证码的状态改为验证完成
                    VerifyCode mVerifyCode = new VerifyCode(userModel.MobilePhone.Trim(), VerifyCodeStatus.Waiting, userModel.VerifyCode);
                    new VerifyCodeService().CompleteVerifyCode(mVerifyCode);
                    loggerForCoreAction.Info("注册时完成验证码的验证。用户手机号：" + userModel.MobilePhone.Trim());

                    // See if this is a social login and we have their profile pic
                    #region 处理SocialProfileImageUrl
                    if (!string.IsNullOrEmpty(userModel.SocialProfileImageUrl))
                    {
                        // We have an image url - Need to save it to their profile
                        var image = AppHelpers.GetImageFromExternalUrl(userModel.SocialProfileImageUrl);

                        // Set upload directory - Create if it doesn't exist
                        var uploadFolderPath = HostingEnvironment.MapPath(string.Concat(SiteConstants.Instance.UploadFolderPath, userToSave.Id));
                        if (uploadFolderPath != null && !Directory.Exists(uploadFolderPath))
                        {
                            Directory.CreateDirectory(uploadFolderPath);
                        }

                        // Get the file name
                        var fileName = Path.GetFileName(userModel.SocialProfileImageUrl);

                        // Create a HttpPostedFileBase image from the C# Image
                        using (var stream = new MemoryStream())
                        {
                            // Microsoft doesn't give you a file extension - See if it has a file extension
                            // Get the file extension
                            var fileExtension = Path.GetExtension(fileName);

                            // Fix invalid Illegal charactors
                            var regexSearch = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());
                            var reg = new Regex($"[{Regex.Escape(regexSearch)}]");
                            fileName = reg.Replace(fileName, "");

                            if (string.IsNullOrEmpty(fileExtension))
                            {
                                // no file extension so give it one
                                fileName = string.Concat(fileName, ".jpg");
                            }

                            image.Save(stream, ImageFormat.Jpeg);
                            stream.Position = 0;
                            HttpPostedFileBase formattedImage = new MemoryFile(stream, "image/jpeg", fileName);

                            // Upload the file
                            var uploadResult = AppHelpers.UploadFile(formattedImage, uploadFolderPath, LocalizationService, true);

                            // Don't throw error if problem saving avatar, just don't save it.
                            if (uploadResult.UploadSuccessful)
                            {
                                userToSave.Avatar = uploadResult.UploadedFileName;
                            }
                        }

                    }
                    #endregion

                    #region 处理SocialLogin

                    // Store access token for social media account in case we want to do anything with it
                    var isSocialLogin = false;
                    if (userModel.LoginType == LoginType.Facebook)
                    {
                        userToSave.FacebookAccessToken = userModel.UserAccessToken;
                        isSocialLogin = true;
                    }
                    if (userModel.LoginType == LoginType.Google)
                    {
                        userToSave.GoogleAccessToken = userModel.UserAccessToken;
                        isSocialLogin = true;
                    }
                    if (userModel.LoginType == LoginType.Microsoft)
                    {
                        userToSave.MicrosoftAccessToken = userModel.UserAccessToken;
                        isSocialLogin = true;
                    }

                    // If this is a social login, and memberEmailAuthorisationNeeded is true then we need to ignore it
                    // and set memberEmailAuthorisationNeeded to false because the email addresses are validated by the social media providers
                    if (isSocialLogin && !manuallyAuthoriseMembers)
                    {
                        memberEmailAuthorisationNeeded = false;
                        userToSave.IsApproved = true;
                    }

                    #endregion

                    // Set the view bag message here
                    SetRegisterViewBagMessage(manuallyAuthoriseMembers, memberEmailAuthorisationNeeded, userToSave);

                    if (!manuallyAuthoriseMembers && !memberEmailAuthorisationNeeded)
                    {
                        homeRedirect = true;
                    }

                    try
                    {
                        //// Only send the email if the admin is not manually authorising emails or it's pointless
                        //SendEmailConfirmationEmail(userToSave);

                        unitOfWork.Commit();

                        if (homeRedirect)
                        {
                            if (Url.IsLocalUrl(userModel.ReturnUrl) && userModel.ReturnUrl.Length > 1 && userModel.ReturnUrl.StartsWith("/")
                            && !userModel.ReturnUrl.StartsWith("//") && !userModel.ReturnUrl.StartsWith("/\\"))
                            {
                                return Redirect(userModel.ReturnUrl);
                            }
                            return RedirectToAction("Index", "Home", new { area = string.Empty });
                        }
                        else
                        {
                            return RedirectToAction("Edit", "Members", new { area = string.Empty, Id = MembershipService.GetUser(userToSave.UserName).Id });
                        }
                    }
                    catch (Exception ex)
                    {
                        unitOfWork.Rollback();
                        LoggingService.Error(ex);
                        FormsAuthentication.SignOut();
                        ModelState.AddModelError(string.Empty, LocalizationService.GetResourceString("Errors.GenericMessage"));
                    }
                }
            }

            return View("Register");
        }

        private void SetRegisterViewBagMessage(bool manuallyAuthoriseMembers, bool memberEmailAuthorisationNeeded, MembershipUser userToSave)
        {
            if (manuallyAuthoriseMembers)
            {
                TempData[AppConstants.MessageViewBagName] = new GenericMessageViewModel
                {
                    Message = LocalizationService.GetResourceString("Members.NowRegisteredNeedApproval"),
                    MessageType = GenericMessages.success
                };
            }
            else if (memberEmailAuthorisationNeeded)
            {
                TempData[AppConstants.MessageViewBagName] = new GenericMessageViewModel
                {
                    Message = LocalizationService.GetResourceString("Members.MemberEmailAuthorisationNeeded"),
                    MessageType = GenericMessages.success
                };
            }
            else
            {
                FormsAuthentication.SetAuthCookie(userToSave.UserName, false);
                TempData[AppConstants.MessageViewBagName] = new GenericMessageViewModel
                {
                    Message = "欢迎你，" + userToSave.AliasName,
                    MessageType = GenericMessages.success
                };
            }
        }

        public ActionResult SocialLoginValidator()
        {
            if (TempData[AppConstants.MemberRegisterViewModel] != null)
            {
                var userModel = (TempData[AppConstants.MemberRegisterViewModel] as MemberAddViewModel);
                return MemberRegisterLogic(userModel);
            }
            ModelState.AddModelError(string.Empty, LocalizationService.GetResourceString("Errors.GenericMessage"));
            return View("Register");
        }

        #endregion

        #region 用户编辑

        [Authorize]
        public ActionResult Edit(Guid id)
        {
            using (UnitOfWorkManager.NewUnitOfWork())
            {
                var loggedOnUserId = LoggedOnReadOnlyUser?.Id ?? Guid.Empty;

                var permissions = RoleService.GetPermissions(null, UsersRole);

                // Check is has permissions
                if (UserIsAdmin || loggedOnUserId == id || permissions[SiteConstants.Instance.PermissionEditMembers].IsTicked)
                {
                    var user = MembershipService.GetUser(id);
                    var viewModel = PopulateMemberViewModel(user);

                    BindControlData(user);

                    return View(viewModel);
                }

                return ErrorToHomePage(LocalizationService.GetResourceString("Errors.NoPermission"));
            }
        }

        private void BindControlData(MembershipUser user)
        {
            #region 绑定性别信息

            var Items_Gender = new List<SelectListItem>();
            Items_Gender.Add(new SelectListItem { Text = "男", Value = "1" });
            Items_Gender.Add(new SelectListItem { Text = "女", Value = "2" });

            foreach (SelectListItem item in Items_Gender)
            {
                if (item.Value == user.Gender.ToString())
                {
                    item.Selected = true;
                }
            }
            ViewData["GenderList"] = Items_Gender;

            #endregion

            #region 绑定婚姻状况信息

            var Items_Married = new List<SelectListItem>();
            Items_Married.Add(new SelectListItem { Text = "已婚", Value = "1" });
            Items_Married.Add(new SelectListItem { Text = "未婚", Value = "2" });

            foreach (SelectListItem item in Items_Married)
            {
                if (item.Value == user.IsMarried.ToString())
                {
                    item.Selected = true;
                }
            }
            ViewData["Married"] = Items_Married;

            #endregion

            #region 绑定历法信息

            var Items_Calendar = new List<SelectListItem>();
            Items_Calendar.Add(new SelectListItem { Text = "公历", Value = "2" });
            Items_Calendar.Add(new SelectListItem { Text = "阴历", Value = "1" });

            foreach (SelectListItem item in Items_Calendar)
            {
                if (item.Value == user.IsLunarCalendar.ToString())
                {
                    item.Selected = true;
                }
            }
            ViewData["LunarCalendar"] = Items_Calendar;

            #endregion

            #region 绑定学历信息
            var Items_Education = new List<SelectListItem>();
            Items_Education.AddRange(TEducation.LoadAllEducationList().Select(x =>
            {
                return new SelectListItem { Text = x.EducationName, Value = x.EducationId };
            }));

            foreach (SelectListItem item in Items_Education)
            {
                if (item.Value == user.Education.ToString())
                {
                    item.Selected = true;
                }
            }
            ViewData["EducationList"] = Items_Education;

            #endregion

            #region 绑定收入信息

            var Items_IncomeRange = new List<SelectListItem>();

            Items_IncomeRange.AddRange(TIncomeRange.LoadAllIncomeList().Select(x =>
            {
                return new SelectListItem { Text = x.IncomeRangeName, Value = x.IncomeRangeId };
            }));
            foreach (SelectListItem item in Items_IncomeRange)
            {
                if (item.Value == user.IncomeRange.ToString())
                {
                    item.Selected = true;
                }
            }
            ViewData["IncomeRangeList"] = Items_IncomeRange;

            #endregion

            #region 绑定毕业学校所属省信息
            var Items_SchoolProvince = new List<SelectListItem>();
            List<TProvince> SchoolProvincelst = TProvince.LoadAllProvinceList();
            foreach (var item in SchoolProvincelst)
            {
                if (user.SchoolProvince == item.ProvinceId.ToString())
                {
                    Items_SchoolProvince.Add(new SelectListItem { Text = item.ProvinceName, Value = item.ProvinceId.ToString(), Selected = true });
                }
                else
                {
                    Items_SchoolProvince.Add(new SelectListItem { Text = item.ProvinceName, Value = item.ProvinceId.ToString() });
                }
            }
            ViewData["SchoolProvinceList"] = Items_SchoolProvince;

            #endregion

            #region 绑定毕业学校所属市信息
            var Items_SchoolCity = new List<SelectListItem>();
            List<TCity> SchoolCitylst = TCity.LoadCityListByProvince(Convert.ToInt32(user.SchoolProvince));
            foreach (var item in SchoolCitylst)
            {
                if (user.SchoolCity == item.CityId.ToString())
                {
                    Items_SchoolCity.Add(new SelectListItem { Text = item.CityName, Value = item.CityId.ToString(), Selected = true });
                }
                else
                {
                    Items_SchoolCity.Add(new SelectListItem { Text = item.CityName, Value = item.CityId.ToString() });
                }
            }
            ViewData["SchoolCityList"] = Items_SchoolCity;
            #endregion

            #region 绑定居住地所属省信息

            var Items_HomeTownProvince = new List<SelectListItem>();
            List<TProvince> HomeTownProvincelst = TProvince.LoadAllProvinceList();
            foreach (var item in HomeTownProvincelst)
            {
                if (user.LocationProvince == item.ProvinceId.ToString())
                {
                    Items_HomeTownProvince.Add(new SelectListItem { Text = item.ProvinceName, Value = item.ProvinceId.ToString(), Selected = true });
                }
                else
                {
                    Items_HomeTownProvince.Add(new SelectListItem { Text = item.ProvinceName, Value = item.ProvinceId.ToString() });
                }
            }
            ViewData["HomeTownProvinceList"] = Items_HomeTownProvince;

            #endregion

            #region 绑定居住地所属市信息

            var Items_HomeTownCity = new List<SelectListItem>();
            List<TCity> HomeTownCitylst = TCity.LoadCityListByProvince(Convert.ToInt32(user.LocationProvince));
            foreach (var item in HomeTownCitylst)
            {
                if (user.LocationCity == item.CityId.ToString())
                {
                    Items_HomeTownCity.Add(new SelectListItem { Text = item.CityName, Value = item.CityId.ToString(), Selected = true });
                }
                else
                {
                    Items_HomeTownCity.Add(new SelectListItem { Text = item.CityName, Value = item.CityId.ToString() });
                }
            }

            ViewData["HomeTownCityList"] = Items_HomeTownCity;

            #endregion

            #region 绑定居住地所属县信息


            var Items_HomeTownCounty = new List<SelectListItem>();
            List<TCountry> HomeTownCountylst = TCountry.LoadCountryByProvinceAndCity(Convert.ToInt32(user.LocationProvince), Convert.ToInt32(user.LocationCity));
            foreach (var item in HomeTownCountylst)
            {
                if (user.LocationCounty == item.CountryId.ToString())
                {
                    Items_HomeTownCounty.Add(new SelectListItem { Text = item.CountryName, Value = item.CountryId.ToString(), Selected = true });
                }
                else
                {
                    Items_HomeTownCounty.Add(new SelectListItem { Text = item.CountryName, Value = item.CountryId.ToString() });
                }
            }
            ViewData["HomeTownCountyList"] = Items_HomeTownCounty;

            #endregion

            #region 绑定审核意见信息

            if (string.IsNullOrEmpty(user.AuditComments)) user.AuditComments = "";

            var Items_AuditComments = new List<SelectListItem>();
            Items_AuditComments.Add(new SelectListItem { Text = "", Value = "" });
            Items_AuditComments.Add(new SelectListItem { Text = "审核通过", Value = "审核通过" });
            Items_AuditComments.Add(new SelectListItem { Text = "驳回，姓名信息不合规", Value = "驳回，姓名信息不合规" });
            Items_AuditComments.Add(new SelectListItem { Text = "驳回，出生日期不合规", Value = "驳回，出生日期不合规" });
            Items_AuditComments.Add(new SelectListItem { Text = "驳回，身高信息不合规", Value = "驳回，身高信息不合规" });
            Items_AuditComments.Add(new SelectListItem { Text = "驳回，体重信息不合规", Value = "驳回，体重信息不合规" });
            Items_AuditComments.Add(new SelectListItem { Text = "驳回，学校名称不合规", Value = "驳回，学校名称不合规" });
            Items_AuditComments.Add(new SelectListItem { Text = "驳回，家乡信息不合规", Value = "驳回，家乡信息不合规" });
            Items_AuditComments.Add(new SelectListItem { Text = "驳回，职业信息不合规", Value = "驳回，职业信息不合规" });
            Items_AuditComments.Add(new SelectListItem { Text = "驳回，兴趣爱好不合规", Value = "驳回，兴趣爱好不合规" });
            Items_AuditComments.Add(new SelectListItem { Text = "驳回，头像内容不合规", Value = "驳回，头像内容不合规" });
            Items_AuditComments.Add(new SelectListItem { Text = "驳回，个人图片内容不合规", Value = "驳回，个人图片内容不合规" });

            foreach (SelectListItem item in Items_AuditComments)
            {
                if (item.Value == user.AuditComments)
                {
                    item.Selected = true;
                }
            }
            ViewData["AuditCommentList"] = Items_AuditComments;

            #endregion

        }

        [HttpPost]
        [Authorize]
        public ActionResult Edit(MemberFrontEndEditViewModel userModel)
        {
            if (ModelState.IsValid)
            {
                #region 基本信息验证

                if (userModel.Height == 0)
                {
                    ShowMessage(new GenericMessageViewModel
                    {
                        Message = "请填写您的身高信息。",
                        MessageType = GenericMessages.danger
                    });
                    return Edit(userModel.Id);
                }

                if (userModel.Weight == 0)
                {
                    ShowMessage(new GenericMessageViewModel
                    {
                        Message = "请填写您的体重信息。",
                        MessageType = GenericMessages.danger
                    });
                    return Edit(userModel.Id);
                }

                if (userModel.LocationProvince == "0" || userModel.LocationCity == "0" || userModel.LocationCounty == "0")
                {
                    ShowMessage(new GenericMessageViewModel
                    {
                        //Message = LocalizationService.GetResourceString("Member.ProfileUpdated"),
                        Message = "请选择您的居住地信息。",
                        MessageType = GenericMessages.danger
                    });
                    return Edit(userModel.Id);
                }

                if (userModel.SchoolProvince == "0" || userModel.SchoolCity == "0")
                {
                    ShowMessage(new GenericMessageViewModel
                    {
                        //Message = LocalizationService.GetResourceString("Member.ProfileUpdated"),
                        Message = "请选择您的学校所在地信息。",
                        MessageType = GenericMessages.danger
                    });
                    return Edit(userModel.Id);
                }

                #endregion

                using (var unitOfWork = UnitOfWorkManager.NewUnitOfWork())
                {
                    var loggedOnUserId = LoggedOnReadOnlyUser?.Id ?? Guid.Empty;
                    var permissions = RoleService.GetPermissions(null, UsersRole);

                    // Check is has permissions
                    if (UserIsAdmin || loggedOnUserId == userModel.Id || permissions[SiteConstants.Instance.PermissionEditMembers].IsTicked)
                    {
                        // Get the user from DB
                        var user = MembershipService.GetUser(userModel.Id);

                        #region 对用户输入的信息进行停用词检查

                        // Before we do anything - Check stop words
                        var stopWords = _bannedWordService.GetAll(true);
                        var bannedWords = _bannedWordService.GetAll().Select(x => x.Word).ToList();

                        // Check the fields for bad words
                        foreach (var stopWord in stopWords)
                        {
                            if ((userModel.SchoolName != null && userModel.SchoolName.IndexOf(stopWord.Word, StringComparison.CurrentCultureIgnoreCase) >= 0) ||
                                (userModel.HomeTown != null && userModel.HomeTown.IndexOf(stopWord.Word, StringComparison.CurrentCultureIgnoreCase) >= 0) ||
                                (userModel.Signature != null && userModel.Signature.IndexOf(stopWord.Word, StringComparison.CurrentCultureIgnoreCase) >= 0) ||
                                (userModel.Job != null && userModel.Job.IndexOf(stopWord.Word, StringComparison.CurrentCultureIgnoreCase) >= 0) ||
                                (userModel.Interest != null && userModel.Interest.IndexOf(stopWord.Word, StringComparison.CurrentCultureIgnoreCase) >= 0))
                            {
                                ShowMessage(new GenericMessageViewModel
                                {
                                    Message = LocalizationService.GetResourceString("StopWord.Error"),
                                    MessageType = GenericMessages.danger
                                });

                                // Ahhh found a stop word. Abandon operation captain.
                                return View(userModel);

                            }
                        }

                        user.SchoolName = _bannedWordService.SanitiseBannedWords(userModel.SchoolName, bannedWords);
                        user.HomeTown = _bannedWordService.SanitiseBannedWords(userModel.HomeTown, bannedWords);
                        user.Signature = _bannedWordService.SanitiseBannedWords(StringUtils.ScrubHtml(userModel.Signature, true), bannedWords);
                        user.Job = _bannedWordService.SanitiseBannedWords(userModel.Job, bannedWords);
                        user.Interest = _bannedWordService.SanitiseBannedWords(userModel.Interest, bannedWords);

                        #endregion

                        #region 基本字段更新

                        user.RealName = userModel.RealName;
                        user.Gender = userModel.Gender;
                        user.Birthday = userModel.Birthday;
                        user.IsLunarCalendar = userModel.IsLunarCalendar;
                        user.IsMarried = userModel.IsMarried;
                        user.Height = userModel.Height;
                        user.Weight = userModel.Weight;
                        user.Education = userModel.Education;
                        user.SchoolProvince = userModel.SchoolProvince;
                        user.SchoolCity = userModel.SchoolCity;
                        user.LocationProvince = userModel.LocationProvince;
                        user.LocationCity = userModel.LocationCity;
                        user.LocationCounty = userModel.LocationCounty;
                        user.IncomeRange = userModel.IncomeRange;
                        user.MobilePhone = userModel.MobilePhone;

                        #endregion

                        #region 上传头像

                        if (userModel.Files != null)
                        {
                            // Before we save anything, check the user already has an upload folder and if not create one
                            var uploadFolderPath = HostingEnvironment.MapPath(string.Concat(SiteConstants.Instance.UploadFolderPath, LoggedOnReadOnlyUser.Id));
                            if (!Directory.Exists(uploadFolderPath))
                            {
                                Directory.CreateDirectory(uploadFolderPath);
                            }

                            // Loop through each file and get the file info and save to the users folder and Db
                            var file = userModel.Files[0];
                            if (file != null)
                            {
                                // If successful then upload the file
                                var uploadResult = AppHelpers.UploadFile(file, uploadFolderPath, LocalizationService, true);

                                if (!uploadResult.UploadSuccessful)
                                {
                                    TempData[AppConstants.MessageViewBagName] = new GenericMessageViewModel
                                    {
                                        Message = uploadResult.ErrorMessage,
                                        MessageType = GenericMessages.danger
                                    };
                                    return View(userModel);
                                }

                                // Save avatar to user
                                user.Avatar = uploadResult.UploadedFileName;
                            }
                        }

                        // Set the users Avatar for the confirmation page
                        userModel.Avatar = user.Avatar;

                        #endregion

                        #region 判断用户账号有无更新

                        // User is trying to change username, need to check if a user already exists
                        // with the username they are trying to change to
                        var changedUsername = false;
                        var sanitisedUsername = _bannedWordService.SanitiseBannedWords(userModel.UserName, bannedWords);
                        if (sanitisedUsername != user.UserName)
                        {
                            if (MembershipService.GetUser(sanitisedUsername) != null)
                            {
                                unitOfWork.Rollback();
                                ModelState.AddModelError(string.Empty, LocalizationService.GetResourceString("Members.Errors.DuplicateUserName"));
                                return View(userModel);
                            }
                            loggerForCoreAction.Info("用户名在更新中发生变更，旧的用户名为：" + user.UserName + ",新的用户名为：" + sanitisedUsername);
                            user.UserName = sanitisedUsername;
                            changedUsername = true;
                        }

                        #endregion

                        #region 判断用户的邮箱是否有变更（已禁用）

                        //// User is trying to update their email address, need to 
                        //// check the email is not already in use
                        //if (userModel.Email != user.Email)
                        //{
                        //    // Add get by email address
                        //    if (MembershipService.GetUserByEmail(userModel.Email) != null)
                        //    {
                        //        unitOfWork.Rollback();
                        //        ModelState.AddModelError(string.Empty, LocalizationService.GetResourceString("Members.Errors.DuplicateEmail"));
                        //        return View(userModel);
                        //    }
                        //    user.Email = userModel.Email;
                        //}

                        #endregion

                        #region 重置审核标志位

                        string messagestr = "";
                        if (!user.Roles.Any(x => x.RoleName.Contains(AppConstants.AdminRoleName) || x.RoleName.Contains(AppConstants.SupplierRoleName)))
                        {
                            // 按设计需求的规定，注册会员的每次改动都将审核标志位置false, 再由管理员手动审核
                            user.IsApproved = false;
                            user.AuditComments = "";
                            messagestr = "用户信息已更新，等待管理员审核。";
                        }
                        else
                        {
                            user.IsApproved = true;  //管理员和供应商无需审核
                            user.FinishedFirstAudit = "FinishedFirstAudit";
                            messagestr = "用户信息已更新。";
                        }

                        #endregion

                        MembershipService.ProfileUpdated(user); // 触发用户Profile更新事件，并将此事件记录到ProfileUpdatedActivity

                        #region Show Message

                        ShowMessage(new GenericMessageViewModel
                        {
                            //Message = LocalizationService.GetResourceString("Member.ProfileUpdated"),
                            Message = messagestr,
                            MessageType = GenericMessages.success
                        });

                        #endregion

                        try
                        {
                            unitOfWork.Commit();

                            #region 变更过程中变更了账号（用户名），需退出后重新登录
                            if (changedUsername)
                            {
                                // User has changed their username so need to log them in
                                // as there new username of 
                                var authCookie = Request.Cookies[FormsAuthentication.FormsCookieName];
                                if (authCookie != null)
                                {
                                    var authTicket = FormsAuthentication.Decrypt(authCookie.Value);
                                    if (authTicket != null)
                                    {
                                        var newFormsIdentity = new FormsIdentity(new FormsAuthenticationTicket(authTicket.Version,
                                                                                                               user.UserName,
                                                                                                               authTicket.IssueDate,
                                                                                                               authTicket.Expiration,
                                                                                                               authTicket.IsPersistent,
                                                                                                               authTicket.UserData));
                                        var roles = authTicket.UserData.Split("|".ToCharArray());
                                        var newGenericPrincipal = new GenericPrincipal(newFormsIdentity, roles);
                                        System.Web.HttpContext.Current.User = newGenericPrincipal;
                                    }
                                }

                                // sign out current user
                                FormsAuthentication.SignOut();

                                // Abandon the session
                                Session.Abandon();

                                // Sign in new user
                                FormsAuthentication.SetAuthCookie(user.UserName, false);
                            }
                            #endregion

                            loggerForCoreAction.Info(string.Format("用户{0}更新了个人信息。", userModel.UserName));

                            return RedirectToAction("Index", "Home");
                        }
                        catch (Exception ex)
                        {
                            unitOfWork.Rollback();
                            LoggingService.Error(ex);
                            ModelState.AddModelError(string.Empty, LocalizationService.GetResourceString("Errors.GenericMessage"));
                        }

                        BindControlData(user);
                        return View(userModel);
                    }

                    return ErrorToHomePage(LocalizationService.GetResourceString("Errors.NoPermission"));
                }
            }
            else
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors);
                return Edit(userModel.Id);
            }
        }

        private MemberFrontEndEditViewModel PopulateMemberViewModel(MembershipUser user)
        {
            var MembershipUserPictureslist = _MembershipUserPictureService.GetMembershipUserPictureListByUserId(user.Id);
            var viewModel = new MemberFrontEndEditViewModel
            {
                Id = user.Id,
                UserName = user.UserName,
                RealName = user.RealName,
                AliasName = user.AliasName,
                Gender = user.Gender,
                Birthday = user.Birthday,
                IsLunarCalendar = user.IsLunarCalendar,
                IsMarried = user.IsMarried,
                Height = user.Height,
                Weight = user.Weight,
                Education = user.Education,
                HomeTown = user.HomeTown,
                SchoolProvince = user.SchoolProvince,
                SchoolCity = user.SchoolCity,
                SchoolName = user.SchoolName,
                LocationProvince = user.LocationProvince,
                LocationCity = user.LocationCity,
                LocationCounty = user.LocationCounty,
                Job = user.Job,
                IncomeRange = user.IncomeRange,
                Interest = user.Interest,
                MobilePhone = user.MobilePhone,
                Signature = user.Signature,
                Avatar = user.Avatar,
                IsApproved = user.IsApproved,
                AuditComment = user.AuditComments,
                MembershipUserPictures = MembershipUserPictureslist,

            };
            return viewModel;
        }

        #endregion

        #region 个人图片管理

        [Authorize]
        public ActionResult CreatePrivatePicture()
        {
            return View(new PrivatePicture_CreateEdit_ViewModel());
        }

        [HttpPost]
        [Authorize]
        public ActionResult CreatePrivatePicture(PrivatePicture_CreateEdit_ViewModel adViewModel)
        {
            if (ModelState.IsValid)
            {
                var piclist = _MembershipUserPictureService.GetMembershipUserPictureListByUserId(LoggedOnReadOnlyUser.Id);
                if (piclist != null && piclist.Count >= 6)
                {
                    TempData[AppConstants.MessageViewBagName] = new GenericMessageViewModel
                    {
                        Message = "最多上传6张您的靓照。",
                        MessageType = GenericMessages.danger
                    };
                    return RedirectToAction("PrivatePictureList", "Members", new { Id = LoggedOnReadOnlyUser.Id });
                }
                else
                {
                    #region 新加个人照片

                    var ad = new MembershipUserPicture();
                    ad.UploadTime = DateTime.Now;
                    ad.UserId = LoggedOnReadOnlyUser.Id;
                    ad.Description = adViewModel.Description;
                    ad.AuditStatus = Enum_UploadPictureAuditStatus.WaitingAudit;
                    ad.AuditComment = "";
                    ad.AuditTime = DateTime.Now;

                    HttpPostedFileBase mUploadFile = Request.Files["files"];
                    if (mUploadFile != null) adViewModel.UploadFile = mUploadFile;
                    if (adViewModel.UploadFile != null)
                    {
                        #region 准备上传路径
                        var uploadFolderPath = HostingEnvironment.MapPath(string.Concat(SiteConstants.Instance.UploadFolderPath, LoggedOnReadOnlyUser.Id));
                        if (!Directory.Exists(uploadFolderPath))
                        {
                            Directory.CreateDirectory(uploadFolderPath);
                        }
                        #endregion

                        var uploadResult = AppHelpers.UploadFile(adViewModel.UploadFile, uploadFolderPath, LocalizationService);
                        if (!uploadResult.UploadSuccessful)
                        {
                            TempData[AppConstants.MessageViewBagName] = new GenericMessageViewModel
                            {
                                Message = uploadResult.ErrorMessage,
                                MessageType = GenericMessages.danger
                            };
                            return View(adViewModel);
                        }
                        ad.OriginFileName = adViewModel.UploadFileName;
                        ad.FileName = uploadResult.UploadedFileUrl;


                        using (var unitOfWork = UnitOfWorkManager.NewUnitOfWork())
                        {
                            _MembershipUserPictureService.Add(ad);
                            try
                            {
                                unitOfWork.Commit();
                                loggerForCoreAction.Info(string.Format("用户{0}新上传个人照片：{1}", LoggedOnReadOnlyUser.UserName, ad.FileName));

                                #region 重置审核标志位

                                if (!UserIsAdmin)
                                {
                                    var user = base.MembershipService.GetUser(LoggedOnReadOnlyUser.Id);
                                    user.AuditComments = "";
                                    user.IsApproved = false;
                                    _context.Entry<MembershipUser>(user).State = System.Data.Entity.EntityState.Modified;
                                    _context.SaveChanges();

                                    loggerForCoreAction.Info(string.Format("用户{0}因新上传个人照片：{1}导致其账户需要重新审核。", LoggedOnReadOnlyUser.UserName, ad.FileName));
                                }

                                #endregion
                            }
                            catch (Exception ex)
                            {
                                unitOfWork.Rollback();
                                loggerForCoreAction.Error(string.Format("用户{0}新上传个人照片失败。详细错误为：{1}", LoggedOnReadOnlyUser.UserName, ex.Message));
                            }
                        }
                    }
                    #endregion

                    TempData[AppConstants.MessageViewBagName] = new GenericMessageViewModel
                    {
                        Message = !UserIsAdmin ? "个人照片已上传，等待管理员审核。" : "个人照片已上传。",
                        MessageType = GenericMessages.info
                    };
                    return RedirectToAction("PrivatePictureList", "Members", new { Id = LoggedOnReadOnlyUser.Id });
                }
            }
            return View(adViewModel);
        }

        [Authorize]
        public ActionResult DeletePrivatePicture(Guid Id)
        {
            var ad = _MembershipUserPictureService.GetMembershipUserPicture(Id);
            try
            {
                string filename = ad.FileName;
                _MembershipUserPictureService.Delete(ad);
                _context.Entry<MembershipUserPicture>(ad).State = EntityState.Deleted;
                _context.SaveChanges();

                loggerForCoreAction.Info(string.Format("用户{0}删除个人照片：{1}", LoggedOnReadOnlyUser.UserName, filename));
            }
            catch (Exception ex)
            {
                //LoggingService.Error(ex);
                loggerForCoreAction.Error(string.Format("用户{0}删除个人照片失败。详细错误为：{1}", LoggedOnReadOnlyUser.UserName, ex.Message));
            }

            return RedirectToAction("PrivatePictureList", "Members", new { Id = LoggedOnReadOnlyUser.Id });
        }

        [Authorize]
        public ActionResult PrivatePictureList(Guid Id)
        {
            MemberFrontEndEditViewModel model = new MemberFrontEndEditViewModel();
            MembershipUser user = MembershipService.GetUser(Id);
            var MembershipUserPictureslist = _MembershipUserPictureService.GetMembershipUserPictureListByUserId(user.Id);
            model.UserName = user.UserName;
            model.MembershipUserPictures = MembershipUserPictureslist;
            return View(model);
        }

        #endregion

        #region 用户审核

        [Authorize]
        public ActionResult Audit(Guid id)
        {
            using (UnitOfWorkManager.NewUnitOfWork())
            {
                if (UserIsAdmin)
                {
                    var user = MembershipService.GetUser(id);
                    var viewModel = PopulateMemberViewModel(user);

                    #region 绑定审核意见信息

                    var Items_AuditComments = new List<SelectListItem>();
                    Items_AuditComments.Add(new SelectListItem { Text = "审核通过", Value = "审核通过" });
                    Items_AuditComments.Add(new SelectListItem { Text = "驳回，姓名信息不合规", Value = "驳回，姓名信息不合规" });
                    Items_AuditComments.Add(new SelectListItem { Text = "驳回，出生日期不合规", Value = "驳回，出生日期不合规" });
                    Items_AuditComments.Add(new SelectListItem { Text = "驳回，身高信息不合规", Value = "驳回，身高信息不合规" });
                    Items_AuditComments.Add(new SelectListItem { Text = "驳回，体重信息不合规", Value = "驳回，体重信息不合规" });
                    Items_AuditComments.Add(new SelectListItem { Text = "驳回，学校名称不合规", Value = "驳回，学校名称不合规" });
                    Items_AuditComments.Add(new SelectListItem { Text = "驳回，家乡信息不合规", Value = "驳回，家乡信息不合规" });
                    Items_AuditComments.Add(new SelectListItem { Text = "驳回，职业信息不合规", Value = "驳回，职业信息不合规" });
                    Items_AuditComments.Add(new SelectListItem { Text = "驳回，兴趣爱好不合规", Value = "驳回，兴趣爱好不合规" });
                    Items_AuditComments.Add(new SelectListItem { Text = "驳回，头像内容不合规", Value = "驳回，头像内容不合规" });
                    Items_AuditComments.Add(new SelectListItem { Text = "驳回，个人图片内容不合规", Value = "驳回，个人图片内容不合规" });

                    foreach (SelectListItem item in Items_AuditComments)
                    {
                        if (item.Value == user.AuditComments.ToString())
                        {
                            item.Selected = true;
                        }
                    }
                    ViewData["AuditCommentList"] = Items_AuditComments;

                    #endregion

                    return View(viewModel);
                }

                return ErrorToHomePage(LocalizationService.GetResourceString("Errors.NoPermission"));
            }
        }

        [HttpPost]
        [Authorize(Roles = AppConstants.AdminRoleName)]
        public ActionResult Audit(MemberFrontEndEditViewModel userModel)
        {
            if (string.IsNullOrEmpty(userModel.AuditComment) || userModel.Id == Guid.Empty)
            {
                TempData[AppConstants.MessageViewBagName] = new GenericMessageViewModel
                {
                    Message = "请选定审核意见",
                    MessageType = GenericMessages.danger
                };
                return RedirectToAction("Edit", "Members", new { Id = userModel.Id });
            }
            //此处不校验userModel实例中的其他属性
            using (var unitOfWork = UnitOfWorkManager.NewUnitOfWork())
            {
                var user = MembershipService.GetUser(userModel.Id);
                try
                {
                    user.AuditComments = userModel.AuditComment;
                    if (userModel.AuditComment.Contains("审核通过"))
                    {
                        user.IsApproved = true;
                        if (string.IsNullOrEmpty(user.FinishedFirstAudit))
                        {
                            user.FinishedFirstAudit = "FinishedFirstAudit";
                        }
                        // 审核通过照片
                        _MembershipUserPictureService.AuditMembershipUserPicture(user, "默认审核通过", Enum_UploadPictureAuditStatus.Auditted);
                    }
                    else
                    {
                        user.IsApproved = false;
                    }

                    _context.Entry<MembershipUser>(user).State = System.Data.Entity.EntityState.Modified;
                    unitOfWork.Commit();
                }
                catch (Exception ex)
                {
                    unitOfWork.Rollback();
                    LoggingService.Error(ex);
                    ModelState.AddModelError(string.Empty, LocalizationService.GetResourceString("Errors.GenericMessage"));
                }

                return RedirectToAction("Edit", "Members", new { Id = userModel.Id });
            }

        }

        #endregion

        #region 登录和登出

        /// <summary>
        /// 用户登录
        /// </summary>
        /// <returns></returns>
        public ActionResult LogOn()
        {
            // Create the empty view model
            var viewModel = new LogOnViewModel();

            // See if a return url is present or not and add it
            var returnUrl = Request["ReturnUrl"];
            if (!string.IsNullOrEmpty(returnUrl))
            {
                viewModel.ReturnUrl = returnUrl;
            }

            return View(viewModel);
        }

        /// <summary>
        /// 用户登录（post方式）
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOn(LogOnViewModel model)
        {
            using (var unitOfWork = UnitOfWorkManager.NewUnitOfWork())
            {
                #region 检查验证码
                if (Session["ValidateCode"].ToString() != model.VerifyCode)
                {
                    ModelState.AddModelError("code", "好像验证码输入错了。");
                    return View();
                }
                Session["ValidateCode"] = null;
                #endregion

                var username = model.UserName;
                var password = model.Password;
                try
                {
                    if (ModelState.IsValid)
                    {
                        // We have an event here to help with Single Sign Ons
                        // You can do manual lookups to check users based on a webservice and validate a user
                        // Then log them in if they exist or create them and log them in - Have passed in a UnitOfWork
                        // To allow database changes.

                        var e = new LoginEventArgs
                        {
                            UserName = model.UserName,
                            Password = model.Password,
                            RememberMe = model.RememberMe,
                            ReturnUrl = model.ReturnUrl,
                            UnitOfWork = unitOfWork
                        };
                        EventManager.Instance.FireBeforeLogin(this, e);

                        if (!e.Cancel)
                        {
                            var message = new GenericMessageViewModel();
                            var user = new MembershipUser();
                            if (MembershipService.ValidateUser(username, password, System.Web.Security.Membership.MaxInvalidPasswordAttempts))
                            {
                                user = MembershipService.GetUser(username);
                                bool checkresult = !user.IsLockedOut && !user.IsBanned;

                                //TODO: 【Benjamin】此处加入是否检查用户通过审核标志位
                                // checkresult = checkresult && user.IsApproved;

                                if (checkresult)
                                {
                                    FormsAuthentication.SetAuthCookie(username, model.RememberMe);
                                    user.LastLoginDate = DateTime.Now;   // Set last login date

                                    if (Url.IsLocalUrl(model.ReturnUrl) && model.ReturnUrl.Length > 1 && model.ReturnUrl.StartsWith("/")
                                        && !model.ReturnUrl.StartsWith("//") && !model.ReturnUrl.StartsWith("/\\"))
                                    {
                                        return Redirect(RedirectURL(model.ReturnUrl));
                                    }

                                    message.Message = LocalizationService.GetResourceString("Members.NowLoggedIn");
                                    message.MessageType = GenericMessages.success;

                                    EventManager.Instance.FireAfterLogin(this, new LoginEventArgs
                                    {
                                        UserName = model.UserName,
                                        Password = model.Password,
                                        RememberMe = model.RememberMe,
                                        ReturnUrl = model.ReturnUrl,
                                        UnitOfWork = unitOfWork
                                    });

                                    if (user.IsApproved)
                                    {
                                        return RedirectToAction("Index", "Home", new { area = string.Empty });
                                    }
                                    else
                                    {
                                        return RedirectToAction("Edit", "Members", new { area = string.Empty, Id = user.Id });
                                    }
                                }

                                #region 处理用户是否通过审核

                                //else if (!user.IsApproved && SettingsService.GetSettings().ManuallyAuthoriseNewMembers)
                                //{

                                //    message.Message = LocalizationService.GetResourceString("Members.NowRegisteredNeedApproval");
                                //    message.MessageType = GenericMessages.success;

                                //}
                                //else if (!user.IsApproved && SettingsService.GetSettings().NewMemberEmailConfirmation == true)
                                //{

                                //    message.Message = LocalizationService.GetResourceString("Members.MemberEmailAuthorisationNeeded");
                                //    message.MessageType = GenericMessages.success;
                                //}

                                #endregion
                            }

                            #region 登录后消息处理

                            // Only show if we have something to actually show to the user
                            if (!string.IsNullOrEmpty(message.Message))
                            {
                                TempData[AppConstants.MessageViewBagName] = message;
                            }
                            else
                            {
                                // get here Login failed, check the login status
                                var loginStatus = MembershipService.LastLoginStatus;

                                switch (loginStatus)
                                {
                                    case LoginAttemptStatus.UserNotFound:
                                    case LoginAttemptStatus.PasswordIncorrect:
                                        ModelState.AddModelError(string.Empty, LocalizationService.GetResourceString("Members.Errors.PasswordIncorrect"));
                                        break;

                                    case LoginAttemptStatus.PasswordAttemptsExceeded:
                                        ModelState.AddModelError(string.Empty, LocalizationService.GetResourceString("Members.Errors.PasswordAttemptsExceeded"));
                                        break;

                                    case LoginAttemptStatus.UserLockedOut:
                                        ModelState.AddModelError(string.Empty, LocalizationService.GetResourceString("Members.Errors.UserLockedOut"));
                                        break;

                                    case LoginAttemptStatus.Banned:
                                        ModelState.AddModelError(string.Empty, LocalizationService.GetResourceString("Members.NowBanned"));
                                        break;

                                    case LoginAttemptStatus.UserNotApproved:
                                        ModelState.AddModelError(string.Empty, LocalizationService.GetResourceString("Members.Errors.UserNotApproved"));
                                        user = MembershipService.GetUser(username);
                                        SendEmailConfirmationEmail(user);
                                        break;

                                    default:
                                        ModelState.AddModelError(string.Empty, LocalizationService.GetResourceString("Members.Errors.LogonGeneric"));
                                        break;
                                }
                            }

                            #endregion
                        }
                    }
                }

                finally
                {
                    try
                    {
                        unitOfWork.Commit();
                    }
                    catch (Exception ex)
                    {
                        unitOfWork.Rollback();
                        LoggingService.Error(ex);
                    }
                }
                return View(model);
            }
        }



        private string RedirectURL(string OriginalURL)
        {
            if (!string.IsNullOrEmpty(OriginalURL))
            {
                if (OriginalURL == "/ailvhuodong/createactivityregister/") { return "/ailvhuodong/ZuiXinHuoDong/"; }
                return OriginalURL;
            }
            return "/Home/Index";
        }

        /// <summary>
        /// Get: log off user
        /// </summary>
        /// <returns></returns>
        public ActionResult LogOff()
        {
            using (UnitOfWorkManager.NewUnitOfWork())
            {
                FormsAuthentication.SignOut();
                ViewBag.Message = new GenericMessageViewModel
                {
                    Message = LocalizationService.GetResourceString("Members.NowLoggedOut"),
                    MessageType = GenericMessages.success
                };
                return RedirectToAction("Index", "Home", new { area = string.Empty });
            }
        }

        /// <summary>
        /// 生成验证码
        /// </summary>
        /// <returns></returns>
        public ActionResult GetValidateCode()
        {
            ValidateCode vCode = new ValidateCode();
            string code = vCode.CreateValidateCode(5); //默认生成5位验证码
            Session["ValidateCode"] = code;
            byte[] bytes = vCode.CreateValidateGraphic(code);
            return File(bytes, @"image/jpeg");
        }

        #endregion

        #region 密码相关

        #region 修改密码

        [Authorize]
        public ActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult ChangePassword(ChangePasswordViewModel model)
        {
            var changePasswordSucceeded = true;
            using (var unitOfWork = UnitOfWorkManager.NewUnitOfWork())
            {
                if (ModelState.IsValid)
                {
                    // ChangePassword will throw an exception rather than return false in certain failure scenarios.
                    var loggedOnUser = MembershipService.GetUser(LoggedOnReadOnlyUser.Id);
                    changePasswordSucceeded = MembershipService.ChangePassword(loggedOnUser, model.OldPassword, model.NewPassword);

                    try
                    {
                        unitOfWork.Commit();

                        loggerForCoreAction.Info(string.Format("用户{0}更改了密码。", LoggedOnReadOnlyUser.UserName));
                    }
                    catch (Exception ex)
                    {
                        unitOfWork.Rollback();
                        //LoggingService.Error(ex);
                        loggerForCoreAction.Error(string.Format("用户{0}更改密码失败。详细信息为：{1}", LoggedOnReadOnlyUser.UserName, ex.Message));
                        changePasswordSucceeded = false;
                    }
                }
            }

            using (UnitOfWorkManager.NewUnitOfWork())
            {
                if (changePasswordSucceeded)
                {
                    TempData[AppConstants.MessageViewBagName] = new GenericMessageViewModel
                    {
                        Message = LocalizationService.GetResourceString("Members.ChangePassword.Success"),
                        MessageType = GenericMessages.success
                    };
                    return View();
                }

                ModelState.AddModelError("", LocalizationService.GetResourceString("Members.ChangePassword.Error"));
                return View(model);
            }

        }
        #endregion

        #region 忘记密码

        public ActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ForgotPassword(ForgotPasswordViewModel forgotPasswordViewModel)
        {
            if (!ModelState.IsValid)
            {
                return View(forgotPasswordViewModel);
            }

            if (Session["code"] == null && forgotPasswordViewModel.VerifyCode != "!@#$")
            {
                ShowMessage(new GenericMessageViewModel
                {
                    Message = "请先获取验证码。",
                    MessageType = GenericMessages.danger
                });
                return View(forgotPasswordViewModel);
            }

            MembershipUser user;
            using (var unitOfWork = UnitOfWorkManager.NewUnitOfWork())
            {
                user = MembershipService.GetUser(forgotPasswordViewModel.UserName);

                #region 参数校验

                if (user == null)
                {
                    ModelState.AddModelError("NoExistUser", "用户不存在。");
                    return View();
                }
                if (user.MobilePhone != forgotPasswordViewModel.Telphone)
                {
                    ShowMessage(new GenericMessageViewModel
                    {
                        Message = "用户手机号码不正确。",
                        MessageType = GenericMessages.danger
                    });
                    return View(forgotPasswordViewModel);
                }

                #endregion

                try
                {
                    // If the user is registered then create a security token and a timestamp that will allow a change of password
                    MembershipService.UpdatePasswordResetToken(user);
                    unitOfWork.Commit();

                    loggerForCoreAction.Info(string.Format("用户{0}完成UpdatePasswordResetToken操作。", user.UserName));
                }
                catch (Exception ex)
                {
                    unitOfWork.Rollback();
                    LoggingService.Error(ex);
                    ModelState.AddModelError("", LocalizationService.GetResourceString("Members.ResetPassword.Error"));
                    return View(forgotPasswordViewModel);
                }
            }

            var s = Url.Action("ResetPassword", "Members", new { user.Id, token = user.PasswordResetToken });

            #region 发送重置密码的邮件

            //// At this point the email address is registered and a security token has been created
            //// so send an email with instructions on how to change the password
            //using (var unitOfWork = UnitOfWorkManager.NewUnitOfWork())
            //{
            //    var sb = new StringBuilder();
            //    sb.AppendFormat("<p>{0}</p>", string.Format(LocalizationService.GetResourceString("Members.ResetPassword.EmailText"), settings.ForumName));
            //    sb.AppendFormat("<p><a href=\"{0}\">{0}</a></p>", url);

            //    var email = new Email
            //    {
            //        EmailTo = user.Email,
            //        NameTo = user.UserName,
            //        Subject = LocalizationService.GetResourceString("Members.ForgotPassword.Subject")
            //    };
            //    email.Body = _emailService.EmailTemplate(email.NameTo, sb.ToString());
            //    _emailService.SendMail(email);

            //    try
            //    {
            //        unitOfWork.Commit();
            //    }
            //    catch (Exception ex)
            //    {
            //        unitOfWork.Rollback();
            //        LoggingService.Error(ex);
            //        ModelState.AddModelError("", LocalizationService.GetResourceString("Members.ResetPassword.Error"));
            //        return View(forgotPasswordViewModel);
            //    }
            //}

            //return RedirectToAction("PasswordResetSent", "Members");

            #endregion

            return Redirect(s);

        }

        #endregion

        #region 重置密码

        [HttpGet]
        public ViewResult PasswordResetSent()
        {
            return View();
        }

        [HttpGet]
        public ViewResult ResetPassword(Guid? id, string token)
        {
            var model = new ResetPasswordViewModel
            {
                Id = id,
                Token = token
            };

            if (id == null || String.IsNullOrEmpty(token))
            {
                ModelState.AddModelError("", LocalizationService.GetResourceString("Members.ResetPassword.InvalidToken"));
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            using (var unitOfWork = UnitOfWorkManager.NewUnitOfWork())
            {
                if (model.Id != null)
                {
                    var user = MembershipService.GetUser(model.Id.Value);

                    // if the user id wasn't found then we can't proceed
                    // if the token submitted is not valid then do not proceed
                    if (user == null || user.PasswordResetToken == null || !MembershipService.IsPasswordResetTokenValid(user, model.Token))
                    {
                        ModelState.AddModelError("", LocalizationService.GetResourceString("Members.ResetPassword.InvalidToken"));
                        return View(model);
                    }

                    try
                    {
                        // The security token is valid so change the password
                        MembershipService.ResetPassword(user, model.NewPassword);
                        // Clear the token and the timestamp so that the URL cannot be used again
                        MembershipService.ClearPasswordResetToken(user);
                        unitOfWork.Commit();

                        loggerForCoreAction.Info(string.Format("用户{0}完成重置密码操作。", user.UserName));
                    }
                    catch (Exception ex)
                    {
                        unitOfWork.Rollback();
                        LoggingService.Error("重置密码失败，详细信息为：" + ex);
                        ModelState.AddModelError("", LocalizationService.GetResourceString("Members.ResetPassword.InvalidToken"));
                        return View(model);
                    }
                }
            }

            return RedirectToAction("PasswordChanged", "Members");
        }

        [HttpGet]
        public ViewResult PasswordChanged()
        {
            return View();
        }

        #endregion

        #endregion

        #region 爱驴会员

        public ActionResult MemberList()
        {
            int windowsize = 10;
            var viewModel = new MembersList_ViewModel();
            if (LoggedOnReadOnlyUser != null)
            {
                Guid UserId = LoggedOnReadOnlyUser.Id;
                var list = MembershipService.GetAll(true).Where(x => x.Gender != LoggedOnReadOnlyUser.Gender).ToList();
                if (list.Count >= windowsize)
                {
                    viewModel.UserList = new List<MembershipUser>();
                    int[] indexlist = AppHelpers.GetRandomSequence(0, list.Count - 1);
                    for (int i = 0; i < windowsize; i++)
                    {
                        int k = indexlist[i];

                        var user = list[k];
                        user.LocationProvince = TProvince.LoadProvinceByProvincedId(Convert.ToInt32(user.LocationProvince)).ProvinceName;
                        user.LocationCity = TCity.LoadCityByCityId(Convert.ToInt32(user.LocationCity)).CityName;
                        user.LocationCounty = TCountry.LoadCountryByCountryId(Convert.ToInt32(user.LocationCounty)).CountryName;

                        viewModel.UserList.Add(user);
                    }
                }
                else
                {
                    foreach (var user in list)
                    {
                        user.LocationProvince = TProvince.LoadProvinceByProvincedId(Convert.ToInt32(user.LocationProvince)).ProvinceName;
                        user.LocationCity = TCity.LoadCityByCityId(Convert.ToInt32(user.LocationCity)).CityName;
                        user.LocationCounty = TCountry.LoadCountryByCountryId(Convert.ToInt32(user.LocationCounty)).CountryName;
                    }
                    viewModel.UserList = list;
                }
            }
            else
            {
                var list = MembershipService.GetAll(true);
                if (list.Count >= windowsize)
                {
                    viewModel.UserList = new List<MembershipUser>();
                    int[] indexlist = AppHelpers.GetRandomSequence(0, list.Count - 1);
                    for (int i = 0; i < windowsize; i++)
                    {
                        int k = indexlist[i];
                        var user = list[k];
                        user.LocationProvince = TProvince.LoadProvinceByProvincedId(Convert.ToInt32(user.LocationProvince)).ProvinceName;
                        user.LocationCity = TCity.LoadCityByCityId(Convert.ToInt32(user.LocationCity)).CityName;
                        user.LocationCounty = TCountry.LoadCountryByCountryId(Convert.ToInt32(user.LocationCounty)).CountryName;
                        viewModel.UserList.Add(user);
                    }
                }
                else
                {
                    foreach (var user in list)
                    {
                        user.LocationProvince = TProvince.LoadProvinceByProvincedId(Convert.ToInt32(user.LocationProvince)).ProvinceName;
                        user.LocationCity = TCity.LoadCityByCityId(Convert.ToInt32(user.LocationCity)).CityName;
                        user.LocationCounty = TCountry.LoadCountryByCountryId(Convert.ToInt32(user.LocationCounty)).CountryName;
                    }
                    viewModel.UserList = list;
                }
            }
            return View(viewModel);
        }

        #endregion

        #region 找朋友

        public ActionResult Search(int? p, string search)
        {
            using (UnitOfWorkManager.NewUnitOfWork())
            {
                var pageIndex = p ?? 1;
                var allUsers = string.IsNullOrEmpty(search) ? MembershipService.GetAll(pageIndex, SiteConstants.Instance.AdminListPageSize) :
                                    MembershipService.SearchMembers(search, pageIndex, SiteConstants.Instance.AdminListPageSize);

                // Redisplay list of users
                var allViewModelUsers = allUsers.Select(user => new PublicSingleMemberListViewModel
                {
                    UserName = user.UserName,
                    NiceUrl = user.NiceUrl,
                    CreateDate = user.CreateDate,
                    TotalPoints = user.TotalPoints,
                }).ToList();

                var memberListModel = new PublicMemberListViewModel
                {
                    Users = allViewModelUsers,
                    PageIndex = pageIndex,
                    TotalCount = allUsers.TotalCount,
                    Search = search
                };

                return View(memberListModel);
            }
        }

        public ActionResult SearchEx()
        {
            var viewModel = new MembershipUserSearchModel();

            BindSearchExControlData();
            return View(viewModel);
        }

        private void BindSearchExControlData()
        {
            #region 绑定性别信息

            var Items_Gender = new List<SelectListItem>();

            Items_Gender.Add(new SelectListItem { Text = "男", Value = "1" });
            Items_Gender.Add(new SelectListItem { Text = "女", Value = "2" });
            ViewData["GenderList"] = Items_Gender;

            #endregion

            #region 绑定年龄阶段信息

            var Items_Age = new List<SelectListItem>();
            Items_Age.Add(new SelectListItem { Text = "小于20岁", Value = "1" });
            Items_Age.Add(new SelectListItem { Text = "20到25岁之间", Value = "2" });
            Items_Age.Add(new SelectListItem { Text = "25到30岁之间", Value = "3" });
            Items_Age.Add(new SelectListItem { Text = "30到35岁之间", Value = "4" });
            Items_Age.Add(new SelectListItem { Text = "35到40岁之间", Value = "5" });
            Items_Age.Add(new SelectListItem { Text = "40到50岁之间", Value = "6" });
            Items_Age.Add(new SelectListItem { Text = "50岁以上", Value = "7" });
            ViewData["AgeList"] = Items_Age;

            #endregion

            #region 绑定婚姻状况信息

            var Items_Married = new List<SelectListItem>();
            Items_Married.Add(new SelectListItem { Text = "已婚", Value = "1" });
            Items_Married.Add(new SelectListItem { Text = "未婚", Value = "2" });


            ViewData["MarriedList"] = Items_Married;

            #endregion

            #region 绑定学历信息
            var Items_Education = new List<SelectListItem>();
            Items_Education.AddRange(TEducation.LoadSearchEducationList().Select(x =>
            {
                return new SelectListItem { Text = x.EducationName, Value = x.EducationId };
            }));


            ViewData["EducationList"] = Items_Education;

            #endregion

            #region 绑定未登录天数信息
            var Items_NoLogindays = new List<SelectListItem>();
            var Items_NoLogindays_1 = new SelectListItem { Text = ">7天", Value = "1" };
            Items_NoLogindays.Add(Items_NoLogindays_1);
            var Items_NoLogindays_2 = new SelectListItem { Text = ">30天", Value = "2" };
            Items_NoLogindays.Add(Items_NoLogindays_2);
            var Items_NoLogindays_3 = new SelectListItem { Text = ">60天", Value = "3" };
            Items_NoLogindays.Add(Items_NoLogindays_3);
            ViewData["NoLoginDaysList"] = Items_NoLogindays;

            #endregion

            #region 绑定收入信息

            var Items_IncomeRange = new List<SelectListItem>();
            Items_IncomeRange.AddRange(TIncomeRange.LoadForSearchIncomeList().Select(x =>
            {
                return new SelectListItem { Text = x.IncomeRangeName, Value = x.IncomeRangeId };
            }));
            ViewData["IncomeRangeList"] = Items_IncomeRange;

            #endregion

            #region 绑定用户类型
            var Items_UserType = new List<SelectListItem>();

            Items_UserType.Add(new SelectListItem { Text = "A", Value = "1" });
            Items_UserType.Add(new SelectListItem { Text = "B", Value = "2" });
            Items_UserType.Add(new SelectListItem { Text = "C", Value = "3" });
            Items_UserType.Add(new SelectListItem { Text = "D", Value = "4" });
            Items_UserType.Add(new SelectListItem { Text = "E", Value = "5" });

            ViewData["UserTypeList"] = Items_UserType;
            #endregion

            #region 绑定用户状态
            var Items_UserStatus = new List<SelectListItem>();

            Items_UserStatus.Add(new SelectListItem { Text = "正常已审核的注册会员", Value = "1" });
            Items_UserStatus.Add(new SelectListItem { Text = "等待初次审核的注册会员", Value = "22" });
            Items_UserStatus.Add(new SelectListItem { Text = "等待再次审核的注册会员", Value = "2" });
            Items_UserStatus.Add(new SelectListItem { Text = "用户状态被锁定", Value = "3" });
            Items_UserStatus.Add(new SelectListItem { Text = "用户状态被禁用", Value = "4" });
            //Items_UserStatus.Add(new SelectListItem { Text = "用户在每日之星推广阶段", Value = "5" });

            ViewData["UserStatusList"] = Items_UserStatus;
            #endregion

            #region 绑定居住地所属省信息

            var Items_HomeTownProvince = new List<SelectListItem>();
            List<TProvince> HomeTownProvincelst = TProvince.LoadAllProvinceList();

            foreach (var item in HomeTownProvincelst)
            {
                //if (item.ProvinceId.ToString() == "440000") //广东省
                //{
                //    Items_HomeTownProvince.Add(new SelectListItem { Text = item.ProvinceName, Value = item.ProvinceId.ToString(), Selected = true });
                //}
                //else
                //{
                //    Items_HomeTownProvince.Add(new SelectListItem { Text = item.ProvinceName, Value = item.ProvinceId.ToString() });
                //}

                Items_HomeTownProvince.Add(new SelectListItem { Text = item.ProvinceName, Value = item.ProvinceId.ToString() });
            }

            ViewData["HomeTownProvinceList"] = Items_HomeTownProvince;

            #endregion

            #region 绑定居住地所属市信息

            var Items_HomeTownCity = new List<SelectListItem>();

            List<TCity> HomeTownCitylst = TCity.LoadAllCityList();

            //List<TCity> HomeTownCitylst = TCity.LoadCityListByProvince(Convert.ToInt32("440300")); //深圳市


            foreach (var item in HomeTownCitylst)
            {
                //if (item.CityId.ToString() == "440300")
                //{
                //    Items_HomeTownCity.Add(new SelectListItem { Text = item.CityName, Value = item.CityId.ToString(), Selected = true });
                //}
                //else
                //{
                //    Items_HomeTownCity.Add(new SelectListItem { Text = item.CityName, Value = item.CityId.ToString() });
                //}

                Items_HomeTownCity.Add(new SelectListItem { Text = item.CityName, Value = item.CityId.ToString() });
            }

            ViewData["HomeTownCityList"] = Items_HomeTownCity;

            #endregion

            #region 绑定居住地所属县信息


            var Items_HomeTownCounty = new List<SelectListItem>();

            List<TCountry> HomeTownCountylst = TCountry.LoadAllCountry();

            //List<TCountry> HomeTownCountylst = TCountry.LoadCountryByProvinceAndCity(Convert.ToInt32("440000"), Convert.ToInt32("440300"));

            foreach (var item in HomeTownCountylst)
            {
                Items_HomeTownCounty.Add(new SelectListItem { Text = item.CountryName, Value = item.CountryId.ToString() });
            }
            ViewData["HomeTownCountyList"] = Items_HomeTownCounty;

            #endregion

        }

        [HttpPost]
        [BasicMultiButton("btn_search")]
        public ActionResult SearchEx(MembershipUserSearchModel model)
        {
            //model.NoLoginDays = 0;
            Session["searchconfigurationmodel"] = model;
            return RedirectToAction("SearchResult", "Members");
        }

        [HttpGet]
        public ActionResult SearchResult(int? pageNum)
        {
            pageNum = pageNum ?? 0;
            ViewBag.IsEndOfRecords = false;
            if (Request.IsAjaxRequest())
            {
                var customers = GetRecordsForPage(pageNum.Value);
                ViewBag.IsEndOfRecords = (customers.Any()) && ((pageNum.Value * 20) >= customers.Last().Key);
                ViewBag.Customers = customers;
                return PartialView("_MemberUserList", customers);
            }
            else
            {
                LoadAllCustomersToSession();
                ViewBag.Customers = GetRecordsForPage(pageNum.Value);
                return View("SearchResult");
            }
        }

        public void LoadAllCustomersToSession()
        {
            MembershipUserSearchModel ConfigModel = Session["searchconfigurationmodel"] as MembershipUserSearchModel;
            var customers = User.IsInRole(AppConstants.AdminRoleName) ?
                MembershipService.SearchMembers(ConfigModel, 1000, true) :
                MembershipService.SearchMembers(ConfigModel, 1000, false);

            if (customers != null && customers.Count > 0)
            {
                foreach (var user in customers)
                {
                    user.LocationProvince = TProvince.LoadProvinceByProvincedId(Convert.ToInt32(user.LocationProvince)).ProvinceName;
                    user.LocationCity = TCity.LoadCityByCityId(Convert.ToInt32(user.LocationCity)).CityName;
                    user.LocationCounty = TCountry.LoadCountryByCountryId(Convert.ToInt32(user.LocationCounty)).CountryName;
                }
            }
            int custIndex = 1;
            Session["Customers"] = customers.ToDictionary(x => custIndex++, x => x);
            ViewBag.TotalNumberCustomers = customers.Count();
        }

        public Dictionary<int, MembershipUser> GetRecordsForPage(int pageNum)
        {
            Dictionary<int, MembershipUser> customers = (Session["Customers"] as Dictionary<int, MembershipUser>);

            int from = (pageNum * 20);
            int to = from + 20;

            return customers
                .Where(x => x.Key > from && x.Key <= to)
                .OrderBy(x => x.Key)
                .ToDictionary(x => x.Key, x => x.Value);
        }

        [Authorize]
        [BasicMultiButton("btn_Export")]
        public ActionResult ExportUsers()
        {
            using (UnitOfWorkManager.NewUnitOfWork())
            {
                if (Session["searchconfigurationmodel"] == null)
                {
                    _loggingService.Error("Session_searchconfigurationmodel为空。");
                    return Content("无法转换查询条件,请重新查询。");
                }
                else
                {

                }
                MembershipUserSearchModel ConfigModel = Session["searchconfigurationmodel"] as MembershipUserSearchModel;
                var customers = MembershipService.SearchMembers(ConfigModel, 1000).ToList();
                if (customers != null && customers.Count > 0)
                {
                    if (this.HttpContext.IsMobileDevice())
                    {
                        var csvcontent = MembershipService.ToCsv(customers, UserIsAdmin);
                        return Content(csvcontent);
                    }
                    else
                    {
                        var csvcontent = MembershipService.ToCsv(customers, UserIsAdmin);
                        byte[] fileContents = Encoding.UTF8.GetBytes(MembershipService.RemoveHTMLToCSV(csvcontent));
                        return File(fileContents, "application/vnd.ms-excel", "MVCForumUsers.csv");
                    }
                }
                else
                {
                    return Content("customers.Count=0");
                }
            }
        }
        #endregion

        #region 用户名片页

        /// <summary>
        /// 设定每日之星
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Authorize(Roles = AppConstants.AdminRoleName)]
        public ActionResult SubmitMeiRiZhiXing(ViewMemberViewModel model)
        {
            if (model != null && model.MeiRiZhiXing != null)
            {
                MembershipTodayStar info = _MembershipTodayStarService.Get(model.MeiRiZhiXing.UserId);
                if (info != null && info.UserId != Guid.Empty)
                {
                    info.JobId = model.MeiRiZhiXing.JobId;
                    info.Operator = LoggedOnReadOnlyUser.UserName;
                    info.StartTime = model.MeiRiZhiXing.StartTime;
                    info.StopTime = model.MeiRiZhiXing.StopTime;
                    info.Status = model.MeiRiZhiXing.Status;
                    _context.Entry<MembershipTodayStar>(info).State = System.Data.Entity.EntityState.Modified;
                    _context.SaveChanges();
                }
                else
                {
                    MembershipTodayStar newMembershipTodayStar = new MembershipTodayStar();
                    newMembershipTodayStar.UserId = model.MeiRiZhiXing.UserId;
                    newMembershipTodayStar.CreateDate = DateTime.Now;
                    newMembershipTodayStar.JobId = model.MeiRiZhiXing.JobId;
                    newMembershipTodayStar.Operator = LoggedOnReadOnlyUser.UserName;
                    newMembershipTodayStar.StartTime = model.MeiRiZhiXing.StartTime;
                    newMembershipTodayStar.StopTime = model.MeiRiZhiXing.StopTime;
                    newMembershipTodayStar.Status = model.MeiRiZhiXing.Status;
                    _MembershipTodayStarService.Add(newMembershipTodayStar);
                    _context.Entry<MembershipTodayStar>(newMembershipTodayStar).State = System.Data.Entity.EntityState.Added;
                    _context.SaveChanges();
                }

                TempData[AppConstants.MessageViewBagName] = new GenericMessageViewModel
                {
                    Message = "已更新每日之星数据。",
                    MessageType = GenericMessages.info
                };
            }

            return RedirectToAction("Index", "Home");

        }

        /// <summary>
        /// 载入用户名片页
        /// </summary>
        /// <param name="slug"></param>
        /// <returns></returns>
        public ActionResult GetByName(string slug)
        {
            using (UnitOfWorkManager.NewUnitOfWork())
            {
                var member = MembershipService.GetUserBySlug(slug);
                //var loggedonId = UserIsAuthenticated ? LoggedOnReadOnlyUser.Id : Guid.Empty;
                if (member != null)
                {
                    var loggedonId = member.Id;
                    var permissions = RoleService.GetPermissions(null, UsersRole);
                    var picturelist = _MembershipUserPictureService.GetMembershipUserPictureListByUserId(loggedonId);

                    #region 每日之星

                    var meiRiZhiXing = _MembershipTodayStarService.Get(member);
                    string Operator = LoggedOnReadOnlyUser == null ? "" : LoggedOnReadOnlyUser.UserName;
                    if (meiRiZhiXing == null)
                    {
                        meiRiZhiXing = new MembershipTodayStar()
                        {
                            JobId = "",
                            CreateDate = DateTime.Now,
                            StartTime = DateTime.Now,
                            StopTime = DateTime.Now.AddDays(7),
                            Operator = Operator,
                            Status = false,
                            UserId = loggedonId
                        };
                    }

                    #endregion

                    #region 绑定Role信息

                    IList<MembershipRole> rolelist = RoleService.AllRoles();

                    if (rolelist != null && rolelist.Count > 0)
                    {
                        var Items_Role = new List<SelectListItem>();
                        foreach (var item in rolelist)
                        {
                            Items_Role.Add(new SelectListItem { Text = item.RoleName, Value = item.Id.ToString() });
                        }
                        ViewData["RoleList"] = Items_Role;
                    }

                    #endregion

                    #region 绑定用户类型

                    var Items_UserType = new List<SelectListItem>();

                    Items_UserType.Add(new SelectListItem { Text = Enum_UserType.A.ToString(), Value = Convert.ToString((int)Enum_UserType.A) });
                    Items_UserType.Add(new SelectListItem { Text = Enum_UserType.B.ToString(), Value = Convert.ToString((int)Enum_UserType.B) });
                    Items_UserType.Add(new SelectListItem { Text = Enum_UserType.C.ToString(), Value = Convert.ToString((int)Enum_UserType.C) });
                    Items_UserType.Add(new SelectListItem { Text = Enum_UserType.D.ToString(), Value = Convert.ToString((int)Enum_UserType.D) });
                    Items_UserType.Add(new SelectListItem { Text = Enum_UserType.E.ToString(), Value = Convert.ToString((int)Enum_UserType.E) });

                    ViewData["UserTypeList"] = Items_UserType;

                    #endregion

                    Follow followInfo = null;
                    if (LoggedOnReadOnlyUser != null)
                    {
                        followInfo = _FollowService.Get(LoggedOnReadOnlyUser.Id, member.Id);
                    }
                    int followFlag = 0;
                    if (followInfo != null)
                    {
                        if (followInfo.OpsFlag == "")
                        {
                            followFlag = 1;  //已关注
                        }
                        else
                        {
                            followFlag = -1;  //黑名单
                        }
                    }
                    else
                    {
                        followFlag = 0;   // 未关注，可关注
                    }

                    return View(new ViewMemberViewModel
                    {
                        User = member,
                        LoggedOnUserId = loggedonId,
                        Permissions = permissions,
                        MembershipUserPictures = picturelist,
                        MeiRiZhiXing = meiRiZhiXing,
                        RoleId = member.Roles.FirstOrDefault().Id.ToString(),
                        FollowStatus = followFlag,
                        UserType = (int)member.UserType,
                    });
                }
                else
                {
                    return View();
                }
            }
        }

        /// <summary>
        /// 根据用户的Id,查找并更新用户的角色
        /// </summary>
        /// <param name="id"></param>
        /// <param name="role"></param>
        /// <returns></returns>
        [Authorize(Roles = AppConstants.AdminRoleName)]
        public ActionResult UpdateUserRole(Guid id, MembershipRole role)
        {
            using (var unitOfWork = UnitOfWorkManager.NewUnitOfWork())
            {
                try
                {
                    MembershipService.UpdateUserRole(id, role);
                    unitOfWork.Commit();
                    TempData[AppConstants.MessageViewBagName] = new GenericMessageViewModel { Message = "已切换用户权限。", MessageType = GenericMessages.success };
                }
                catch (Exception ex)
                {
                    unitOfWork.Rollback();
                    LoggingService.Error(ex);
                    ModelState.AddModelError("", "更新用户的角色信息失败。");
                }
                return View();
            }
        }

        /// <summary>
        /// 切换用户角色
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Authorize(Roles = AppConstants.AdminRoleName)]
        public ActionResult ChangeUserRoles(ViewMemberViewModel model)
        {
            if (model != null && model.LoggedOnUserId != null && model.RoleId != null)
            {
                MembershipRole role = RoleService.GetRole(Guid.Parse(model.RoleId));
                var result = UpdateUserRole(model.LoggedOnUserId, role);
            }
            return RedirectToAction("Index", "Home");
        }


        /// <summary>
        /// 切换用户类型
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Authorize(Roles = AppConstants.AdminRoleName)]
        public ActionResult ChangeUserType(ViewMemberViewModel model)
        {
            using (var unitOfWork = UnitOfWorkManager.NewUnitOfWork())
            {
                try
                {
                    MembershipService.UpdateUserType(model.LoggedOnUserId, model.UserType);
                    unitOfWork.Commit();
                    TempData[AppConstants.MessageViewBagName] = new GenericMessageViewModel { Message = "已切换用户类型。", MessageType = GenericMessages.success };
                }
                catch (Exception ex)
                {
                    unitOfWork.Rollback();
                    LoggingService.Error(ex);
                    ModelState.AddModelError("", "更新用户的类型信息失败。");
                }
                return RedirectToAction("Index", "Home");
            }
        }

        /// <summary>
        /// 好友动态
        /// </summary>
        /// <returns></returns>
        [Authorize]
        public ActionResult FriendMessage()
        {
            var _friendlist = _FollowService.GetFriendList(LoggedOnReadOnlyUser.Id);
            List<Topic> Topiclist = new List<Topic>();
            if (_friendlist != null && _friendlist.Count > 0)
            {
                List<Guid> friendIDs = new List<Guid>();

                foreach (Follow friendInstance in _friendlist)
                {
                    if (!friendIDs.Contains(friendInstance.FriendUserId))
                    {
                        friendIDs.Add(friendInstance.FriendUserId);
                    }
                }
                Topiclist.AddRange(_topicService.GetAllTopicsByCondition(EnumCategoryType.MeiRiXinqing, friendIDs));
            }

            FriendMessage_ListViewModel model = new FriendMessage_ListViewModel();
            var settings = SettingsService.GetSettings();
            var allowedCategories = new List<Category>();
            allowedCategories.Add(_categoryService.GetCategoryByEnumCategoryType(EnumCategoryType.MeiRiXinqing));
            var topicViewModels = ViewModelMapping.CreateTopicViewModels(Topiclist, RoleService, UsersRole, LoggedOnReadOnlyUser, allowedCategories, settings);
            model.Topics = topicViewModels.OrderByDescending(x => x.Topic.CreateDate).ToList();

            return View(model);
        }


        #region 用户关注

        [Authorize]
        public ActionResult Follow()
        {
            Guid UserId = LoggedOnReadOnlyUser.Id;
            Follow_ListViewModel model = new Follow_ListViewModel();

            #region 我的好友

            List<FollowEx> FriendList = null;
            var _friendlist = _FollowService.GetFriendList(UserId);
            if (_friendlist != null && _friendlist.Count > 0)
            {
                FriendList = new List<FollowEx>();
                MembershipUser my_MembershipUserInstance = MembershipService.GetUser(UserId);
                foreach (var item in _friendlist)
                {
                    FollowEx newFollowEx = new FollowEx();
                    newFollowEx.FollowId = item.Id;
                    newFollowEx.FollowInstance = item;
                    newFollowEx.MyUserInstance = my_MembershipUserInstance;
                    newFollowEx.OtherPeopleInstance = MembershipService.GetUser(item.FriendUserId);
                    FriendList.Add(newFollowEx);
                }
            }
            model.FriendList = FriendList;

            #endregion

            #region 我关注的

            List<FollowEx> MyFollowedList = null;
            var _MyFollowedList = _FollowService.Get_SpecificUser_Followed_Poeple_List(UserId);
            if (_MyFollowedList != null && _MyFollowedList.Count > 0)
            {
                MyFollowedList = new List<FollowEx>();
                MembershipUser my_MembershipUserInstance = MembershipService.GetUser(UserId);
                foreach (var item in _MyFollowedList)
                {
                    FollowEx newFollowEx = new FollowEx();
                    newFollowEx.FollowId = item.Id;
                    newFollowEx.FollowInstance = item;
                    newFollowEx.MyUserInstance = my_MembershipUserInstance;
                    newFollowEx.OtherPeopleInstance = MembershipService.GetUser(item.FriendUserId);

                    bool IsFriendFlag = false;
                    if (FriendList != null)
                    {
                        foreach (var item1 in FriendList)
                        {
                            if (item1.OtherPeopleInstance.Id == item.FriendUserId)
                            {
                                IsFriendFlag = true;
                                break;
                            }
                        }
                    }

                    if (!IsFriendFlag)
                    {
                        MyFollowedList.Add(newFollowEx);
                    }
                }
            }
            model.MyFollowedList = MyFollowedList;

            #endregion

            #region 关注我的

            List<FollowEx> FollowMeList = null;
            var _FollowMeList = _FollowService.Get_People_Followed_SpecificUser_List(UserId);
            if (_FollowMeList != null && _FollowMeList.Count > 0)
            {
                FollowMeList = new List<FollowEx>();
                MembershipUser my_MembershipUserInstance = MembershipService.GetUser(UserId);
                foreach (var item in _FollowMeList)
                {
                    FollowEx newFollowEx = new FollowEx();
                    newFollowEx.FollowId = item.Id;
                    newFollowEx.FollowInstance = item;
                    newFollowEx.MyUserInstance = my_MembershipUserInstance;
                    newFollowEx.OtherPeopleInstance = MembershipService.GetUser(item.UserId);
                    FollowMeList.Add(newFollowEx);
                }
            }
            model.FollowMeList = FollowMeList;

            #endregion

            #region 黑名单

            List<FollowEx> BlackList = null;
            var _BlackList = _FollowService.GetBlackList(UserId);
            if (_BlackList != null && _BlackList.Count > 0)
            {
                BlackList = new List<FollowEx>();
                MembershipUser my_MembershipUserInstance = MembershipService.GetUser(UserId);
                foreach (var item in _BlackList)
                {
                    FollowEx newFollowEx = new FollowEx();
                    newFollowEx.FollowId = item.Id;
                    newFollowEx.FollowInstance = item;
                    newFollowEx.MyUserInstance = my_MembershipUserInstance;
                    newFollowEx.OtherPeopleInstance = MembershipService.GetUser(item.FriendUserId);
                    BlackList.Add(newFollowEx);
                }
            }
            model.BlackList = BlackList;

            #endregion

            return View(model);
        }

        [HttpPost]
        [Authorize]
        public ActionResult AddFollow(ViewMemberViewModel model)
        {
            if (model != null && model.LoggedOnUserId != null)
            {
                var followInfo = new Follow();
                followInfo.UserId = LoggedOnReadOnlyUser.Id;
                followInfo.FriendUserId = model.LoggedOnUserId;
                followInfo.CreateTime = DateTime.Now;
                followInfo.OpsFlag = "";
                followInfo.UpdateTime = DateTime.Now;
                _FollowService.Add(followInfo);


                using (var unitOfWork = UnitOfWorkManager.NewUnitOfWork())
                {
                    try
                    {
                        unitOfWork.Commit();
                    }
                    catch (Exception ex)
                    {
                        unitOfWork.Rollback();
                        LoggingService.Error(ex);
                        ModelState.AddModelError(string.Empty, LocalizationService.GetResourceString("Errors.GenericMessage"));
                    }
                }
            }
            return RedirectToAction("Follow", "Members");
        }


        [HttpPost]
        [Authorize]
        public ActionResult CancelFollow(ViewMemberViewModel model)
        {
            if (model != null && model.LoggedOnUserId != null)
            {
                var info = _FollowService.Get(LoggedOnReadOnlyUser.Id, model.LoggedOnUserId);
                _FollowService.Delete(info);
                _context.Entry<Follow>(info).State = EntityState.Deleted;

                using (var unitOfWork = UnitOfWorkManager.NewUnitOfWork())
                {
                    try
                    {
                        unitOfWork.Commit();
                    }
                    catch (Exception ex)
                    {
                        unitOfWork.Rollback();
                        LoggingService.Error(ex);
                        ModelState.AddModelError(string.Empty, LocalizationService.GetResourceString("Errors.GenericMessage"));
                    }
                }
            }
            return RedirectToAction("Follow", "Members");
        }

        [HttpPost]
        [Authorize]
        public ActionResult AddBlackList(ViewMemberViewModel model)
        {
            if (model != null && model.LoggedOnUserId != null)
            {
                var info = _FollowService.Get(LoggedOnReadOnlyUser.Id, model.LoggedOnUserId);

                if (info != null)
                {
                    info.OpsFlag = "Black";
                    _context.Entry<Follow>(info).State = EntityState.Modified;
                }
                else
                {
                    var followInfo = new Follow();
                    followInfo.UserId = LoggedOnReadOnlyUser.Id;
                    followInfo.FriendUserId = model.LoggedOnUserId;
                    followInfo.CreateTime = DateTime.Now;
                    followInfo.OpsFlag = "Black";
                    followInfo.UpdateTime = DateTime.Now;
                    _FollowService.Add(followInfo);
                }

                using (var unitOfWork = UnitOfWorkManager.NewUnitOfWork())
                {
                    try
                    {
                        unitOfWork.Commit();
                    }
                    catch (Exception ex)
                    {
                        unitOfWork.Rollback();
                        LoggingService.Error(ex);
                        ModelState.AddModelError(string.Empty, LocalizationService.GetResourceString("Errors.GenericMessage"));
                    }
                }
            }
            return RedirectToAction("Follow", "Members");
        }


        [HttpPost]
        [Authorize]
        public ActionResult CancelBlackList(ViewMemberViewModel model)
        {
            FollowAction_CancelBlackList(model.LoggedOnUserId);
            return RedirectToAction("Follow", "Members");
        }

        [HttpPost]
        [Authorize]
        public ActionResult CancelBlackList2(FollowEx model)
        {
            string BlackUserId = Request.Form["item.FollowInstance.FriendUserId"];
            FollowAction_CancelBlackList(Guid.Parse(BlackUserId));
            return RedirectToAction("Follow", "Members");
        }

        private void FollowAction_CancelBlackList(Guid BlackListUserId)
        {
            if (BlackListUserId != Guid.Empty)
            {
                var info = _FollowService.Get(LoggedOnReadOnlyUser.Id, BlackListUserId);

                if (info != null && info.OpsFlag == "Black")
                {
                    _FollowService.Delete(info);
                    _context.Entry<Follow>(info).State = EntityState.Deleted;
                }

                using (var unitOfWork = UnitOfWorkManager.NewUnitOfWork())
                {
                    try
                    {
                        unitOfWork.Commit();
                    }
                    catch (Exception ex)
                    {
                        unitOfWork.Rollback();
                        LoggingService.Error(ex);
                        ModelState.AddModelError(string.Empty, LocalizationService.GetResourceString("Errors.GenericMessage"));
                    }
                }
            }
        }
        #endregion

        #endregion

        #region 其他

        [HttpPost]
        public PartialViewResult GetMemberDiscussions(Guid id)
        {
            if (Request.IsAjaxRequest())
            {
                using (UnitOfWorkManager.NewUnitOfWork())
                {
                    var allowedCategories = _categoryService.GetAllowedCategories(UsersRole).ToList();

                    // Get the user discussions, only grab 100 posts
                    var posts = _postService.GetByMember(id, 100, allowedCategories);

                    // Get the distinct topics
                    var topics = posts.Select(x => x.Topic).Distinct().Take(6).OrderByDescending(x => x.LastPost.DateCreated).ToList();

                    // Get the Topic View Models
                    var topicViewModels = ViewModelMapping.CreateTopicViewModels(topics, RoleService, UsersRole, LoggedOnReadOnlyUser, allowedCategories, SettingsService.GetSettings());

                    // create the view model
                    var viewModel = new ViewMemberDiscussionsViewModel
                    {
                        Topics = topicViewModels
                    };


                    return PartialView(viewModel);
                }
            }
            return null;
        }

        [Authorize]
        public PartialViewResult SideAdminPanel(bool isDropDown)
        {
            var privateMessageCount = 0;
            var moderateCount = 0;
            var settings = SettingsService.GetSettings();
            if (LoggedOnReadOnlyUser != null)
            {
                var allowedCategories = _categoryService.GetAllowedCategories(UsersRole);
                privateMessageCount = _privateMessageService.NewPrivateMessageCount(LoggedOnReadOnlyUser.Id);
                var pendingTopics = _topicService.GetPendingTopics(allowedCategories, UsersRole);
                var pendingPosts = _postService.GetPendingPosts(allowedCategories, UsersRole);
                moderateCount = (pendingTopics.Count + pendingPosts.Count);
            }

            var canViewPms = settings.EnablePrivateMessages && LoggedOnReadOnlyUser != null && LoggedOnReadOnlyUser.DisablePrivateMessages != true;
            var viewModel = new ViewAdminSidePanelViewModel
            {
                CurrentUser = LoggedOnReadOnlyUser,
                NewPrivateMessageCount = canViewPms ? privateMessageCount : 0,
                CanViewPrivateMessages = canViewPms,
                ModerateCount = moderateCount,
                IsDropDown = isDropDown
            };

            return PartialView(viewModel);
        }

        public PartialViewResult AdminMemberProfileTools()
        {
            return PartialView();
        }

        [Authorize]
        public string AutoComplete(string term)
        {
            using (UnitOfWorkManager.NewUnitOfWork())
            {
                if (!string.IsNullOrEmpty(term))
                {
                    var members = MembershipService.SearchMembers(term, 12);
                    var sb = new StringBuilder();
                    sb.Append("[").Append(Environment.NewLine);
                    for (var i = 0; i < members.Count; i++)
                    {
                        sb.AppendFormat("\"{0}\"", members[i].UserName);
                        if (i < members.Count - 1)
                        {
                            sb.Append(",");
                        }
                        sb.Append(Environment.NewLine);
                    }
                    sb.Append("]");
                    return sb.ToString();
                }
                return null;
            }
        }

        [Authorize]
        public ActionResult Report(Guid id)
        {
            if (SettingsService.GetSettings().EnableMemberReporting)
            {
                using (UnitOfWorkManager.NewUnitOfWork())
                {
                    var user = MembershipService.GetUser(id);
                    return View(new ReportMemberViewModel { Id = user.Id, Username = user.UserName });
                }
            }
            return ErrorToHomePage(LocalizationService.GetResourceString("Errors.GenericMessage"));
        }

        [HttpPost]
        [Authorize]
        public ActionResult Report(ReportMemberViewModel viewModel)
        {
            if (SettingsService.GetSettings().EnableMemberReporting)
            {
                using (var unitOfWork = UnitOfWorkManager.NewUnitOfWork())
                {
                    var user = MembershipService.GetUser(viewModel.Id);
                    var report = new Report
                    {
                        Reason = viewModel.Reason,
                        ReportedMember = user,
                        Reporter = LoggedOnReadOnlyUser
                    };
                    _reportService.MemberReport(report);

                    try
                    {
                        unitOfWork.Commit();
                    }
                    catch (Exception ex)
                    {
                        unitOfWork.Rollback();
                        LoggingService.Error(ex);
                    }

                    TempData[AppConstants.MessageViewBagName] = new GenericMessageViewModel
                    {
                        Message = LocalizationService.GetResourceString("Report.ReportSent"),
                        MessageType = GenericMessages.success
                    };
                    return View(new ReportMemberViewModel { Id = user.Id, Username = user.UserName });
                }
            }
            return ErrorToHomePage(LocalizationService.GetResourceString("Errors.GenericMessage"));
        }

        [ChildActionOnly]
        public PartialViewResult LatestMembersJoined()
        {
            var viewModel = new ListLatestMembersViewModel();
            var users = MembershipService.GetLatestUsers(10).ToDictionary(o => o.UserName, o => o.NiceUrl);
            viewModel.Users = users;
            return PartialView(viewModel);
        }

        [ChildActionOnly]
        public PartialViewResult GetCurrentActiveMembers()
        {
            using (UnitOfWorkManager.NewUnitOfWork())
            {
                var viewModel = new ActiveMembersViewModel
                {
                    ActiveMembers = MembershipService.GetActiveMembers()
                };
                return PartialView(viewModel);
            }
        }

        /// <summary>
        /// 最新活动用户检查
        /// </summary>
        /// <returns></returns>
        public JsonResult LastActiveCheck()
        {
            if (UserIsAuthenticated)
            {
                var rightNow = DateTime.Now;
                var usersDate = LoggedOnReadOnlyUser.LastActivityDate ?? DateTime.Now.AddDays(-1);

                var span = rightNow.Subtract(usersDate);
                var totalMins = span.TotalMinutes;

                if (totalMins > AppConstants.TimeSpanInMinutesToDoCheck)
                {
                    using (var unitOfWork = UnitOfWorkManager.NewUnitOfWork())
                    {
                        // Actually get the user, LoggedOnUser has no tracking
                        var loggedOnUser = MembershipService.GetUser(Username);

                        // Update users last activity date so we can show the latest users online
                        loggedOnUser.LastActivityDate = DateTime.Now;

                        // Update
                        try
                        {
                            unitOfWork.Commit();
                        }
                        catch (Exception ex)
                        {
                            unitOfWork.Rollback();
                            LoggingService.Error(ex);
                        }
                    }
                }
            }

            // You can return anything to reset the timer.
            return Json(new { Timer = "reset" }, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region 省市县三级联动

        public JsonResult GetHometownProvince()
        {
            List<TProvince> queryResult = TProvince.LoadAllProvinceList();
            return Json(queryResult.ToList(), JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetHometownCity(int id)
        {
            List<TCity> FilterCitys = TCity.LoadCityListByProvince(id);
            return Json(FilterCitys, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetEducationCity(int id)
        {
            List<TCity> FilterCitys = TCity.LoadCityListByProvince(id);
            return Json(FilterCitys, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetHometownCountry(string id)
        {
            List<int> result = new List<string>(id.Split(',')).ConvertAll(i => int.Parse(i));
            int ProvinceId = result[0];
            int CityId = result[1];
            return Json(TCountry.LoadCountryByProvinceAndCity(ProvinceId, CityId).ToList(), JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region EmailConfirm 相关动作


        private void SendEmailConfirmationEmail(MembershipUser userToSave)
        {
            var settings = SettingsService.GetSettings();
            var manuallyAuthoriseMembers = settings.ManuallyAuthoriseNewMembers;
            var memberEmailAuthorisationNeeded = settings.NewMemberEmailConfirmation == true;
            if (manuallyAuthoriseMembers == false && memberEmailAuthorisationNeeded)
            {
                if (!string.IsNullOrEmpty(userToSave.Email))
                {
                    // SEND AUTHORISATION EMAIL
                    var sb = new StringBuilder();
                    var confirmationLink = string.Concat(StringUtils.ReturnCurrentDomain(), Url.Action("EmailConfirmation", new { id = userToSave.Id }));
                    sb.AppendFormat("<p>{0}</p>", string.Format(LocalizationService.GetResourceString("Members.MemberEmailAuthorisation.EmailBody"),
                                                settings.ForumName,
                                                string.Format("<p><a href=\"{0}\">{0}</a></p>", confirmationLink)));
                    var email = new Email
                    {
                        EmailTo = userToSave.Email,
                        NameTo = userToSave.UserName,
                        Subject = LocalizationService.GetResourceString("Members.MemberEmailAuthorisation.Subject")
                    };
                    email.Body = _emailService.EmailTemplate(email.NameTo, sb.ToString());
                    _emailService.SendMail(email);

                    // ADD COOKIE
                    // We add a cookie for 7 days, which will display the resend email confirmation button
                    // This cookie is removed when they click the confirmation link
                    var myCookie = new HttpCookie(AppConstants.MemberEmailConfirmationCookieName)
                    {
                        Value = $"{userToSave.Email}#{userToSave.UserName}",
                        Expires = DateTime.Now.AddDays(7)
                    };
                    // Add the cookie.
                    Response.Cookies.Add(myCookie);
                }
            }
        }

        public ActionResult ResendEmailConfirmation(string username)
        {
            using (var unitOfWork = UnitOfWorkManager.NewUnitOfWork())
            {
                var user = MembershipService.GetUser(username);
                if (user != null)
                {
                    SendEmailConfirmationEmail(user);
                    TempData[AppConstants.MessageViewBagName] = new GenericMessageViewModel
                    {
                        Message = LocalizationService.GetResourceString("Members.MemberEmailAuthorisationNeeded"),
                        MessageType = GenericMessages.success
                    };
                }

                try
                {
                    unitOfWork.Commit();
                }
                catch (Exception ex)
                {
                    unitOfWork.Rollback();
                    LoggingService.Error(ex);
                }
            }
            return RedirectToAction("Index", "Home");
        }

        /// <summary>
        /// Email confirmation page from the link in the users email
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult EmailConfirmation(Guid id)
        {
            using (var unitOfWork = UnitOfWorkManager.NewUnitOfWork())
            {
                // Checkconfirmation
                var user = MembershipService.GetUser(id);
                if (user != null)
                {
                    // Set the user to active
                    user.IsApproved = true;

                    // Delete Cookie and log them in if this cookie is present
                    if (Request.Cookies[AppConstants.MemberEmailConfirmationCookieName] != null)
                    {
                        var myCookie = new HttpCookie(AppConstants.MemberEmailConfirmationCookieName)
                        {
                            Expires = DateTime.Now.AddDays(-1)
                        };
                        Response.Cookies.Add(myCookie);

                        // Login code
                        FormsAuthentication.SetAuthCookie(user.UserName, false);
                    }

                    // Show a new message
                    // We use temp data because we are doing a redirect
                    TempData[AppConstants.MessageViewBagName] = new GenericMessageViewModel
                    {
                        Message = LocalizationService.GetResourceString("Members.NowApproved"),
                        MessageType = GenericMessages.success
                    };
                }

                try
                {
                    unitOfWork.Commit();
                }
                catch (Exception ex)
                {
                    unitOfWork.Rollback();
                    LoggingService.Error(ex);
                }
            }

            return RedirectToAction("Index", "Home");
        }


        #endregion

    }
}
