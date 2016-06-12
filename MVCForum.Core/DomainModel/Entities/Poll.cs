using System;
using System.Collections.Generic;
using MVCForum.Utilities;

namespace MVCForum.Domain.DomainModel
{
    /// <summary>
    /// 
    /// </summary>
    public partial class Poll : Entity
    {
        public Poll()
        {
            Id = GuidComb.GenerateComb();
        }
        /// <summary>
        /// 投票实例的流水号
        /// </summary>
        public Guid Id { get; set; }
        /// <summary>
        /// 当前投票是否已经关闭
        /// </summary>
        public bool IsClosed { get; set; }
        /// <summary>
        /// 投票创建时间
        /// </summary>
        public DateTime DateCreated { get; set; }
        /// <summary>
        /// 几天后关闭此投票
        /// </summary>
        public int? ClosePollAfterDays { get; set; }
        /// <summary>
        /// 投票实例的创建者
        /// </summary>
        public virtual MembershipUser User { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public virtual IList<PollAnswer> PollAnswers { get; set; } 
    }
}
