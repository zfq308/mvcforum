using System;
using System.Linq;
using System.Collections.Generic;
using MVCForum.Domain.DomainModel.Entities;
using MVCForum.Utilities;
using System.Data.SqlTypes;

namespace MVCForum.Domain.DomainModel
{


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
        Specific = 2,
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
        Registing=1,
        /// <summary>
        /// 截止报名
        /// </summary>
        StopRegister=2,
        /// <summary>
        /// 活动已结束
        /// </summary>
        Finished=3,
    }


    /// <summary>
    /// 爱驴活动定义类
    /// </summary>
    public partial class AiLvHuoDong : Entity
    {
        public AiLvHuoDong()
        {
            Id = GuidComb.GenerateComb();
        }

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
        public int LeiBie { get; set; }

        /// <summary>
        /// 活动要求
        /// </summary>
        public int YaoQiu { get; set; }

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
        public int ZhuangTai { set; get; }

        /// <summary>
        /// 记录生成时间
        /// </summary>
        public DateTime CreatedTime { get; set; }


        public virtual IList<AiLvHuoDongDetail> AiLvHuoDongDetails { get; set; }

    }

   
}