using System.Collections.Generic;

namespace MVCForum.Domain.Constants
{
    /// <summary>
    /// 常量定义类
    /// </summary>
    public static class AppConstants
    {
        public const int SaltSize = 24;
        public const string EditorType = "forumeditor";

        // Scheduled Tasks
        public const string DefaultTaskGroup = "MVCForumTaskGroup";

        // Cookie names
        public const string LanguageIdCookieName = "LanguageCulture";
        public const string MemberEmailConfirmationCookieName = "MVCForumEmailConfirmation";

        // Cache names
        public const string SettingsCacheName = "MainSettings";
        public const string LocalisationCacheName = "Localization-";
        public static string LanguageStrings = string.Concat(LocalisationCacheName, "LangStrings-");
        public const string MemberCacheName = "#member#-{0}";

        // View Bag / Temp Data Constants
        public const string MessageViewBagName = "Message";
        public const string DefaultCategoryViewBagName = "DefaultCategory";
        public const string GlobalClass = "GlobalClass";
        public const string CurrentAction = "CurrentAction";
        public const string CurrentController = "CurrentController";
        public const string MemberRegisterViewModel = "MemberRegisterViewModel";

        #region 系统角色定义

        // Main admin role [This should never be changed]
        public const string AdminRoleName = "Admin";

        // Main Supplier role [This should never be changed]
        public const string SupplierRoleName = "Supplier";

        // Main guest role [This should never be changed]
        // This is the role a non logged in user defaults to
        public const string GuestRoleName = "Guest";

        #endregion


        // Paging options
        public const string PagingUrlFormat = "{0}?p={1}";

        // How long 
        public const int TimeSpanInMinutesToShowMembers = 12;

        /// <summary>
        /// Last Activity Time Check. 
        /// </summary>
        public const int TimeSpanInMinutesToDoCheck = 3;


        public const string EditorTemplateColourPicker = "colourpicker";

        //Querystring names
        public const string PostOrderBy = "order";
        public const string AllPosts = "all";

        //Mobile Check Name
        public const string IsMobileDevice = "IsMobileDevice";


        public static List<string> ReflectionDllsToAvoid = new List<string>
        {
            "AjaxMin.dll",
            "AntiXssLibrary.dll",
            "Antlr3.Runtime.dll",
            "Common.Logging.Core.dll",
            "Common.Logging.dll",
            "DotNetOpenAuth",
            "EcmaScript.NET.dll",
            "EFCache.dll",
            "EntityFramework.dll",
            "EntityFramework.SqlServer.dll",
            "EntityFramework.SqlServerCompact.dll",
            "HtmlAgilityPack.dll",
            "HtmlSanitizationLibrary.dll",
            "Iesi.Collections.dll",
            "ImageProcessor.dll",
            "ImageProcessor.Web.dll",
            "log4net.dll",
            "Microsoft",
            "Microsoft.Owin.dll",
            "Microsoft.Practices.ServiceLocation.dll",
            "Microsoft.Practices.Unity.Configuration.dll",
            "Microsoft.Practices.Unity.dll",
            "Microsoft.Practices.Unity.RegistrationByConvention.dll",
            "Microsoft.Web.Infrastructure.dll",
            "Microsoft.Web.Services3.dll",
            "Newtonsoft.Json.dll",
            "Quartz.dll",
            "Quartz.Unity.45.dll",
            "Skybrud.Social.dll",
            "SquishIt.Framework.dll",
            "SquishIt.Mvc.dll",
            "System.Data.SqlServerCe.dll",
            "System.Net.Http.dll",
            "System.Net.Http.Formatting.dll",
            "System.Net.Http.WebRequest.dll",
            "System.Web.Helpers.dll",
            "System.Web.Http.dll",
            "System.Web.Http.WebHost.dll",
            "System.Web.Mvc.dll",
            "System.Web.Optimization.dll",
            "System.Web.Razor.dll",
            "System.Web.WebPages.Deployment.dll",
            "System.Web.WebPages.dll",
            "System.Web.WebPages.Razor.dll",
            "Unity.Mvc4.dll",
            "Unity.WebApi.dll",
            "Unity.WebApi.dll",
            "WebActivator.dll",
            "WebGrease.dll",
            "Yahoo.Yui.Compressor.dll",

        };

    }
}
