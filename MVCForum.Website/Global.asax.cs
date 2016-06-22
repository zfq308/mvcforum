﻿using System;
using System.Globalization;
using System.Threading;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using MVCForum.Domain.Events;
using MVCForum.Domain.Interfaces.Services;
using MVCForum.Domain.Interfaces.UnitOfWork;
using MVCForum.IOC;
using MVCForum.Utilities;
using MVCForum.Website.Application;
using MVCForum.Website.Application.ScheduledJobs;
using MVCForum.Website.Application.ViewEngine;
using System.Web.Helpers;
using System.IO;

namespace MVCForum.Website
{
    public class MvcApplication : HttpApplication
    {

        public IUnitOfWorkManager UnitOfWorkManager => ServiceFactory.Get<IUnitOfWorkManager>();
        public IBadgeService BadgeService => ServiceFactory.Get<IBadgeService>();
        public ISettingsService SettingsService => ServiceFactory.Get<ISettingsService>();
        public ILoggingService LoggingService => ServiceFactory.Get<ILoggingService>();
        public ILocalizationService LocalizationService => ServiceFactory.Get<ILocalizationService>();
        public IReflectionService ReflectionService => ServiceFactory.Get<IReflectionService>();


        protected void Application_Start()
        {
            log4net.Config.XmlConfigurator.Configure(new FileInfo(Server.MapPath("~/Web.config")));

            log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

            AreaRegistration.RegisterAllAreas();
            logger.Info("Run AreaRegistration.RegisterAllAreas, done.");

            GlobalConfiguration.Configure(WebApiConfig.Register);
            logger.Info("Run GlobalConfiguration.Configure, done.");

            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            logger.Info("Run FilterConfig.RegisterGlobalFilters, done.");

            // https://msdn.microsoft.com/zh-cn/library/system.web.helpers.antiforgeryconfig.suppressidentityheuristicchecks.aspx?cs-save-lang=1&cs-lang=fsharp
            // 获取或设置一个值，该值可指示防伪系统是否应跳过检查指示系统滥用的条件。如果防伪系统不应检查可能的滥用，则为 true；否则为 false。
            AntiForgeryConfig.SuppressIdentityHeuristicChecks = true;

            // Start unity
            var unityContainer = UnityHelper.Start();
            logger.Info("Start unityContainer, Done.");
            // Routes
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            // Store the value for use in the app
            Application["Version"] = AppHelpers.GetCurrentVersionNo();

            // If the same carry on as normal
            LoggingService.Initialise(ConfigUtils.GetAppSettingInt32("LogFileMaxSizeBytes", 10000));
            LoggingService.Error("Application_Start");

            logger.Info("LoggingService recording completed.");

            #region TODO: Benjamin, 可考虑取消这一段“反射badges系列组件”的代码

            // Get assemblies for badges, events etc...
            var loadedAssemblies = ReflectionService.GetAssemblies();
            logger.Info("Loaded Assemblies completed.");
            // Do the badge processing
            using (var unitOfWork = UnitOfWorkManager.NewUnitOfWork())
            {
                try
                {
                    BadgeService.SyncBadges(loadedAssemblies);
                    unitOfWork.Commit();
                }
                catch (Exception ex)
                {
                    LoggingService.Error($"Error processing badge classes: {ex.Message}");
                }
            }
            logger.Debug("BadgeService.SyncBadges completed.");

            #endregion

            // Set the view engine
            ViewEngines.Engines.Clear();
            ViewEngines.Engines.Add(new ForumViewEngine(SettingsService.GetSettings().Theme));

            // Initialise the events
            EventManager.Instance.Initialize(LoggingService, loadedAssemblies);

            // Finally Run scheduled tasks
            ScheduledRunner.Run(unityContainer);
        }

        protected void Application_AcquireRequestState(object sender, EventArgs e)
        {
            //It's important to check whether session object is ready
            if (HttpContext.Current.Session != null)
            {
                // Set the culture per request
                var ci = new CultureInfo(LocalizationService.CurrentLanguage.LanguageCulture);
                Thread.CurrentThread.CurrentUICulture = ci;
                Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture(ci.Name);
            }
        }

        protected void Application_Error(object sender, EventArgs e)
        {
            var lastError = Server.GetLastError();
            if (lastError is HttpAntiForgeryException)
            {
                Response.Clear();
                Server.ClearError(); //make sure you log the exception first
                Response.Redirect("/error/antiforgery", true);
            }

            // Don't flag missing pages or changed urls, as just clogs up the log
            if (!lastError.Message.Contains("was not found or does not implement IController"))
            {
                LoggingService.Error(lastError);
            }
        }
    }
}