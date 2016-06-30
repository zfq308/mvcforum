using System;
using System.Web;

namespace MVCForum.Website
{

    public class JpgHandler : IHttpHandler
    {

        public void ProcessRequest(HttpContext context)
        {
            // 获取文件服务器端物理路径
            var fileName = context.Server.MapPath(context.Request.FilePath);

            // 如果UrlReferrer为空，则显示一张默认的禁止盗链的图片

            if (context.Request.UrlReferrer.Host == null)
            {
                context.Response.ContentType = "image/JPEG";
                context.Response.WriteFile("/error.jpg");
            }
            else
            {
                // 如果 UrlReferrer中不包含自己站点主机域名，则显示一张默认的禁止盗链的图片
                var host = context.Request.UrlReferrer.Host.ToLower();
                if (host.IndexOf("ailvlove.com", StringComparison.Ordinal) > 0 || host.IndexOf("localhost", StringComparison.Ordinal) > 0)
                {
                    context.Response.ContentType = "image/JPEG";
                    context.Response.WriteFile(fileName);
                }
                else
                {
                    context.Response.ContentType = "image/JPEG";
                    context.Response.WriteFile("/error.jpg");
                }
            }
        }

        public bool IsReusable
        {
            get { return true; }
        }
    }
}