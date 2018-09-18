using System;
using System.Linq;
using System.Linq.Expressions;

namespace Framework.Domain.Infrastructure.Extensions
{
    public static class QueryableExtensions
    {
        public static IQueryable<TSource> WhereIf<TSource>(this IQueryable<TSource> source, bool condition,
            Expression<Func<TSource, bool>> predicate)
        {
            return condition ? source.Where(predicate) : source;
        }
        public static IQueryable<TSource> WhereIf<TSource>(this IQueryable<TSource> source, bool condition,
            Expression<Func<TSource, bool>> predicate, Expression<Func<TSource, bool>> sedicate)
        {
            return condition ? source.Where(predicate) : source.Where(sedicate);
        }
    }
}
