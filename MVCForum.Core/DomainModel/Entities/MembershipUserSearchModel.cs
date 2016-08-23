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
    /// 会员搜索模型类
    /// </summary>
    public class MembershipUserSearchModel
    {
        [Display(Name = "账号")]
        [StringLength(50, MinimumLength = 4, ErrorMessage = "账号是一段长度介于4位到50位的文字")]
        public string UserName { get; set; }

        [Display(Name = "真实姓名")]
        [StringLength(16, MinimumLength = 2, ErrorMessage = "您的姓名是一段长度介于2位到16位的文字")]
        public string RealName { get; set; }

        [Display(Name = "昵称")]
        [StringLength(24, MinimumLength = 2, ErrorMessage = "您的昵称是一段长度介于2位到16位的文字")]
        public string AliasName { get; set; }

        [Display(Name = "性别")]
        public string Gender { get; set; }

        [Display(Name = "年龄")]
        public string AgeRange { get; set; }

        [Display(Name = "学历")]
        public string Education { get; set; }

        [Display(Name = "毕业院校")]
        [StringLength(20, MinimumLength = 2)]
        public string SchoolName { get; set; }

        [Display(Name = "居住地所在省份")]
        public string LocationProvince { get; set; }

        [Display(Name = "居住地所在城市")]
        public string LocationCity { get; set; }

        [Display(Name = "居住地所在县区")]
        public string LocationCounty { get; set; }

        [Display(Name = "职业")]
        [StringLength(20, MinimumLength = 2, ErrorMessage = "职业是一段长度介于2位到20位的文字")]
        public string Job { get; set; }

        [Display(Name = "月收入区段")]
        public string IncomeRange { get; set; }

        [Display(Name = "婚否")]
        public string IsMarried { get; set; }

        [Display(Name = "最近未登录天数")]
        public int NoLoginDays { get; set; }

        [Display(Name = "会员类别")]
        public string UserType { get; set; }

        [Display(Name = "会员状态")]
        public string UserStatus { get; set; }

    }

}