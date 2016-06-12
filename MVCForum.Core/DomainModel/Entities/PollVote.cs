using System;
using MVCForum.Utilities;

namespace MVCForum.Domain.DomainModel
{
    /// <summary>
    /// 投票表决类
    /// </summary>
    public partial class PollVote
    {
        public PollVote()
        {
            Id = GuidComb.GenerateComb();
        }

        public Guid Id { get; set; }
        public virtual PollAnswer PollAnswer { get; set; }
        public virtual MembershipUser User { get; set; }
    }
}
