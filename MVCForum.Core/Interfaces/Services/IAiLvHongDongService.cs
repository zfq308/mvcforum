using System;
using System.Linq;
using System.Collections.Generic;
using MVCForum.Domain.DomainModel;
using MVCForum.Domain.Interfaces.UnitOfWork;

namespace MVCForum.Domain.Interfaces.Services
{

    public partial interface IAiLvHuoDongService
    {
        AiLvHuoDong Add(AiLvHuoDong newHuoDong);
        AiLvHuoDong Get(Guid id);
        IList<AiLvHuoDong> GetAiLvHongDongListByName(string SearchCondition);
        IList<AiLvHuoDong> GetAll();
        IList<AiLvHuoDong> GetRecentAiLvHuodong(int amountToTake);
        IList<AiLvHuoDong> GetAllAiLvHuodongByStatus(Enum_HuoDongZhuangTai status);
        PagedList<AiLvHuoDong> GetAll(int pageIndex, int pageSize);
        bool Delete(AiLvHuoDong huodong);

        bool Update_ZhuangTai();
        bool AuditAiLvHuodong(AiLvHuoDong ailvhuodongInstance, bool auditresult);
    }


}