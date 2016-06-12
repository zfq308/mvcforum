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
    /// 投票服务
    /// </summary>
    public partial class PollService : IPollService
    {
        private readonly MVCForumContext _context;
        public PollService(IMVCForumContext context)
        {
            _context = context as MVCForumContext;
        }

        /// <summary>
        /// 取得所有投票对象的实例集合
        /// </summary>
        /// <returns></returns>
        public List<Poll> GetAllPolls()
        {
            return _context.Poll.ToList();
        }

        /// <summary>
        /// 新增加一个投票
        /// </summary>
        /// <param name="poll"></param>
        /// <returns></returns>
        public Poll Add(Poll poll)
        {
            poll.DateCreated = DateTime.Now;
            poll.IsClosed = false;
            return _context.Poll.Add(poll);
        }

        /// <summary>
        /// 取得一个投票的实例对象
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Poll Get(Guid id)
        {
            return _context.Poll.FirstOrDefault(x => x.Id == id);
        }

        /// <summary>
        /// 删除一个投票的实例
        /// </summary>
        /// <param name="item"></param>
        public void Delete(Poll item)
        {
            _context.Poll.Remove(item);
        }
    }
}
