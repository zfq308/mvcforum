using System;
using System.Collections.Generic;
using System.Configuration;
using System.Web;
using System.Web.Configuration;
using System.Web.Hosting;
using System.Xml;

namespace MVCForum.Utilities
{
    /// <summary>
    /// 配置工具类
    /// </summary>
    public static class ConfigUtils
    {
        /// <summary>
        /// WebConfig文件的IO路径
        /// </summary>
        private static string WebConfigPath
        {
            get { return HostingEnvironment.MapPath("~/web.config"); }
        }

        private static XmlDocument GetWebConfig()
        {
            var xDoc = new XmlDocument();
            xDoc.Load(WebConfigPath);
            return xDoc;
        }

        /// <summary>
        /// 取得Appsetting节点的值
        /// </summary>
        /// <param name="appSettingName">Appsetting节点的名字</param>
        /// <returns>App setting value</returns>
        public static string GetAppSetting(string appSettingName)
        {
            if (appSettingName.IsNullEmpty())
            {
                throw new ApplicationException("AppSetting is null");
            }
            return WebConfigurationManager.AppSettings[appSettingName];
        }

        /// <summary>
        /// 取得Appsetting节点的值，或当无此节点时返回默认值
        /// </summary>
        /// <param name="appSettingName">Appsetting节点的名字</param>
        /// <param name="defaultValue">默认值</param>
        /// <returns>App setting value</returns>
        public static string GetAppSetting(string appSettingName, string defaultValue)
        {
            var appValue = defaultValue;

            try
            {
                if (!appSettingName.IsNullEmpty())
                {
                    appValue = WebConfigurationManager.AppSettings[appSettingName];

                    if (appValue == null) // Not found in app settings
                    {
                        appValue = defaultValue;
                    }
                }
            }
            catch
            {
                // Empty - default value
            }

            return appValue;
        }

        /// <summary>
        /// Gets application setting and returns as Int32
        /// </summary>
        /// <param name="appSettingame"></param>
        /// <param name="defaultValue"></param>
        /// <returns>App Setting Value</returns>
        public static int GetAppSettingInt32(string appSettingame, int defaultValue)
        {
            return GetAppSetting(appSettingame, defaultValue.ToString()).ToInt32();
        }

        /// <summary>
        /// Updates an app setting in the config based on name
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public static bool UpdateAppSetting(string name, string value)
        {
            try
            {
                var config = WebConfigurationManager.OpenWebConfiguration("~/");
                config.AppSettings.Settings[name].Value = value;
                config.Save(ConfigurationSaveMode.Modified, false);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static bool InsertAppSetting(string name, string value)
        {
            try
            {
                var xDoc = GetWebConfig();

                var settingNodes = xDoc.GetElementsByTagName("appSettings")[0];

                var newSettingNode = xDoc.CreateElement("add");
                newSettingNode.SetAttribute("key", name);
                newSettingNode.SetAttribute("value", value);

                settingNodes.AppendChild(newSettingNode);
                xDoc.Save(WebConfigPath);
                return true;
            }
            catch (Exception)
            {
                return false;
            }            
        }

        public static bool InsertAppSetting(Dictionary<string, string> multipleSettings)
        {
            try
            {
                var xDoc = GetWebConfig();

                var settingNodes = xDoc.GetElementsByTagName("appSettings")[0];

                foreach (var dict in multipleSettings)
                {
                    var newSettingNode = xDoc.CreateElement("add");
                    newSettingNode.SetAttribute("key", dict.Key);
                    newSettingNode.SetAttribute("value", dict.Value);
                    settingNodes.AppendChild(newSettingNode);   
                }

                xDoc.Save(WebConfigPath);

                return true;
            }
            catch (Exception)
            {
                return false;
            }   
        }

        public static bool ChangeApplicationName(string newName, string currentAppName)
        {
            try
            {
                var xDoc = GetWebConfig();

                var membershipProviders = xDoc.GetElementsByTagName("membership")[0];
                var xpathToSetting = string.Format("//add[@name='{0}']", currentAppName);
                var provider = membershipProviders.SelectSingleNode(xpathToSetting);
                if (provider != null && provider.Attributes != null)
                {
                    var idAttribute = provider.Attributes["applicationName"];
                    if (idAttribute != null)
                    {
                        idAttribute.Value = newName;
                        xDoc.Save(WebConfigPath);
                        return true;
                    }
                }
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }


        #region 连接字符串操作
        /// <summary>
        /// 加入连接字符串
        /// </summary>
        /// <param name="name"></param>
        /// <param name="connectionString"></param>
        /// <param name="providerName"></param>
        /// <returns></returns>
        public static bool InsertConnectionString(string name, string connectionString, string providerName = "System.Data.SqlClient")
        {
            try
            {
                var xDoc = GetWebConfig();

                var connectionStrings = xDoc.GetElementsByTagName("connectionStrings")[0];

                var newSettingNode = xDoc.CreateElement("add");
                newSettingNode.SetAttribute("name", name);
                newSettingNode.SetAttribute("connectionString", connectionString);
                newSettingNode.SetAttribute("providerName", providerName);

                connectionStrings.AppendChild(newSettingNode);
                xDoc.Save(WebConfigPath);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
      
        /// <summary>
        /// 更新连接字符串
        /// </summary>
        /// <param name="name">连接字符串名字</param>
        /// <param name="value">连接字符串的内容值</param>
        /// <returns></returns>
        public static bool UpdateConnectionString(string name, string value)
        {
            try
            {
                var xDoc = GetWebConfig();
                var connectionStrings = xDoc.GetElementsByTagName("connectionStrings")[0];
                var xpathToConnString = string.Format("//add[@name='{0}']", name);
                var connectionstring = connectionStrings.SelectSingleNode(xpathToConnString);
                if (connectionstring != null && connectionstring.Attributes != null)
                {
                    var connectionStringAttribute = connectionstring.Attributes["connectionString"];
                    if (connectionStringAttribute != null)
                    {
                        connectionStringAttribute.Value = value;
                        xDoc.Save(WebConfigPath);
                        return true;
                    }
                }
                return false;

            }
            catch (Exception)
            {
                return false;
            }
        }
        #endregion

    

    }
}
