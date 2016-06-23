using MVCForum.Domain.DomainModel;
using MVCForum.Domain.DomainModel.Activity;
using MVCForum.Website.Application;
using MVCForum.Website.Application.ActionFilterAttributes;
using System.Collections.Generic;

namespace MVCForum.Website.ViewModels
{
    #region 暂不使用

    public class ListCategoriesViewModels
    {
        public MembershipUser MembershipUser { get; set; }
        public MembershipRole MembershipRole { get; set; }
    }

    public class AllRecentActivitiesViewModel
    {
        public PagedList<ActivityBase> Activities { get; set; }

        public int? PageIndex { get; set; }
        public int? TotalCount { get; set; }
    }

    public class TermsAndConditionsViewModel
    {
        public string TermsAndConditions { get; set; }

        [ForumMvcResourceDisplayName("TermsAndConditions.Label.Agree")]
        [MustBeTrue(ErrorMessage = "TermsAndConditions.Label.AgreeError")]
        public bool Agree { get; set; }
    }

    #endregion

    public class AiLvHomeViewModel
    {
        public List<AiLvHuoDong> AiLv_ZuiXinHuoDongTop5 { get; set; }

        //public List<AiLvHuoDongJiLu> AiLv_ZuiXiJiLuTop5 { get; set; }

        public List<MembershipUser> AiLv_ZuiXinHuiYuanTop5 { get; set; }

        public List<MembershipTodayStar> AiLv_MeiRiZhiXingTop5 { get; set; }

        public List<Topic> AiLv_ZuiXinFuWuTop5 { get; set; }

        public List<Topic> AiLv_ZuiXinZiXunTop5 { get; set; }


        
    }

}