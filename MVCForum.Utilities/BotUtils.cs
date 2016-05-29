using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MVCForum.Utilities
{
    /// <summary>
    /// 搜索引擎工具类
    /// </summary>
    public static class BotUtils
    {
        /// <summary>
        /// 判断当前用户的请求是否来源于外缘网址的爬虫机器人
        /// </summary>
        /// <returns></returns>
        public static bool UserIsBot()
        {
            if (HttpContext.Current.Request.UserAgent != null)
            {
                var userAgent = HttpContext.Current.Request.UserAgent.ToLower();
                var botKeywords = new List<string> { "bot", "spider", "google", "yahoo", "search", "crawl", "slurp", "msn", "teoma", "ask.com", "bing", "accoona" };
                return botKeywords.Any(userAgent.Contains);
            }
            return true;
        }
    }
}
