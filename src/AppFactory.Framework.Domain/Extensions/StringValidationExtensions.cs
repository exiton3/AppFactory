using System.Globalization;

namespace AppFactory.Framework.Domain.Extensions;

public static class StringValidationExtensions
{

    public static bool IsEmpty(this string value)
    {
        return string.IsNullOrWhiteSpace(value);
    }
    public static bool IsValidUsDate(this string dateTimeStr)
    {
        if (DateTime.TryParseExact(dateTimeStr, FormatConstants.DateTimeFormat, CultureInfo.InvariantCulture,
                DateTimeStyles.AllowWhiteSpaces, out var dt))
        {
            return dt.Year >= 2000;
        }

        return false;
    }

    public static bool IsValidDateFormat(this string dateTimeStr)
    {
        if (DateTime.TryParseExact(dateTimeStr, FormatConstants.DateTimeFormat, CultureInfo.InvariantCulture,
                DateTimeStyles.AllowWhiteSpaces, out var dt))
            return true;

        return false;
    }

    public static bool IsDateGreaterOrEqual(this string firstDateStr, DateTime second)
    {
        if (DateTime.TryParseExact(firstDateStr, FormatConstants.DateTimeFormat, CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces, out var first))
        {
            return first >= second;
        }

        return false;
    }

    public static bool IsDateGreaterOrEqualToSecondDate(this string firstDateStr, string secondDateStr)
    {
        DateTime.TryParseExact(firstDateStr, FormatConstants.DateTimeFormat, CultureInfo.InvariantCulture,
            DateTimeStyles.AllowWhiteSpaces, out var first);
        DateTime.TryParseExact(secondDateStr, FormatConstants.DateTimeFormat, CultureInfo.InvariantCulture,
            DateTimeStyles.AllowWhiteSpaces, out var second);

        return first >= second;
    }

    public static bool EqualsIgnoreCase(this string firstString, string secondString)
    {
        return firstString.Equals(secondString, StringComparison.InvariantCultureIgnoreCase);
    }
}