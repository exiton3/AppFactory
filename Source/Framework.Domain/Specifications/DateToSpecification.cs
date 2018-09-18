using System;
using System.Linq.Expressions;

namespace Framework.Domain.Specifications
{
    public class DateToSpecification<T> : SpecificationBase<T> where T : class 
    {
        private readonly object _value;

        public DateToSpecification(object filterValue, string propertyName)
        {
            _value = filterValue;
            Property = propertyName;
        }

        public string Property { get; }


        public override Expression<Func<T, bool>> GetExpression()
        {
            ParameterExpression paramExp = Expression.Parameter(typeof(T), "x");
            MemberExpression memberExp = Expression.Property(paramExp, Property);

            var equalExpression = Expression.LessThanOrEqual(memberExp,Expression.Constant(Convert.ToDateTime(_value)));

            return Expression.Lambda<Func<T, bool>>(equalExpression, paramExp);
        }
    }
}