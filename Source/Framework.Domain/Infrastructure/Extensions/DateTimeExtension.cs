using System;
using System.Linq;

namespace Framework.Domain.Infrastructure.Extensions
{
    public static class DateTimeExtension
    {
        // Pacific Standard Time information regardless of localization.
        // This is UTC-8 as well as Pacific Standard Time(Mexico) but it has additional adjustment rules
        static readonly TimeZoneInfo PacificTimeZone = TimeZoneInfo.GetSystemTimeZones()
                                                           .FirstOrDefault(x => ((int)x.BaseUtcOffset.TotalHours == -8) && x.GetAdjustmentRules().Length > 1)
                                                       ?? TimeZoneInfo.Utc;

        public static DateTime ToPacificTime(this DateTime dt)
        {
            return TimeZoneInfo.ConvertTimeFromUtc(dt, PacificTimeZone);
        }

        public static DateTime Min(this DateTime dt1, DateTime dt2)
        {
            return (dt2 < dt1) ? dt2 : dt1;
        }

        public static DateTime Max(this DateTime dt1, DateTime dt2)
        {
            return (dt2 > dt1) ? dt2 : dt1;
        }

        public static bool IsInOpenInverval(this DateTime dt, DateTime start, DateTime end)
        {
            return start <= dt && dt <= end;
        }

        public static bool IsInClosedInverval(this DateTime dt, DateTime start, DateTime end)
        {
            return start < dt && dt < end;
        }

        public static DateTime ReplaceMinDate(this DateTime dt, DateTime substDate)
        {
            return (dt == DateTime.MinValue) ? substDate: dt;
        }
    }
}