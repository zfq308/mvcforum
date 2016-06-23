using System;
using System.Linq;
using System.Collections.Generic;
using MVCForum.Domain.DomainModel;
using MVCForum.Domain.Interfaces.UnitOfWork;

namespace MVCForum.Domain.Interfaces.Services
{

    public partial interface IADSettingService
    {
        ADSetting Add(ADSetting newAD);
        ADSetting Get(Guid id);
        IList<ADSetting> GetAll();
        bool Delete(ADSetting ad, IUnitOfWork unitOfWork);

    }

}