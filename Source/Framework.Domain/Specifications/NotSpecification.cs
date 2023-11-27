using System;
using System.Linq.Expressions;

namespace AppFactory.Framework.Domain.Specifications
{
    public class NotSpecification<TEntity> : SpecificationBase<TEntity> where TEntity : class
    {
        readonly ISpecification<TEntity> _specification;
        
        public NotSpecification(ISpecification<TEntity> specification)
        {
            _specification = specification;
        }

        public override Expression<Func<TEntity, bool>> GetExpression()
        {
            return Expression.Lambda<Func<TEntity, bool>>(Expression.Negate(_specification.GetExpression()));
        }
    }
}
