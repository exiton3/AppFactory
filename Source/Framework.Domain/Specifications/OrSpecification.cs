using System;
using System.Linq.Expressions;

namespace Framework.Domain.Specifications
{
    public class OrSpecification<TEntity> : SpecificationBase<TEntity> where TEntity : class
    {
        readonly ISpecification<TEntity> _leftSpecification;
        readonly ISpecification<TEntity> _rightSpecification;
        
        public OrSpecification(ISpecification<TEntity> left, ISpecification<TEntity> right)
        {
            _leftSpecification = left;
            _rightSpecification = right;
        }

        public override Expression<Func<TEntity, bool>> GetExpression()
        {
            Expression<Func<TEntity, bool>> expr1 = _leftSpecification.GetExpression();
            Expression<Func<TEntity, bool>> expr2 = _rightSpecification.GetExpression();
            Expression<Func<TEntity, bool>> expr3 = FilterExpressionExtension.UpdateParameter(expr2, expr1.Parameters[0]);

            return Expression.Lambda<Func<TEntity, bool>>(Expression.Or(expr1.Body, expr3.Body), expr1.Parameters[0]);
        }
    }
}
