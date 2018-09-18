using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Framework.Domain.Infrastructure.Extensions
{
    public static class StringExtension
    {
        public static int GetLevenshteinDistance(this string first, string second)
        {
            if (string.IsNullOrEmpty(first) || string.IsNullOrEmpty(second)) return 0;

            int lengthFirst = first.Length;
            int lengthSecond = second.Length;
            int[,] distances = new int[lengthFirst + 1, lengthSecond + 1];

            for (int i = 0; i <= lengthFirst; distances[i, 0] = i++)
            {
            }
            for (int j = 0; j <= lengthSecond; distances[0, j] = j++)
            {
            }

            for (int i = 1; i <= lengthFirst; i++)
                for (int j = 1; j <= lengthSecond; j++)
                {
                    int cost = second[j - 1] == first[i - 1] ? 0 : 1;
                    distances[i, j] = Math.Min(
                        Math.Min(distances[i - 1, j] + 1, distances[i, j - 1] + 1),
                        distances[i - 1, j - 1] + cost);
                }
            return distances[lengthFirst, lengthSecond];
        }
        
        public static string JoinBySeparator(this IEnumerable<string> list, char separator)
        {
            return string.Join(separator.ToString(CultureInfo.CurrentCulture), list);
        }

        public static string Join(this IEnumerable<string> list)
        {
            return string.Join(string.Empty, list);
        }

        public static string GetClosestValue(this string value, IEnumerable<string> listOfValues)
        {
            string[] strings = listOfValues.ToArray();
            return strings.Count() > 1
                ? strings.Aggregate(
                    (s1, s2) => s1.GetLevenshteinDistance(value) < s2.GetLevenshteinDistance(value) ? s1 : s2)
                : strings.FirstOrDefault();
        }

        public static string RemoveSeparators(this string s, char[] separators)
        {
            return string.IsNullOrEmpty(s) ? s : string.Concat(s.Split(separators, StringSplitOptions.RemoveEmptyEntries));
        }

        public static string TruncateLongString(this string str, int maxLength)
        {
            return str.Substring(0, Math.Min(str.Length, maxLength));
        }

        public static string TruncateEnd(this string str, int endingLength)
        {
            return str.Substring(0, Math.Max(str.Length - endingLength, 0));
        }

        public static string SubstringFromEnd(this string s, int lenght)
        {
            return s.Substring(s.Length - lenght, lenght);
        }

        public static string SubstringFromBegin(this string s, int lenght)
        {
            return s.Substring(0, lenght);
        }

        public static DateTime? ToDate(this string dateTimeStr, string dateFmt)
        {
            DateTime dt;
            if (DateTime.TryParseExact(dateTimeStr, dateFmt, CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces, out dt))
            {
                return dt;
            }
            return null;
        }

        public static int? ToInt(this string s)
        {
            int i;
            if (Int32.TryParse(s, out i))
            {
                return i;
            }
            return null;
        }

        public static string RemoveFromEnd(this string s, string suffix)
        {
            if (s.EndsWith(suffix))
            {
                return s.Substring(0, s.Length - suffix.Length);
            }
            else
            {
                return s;
            }
        }

        public static bool IsEmpty(this string value)
        {
            return string.IsNullOrWhiteSpace(value);
        }

        public static bool IsNotEmpty(this string value)
        {
            return !string.IsNullOrWhiteSpace(value);
        }

        public static bool IsEqualIgnoreCase(this string a, string b)
        {
            return string.Equals(a, b, StringComparison.CurrentCultureIgnoreCase);
        }
    }
}