using System;
using System.Linq.Expressions;

namespace AppFactory.Framework.Domain.Specifications
{
    public abstract class SpecificationBase<TEntity> : ISpecification<TEntity> where TEntity : class
    {
        public abstract Expression<Func<TEntity, bool>> GetExpression();

        public ISpecification<TEntity> And(ISpecification<TEntity> specification)
        {
            return new AndSpecification<TEntity>(this, specification);
        }

        public ISpecification<TEntity> Or(ISpecification<TEntity> specification)
        {
            return new OrSpecification<TEntity>(this, specification);
        }

        public ISpecification<TEntity> Not(ISpecification<TEntity> specification)
        {
            return new NotSpecification<TEntity>(specification);
        }
    }
}
