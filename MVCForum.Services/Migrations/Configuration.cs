using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Data.SqlTypes;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Hosting;
using MVCForum.Domain.Constants;
using MVCForum.Domain.DomainModel;
using MVCForum.Services.Data.Context;
using MVCForum.Utilities;
using System.Diagnostics;

namespace MVCForum.Services.Migrations
{
    internal sealed class Configuration : DbMigrationsConfiguration<MVCForumContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
            AutomaticMigrationDataLossAllowed = true;
        }

        /// <summary>
        /// 初始化系统安装代码
        /// </summary>
        /// <param name="context"></param>
        protected override void Seed(MVCForumContext context)
        {
            const string langCulture = "zh-CN";   // 系统默认使用的语言
            const string defaultAdminUsername = "admin";  //系统默认管理员账号名称
            const string defaultAdminstratorPassword = "password"; //系统默认管理员账号密码

            log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
            logger.Debug("Start Configuration.Seed.");

            Stopwatch MyStopWatch = new Stopwatch();
            MyStopWatch.Start();


            logger.Debug("Start to get Language.");
            var language = context.Language.FirstOrDefault(x => x.LanguageCulture == langCulture);
            logger.Debug("Stop to get Language.");
            bool IsInitProcess = (language == null);  // 以默认语言是否被安装作为是否初始化的依据

            if (IsInitProcess)
            {
                #region 写入系统默认的语言定义

                var cultureInfo = LanguageUtils.GetCulture(langCulture);
                language = new Language
                {
                    Name = cultureInfo.EnglishName,
                    LanguageCulture = cultureInfo.Name,
                };
                context.Language.Add(language);
                context.SaveChanges();

                #endregion

                #region 载入默认的语言本地化资源文件，并将其写入数据库

                // Now add the default language strings
                var file = HostingEnvironment.MapPath(@"~/Installer/zh-CN.csv");
                var commaSeparator = new[] { ',' };
                if (file != null)
                {
                    // Unpack the data
                    var allLines = new List<string>();
                    using (var streamReader = new StreamReader(file, Encoding.UTF8, true))
                    {
                        while (streamReader.Peek() >= 0)
                        {
                            allLines.Add(streamReader.ReadLine());
                        }
                    }

                    #region Read the CSV file and import all the keys and values

                    var lineCounter = 0;
                    foreach (var csvline in allLines)
                    {
                        var line = csvline;
                        if (line.StartsWith("\""))
                        {
                            line = line.Replace("\"", "");
                        }

                        lineCounter++;

                        // Only split on the first comma, so the value strings can have commas in
                        var keyValuePair = line.Split(commaSeparator, 2, StringSplitOptions.None);

                        // Get the key and value
                        var key = keyValuePair[0];
                        var value = keyValuePair[1];

                        if (string.IsNullOrEmpty(key))
                        {
                            // Ignore empty keys
                            continue;
                        }

                        if (string.IsNullOrEmpty(value))
                        {
                            // Ignore empty values
                            continue;
                        }

                        // Trim both the key and value
                        key = key.Trim();
                        value = value.Trim();

                        // Create the resource key
                        var resourceKey = new LocaleResourceKey
                        {
                            Name = key,
                            DateAdded = DateTime.Now,
                        };
                        context.LocaleResourceKey.Add(resourceKey);

                        // Set the value for the resource
                        var stringResource = new LocaleStringResource
                        {
                            Language = language,
                            LocaleResourceKey = resourceKey,
                            ResourceValue = value
                        };
                        context.LocaleStringResource.Add(stringResource);
                    }
                    #endregion

                    // 保存本地化的语言条目
                    context.SaveChanges();
                }

                #endregion

                #region 创建系统的默认五种角色
                var saveRoles = false;
                // Create the admin role if it doesn't exist
                var adminRole = context.MembershipRole.FirstOrDefault(x => x.RoleName == AppConstants.AdminRoleName);
                if (adminRole == null)
                {
                    adminRole = new MembershipRole { RoleName = AppConstants.AdminRoleName };
                    context.MembershipRole.Add(adminRole);
                    saveRoles = true;
                }

                // Create the Supplier role if it doesn't exist
                var SupplierRole = context.MembershipRole.FirstOrDefault(x => x.RoleName == AppConstants.SupplierRoleName);
                if (SupplierRole == null)
                {
                    SupplierRole = new MembershipRole { RoleName = AppConstants.SupplierRoleName };
                    context.MembershipRole.Add(SupplierRole);
                    saveRoles = true;
                }

                // Create the Standard role if it doesn't exist
                var standardRole = context.MembershipRole.FirstOrDefault(x => x.RoleName == SiteConstants.Instance.StandardMembers);
                if (standardRole == null)
                {
                    standardRole = new MembershipRole { RoleName = SiteConstants.Instance.StandardMembers };
                    context.MembershipRole.Add(standardRole);
                    saveRoles = true;
                }

                //// Create the WaitAuditMembers role if it doesn't exist
                //var WaitAuditMembersRole = context.MembershipRole.FirstOrDefault(x => x.RoleName == SiteConstants.Instance.WaitAuditMembers);
                //if (WaitAuditMembersRole == null)
                //{
                //    WaitAuditMembersRole = new MembershipRole { RoleName = SiteConstants.Instance.WaitAuditMembers };
                //    context.MembershipRole.Add(WaitAuditMembersRole);
                //    saveRoles = true;
                //}

                // Create the Guest role if it doesn't exist
                var guestRole = context.MembershipRole.FirstOrDefault(x => x.RoleName == AppConstants.GuestRoleName);
                if (guestRole == null)
                {
                    guestRole = new MembershipRole { RoleName = AppConstants.GuestRoleName };
                    context.MembershipRole.Add(guestRole);
                    saveRoles = true;
                }

                if (saveRoles)
                {
                    context.SaveChanges();
                }
                #endregion

                #region 创建一个Sample 类型定义和系统自带的三类系统级类型

                if (!context.Category.Any())
                {
                    //创建默认Sample类型
                    context.Category.Add(Category.GenerateCategory(EnumCategoryType.SampleCategory));

                    //创建每日心情日记本（系统）类型
                    context.Category.Add(Category.GenerateCategory(EnumCategoryType.MeiRiXinqing));

                    // 创建最新资讯（系统）类型
                    context.Category.Add(Category.GenerateCategory(EnumCategoryType.AiLvZiXun ));

                    // 创建最新服务（系统）类型
                    context.Category.Add(Category.GenerateCategory(EnumCategoryType.AiLvFuWu ));

                    // 创建最新服务（系统）类型
                    context.Category.Add(Category.GenerateCategory(EnumCategoryType.AiLvJiLu));


                    context.SaveChanges();
                }

                #endregion

                #region 检查系统的参数设置类，若当前还在初始化阶段，则按默认设置条件设定参数设置值

                var currentSettings = context.Setting.FirstOrDefault();
                if (currentSettings == null)
                {
                    #region 创建默认的系统设置参数



                    //TODO: please change the ForumURL
                    var settings = new Settings
                    {
                        ForumName = "MVCForum",
                        ForumUrl = "http://localhost:9666/",
                        IsClosed = false,
                        EnableRSSFeeds = true,
                        DisplayEditedBy = true,
                        EnablePostFileAttachments = false,
                        EnableMarkAsSolution = true,
                        EnableSpamReporting = true,
                        EnableMemberReporting = true,
                        EnableEmailSubscriptions = true,
                        ManuallyAuthoriseNewMembers = false,
                        EmailAdminOnNewMemberSignUp = true,
                        TopicsPerPage = 20,
                        PostsPerPage = 20,
                        EnablePrivateMessages = true,
                        MaxPrivateMessagesPerMember = 50,
                        PrivateMessageFloodControl = 1,
                        EnableSignatures = false,
                        EnablePoints = true,
                        PointsAllowedToVoteAmount = 1,
                        PointsAllowedForExtendedProfile = 1,
                        PointsAddedPerPost = 1,
                        PointsAddedForSolution = 4,
                        PointsDeductedNagativeVote = 2,
                        PointsAddedPostiveVote = 2,
                        AdminEmailAddress = "my@email.com",
                        NotificationReplyEmail = "noreply@myemail.com",
                        SMTPEnableSSL = false,
                        Theme = "Metro",
                        NewMemberStartingRole = standardRole,
                        DefaultLanguage = language,
                        ActivitiesPerPage = 20,
                        EnableAkisment = false,
                        EnableSocialLogins = false,
                        EnablePolls = true,
                        MarkAsSolutionReminderTimeFrame = 7,
                        EnableEmoticons = true,
                        DisableStandardRegistration = false
                    };

                    context.Setting.Add(settings);
                    context.SaveChanges();

                    #endregion
                }

                #endregion

                #region 建立系统的全局权限定义和局部权限定义
                // 检查是否有“Edit Posts”的权限作为依据
                if (context.Permission.FirstOrDefault(x => x.Name == SiteConstants.Instance.PermissionEditPosts) == null)
                {
                    #region 创建局部权限

                    //创建“Edit Posts”权限
                    var permission = new Permission { Name = SiteConstants.Instance.PermissionEditPosts };
                    context.Permission.Add(permission);

                    // NOTE: Because this is null - We assumed it's a new install so carry on checking and adding the other permissions

                    // 创建“Read Only”权限
                    if (context.Permission.FirstOrDefault(x => x.Name == SiteConstants.Instance.PermissionReadOnly) == null)
                    {
                        var p = new Permission { Name = SiteConstants.Instance.PermissionReadOnly };
                        context.Permission.Add(p);
                    }

                    // 创建“Delete Posts”权限
                    if (context.Permission.FirstOrDefault(x => x.Name == SiteConstants.Instance.PermissionDeletePosts) == null)
                    {
                        var p = new Permission { Name = SiteConstants.Instance.PermissionDeletePosts };
                        context.Permission.Add(p);
                    }

                    // 创建“Sticky Topics”权限
                    if (context.Permission.FirstOrDefault(x => x.Name == SiteConstants.Instance.PermissionCreateStickyTopics) ==
                        null)
                    {
                        var p = new Permission { Name = SiteConstants.Instance.PermissionCreateStickyTopics };
                        context.Permission.Add(p);
                    }

                    // 创建“Lock Topics”权限
                    if (context.Permission.FirstOrDefault(x => x.Name == SiteConstants.Instance.PermissionLockTopics) == null)
                    {
                        var p = new Permission { Name = SiteConstants.Instance.PermissionLockTopics };
                        context.Permission.Add(p);
                    }

                    // 创建“Vote In Polls”权限
                    if (context.Permission.FirstOrDefault(x => x.Name == SiteConstants.Instance.PermissionVoteInPolls) == null)
                    {
                        var p = new Permission { Name = SiteConstants.Instance.PermissionVoteInPolls };
                        context.Permission.Add(p);
                    }

                    // 创建“Create Polls”权限
                    if (context.Permission.FirstOrDefault(x => x.Name == SiteConstants.Instance.PermissionCreatePolls) == null)
                    {
                        var p = new Permission { Name = SiteConstants.Instance.PermissionCreatePolls };
                        context.Permission.Add(p);
                    }

                    // 创建“Create Topics”权限
                    if (context.Permission.FirstOrDefault(x => x.Name == SiteConstants.Instance.PermissionCreateTopics) == null)
                    {
                        var p = new Permission { Name = SiteConstants.Instance.PermissionCreateTopics };
                        context.Permission.Add(p);
                    }

                    // 创建“Attach Files”权限
                    if (context.Permission.FirstOrDefault(x => x.Name == SiteConstants.Instance.PermissionAttachFiles) == null)
                    {
                        var p = new Permission { Name = SiteConstants.Instance.PermissionAttachFiles };
                        context.Permission.Add(p);
                    }

                    // 创建“Deny Access”权限
                    if (context.Permission.FirstOrDefault(x => x.Name == SiteConstants.Instance.PermissionDenyAccess) == null)
                    {
                        var p = new Permission { Name = SiteConstants.Instance.PermissionDenyAccess };
                        context.Permission.Add(p);
                    }
                    #endregion

                    #region 全局权限

                    // 创建“Deny Access”全局权限
                    if (context.Permission.FirstOrDefault(x => x.Name == SiteConstants.Instance.PermissionEditMembers) == null)
                    {
                        var p = new Permission { Name = SiteConstants.Instance.PermissionEditMembers, IsGlobal = true };
                        context.Permission.Add(p);
                    }

                    // 创建“Insert Editor Images”全局权限
                    if (context.Permission.FirstOrDefault(x => x.Name == SiteConstants.Instance.PermissionInsertEditorImages) == null)
                    {
                        var p = new Permission { Name = SiteConstants.Instance.PermissionInsertEditorImages, IsGlobal = true };
                        context.Permission.Add(p);
                    }

                    #endregion

                    context.SaveChanges();
                }

                #endregion

                #region 若默认管理员账号不存在，则创建默认的系统管理员账号

                if (context.MembershipUser.FirstOrDefault(x => x.UserName == defaultAdminUsername) == null)
                {
                    #region  创建管理员账号并分配管理员角色
                    var admin = new MembershipUser
                    {
                        #region 基本属性赋值

                        #region 基本信息
                        UserName = defaultAdminUsername,
                        RealName = "默认管理员",
                        AliasName = "admin",
                        Email = "admin@email.com",
                        Gender = 1,
                        Birthday = new DateTime(2000, 1, 1),
                        IsLunarCalendar = false,
                        IsMarried = true,
                        Height = 180,
                        Weight = 100,
                        Education = "硕士",
                        Location = "深圳市",
                        SchoolProvince = "110000",
                        SchoolCity = "110100",
                        SchoolName = "我的大学",
                        HomeTownProvince = "110000",
                        HomeTownCity = "110100",
                        HomeTownCounty = "110108",
                        Job = "工程师",
                        IncomeRange = 0,
                        Interest = "发呆",
                        MobilePhone = "13686886937",
                        #endregion

                        Password = defaultAdminstratorPassword,
                        IsApproved = true,
                        CreateDate = DateTime.Now,
                        LastLoginDate = DateTime.Now,
                        LastUpdateTime = DateTime.Now,
                        UserType = 1,
                        Slug = ServiceHelpers.CreateUrl(defaultAdminUsername),
                        DisablePosting = false,
                        DisablePrivateMessages = false,
                        DisableFileUploads = false,
                        Comment = "系统管理员默认账号",
                        Signature = "",
                        Website = "",
                        Twitter = "",
                        Facebook = "",
                        Avatar = ""

                        #endregion
                    };
                    // Hash the password
                    var salt = StringUtils.CreateSalt(AppConstants.SaltSize);
                    var hash = StringUtils.GenerateSaltedHash(admin.Password, salt);
                    admin.Password = hash;
                    admin.PasswordSalt = salt;

                    // Put the admin in the admin role
                    admin.Roles = new List<MembershipRole> { adminRole };

                    context.MembershipUser.Add(admin);
                    context.SaveChanges();

                    #endregion

                    #region 生成“创建管理员”话题并发布内容

                    const string ReadmeTopicName = "Read Me";

                    #region 创建Readme的话题
                    //var category = context.Category.FirstOrDefault();
                    var SampleCategory = context.Category.Where(x => x.Name == "Example Category").SingleOrDefault();
                    var topic = new Topic
                    {
                        TopicType = "BasicTopic",
                        Category = SampleCategory,
                        CreateDate = DateTime.Now,
                        User = admin,
                        IsSticky = true,
                        Name = ReadmeTopicName,
                        Slug = ServiceHelpers.CreateUrl(ReadmeTopicName)
                    };

                    context.Topic.Add(topic);
                    context.SaveChanges();

                    #endregion

                    #region 创建Readme的公示帖子

                    const string readMeText = @"<h2>Administration</h2>
<p>We have auto created an admin user for you to manage the site</p>
<p>Username: <strong>admin</strong><br />Password: <strong>password</strong></p>
<p>Once you have logged in, you can manage the forum <a href=""/admin/"">through the admin section</a>. </p>
<p><strong><font color=""#ff0000"">Important:</font> </strong>Please update the admin password (and username) before putting this site live.</p>
<h2>Documentation</h2>
<p>We have documentation on Github in the WIKI</p>
<p><a href=""https://github.com/YodasMyDad/mvcforum/wiki"">https://github.com/YodasMyDad/mvcforum/wiki</a></p>
<p>You can also grab the source code from Github too.</p>";

                    var post = new Post
                    {
                        DateCreated = DateTime.Now,
                        DateEdited = DateTime.Now,
                        Topic = topic,
                        IsTopicStarter = true,
                        User = admin,
                        PostContent = readMeText,
                        SearchField = ReadmeTopicName
                    };

                    topic.LastPost = post;

                    context.Post.Add(post);
                    context.SaveChanges();

                    #endregion

                    #endregion

                }
                #endregion
            }
            else
            {
                #region 系统已上线，后续有其他的拓展数据需要加入时，请在此处编码

                #endregion
            }

            MyStopWatch.Stop();
            decimal t = MyStopWatch.ElapsedMilliseconds;

            logger.Debug(string.Format("timecost:{0} seconds, flag:{1}.", t / 1000, IsInitProcess.ToString()));

        }
    }
}
