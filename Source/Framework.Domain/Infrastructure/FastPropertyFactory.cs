using System;
using System.Linq.Expressions;

namespace Framework.Domain.Infrastructure
{
    public static class FastPropertyFactory
    {
        public static Func<TInstance, object> GeneratePropertyGetter<TInstance>(string propertyName)
        {
            var parameterExpression = Expression.Parameter(typeof(TInstance), "value");

            var propertyValueExpression = Expression.Property(parameterExpression, propertyName);
            var convertExp = (Expression)Expression.Convert(propertyValueExpression, typeof(object));
            var propertyGetter =
                Expression.Lambda<Func<TInstance, object>>(convertExp, parameterExpression).Compile();

            return propertyGetter;
        }
    }
}