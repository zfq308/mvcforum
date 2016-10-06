using System;
using System.Linq;
using System.Collections.Generic;
using MVCForum.Domain.DomainModel;
using MVCForum.Domain.Interfaces;
using MVCForum.Domain.Interfaces.Services;
using MVCForum.Services.Data.Context;

namespace MVCForum.Services
{
    public partial class ActivityRegisterService : IActivityRegisterService
    {
        log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #region 成员变量

        private readonly MVCForumContext _context;
        private readonly ISettingsService _settingsService;
        private readonly ILoggingService _loggingService;
        private readonly IMembershipService _membershipService;

        #endregion

        #region 建构式

        public ActivityRegisterService(IMVCForumContext context, ISettingsService settingsService, ILoggingService loggingService, IMembershipService membershipService)
        {
            _settingsService = settingsService;
            _loggingService = loggingService;
            _membershipService = membershipService;
            _context = context as MVCForumContext;

        }

        #endregion

        #region 增删爱驴活动报名实例

        public ActivityRegister Add(ActivityRegister newRegister)
        {
            return _context.ActivityRegister.Add(newRegister);
        }

        public bool Delete(ActivityRegister RegisterInfo)
        {
            try
            {
                _context.ActivityRegister.Remove(RegisterInfo);
                return true;
            }
            catch (Exception ex)
            {
                _loggingService.Error(ex);
            }
            return false;
        }

        #endregion

        #region 检查报名状态

        /// <summary>
        /// 检查报名状态
        /// </summary>
        /// <param name="huodong"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        public Enum_VerifyActivityRegisterStatus CheckRegisterStatus(AiLvHuoDong huodong, MembershipUser user)
        {
            if (user == null || user.IsApproved == false)
            {
                return Enum_VerifyActivityRegisterStatus.Fail_VerifyUserApproveStatus;
            }

            var result = CheckDuplicationRegister(huodong, user);
            switch (result)
            {
                case -1:
                    return Enum_VerifyActivityRegisterStatus.Fail_VerifyRegisteredTheActivity;
                case -2:
                    return Enum_VerifyActivityRegisterStatus.Fail_VerifyRegisteredNoPay;
                case -3:
                    return Enum_VerifyActivityRegisterStatus.Fail_VerifyRegisteredPaied;
                default:
                    break;
            }

            if (!CheckHuoDongJieZhiShijian(huodong))
            {
                return Enum_VerifyActivityRegisterStatus.Fail_BeyondDeadlineTime;
            }

            if (!CheckUserMarriedStatus(huodong, user))
            {
                return Enum_VerifyActivityRegisterStatus.Fail_VerifyMarriedStatus;
            }

            if (!CheckUserGender(huodong, user))
            {
                return Enum_VerifyActivityRegisterStatus.Fail_VerifyUserGender;
            }
            return Enum_VerifyActivityRegisterStatus.Success;
        }

        /// <summary>
        /// 检查用户是否已经注册参加这个活动
        /// </summary>
        /// <param name="huodong">活动实例</param>
        /// <param name="user">用户实例</param>
        /// <returns></returns>
        private int CheckDuplicationRegister(AiLvHuoDong huodong, MembershipUser user)
        {
            var item = _context.ActivityRegister.AsNoTracking().Where(x => x.Id == huodong.Id && x.UserId == user.Id);
            if (item != null && item.Count() > 0)
            {
                //已存在注册信息
                if (huodong.Feiyong == 0)
                {
                    return -1; // 已注册，但活动为免费活动，无需支付
                }
                else
                {
                    //已存在注册信息，且活动为非免费活动
                    if (item.ToList()[0].FeeId == "000000")
                    {
                        return -2;   //还未支付
                    }
                    else
                    {
                        return -3; //已完成支付
                    }
                }
            }
            else
            {
                return 0; //用户暂未注册此活动，可以注册
            }
        }

        /// <summary>
        /// 检查活动截止时间
        /// </summary>
        /// <param name="huodong"></param>
        /// <returns></returns>
        private bool CheckHuoDongJieZhiShijian(AiLvHuoDong huodong)
        {
            if (huodong != null)
            {
                return DateTime.Now < huodong.BaoMingJieZhiTime;
            }
            return false;
        }

        #region 检查男女性别比例

        /// <summary>
        /// 检查特定用户user是否符合爱驴活动实例huodong的男女比例
        /// </summary>
        /// <param name="huodong"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        private bool CheckUserGender(AiLvHuoDong huodong, MembershipUser user)
        {
            if (huodong != null && user != null)
            {
                int YuGuRenShu = huodong.YuGuRenShu;
                string XingBieBiLi = huodong.XingBieBiLi;

                if (YuGuRenShu <= 0) return false;

                if (CheckXingBieBiLi(XingBieBiLi))
                {
                    string[] pair = XingBieBiLi.Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                    if (pair.Length != 2) return false;

                    int BoyRate = 0;
                    int.TryParse(pair[0], out BoyRate);

                    int GirlRate = 0;
                    int.TryParse(pair[1], out GirlRate);

                    if (BoyRate + GirlRate == 0) return false;

                    if (user.Gender == Enum_Gender.boy)
                    {
                        int BoyNumber = (int)((YuGuRenShu * BoyRate) / (BoyRate + GirlRate));
                        return _context.ActivityRegister.Where(x => x.Id == huodong.Id && x.UserGender == Enum_Gender.boy).Count() + 1 <= BoyNumber;
                    }
                    else
                    {
                        int GirlNumber = (int)((YuGuRenShu * GirlRate) / (BoyRate + GirlRate));
                        return _context.ActivityRegister.Where(x => x.Id == huodong.Id && x.UserGender == Enum_Gender.girl).Count() + 1 <= GirlNumber;
                    }
                }

                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 检查男女比例
        /// </summary>
        /// <param name="XingBieBiLi"></param>
        /// <returns></returns>
        private bool CheckXingBieBiLi(string XingBieBiLi)
        {
            string[] pair = XingBieBiLi.Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
            if (pair.Length != 2)
            {
                return false;
            }

            int BoyRate = 0;
            int.TryParse(pair[0], out BoyRate);

            int GirlRate = 0;
            int.TryParse(pair[1], out GirlRate);

            if (BoyRate + GirlRate == 0) return false;

            return true;
        }

        #endregion

        /// <summary>
        /// 当活动要求为单身用户时， 检查用户的婚姻情况
        /// </summary>
        /// <param name="huodong"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        private bool CheckUserMarriedStatus(AiLvHuoDong huodong, MembershipUser user)
        {
            if (huodong != null && user != null)
            {
                if (huodong.YaoQiu == Enum_HuoDongYaoQiu.Single && user.IsMarried == Enum_MarriedStatus.Married)
                {
                    return false;
                }
                return true;
            }
            return false;
        }

        #endregion

        public ActivityRegister Get(Guid id)
        {
            return _context.ActivityRegister.AsNoTracking().FirstOrDefault(x => x.DetailsId == id);
        }


        public ActivityRegister Get(AiLvHuoDong huodong, MembershipUser user)
        {
            var item = _context.ActivityRegister.AsNoTracking().Where(x => x.Id == huodong.Id && x.UserId == user.Id);
            if (item != null && item.Count() > 0)
            {
                return item.ToList()[0];
            }
            return null;
        }

        public IList<ActivityRegister> GetActivityRegisterListByHongDong(AiLvHuoDong HuoDong)
        {
            if (HuoDong != null)
            {
                return GetActivityRegisterListByHongDongId(HuoDong.Id);
            }
            return null;
        }

        public IList<ActivityRegister> GetActivityRegisterListByHongDongId(Guid HuoDongId)
        {
            return _context.ActivityRegister.AsNoTracking().Where(x => x.Id == HuoDongId).ToList();
        }


        public int CountRegistedNumber(Guid HuoDongId)
        {
            return _context.ActivityRegister.AsNoTracking().Where(x => x.Id == HuoDongId).Sum(y => y.JoinPeopleNumber);
        }

        public int CountRegistedNumber(AiLvHuoDong HuoDong)
        {
            if (HuoDong != null)
            {
                return CountRegistedNumber(HuoDong.Id);
            }
            return 0;
        }

        public int CountPaidNumber(Guid HuoDongId)
        {
            return _context.ActivityRegister.AsNoTracking().Where(x => x.Id == HuoDongId &&
            x.FeeStatus == Enum_FeeStatus.PayedFee).Sum(y => y.JoinPeopleNumber);

        }

        public int CountPaidNumber(AiLvHuoDong HuoDong)
        {
            if (HuoDong != null)
            {
                return CountPaidNumber(HuoDong.Id);
            }
            return 0;
        }

        public void ConfirmPay(Guid DetailsId, string FeeId, DateTime PayCompletedTime)
        {
            var ar = Get(DetailsId);
            if (ar != null)
            {
                ar.FeeId = FeeId;
                ar.FeeStatus = Enum_FeeStatus.PayedFee;
                _context.Entry<ActivityRegister>(ar).State = System.Data.Entity.EntityState.Modified;
            }
        }
    }


}