一些常见的MVC功能
1. 输出一个Partial区块到前台页面

2. 调到用户账户编辑页
 <a href="@Url.Action("Edit", "Members", new { id = Model.CurrentUser.Id })">@Html.LanguageString("Buttons.Edit") @Model.CurrentUser.UserName</a>



 ========================MiniProfiler的使用方法=========================================================
 1. 从Nuget中依次加入： MiniProfiler， MiniProfiler.EF6，MiniProfiler.Mvc4
 2. 在shared/_Layout.cshtml文件中：
 （1）在最顶上加入：
 @using StackExchange.Profiling
 （2）在</body>标签前加入：
   @MiniProfiler.RenderIncludes()   
</body>

3.在Global.asax.cs 文件中
（1）.加入
using StackExchange.Profiling;
using StackExchange.Profiling.EntityFramework6;
(2).在 protected void Application_Start()方法的EF操作指令前，加入：
 protected void Application_Start()
{
    AreaRegistration.RegisterAllAreas();
    GlobalConfiguration.Configure(WebApiConfig.Register);
    FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);

    //此行代码要加在所有EF操作之前
    MiniProfilerEF6.Initialize();
...
}
(3) 加入两个新的事件处理方法
protected void Application_BeginRequest()
{
    if (Request.IsLocal)
    {
        MiniProfiler.Start();
    }
}
protected void Application_EndRequest()
{
    MiniProfiler.Stop();
}

4. 在web.config文件的<system.webServer>节点下的   <handlers>节点加入：
   <add name="MiniProfiler" path="mini-profiler-resources/*" verb="*" type="System.Web.Routing.UrlRoutingModule" resourceType="Unspecified" preCondition="integratedMode" />
 如下所示：
<system.webServer>
......
    <handlers>
      <remove name="ExtensionlessUrlHandler-Integrated-4.0" />
      <remove name="OPTIONSVerbHandler" />
      <remove name="TRACEVerbHandler" />
      <add name="ExtensionlessUrlHandler-Integrated-4.0" path="*." verb="*" type="System.Web.Handlers.TransferRequestHandler" preCondition="integratedMode,runtimeVersionv4.0" />
      <add name="MiniProfiler" path="mini-profiler-resources/*" verb="*" type="System.Web.Routing.UrlRoutingModule" resourceType="Unspecified" preCondition="integratedMode" />
    </handlers>
......
</system.webServer>

5.其他地方无需人为操作，系统会自动处理。
========================================================================================================



===================================增加一张新的数据表操作步骤===============================================
1. 在MVCForum.Domain/DomainModel/Entities目录下加入新的实体类的定义，其格式为

#region	
using System;
using System.Linq;
using System.Collections.Generic;
using MVCForum.Domain.DomainModel.Entities;
using MVCForum.Utilities;
using System.Data.SqlTypes;

namespace MVCForum.Domain.DomainModel
{

    /// <summary>
    /// XXXXXX的设定类
    /// </summary>
    public partial class XXXXXX : Entity
    {
        public XXXXXX()
        {
            Id = GuidComb.GenerateComb();
        }
        /// <summary>
        /// 流水号
        /// </summary>
        public Guid Id { get; set; }

		//public string P1 { get; set; }
		//public int P2 { get; set; }
		//public DateTime P3 { get; set; }
	}

}
#endregion

2. 在MVCForum.Domain/Interfaces/Services目录下加入新的Service接口类，其格式为

using System;
using System.Linq;
using System.Collections.Generic;
using MVCForum.Domain.DomainModel;
using MVCForum.Domain.Interfaces.UnitOfWork;

namespace MVCForum.Domain.Interfaces.Services
{

    public partial interface IXXXXXXService
    {
        XXXXXX Add(XXXXXX newinfo);
        XXXXXX Get(Guid id);
        IList<XXXXXX> GetAll();
        bool Delete(XXXXXX ad, IUnitOfWork unitOfWork);
    }
}

3. 在MVCForum.Services/Data/Mapping目录下加入新的实体数据映射类， 其格式为

using System;
using System.Linq;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using MVCForum.Domain.DomainModel;

namespace MVCForum.Services.Data.Mapping
{
    public class XXXXXXMapping : EntityTypeConfiguration<XXXXXX>
    {
        public XXXXXXMapping()
        {
            HasKey(x => x.Id);
            Property(x => x.Id).IsRequired();
            //Property(x => x.ImageName).IsRequired().HasMaxLength(100);
            //Property(x => x.ImageOriginName).IsRequired().HasMaxLength(100);
            //Property(x => x.Link).IsRequired().HasMaxLength(300);
            //Property(x => x.CreateTime).IsRequired();
        }
    }
}

4. 在MVCForum.Services/Data/Context/MVCForumContext.cs. 在其中加入新的数据表对应属性,其格式为：

 public DbSet<XXXXXX> XXXXXX { get; set; }

5. 在MVCForum.Services目录下实现新的Service接口类。其格式为：

using System;
using System.Linq;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Data.Entity;
using System.Text;
using System.Web;
using System.Web.Hosting;
using System.Web.Security;
using MVCForum.Domain.Constants;
using MVCForum.Domain.DomainModel;
using MVCForum.Domain.DomainModel.Entities;
using MVCForum.Domain.Events;
using MVCForum.Domain.Interfaces;
using MVCForum.Domain.Interfaces.Services;
using MVCForum.Domain.Interfaces.UnitOfWork;
using MVCForum.Services.Data.Context;
using MVCForum.Utilities;

namespace MVCForum.Services
{

    public partial class XXXXXXService : IXXXXXXService
    {
        log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly MVCForumContext _context;
        public XXXXXXService()
        {
            _context = new MVCForumContext();
        }

        public XXXXXX Add(ADSetting newAD)
        {
            return _context.XXXXXX.Add(newAD);
        }

        public bool Delete(XXXXXX ad, IUnitOfWork unitOfWork)
        {
            try
            {
                _context.XXXXXX.Remove(ad);
                return true;
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
            }
            return false;
        }

        public XXXXXX Get(Guid id)
        {
            return _context.XXXXXX.AsNoTracking().FirstOrDefault(x => x.Id == id);
        }

        public IList<XXXXXX> GetAll()
        {
            return _context.XXXXXX.AsNoTracking().ToList();
        }
    }

}

6. 在MVCForum.IOC项目的UnityHelper.cs文件的BuildUnityContainer方法中加入以下代码：
  
  container.BindInRequestScope<IXXXXXXService, XXXXXXService>();

至此，数据类型定义部分编写完毕。


===========================================================================================================




$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$  WebSite项目操作步骤   $$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$

1. 项目的布局定义View文件存在于：Views\Shared\_Layout.cshtml，在其上拓展了两个新的布局文件，针对FullWidth和有右侧侧栏的布局文件_LayoutFullWidth.cshtml和_LayoutRightSideBar.cshtml














