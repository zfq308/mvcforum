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

    public partial class ADSettingService : IADSettingService
    {
        log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly MVCForumContext _context;
        public ADSettingService()
        {
            _context = new MVCForumContext();
        }

        public ADSetting Add(ADSetting newAD)
        {
            return _context.ADSetting.Add(newAD);
        }

        public bool Delete(ADSetting ad)
        {
            try
            {
                _context.ADSetting.Remove(ad);
                return true;
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
            }
            return false;
        }

        public ADSetting Get(Guid id)
        {
            return _context.ADSetting.AsNoTracking().FirstOrDefault(x => x.Id == id);
        }

        public IList<ADSetting> GetAll()
        {
            return _context.ADSetting.AsNoTracking().ToList();
        }

        public IList<ADSetting> GetRecentTop5()
        {
            return _context.ADSetting.OrderByDescending(x => x.CreateTime).Take(5).ToList();
        }
    }

}