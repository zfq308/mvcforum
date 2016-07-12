using System;
using MVCForum.Utilities;

namespace MVCForum.Domain.DomainModel
{
    public enum PointsFor
    {
        Post, Vote, Solution, Badge, Tag, Spam, Profile, Manual
    }
    public partial class MembershipUserPoints : Entity
    {
        public MembershipUserPoints()
        {
            Id = GuidComb.GenerateComb();
        }
        public Guid Id { get; set; }
        public int Points { get; set; }
        public DateTime DateAdded { get; set; }
        public PointsFor PointsFor { get; set; }
        public Guid? PointsForId { get; set; }
        public string Notes { get; set; }
        public virtual MembershipUser User { get; set; }
    }



    /// <summary>
    /// 用户上传个人图片类
    /// </summary>
    public partial class MembershipUserPicture : Entity
    {
        public MembershipUserPicture()
        {
            Id = GuidComb.GenerateComb();
        }
        public Guid Id { get; set; }

        /// <summary>
        /// 图片所属MemberUser的Id值
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// 图片上传后的文件名
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// 图片的原始文件名
        /// </summary>
        public string OriginFileName { get; set; }

        /// <summary>
        /// 图片描述
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 图片上传时间
        /// </summary>
        public DateTime UploadTime { get; set; }

        /// <summary>
        /// 审核标志位
        /// </summary>
        public Enum_UploadPictureAuditStatus AuditStatus { get; set; }

        /// <summary>
        /// 审核意见
        /// </summary>
        public string AuditComment { get; set; }

        /// <summary>
        /// 审核时间
        /// </summary>
        public DateTime AuditTime { get; set; }

        public virtual MembershipUser User { get; set; }

    }

    /// <summary>
    /// 用户上传的图片的审核标志位
    /// </summary>
    public enum Enum_UploadPictureAuditStatus
    {
        /// <summary>
        /// 已审核
        /// </summary>
        Auditted = 1,
        /// <summary>
        /// 待审核
        /// </summary>
        WaitingAudit = 0,
    }



}
