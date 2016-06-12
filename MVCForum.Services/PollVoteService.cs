using System;
using System.Collections.Generic;
using System.Linq;
using MVCForum.Domain.DomainModel;
using MVCForum.Domain.Interfaces;
using MVCForum.Domain.Interfaces.Services;
using MVCForum.Services.Data.Context;

namespace MVCForum.Services
{
    /// <summary>
    /// 投票表决服务
    /// </summary>
    public partial class PollVoteService : IPollVoteService
    {
        private readonly MVCForumContext _context;
        public PollVoteService(IMVCForumContext context)
        {
            _context = context as MVCForumContext;
        }

        /// <summary>
        /// 新增一个表决记录
        /// </summary>
        /// <param name="pollVote"></param>
        /// <returns></returns>
        public PollVote Add(PollVote pollVote)
        {
            return _context.PollVote.Add(pollVote);
        }

        /// <summary>
        /// 用户是否已经表决完成
        /// </summary>
        /// <param name="answerId"></param>
        /// <param name="userId">用户账号流水Id </param>
        /// <returns></returns>
        public bool HasUserVotedAlready(Guid answerId, Guid userId)
        {
            var vote = _context.PollVote.FirstOrDefault(x => x.PollAnswer.Id == answerId && x.User.Id == userId);
            return (vote != null);
        }

        /// <summary>
        /// 删除表决记录
        /// </summary>
        /// <param name="pollVote"></param>
        public void Delete(PollVote pollVote)
        {
            _context.PollVote.Remove(pollVote);
        }

        public PollVote Get(Guid id)
        {
            return _context.PollVote.FirstOrDefault(x => x.Id == id);
        }

        public List<PollVote> GetAllPollVotes()
        {
            return _context.PollVote.ToList();
        }

    }
}
