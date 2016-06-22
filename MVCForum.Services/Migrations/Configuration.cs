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
        /// ��ʼ��ϵͳ��װ����
        /// </summary>
        /// <param name="context"></param>
        protected override void Seed(MVCForumContext context)
        {
            const string langCulture = "zh-CN";   // ϵͳĬ��ʹ�õ�����
            const string defaultAdminUsername = "admin";  //ϵͳĬ�Ϲ���Ա�˺�����
            const string defaultAdminstratorPassword = "password"; //ϵͳĬ�Ϲ���Ա�˺�����

            log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
            logger.Debug("Start Configuration.Seed.");

            Stopwatch MyStopWatch = new Stopwatch();
            MyStopWatch.Start();


            logger.Debug("Start to get Language.");
            var language = context.Language.FirstOrDefault(x => x.LanguageCulture == langCulture);
            logger.Debug("Stop to get Language.");
            bool IsInitProcess = (language == null);  // ��Ĭ�������Ƿ񱻰�װ��Ϊ�Ƿ��ʼ��������

            if (IsInitProcess)
            {
                #region д��ϵͳĬ�ϵ����Զ���

                var cultureInfo = LanguageUtils.GetCulture(langCulture);
                language = new Language
                {
                    Name = cultureInfo.EnglishName,
                    LanguageCulture = cultureInfo.Name,
                };
                context.Language.Add(language);
                context.SaveChanges();

                #endregion

                #region ����Ĭ�ϵ����Ա��ػ���Դ�ļ���������д�����ݿ�

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

                    // ���汾�ػ���������Ŀ
                    context.SaveChanges();
                }

                #endregion

                #region ����ϵͳ��Ĭ�����ֽ�ɫ
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

                #region ����һ��Sample ���Ͷ����ϵͳ�Դ�������ϵͳ������

                if (!context.Category.Any())
                {
                    #region ����Ĭ��Sample����
                    const string exampleCatName = "Example Category";
                    var exampleCat = new Category
                    {
                        Name = exampleCatName,
                        ModeratePosts = false,
                        ModerateTopics = false,
                        Slug = ServiceHelpers.CreateUrl(exampleCatName),
                        DateCreated = DateTime.Now,
                        IsSystemCategory = false,
                        ShowTheCategoryCondition = 0
                    };
                    context.Category.Add(exampleCat);
                    #endregion

                    #region ����ÿ�������ռǱ���ϵͳ������
                    const string DailyRecordCatName = "Sys_DailyRecord";
                    var DailyRecordCat = new Category
                    {
                        Name = DailyRecordCatName,
                        ModeratePosts = false,
                        ModerateTopics = false,
                        Slug = ServiceHelpers.CreateUrl(DailyRecordCatName),
                        DateCreated = DateTime.Now,

                        IsSystemCategory = true,
                        ShowTheCategoryCondition = 0
                    };
                    context.Category.Add(DailyRecordCat);

                    #endregion

                    #region ����������Ѷ��ϵͳ������
                    const string LatestNewsCatName = "Sys_LatestNews";
                    var LatestNewsCat = new Category
                    {
                        Name = LatestNewsCatName,
                        ModeratePosts = false,
                        ModerateTopics = false,
                        Slug = ServiceHelpers.CreateUrl(LatestNewsCatName),
                        DateCreated = DateTime.Now,

                        IsSystemCategory = true,
                        ShowTheCategoryCondition = 0
                    };
                    context.Category.Add(LatestNewsCat);

                    #endregion

                    #region ����������Ϣ��ϵͳ������
                    const string ProvideServiceCatName = "Sys_ProvideService";
                    var ProvideServiceCat = new Category
                    {
                        Name = ProvideServiceCatName,
                        ModeratePosts = false,
                        ModerateTopics = false,
                        Slug = ServiceHelpers.CreateUrl(ProvideServiceCatName),
                        DateCreated = DateTime.Now,

                        IsSystemCategory = true,
                        ShowTheCategoryCondition = 0
                    };
                    context.Category.Add(ProvideServiceCat);

                    #endregion

                    context.SaveChanges();
                }

                #endregion

                #region ���ϵͳ�Ĳ��������࣬����ǰ���ڳ�ʼ���׶Σ���Ĭ�����������趨��������ֵ

                var currentSettings = context.Setting.FirstOrDefault();
                if (currentSettings == null)
                {
                    #region ����Ĭ�ϵ�ϵͳ���ò���



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

                #region ����ϵͳ��ȫ��Ȩ�޶���;ֲ�Ȩ�޶���
                // ����Ƿ��С�Edit Posts����Ȩ����Ϊ����
                if (context.Permission.FirstOrDefault(x => x.Name == SiteConstants.Instance.PermissionEditPosts) == null)
                {
                    #region �����ֲ�Ȩ��

                    //������Edit Posts��Ȩ��
                    var permission = new Permission { Name = SiteConstants.Instance.PermissionEditPosts };
                    context.Permission.Add(permission);

                    // NOTE: Because this is null - We assumed it's a new install so carry on checking and adding the other permissions

                    // ������Read Only��Ȩ��
                    if (context.Permission.FirstOrDefault(x => x.Name == SiteConstants.Instance.PermissionReadOnly) == null)
                    {
                        var p = new Permission { Name = SiteConstants.Instance.PermissionReadOnly };
                        context.Permission.Add(p);
                    }

                    // ������Delete Posts��Ȩ��
                    if (context.Permission.FirstOrDefault(x => x.Name == SiteConstants.Instance.PermissionDeletePosts) == null)
                    {
                        var p = new Permission { Name = SiteConstants.Instance.PermissionDeletePosts };
                        context.Permission.Add(p);
                    }

                    // ������Sticky Topics��Ȩ��
                    if (context.Permission.FirstOrDefault(x => x.Name == SiteConstants.Instance.PermissionCreateStickyTopics) ==
                        null)
                    {
                        var p = new Permission { Name = SiteConstants.Instance.PermissionCreateStickyTopics };
                        context.Permission.Add(p);
                    }

                    // ������Lock Topics��Ȩ��
                    if (context.Permission.FirstOrDefault(x => x.Name == SiteConstants.Instance.PermissionLockTopics) == null)
                    {
                        var p = new Permission { Name = SiteConstants.Instance.PermissionLockTopics };
                        context.Permission.Add(p);
                    }

                    // ������Vote In Polls��Ȩ��
                    if (context.Permission.FirstOrDefault(x => x.Name == SiteConstants.Instance.PermissionVoteInPolls) == null)
                    {
                        var p = new Permission { Name = SiteConstants.Instance.PermissionVoteInPolls };
                        context.Permission.Add(p);
                    }

                    // ������Create Polls��Ȩ��
                    if (context.Permission.FirstOrDefault(x => x.Name == SiteConstants.Instance.PermissionCreatePolls) == null)
                    {
                        var p = new Permission { Name = SiteConstants.Instance.PermissionCreatePolls };
                        context.Permission.Add(p);
                    }

                    // ������Create Topics��Ȩ��
                    if (context.Permission.FirstOrDefault(x => x.Name == SiteConstants.Instance.PermissionCreateTopics) == null)
                    {
                        var p = new Permission { Name = SiteConstants.Instance.PermissionCreateTopics };
                        context.Permission.Add(p);
                    }

                    // ������Attach Files��Ȩ��
                    if (context.Permission.FirstOrDefault(x => x.Name == SiteConstants.Instance.PermissionAttachFiles) == null)
                    {
                        var p = new Permission { Name = SiteConstants.Instance.PermissionAttachFiles };
                        context.Permission.Add(p);
                    }

                    // ������Deny Access��Ȩ��
                    if (context.Permission.FirstOrDefault(x => x.Name == SiteConstants.Instance.PermissionDenyAccess) == null)
                    {
                        var p = new Permission { Name = SiteConstants.Instance.PermissionDenyAccess };
                        context.Permission.Add(p);
                    }
                    #endregion

                    #region ȫ��Ȩ��

                    // ������Deny Access��ȫ��Ȩ��
                    if (context.Permission.FirstOrDefault(x => x.Name == SiteConstants.Instance.PermissionEditMembers) == null)
                    {
                        var p = new Permission { Name = SiteConstants.Instance.PermissionEditMembers, IsGlobal = true };
                        context.Permission.Add(p);
                    }

                    // ������Insert Editor Images��ȫ��Ȩ��
                    if (context.Permission.FirstOrDefault(x => x.Name == SiteConstants.Instance.PermissionInsertEditorImages) == null)
                    {
                        var p = new Permission { Name = SiteConstants.Instance.PermissionInsertEditorImages, IsGlobal = true };
                        context.Permission.Add(p);
                    }

                    #endregion

                    context.SaveChanges();
                }

                #endregion

                #region ��Ĭ�Ϲ���Ա�˺Ų����ڣ��򴴽�Ĭ�ϵ�ϵͳ����Ա�˺�

                if (context.MembershipUser.FirstOrDefault(x => x.UserName == defaultAdminUsername) == null)
                {
                    #region  ��������Ա�˺Ų��������Ա��ɫ
                    var admin = new MembershipUser
                    {
                        #region �������Ը�ֵ

                        #region ������Ϣ
                        UserName = defaultAdminUsername,
                        RealName = "Ĭ�Ϲ���Ա",
                        AliasName="admin",
                        Email = "admin@email.com",
                        Gender = 1,
                        Birthday = new DateTime(2000, 1, 1),
                        IsLunarCalendar = false,
                        IsMarried = true,
                        Height = 180,
                        Weight = 100,
                        Education = "˶ʿ",
                        Location = "������",
                        SchoolProvince = "110000",
                        SchoolCity = "110100",
                        SchoolName = "�ҵĴ�ѧ",
                        HomeTownProvince = "110000",
                        HomeTownCity = "110100",
                        HomeTownCounty = "110108",
                        Job = "����ʦ",
                        IncomeRange = 0,
                        Interest = "����",
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
                        Comment = "ϵͳ����ԱĬ���˺�",
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

                    #region ���ɡ���������Ա�����Ⲣ��������

                    const string ReadmeTopicName = "Read Me";

                    #region ����Readme�Ļ���
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

                    #region ����Readme�Ĺ�ʾ����

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
                #region ϵͳ�����ߣ���������������չ������Ҫ����ʱ�����ڴ˴�����

                #endregion
            }

            MyStopWatch.Stop();
            decimal t = MyStopWatch.ElapsedMilliseconds;

            logger.Debug(string.Format("timecost:{0} seconds, flag:{1}.", t / 1000, IsInitProcess.ToString()));
           
        }
    }
}
