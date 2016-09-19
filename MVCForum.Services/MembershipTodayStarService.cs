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
using MVCForum.Domain.DomainModel.General;
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
        #region 成员变量

        log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IMembershipService _membershipService;
        private readonly MVCForumContext _context;

        #endregion

        #region 建构式

        public MembershipTodayStarService(IMembershipService membershipService, IMVCForumContext context)
        {
            _context = context as MVCForumContext;
            _membershipService = membershipService;
        }

        #endregion

        /// <summary>
        /// 移除过期的每日之星
        /// </summary>
        public void BatchRemove()
        {
            try
            {
                var collection = _context.MembershipTodayStar.Where(x => x.Status == true).ToList();
                foreach (MembershipTodayStar item in collection)
                {
                    if (item.StopTime < DateTime.Now) item.Status = false;
                }
                _context.SaveChanges();

            }
            catch (Exception)
            {

                throw;
            }
        }

        public MembershipTodayStar Add(MembershipTodayStar info)
        {
            return _context.MembershipTodayStar.Add(info);
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

        public MembershipTodayStar Get(Guid UserId)
        {
            return _context.MembershipTodayStar.Where(x => x.UserId == UserId).FirstOrDefault();
        }

        public List<MembershipUser> LoadAllAvailidUsers()
        {
            var UserIdList = _context.MembershipTodayStar.Where(x => x.Status == true).OrderByDescending(x => x.CreateDate).Select(x => x.UserId).ToList();
            List<MembershipUser> returnlist = null;
            if (UserIdList != null && UserIdList.Count > 0)
            {
                returnlist = new List<MembershipUser>();
                foreach (var userId in UserIdList)
                {
                    var user = _membershipService.GetUser(userId);
                    if (!returnlist.Contains(user))
                    {
                        returnlist.Add(user);
                    }
                }
                return returnlist;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public List<MembershipUser> LoadNewestTodayStars(int count)
        {
            if (count <= 0) count = 5;
            var UserIdList = _context.MembershipTodayStar.Where(x => x.Status == true).OrderByDescending(x => x.CreateDate).Take(count).Select(x => x.UserId).ToList();
            List<MembershipUser> returnlist = null;
            if (UserIdList != null && UserIdList.Count > 0)
            {
                returnlist = new List<MembershipUser>();
                foreach (var userId in UserIdList)
                {
                    var user = _membershipService.GetUser(userId);
                    if (!returnlist.Contains(user))
                    {
                        user.LocationProvince = TProvince.LoadProvinceByProvincedId(Convert.ToInt32(user.LocationProvince)).ProvinceName;

                        user.LocationCity = TCity.LoadCityByCityId(Convert.ToInt32(user.LocationCity)).CityName;

                        user.LocationCounty = TCountry.LoadCountryByCountryId(Convert.ToInt32(user.LocationCounty)).CountryName;

                        returnlist.Add(user);
                    }
                }
                return returnlist;
            }
            else
            {
                return null;
            }
        }
    }
}