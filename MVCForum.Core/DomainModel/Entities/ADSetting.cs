using System;
using System.Linq;
using System.Collections.Generic;
using MVCForum.Domain.DomainModel.Entities;
using MVCForum.Utilities;
using System.Data.SqlTypes;

namespace MVCForum.Domain.DomainModel
{

    public enum Enum_ADType
    {
        MainType = 1,
    }


    /// <summary>
    /// 广告窗的设定类
    /// </summary>
    public partial class ADSetting : Entity
    {
        public ADSetting()
        {
            Id = GuidComb.GenerateComb();
        }
        /// <summary>
        /// 广告的流水号
        /// </summary>
        public Guid Id { get; set; }

        public int ADType { get; set; }

        public string ImageName { get; set; }

        public string ImageSaveURL { get; set; }

        public string Link { get; set; }

        public DateTime CreateTime { get; set; }

    }
    
}