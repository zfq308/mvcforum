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

    public partial class MembershipTodayStarService : IMembershipTodayStarService
    {

        log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IMembershipService _membershipService;
        private readonly MVCForumContext _context;
        public MembershipTodayStarService(IMembershipService membershipService, IMVCForumContext context)
        {
            _context = context as MVCForumContext;
            _membershipService = membershipService;
        }

        public MembershipTodayStar Add(MembershipTodayStar info)
        {
            return _context.MembershipTodayStar.Add(info);
        }

        public void BatchRemove()
        {
            try
            {
                var collection = _context.MembershipTodayStar.Where(x => x.Status == (int)MembershipTodayStarStatus.Valid).ToList();
                foreach (MembershipTodayStar item in collection)
                {
                    if (item.StopTime < DateTime.Now) item.Status = (int)MembershipTodayStarStatus.Invalid;
                }
                _context.SaveChanges();

            }
            catch (Exception)
            {

                throw;
            }
        }

        public bool Delete(MembershipUser user, IUnitOfWork unitOfWork)
        {
            try
            {
                var collection = _context.MembershipTodayStar.Where(x => x.UserId == user.Id).ToList();
                _context.MembershipTodayStar.RemoveRange(collection);
                _context.SaveChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }

        }

        public MembershipTodayStar Get(MembershipUser user)
        {
            if (user != null && !string.IsNullOrEmpty(user.Id.ToString()))
            {
                return Get(user.Id);
            }
            else
            {
                return null;
            }
        }

        public MembershipTodayStar Get(Guid id)
        {
            return _context.MembershipTodayStar.Where(x => x.UserId == id).FirstOrDefault();
        }

        public List<MembershipTodayStar> LoadAllAvailidUsers()
        {
            return _context.MembershipTodayStar.Where(x => x.Status == (int)MembershipTodayStarStatus.Valid).ToList();
        }
    }

}