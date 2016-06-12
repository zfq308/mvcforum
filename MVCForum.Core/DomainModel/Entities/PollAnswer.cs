using System;
using System.Collections.Generic;
using MVCForum.Utilities;

namespace MVCForum.Domain.DomainModel
{
    /// <summary>
    /// 投票结果类
    /// </summary>
    public partial class PollAnswer
    {
        public PollAnswer()
        {
            Id = GuidComb.GenerateComb();
        }
        /// <summary>
        /// 投票结果流水码
        /// </summary>
        public Guid Id { get; set; }
        /// <summary>
        /// 投票结果
        /// </summary>
        public string Answer { get; set; }
        /// <summary>
        /// 投票类
        /// </summary>
        public virtual Poll Poll { get; set; }
        /// <summary>
        /// 表决集合
        /// </summary>
        public virtual IList<PollVote> PollVotes { get; set; }
    }
}
