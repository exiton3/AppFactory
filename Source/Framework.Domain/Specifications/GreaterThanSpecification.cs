using System;
using System.Linq.Expressions;

namespace AppFactory.Framework.Domain.Specifications
{
    public class GreaterThanSpecification<T> : SpecificationBase<T> where T : class
    {
        private readonly object _value;

        public GreaterThanSpecification(object filterValue, string propertyName)
        {
            _value = filterValue;
            Property = propertyName;
        }

        public string Property { get; private set; }


        public override Expression<Func<T, bool>> GetExpression()
        {
            ParameterExpression paramExp = Expression.Parameter(typeof(T), "x");
            MemberExpression memberExp = Expression.Property(paramExp, Property);

            var equalExpression = Expression.GreaterThan(Expression.Constant(_value), memberExp);

            return Expression.Lambda<Func<T, bool>>(equalExpression, paramExp);
        }
    }
}