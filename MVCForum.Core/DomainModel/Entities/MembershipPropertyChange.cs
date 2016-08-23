using System;
using System.Linq;
using System.Collections.Generic;
using MVCForum.Domain.DomainModel.Entities;
using MVCForum.Utilities;
using System.Data.SqlTypes;
using System.ComponentModel.DataAnnotations;

namespace MVCForum.Domain.DomainModel
{

    /// <summary>
    /// 用户属性变更记录项
    /// </summary>
    public partial class MembershipPropertyChange : Entity
    {
        #region 建构式

        public MembershipPropertyChange()
        {
            ChangeId = GuidComb.GenerateComb();
        }

        public MembershipPropertyChange(Guid userid, string propertyname, string propertytype, string sourcevalue, string targetvalue)
        {
            ChangeId = GuidComb.GenerateComb();
            UserId = userid;
            PropertyName = propertyname;
            PropertyTypeName = propertytype;
            SourceValue = sourcevalue;
            TargetValue = targetvalue;
        }

        #endregion

        #region 属性

        /// <summary>
        /// 用户属性变更Id
        /// </summary>
        public Guid ChangeId { get; set; }
        /// <summary>
        /// 用户属性变更申请记录的生成时间
        /// </summary>
        public DateTime CreateDate { get; set; }
        /// <summary>
        /// 要变更的属性名
        /// </summary>
        public string PropertyName { get; set; }
        /// <summary>
        /// 属性的数据类型，string, int or Datetime
        /// </summary>
        public string PropertyTypeName { get; set; }
        /// <summary>
        /// 变更前的值
        /// </summary>
        public string SourceValue { get; set; }
        /// <summary>
        /// 变更后的值
        /// </summary>
        public string TargetValue { get; set; }
        /// <summary>
        /// 审核标记
        /// </summary>
        public int ApprovedFlag { get; set; }
        /// <summary>
        /// 审核时间
        /// </summary>
        public DateTime ApprovedTime { get; set; }
        /// <summary>
        /// 审核人
        /// </summary>
        public string ApprovedMan { get; set; }
        /// <summary>
        /// 审核结论
        /// </summary>
        public string ApproveMessage { get; set; }

        /// <summary>
        /// 申请变更的用户Id
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// 申请变更的用户实例
        /// </summary>
        public virtual MembershipUser User { get; set; }

        #endregion

    }

}