using System;
using System.Linq;
using System.Collections.Generic;
using MVCForum.Domain.DomainModel.Entities;
using MVCForum.Utilities;
using System.Data.SqlTypes;

namespace MVCForum.Domain.DomainModel
{
    #region 枚举定义

    /// <summary>
    /// 活动类别枚举
    /// </summary>
    public enum Enum_HuoDongLeiBie
    {
        /// <summary>
        /// 自由报名
        /// </summary>
        FreeRegister = 1,
        /// <summary>
        /// 特殊邀请
        /// </summary>
        SpecicalRegister = 2,

    }

    /// <summary>
    /// 活动要求枚举
    /// </summary>
    public enum Enum_HuoDongYaoQiu
    {
        /// <summary>
        /// 单身人士
        /// </summary>
        Single = 1,
        
        
        /// <summary>
        /// 特别邀请
        /// </summary>
        Specific = 2, //TODO: 此项是否可以删除，它完全可以通过Enum_HuoDongLeiBie.SpecicalRegister 来代替。


        /// <summary>
        /// 无要求，全员皆可
        /// </summary>
        None = 3,
    }

    /// <summary>
    /// 活动状态枚举
    /// </summary>
    public enum Enum_HuoDongZhuangTai
    {
        /// <summary>
        /// 报名中
        /// </summary>
        Registing = 1,
        /// <summary>
        /// 截止报名
        /// </summary>
        StopRegister = 2,
        /// <summary>
        /// 活动已结束
        /// </summary>
        Finished = 3,
    }

    /// <summary>
    /// 审核标志位枚举
    /// </summary>
    public enum Enum_ShenHeBiaoZhi
    {
        /// <summary>
        /// 等待审核
        /// </summary>
        WaitingAudit = 0,
        /// <summary>
        /// 审核成功
        /// </summary>
        AuditSuccess = 1,
        /// <summary>
        /// 审核不通过，驳回
        /// </summary>
        AuditReject = 2,
    }

    #endregion

    /// <summary>
    /// 爱驴活动定义类
    /// </summary>
    public partial class AiLvHuoDong : Entity
    {
        public AiLvHuoDong()
        {
            Id = GuidComb.GenerateComb();
        }

        #region 属性

        /// <summary>
        /// 爱驴活动流水号
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// 活动名称
        /// </summary>
        public string MingCheng { get; set; }

        /// <summary>
        /// 活动类别
        /// </summary>
        public Enum_HuoDongLeiBie LeiBie { get; set; }

        /// <summary>
        /// 活动要求
        /// </summary>
        public Enum_HuoDongYaoQiu YaoQiu { get; set; }

        /// <summary>
        /// 活动开始时间
        /// </summary>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// 活动结束时间
        /// </summary>
        public DateTime StopTime { get; set; }

        /// <summary>
        /// 活动报名截止时间
        /// </summary>
        public DateTime BaoMingJieZhiTime { get; set; }

        /// <summary>
        /// 活动举办地点
        /// </summary>
        public string DiDian { get; set; }

        /// <summary>
        /// 活动举办流程
        /// </summary>
        public string LiuCheng { get; set; }

        /// <summary>
        /// 活动费用（RMB/人）
        /// </summary>
        public int Feiyong { get; set; }

        /// <summary>
        /// 费用说明
        /// </summary>
        public string FeiyongShuoMing { get; set; }

        /// <summary>
        /// 注意事项
        /// </summary>
        public string ZhuYiShiXiang { get; set; }

        /// <summary>
        /// 预估参加人数
        /// </summary>
        public int YuGuRenShu { get; set; }

        /// <summary>
        /// 男女生参加的性别比例
        /// </summary>
        public string XingBieBiLi { get; set; }

        /// <summary>
        /// 邀请码
        /// </summary>
        public string YaoQingMa { get; set; }

        /// <summary>
        /// 活动状态
        /// </summary>
        public Enum_HuoDongZhuangTai ZhuangTai { set; get; }

        /// <summary>
        /// 若生成记录用户是供应商，则记下供应商的Id
        /// </summary>
        public string GongYingShangUserId { get; set; }

        /// <summary>
        /// 活动发布审核标志位，若活动为管理员创建，其值为Enum_ShenHeBiaoZhi.AuditSuccess，
        /// 若活动为供应商创建，其值初始时为WaitingAudit，审核通过后为Enum_ShenHeBiaoZhi.AuditSuccess，
        /// 驳回审核后，其值为Enum_ShenHeBiaoZhi.AuditReject
        /// </summary>
        public Enum_ShenHeBiaoZhi ShenHeBiaoZhi { get; set; }

        /// <summary>
        /// 审核意见
        /// </summary>
        public string AuditComments { get; set; }

        /// <summary>
        /// 记录生成时间
        /// </summary>
        public DateTime CreatedTime { get; set; }

        /// <summary>
        /// 活动标题图片
        /// </summary>
        public string Avatar { get; set; }

        /// <summary>
        /// 活动报名明细
        /// </summary>
        public virtual IList<ActivityRegister> ActivityRegisters { get; set; }

        #endregion
    }




}