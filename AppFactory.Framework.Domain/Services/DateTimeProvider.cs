namespace AppFactory.Framework.Domain.Services;

public class DateTimeProvider : IDateTimeProvider
{
    public DateTime GetTodayDate()
    {
        return DateTime.Now.Date;
    }

    public DateTime GetCurrentDateTime()
    {
        return DateTime.Now;
    }
}