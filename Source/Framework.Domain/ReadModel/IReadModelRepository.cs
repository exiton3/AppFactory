using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace AppFactory.Framework.Domain.ReadModel
{
    public interface IReadModelRepository<TReadModel> where TReadModel : class, IReadModel
    {
        Task<IEnumerable<TReadModel>> GetAllAsync();
        TReadModel Get(int id);
        void Add(TReadModel entity);
        void Delete(TReadModel entity);
        void DeleteById(int id);
        void Update(TReadModel entity);
        Task AddAsync(TReadModel entity);
        Task UpdateAsync(TReadModel entity);
        Task<TReadModel> GetAsync(int id);
        Task DeleteAsync(Expression<Func<TReadModel, bool>> predicate);
        Task DeleteAsync(TReadModel entity);
        Task DeleteAsync(int id);
    }
}