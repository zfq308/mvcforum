using System;
using System.Linq;
using System.Collections.Generic;
using MVCForum.Domain.DomainModel.Entities;
using MVCForum.Utilities;
using System.Data.SqlTypes;

namespace MVCForum.Domain.DomainModel
{

    public class ActivityRegisterForOrder
    {
        /// <summary>
        /// 公众号名称，由商户传入
        /// </summary>
        public string appId { get; set; }
        /// <summary>
        /// 时间戳，自1970年以来的秒数
        /// </summary>
        public string timeStamp { get; set; }
        /// <summary>
        /// 随机串
        /// </summary>
        public string nonceStr { get; set; }

        public string packageValue { get; set; }
        /// <summary>
        /// 微信签名
        /// </summary>
        public string paySign { get; set; }
        /// <summary>
        /// 微信支付后的返回消息
        /// </summary>
        public string msg { get; set; }

        public string detailsId { get; set; }
    }

}