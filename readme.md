一些常见的MVC功能
1. 输出一个Partial区块到前台页面

2. 调到用户账户编辑页
 <a href="@Url.Action("Edit", "Members", new { id = Model.CurrentUser.Id })">@Html.LanguageString("Buttons.Edit") @Model.CurrentUser.UserName</a>




增加一张新的数据表操作步骤
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

4. 根据编写
4. 在MVCForum.Services、Data/Context/MVCForumContext.cs. 在其中加入新的数据表对应属性