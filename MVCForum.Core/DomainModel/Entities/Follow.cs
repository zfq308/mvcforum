using System;
using System.Linq;
using System.Collections.Generic;
using MVCForum.Domain.DomainModel.Entities;
using MVCForum.Utilities;
using System.Data.SqlTypes;

namespace MVCForum.Domain.DomainModel
{

    /// <summary>
    /// 好友关注
    /// </summary>
    public partial class Follow : Entity
    {

        public Follow()
        {
            Id = GuidComb.GenerateComb();
        }

        #region 属性
      
        public Guid Id { get; set; }

        public Guid UserId { get; set; }

        public Guid FriendUserId { get; set; }

        public DateTime CreateTime { get; set; }

        public DateTime UpdateTime { get; set; }

        public string OpsFlag { get; set; }


        #endregion
    }

}