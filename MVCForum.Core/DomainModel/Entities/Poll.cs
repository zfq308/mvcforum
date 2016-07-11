using System;
using System.Collections.Generic;
using MVCForum.Utilities;

namespace MVCForum.Domain.DomainModel
{
    /// <summary>
    /// 在线调查问卷的实体类
    /// </summary>
    public partial class Poll : Entity
    {
        public Poll()
        {
            Id = GuidComb.GenerateComb();
        }

        #region 属性

        /// <summary>
        ///在线调查问卷的流水号
        /// </summary>
        public Guid Id { get; set; }
        /// <summary>
        /// 在线调查问卷是否已经关闭
        /// </summary>
        public bool IsClosed { get; set; }
        /// <summary>
        /// 在线调查问卷的创建时间
        /// </summary>
        public DateTime DateCreated { get; set; }
        /// <summary>
        /// 问卷的关闭时间
        /// </summary>
        public int? ClosePollAfterDays { get; set; }
        /// <summary>
        /// 在线调查问卷的创建者
        /// </summary>
        public virtual MembershipUser User { get; set; }
        /// <summary>
        /// 在线调查问卷的答复
        /// </summary>
        public virtual IList<PollAnswer> PollAnswers { get; set; }

        #endregion
    }

    /// <summary>
    /// 在线调查问卷回答选项类
    /// </summary>
    public partial class PollAnswer
    {
        public PollAnswer()
        {
            Id = GuidComb.GenerateComb();
        }

        /// <summary>
        /// 在线调查问卷回答实例的流水码
        /// </summary>
        public Guid Id { get; set; }
        /// <summary>
        /// 在线调查问卷的选项内容
        /// </summary>
        public string Answer { get; set; }
        /// <summary>
        /// 此回答隶属的在线调查问卷的实体
        /// </summary>
        public virtual Poll Poll { get; set; }
        /// <summary>
        /// 表决集合
        /// </summary>
        public virtual IList<PollVote> PollVotes { get; set; }
    }

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
