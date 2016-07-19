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
        log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #region 建构式

        #region Private defination
        private readonly IPostService _postService;
        private readonly ITopicService _topicService;
        private readonly IReportService _reportService;
        private readonly IEmailService _emailService;
        private readonly IPrivateMessageService _privateMessageService;
        private readonly IBannedWordService _bannedWordService;
        private readonly ICategoryService _categoryService;
        private readonly IVerifyCodeService _verifyCodeService;
        private readonly MVCForumContext _context;
        #endregion

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
            ITopicService topicService)
            : base(loggingService, unitOfWorkManager, membershipService, localizationService, roleService, settingsService)
        {
            _context = context as MVCForumContext;
            _postService = postService;
            _reportService = reportService;
            _emailService = emailService;
            _privateMessageService = privateMessageService;
            _bannedWordService = bannedWordService;
            _categoryService = categoryService;
            _topicService = topicService;
            _verifyCodeService = verifyCodeService;
            

        }

        #endregion

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
                logger.Error(ex.Message);
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

                // 短信验证码存入session(session的默认失效时间30分钟) 
                //也可存入Memcached缓存
                Session.Add("code", code);
                try
                {
                    _verifyCodeService.SendVerifyCode(new VerifyCode(MobilePhone, VerifyCodeStatus.Waiting, code));
                    result = true;// 成功    
                }
                catch (Exception ex)
                {
                    result = false;// 失败    
                    logger.Error(ex.Message);
                }
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
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
                    }
                    catch (Exception ex)
                    {
                        result = false;// 失败    
                        logger.Error(ex.Message);
                    }
                }
                else
                {
                    result = false;// 失败  
                    logger.Error(UserName + "对应的用户信息不存在。");
                    //result = "failNoExistUser";// 失败，用户信息不存在。
                    ModelState.AddModelError("NoExistUser", "用户信息不存在。");
                }
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                logger.Error("发生错误，详细信息为：" + ex.Message);
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
                logger.Warn("当前系统禁止注册。");
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
                    //将等待验证（Waiting）的验证码的状态改为验证完成
                    VerifyCode mVerifyCode = new VerifyCode(userModel.MobilePhone.Trim(), VerifyCodeStatus.Waiting, userModel.VerifyCode);
                    new VerifyCodeService().CompleteVerifyCode(mVerifyCode);

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
                // If not manually authorise then log the user in
                FormsAuthentication.SetAuthCookie(userToSave.UserName, false);
                TempData[AppConstants.MessageViewBagName] = new GenericMessageViewModel
                {
                    //Message = LocalizationService.GetResourceString("Members.NowRegistered"),
                    Message = "欢迎你，" + userToSave.AliasName,
                    MessageType = GenericMessages.success
                };
            }
        }

        public ActionResult SocialLoginValidator()
        {
            // Store the viewModel in TempData - Which we'll use in the register logic
            if (TempData[AppConstants.MemberRegisterViewModel] != null)
            {
                var userModel = (TempData[AppConstants.MemberRegisterViewModel] as MemberAddViewModel);

                // Do the register logic
                return MemberRegisterLogic(userModel);
            }

            ModelState.AddModelError(string.Empty, LocalizationService.GetResourceString("Errors.GenericMessage"));
            return View("Register");
        }

        #endregion

        #region 生成测试账号
        [HttpPost]
        [BasicMultiButton("Btn_Generate5SupplierAccount")]
        public ActionResult GenerateTestAccount()
        {
            Stopwatch MyStopWatch = new Stopwatch(); //性能计时器
            MyStopWatch.Start(); //启动计时器

            using (var unitOfWork = UnitOfWorkManager.NewUnitOfWork())
            {
                try
                {
                    MembershipService.Create5SupplierAccount();
                    unitOfWork.Commit();
                }
                catch (Exception ex)
                {
                    unitOfWork.Rollback();
                    LoggingService.Error(ex);
                    FormsAuthentication.SignOut();
                    ModelState.AddModelError(string.Empty, LocalizationService.GetResourceString("Errors.GenericMessage"));
                }
            }

            ShowMessage(new GenericMessageViewModel
            {
                //Message = LocalizationService.GetResourceString("Member.ProfileUpdated"),
                Message = "生成5个供应商测试账号执行完毕",
                MessageType = GenericMessages.success
            });

            MyStopWatch.Stop();
            decimal t = MyStopWatch.ElapsedMilliseconds;
            logger.Debug(string.Format("生成5个供应商测试账号执行完毕.Timecost:{0} seconds.", t / 1000));

            return RedirectToAction("Register", "Members");
            //return Content("生成5个供应商测试账号执行完毕");
        }

        [HttpPost]
        [BasicMultiButton("Btn_Generate50TestAccount")]
        public ActionResult GenerateTestAccount2()
        {
            Stopwatch MyStopWatch = new Stopwatch(); //性能计时器
            MyStopWatch.Start(); //启动计时器

            using (var unitOfWork = UnitOfWorkManager.NewUnitOfWork())
            {
                try
                {
                    MembershipService.Create50TestAccount();
                    unitOfWork.Commit();
                }
                catch (Exception ex)
                {
                    unitOfWork.Rollback();
                    LoggingService.Error(ex);
                    FormsAuthentication.SignOut();
                    ModelState.AddModelError(string.Empty, LocalizationService.GetResourceString("Errors.GenericMessage"));
                }
            }
            ShowMessage(new GenericMessageViewModel
            {
                //Message = LocalizationService.GetResourceString("Member.ProfileUpdated"),
                Message = "生成50个测试账号执行完毕",
                MessageType = GenericMessages.success
            });

            MyStopWatch.Stop();
            decimal t = MyStopWatch.ElapsedMilliseconds;
            logger.Debug(string.Format("生成50个测试账号执行完毕.Timecost:{0} seconds.", t / 1000));
            return RedirectToAction("Register", "Members");
            //return Content("生成50个测试账号执行完毕");
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
            Items_Gender.Add(new SelectListItem { Text = "女", Value = "0" });

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
            Items_Married.Add(new SelectListItem { Text = "未婚", Value = "0" });

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
            Items_Education.Add(new SelectListItem { Text = "高中以下", Value = "1" });
            Items_Education.Add(new SelectListItem { Text = "高中，中专", Value = "2" });
            Items_Education.Add(new SelectListItem { Text = "大专", Value = "3" });
            Items_Education.Add(new SelectListItem { Text = "本科", Value = "4" });
            Items_Education.Add(new SelectListItem { Text = "硕士", Value = "5" });
            Items_Education.Add(new SelectListItem { Text = "博士", Value = "6" });
            Items_Education.Add(new SelectListItem { Text = "其他", Value = "7" });
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
            Items_IncomeRange.Add(new SelectListItem { Text = "1万以下", Value = "1" });
            Items_IncomeRange.Add(new SelectListItem { Text = "1万至5万", Value = "2" });
            Items_IncomeRange.Add(new SelectListItem { Text = "5万以上", Value = "3" });
            Items_IncomeRange.Add(new SelectListItem { Text = "不好说", Value = "4" });
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
            Items_AuditComments.Add(new SelectListItem { Text = "驳回，内容不合规", Value = "驳回，内容不合规" });

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
                            logger.Info("用户名在更新中发生变更，旧的用户名为：" + user.UserName + ",新的用户名为：" + sanitisedUsername);
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
                            messagestr = "用户信息已更新，等待管理员审核。";
                        }
                        else
                        {
                            user.IsApproved = true;  //管理员和供应商无需审核
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

        private static MemberFrontEndEditViewModel PopulateMemberViewModel(MembershipUser user)
        {
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
                

            };
            return viewModel;
        }

        #endregion



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
                    Items_AuditComments.Add(new SelectListItem { Text = "审核通过", Value = "1" });
                    Items_AuditComments.Add(new SelectListItem { Text = "驳回，内容不合规", Value = "0" });

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
        [Authorize]
        public ActionResult Audit(MemberFrontEndEditViewModel userModel)
        {
            if (string.IsNullOrEmpty(userModel.AuditComment) || userModel.Id == Guid.Empty)
            {
                return View(userModel);
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
                    }
                    else
                    {
                        user.IsApproved = false;
                    }

                    _context.Entry<MembershipUser>(user).State = System.Data.Entity.EntityState.Modified;
                    //EntityOperationUtils.ModifyObject(user);
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
                                        return Redirect(model.ReturnUrl);
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
                    }
                    catch (Exception ex)
                    {
                        unitOfWork.Rollback();
                        LoggingService.Error(ex);
                        changePasswordSucceeded = false;
                    }
                }
            }

            // Commited successfully carry on
            using (UnitOfWorkManager.NewUnitOfWork())
            {
                if (changePasswordSucceeded)
                {
                    // We use temp data because we are doing a redirect
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

            MembershipUser user;
            using (var unitOfWork = UnitOfWorkManager.NewUnitOfWork())
            {
                user = MembershipService.GetUser(forgotPasswordViewModel.UserName);
                if (user == null)
                {
                    ModelState.AddModelError("NoExistUser", "用户不存在。");
                    return View();
                }

                try
                {
                    // If the user is registered then create a security token and a timestamp that will allow a change of password
                    MembershipService.UpdatePasswordResetToken(user);
                    unitOfWork.Commit();
                }
                catch (Exception ex)
                {
                    unitOfWork.Rollback();
                    LoggingService.Error(ex);
                    ModelState.AddModelError("", LocalizationService.GetResourceString("Members.ResetPassword.Error"));
                    return View(forgotPasswordViewModel);
                }
            }


            var settings = SettingsService.GetSettings();
            var s2 = settings.ForumUrl.TrimEnd('/');
            var s = Url.Action("ResetPassword", "Members", new { user.Id, token = user.PasswordResetToken });
            var RedirectURL = string.Concat(s2, s);
            //var url = new Uri(RedirectURL);
            //return Redirect(url.ToString());

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

            return Redirect(RedirectURL);


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
        public ActionResult ResetPassword(ResetPasswordViewModel postedModel)
        {
            if (!ModelState.IsValid)
            {
                return View(postedModel);
            }

            using (var unitOfWork = UnitOfWorkManager.NewUnitOfWork())
            {
                if (postedModel.Id != null)
                {
                    var user = MembershipService.GetUser(postedModel.Id.Value);

                    // if the user id wasn't found then we can't proceed
                    // if the token submitted is not valid then do not proceed
                    if (user == null || user.PasswordResetToken == null || !MembershipService.IsPasswordResetTokenValid(user, postedModel.Token))
                    {
                        ModelState.AddModelError("", LocalizationService.GetResourceString("Members.ResetPassword.InvalidToken"));
                        return View(postedModel);
                    }

                    try
                    {
                        // The security token is valid so change the password
                        MembershipService.ResetPassword(user, postedModel.NewPassword);
                        // Clear the token and the timestamp so that the URL cannot be used again
                        MembershipService.ClearPasswordResetToken(user);
                        unitOfWork.Commit();
                    }
                    catch (Exception ex)
                    {
                        unitOfWork.Rollback();
                        LoggingService.Error(ex);
                        ModelState.AddModelError("", LocalizationService.GetResourceString("Members.ResetPassword.InvalidToken"));
                        return View(postedModel);
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

        #region 省市县三级联动

        public JsonResult GetHometownProvince()
        {
            List<TProvince> queryResult = TProvince.LoadAllProvinceList();
            return Json(queryResult.ToList(), JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetHometownCity(int id)
        {
            List<TCity> allCity = TCity.LoadAllCityList();
            var a = allCity.Where(P => P.ProvinceId.Equals(id)).ToList();
            return Json(allCity.Where(P => P.ProvinceId == id).ToList(), JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetEducationCity(int id)
        {
            List<TCity> allCity = TCity.LoadAllCityList();
            var a = allCity.Where(P => P.ProvinceId.Equals(id)).ToList();
            return Json(allCity.Where(P => P.ProvinceId == id).ToList(), JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetHometownCountry(string id)
        {
            List<int> result = new List<string>(id.Split(',')).ConvertAll(i => int.Parse(i));
            int ProvinceId = result[0];
            int CityId = result[1];
            return Json(TCountry.LoadAllCountry().Where(E => E.ProvinceId == ProvinceId && E.CityId == CityId).ToList(), JsonRequestBehavior.AllowGet);
        }

        #endregion

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

        public ActionResult GetByName(string slug)
        {
            using (UnitOfWorkManager.NewUnitOfWork())
            {
                var member = MembershipService.GetUserBySlug(slug);
                var loggedonId = UserIsAuthenticated ? LoggedOnReadOnlyUser.Id : Guid.Empty;
                var permissions = RoleService.GetPermissions(null, UsersRole);

                // Localise the badge names
                foreach (var item in member.Badges)
                {
                    var partialKey = string.Concat("Badge.", item.Name);
                    item.DisplayName = LocalizationService.GetResourceString(string.Concat(partialKey, ".Name"));
                    item.Description = LocalizationService.GetResourceString(string.Concat(partialKey, ".Desc"));
                }

                return View(new ViewMemberViewModel
                {
                    User = member,
                    LoggedOnUserId = loggedonId,
                    Permissions = permissions
                });
            }
        }

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


        /// <summary>
        /// 根据用户的Id,查找并更新用户的角色
        /// </summary>
        /// <param name="id"></param>
        /// <param name="role"></param>
        /// <returns></returns>
        [Authorize(Roles = AppConstants.AdminRoleName)]
        public ActionResult UpdateUserRole(Guid id, MembershipRole role)
        {
            //TODO: 此方法待测试
            using (var unitOfWork = UnitOfWorkManager.NewUnitOfWork())
            {
                try
                {
                    MembershipService.UpdateUserRole(id, role);
                    unitOfWork.Commit();
                    TempData[AppConstants.MessageViewBagName] = new GenericMessageViewModel { Message = "", MessageType = GenericMessages.success };
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


    }
}
