using MVCForum.Domain.DomainModel;
using System.Collections.Generic;

namespace MVCForum.Website.ViewModels
{
    public class SearchViewModel
    {
        public List<PostViewModel> Posts { get; set; }
        public int? PageIndex { get; set; }
        public int? TotalCount { get; set; }
        public int? TotalPages { get; set; }
        public string Term { get; set; }

    }

    public class Search2ViewMode
    {
        public List<ViewMemberViewModel> Users { get; set; }
        public int? PageIndex { get; set; }
        public int? TotalCount { get; set; }
        public int? TotalPages { get; set; }

        public MembershipUserSearchModel searchModel { get; set; }

    }
}