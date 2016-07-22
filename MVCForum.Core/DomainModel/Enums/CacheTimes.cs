namespace MVCForum.Domain.DomainModel.Enums
{
    /// <summary>
    /// 缓存时间
    /// </summary>
    public enum CacheTimes
    {
        OneMinute = 1 * 60,
        OneHour = 60 * 60,
        TwoHours = 120 * 60,
        SixHours = 360 * 60,
        TwelveHours = 720 * 60,
        OneDay = 1440 * 60
    }
}
