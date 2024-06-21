namespace AppFactory.Framework.Domain.Services;

public interface IDateTimeProvider
{
    DateTime GetTodayDate();
    DateTime GetCurrentDateTime();
}