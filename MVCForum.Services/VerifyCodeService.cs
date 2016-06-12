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
        const int MaxVerifyTimes = 3;
        const int MaxVerifyCodeLifeTimeHour = 1;  // ����1Сʱ���Զ�ʧЧ����

        log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly MVCForumContext _context;

        public VerifyCodeService(MVCForumContext context)
        {
            _context = context;
        }


        public void CheckInvalidVerifyCode()
        {
            var list = _context.VerifyCode
                .Where(x => x.Status == (int)VerifyCodeStatus.Waiting && DateTime.Now.Subtract(x.DateCreated).TotalHours > MaxVerifyCodeLifeTimeHour)
                .ToList();

            if (list != null && list.Count > 0)
            {
                Parallel.ForEach(list, (m) =>
                {
                    if (m.Status == (int)VerifyCodeStatus.Waiting)
                    {
                        m.Status = (int)VerifyCodeStatus.Invalid;
                        m.LastUpdate = DateTime.Now;
                    }
                });
                _context.SaveChanges();
            }
        }

        public void CompleteVerifyCode(Guid id)
        {
            var obj = Get(id);
            if (obj != null && obj.Status != (int)VerifyCodeStatus.VerifySuccess)
            {
                obj.Status = (int)VerifyCodeStatus.VerifySuccess;
                obj.LastUpdate = DateTime.Now;
                _context.SaveChanges();
            }
        }

        public void CompleteVerifyCode(VerifyCode verifyCode)
        {
            if (verifyCode != null)
            {
                CompleteVerifyCode(verifyCode.Id);
            }
            else
            {
                throw new ArgumentNullException("verifyCode");
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
        /// ɾ�����ڵ���֤��
        /// </summary>
        public void DeleteInvalidVerifyCode()
        {
            var list = _context.VerifyCode
               .Where(x => x.Status == (int)VerifyCodeStatus.Invalid)
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

        /// <summary>
        /// ����ض��ֻ����ڿ��л��е�δ��֤ͨ���ļ�¼��
        /// </summary>
        /// <param name="MobileNumber">�ض����ֻ�����</param>
        /// <returns></returns>
        public int GetCountByMobileNumber(string MobileNumber)
        {
            return _context.VerifyCode
                .Where(x => x.MobileNumber.Trim() == MobileNumber.Trim() && x.Status != (int)VerifyCodeStatus.VerifySuccess)
                .ToList().Count;
        }

        /// <summary>
        /// ���ֻ�������֤��
        /// </summary>
        /// <param name="verifyCode">��֤���ʵ��</param>
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

                    // ����֤��ʵ��д�����ݿ�
                    _context.VerifyCode.Add(verifyCode);
                }
                else
                {
                    logger.Warn(string.Format("��ǰ�ֻ�����{0}����ע������", verifyCode.MobileNumber));
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