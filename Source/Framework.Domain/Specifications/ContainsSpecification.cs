using System;
using System.Linq.Expressions;
using System.Reflection;

namespace AppFactory.Framework.Domain.Specifications
{
    public class ContainsSpecification<T> : SpecificationBase<T> where T : class
    {
        private readonly string _value;

       
        public ContainsSpecification(string value, string propertyName)
            
        {
            _value = value;
            Property = propertyName;
        }

        public string Property { get; set; }

        public override Expression<Func<T, bool>> GetExpression()
        {
            ParameterExpression paramExp = Expression.Parameter(typeof(T), "x");
            MemberExpression memberExp = Expression.Property(paramExp, Property);
            var constantExpression = Expression.Constant(_value);
            var filtersMethodInfo = typeof(string).GetMethod("Contains", BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

            var containsExpression = Expression.Call(memberExp, filtersMethodInfo, constantExpression);

            return Expression.Lambda<Func<T, bool>>(containsExpression, paramExp);

        }
    }
}