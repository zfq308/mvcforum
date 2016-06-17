using System;
using System.Linq;
using System.Collections.Generic;
using MVCForum.Domain.DomainModel;
using MVCForum.Domain.Interfaces.UnitOfWork;

namespace MVCForum.Domain.Interfaces.Services
{

    public partial interface IMembershipTodayStarService
    {
        MembershipTodayStar Add(MembershipTodayStar info);
       
        MembershipTodayStar Get(Guid id);
        MembershipTodayStar Get(MembershipUser user);
        List<MembershipTodayStar> LoadAllAvailidUsers();
        void BatchRemove();
        bool Delete(MembershipUser user, IUnitOfWork unitOfWork);
    }

}