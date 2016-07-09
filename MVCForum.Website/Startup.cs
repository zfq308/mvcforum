using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(MVCForum.Website.Startup))]
[assembly: log4net.Config.XmlConfigurator(ConfigFile = "Web.config", Watch = true)]
namespace MVCForum.Website
{
    // 配置Log4Net
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            // 有关如何配置应用程序的详细信息，请访问 http://go.microsoft.com/fwlink/?LinkID=316888
        }
    }
}
