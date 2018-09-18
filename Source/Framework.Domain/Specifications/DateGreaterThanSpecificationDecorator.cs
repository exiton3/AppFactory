using System;
using System.Data.SqlTypes;
using System.Linq.Expressions;
using System.Reflection;

namespace Framework.Domain.Specifications
{
    public class DateGreaterThanSpecificationDecorator<T> : SpecificationBase<T> where T : class
    {
        private readonly string _property;
        private readonly DateTime? _value;
        private readonly ISpecification<T> _specification;

        public DateGreaterThanSpecificationDecorator(ISpecification<T> specification, object value, string propertyName)
        {
            _specification = specification;
            _property = propertyName;
            _value = (DateTime?)value;
        }

        public override Expression<Func<T, bool>> GetExpression()
        {
            ParameterExpression paramExp = Expression.Parameter(typeof(T), "x");

            var prop = Expression.Property(paramExp, _property);
            var propType = ((PropertyInfo)prop.Member).PropertyType;
            MemberExpression memberExp = Expression.Property(paramExp, _property);

            Expression gtExpression = null;
            if (propType == typeof(DateTime?))
            {
                var nullCheck = Expression.NotEqual(memberExp, Expression.Constant(null, typeof(object)));
                var condition = Expression.GreaterThan(Expression.Constant(_value),
                    Expression.Convert(memberExp, typeof(DateTime)));
                gtExpression = Expression.AndAlso(nullCheck, condition);
            }
            if (propType == typeof(DateTime))
            {
                gtExpression = Expression.GreaterThan(Expression.Constant(_value ?? (DateTime)SqlDateTime.MinValue), memberExp);
            }
            return gtExpression == null
                ? _specification.GetExpression()
                : Expression.Lambda<Func<T, bool>>(gtExpression, paramExp);
        }
    }
}