using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web;
using System.Web.Mvc;
using MVCForum.Domain.DomainModel;
using MVCForum.Domain.DomainModel.Enums;
using MVCForum.Website.Application;

namespace MVCForum.Website.ViewModels
{

    public class AiLvHuoDongEditModel
    {
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
    }

}