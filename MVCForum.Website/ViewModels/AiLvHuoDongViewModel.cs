using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web;
using System.Web.Mvc;
using MVCForum.Domain.DomainModel;
using MVCForum.Domain.DomainModel.Enums;
using MVCForum.Website.Application;
using MVCForum.Domain.Constants;
using MVCForum.Website.Application.ActionFilterAttributes;

namespace MVCForum.Website.ViewModels
{

    public class AiLvHuoDong_ListViewModel
    {
        public IList<AiLvHuoDong> AiLvHuoDongList { get; set; }
    }

    public class AiLvHuoDong_CreateEdit_ViewModel
    {
        /// <summary>
        /// 爱驴活动流水号
        /// </summary>
        [HiddenInput]
        public Guid Id { get; set; }

        /// <summary>
        /// 活动名称
        /// </summary>
        [Required(ErrorMessage = "你是不是忘记填写活动名称？")]
        [Display(Name = "活动名称")]
        [StringLength(50)]
        public string MingCheng { get; set; }

        /// <summary>
        /// 活动类别
        /// </summary>
        [Display(Name = "活动类别")]
        public Enum_HuoDongLeiBie LeiBie { get; set; }

        /// <summary>
        /// 活动要求
        /// </summary>
        [Display(Name = "活动要求")]
        public Enum_HuoDongYaoQiu YaoQiu { get; set; }

        /// <summary>
        /// 活动开始时间
        /// </summary>
        [Display(Name = "活动开始时间")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd HH:mm}")]
        public DateTime StartTime { get; set; }

        /// <summary>
        /// 活动结束时间
        /// </summary>
        [Display(Name = "活动结束时间")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd HH:mm}")]
        [DateTimeNotLessThan("StartTime", "StartTime")]
        public DateTime StopTime { get; set; }

        /// <summary>
        /// 活动报名截止时间
        /// </summary>
        [Display(Name = "报名截止时间")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd HH:mm}")]
        public DateTime BaoMingJieZhiTime { get; set; }

        /// <summary>
        /// 活动举办地点
        /// </summary>
        [Required(ErrorMessage = "你是不是忘记填写活动地址？")]
        [Display(Name = "活动举办地点")]
        [StringLength(50)]
        public string DiDian { get; set; }

        /// <summary>
        /// 活动举办流程
        /// </summary>
        [UIHint(AppConstants.EditorType), AllowHtml]
        [Required(ErrorMessage = "你是不是忘记填写活动流程？")]
        [Display(Name = "活动举办流程")]
        [StringLength(4000)]
        public string LiuCheng { get; set; }

        /// <summary>
        /// 活动费用（RMB/人）
        /// </summary>
        [Display(Name = "活动报名费用")]
        [Range(1, 100000)]
        public int Feiyong { get; set; }

        /// <summary>
        /// 费用说明
        /// </summary>
        [UIHint(AppConstants.EditorType), AllowHtml]
        [Required(ErrorMessage = "你是不是忘记填写活动费用说明？")]
        [Display(Name = "活动费用说明")]
        [StringLength(400)]
        public string FeiyongShuoMing { get; set; }

        /// <summary>
        /// 注意事项
        /// </summary>
        [UIHint(AppConstants.EditorType), AllowHtml]
        [Required(ErrorMessage = "你是不是忘记填写活动注意事项？")]
        [Display(Name = "活动注意事项")]
        [StringLength(400)]
        public string ZhuYiShiXiang { get; set; }

        /// <summary>
        /// 预估参加人数
        /// </summary>
        [Display(Name = "预估人数")]
        [Range(1, 5000)]
        public int YuGuRenShu { get; set; }

        /// <summary>
        /// 男女生参加的性别比例
        /// </summary>
        [Required(ErrorMessage = "你是不是忘记填写参与人员的性别比例？")]
        [Display(Name = "参与人员的性别比例")]
        [StringLength(20)]
        public string XingBieBiLi { get; set; }

        /// <summary>
        /// 邀请码
        /// </summary>
        [Display(Name = "参与人员的邀请码")]
        [StringLength(6)]
        public string YaoQingMa { get; set; }

        /// <summary>
        /// 活动状态
        /// </summary>
        [Display(Name = "活动状态")]
        public Enum_HuoDongZhuangTai ZhuangTai { set; get; }

        /// <summary>
        /// 若生成记录用户是供应商，则记下供应商的Id
        /// </summary>
        public string GongYingShangUserId { get; set; }

        /// <summary>
        /// 活动发布审核标志位，若活动为管理员创建，其值为1，若活动为供应商创建，其值初始时为0，审核通过后为1，驳回审核后，其值为2.
        /// </summary>
        [Display(Name = "审核标志")]
        public Enum_ShenHeBiaoZhi ShenHeBiaoZhi { get; set; }

        ///// <summary>
        ///// 记录生成时间
        ///// </summary>
        //public DateTime CreatedTime { get; set; }

        /// <summary>
        /// 参加活动的男生集合
        /// </summary>
        public IList<MembershipUser> BoyJoinner { get; set; }
        /// <summary>
        /// 参加活动的女生集合
        /// </summary>
        public IList<MembershipUser> GirlJoiner { get; set; }
    }




}