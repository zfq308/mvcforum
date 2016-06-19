using System;
using MVCForum.Utilities;

namespace MVCForum.Domain.DomainModel.Activity
{
    /// <summary>
    /// 目前系统所支持的用户活动类别
    /// </summary>
    public enum ActivityType
    {
        BadgeAwarded,
        MemberJoined,
        ProfileUpdated,
    }

    public class Activity : Entity
    {
        public Activity()
        {
            Id = GuidComb.GenerateComb();
        }
        public Guid Id { get; set; }
        public string Type { get; set; }
        public string Data { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
