using System;
using System.Linq.Expressions;

namespace AppFactory.Framework.Domain.Specifications
{
    public interface ISpecification
    {
    }

    public interface ISpecification<TEntity> : ISpecification where TEntity : class
    {
        Expression<Func<TEntity,bool>> GetExpression();
        ISpecification<TEntity> And(ISpecification<TEntity> specification);
        ISpecification<TEntity> Or(ISpecification<TEntity> specification);
        ISpecification<TEntity> Not(ISpecification<TEntity> specification);
    }
}
