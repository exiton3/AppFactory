using System.Collections.Generic;
using System.Linq;

namespace Framework.Domain.Infrastructure.Extensions
{
    public static class ListExtension
    {
        public static List<List<T>> Split<T>(this List<T> source, int size)
        {
            return source
                .Select((x, i) => new { Index = i, Value = x })
                .GroupBy(x => x.Index / size)
                .Select(x => x.Select(v => v.Value).ToList())
                .ToList();
        }

        //re-implementation due to performance issues of basic
        public static IEnumerable<T> SkipFast<T>(this IEnumerable<T> source, int count)
        {
            const int speedBorder = 50000;
            if (source is T[] arr && arr.Length > speedBorder)
            {
                for (int i = count; i < arr.Length; i++)
                {
                    yield return arr[i];
                }
            }
            else if (source is IList<T> list && list.Count > speedBorder)
            {
                for (int i = count; i < list.Count; i++)
                {
                    yield return list[i];
                }
            }
            else
            {
                // .NET framework
                using (IEnumerator<T> e = source.GetEnumerator())
                {
                    while (count > 0 && e.MoveNext()) count--;
                    if (count <= 0)
                    {
                        while (e.MoveNext()) yield return e.Current;
                    }
                }
            }
        }
    }
}
