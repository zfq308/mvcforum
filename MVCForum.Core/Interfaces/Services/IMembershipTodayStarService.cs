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
       
        MembershipTodayStar Get(Guid UserId);
        MembershipTodayStar Get(MembershipUser userInstance);
        List<MembershipTodayStar> LoadAllAvailidUsers();
        List<MembershipUser> LoadNewestTodayStars(int count);
        void BatchRemove();
        bool Delete(MembershipUser user, IUnitOfWork unitOfWork);
    }

}