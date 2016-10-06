using System;
using System.Linq;
using System.Collections.Generic;
using MVCForum.Domain.DomainModel;
using MVCForum.Domain.Interfaces.UnitOfWork;

namespace MVCForum.Domain.Interfaces.Services
{



    public partial interface IActivityRegisterService
    {
        ActivityRegister Add(ActivityRegister newRegister);
        ActivityRegister Get(Guid id);
        ActivityRegister Get(AiLvHuoDong huodong, MembershipUser user);
        bool Delete(ActivityRegister RegisterInfo);
        void ConfirmPay(Guid DetailsId, String FeeId);

        IList<ActivityRegister> GetActivityRegisterListByHongDongId(Guid HuoDongId);
        IList<ActivityRegister> GetActivityRegisterListByHongDong(AiLvHuoDong HuoDong);

        /// <summary>
        /// 特定活动Id的已报名人数
        /// </summary>
        /// <param name="HuoDongId"></param>
        /// <returns></returns>
        int CountRegistedNumber(Guid HuoDongId);
        /// <summary>
        /// 特定活动实例的已报名人数
        /// </summary>
        /// <param name="HuoDong"></param>
        /// <returns></returns>
        int CountRegistedNumber(AiLvHuoDong HuoDong);
        /// <summary>
        /// 特定活动Id的已支付人数
        /// </summary>
        /// <param name="HuoDongId"></param>
        /// <returns></returns>
        int CountPaidNumber(Guid HuoDongId);
        /// <summary>
        /// 特定活动实例的已支付人数
        /// </summary>
        /// <param name="HuoDong"></param>
        /// <returns></returns>
        int CountPaidNumber(AiLvHuoDong HuoDong);


        /// <summary>
        /// 检查报名状态
        /// </summary>
        /// <param name="huodong"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        Enum_VerifyActivityRegisterStatus CheckRegisterStatus(AiLvHuoDong huodong, MembershipUser user);



    }

}