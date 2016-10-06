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
using MVCForum.Services;
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

            Stopwatch MyStopWatch = new Stopwatch(); //性能计时器

            log4net.ILog logger = log4net.LogManager.GetLogger("CoreActionLog");
            logger.Info("Start_Configuration.Seed().");

            MyStopWatch.Start(); //启动计时器

            var language = context.Language.FirstOrDefault(x => x.LanguageCulture == langCulture);
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

                #region 创建系统的Category实例

                Category Category_SampleCategory = null;
                Category Category_MeiRiXinqing = null;
                Category Category_AiLvZiXun = null;
                Category Category_AiLvFuWu = null;
                Category Category_AiLvJiLu = null;

                #region 添加Category实例

                if (!context.Category.Any())
                {
                    Category_SampleCategory = Category.GenerateCategory(EnumCategoryType.SampleCategory); //创建默认Sample类型
                    Category_MeiRiXinqing = Category.GenerateCategory(EnumCategoryType.MeiRiXinqing); //创建每日心情日记本（系统）类型
                    Category_AiLvZiXun = Category.GenerateCategory(EnumCategoryType.AiLvZiXun); // 创建最新资讯（系统）类型
                    Category_AiLvFuWu = Category.GenerateCategory(EnumCategoryType.AiLvFuWu); // 创建最新服务（系统）类型
                    Category_AiLvJiLu = Category.GenerateCategory(EnumCategoryType.AiLvJiLu);// 创建爱驴记录（系统）类型

                    context.Category.Add(Category_SampleCategory);
                    context.Category.Add(Category_MeiRiXinqing);
                    context.Category.Add(Category_AiLvZiXun);
                    context.Category.Add(Category_AiLvFuWu);
                    context.Category.Add(Category_AiLvJiLu);

                    context.SaveChanges();
                }
                #endregion

                #endregion

                #region 检查系统的参数设置类，若当前还在初始化阶段，则按默认设置条件设定参数设置值

                var currentSettings = context.Setting.FirstOrDefault();
                if (currentSettings == null)
                {
                    #region 创建默认的系统设置参数
                    var settings = new Settings
                    {
                        ForumName = "爱驴网，爱驴户外",
                        ForumUrl = "http://www.ailvlove.com/",
                        ForumKeepAliveURL = "http://www.ailvlove.com/",
                        IsClosed = false,
                        DisplayEditedBy = true,
                        EnablePostFileAttachments = false,

                        EnableSpamReporting = true,
                        EnableMemberReporting = true,
                        ManuallyAuthoriseNewMembers = true,
                        TopicsPerPage = 10,
                        PostsPerPage = 10,
                        EnableEmoticons = true,
                        EnablePrivateMessages = true,
                        MaxPrivateMessagesPerMember = 200,
                        PrivateMessageFloodControl = 1,
                        Theme = "Metro",
                        NewMemberStartingRole = standardRole,
                        DisableStandardRegistration = false,
                        DefaultLanguage = language,
                        ActivitiesPerPage = 10,

                        UCPaasConfig_Account = "4c890bb2b861ac9eac78d381efea6cb2",
                        UCPaasConfig_Token = "29755cc149863eced52be76f3f6bcebf",
                        UCPaasConfig_AppId = "3b77d120b23c4d5bb3d38e5e01868550",
                        UCPaasConfig_TemplatedId = "26343",


                        #region 非关键设置项

                        EnableRSSFeeds = false,
                        EnableEmailSubscriptions = false,
                        EmailAdminOnNewMemberSignUp = false,
                        EnableSignatures = false,
                        EnablePoints = false,

                        EnableSocialLogins = false,
                        EnablePolls = false,
                        PointsAllowedToVoteAmount = 1,
                        PointsAllowedForExtendedProfile = 1,
                        PointsAddedPerPost = 1,
                        PointsAddedForSolution = 4,
                        PointsDeductedNagativeVote = 2,
                        PointsAddedPostiveVote = 2,
                        AdminEmailAddress = "zfq3082002@163.com",
                        NotificationReplyEmail = "noreply@myemail.com",
                        SMTPEnableSSL = false,
                        EnableMarkAsSolution = false,
                        MarkAsSolutionReminderTimeFrame = 7,
                        EnableAkisment = false,
                        AkismentKey = "0a186c65abb7"

                        #endregion

                    };

                    context.Setting.Add(settings);
                    context.SaveChanges();

                    #endregion
                }

                #endregion

                #region 建立系统的全局权限定义和局部权限定义

                Permission Permission_EditPosts = null;
                Permission Permission_ReadOnly = null;
                Permission Permission_DeletePosts = null;
                Permission Permission_CreateStickyTopics = null;
                Permission Permission_LockTopics = null;
                Permission Permission_VoteInPolls = null;
                Permission Permission_CreatePolls = null;
                Permission Permission_CreateTopics = null;
                Permission Permission_AttachFiles = null;
                Permission Permission_DenyAccess = null;


                Permission Permission_Global_EditMembers = null;
                Permission Permission_Global_InsertEditorImages = null;

                #region 创建局部权限

                //创建“Edit Posts”权限
                if (context.Permission.FirstOrDefault(x => x.Name == SiteConstants.Instance.PermissionEditPosts) == null)
                {
                    Permission_EditPosts = new Permission { Name = SiteConstants.Instance.PermissionEditPosts };
                    context.Permission.Add(Permission_EditPosts);
                }

                // 创建“Read Only”权限
                if (context.Permission.FirstOrDefault(x => x.Name == SiteConstants.Instance.PermissionReadOnly) == null)
                {
                    Permission_ReadOnly = new Permission { Name = SiteConstants.Instance.PermissionReadOnly };
                    context.Permission.Add(Permission_ReadOnly);
                }

                // 创建“Delete Posts”权限
                if (context.Permission.FirstOrDefault(x => x.Name == SiteConstants.Instance.PermissionDeletePosts) == null)
                {
                    Permission_DeletePosts = new Permission { Name = SiteConstants.Instance.PermissionDeletePosts };
                    context.Permission.Add(Permission_DeletePosts);
                }

                // 创建“Sticky Topics”权限
                if (context.Permission.FirstOrDefault(x => x.Name == SiteConstants.Instance.PermissionCreateStickyTopics) == null)
                {
                    Permission_CreateStickyTopics = new Permission { Name = SiteConstants.Instance.PermissionCreateStickyTopics };
                    context.Permission.Add(Permission_CreateStickyTopics);
                }

                // 创建“Lock Topics”权限
                if (context.Permission.FirstOrDefault(x => x.Name == SiteConstants.Instance.PermissionLockTopics) == null)
                {
                    Permission_LockTopics = new Permission { Name = SiteConstants.Instance.PermissionLockTopics };
                    context.Permission.Add(Permission_LockTopics);
                }

                // 创建“Vote In Polls”权限
                if (context.Permission.FirstOrDefault(x => x.Name == SiteConstants.Instance.PermissionVoteInPolls) == null)
                {
                    Permission_VoteInPolls = new Permission { Name = SiteConstants.Instance.PermissionVoteInPolls };
                    context.Permission.Add(Permission_VoteInPolls);
                }

                // 创建“Create Polls”权限
                if (context.Permission.FirstOrDefault(x => x.Name == SiteConstants.Instance.PermissionCreatePolls) == null)
                {
                    Permission_CreatePolls = new Permission { Name = SiteConstants.Instance.PermissionCreatePolls };
                    context.Permission.Add(Permission_CreatePolls);
                }

                // 创建“Create Topics”权限
                if (context.Permission.FirstOrDefault(x => x.Name == SiteConstants.Instance.PermissionCreateTopics) == null)
                {
                    Permission_CreateTopics = new Permission { Name = SiteConstants.Instance.PermissionCreateTopics };
                    context.Permission.Add(Permission_CreateTopics);
                }

                // 创建“Attach Files”权限
                if (context.Permission.FirstOrDefault(x => x.Name == SiteConstants.Instance.PermissionAttachFiles) == null)
                {
                    Permission_AttachFiles = new Permission { Name = SiteConstants.Instance.PermissionAttachFiles };
                    context.Permission.Add(Permission_AttachFiles);
                }

                // 创建“Deny Access”权限
                if (context.Permission.FirstOrDefault(x => x.Name == SiteConstants.Instance.PermissionDenyAccess) == null)
                {
                    Permission_DenyAccess = new Permission { Name = SiteConstants.Instance.PermissionDenyAccess };
                    context.Permission.Add(Permission_DenyAccess);
                }
                #endregion

                #region 全局权限

                // 创建“Deny Access”全局权限
                if (context.Permission.FirstOrDefault(x => x.Name == SiteConstants.Instance.PermissionEditMembers) == null)
                {
                    Permission_Global_EditMembers = new Permission { Name = SiteConstants.Instance.PermissionEditMembers, IsGlobal = true };
                    context.Permission.Add(Permission_Global_EditMembers);
                }

                // 创建“Insert Editor Images”全局权限
                if (context.Permission.FirstOrDefault(x => x.Name == SiteConstants.Instance.PermissionInsertEditorImages) == null)
                {
                    Permission_Global_InsertEditorImages = new Permission { Name = SiteConstants.Instance.PermissionInsertEditorImages, IsGlobal = true };
                    context.Permission.Add(Permission_Global_InsertEditorImages);
                }

                #endregion

                context.SaveChanges();


                #endregion

                #region 建立Category，Permission，Role映射关联关系

                CategoryPermissionForRoleService cprs = new CategoryPermissionForRoleService(context);


                #region 普通用户在每日心情模块的权限设定
                cprs.Add(new CategoryPermissionForRole()
                {
                    Category = Category_MeiRiXinqing,
                    MembershipRole = standardRole,
                    Permission = Permission_CreateTopics,
                    IsTicked = true,
                });

                cprs.Add(new CategoryPermissionForRole()
                {
                    Category = Category_MeiRiXinqing,
                    MembershipRole = standardRole,
                    Permission = Permission_AttachFiles,
                    IsTicked = true,
                });

                cprs.Add(new CategoryPermissionForRole()
                {
                    Category = Category_MeiRiXinqing,
                    MembershipRole = standardRole,
                    Permission = Permission_Global_InsertEditorImages,
                    IsTicked = true,
                });
                #endregion

                #region 供 应 商 在每日心情模块的权限设定

                cprs.Add(new CategoryPermissionForRole()
                {
                    Category = Category_MeiRiXinqing,
                    MembershipRole = SupplierRole,
                    Permission = Permission_CreateTopics,
                    IsTicked = true,
                });

                cprs.Add(new CategoryPermissionForRole()
                {
                    Category = Category_MeiRiXinqing,
                    MembershipRole = SupplierRole,
                    Permission = Permission_AttachFiles,
                    IsTicked = true,
                });

                cprs.Add(new CategoryPermissionForRole()
                {
                    Category = Category_MeiRiXinqing,
                    MembershipRole = SupplierRole,
                    Permission = Permission_Global_InsertEditorImages,
                    IsTicked = true,
                });

                #endregion

                #region 供应商在爱驴记录模块的权限设定

                cprs.Add(new CategoryPermissionForRole()
                {
                    Category = Category_AiLvJiLu,
                    MembershipRole = SupplierRole,
                    Permission = Permission_CreateTopics,
                    IsTicked = true,
                });

                cprs.Add(new CategoryPermissionForRole()
                {
                    Category = Category_AiLvJiLu,
                    MembershipRole = SupplierRole,
                    Permission = Permission_AttachFiles,
                    IsTicked = true,
                });

                cprs.Add(new CategoryPermissionForRole()
                {
                    Category = Category_AiLvJiLu,
                    MembershipRole = SupplierRole,
                    Permission = Permission_Global_InsertEditorImages,
                    IsTicked = true,
                });

                #endregion

                //cprs.Add(new CategoryPermissionForRole()
                //{
                //    Category = Category_AiLvZiXun,
                //    MembershipRole = adminRole,
                //    Permission = Permission_Global_InsertEditorImages,
                //    IsTicked = true,
                //});

                context.SaveChanges();

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
                        RealName = "系统管理员",
                        AliasName = "admin",
                        Email = "admin@email.com",
                        Gender = Enum_Gender.boy,
                        Birthday = new DateTime(2000, 1, 1),
                        IsLunarCalendar = Enum_Calendar.PublicCalendar,
                        IsMarried = Enum_MarriedStatus.Married,
                        Height = 180,
                        Weight = 100,
                        Education = "5",
                        HomeTown = "深圳市龙华新区",
                        SchoolProvince = "110000",
                        SchoolCity = "110100",
                        SchoolName = "我的大学",
                        LocationProvince = "110000",
                        LocationCity = "110100",
                        LocationCounty = "110108",
                        Job = "BigBoss",
                        IncomeRange = Enum_IncomeRange.R_1WTo5W,
                        Interest = "发呆",
                        MobilePhone = "13686886937",
                        Avatar = "",
                        #endregion

                        Password = defaultAdminstratorPassword,
                        IsApproved = true,  //管理员默认为审核通过状态
                        CreateDate = DateTime.Now,
                        LastLoginDate = DateTime.Now,
                        LastUpdateTime = DateTime.Now,
                        UserType = Enum_UserType.A,
                        Slug = ServiceHelpers.CreateUrl(defaultAdminUsername),
                        DisablePosting = false,
                        DisablePrivateMessages = false,
                        DisableFileUploads = false,
                        Comment = "系统管理员默认账号"


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
                        TopicType = Enum_TopicType.Announcement,
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

            logger.Info(string.Format("Configuration.Seed() execution completed.Timecost:{0} seconds, IsInitProcess:{1}.", t / 1000, IsInitProcess.ToString()));

        }
    }
}
