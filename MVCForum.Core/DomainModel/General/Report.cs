namespace MVCForum.Domain.DomainModel
{
    /// <summary>
    /// 用户举报实例类
    /// </summary>
    public partial class Report
    {
        /// <summary>
        /// 举报原因
        /// </summary>
        public string Reason { get; set; }
        /// <summary>
        /// 举报人
        /// </summary>
        public virtual MembershipUser Reporter { get; set; }
        /// <summary>
        /// 被举报人
        /// </summary>
        public virtual MembershipUser ReportedMember { get; set; }
        /// <summary>
        /// 被举报的帖子
        /// </summary>
        public virtual Post ReportedPost { get; set; }
    }
}
