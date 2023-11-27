using System;
using System.Collections.Generic;
using System.Linq;

namespace AppFactory.Framework.Domain.Infrastructure.Extensions
{
    public static class CollectionExtension
    {
        public static HashSet<T> ToHashSet<T>(this IEnumerable<T> source)
        {
            return new HashSet<T>(source);
        }

        public static IList<T> Clone<T>(this IList<T> listToClone) where T : ICloneable
        {
            return listToClone.Select(item => (T)item.Clone()).ToList();
        }
    }
}