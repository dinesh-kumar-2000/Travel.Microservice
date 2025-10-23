namespace SharedKernel.Services;

public interface IDateTimeService
{
    DateTime UtcNow { get; }
    DateTime Now { get; }
    DateTimeOffset UtcNowOffset { get; }
    DateTimeOffset NowOffset { get; }
    TimeZoneInfo DefaultTimeZone { get; }
    DateTime ConvertToTimeZone(DateTime utcDateTime, TimeZoneInfo timeZone);
    DateTime ConvertFromTimeZone(DateTime localDateTime, TimeZoneInfo timeZone);
    DateTime ConvertToDefaultTimeZone(DateTime utcDateTime);
    DateTime ConvertFromDefaultTimeZone(DateTime localDateTime);
}

public class DateTimeService : IDateTimeService
{
    public DateTime UtcNow => DateTime.UtcNow;
    public DateTime Now => DateTime.Now;
    public DateTimeOffset UtcNowOffset => DateTimeOffset.UtcNow;
    public DateTimeOffset NowOffset => DateTimeOffset.Now;
    
    public TimeZoneInfo DefaultTimeZone { get; }

    public DateTimeService(TimeZoneInfo? defaultTimeZone = null)
    {
        DefaultTimeZone = defaultTimeZone ?? TimeZoneInfo.Local;
    }

    public DateTime ConvertToTimeZone(DateTime utcDateTime, TimeZoneInfo timeZone)
    {
        return TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, timeZone);
    }

    public DateTime ConvertFromTimeZone(DateTime localDateTime, TimeZoneInfo timeZone)
    {
        return TimeZoneInfo.ConvertTimeToUtc(localDateTime, timeZone);
    }

    public DateTime ConvertToDefaultTimeZone(DateTime utcDateTime)
    {
        return ConvertToTimeZone(utcDateTime, DefaultTimeZone);
    }

    public DateTime ConvertFromDefaultTimeZone(DateTime localDateTime)
    {
        return ConvertFromTimeZone(localDateTime, DefaultTimeZone);
    }
}
