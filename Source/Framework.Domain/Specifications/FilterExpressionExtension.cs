using System;
using System.Linq.Expressions;

namespace Framework.Domain.Specifications
{
    public static class FilterExpressionExtension
    {
        public static Expression<Func<T, bool>> UpdateParameter<T>(
            Expression<Func<T, bool>> expr,
            ParameterExpression newParameter)
        {
            var visitor = new ParameterUpdateVisitor(expr.Parameters[0], newParameter);
            var body = visitor.Visit(expr.Body);

            return
                Expression.Lambda<Func<T, bool>>(body, newParameter);
        }
    }
}
