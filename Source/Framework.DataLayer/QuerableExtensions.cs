using System.Linq;
using System.Linq.Expressions;
using AppFactory.Framework.Domain.Paging;
using AppFactory.Framework.Domain.Repositories;

namespace AppFactory.Framework.DataLayer
{
    public static class QuerableExtensions
    {
        public static IQueryable<TEntity> ToPage<TEntity>(this IQueryable<TEntity> query, PagingSettings pagingSettings) where TEntity : class
        {
            if (pagingSettings != null)
            {
                return query.Skip((pagingSettings.PageNumber - 1)*pagingSettings.PageSize).Take(pagingSettings.PageSize);
            }
            return query;
        }

        //todo: unit tests
        public static IQueryable<T> OrderByFields<T>(this IQueryable<T> query, SortingSettings sortingSettings)
        {
            var result = query;

            var isFirstSortingRule = true;

            foreach (var sortingRule in sortingSettings.SortingRules)
            {
                result = ApplySortingRule(result, isFirstSortingRule, sortingRule);
                isFirstSortingRule = false;
            }

            if (!sortingSettings.SortingRules.Any())
            {
                result = ApplySortingRule(result, true, new SortingRule());
            }

            return result;
        }

        private static IQueryable<T> ApplySortingRule<T>(IQueryable<T> query, bool isFirstSortingRule, SortingRule sortingRule)
        {
            var exp = PropertyGetterExpression<T>(sortingRule);

            var method = GetMethodName(sortingRule, isFirstSortingRule);

            var types = new[] { query.ElementType, exp.Body.Type };

            var callExpression = Expression.Call(typeof(Queryable), method, types, query.Expression, exp);

            query = query.Provider.CreateQuery<T>(callExpression);
            return query;
        }

        private static string GetMethodName(SortingRule sortingRule, bool isFirstSortingRule)
        {
            var method = isFirstSortingRule ? "OrderBy" : "ThenBy";

            var methodWithOrder = sortingRule.SortOrder.Equals(SortOrder.Asc) ? method : method + "Descending";

            return methodWithOrder;
        }

        private static LambdaExpression PropertyGetterExpression<T>(SortingRule sortingSettings)
        {
            var param = Expression.Parameter(typeof (T), "p");
            var propertyName = string.IsNullOrEmpty(sortingSettings.Field) ? "Id" : sortingSettings.Field;
            var prop = Expression.Property(param, propertyName);
            var exp = Expression.Lambda(prop, param);
            return exp;
        }
    }
}