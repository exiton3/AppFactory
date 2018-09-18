using System.Collections.Generic;

namespace Framework.Domain.Infrastructure.Extensions
{
    public static class LinkedListExtension
    {
        public static LinkedList<T> AddAll<T>(this LinkedList<T> source, IEnumerable<T> items)
        {
            if (items == null) return source;

            foreach (var item in items)
            {
                source.AddLast(item);
            }

            return source;
        }

        public static LinkedListNode<T> Add<T>(this LinkedList<T> source, T item)
        {
            if (item == null) return source.Last;
            return source.AddLast(item);
        }
    }
}
