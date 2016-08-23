using System;
using System.Linq;
using System.Collections.Generic;
using MVCForum.Domain.DomainModel.Entities;
using MVCForum.Utilities;
using System.Data.SqlTypes;
using System.ComponentModel.DataAnnotations;

namespace MVCForum.Domain.DomainModel
{
    /// <summary>
    /// 今日之星特权状态枚举
    /// </summary>
    public enum MembershipTodayStarStatus
    {
        /// <summary>
        /// 今日之星特权生效中
        /// </summary>
        Valid = 1,
        /// <summary>
        /// 今日之星特权无效中
        /// </summary>
        Invalid = 0,
    }

    /// <summary>
    /// 用户今日之星特权的实体类
    /// </summary>
    public partial class MembershipTodayStar : Entity
    {
        /// <summary>
        /// 默认建构式
        /// </summary>
        public MembershipTodayStar()
        {
            Id = GuidComb.GenerateComb();
        }

        /// <summary>
        /// 用户今日之星特权的实体类的参数化建构式
        /// </summary>
        /// <param name="userid">今日之星特权的用户Id</param>
        /// <param name="starttime">今日之星特权的起始时间</param>
        /// <param name="stoptime">今日之星特权的结束时间</param>
        /// <param name="status">今日之星特权的状态</param>
        public MembershipTodayStar(Guid userid, DateTime starttime, DateTime stoptime, MembershipTodayStarStatus status)
        {
            Id = GuidComb.GenerateComb();
            UserId = userid;
            StartTime = starttime;
            StopTime = stoptime;
            Status = status == MembershipTodayStarStatus.Valid ? true : false;
        }

        #region 相关属性

        /// <summary>
        /// 今日之星流水Id
        /// </summary>
        public Guid Id { get; set; }
        /// <summary>
        /// 申请今日之星的用户Id
        /// </summary>
        public Guid UserId { get; set; }
        /// <summary>
        /// 今日之星申请记录的生成时间
        /// </summary>
        public DateTime CreateDate { get; set; }
        /// <summary>
        /// 记录操作员信息
        /// </summary>
        public string Operator { get; set; }
        /// <summary>
        /// 今日之星记录的状态：1为有效，0为无效
        /// </summary>
        public bool Status { get; set; }
        /// <summary>
        /// 今日之星操作票据流水号，默认为空，有值时为支付/购买交易的流水号
        /// </summary>
        public string JobId { get; set; }
        /// <summary>
        /// 今日之星特权的起始时间
        /// </summary>
        public DateTime StartTime { get; set; }
        /// <summary>
        /// 今日之星特权的结束时间
        /// </summary>
        public DateTime StopTime { get; set; }

        #endregion


    }

}