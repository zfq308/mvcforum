using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using System.Threading.Tasks;
using MVCForum.Domain.DomainModel;
using MVCForum.Domain.Interfaces.Services;
using MVCForum.Services.Data.Context;
using System.Text;
using System.IO;
using System.Net;
using MVCForum.Domain.Interfaces;

namespace MVCForum.Services
{

    public partial class VerifyCodeService : IVerifyCodeService
    {
     log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #region 建构式

        const int MaxVerifyTimes = 3;
        const int MaxVerifyCodeLifeTimeHour = 1;  // 短信1小时后自动失效过期

        private readonly MVCForumContext _context;

        public VerifyCodeService(MVCForumContext context)
        {
            _context = context;
        }
        public VerifyCodeService()
        {
            _context = new MVCForumContext();
        }

        #endregion

        public void CheckInvalidVerifyCode()
        {
            DateTime d = DateTime.Now - TimeSpan.FromHours(1);
            var list = _context.VerifyCode
                .Where(x => x.Status == (int)VerifyCodeStatus.Waiting && x.DateCreated < d)
                .ToList();

            if (list != null && list.Count > 0)
            {
                Parallel.ForEach(list, (m) =>
                {
                    if (m.Status == (int)VerifyCodeStatus.Waiting)
                    {
                        m.Status = VerifyCodeStatus.Invalid;
                        m.LastUpdate = DateTime.Now;
                    }
                });
                _context.SaveChanges();
            }
        }

        public void CompleteVerifyCode(Guid id)
        {
            var obj = Get(id);
            if (obj != null && obj.Status != VerifyCodeStatus.VerifySuccess)
            {
                obj.Status = VerifyCodeStatus.VerifySuccess;
                obj.LastUpdate = DateTime.Now;
                _context.SaveChanges();
            }
        }

        public void CompleteVerifyCode(VerifyCode verifyCode)
        {
            if (verifyCode != null)
            {
                var code = Get(verifyCode.MobileNumber, verifyCode.VerifyNumber, verifyCode.Status);
                if (code != null)
                {
                    CompleteVerifyCode(code.Id);
                }
                else
                {
                    logger.Error("注册码回写时找不到原纪录。");
                }
            }
            else
            {
                throw new ArgumentNullException(nameof(verifyCode));
            }
        }

        public void Delete(List<VerifyCode> verifyCodeList)
        {
            if (verifyCodeList != null && verifyCodeList.Count > 0)
            {
                Parallel.ForEach(verifyCodeList, (m) => { Delete(m); });
            }
        }

        public void Delete(VerifyCode verifyCode)
        {
            _context.VerifyCode.Remove(verifyCode);
        }

        /// <summary>
        /// 删除过期的验证码
        /// </summary>
        public void DeleteInvalidVerifyCode()
        {
            var list = _context.VerifyCode
               .Where(x => x.Status == VerifyCodeStatus.Invalid)
               .ToList();

            if (list != null && list.Count > 0)
            {
                Delete(list);
            }
        }

        public VerifyCode Get(Guid id)
        {
            return _context.VerifyCode.FirstOrDefault(x => x.Id == id);
        }

        private VerifyCode Get(string mobileNumber, string verifyCode, VerifyCodeStatus status)
        {
            return _context.VerifyCode.FirstOrDefault(x => x.MobileNumber == mobileNumber && x.VerifyNumber == verifyCode && x.Status == status);
        }

        /// <summary>
        /// 检查特定手机号在库中还有的未验证通过的记录数
        /// </summary>
        /// <param name="mobileNumber">特定的手机号码</param>
        /// <returns></returns>
        public int GetCountByMobileNumber(string mobileNumber)
        {
            return _context.VerifyCode
                .Where(x => x.MobileNumber.Trim() == mobileNumber.Trim() && x.Status != VerifyCodeStatus.VerifySuccess)
                .ToList().Count;
        }

        /// <summary>
        /// 给手机发送验证码
        /// </summary>
        /// <param name="verifyCode">验证码的实例</param>
        public void SendVerifyCode(VerifyCode verifyCode)
        {
            if (verifyCode != null && !string.IsNullOrEmpty(verifyCode.MobileNumber))
            {
                verifyCode.MobileNumber = verifyCode.MobileNumber.Trim();
                if (GetCountByMobileNumber(verifyCode.MobileNumber) <= MaxVerifyTimes)
                {
                    #region Config SMS

                    ISendSMSService service = null;

                    SettingsService ss = new SettingsService(_context, new CacheService());
                    var currentSettings = ss.GetSettings();
                    if (currentSettings != null && !string.IsNullOrEmpty(currentSettings.UCPaasConfig_Account)
                                             && !string.IsNullOrEmpty(currentSettings.UCPaasConfig_Token)
                                             && !string.IsNullOrEmpty(currentSettings.UCPaasConfig_AppId)
                                             && !string.IsNullOrEmpty(currentSettings.UCPaasConfig_TemplatedId)
                      )
                    {
                        service = new SendSMSService_UCPaas(new UCPaasConfig(currentSettings.UCPaasConfig_Account, currentSettings.UCPaasConfig_Token,
                            currentSettings.UCPaasConfig_AppId, currentSettings.UCPaasConfig_TemplatedId));
                    }
                    else
                    {
                        logger.Warn("Use Dev SMS setting to send message in UCPaas platform. Please check the SMS setting or Cache.");
                        service = new SendSMSService_UCPaas();
                    }
                    #endregion

                    string result = service.Send(verifyCode.VerifyNumber, verifyCode.MobileNumber);
                    verifyCode.ReturnMessage = result;

                    // 将验证码实例写入数据库
                    _context.VerifyCode.Add(verifyCode);
                    _context.SaveChanges();
                }
                else
                {
                    logger.Warn(string.Format("当前手机号码{0}超过注册限制", verifyCode.MobileNumber));
                    throw new ArgumentNullException("verifyCode");
                }
            }
            else
            {
                logger.Error("The MobileNumber can't empty.");
                throw new ArgumentException("The MobileNumber can't empty.");
            }
        }
    }
}