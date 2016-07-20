using System;
using System.Linq;
using System.Collections.Generic;
using MVCForum.Domain.DomainModel.Entities;
using MVCForum.Utilities;
using System.Data.SqlTypes;

namespace MVCForum.Domain.DomainModel
{

    public partial class AiLvJiLu : Entity
    {
        public AiLvJiLu()
        {
            Id = GuidComb.GenerateComb();
        }

        #region 属性
        /// <summary>
        /// 爱驴记录流水号
        /// </summary>
        public Guid Id { get; set; }


        public Guid AiLvHuodongId { get; set; }

        public string JiluContent { get; set; }

        public DateTime CreateTime { get; set; }

        public DateTime UpdateTime { get; set; }

        public string UpdateOperate { get; set; }



        #endregion
    }

}