using System;
using System.Linq;
using System.Collections.Generic;
using MVCForum.Domain.DomainModel.Entities;
using MVCForum.Utilities;
using System.Data.SqlTypes;

namespace MVCForum.Domain.DomainModel
{
    public enum Enum_FeeStatus
    {
        /// <summary>
        /// 不需要缴费
        /// </summary>
        Free = 0,
        /// <summary>
        /// 需要缴费，但还没有缴费
        /// </summary>
        UnPayFee = 1,
        /// <summary>
        /// 需要缴费，已缴费
        /// </summary>
        PayedFee = 2,
    }

    /// <summary>
    /// 爱驴活动报名类
    /// </summary>
    public partial class ActivityRegister : Entity
    {
        #region 建构式

        private ActivityRegister()
        {

        }

        public ActivityRegister(Guid huodongId, MembershipUser user)
        {
            DetailsId = GuidComb.GenerateComb();
            Id = huodongId;
            if (user != null && user.IsApproved)
            {
                UserId = user.Id;
                UserGender = user.Gender;
                UserTelphone = user.MobilePhone;
            }
        }

        #endregion

        /// <summary>
        /// 爱驴活动报名流水号
        /// </summary>
        public Guid DetailsId { get; set; }

        /// <summary>
        /// 爱驴活动流水号(主表)
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// 报名用户的Id
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// 报名用户的性别（为快速查询做的冗余）
        /// </summary>
        public Enum_Gender UserGender { get; set; }

        /// <summary>
        /// 报名用户的婚姻状态（为快速查询做的冗余）
        /// </summary>
        public Enum_MarriedStatus UserMarriedStatus { get; set; }

        /// <summary>
        /// 报名用户的电话（为快速查询做的冗余）
        /// </summary>
        public string UserTelphone { get; set; }

        /// <summary>
        /// 总报名人数（为以后拓展使用，例如偕同人员人数，默认为1）
        /// </summary>
        public int JoinPeopleNumber { get; set; }

        /// <summary>
        /// 报名时间（记录生成时间）
        /// </summary>
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// 缴费渠道
        /// </summary>
        public string FeeSource { get; set; }

        /// <summary>
        /// 缴费流水码
        /// </summary>
        public string FeeId { get; set; }

        /// <summary>
        /// 缴费金额
        /// </summary>
        public int FeeNumber { get; set; }

        /// <summary>
        /// 缴费状态
        /// </summary>
        public Enum_FeeStatus FeeStatus { get; set; }

        /// <summary>
        /// 支付完成，更新缴费状态和此时间
        /// </summary>
        public DateTime PayCompletedTime { get; set; }

        /// <summary>
        /// 支付用户
        /// </summary>
        public virtual MembershipUser User { get; set; }
    }



}