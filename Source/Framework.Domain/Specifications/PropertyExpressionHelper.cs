using System;
using System.Linq.Expressions;

namespace AppFactory.Framework.Domain.Specifications
{
    public static class PropertyExpressionHelper
    {
        public static Func<TContainer, TProperty> InitializeGetter<TContainer, TProperty>(Expression<Func<TContainer, TProperty>> getterExpression)
        {
            return getterExpression.Compile();
        }

        public static string GetPropertyName<TContainer, TProperty>(Expression<Func<TContainer, TProperty>> expression)
        {
            var unary = expression.Body as UnaryExpression;
            var member = expression.Body as MemberExpression ?? (unary != null ? unary.Operand as MemberExpression : null);
            if (member != null)
                return member.Member.Name;

            throw new ArgumentException("Expression is not a member access", "expression");
        }

        public static string GetPropertyName<T>(Expression<Func<T, object>> expression)
        {
            return GetPropertyName<T, object>(expression);
        }
    }
}