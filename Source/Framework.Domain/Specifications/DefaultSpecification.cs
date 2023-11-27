using System;
using System.Linq.Expressions;

namespace AppFactory.Framework.Domain.Specifications
{
    public class DefaultSpecification<TEntity> : SpecificationBase<TEntity> where TEntity : class
    {
        public override Expression<Func<TEntity, bool>> GetExpression()
        {
            return entity => true;
        }
    }
}
